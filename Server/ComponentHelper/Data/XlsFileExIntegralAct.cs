using FlexCel.Core;
using Proryv.AskueARM2.Server.DBAccess.Internal;
using Proryv.AskueARM2.Server.DBAccess.Internal.TClasses;
using Proryv.AskueARM2.Server.DBAccess.Public.Calculation.Balances.Intefaces;
using Proryv.AskueARM2.Server.VisualCompHelpers.Calculators;
using Proryv.AskueARM2.Server.WCF;
using Proryv.Servers.Calculation.DBAccess.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Proryv.Servers.Calculation.DBAccess.Common.Data;

namespace Proryv.AskueARM2.Server.VisualCompHelpers.Data
{
    public class XlsFileExIntegralAct: XlsFileExBase
    {
        private readonly XlsFileParamIntegralAct _paramIntegralAct;

        #region Конструкторы

        public XlsFileExIntegralAct(XlsFileParamIntegralAct paramIntegralAct) : base()
        {
            _paramIntegralAct = paramIntegralAct;
        }
        public XlsFileExIntegralAct(XlsFileParamIntegralAct paramIntegralAct, bool aAllowOverwritingFiles) : base(aAllowOverwritingFiles)
        {
            _paramIntegralAct = paramIntegralAct;
        }
        public XlsFileExIntegralAct(XlsFileParamIntegralAct paramIntegralAct, string aFileName) : base(aFileName)
        {
            _paramIntegralAct = paramIntegralAct;
        }
        public XlsFileExIntegralAct(XlsFileParamIntegralAct paramIntegralAct, string aFileName, bool aAllowOverwritingFiles) : base(aFileName, aAllowOverwritingFiles)
        {
            _paramIntegralAct = paramIntegralAct;
        }
        public XlsFileExIntegralAct(XlsFileParamIntegralAct paramIntegralAct, int aSheetCount, bool aAllowOverwritingFiles) : base(aSheetCount, aAllowOverwritingFiles)
        {
            _paramIntegralAct = paramIntegralAct;
        }
        public XlsFileExIntegralAct(XlsFileParamIntegralAct paramIntegralAct, Stream aStream, bool aAllowOverwritingFiles) : base(aStream, aAllowOverwritingFiles)
        {
            _paramIntegralAct = paramIntegralAct;
        }
        public XlsFileExIntegralAct(XlsFileParamIntegralAct paramIntegralAct, int aSheetCount, TExcelFileFormat aFileFormat, bool aAllowOverwritingFiles) : base(aSheetCount, aFileFormat, aAllowOverwritingFiles)
        {
            _paramIntegralAct = paramIntegralAct;
        }

        #endregion

