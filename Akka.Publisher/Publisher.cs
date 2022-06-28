
using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;

public partial class Program
{
    public sealed class Publisher : ReceiveActor
    {
        public Publisher()
        {
            var mediator = DistributedPubSub.Get(Context.System).Mediator;
            Receive<string>(input => mediator.Tell(new Publish("content", input.ToUpperInvariant())));
        }
    }
}
