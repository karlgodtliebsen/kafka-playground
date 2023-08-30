using Confluent.Kafka;

namespace InsideOut.Serdes;

public class KafkaRawDeserializer : IDeserializer<byte[]>
{
  public byte[] Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
  {
    return isNull ? default : data.ToArray();
  }
}