        /// <summary>
        /// Пишем информацию о смене счетчика
        /// </summary>
        /// <param name="integralActTi"></param>
        /// <param name="row"></param>
        /// <param name="comment"></param>
        /// <param name="tiNum"></param>
        /// <param name="channelName"></param>
        /// <param name="firstVal"></param>
        /// <param name="lastVal"></param>
        /// <returns></returns>
        public bool WriteChangePuInfo(List<TMetersTO_Information> metersInformations, string name, double voltage,
            StringBuilder comment, int tiNum, string channelName,
            TINTEGRALVALUES_DB firstVal, TINTEGRALVALUES_DB lastVal
            )
        {
            if (metersInformations == null || metersInformations.Count == 0) return false;

            var to = metersInformations.First();

            //Первая запись
            if (string.IsNullOrEmpty(to.MeterSerialNumberBefo))
            {
                //Это начало действия нового стчетчика
                comment.AppendFormat("Установка ПУ №{0}\n"
                                     + "дата установки {1:dd.MM.yyyy HH:mm}\n"
                                     + "первое значение после установки{2:" +
                                     IntegralFormat + "}\n",
                    to.MeterSerialNumberAfter, to.ExchangeDateTime,
                    to.DataAfter / (double)_paramIntegralAct.UnitDigit);
            }
            else
            {
                comment.AppendFormat("Замена ПУ №{0} на №{1}\n"
                                     + "дата замены {2:dd.MM.yyyy HH:mm}\n"
                                     + "последнее значение перед заменой {3:0.#######}\n"
                                     + "первое значение после замены {4:0.#######}\n",
                to.MeterSerialNumberBefo, to.MeterSerialNumberAfter, to.ExchangeDateTime, to.DataBefo / (double)_paramIntegralAct.UnitDigit, to.DataAfter / (double)_paramIntegralAct.UnitDigit);

                SetRowHeight(Row, 2 * 250);

                //Информация до смены
                SetCellValue(Row, StartCol + 3, to.MeterSerialNumberBefo + " ", CenterFormatThin); //старый номер ПУ
                SetCellValue(Row, StartCol + 5, to.DataBefo / (double)_paramIntegralAct.UnitDigit, IntegralFormat); // показания старого ПУ на начало периода

                if (firstVal != null)
                {
                    SetCellValue(Row, StartCol + 6, (firstVal.F_VALUE) / (double)_paramIntegralAct.UnitDigit, IntegralFormat); // последние данные старого ПУ

                    if ((firstVal.F_FLAG & VALUES_FLAG_DB.AllAlarmStatuses) != VALUES_FLAG_DB.None
                        || (firstVal.EventDateTime - _paramIntegralAct.DTStart).TotalDays > 1)
                    {
                        SetCellBkColor(Row, StartCol + 6, _noDrumsColor);
                        SetComment(Row, StartCol + 6,
                            firstVal.EventDateTime.ToString("dd.MM.yyyy HH:mm") +
                            " :\n" + firstVal.F_FLAG.FlagToString("\n"));
                        SetCommentProperties(Row, StartCol + 6, _commentProps);
                    }
                }

                SetCellFloatValue(Row, StartCol + 7, to.IntDataBefor / (double)_paramIntegralAct.UnitDigit, true, need0: false); // Разница
                SetCellFloatValue(Row, StartCol + 8, to.CoeffTransformationBefo, true, need0: false); // Коэффициент ПУ до смены

                SetCellValue(Row, StartCol + 9, to.HhDataBefor / (double)_paramIntegralAct.UnitDigit, IntegralFormat); // Получасовки до смены
                                                                                                                       //if (isInterval) SetCellValue(Row, StartCol + 10, to.HhDataBefor, IntegralFormat); // Получасовки до смены
                Row++;
            }

            //промежуточные записи
            foreach (var po in metersInformations.Skip(1).Take(metersInformations.Count - 1))
            {
                comment.AppendFormat("Замена ПУ №{0} на №{1}\n"
                                     + "дата замены {2:dd.MM.yyyy HH:mm}\n"
                                     + "последнее значение перед заменой {3:0.#######}\n"
                                     + "первое значение после замены {4:0.#######}\n",
                po.MeterSerialNumberBefo, po.MeterSerialNumberAfter, po.ExchangeDateTime, po.DataBefo / (double)_paramIntegralAct.UnitDigit, po.DataAfter / (double)_paramIntegralAct.UnitDigit);

                SetRowHeight(Row, 2 * 250);

                //Информация до смены
                SetCellValue(Row, StartCol + 1, name + " после замены ", LeftFormatThin);
                SetCellValue(Row, StartCol + 2, voltage, CenterFormatThin);
                SetCellValue(Row, StartCol + 3, po.MeterSerialNumberBefo + " ", CenterFormatThin); //старый номер ПУ
                SetCellValue(Row, StartCol + 4, channelName, CenterFormatThin);
                SetCellValue(Row, StartCol + 5, po.DataBefo / (double)_paramIntegralAct.UnitDigit, IntegralFormat); // показания старого ПУ на начало периода
                SetCellValue(Row, StartCol + 6, to.DataAfter / (double)_paramIntegralAct.UnitDigit, IntegralFormat); // последние данные старого ПУ
                SetCellFloatValue(Row, StartCol + 7, po.IntDataBefor / (double)_paramIntegralAct.UnitDigit, true, need0: false); // Разница
                SetCellFloatValue(Row, StartCol + 8, po.CoeffTransformationBefo, true, need0: false); // Коэффициент ПУ до смены

                SetCellValue(Row, StartCol + 9, po.IntDataBefor / (double)_paramIntegralAct.UnitDigit * po.CoeffTransformationBefo, IntegralFormat); // Получасовки до смены
                to = po; //Сохраняем предыдущее
                Row++;
            }

            //Последняя запись
            to = metersInformations.Last();
            
            SetRowHeight(Row, 2 * 250);

            //Последняя строка со значениями после смены
            //SetCellValue(Row, _startCol, tiNum, 0);
            SetCellValue(Row, StartCol + 1, name + " после замены ", LeftFormatThin);
            SetCellValue(Row, StartCol + 2, voltage, CenterFormatThin);
            //SetCellAlignH(xls, Row, startCol + 2, THFlxAlignment.center);
            SetCellValue(Row, StartCol + 4, channelName, CenterFormatThin);
            SetCellValue(Row, StartCol + 3, to.MeterSerialNumberAfter + " ", CenterFormatThin); //новый номер ПУ
            SetCellValue(Row, StartCol + 6, to.DataAfter / (double)_paramIntegralAct.UnitDigit, IntegralFormat); // показания нового ПУ на конец периода

            if (lastVal != null)
            {
                SetCellValue(Row, StartCol + 5, (lastVal.F_VALUE) / (double)_paramIntegralAct.UnitDigit, IntegralFormat); // Первые данные после смены
                if ((lastVal.F_FLAG & VALUES_FLAG_DB.AllAlarmStatuses) != VALUES_FLAG_DB.None
                    || (_paramIntegralAct.DTEnd - lastVal.EventDateTime).TotalDays > 1)
                {
                    SetCellBkColor(Row, StartCol + 5, _noDrumsColor);
                    SetComment(Row, StartCol + 5, lastVal.EventDateTime.ToString("dd.MM.yyyy HH:mm") +
                        " :\n" + lastVal.F_FLAG.FlagToString("\n"));
                    SetCommentProperties(Row, StartCol + 5, _commentProps);
                }
            }

            SetCellFloatValue(Row, StartCol + 7, to.HhDataAfter / (double)_paramIntegralAct.UnitDigit / to.CoeffTransformationAfter,
                                true, need0: Need0); // Разница показаний

            SetCellFloatValue(Row, StartCol + 8, to.CoeffTransformationAfter, true, need0: false); // Коэффициент ПУ после смены
            SetCellValue(Row, StartCol + 9, to.HhDataAfter / (double)_paramIntegralAct.UnitDigit, IntegralFormat); // Получасовки после смены

            return true;
        }

