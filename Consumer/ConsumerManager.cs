using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class RabbitConfiguration
{
    public string HostName { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string ExchangeName { get; set; }        
    public RabbitQueueConfiguration EnrollQueue { get; set; }
}

public class RabbitQueueConfiguration
{
    public string Name { get; set; }
    public int Consumers { get; set; }
    public string MessageServiceName { get; set; }
}

public class ConsumerManager
{
    private readonly Settings _settings;
    private readonly IServiceProvider _services;

    private CancellationTokenSource _cts;
    private List<ConsumerTask> _consumerTasks;        

    public bool Enabled { get; private set; }

    public ConsumerManager(Settings settings, IServiceProvider services)
    {
        _settings = settings;            
        _services = services;

        _cts = new CancellationTokenSource();
    }

    public void Start()
    {
        if (!Enabled)
        {
            Enabled = true;

            if (_cts.IsCancellationRequested)
                _cts = new CancellationTokenSource();

            _consumerTasks = new List<ConsumerTask>();            

            // Inicia consumidores da fila de Mensagens
            for (int i = 0; i < _settings.Rabbit.EnrollQueue.Consumers; i++)
            {
                var scope = _services.CreateScope();
                var receivedMessageServices = scope.ServiceProvider.GetServices<IReceivedMessageService>();
                var enrollConsumerMessageService = receivedMessageServices.FirstOrDefault(h => h.GetType().Name == _settings.Rabbit.EnrollQueue.MessageServiceName);

                _consumerTasks.Add(new ConsumerTask(_cts.Token, enrollConsumerMessageService, new QueueParameters
                {
                    HostName = _settings.Rabbit.HostName,
                    UserName = _settings.Rabbit.UserName,
                    Password = _settings.Rabbit.Password,
                    QueueName = _settings.Rabbit.ExchangeName + _settings.Rabbit.EnrollQueue.Name
                }, scope));                    
            }           

            _consumerTasks.ForEach(c => { c.Start(); });
        }
    }

    public void Stop()
    {
        if (Enabled)
        {
            try
            {
                Enabled = false;

                if (_cts != null)
                    _cts.Cancel();

                _consumerTasks.ForEach(c => { c.Dispose(); });
            }
            catch (Exception) { /**/ }
        }
    }
}