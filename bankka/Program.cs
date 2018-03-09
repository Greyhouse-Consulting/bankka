using System;
using Akka.Actor;
using Akka.Configuration;
using Akka.DI.AutoFac;
using Akka.DI.Core;
using Akka.Routing;
using Autofac;
using bankka.Actors;
using bankka.Db;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Topshelf;

namespace bankka
{


    public class BankkaService
    {
        private static ActorSystem _system;
        private static IActorRef _accountRouter;
        private static  AutoFacDependencyResolver _resolver;
        private IActorRef _customerClerkActor;
        private IActorRef _accountClerkRouter;

        public void Start()
        {

            Log.Information("Starting System bankka");
            _system = ActorSystem.Create("bankka", GetConfig());

            var options = new DbContextOptionsBuilder<BankkaContext>()
                .UseInMemoryDatabase("bankka")
                .Options;

            var container = CreateContainer(options);

            _resolver = new AutoFacDependencyResolver(container, _system);

            _customerClerkActor = _system.ActorOf(_system.DI().Props<CustomerClerkActor>().WithRouter(FromConfig.Instance), "customerClerks");
            _accountClerkRouter = _system.ActorOf(_system.DI().Props<AccountClerkActor>().WithRouter(FromConfig.Instance), "accountClerks");
        }

        private static IContainer CreateContainer(DbContextOptions<BankkaContext> options)
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(Log.Logger).As<ILogger>();
            builder.RegisterType<AccountActor>();
            builder.RegisterType<CustomerActor>();
            builder.RegisterType<CustomerClerkActor>();
            builder.RegisterType<AccountClerkActor>();
            builder.RegisterInstance(_system).As<ActorSystem>();
            builder.RegisterType<DbContextFactory>().As<IDbContextFactory>();
            builder.RegisterInstance(options).As<DbContextOptions<BankkaContext>>();
            var container = builder.Build();
            return container;
        }


        public void Stop()
        {
            Log.Information("Shutting down");
            CoordinatedShutdown.Get(_system).Run().Wait(TimeSpan.FromSeconds(2));
        }

        private static Config GetConfig()
        {
            var configString = @"
                    akka {
                        actor { 
		                    provider = ""Akka.Cluster.ClusterActorRefProvider, Akka.Cluster""
	                            deployment {
								  /customerClerks {
									router = round-robin-pool
                                    nr-of-instances = 3
                                    cluster {
                                        enabled = on
                                        max-nr-of-instances-per-node = 3
                                        allow-local-routees = on
                                        use-role = core
                                    }
                                 }
								  /accountClerks {
									router = consistent-hashing-pool
                                    nr-of-instances = 3
                                    virtual-nodes-factor = 10
                                    cluster {
                                        enabled = on
                                        max-nr-of-instances-per-node = 3
                                        allow-local-routees = on
                                        use-role = core
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
                            roles = [core]
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
            try
            {
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
                });
                //10



                var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());  //11
                Environment.ExitCode = exitCode;

                //{

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
