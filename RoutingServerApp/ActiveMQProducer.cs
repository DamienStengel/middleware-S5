using System;
using System.Diagnostics;
using System.ServiceModel.Channels;
using Apache.NMS;
using Apache.NMS.ActiveMQ;
using ISession = Apache.NMS.ISession;

namespace RoutingServer
{
    public class ActiveMQProducer
    {
        private readonly string brokerUri = "activemq:tcp://localhost:61616";
        private readonly string queueName = "itinerary";

        public void SendMessage(string message)
        {
            try
            {
                ConnectionFactory connectionFactory = new ConnectionFactory(brokerUri);
                using (IConnection connection = connectionFactory.CreateConnection())
                {
                    connection.Start();
                    using (ISession session = connection.CreateSession())
                    {
                        IDestination destination = session.GetQueue(queueName);
                        using (IMessageProducer producer = session.CreateProducer(destination))
                        {
                            ITextMessage messageQueue = session.CreateTextMessage(message);
                            producer.Send(messageQueue);
                        }
                        session.Close();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error in SendMessage: {ex}");
                throw;
            }

        }

    }
}