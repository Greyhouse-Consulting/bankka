using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using bankka.Api.Controllers;
using bankka.Api.Models;
using bankka.Commands.Customers;
using Newtonsoft.Json;

namespace bankka.loader
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();


        static void Main(string[] args)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");
            Console.WriteLine("Press any key to start. (Make sure all processes has been started)");
            Console.ReadKey();
        
            var accountModel = new AccountModel
            {
                Name = "Kalle Kula",
                PhoneNumber = "1234567890"
            };
            
            Console.WriteLine("Starting session");
            var sw = new Stopwatch();
            sw.Start();
            CreateCustomerAndAccounts(accountModel);
            CreateCustomerAndAccounts(accountModel);
            CreateCustomerAndAccounts(accountModel);
            CreateCustomerAndAccounts(accountModel);
            sw.Stop();

            Console.WriteLine($"Session took {sw.Elapsed}");

            Console.WriteLine("Press any key to end");
            Console.ReadKey();
        }

        private static void CreateCustomerAndAccounts(AccountModel accountModel)
        {
            var stringTask = AsyncHelpers.RunSync(() => client.PostAsync("http://localhost:61255/api/customers",
                new StringContent(JsonConvert.SerializeObject(accountModel), Encoding.UTF8, "application/json")));


            var customerId =
                JsonConvert.DeserializeObject<int>(AsyncHelpers.RunSync(() => stringTask.Content.ReadAsStringAsync()));

            Console.WriteLine($"Custumer with id {customerId} created");
            for (int i = 0; i < 30; i++)
            {
                var accountRequest = new CreateAccountModel
                {
                    Id = customerId,
                    Name = "Account"
                };

                var accountReponse = AsyncHelpers.RunSync(() => client.PostAsync("http://localhost:61255/api/accounts",
                    new StringContent(JsonConvert.SerializeObject(accountRequest), Encoding.UTF8, "application/json")));

                var accountResponseBody =
                    JsonConvert.DeserializeObject<OpenAccountResponse>(AsyncHelpers.RunSync(() =>
                        accountReponse.Content.ReadAsStringAsync()));

                Console.WriteLine($"Account with id {accountResponseBody.AccountId} created");
            }
        }
    }


    public static class AsyncHelpers
    {
        /// <summary>
        /// Execute's an async Task<T> method which has a void return value synchronously
        /// </summary>
        /// <param name="task">Task<T> method to execute</param>
        public static void RunSync(Func<Task> task)
        {
            var oldContext = SynchronizationContext.Current;
            var synch = new ExclusiveSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(synch);
            synch.Post(async _ =>
            {
                try
                {
                    await task();
                }
                catch (Exception e)
                {
                    synch.InnerException = e;
                    throw;
                }
                finally
                {
                    synch.EndMessageLoop();
                }
            }, null);
            synch.BeginMessageLoop();

            SynchronizationContext.SetSynchronizationContext(oldContext);
        }

        /// <summary>
        /// Execute's an async Task<T> method which has a T return type synchronously
        /// </summary>
        /// <typeparam name="T">Return Type</typeparam>
        /// <param name="task">Task<T> method to execute</param>
        /// <returns></returns>
        public static T RunSync<T>(Func<Task<T>> task)
        {
            var oldContext = SynchronizationContext.Current;
            var synch = new ExclusiveSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(synch);
            T ret = default(T);
            synch.Post(async _ =>
            {
                try
                {
                    ret = await task();
                }
                catch (Exception e)
                {
                    synch.InnerException = e;
                    throw;
                }
                finally
                {
                    synch.EndMessageLoop();
                }
            }, null);
            synch.BeginMessageLoop();
            SynchronizationContext.SetSynchronizationContext(oldContext);
            return ret;
        }

        private class ExclusiveSynchronizationContext : SynchronizationContext
        {
            private bool done;
            public Exception InnerException { get; set; }
            readonly AutoResetEvent workItemsWaiting = new AutoResetEvent(false);

            readonly Queue<Tuple<SendOrPostCallback, object>> items =
                new Queue<Tuple<SendOrPostCallback, object>>();

            public override void Send(SendOrPostCallback d, object state)
            {
                throw new NotSupportedException("We cannot send to our same thread");
            }

            public override void Post(SendOrPostCallback d, object state)
            {
                lock (items)
                {
                    items.Enqueue(Tuple.Create(d, state));
                }

                workItemsWaiting.Set();
            }

            public void EndMessageLoop()
            {
                Post(_ => done = true, null);
            }

            public void BeginMessageLoop()
            {
                while (!done)
                {
                    Tuple<SendOrPostCallback, object> task = null;
                    lock (items)
                    {
                        if (items.Count > 0)
                        {
                            task = items.Dequeue();
                        }
                    }

                    if (task != null)
                    {
                        task.Item1(task.Item2);
                        if (InnerException != null) // the method threw an exeption
                        {
                            throw new AggregateException("AsyncHelpers.Run method threw an exception.", InnerException);
                        }
                    }
                    else
                    {
                        workItemsWaiting.WaitOne();
                    }
                }
            }

            public override SynchronizationContext CreateCopy()
            {
                return this;
            }
        }
    }
}