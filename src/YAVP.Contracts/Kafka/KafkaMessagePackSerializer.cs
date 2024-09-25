using Confluent.Kafka;
using MessagePack;

namespace YAVP.Contracts.Kafka;

public sealed class KafkaMessagePackSerializer<T> : ISerializer<T> where T : class
{
    public byte[] Serialize(T data, SerializationContext context)
    {
#pragma warning disable CS8603 // Possible null reference return.
        return data is null
            ? null
            : MessagePackSerializer.Serialize(data);
#pragma warning restore CS8603 // Possible null reference return.
    }
}
