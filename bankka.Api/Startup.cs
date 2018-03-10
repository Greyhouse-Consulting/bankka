using System;
using Akka.Actor;
using Akka.Configuration;
using Akka.Routing;
using bankka.Api.Controllers;
using bankka.Api.Models;
using bankka.Api.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            services.AddMvc().AddFluentValidation();

            services.AddTransient<IValidator<CreateAccountModel>, CreateAccountModelValidator>();
            services.AddSingleton(typeof(ActorSystem), ActorSystem);
        }

        private void SetupAkka()
        {
            var config = GetConfig();

            ActorSystem = ActorSystem.Create("bankka", config);

            var router = ActorSystem.ActorOf(Props.Empty.WithRouter(FromConfig.Instance), "customerClerks");
            var accountClerks = ActorSystem.ActorOf(Props.Empty.WithRouter(FromConfig.Instance), "accountClerks");
            //var router = ActorSystem.ActorOf(Props.Empty.WithRouter(new BroadcastPool(4)), "tasker");
            SystemActors.CustomerActor = router;// ActorSystem.ActorOf(Props.Create(() => new CommandProcessor(router)), "commands");
            SystemActors.AccountClerks = accountClerks;// ActorSystem.ActorOf(Props.Create(() => new CommandProcessor(router)), "commands");
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
						        /customerClerks {
									router = round-robin-group
                                    routees.paths = [""/user/customerClerks""]
                                    nr-of-instances = 3 #
                                    cluster {
                                        enabled = on
                                        max-nr-of-instances-per-node = 3
                                        allow-local-routees = off
                                        use-role = core
                                    }
                                }
		
					             /accountClerks {
									router = consistent-hashing-group
                                    routees.paths = [""/user/accountClerks""]
                                    nr-of-instances = 3 #
                                    virtual-nodes-factor = 10
                                    cluster {
                                        enabled = on
                                        max-nr-of-instances-per-node = 3
                                        allow-local-routees = off
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
                                port = 16002
		                    }
	                    }     
											
	                    cluster {
		                    #will inject this node as a self-seed node at run-time
		                    seed-nodes = [""akka.tcp://bankka@127.0.0.1:4053""] #manually populate other seed nodes here, i.e. ""akka.tcp://lighthouse@127.0.0.1:4053"", ""akka.tcp://lighthouse@127.0.0.1:4044""
			                roles = [api]
	                    }
                    }
            ";
            return ConfigurationFactory.ParseString(configString);
        }
    }
}
