
using Akka.Actor;
using Akka.Cluster.Hosting;
using Akka.Cluster.Tools.PublishSubscribe;
using Akka.Hosting;
using Akka.Remote.Hosting;
using Microsoft.Extensions.Hosting;
using Petabridge.Cmd.Cluster;
using Petabridge.Cmd.Host;
using Petabridge.Cmd.Remote;

public partial class Program
{
    static string pubSubSystem = "PubSubSystem";
    static int port = 8110;
    public static IActorRef pubsub;
    public static IActorRef publisherActor;
    public static void Main(string[] args)
    {
        Console.WriteLine("SeedApp");

        var appBuilder = new HostBuilder();
        appBuilder.ConfigureServices((context, service) =>
        {
            service.AddAkka(pubSubSystem, options =>
            {
                options
                    .WithRemoting("localhost", port)
                    .WithClustering(new ClusterOptions
                    {
                        Roles = new string[] { "Publisher" },
                        SeedNodes = new[] {
                        Address.Parse($"akka.tcp://{pubSubSystem}@localhost:{port}")
                        }
                    })
                    .AddPetabridgeCmd(cmd =>
                    {
                        Console.WriteLine("   PetabridgeCmd Added");
                        cmd.RegisterCommandPalette(new RemoteCommands());
                        cmd.RegisterCommandPalette(ClusterCommands.Instance);
                    })
                    .StartActors((actorSystem, actorRegistery) =>
                    {
                        pubsub = DistributedPubSub.Get(actorSystem).Mediator;
                        publisherActor = actorSystem.ActorOf<Publisher>("Publisher");
                    });
            });

        });


        var app = appBuilder.Build();
        app.RunAsync();

        Thread.Sleep(2000);

        while (true)
        {
            var str = Console.ReadLine();
            if (str.Equals("exit", StringComparison.OrdinalIgnoreCase))
                break;
            publisherActor.Tell(str);
        }


    }
}
