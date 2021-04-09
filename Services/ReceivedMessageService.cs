using ProtoBuf;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

public class ReceivedMessageService : IReceivedMessageService
{
    public async Task Process(byte[] data)
    {
        // Deserializa mensagem lida da fila (RabbitMQ)
        RabbitMQ.ReceivedMessage receivedMessage;
        using (var st = new MemoryStream(data))
        {
            receivedMessage = Serializer.Deserialize<RabbitMQ.ReceivedMessage>(st);
        }

        // Lógica de processamento da msg
    }
}