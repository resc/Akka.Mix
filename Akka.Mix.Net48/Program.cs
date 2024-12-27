using System.Reflection;
using Akka.Cluster.Hosting;
using Akka.Cluster.Hosting.SBR;
using Akka.DependencyInjection;
using Akka.Hosting;
using Akka.Mix.Messages;
using Akka.Remote.Hosting;
using Microsoft.Extensions.Hosting;

namespace Akka.Mix.Net48;


public class Program
{
    public static async Task Main(string[] args)
    {
        var name = "Akka-Mix";
        var builder = Host.CreateDefaultBuilder(args);
        builder.ConfigureServices(services =>
        {
            services.AddAkka(name, builder =>
            {
                builder
                    .WithAkkaMixMessagesSerializer()
                    .WithRemoting(options =>
                    {
                        options.HostName = "localhost";
                        options.Port = 8101;
                    })
                    .WithClustering(new ClusterOptions
                    {
                        SeedNodes = [
                            "akka.tcp://Akka-Mix@localhost:8100",
                            "akka.tcp://Akka-Mix@localhost:8101"
                            ],
                        SplitBrainResolver = SplitBrainResolverOption.Default,
                    })

                    .WithDistributedPubSub("")
                    .StartActors((system, registry) =>
                    {
                        var resolver = DependencyResolver.For(system);
                        system.ActorOf(resolver.Props<Pinger>(), "pinger");
                        system.ActorOf(resolver.Props<CountReporterManager>(), "plant-reporters");
                    });

            });
        });

        var host = builder.Build();
        await host.RunAsync();
    }
}
