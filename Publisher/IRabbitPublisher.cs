public interface IPublisher
{
    void Enqueue(byte[] message, string routingKey);
}