﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Akka.Actor;
using Akka.Configuration;
using Akka.DI.AutoFac;
using Akka.DI.Core;
using Autofac;
using bankka.Actors;
using bankka.Commands.Accounts;
using bankka.Commands.Customers;
using bankka.Db;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Core;
using Topshelf;

namespace bankka
{


    public class BankkaService
    {

        public static ActorSystem System;
        public void Start()
        {

            Log.Information("Starting System bankka");
            System = ActorSystem.Create("bankka", GetConfig());

            var options = new DbContextOptionsBuilder<BankkaContext>()
                .UseInMemoryDatabase("my_bankka_database")
                .Options;





            var builder = new ContainerBuilder();

            builder.RegisterInstance(Log.Logger).As<ILogger>();
            builder.RegisterType<AccountActor>();
            builder.RegisterType<CustomerActor>();
            builder.RegisterInstance(System).As<ActorSystem>();
            builder.RegisterType<DbContextFactory>().As<IDbContextFactory>();
            builder.RegisterInstance(options);
            var container = builder.Build();

            var resolver = new AutoFacDependencyResolver(container, System);


            var customerIds = CreateCustomer();
            var customers = new List<IActorRef>();


            //foreach (var customerId in customerIds)
            //{

            //    var customer = System.ActorOf(System.DI().Props<CustomerActor>(), customerId.ToString());
            //    customers.Add(customer);

            //}
        }

        private static IList<long> CreateCustomer()
        {
            var customerIds = new List<long>();

            //var rnd = new Random();

            //for (int i = 0; i < 20; i++)
            //{
            //    StringBuilder sb = new StringBuilder();
            //    for (int j = 0; j < 10; j++)
            //    {
            //        sb.Append(rnd.Next(1, 9));
            //    }
            //    customerIds.Add(Convert.ToInt64(sb.ToString()));
            //}

            customerIds.Add(123456);

            return customerIds;
        }
        public void Stop()
        {
            Log.Information("Shutting down");
            CoordinatedShutdown.Get(System).Run().Wait(TimeSpan.FromSeconds(2));
        }

        private static Config GetConfig()
        {
            var configString = @"
                    akka {
                        actor { 
		                    provider = ""Akka.Cluster.ClusterActorRefProvider, Akka.Cluster""
	                            deployment {
								  /tasker {
									router = consistent-hashing-group
                                    routees.paths = [""/user/api""]
                                    virtual-nodes-factor = 8
                                    cluster {
                                        enabled = on
                                        max-nr-of-instances-per-node = 2
                                        allow-local-routees = off
                                        use-role = tracker
                                    }
                                }                
                            }
	                    }
                       loglevel=DEBUG,
                       loggers=[""Akka.Logger.Serilog.SerilogLogger, Akka.Logger.Serilog""]
                        remote {
		                    log-remote-lifecycle-events = DEBUG
		                    dot-netty.tcp {
			                    transport-class = ""Akka.Remote.Transport.DotNetty.TcpTransport, Akka.Remote""
			                    applied-adapters = []
			                    transport-protocol = tcp
			                    #will be populated with a dynamic host-name at runtime if left uncommented
			                    #public-hostname = ""POPULATE STATIC IP HERE""
			                    hostname = ""127.0.0.1""
			                    port = 16001
		                    }
	                    }     
											
	                    cluster {
		                    #will inject this node as a self-seed node at run-time
		                    seed-nodes = [""akka.tcp://bankka@127.0.0.1:4053""] #manually populate other seed nodes here, i.e. ""akka.tcp://lighthouse@127.0.0.1:4053"", ""akka.tcp://lighthouse@127.0.0.1:4044""
                            roles = [web]
	                    }
                    }
            ";
            return ConfigurationFactory.ParseString(configString);
        }
    }
    class Program
    {




        static void Main(string[] args)
        {
            var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
            Log.Logger = logger;
            var rc = HostFactory.Run(x =>                                   //1
            {
                x.UseSerilog(logger);
                x.Service<BankkaService>(s =>                                   //2
                {
                    s.ConstructUsing(name => new BankkaService());                //3
                    s.WhenStarted(tc => tc.Start());                         //4
                    s.WhenStopped(tc => tc.Stop());                          //5
                });

                x.SetDescription("Bankka Host");                   //7
                x.SetDisplayName("Bankka");                                  //8
                x.SetServiceName("Bankka");                                  //9
            });                                                             //10



            var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());  //11
            Environment.ExitCode = exitCode;

            //{





            //    var customers = new List<IActorRef>();

            //    var customerAccounts = new List<KeyValuePair<long, long>>();

            //    var rnd = new Random();




            //    for (var i = 0; i < rnd.Next(40, 100); i++)
            //    {
            //        var customerAccount = customerAccounts[rnd.Next(0, customerAccounts.Count - 1)];

            //        var customer = customers.First(c => c.Path.Name == customerAccount.Key.ToString());

            //        customer.Tell(new CustomerDepositCommand(customerAccount.Value, Convert.ToDecimal(rnd.NextDouble() * 300)));
            //    }

            //    Console.WriteLine("Press any key to quit.");

            //    while (!Console.KeyAvailable)
            //    {
            //        Thread.Sleep(1000);
            //    }


            //}
        }


    }

}
