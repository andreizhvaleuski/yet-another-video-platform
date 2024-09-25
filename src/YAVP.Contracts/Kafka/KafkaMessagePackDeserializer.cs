using Confluent.Kafka;
using MessagePack;

namespace YAVP.Contracts.Kafka;

public sealed class KafkaMessagePackDeserializer<T> : IDeserializer<T> where T : class
{
    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
#pragma warning disable CS8603 // Possible null reference return.
        return isNull
            ? null
            : MessagePackSerializer.Deserialize<T>(data.ToArray());
#pragma warning restore CS8603 // Possible null reference return.
    }
}
