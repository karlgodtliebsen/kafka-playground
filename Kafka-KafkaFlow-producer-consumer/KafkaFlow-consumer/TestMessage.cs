using System.Runtime.Serialization;

namespace Consumer;

[DataContract]
public class TestMessage
{
    [DataMember(Order = 1)]
    public string Text { get; set; }
}