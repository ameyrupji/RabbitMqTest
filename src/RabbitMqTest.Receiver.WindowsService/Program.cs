using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMqTest.Receiver.WindowsService
{
    using System.Threading;
    using RabbitMQ.Client;
    using log4net;
    using log4net.Config;
    using Topshelf;

    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                log4net.Config.XmlConfigurator.Configure();
                x.Service<TownCrier>(s =>
                {
                    s.ConstructUsing(name => new TownCrier());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAsLocalSystem();

                x.SetDescription("Sample Topshelf Host");
                x.SetDisplayName("Stuff");
                x.SetServiceName("stuff");
            }); 
        }
    }

    public class TownCrier
    {
        readonly System.Timers.Timer _timer;
        public TownCrier()
        {
            _timer = new System.Timers.Timer(1000) { AutoReset = true };
            _timer.Elapsed += (sender, eventArgs) => Receive.Message();
        }
        public void Start() { _timer.Start(); }
        public void Stop() { _timer.Stop(); }
    }

    class Receive
    {
        public static void Message()
        {
            ILog log = LogManager.GetLogger(typeof(Receive)); 
 
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare("hello", false, false, false, null);

                    var consumer = new QueueingBasicConsumer(channel);
                    channel.BasicConsume("hello", true, consumer);

                    log.Debug(" [*] Waiting for messages.");
                    while (true)
                    {
                        Thread.Sleep(10000);
                        var ea = consumer.Queue.Dequeue();

                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        log.Debug(String.Format(" [x] Received {0}", message));
                    }
                }
            }
        }
    }
}
