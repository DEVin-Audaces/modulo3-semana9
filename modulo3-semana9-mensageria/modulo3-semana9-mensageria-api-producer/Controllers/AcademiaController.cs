using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Text;

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

        /// <summary>
        /// Criar uma ConnectionFactory
        /// Configuracao da mensageria
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
