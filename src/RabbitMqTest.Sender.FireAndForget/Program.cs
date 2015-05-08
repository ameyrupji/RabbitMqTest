namespace RabbitMqTest.Sender.FireAndForget
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Nito.AspNetBackgroundTasks;
    using RabbitMQ.Client;

    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("\nSend Message: ");
                var message = Console.ReadLine();
                BackgroundTaskManager.Run(() =>
                {
                    Send.Message(message);
                });
            }
        }
    }


    class Send
    {
        public async static void Message(string message)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare("hello", false, false, false, null);

                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish("", "hello", null, body);
                    Console.WriteLine(" [x] Sent {0}", message);
                }
            }
        }
    }
}
