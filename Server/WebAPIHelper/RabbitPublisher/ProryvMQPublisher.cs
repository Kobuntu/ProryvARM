using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RabbitMQ.Client;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Text;
using RabbitMQ.Client.Events;

namespace Proryv.Servers.MQ.Publisher
{




    public class ProryvMQPublisher : IDisposable
    {
        private readonly JsonSerializer serializer = new JsonSerializer() { Converters = { new IsoDateTimeConverter() } };
        public ConnectionFactory connectionFactory;
        protected IModel channel;
        IConnection con;
        IBasicProperties props;
        ///  const string currentqueueName =  // Текущие тревоги
        const string archiveQueueName = "/DB/Alarms_Archive/"; // Архивные тревоги

        public const string exchange = "ENERGO_DB";
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public ProryvMQPublisher(string hostName, string userName, string password, string virtualHost, int port = 5672, byte deliveryMode = 2, string expiration = "36000000")
        {

            try
            {


                Logger.Info($"ProryvMQPublisher({hostName},{userName},{password},{virtualHost},{5672},{deliveryMode},{expiration}");

                connectionFactory = new ConnectionFactory();
                connectionFactory.HostName = hostName;
                connectionFactory.UserName = userName;
                connectionFactory.Password = password;
                connectionFactory.VirtualHost = virtualHost;
                connectionFactory.Port = port;
                con = connectionFactory.CreateConnection();
                channel = con.CreateModel();
                Logger.Info($"ProryvMQPublisher() Create Model");
                props = channel.CreateBasicProperties();
                props.ContentType = "text/plain";
                props.DeliveryMode = 2;
                props.Expiration = expiration;

                channel.ExchangeDeclare(exchange, ExchangeType.Topic, true);
                Logger.Info($"ProryvMQPublisher() ExchangeDeclare {exchange}");

            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }


        }


        #region ALARMS


        public void Pubish_Alarm_TI(object alarm, string User_ID, int TI_ID)
        {
            var quename = $"{User_ID}.Alarms_Archive.Alarms_Archive_To_TI.{TI_ID}";

            var alarmJson = JsonConvert.SerializeObject(alarm);

            byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(alarmJson);

            channel.BasicPublish(exchange, quename, props, messageBodyBytes);
        }

        public void Pubish_Alarm_Formula(object alarm, string User_ID, string Formula_UN)
        {
            var quename = $"{User_ID}.Alarms_Archive.Alarms_Archive_To_Formula.{Formula_UN}";

            var alarmJson = JsonConvert.SerializeObject(alarm);

            byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(alarmJson);

            channel.BasicPublish(exchange, quename, props, messageBodyBytes);
        }

        public void Pubish_Alarm_Master61968(object alarm, string User_ID, int? master61968System_ID)
        {

            var quename = $"{User_ID}.Alarms_Archive.Alarms_Archive_To_Master61968_SlaveSystems.{master61968System_ID}";

            var alarmJson = JsonConvert.SerializeObject(alarm);

            byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(alarmJson);

            channel.BasicPublish(exchange, quename, props, messageBodyBytes);
        }

        public void Pubish_Alarm_BalancePS(object alarm, string User_ID, string BalancePS_UN)
        {
            var quename = $"{User_ID}.Alarms_Archive.Alarms_Archive_To_Balance_PS.{BalancePS_UN}";

            var alarmJson = JsonConvert.SerializeObject(alarm);

            byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(alarmJson);

            channel.BasicPublish(exchange, quename, props, messageBodyBytes);
        }

        public void Pubish_Alarm_BalanceFreeHierarchyObject(object alarm, string User_ID, string balanceFreeHierarchyObject_UN)
        {
            var quename = $"{User_ID}.Alarms_Archive.Alarms_Archive_To_Balance_FreeHierarchy.{balanceFreeHierarchyObject_UN}";

            var alarmJson = JsonConvert.SerializeObject(alarm);

            byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(alarmJson);

            channel.BasicPublish(exchange, quename, props, messageBodyBytes);
        }

        public void Pubish_Alarm_PS(object alarm, string User_ID, int PS_ID)
        {
            var quename = $"{User_ID}.Alarms_Archive.Alarms_Archive_To_PS.{PS_ID}";

            var alarmJson = JsonConvert.SerializeObject(alarm);

            byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(alarmJson);

            channel.BasicPublish(exchange, quename, props, messageBodyBytes);
        }

        public void Pubish_Alarm_User(object alarm, string User_ID, string UserID)
        {
            var quename = $"{User_ID}.Alarms_Archive.Alarms_Archive_To_User.{UserID}";

            var alarmJson = JsonConvert.SerializeObject(alarm);

            byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(alarmJson);

            channel.BasicPublish(exchange, quename, props, messageBodyBytes);
        }


        public void Pubish_Alarm_Confirm(Guid alarmId, string userId)
        {



            var quename = $"{userId}.Alarms_Archive.Alarms_Archive_Confirm.{alarmId}";

            var alarmJson = JsonConvert.SerializeObject(alarmId);

            byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(alarmJson);

            channel.BasicPublish(exchange, quename, props, messageBodyBytes);
        }

        public void Dispose()
        {
            // connectionFactory.dispose
            channel.Dispose();
            con.Dispose();


        }
        #endregion


        #region  IEC104UA


        public void Pubish_IEC104UAConvertor_NodeWriteValue(object datavalue, string NodeId)
        {

            Logger.Info($"ProryvMQPublisher Pubish_IEC104UAConvertor_NodeWriteValue ({datavalue},{NodeId})");

            try
            {
                if (datavalue != null)
                {
                    var quename = "IEC104UAConvertor." + NodeId;

                    var alarmJson = JsonConvert.SerializeObject(datavalue);


                    byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(alarmJson);

                    channel.BasicPublish(exchange, quename, props, messageBodyBytes);
                }
            }
            catch (Exception e)
            {

                Logger.Error(e);
                throw;
            }





        }


        public void Recieve()
        {
            using (var connection = connectionFactory.CreateConnection())
            using (var channel2 = connection.CreateModel())
            {
                var consumer = new EventingBasicConsumer(channel2);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;

                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(ea.RoutingKey + " [x] Received {0}", message);
                };

                var queue = "IEC104Listener_" + Guid.NewGuid();
                channel2.QueueDeclare(queue, false, false, true, new ConcurrentDictionary<string, object>());
                channel2.QueueBind(queue, exchange, "IEC104UAConvertor.*");

                channel2.BasicConsume(queue, true, consumer);




                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }

        }



        #endregion
    }
}
