using RabbitMQ.Client;

public class RabbitPublisher : IRabbitPublisher
{
    private readonly Settings _settings;
    private IConnection _connection;
    private IModel _model;
    private IBasicProperties _properties;
    private readonly object _queueWritelock = new object();

    public RabbitPublisher(Settings settings)
    {
        _settings = settings;

        var factory = new ConnectionFactory()
        {
            HostName = _settings.Rabbit.HostName,
            UserName = _settings.Rabbit.UserName,
            Password = _settings.Rabbit.Password,
            Port = AmqpTcpEndpoint.UseDefaultPort
        };

        _connection = factory.CreateConnection();
        _model = _connection.CreateModel();

        _properties = _model.CreateBasicProperties();            
        _properties.Persistent = true;

        _model.ExchangeDeclare(_settings.Rabbit.ExchangeName, "direct", durable: true, autoDelete: false, arguments: null);
    }

    ~RabbitPublisher()
    {
        if (_model.IsOpen)
            _model.Close();

        if (_connection.IsOpen)
            _connection.Close();
    }

    public void Enqueue(byte[] message, string routingKey)
    {
        lock (_queueWritelock)
        {
            _model.BasicPublish(_settings.Rabbit.ExchangeName, routingKey, _properties, message);
        }
    }    
}
