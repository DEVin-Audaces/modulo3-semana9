using Microsoft.Extensions.Configuration.EnvironmentVariables;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace modulo3_semana9_mensageria_academia_consumer
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Aerobico();
            Musculacao();

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }

        private static void Aerobico()
        {
            IConnectionFactory factory = ConnectionFactory();
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
            var aerobicoConsummerEvent = new EventingBasicConsumer(channel);

            aerobicoConsummerEvent.Received += (model, basicDeliverEventArgs) => 
            {
                basicDeliverEventArgs.RoutingKey = "aerobico-route";
                basicDeliverEventArgs.Exchange = "aerobico-exchange";

                var body = basicDeliverEventArgs.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                Console.WriteLine($"Mensagem recebido no consumer aerobico | {message}");
            };

            channel.BasicConsume(queue: "aerobico-queue",
                                 autoAck: true,
                                 consumer: aerobicoConsummerEvent);
        }

        private static void Musculacao()
        {
            IConnectionFactory factory = ConnectionFactory();
            var connection = factory.CreateConnection(); ;
            var channel = connection.CreateModel();

            var musculacaoConsumerEvent = new EventingBasicConsumer(channel);

            musculacaoConsumerEvent.Received += (model, basicDeliverEventArgs) =>
            {
                basicDeliverEventArgs.RoutingKey = "musculacao-route";
                basicDeliverEventArgs.Exchange = "musculacao-exchange";

                var body = basicDeliverEventArgs.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var treino = JsonSerializer.Deserialize<List<Treino>>(message);

                Console.WriteLine($"Mensagem recebido no consumer musculacao | {treino}");

                channel.BasicAck(deliveryTag: basicDeliverEventArgs.DeliveryTag, multiple: false);
            };

            channel.BasicConsume(queue: "musculacao-queue",
                                 autoAck: false,
                                 consumer: musculacaoConsumerEvent);

        }

        public record Treino(int Carga, int Repetir, string ExercicoExecutado, DateTime DataHora);

        //private static void AerobicoConsummerEvent_Received(object? sender, BasicDeliverEventArgs e)
        //{
        //    e.Exchange
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// Criar uma ConnectionFactory
        /// Configuracao da mensageria no consumidor
        /// </summary>
        /// <returns></returns>
        private static IConnectionFactory ConnectionFactory()
        {
            ConnectionFactory factory = new()
            {
                VirtualHost = "academia",
                Uri = new Uri("amqp://guest:guest@host.docker.internal:5672/")
            };

            return factory;
        }
    }
}