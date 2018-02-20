using System;
using System.Collections.Generic;
using System.Text;
using Akka.Actor;
using bankka.Commands.Customers;

namespace bankka.Actors
{
    public class CoreActor : ReceiveActor
    {
        private readonly IActorRef _customerRouter;

        public CoreActor(IActorRef customerRouter)
        {
            _customerRouter = customerRouter;
            Receive<OpenAccountCommand>(command => { _customerRouter.Forward(command); });
        }
    }
}
