using System.Collections.Generic;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Proryv.AskueARM2.Server.DBAccess.Internal.DB;
using RabbitMQ.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Proryv.Servers.MQ.Publisher;

namespace Proryv.AskueARM2.Server.WCF
{
    /// <summary>
    /// Уведомления через сервер RabbitMQ
    /// </summary>
    public class RabbitMQNotifier : IAlarmNotifyer
    {
        ProryvMQPublisher publisher;

        public RabbitMQNotifier(string hostName, string userName, string password, string virtualHost, int port = 5672, byte deliveryMode = 2, string expiration = "36000000")
        {

            publisher = new ProryvMQPublisher(hostName, userName, password, virtualHost, port, deliveryMode, expiration);
        }

        public void NewAlarm(CurrentAlarmNotified alarm)
        {
            if (alarm.TI_ID != null)
                publisher.Pubish_Alarm_TI(alarm, alarm.Alarm.User_ID, alarm.TI_ID.Value);
            if (alarm.PS_ID != null)
                publisher.Pubish_Alarm_TI(alarm, alarm.Alarm.User_ID, alarm.PS_ID.Value);
            if (!string.IsNullOrEmpty(alarm.User_ID))
                publisher.Pubish_Alarm_User(alarm, alarm.Alarm.User_ID, alarm.User_ID);
            if (!string.IsNullOrEmpty(alarm.BalanceFreeHierarchyObject_UN))
                publisher.Pubish_Alarm_BalanceFreeHierarchyObject(alarm, alarm.Alarm.User_ID, alarm.BalanceFreeHierarchyObject_UN);
            if (!string.IsNullOrEmpty(alarm.BalancePS_UN))
                publisher.Pubish_Alarm_BalancePS(alarm, alarm.Alarm.User_ID, alarm.BalancePS_UN);
            if (!string.IsNullOrEmpty(alarm.Formula_UN))
                publisher.Pubish_Alarm_Formula(alarm, alarm.Alarm.User_ID, alarm.Formula_UN);
            if (!string.IsNullOrEmpty(alarm.Formula_UN))
                publisher.Pubish_Alarm_Master61968(alarm, alarm.Alarm.User_ID, alarm.Master61968System_ID);


        }

        public void AlarmConfirm(Guid alarmId, string userId)
        {

         
                publisher.Pubish_Alarm_Confirm(alarmId, userId);

        }

        public void Dispose()
        {
            publisher.Dispose();

        }
    }
}
