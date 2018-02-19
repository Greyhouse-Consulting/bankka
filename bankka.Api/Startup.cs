using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Akka.Routing;
using bankka.Api.Controllers;
using bankka.Commands.Customers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace bankka.Api
{
    public class Startup
    {
        protected static ActorSystem ActorSystem;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            SetupAkka();
            services.AddMvc();
            services.AddSingleton(typeof(ActorSystem), ActorSystem);
        }

        private void SetupAkka()
        {
            var config = GetConfig();

            ActorSystem = ActorSystem.Create("bankka", config);

            var router = ActorSystem.ActorOf(Props.Empty.WithRouter(FromConfig.Instance), "tasker");
            SystemActors.CommandActor = ActorSystem.ActorOf(Props.Create(() => new CommandProcessor(router)),
                "commands");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime applicationLifetime)
        {

            applicationLifetime.ApplicationStopping.Register(StopApplication);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }

        private void StopApplication()
        {
            CoordinatedShutdown.Get(ActorSystem).Run().Wait(TimeSpan.FromSeconds(2));
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
                                    routees.paths = [""/user/customer""]
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
                                port = 16002
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

    public  class CommandProcessor : ReceiveActor
    {
        protected readonly IActorRef CommandRouter;
        public class CreateAccount
        {
            public CreateAccount(string accountId)
            {
                AccountId = accountId;
            }

            public string AccountId { get; private set; }
        }
     
        private  void Receives()
        {
            Receive<CreateAccount>(x => 
            {
                try
                {
                    var openAccount = new OpenAccountCommand();
                    
                    CommandRouter.Tell(openAccount);
                    CommandRouter.Ask<Routees>(new GetRoutees())
                        .ContinueWith(tr => { Console.WriteLine("I'm here"); })
                        .PipeTo(Sender);

                    Sender.Tell("AccountCreated");

                }
                catch (Exception ex)
                {

                    Console.Write(ex.ToString());
                    throw;
                }
            });
        }

        public CommandProcessor(IActorRef commandRouter)
        {
            CommandRouter = commandRouter;
            Receives();
        }
    }
}
