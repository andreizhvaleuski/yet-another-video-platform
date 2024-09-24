using Confluent.Kafka;
using MessagePack;

public class KafkaMessagePackSerializer<T> : ISerializer<T> where T : class
{
    public byte[]? Serialize(T data, SerializationContext context)
    {
        return data is null
            ? null
            : MessagePackSerializer.Serialize(data);
    }
}