        public bool WriteChangeTransformatorInfo(List<TTransformators_Information> transformatorsInformation,
            string name, double voltage, string meterSerialNumber,
            StringBuilder comment, int tiNum, string channelName)
        {
            if (transformatorsInformation == null || transformatorsInformation.Count == 0) return false;

            for (var i = 0; i < transformatorsInformation.Count; i++)
            {
                //isKtChanged = true;

                var to = transformatorsInformation[i];

                DateTime dt = to.ExchangeDateTime.ServerToClient(_paramIntegralAct.TimeZoneId);

                var strK =
                    (to.TransformatorsChangedType &
                     enumTransformatorsChangedType.ChangedCoefI) ==
                    enumTransformatorsChangedType.ChangedCoefI
                        ? "Кт"
                        : "Кн";

                comment.AppendFormat("Замена {0}  {1}\n"
                                     + "дата замены {2:dd.MM.yyyy HH:mm}\n"
                                     + "значение {5} перед заменой {3:##}\n"
                                     + "значение {5} после замены {4:##}\n",
                    to.TransformatorsChangedString, to.TiName,
                    dt, to.CoeffTransformationBefo, to.CoeffTransformationAfter, strK);

                //Внимание возможно наткнулись на пересекающийся диапазон!!! 
                //Пишем это в комментарий, т.к. это косяк
                if (dt < _paramIntegralAct.DTStart || dt > _paramIntegralAct.DTEnd)
                {
                    comment.Append(
                        "(Возможно произошло пересечение диапазонов \n времени действия трансформатора!!!)");
                }

                double? value = null;

                SetCellFloatValue(Row, StartCol + 8, to.CoeffTransformationBefo,
                    true, false); // Коэффициент ПУ до смены
                SetCellFloatValue(Row, StartCol + 9, to.HhDataBefor,
                    true, need0: Need0); // Барабан после смены
                SetCellFloatValue(Row, StartCol + 10, to.HhDataBefor,
                    false, need0: Need0); // Получасовки после смены

                SetRowHeight(Row, 2 * 250);

                Row++;

                //Новая строка со значениями после смены
                SetCellValue(Row, StartCol, tiNum, CenterFormatThin);
                SetCellValue(Row, StartCol + 1, name, LeftFormatThin);
                SetCellValue(Row, StartCol + 2, voltage, CenterFormatThin);
                //SetCellAlignH(_xls, Row, StartCol + 2, THFlxAlignment.center);
                SetCellValue(Row, StartCol + 3, meterSerialNumber + " ",
                    CenterFormatThin);
                SetCellValue(Row, StartCol + 4, channelName, CenterFormatThin);

                SetCellFloatValue(Row, StartCol + 8, to.CoeffTransformationAfter,
                    true, false); // Коэффициент ПУ после смены
                SetCellFloatValue(Row, StartCol + 9, to.HhDataAfter,
                    true, need0: Need0); // Барабан после смены
                SetCellFloatValue(Row, StartCol + 10, to.HhDataAfter,
                    false, need0: Need0); // Получасовки после смены

                //Считаем сумму барабанов 
                //drumVal += to.HhDataBefor;
                //if (i == transformatorsInformation.Count - 1)
                //{
                //    drumVal += to.HhDataAfter;
                //}

                SetRowHeight(Row, 3 * 250);
            }

            return true;
        }

