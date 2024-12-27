using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;
using Akka.Actor;
using Akka.Hosting;
using Akka.Serialization;
using Google.Protobuf;

namespace Akka.Mix.Messages;

/// <summary>
/// This is a custom serializer for the Akka.Mix.Messages namespace.    
/// It uses Google.Protobuf to serialize and deserialize messages.
/// </summary>
public sealed class MessageSerializer : SerializerWithStringManifest
{
    public override int Identifier => 841;

    private static readonly ImmutableDictionary<string, Type> _manifestTypes = ImmutableDictionary<string, Type>.Empty;
    private static readonly ImmutableDictionary<Type, string> _manifestStrings = ImmutableDictionary<Type, string>.Empty;
    private static readonly ImmutableDictionary<Type, Func<object, byte[]>> _toBytes = ImmutableDictionary<Type, Func<object, byte[]>>.Empty;
    private static readonly ImmutableDictionary<string, Func<byte[], object>> _fromBytes = ImmutableDictionary<string, Func<byte[], object>>.Empty;

    public static IEnumerable<Type> MessageTypes => _manifestTypes.Values;
    public static IEnumerable<string> MessageManifests => _manifestStrings.Values;

    private static readonly Func<object, byte[]> _ToByteArray = ToByteArray;
    private static byte[] ToByteArray(object msg)
    {
        if (msg is IMessage message)
            return message.ToByteArray();

        throw new Exception($"Message is not an instance of IMessage: {msg.GetType()}");
    }

    static MessageSerializer()
    {
        foreach (var type in GetDefinedMessageTypes())
        {
            string manifest = type.Name;
            try
            {
                _manifestTypes = _manifestTypes.Add(manifest, type);
                _manifestStrings = _manifestStrings.Add(type, manifest);
                _toBytes = _toBytes.Add(type, _ToByteArray);
                _fromBytes = _fromBytes.Add(manifest, GetParserInstance(type).ParseFrom);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to register message type {type} with manifest '{manifest}'", ex);
            }
        }
    }

    private static IEnumerable<Type> GetDefinedMessageTypes()
    {
        return Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(IMessage).IsAssignableFrom(t));
    }

    private static MessageParser GetParserInstance(Type type)
    {
        var flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;
        var parser = type.GetProperty("Parser", flags);
        return parser?.GetValue(null) as MessageParser ?? throw new Exception($"Parser property for type {type} not found or is null");
    }

    public MessageSerializer(ExtendedActorSystem system) : base(system)
    {
    }

    public override object FromBinary(byte[] bytes, string manifest)
    {
        if (_fromBytes.TryGetValue(manifest, out var fromBytes))
            return fromBytes(bytes);

        throw new Exception($"No deserializer found for manifest '{manifest}'");
    }

    public override string Manifest(object o)
    {
        if (_manifestStrings.TryGetValue(o.GetType(), out var manifestString))
            return manifestString;

        throw new Exception($"No manifest found for type {o.GetType()}");
    }

    public override byte[] ToBinary(object obj)
    {
        if (_toBytes.TryGetValue(obj.GetType(), out var toBytes))
            return toBytes(obj);

        throw new Exception($"No serializer found for type {obj.GetType()}");
    }
}
 