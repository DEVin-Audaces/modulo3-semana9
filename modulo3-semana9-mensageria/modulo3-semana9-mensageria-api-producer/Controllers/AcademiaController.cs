using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace modulo3_semana9_mensageria_api_producer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AcademiaController : ControllerBase
    {
        [HttpGet("aerobico")]
        public ActionResult<bool> Aerobico()
        {
            var factory = ConnectionFactory();

            //Connection com o broker (RabbitMQ)
            using (IConnection connection = factory.CreateConnection())
            {
                //Channel criando assim um virtual connection
                using (var channel = connection.CreateModel())
                {
                    while (true)
                    {
                        IDictionary<string, object> keyValuePairs = new Dictionary<string, object>();

                        KeyValuePair<string, object> keyValuePair = new KeyValuePair<string, object>("teste", 1);
                        keyValuePairs.Add(keyValuePair);

                        //Exchange do broker
                        channel.ExchangeDeclare(exchange: "aerobico-exchange", "topic", arguments: keyValuePairs);

                        //Queue do broker
                        channel.QueueDeclare(queue: "aerobico-queue",
                                             durable: false,
                                             exclusive: false,
                                             autoDelete: false);

                        //Binding do exchange para queue
                        channel.QueueBind(queue: "aerobico-queue",
                                          exchange: "aerobico-exchange",
                                          routingKey: "aerobico-route");

                        //mensagem
                        string mensagem = $"Ola mundo {DateTime.Now}";

                        //Converter em um array de bytes
                        var body = Encoding.UTF8.GetBytes(mensagem);

                        //publicar no broker
                        channel.BasicPublish(exchange: "aerobico-exchange",
                                             routingKey: "aerobico-route",
                                             body: body);
                    }
                }
            }


            return Ok(true);
        }

        [HttpGet("musculacao/{condicao}")]
        public ActionResult<bool> Musculacao(bool condicao)
        {

            var factory = ConnectionFactory();

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {

                    while (true)
                    {
                        channel.ExchangeDeclare("musculacao-exchange", "direct");
                        channel.QueueDeclare(queue: "musculacao-queue",
                                             durable: true,
                                             exclusive: false,
                                             autoDelete: false);

                        channel.QueueBind(queue: "musculacao-queue",
                                          exchange: "musculacao-exchange",
                                          routingKey: "musculacao-route");

                        List<Treino> treino = new List<Treino>();

                        if (condicao)
                        {
                            for (int i = 0; i < 1000; i++)
                            {
                                //Mais carga, menos repeticoes
                                treino.Add(new(50, 3, "supino", DateTime.Now));
                            }
                        }
                        else
                        {
                            for (int i = 0; i < 1500; i++)
                            {
                                //Mais repeticoes, menos carga
                                treino.Add(new(20, 5, "supino", DateTime.Now));
                            }
                        }

                        string jsonString = JsonSerializer.Serialize(treino);
                        var body = Encoding.UTF8.GetBytes(jsonString);

                        channel.BasicPublish(exchange: "musculacao-exchange",
                                             routingKey: "musculacao-route",
                                             body: body);

                        condicao = !condicao;
                    }
                }
            }

            return Ok(true);
        }

        /// <summary>
        /// Criar uma ConnectionFactory
        /// Configuracao da mensageria no producer
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

    public record Treino(int Carga, int Repetir, string ExercicoExecutado, DateTime DataHora);
}
