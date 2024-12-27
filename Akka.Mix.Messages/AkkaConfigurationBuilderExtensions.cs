using Akka.Hosting;

namespace Akka.Mix.Messages;

public static class AkkaConfigurationBuilderExtensions
{
    /// <summary> Adds the Akka.Mix.Messages.Serializer to the Akka configuration. This serializer uses Google.Protobuf to serialize and deserialize messages. </summary> 
    public static AkkaConfigurationBuilder WithAkkaMixMessagesSerializer(this AkkaConfigurationBuilder builder)
    {
        return builder.WithCustomSerializer(
            typeof(MessageSerializer).Namespace!,
            MessageSerializer.MessageTypes,
            (system) => new MessageSerializer(system));
    }
}
 