        /// <summary>
        /// Пишем интегралы
        /// </summary>
        /// <param name="firstVal"></param>
        /// <param name="lastVal"></param>
        /// <param name="metersDelta"></param>
        /// <param name="deltaValWithCoeff"></param>
        /// <param name="dtStart"></param>
        /// <param name="dtEnd"></param>
        /// <param name="commentProps"></param>
        public void WriteIntegralInfo(TINTEGRALVALUES_DB firstVal, TINTEGRALVALUES_DB lastVal,
            double metersDelta, double deltaValWithCoeff, StringBuilder comment)
        {
            var coeffUnit = ((double)_paramIntegralAct.UnitDigit);
            var isIntegralThroughZero = false;

            if (lastVal != null)
            {
                SetCellFloatValue(Row, StartCol + 5, ((lastVal.F_VALUE) / coeffUnit), true, need0: Need0); // на конеч
                if ((lastVal.F_FLAG & VALUES_FLAG_DB.AllAlarmStatuses) != VALUES_FLAG_DB.None || (_paramIntegralAct.DTEnd - lastVal.EventDateTime).TotalDays > 1)
                {
                    SetCellBkColor(Row, StartCol + 5, _noDrumsColor);
                    SetComment(Row, StartCol + 5, lastVal.EventDateTime.ToString("dd.MM.yyyy HH:mm") + " :\n" + lastVal.F_FLAG.FlagToString("\n"));
                    SetCommentProperties(Row, StartCol + 5, _commentProps);
                }
                
                if (lastVal.F_FLAG.HasFlag(VALUES_FLAG_DB.IsIntegralThroughZero)) //Обработка перехода через 0 
                {
                    isIntegralThroughZero = true;
                    SetCellBkColor(Row, StartCol + 5, _noDrumsColor);
                    SetComment(Row, StartCol + 5, lastVal.F_FLAG.FlagToString("\n"));
                    SetCommentProperties(Row, StartCol + 5, _commentProps);
                }
            }
            else
            {
                SetComment(Row, StartCol + 5, "Показание не найдено, или отсутствует");
                SetCellFormat(Row, StartCol + 5, IntegralFormat);
                SetCellBkColor(Row, StartCol + 5, _noDrumsColor);
            }

            if (firstVal != null)
            {
                SetCellFloatValue(Row, StartCol + 6, ((firstVal.F_VALUE) / coeffUnit), true, need0: Need0); // на начало
                if ((firstVal.F_FLAG & VALUES_FLAG_DB.AllAlarmStatuses) != VALUES_FLAG_DB.None || (firstVal.EventDateTime - _paramIntegralAct.DTStart).TotalDays > 1)
                {
                    SetCellBkColor(Row, StartCol + 6, _noDrumsColor);
                    SetComment(Row, StartCol + 6, firstVal.EventDateTime.ToString("dd.MM.yyyy HH:mm") + " :\n" + firstVal.F_FLAG.FlagToString("\n"));
                    SetCommentProperties(Row, StartCol + 6, _commentProps);
                }

                if (firstVal.F_FLAG.HasFlag(VALUES_FLAG_DB.IsIntegralThroughZero)) //Обработка перехода через 0 
                {
                    isIntegralThroughZero = true;
                    SetCellBkColor(Row, StartCol + 6, _noDrumsColor);
                    SetComment(Row, StartCol + 6, firstVal.F_FLAG.FlagToString("\n"));
                    SetCommentProperties(Row, StartCol + 6, _commentProps);
                }
            }
            else
            {
                SetComment(Row, StartCol + 6, "Показание не найдено, или отсутствует");
                SetCellFormat(Row, StartCol + 6, IntegralFormat);
                SetCellBkColor(Row, StartCol + 6, _noDrumsColor);
            }

            SetCellFloatValue(Row, StartCol + 7, metersDelta / coeffUnit, true, need0: Need0); // разность

            SetCellFloatValue(Row, StartCol + 9, deltaValWithCoeff / coeffUnit, true, need0: Need0); //Количество энергии учтеной ПУ

            //Считаем сумму барабанов 
            //drumVal += iVal.DiffWithCoeff / 1000;

            //Достоверность для ПУ
            if (firstVal == null || firstVal.F_FLAG.HasFlag(VALUES_FLAG_DB.DataNotFull)
                || lastVal == null || lastVal.F_FLAG.HasFlag(VALUES_FLAG_DB.DataNotFull))
            {
                SetCellBkColor(Row, StartCol + 9, _noDrumsColor);
            }
            else if (firstVal.F_FLAG.HasFlag(VALUES_FLAG_DB.UpToTimeZone) 
                || lastVal.F_FLAG.HasFlag(VALUES_FLAG_DB.UpToTimeZone))
            {
                SetCellBkColor(Row, StartCol + 9, _offsetFromMoscowEnbledForDrumsColor);
                SetComment(Row, StartCol + 9, (firstVal.F_FLAG | lastVal.F_FLAG).FlagToString("\n"));
                SetCommentProperties(Row, StartCol + 9, _commentProps);
            }

            if (isIntegralThroughZero)
            {
                comment.Append("Переход через 0\n");
            }
        }

