
using Akka.Actor;
using Akka.Cluster;
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
    static string clusterSystem = "Pub.Sub.System";
    static int port = 8110;
    public static void Main(string[] args)
    {
        Console.WriteLine("SeedApp");

        var appBuilder = new HostBuilder();
        appBuilder.ConfigureServices((context, service) =>
        {
            service.AddAkka(clusterSystem, options =>
            {
                options
                    .WithRemoting("localhost", port)
                    .WithClustering(new ClusterOptions
                    {
                        Roles = new string[] { "Publisher" },
                        SeedNodes = new[] {
                        Address.Parse($"akka.tcp://{clusterSystem}@localhost:{port}")
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
                        var pubsub = DistributedPubSub.Get(actorSystem);
                        var actor = actorSystem.ActorOf<Publisher>("Publisher");
                        actor.Tell("This Message Sent From Publisher");
                    });
            });

        });


        var app = appBuilder.Build();
        app.Run();
        
        Console.ReadLine();
    }
}
