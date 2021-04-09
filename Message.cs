using ProtoBuf;

[ProtoContract]
public class Message
{
    [ProtoMember(1)]
    public string MessageType { get; set; }

    [ProtoMember(2)]
    public Guid PersonId { get; set; }

    [ProtoMember(3)]
    public Guid ImageId { get; set; }

    [ProtoMember(4)]
    public DateTime ReceivedDate { get; set; }
}