        /// <summary>
        /// Пишем информацию об ОВ
        /// </summary>
        /// <returns></returns>
        public bool WriteOvInfo(IEnumerable<TOV_Values> ovs, string tiName, byte channel,
            double voltage, Action after,  bool isOvExcludedFromTi = false)
        {
            if (ovs == null || !ovs.Any()) return false;

            foreach (var ov in ovs)
            {
                if (ov == null) continue;

                var coeffUnit = ((double)ov.UnitDigit / (double)_paramIntegralAct.UnitDigit);

                var channelName = ChannelNameByNumber(channel);
                var ovName = ov.Name + " на присоединении ";
                
                SetCellValue(Row, StartCol + 1, ovName + tiName, LeftFormatThin);
                SetCellValue(Row, StartCol + 2, voltage, CenterFormatThin);
                SetCellValue(Row, StartCol + 3, ov.MeterSerialNumber + " ", CenterFormatThin);
                SetCellValue(Row, StartCol + 4, channelName, CenterFormatThin);
                

                if (ov.Val_ListDrum != null) //Если есть значения барабанов
                {
                    var deltaVal = ov.Val_ListDrum.Diff; //Это запрашиваем в Вт
                    var firstVal = ov.Val_ListDrum.First;
                    var lastVal = ov.Val_ListDrum.Last;

                    if (lastVal != null)
                    {
                        SetCellFloatValue(Row, StartCol + 5, lastVal.F_VALUE / (double)_paramIntegralAct.UnitDigit, true, Need0); // на конеч
                        SetCellBkColor(Row, StartCol + 5, _noDrumsColor);
                        SetComment(Row, StartCol + 5,
                            lastVal.EventDateTime.AddMinutes(30).ToString("dd.MM.yyyy HH:mm") + " :\n" +
                            lastVal.F_FLAG.FlagToString("\n"));
                        SetCommentProperties(Row, StartCol + 5, _commentProps);
                    }

                    if (firstVal != null)
                    {
                        SetCellFloatValue(Row, StartCol + 6, firstVal.F_VALUE / (double)_paramIntegralAct.UnitDigit, true, Need0); // на начало

                        SetCellBkColor(Row, StartCol + 6, _noDrumsColor);
                        SetComment(Row, StartCol + 6,
                            firstVal.EventDateTime.ToString("dd.MM.yyyy HH:mm") + " :\n" +
                            firstVal.F_FLAG.FlagToString("\n"));
                        SetCommentProperties(Row, StartCol + 6, _commentProps);
                    }


                    SetCellFloatValue(Row, StartCol + 7, deltaVal / (double)_paramIntegralAct.UnitDigit, true, Need0);// разность

                    double coeff;
                    if (ov.CoeffTransformations != null &&
                        ov.CoeffTransformations.Count > 0)
                    {
                        coeff = ov.CoeffTransformations.Last().PeriodValue ?? 1;
                    }
                    else
                    {
                        coeff = 1;
                    }

                    SetCellFloatValue(Row, StartCol + 8, coeff, true, need0: false); // Коэффициент ПУ
                }

                var ovSum = ov.Val_List.Sum(v => v.F_VALUE) * coeffUnit;

                var unitDigitName = UnitDigitToName(_paramIntegralAct.UnitDigit);
                var comment = "Замещал с " + ov.DTServerStart.ServerToClient(_paramIntegralAct.TimeZoneId).ToString("dd.MM.yyyy HH:mm") +
                              "\n по "
                              + ov.DTServerEnd.AddMinutes(30).ServerToClient(_paramIntegralAct.TimeZoneId).ToString("dd.MM.yyyy HH:mm") +
                              "\nзначение "
                              + Math.Round(ovSum, _paramIntegralAct.DoublePrecisionProfile) + " " + unitDigitName;

                if (ov.ActUndercountValues != null && ov.ActUndercountValues.Count > 0)
                {
                    comment += "\nКорректировка по Акту недоучета:";

                    foreach (var actInfo in ov.ActUndercountValues)
                    {
                        if (actInfo.Halfhours == null || actInfo.Halfhours.Count == 0) continue;

                        var actSum = actInfo.Halfhours.Sum(v => v.Value) / (double)_paramIntegralAct.UnitDigit;

                        //ovSum -= actSum;

                        comment += string.Format("\n " + actInfo.ActMode + ": {0:0.###} " + unitDigitName,
                            //+ " с {1:dd.MM.yyyy HH:mm} по {2:dd.MM.yyyy HH:mm}\n"
                            //+ "{3}",
                            actSum);
                    }
                }

                SetCellFloatValue(Row, StartCol + 9, ovSum, true, Need0);
                SetCellValue(Row, StartCol + Col, comment, CenterFormatThin);

                if (after != null)
                {
                    after();
                }

                if (isOvExcludedFromTi)
                {
                    //Если значение ОВ не попало в значение самой ТИ
                    SetCellFloatValue(Row, StartCol + 10, ovSum , false, Need0);
                }
                else
                {
                    SetCellFormat(Row, StartCol + Col, CenterFormatThin);
                }

                //+ " значение " +
                //Math.Round(ov.Val_List.Sum(v=>v.F_VALUE), _doublePrecisionProfile) + " " + _unitDigitName;

                Row++;
            }

            return true;
        }

        private string ChannelNameByNumber(byte channel)
        {
            var ss = string.Empty;

            if (_paramIntegralAct!=null && _paramIntegralAct.ChannelNames!=null 
                && _paramIntegralAct.ChannelNames.TryGetValue(channel, out ss))
            {
                return ss;
            }

            switch (channel)
            {
                case 1:
                    ss = " АП";
                    break;
                case 2:
                    ss = " AO";
                    break;
                case 3:
                    ss = " РП";
                    break;
                case 4:
                    ss = " РO";
                    break;
            }
            return ss;
        }

        private string UnitDigitToName(EnumUnitDigit unitDigit)
        {
            switch (unitDigit)
            {
                case EnumUnitDigit.Null:
                case EnumUnitDigit.None:
                    return "Вт*ч";
                case EnumUnitDigit.Kilo:
                    return "кВт*ч";
                case EnumUnitDigit.Mega:
                    return "МВт*ч";
            }

            return string.Empty;
        }
    }
}
