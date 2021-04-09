using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.MessagePatterns;
using System;
using System.Threading;
using System.Threading.Tasks;

public class ConsumerTask: IDisposable
{
    private readonly IReceivedMessageService _receivedMessageService;
    private readonly IServiceScope _scope;

    private CancellationToken _cancelationToken;        
    private readonly QueueParameters _parameters;

    private Task _internalTask { get; set; }

    public ConsumerTask(CancellationToken cancelationToken, IReceivedMessageService receivedMessageService, QueueParameters parameters, IServiceScope scope)
    {
        _receivedMessageService = receivedMessageService;
        _scope = scope;
        _cancelationToken = cancelationToken;
        _parameters = parameters;
    }

    public void Start()
    {
        _internalTask = Task.Factory.StartNew(async () => { await ProcessMessage(); } );
    }

    private async Task ProcessMessage()
    {
        while (true)
        {
            if (_cancelationToken.IsCancellationRequested)
                break;

            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = _parameters.HostName,
                    UserName = _parameters.UserName,
                    Password = _parameters.Password,
                    Port = AmqpTcpEndpoint.UseDefaultPort
                };
                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        channel.BasicQos(0, 1, false);
                        channel.QueueDeclare(
                            queue: _parameters.QueueName,
                            durable: true,
                            exclusive: false,
                            autoDelete: false,
                            arguments: null);

                        var subscription = new Subscription(channel, _parameters.QueueName, false);

                        while (true)
                        {
                            if (_cancelationToken.IsCancellationRequested)
                                break;

                            BasicDeliverEventArgs basicDeliveryEventArgs = subscription.Next();

                            try
                            {
                                await _receivedMessageService.Process(basicDeliveryEventArgs.Body); 
                                subscription.Ack(basicDeliveryEventArgs);                                   
                            }
                            catch { /* O log de erro já será gravado pela classe que processa a mensagem */ }
                        }
                    }
                }
            }
            catch (Exception) { /**/ }
        }
    }

    public void Dispose()
    {
        if (_internalTask != null)
        {
            _internalTask.Wait();
            _internalTask.Dispose();
            _internalTask = null;
            _scope.Dispose();
        }
    }
}