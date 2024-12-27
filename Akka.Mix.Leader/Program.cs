using Akka.Cluster.Hosting;
using Akka.Cluster.Hosting.SBR;
using Akka.DependencyInjection;
using Akka.Hosting;
using Akka.Mix.Messages;
using Akka.Remote.Hosting;
using Microsoft.Extensions.Hosting;

namespace Akka.Mix.Leader;

public class Program
{
    public static async Task Main(string[] args)
    {
        const string actorSystemName = "Akka-Mix";
        const string hostName = "localhost";
        const int myPort = 8100;
        const int otherPort = 8101;

        var builder = Host.CreateDefaultBuilder(args);
        builder.ConfigureServices(services =>
        {
            services.AddAkka(actorSystemName, builder =>
            { 
                builder
                    .WithAkkaMixMessagesSerializer()
                    .WithRemoting(options =>
                    {
                        options.HostName = hostName;
                        options.Port = myPort;
                    })
                    .WithClustering(new ClusterOptions
                    { 
                        SeedNodes = [
                            // we need all the nodes in the cluster defined here
                            // otherwise if the cluster-seed dies, the DistributedPubSub 
                            // will not re-start properly when the seed comes back up.                            
                            $"akka.tcp://{actorSystemName}@{hostName}:{myPort}", 
                            $"akka.tcp://{actorSystemName}@{hostName}:{otherPort}", 
                        ],
                        SplitBrainResolver = SplitBrainResolverOption.Default,
                    })
                    // Run DistributedPubSub on all nodes by specifying empty role
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
