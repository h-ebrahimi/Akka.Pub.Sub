using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;

public partial class Program
{
    public sealed class Subscriber : ReceiveActor
    {
        private readonly IActorRef _mediator;
        public Subscriber()
        {
            _mediator = DistributedPubSub.Get(Context.System).Mediator;
            _mediator.Tell(new Subscribe("content", Self));
            Receive<SubscribeAck>(ack =>
            {
                if (ack != null && ack.Subscribe.Topic == "content" && ack.Subscribe.Ref.Equals(Self))
                {
                    Become(Ready);
                }
            });
        }
        private void Ready()
        {
            Receive<string>(message => Console.WriteLine("Got {0}", message));
        }
    }

}