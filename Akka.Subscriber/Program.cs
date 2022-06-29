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
    public static void Main(string[] args)
    {
        Console.WriteLine("NonSeedApp");

        var builder = new HostBuilder();
        builder.ConfigureServices((context, service) =>
        {
            service.AddAkka(pubSubSystem, options =>
            {
                options
                    .WithRemoting("Localhost", 0)
                    .WithClustering(new ClusterOptions
                    {
                        Roles = new string[] { "Subscriber" },
                        SeedNodes = new[] {
                        Address.Parse($"akka.tcp://{pubSubSystem}@localhost:8110")
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
                        actorSystem.ActorOf<Subscriber>("Subscriber01");
                        actorSystem.ActorOf<Subscriber>("Subscriber02");
                        actorSystem.ActorOf<Subscriber>("Subscriber03");
                        actorSystem.ActorOf<Subscriber>("Subscriber04");
                    });
            });
        });

        builder.Build().Run();

        Console.ReadLine();
    }

}