using System.Configuration;
using Akka.Cluster.Hosting;
using Akka.Cluster.Hosting.SBR;
using Akka.DependencyInjection;
using Akka.Hosting;
using Akka.Mix.Messages;
using Akka.Remote.Hosting;
using Microsoft.Extensions.Hosting;

namespace Akka.Mix.Net60;

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
                        options.Port = 8100;
                    })
                    .WithClustering(new ClusterOptions
                    { 
                        SeedNodes = ["akka.tcp://Akka-Mix@localhost:8100"],
                        SplitBrainResolver = SplitBrainResolverOption.Default,
                        MinimumNumberOfMembers = 1,
                    })
                    .WithDistributedPubSub("")
                    .StartActors((system, registry) =>
                    {
                        var resolver = DependencyResolver.For(system);
                        system.ActorOf(resolver.Props<Ponger>(), "ponger");
                        system.ActorOf(resolver.Props<CountReportProcessor>(), CountReport.ProcessorName);
                    });
            });
        });

        var host = builder.Build();
        await host.RunAsync();
    }
}
