using Proryv.AskueARM2.Server.DBAccess.Internal;
using Proryv.AskueARM2.Server.DBAccess.Internal.Enums;
using Proryv.AskueARM2.Server.DBAccess.Internal.TClasses;
using Proryv.AskueARM2.Server.DBAccess.Public.Calculation.ArchTech.Data;
using Proryv.AskueARM2.Server.DBAccess.Public.Utils;
using Proryv.AskueARM2.Server.DBAccess.Public.Utils.Data;
using Proryv.AskueARM2.Server.WCF;
using Proryv.Servers.Calculation.DBAccess.Common.Data;
using Proryv.Servers.Calculation.DBAccess.Interface;
using Proryv.Servers.Calculation.DBAccess.Interface.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Proryv.AskueARM2.Server.DBAccess.Public.Calculation.ArchTech
{
    /// <summary>
    /// Запрос тех данных по ТИ
    /// </summary>
    public class ArchTechTiRequester : ArchTechRequesterBase
    {
        public List<IntPair> TiIds;

        public ArchTechTiRequester(ArchTechRequestParams requestParams, IGrouping<enumTypeHierarchy, ArchTechRequestParam> objectIds) : base(requestParams)
        {
            RequestParams = requestParams;
            Errors = new StringBuilder();
            TiIds = objectIds
                .Select(id => new IntPair(id.ID.ID, id.ChannelType))
                .ToList();
        }
        #region Работаем с базой данных

        public override List<ArchTechArchive> InvokeReadArchive()
        {
            using (var cnctns = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = new SqlCommand("usp2_ArchTech_Select", cnctns))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 600;
                cmd.Parameters.AddWithValue("@DTStart", RequestParams.DtStart);
                cmd.Parameters.AddWithValue("@DTEnd", RequestParams.DtEnd);
                cmd.Parameters.AddWithValue("@UseLossesCoefficient", RequestParams.UseLossesCoefficient);
                cmd.Parameters.AddWithValue("@UseCoeffTransformation", RequestParams.UseCoeffTransformation);

                if (RequestParams.TechProfilePeriod.HasValue)
                {
                    cmd.Parameters.AddWithValue("@techProfilePeriod", RequestParams.TechProfilePeriod);
                }

                var userDefinedTypeTable = SQLTableTypeAdapter<IntPair>.ArchiveValuesToDataTable(TiIds);
                //Таблица с ТИ
                cmd.Parameters.AddWithValue("@TIArray", userDefinedTypeTable);
                cnctns.Open();
                //Читаем данные
                using (var dr = cmd.ExecuteReader())
                {
                    Dictionary<int, Dictionary<byte, List<TPeriodCoeff>>> coeffTransformationDict = null;
                    Dictionary<int, Dictionary<byte, List<TLossesCoefficient>>> lossesCoefficientsDict = null;

                    #region Таблица с коэфф. потерь для ТИ

                    if (RequestParams.UseLossesCoefficient)
                    {
                        lossesCoefficientsDict = ReadLossesCoefficientsDict(dr);
                        dr.NextResult();
                    }

                    #endregion

                    #region Чтение коэфф. трансформации

                    if (RequestParams.UseCoeffTransformation)
                    {
                        coeffTransformationDict = ReadCoeffTransformation(dr);
                        dr.NextResult();
                    }

                    #endregion

                    return ReadArchives(dr, coeffTransformationDict, lossesCoefficientsDict);
                }
            }
        }

        private Dictionary<int, Dictionary<byte, List<TLossesCoefficient>>> ReadLossesCoefficientsDict(SqlDataReader dr)
        {
            var lossesCoefficientsDict = new Dictionary<int, Dictionary<byte, List<TLossesCoefficient>>>();

            if (!dr.HasRows) return lossesCoefficientsDict;

            var tiIndx = dr.GetOrdinal("TI_ID");
            var channelIndx = dr.GetOrdinal("ChannelType");
            var startDtIndx = dr.GetOrdinal("StartDateTime");
            var endDtIndx = dr.GetOrdinal("FinishDateTime");
            var lossesCoefficientIndx = dr.GetOrdinal("LossesCoefficient");

            Dictionary<byte, List<TLossesCoefficient>> lossesCoefficientsByChannel = null;
            List<TLossesCoefficient> lossesCoefficients = null;
            var prevTiId = -1;
            byte prevChannel = 0;

            while (dr.Read())
            {
                var tiId = dr.GetInt32(tiIndx);
                if (tiId != prevTiId)
                {
                    lossesCoefficientsByChannel = new Dictionary<byte, List<TLossesCoefficient>>();
                    lossesCoefficientsDict[tiId] = lossesCoefficientsByChannel;

                    prevTiId = tiId;
                }

                var channelType = dr.GetByte(channelIndx);
                if (channelType != prevChannel)
                {
                    lossesCoefficients = new List<TLossesCoefficient>();
                    lossesCoefficientsByChannel[channelType] = lossesCoefficients;

                    prevChannel = channelType;
                }

                lossesCoefficients.Add(new TLossesCoefficient
                {
                    TI_ID = tiId,
                    StartDateTime = dr.GetDateTime(startDtIndx),
                    FinishDateTime = dr[endDtIndx] as DateTime?,
                    LossesCoefficient = dr.GetDouble(lossesCoefficientIndx),
                });
            }

            return lossesCoefficientsDict;
        }

        private Dictionary<int, Dictionary<byte, List<TPeriodCoeff>>> ReadCoeffTransformation(SqlDataReader dr)
        {
            var dbRequest = new List<Tuple<int, byte, TPeriodCoeff>>();

            if (dr.HasRows)
            {
                var coeffIndx = dr.GetOrdinal("Coeff");
                var startIndx = dr.GetOrdinal("StartDateTime");
                var endIndx = dr.GetOrdinal("FinishDateTime");
                var tiIndx = dr.GetOrdinal("ti_id");
                var channelIndx = dr.GetOrdinal("ChannelType");

                while (dr.Read())
                {
                    dbRequest.Add(new Tuple<int, byte, TPeriodCoeff>(dr.GetInt32(tiIndx), dr.GetByte(channelIndx), new TPeriodCoeff
                    {
                        PeriodValue = dr[coeffIndx] as double?,
                        StartDateTime = (DateTime)dr[startIndx],
                        FinishDateTime = dr[endIndx] as DateTime?,
                    }));
                }
            }

            return dbRequest
                .GroupBy(g => g.Item1)
                .ToDictionary(k => k.Key,
                    v => v.GroupBy(g => g.Item2).ToDictionary(k => k.Key, vv => vv.Select(vvvv => vvvv.Item3).ToList()));
        }

        private List<ArchTechArchive> ReadArchives(SqlDataReader dr,
            Dictionary<int, Dictionary<byte, List<TPeriodCoeff>>> coeffTransformationDict,
            Dictionary<int, Dictionary<byte, List<TLossesCoefficient>>> lossesCoefficientsDict)
        {
            var archiveDict = new Dictionary<int, Dictionary<byte, ArchTechArchive>>();
            var result = new List<ArchTechArchive>();

            var firstIndexUTC = RequestParams.DtStart.ClientToUtc(RequestParams.TimeZoneId);

            //-------------Индексы в базе-----------------
            var tiIdOrdinal = dr.GetOrdinal("TI_ID");
            var channelOrdinal = dr.GetOrdinal("ChannelType");
            var eventDateOrdinal = dr.GetOrdinal("EventDate");
            var validOrdinal = dr.GetOrdinal("ValidStatus");
            var techProfilePeriodOrdinal = dr.GetOrdinal("TechProfilePeriod");
            var numberFirstColumn = dr.GetOrdinal("VAL_01");

            int? prevTiId = null;
            byte? prevChannel = null;

            ArchTechArchive currArchTechArchive = null;

            var du = (double)RequestParams.UnitDigit;
            if (du == 0) du = 1.0;

            PeriodCoeffWorker<double> coeffTranformationWorker = null;
            PeriodCoeffWorker<double> coeffLossesWorker = null;

            while (dr.Read())
            {
                var baseEventHour = dr.GetDateTime(eventDateOrdinal);
                var baseEventHourClient = baseEventHour.ServerToClient(RequestParams.TimeZoneId);
                var techProfilePeriod = (int)dr[techProfilePeriodOrdinal];

                var offsetFromFirst = (baseEventHour.ServerToUtc() - firstIndexUTC).TotalMinutes / techProfilePeriod;
                var tiId = dr.GetInt32(tiIdOrdinal);
                var channelType = dr.GetByte(channelOrdinal);
                var validStatus = dr.GetInt64(validOrdinal);

                if (techProfilePeriod == 0) continue; //Неправильный тип

                # region Отслеживаем изменения параметра учета

                if (!prevTiId.HasValue || !prevChannel.HasValue || prevTiId != tiId || prevChannel != channelType)
                {
                    coeffTranformationWorker = null;

                    Dictionary<byte, ArchTechArchive> archivesByChannel;
                    if (!archiveDict.TryGetValue(tiId, out archivesByChannel) || archivesByChannel == null)
                    {
                        archivesByChannel = new Dictionary<byte, ArchTechArchive>();
                        archiveDict[tiId] = archivesByChannel;
                    }

                    if (!archivesByChannel.TryGetValue(channelType, out currArchTechArchive))
                    {
                        currArchTechArchive = new ArchTechArchive(
                            new ID_TypeHierarchy
                            {
                                ID = tiId,
                                TypeHierarchy = enumTypeHierarchy.Info_TI,
                            },
                            channelType,
                            (EnumTechProfilePeriod)techProfilePeriod,
                            firstIndexUTC
                        );

                        archivesByChannel[channelType] = currArchTechArchive;
                        result.Add(currArchTechArchive);
                    }

                    //currArchTechArchive =
                    //    archives.FirstOrDefault(a => a.RequestParam.TI_ID == tiId && a.RequestParam.ChannelType == channelType);

                    if (currArchTechArchive == null) continue;

                    #region Помошники помогающие быстро найти коэфф. для минутки

                    if (coeffTransformationDict != null)
                    {
                        Dictionary<byte, List<TPeriodCoeff>> coeffByChannelDictionary;
                        if (coeffTransformationDict.TryGetValue(tiId, out coeffByChannelDictionary) && coeffByChannelDictionary != null)
                        {
                            List<TPeriodCoeff> coeffs;
                            if (coeffByChannelDictionary.TryGetValue(channelType, out coeffs) &&
                                coeffs != null)
                            {
                                coeffTranformationWorker = new PeriodCoeffWorker<double>(coeffs,
                                RequestParams.DtStart, RequestParams.DtEnd,
                                new TPeriodCoeff
                                {
                                    PeriodValue = 1,
                                    StartDateTime = RequestParams.DtStart,
                                    FinishDateTime = RequestParams.DtEnd,
                                });
                            }
                        }
                    }

                    if (lossesCoefficientsDict != null)
                    {
                        Dictionary<byte, List<TLossesCoefficient>> lossesCoeffByChannelDictionary;
                        if (lossesCoefficientsDict.TryGetValue(tiId, out lossesCoeffByChannelDictionary) && lossesCoeffByChannelDictionary != null)
                        {
                            List<TLossesCoefficient> coeffs;
                            if (lossesCoeffByChannelDictionary.TryGetValue(channelType, out coeffs) &&
                                coeffs != null)
                            {
                                coeffLossesWorker = new PeriodCoeffWorker<double>(coeffs,
                                RequestParams.DtStart, RequestParams.DtEnd,
                                new TPeriodCoeff
                                {
                                    PeriodValue = 1,
                                    StartDateTime = RequestParams.DtStart,
                                    FinishDateTime = RequestParams.DtEnd,
                                });
                            }
                        }
                    }

                    #endregion

                    prevTiId = tiId;
                    prevChannel = channelType;
                }

                #endregion

                int firstColumn = numberFirstColumn, lastColumn;

                if (RequestParams.DtStart > baseEventHourClient)
                {
                    firstColumn += (int)Math.Ceiling((double)RequestParams.DtStart.Minute / techProfilePeriod); //Округляем в большую сторону
                }

                if (RequestParams.DtEnd < baseEventHourClient.AddHours(1))
                {
                    lastColumn = RequestParams.DtEnd.Minute / techProfilePeriod + numberFirstColumn;
                }
                else
                {
                    lastColumn = 60 / techProfilePeriod + numberFirstColumn;
                }

                if (coeffTranformationWorker != null)
                {
                    coeffTranformationWorker.GoNextPeriodIfNotTotal(baseEventHour, baseEventHour.AddHours(1));
                }

                if (coeffLossesWorker != null)
                {
                    coeffLossesWorker.GoNextPeriodIfNotTotal(baseEventHour, baseEventHour.AddHours(1));
                }

                for (var i = firstColumn; i <= lastColumn; i++)
                {
                    var val = dr[i] as double?;
                    if (!val.HasValue) continue; //Пропускаем пустые

                    var minuteIndex = i - numberFirstColumn;

                    VALUES_FLAG_DB currFlag;

                    if ((validStatus >> minuteIndex & 1) == 1)
                    {
                        currFlag = VALUES_FLAG_DB.NotCorrect;
                    }
                    else
                    {
                        currFlag = VALUES_FLAG_DB.DataCorrect;
                    }

                    if (val > 0)
                    {
                        //Коэфф. трансформации
                        IPeriodBase<double> currentCoeff;
                        if (coeffTranformationWorker.TryGetCurrentPeriodOrFindForHalfhour(minuteIndex,
                                                    out currentCoeff, techProfilePeriod) && currentCoeff.PeriodValue.HasValue)
                        {
                            val = Math.Round(val.Value * currentCoeff.PeriodValue.Value, 8);
                        }
                    }
                    else
                    {
                        if (val == -1)
                        {
                            currFlag = VALUES_FLAG_DB.DataNotComplete;
                        }
                        val = 0;
                    }

                    currArchTechArchive.ArchTechValues[(int)(offsetFromFirst + minuteIndex)] =
                        new TVALUES_DB
                        {
                            F_FLAG = currFlag,
                            F_VALUE = val.Value / du,
                        }; //(EnumTechProfilePeriod)techProfilePeriod)
                }
            }

            return result;
        }

        #endregion
    }
}
