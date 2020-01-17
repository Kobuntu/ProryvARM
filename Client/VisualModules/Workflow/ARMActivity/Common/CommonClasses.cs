using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;

namespace Proryv.Workflow.Activity.ARM
{

    public class LoginParams
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    //тип считывания мгновенных значений
    [Serializable]
    public enum enumEnrgyQualityRequestType
    { 
        Archive = 0,
        Last = 1,
    }

    public enum enumReturnProfile
    {
        ReturnOnlyMainValues = 0,
        ReturnOnlyCalcValues = 1,
    }


    [Serializable]
    public enum enumManualReadRequestType
    { 
        Integrals = 1,
        QualityValue = 2,
        TarifSchedule =3,
        Profile = 4,
        JournalEvents = 5,
    }

    [Serializable]
    public enum enumManualUspdRequestType
    {
        Reglaments = 1, //Регламентный опрос
        AccessMeters = 2, //Опрос перечня доступных ПУ
    }


    [Serializable]
    public enum enumManualReadRequestPriority
    {
        Normal = 0,
        Hight = 1,
    }

    [Serializable]
    public enum enumChanelType
    {
        AI = 1,//АП
        AO = 2,//АО
        RI = 3,//РП
        RO = 4,//РО
        T1AI = 51,//АП Тариф 1
        T1AO = 52,
        T1RI = 53,
        T1RO = 54,
        T2AI = 61,//АП Тариф 2
        T2AO = 62,//АО Тариф 2
        T2RI = 63,
        T2RO = 64,
        T3AI = 71,//АП Тариф 3
        T3AO = 72,
        T3RI = 73,
        T3RO = 74,    
    }

    [Serializable]
    public enum enumArchiveObjectType
    {
        Dict_HierLev3 = 2,
        Dict_PS = 3, 
        Info_TI = 4,
        Section = 5, 
        Info_TP = 8, 
        Formula = 11, 
        Info_Integral = 17,
    }

    [Serializable]
    public enum enumFormulsObjectType
    {
        Dict_HierLev1 = 0,
        Dict_HierLev2 = 1,
        Dict_HierLev3 = 2,
        Dict_PS = 3,
        Info_TI = 4,
    }


    [Serializable]
    public class TIinfo
    {
        public int TI_ID { get; set; }
        public int PS_ID { get; set; }
        public string TIName { get; set; }
        public byte TIType { get; set; }
        public string TIATSCode { get; set; }
        public byte Commercial { get; set; }
        public double Voltage { get; set; }
        public int? SectionNumber { get; set; }
        public byte AccountType { get; set; }
        public byte? PhaseNumber { get; set; }
        public byte? CustomerKind { get; set; }
    }

    [Serializable]
    public class BalanceInfo
    {
        public string UserName { get; set; }
        public string BalancePS_UN { get; set; }
        public string BalancePSName { get; set; }
        public string User_ID { get; set; }
        public System.Nullable<byte> HierLev1_ID { get; set; }
        public System.Nullable<int> HierLev2_ID { get; set; }
        public System.Nullable<int> HierLev3_ID { get; set; }
        public System.Nullable<int> PS_ID { get; set; }
        public System.Nullable<int> TI_ID { get; set; }
        public System.Nullable<byte> BalancePSType_ID { get; set; }
        public System.Nullable<bool> ForAutoUse { get; set; }
        public System.Nullable<double> HighLimit { get; set; }
        public System.Nullable<double> LowerLimit { get; set; }
        public System.DateTime DispatchDateTime { get; set; }
    }


    [Serializable]
    public class HierLev3BalanceInfo
    {
        public string UserName { get; set; }
        public string Balance_HierLev3_UN { get; set; }
        public string BalanceHierLev3Name { get; set; }
        public string User_ID { get; set; }
        public System.Nullable<byte> HierLev1_ID { get; set; }
        public System.Nullable<int> HierLev2_ID { get; set; }
        public System.Nullable<int> HierLev3_ID { get; set; }
        public System.Nullable<byte> BalanceHierLev3Type_ID { get; set; }
        public System.Nullable<bool> ForAutoUse { get; set; }
        public System.Nullable<double> HighLimit { get; set; }
        public System.Nullable<double> LowerLimit { get; set; }    
    }

    [Serializable]
    public class FormulaInfo 
    {
        public string UserName { get; set; }
        public string Formula_UN { get; set; }
        public string FormulaName { get; set; }
        public string User_ID { get; set; }
        public System.Nullable<byte> HierLev1_ID { get; set; }
        public System.Nullable<int> HierLev2_ID { get; set; }
        public System.Nullable<int> HierLev3_ID { get; set; }
        public System.Nullable<int> PS_ID { get; set; }
        public System.Nullable<int> TI_ID { get; set; }
        public byte FormulaType_ID { get; set; }
        public System.Nullable<double> HighLimit { get; set; }
        public System.Nullable<double> LowerLimit { get; set; }
        public System.Nullable<byte> FormulaClassification_ID { get; set; }
        public System.Nullable<double> Voltage { get; set; }
    }

    [Serializable]
    public class EventsJournalTI
    {
        public int TI_ID { get; set; }
        public System.DateTime EventDateTime { get; set; }
        public int EventCode { get; set; }
        public System.DateTime DispatchDateTime { get; set; }
        public System.Nullable<long> ExtendedEventCode { get; set; }
        public System.Nullable<byte> Event61968Domain_ID { get; set; }
        public System.Nullable<byte> Event61968DomainPart_ID { get; set; }
        public System.Nullable<byte> Event61968Type_ID { get; set; }
        public System.Nullable<int> Event61968Index_ID { get; set; }
        public string Event61968Param { get; set; }
    }


    public class UserHelper
    {
        private static UserInfo GetUserInfoByUserName(string userName)
        {
            List<UserInfo> uList = ARM_Service.EXPL_Get_All_Users();
            foreach (UserInfo u in uList)
            {
                if (u.UserName.ToLower(CultureInfo.InvariantCulture) == userName.ToLower(CultureInfo.InvariantCulture))
                {
                    return u;
                }
            }
            return null;
        }

        public static string GetEmailByUserName(string userName)
        {
            UserInfo uInfo = GetUserInfoByUserName(userName);
            if (uInfo != null)
                return uInfo.Email;
            return null;
        }

        public static string GetIdByUserName(string userName)
        {
            UserInfo uInfo = GetUserInfoByUserName(userName);
            if (uInfo != null)
                return uInfo.User_ID;
            return null;
        }
    }

}
