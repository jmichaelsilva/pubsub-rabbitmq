using System.Threading.Tasks;

public interface IReceivedMessageService
{
    Task Process(byte[] data);
}
