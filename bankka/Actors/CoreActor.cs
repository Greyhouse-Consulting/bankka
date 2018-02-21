using Akka.Actor;
using bankka.Commands.Customers;

namespace bankka.Actors
{
    public class CoreActor : ReceiveActor
    {
        public CoreActor(IActorRef clerkRouter)
        {
            Receive<NewCustomerCommand>(command => { clerkRouter.Forward(command); });
            Receive<OpenAccountCommand>(command => { clerkRouter.Forward(command); });
        }
    }
}
