using System;
using System.Collections.Generic;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Proryv.AskueARM2.Client.Visual.Common.GlobalDictionary;

namespace Proryv.Workflow.Activity.ARM
{
    public class EnumARMTypeConverterBase : EnumConverter
    {
        Type _enumType;
        List<Tuple<string, object>> Wrap = new List<Tuple<string, object>>();

        public EnumARMTypeConverterBase(Type type)
            : base(type)
        {
            _enumType = type;
        }

        public void AddWrap(string EnumItemName, object EnumValue)
        {
            Wrap.Add(new Tuple<string, object>(EnumItemName, EnumValue));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context,
          Type destType)
        {
            return destType == typeof(string);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context,
          Type srcType)
        {
            return srcType == typeof(string);
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }


        public override object ConvertTo(ITypeDescriptorContext context,
          CultureInfo culture, object value, Type destType)
        {
            foreach (var k in Wrap)
            {
                if (k.Item2.Equals(value))
                    return k.Item1;

            }

            return value.ToString();
        }

        public override object ConvertFrom(ITypeDescriptorContext context,
          CultureInfo culture, object value)
        {

            foreach (var k in Wrap)
            {
                if (k.Item1 == (string)value)
                    return k.Item2;
            }

            return Enum.Parse(_enumType, (string)value);
        }


        public string GetDescriprion(object value)
        {
            foreach (var k in Wrap)
            {
                if (k.Item2.Equals(value))
                    return k.Item1;

            }

            return value.ToString();
        }

    }
    //====================
    public class EnumProfileTypeConverter : EnumARMTypeConverterBase
    {
        void LocalizeEnum()
        {
            AddWrap("Основной профиль", enumReturnProfile.ReturnOnlyMainValues);
            AddWrap("Расчетный профиль", enumReturnProfile.ReturnOnlyCalcValues);
        }

        public EnumProfileTypeConverter()
            : base(typeof(enumReturnProfile))
        {
            LocalizeEnum();
        }

        public EnumProfileTypeConverter(Type type)
            : base(type)
        {
            LocalizeEnum();
        }
    }



    public class EnumDiscreteTypeConverter : EnumARMTypeConverterBase
    {
        void LocalizeEnum()
        {
            AddWrap("30 минутки", enumTimeDiscreteType.DBHalfHours);
            AddWrap("Часовки", enumTimeDiscreteType.DBHours);
            AddWrap("Сутки", enumTimeDiscreteType.DB24Hour);
            AddWrap("По месяцу", enumTimeDiscreteType.DBMonth);
            AddWrap("Сумма за весь интервал", enumTimeDiscreteType.DBInterval);
        }

        public EnumDiscreteTypeConverter()
            : base(typeof(enumTimeDiscreteType))
        {
            LocalizeEnum();
        }

        public EnumDiscreteTypeConverter(Type type)
            : base(type)
        {
            LocalizeEnum();
        }
    }

    public class EnumUnitDigitTypeConverter : EnumARMTypeConverterBase
    {
        void LocalizeEnum()
        {
            AddWrap("<Отсутствуют>", EnumUnitDigit.Null);
            AddWrap("Нет", EnumUnitDigit.None);
            AddWrap("Кило", EnumUnitDigit.Kilo);
            AddWrap("Мега", EnumUnitDigit.Mega);
        }

        public EnumUnitDigitTypeConverter()
            : base(typeof(EnumUnitDigit))
        {
            LocalizeEnum();
        }

        public EnumUnitDigitTypeConverter(Type type)
            : base(type)
        {
            LocalizeEnum();
        }
    }


    public class EnumTypeHierarchyTypeConverter : EnumARMTypeConverterBase
    {
        void LocalizeEnum()
        {
            AddWrap("Уровень 1", enumTypeHierarchy.Dict_HierLev1);
            AddWrap("Уровень 2", enumTypeHierarchy.Dict_HierLev2);
            AddWrap("Уровень 3", enumTypeHierarchy.Dict_HierLev3);
            AddWrap("Объект", enumTypeHierarchy.Dict_PS);
            AddWrap("ТИ", enumTypeHierarchy.Info_TI);
            AddWrap("Сечения", enumTypeHierarchy.Section);
            AddWrap("КА", enumTypeHierarchy.Info_ContrTI);
            AddWrap("ПС КА", enumTypeHierarchy.Dict_Contr_PS);
            AddWrap("ТП", enumTypeHierarchy.Info_TP);
            AddWrap("Формула ТИ в ТП", enumTypeHierarchy.Formula_TP_OurSide);
            AddWrap("Формула ТИ КА в ТП", enumTypeHierarchy.Formula_TP_CA);
            AddWrap("Формула", enumTypeHierarchy.Formula);
            AddWrap("Предприятие КА", enumTypeHierarchy.Dict_ContrObject);
            AddWrap("МРСК", enumTypeHierarchy.Dict_HierLev0);
            AddWrap("Ценовая зона", enumTypeHierarchy.PriceZone);
            AddWrap("Субъекты территорий", enumTypeHierarchy.ObjectTerritory);
            AddWrap("Объект потребителя (ЭПУ)", enumTypeHierarchy.Dict_DirectConsumer);
            AddWrap("Интегральное значение", enumTypeHierarchy.Info_Integral);
            AddWrap("РЭС", enumTypeHierarchy.Dict_RES);
            AddWrap("Юр. лицо", enumTypeHierarchy.JuridicalPerson);
            AddWrap("Бытовой потребитель", enumTypeHierarchy.BitAbonent);
        }

        public EnumTypeHierarchyTypeConverter()
            : base(typeof(enumTypeHierarchy))
        {
            LocalizeEnum();
        }

        public EnumTypeHierarchyTypeConverter(Type type)
            : base(type)
        {
            LocalizeEnum();
        }
    }


    public class EnumArchiveObjectTypeTypeConverter : EnumARMTypeConverterBase
    {
        void LocalizeEnum()
        {
            AddWrap("Баланс уровня 3", enumArchiveObjectType.Dict_HierLev3);
            AddWrap("Баланс ПС", enumArchiveObjectType.Dict_PS);
            AddWrap("ТИ", enumArchiveObjectType.Info_TI);
            AddWrap("Сечение", enumArchiveObjectType.Section);
            AddWrap("ТП", enumArchiveObjectType.Info_TP);
            AddWrap("Формула", enumArchiveObjectType.Formula);
            AddWrap("Барабаны", enumArchiveObjectType.Info_Integral);
            
        }

        public EnumArchiveObjectTypeTypeConverter()
            : base(typeof(enumArchiveObjectType))
        {
            LocalizeEnum();
        }

        public EnumArchiveObjectTypeTypeConverter(Type type)
            : base(type)
        {
            LocalizeEnum();
        }
    }       


    public class EnumTypeInformationTypeConverter : EnumARMTypeConverterBase
    {
        void LocalizeEnum()
        {
            AddWrap("Энергия", enumTypeInformation.Energy);
            AddWrap("Мощность", enumTypeInformation.Power);
        }

        public EnumTypeInformationTypeConverter()
            : base(typeof(enumTypeInformation))
        {
            LocalizeEnum();
        }
        
        public EnumTypeInformationTypeConverter(Type type)
            : base(type)
        {
            LocalizeEnum();
        }
    }


    public class EnumEnrgyQualityRequestTypeConverter : EnumARMTypeConverterBase
    {
        void LocalizeEnum()
        {
            AddWrap("Архивные", enumEnrgyQualityRequestType.Archive);
            AddWrap("Текущие", enumEnrgyQualityRequestType.Last);
        }

        public EnumEnrgyQualityRequestTypeConverter()
            : base(typeof(enumEnrgyQualityRequestType))
        {
            LocalizeEnum();
        }

        public EnumEnrgyQualityRequestTypeConverter(Type type)
            : base(type)
        {
            LocalizeEnum();
        }
    }


    public class enumArchTechParamTypeConverter : EnumARMTypeConverterBase
    {
        void LocalizeEnum()
        {
            AddWrap("A+, Вт", enumArchTechParamType.ActiveAggImport);
            AddWrap("A-, Вт", enumArchTechParamType.ActiveAggExport);
            AddWrap("A Q1, Вт", enumArchTechParamType.ActiveAggQ1);
            AddWrap("A Q2, Вт", enumArchTechParamType.ActiveAggQ2);
            AddWrap("A Q3, Вт", enumArchTechParamType.ActiveAggQ3);
            AddWrap("A Q4, Вт", enumArchTechParamType.ActiveAggQ4);
            AddWrap("A+ Фаза 1, Вт", enumArchTechParamType.ActiveL1Import);
            AddWrap("A- Фаза 1, Вт", enumArchTechParamType.ActiveL1Export);
            AddWrap("A Фаза 1 Q1, Вт", enumArchTechParamType.ActiveL1Q1);
            AddWrap("A Фаза 1 Q2, Вт", enumArchTechParamType.ActiveL1Q2);
            AddWrap("A Фаза 1 Q3, Вт", enumArchTechParamType.ActiveL1Q3);
            AddWrap("A Фаза 1 Q4, Вт", enumArchTechParamType.ActiveL1Q4);
            AddWrap("A+ Фаза 2, Вт", enumArchTechParamType.ActiveL2Import);
            AddWrap("A- Фаза 2, Вт", enumArchTechParamType.ActiveL2Export);
            AddWrap("A Фаза 2 Q1, Вт", enumArchTechParamType.ActiveL2Q1);
            AddWrap("A Фаза 2 Q2, Вт", enumArchTechParamType.ActiveL2Q2);
            AddWrap("A Фаза 2 Q3, Вт", enumArchTechParamType.ActiveL2Q3);
            AddWrap("A Фаза 2 Q4, Вт", enumArchTechParamType.ActiveL2Q4);
            AddWrap("A+ Фаза 3, Вт", enumArchTechParamType.ActiveL3Import);
            AddWrap("A- Фаза 3, Вт", enumArchTechParamType.ActiveL3Export);
            AddWrap("A Фаза 3 Q1, Вт", enumArchTechParamType.ActiveL3Q1);
            AddWrap("A Фаза 3 Q2, Вт", enumArchTechParamType.ActiveL3Q2);
            AddWrap("A Фаза 3 Q3, Вт", enumArchTechParamType.ActiveL3Q3);
            AddWrap("A Фаза 3 Q4, Вт", enumArchTechParamType.ActiveL3Q4);
            AddWrap("R+, вар", enumArchTechParamType.ReactiveAggImport);
            AddWrap("R-, вар", enumArchTechParamType.ReactiveAggExport);
            AddWrap("R Q1, вар", enumArchTechParamType.ReactiveAggQ1);
            AddWrap("R Q2, вар", enumArchTechParamType.ReactiveAggQ2);
            AddWrap("R Q3, вар", enumArchTechParamType.ReactiveAggQ3);
            AddWrap("R Q4, вар", enumArchTechParamType.ReactiveAggQ4);
            AddWrap("R+ Фаза 1, вар", enumArchTechParamType.ReactiveL1Import);
            AddWrap("R- Фаза 1, вар", enumArchTechParamType.ReactiveL1Export);
            AddWrap("R Фаза 1 Q1, вар", enumArchTechParamType.ReactiveL1Q1);
            AddWrap("R Фаза 1 Q2, вар", enumArchTechParamType.ReactiveL1Q2);
            AddWrap("R Фаза 1 Q3, вар", enumArchTechParamType.ReactiveL1Q3);
            AddWrap("R Фаза 1 Q4, вар", enumArchTechParamType.ReactiveL1Q4);
            AddWrap("R+ Фаза 2, вар", enumArchTechParamType.ReactiveL2Import);
            AddWrap("R- Фаза 2, вар", enumArchTechParamType.ReactiveL2Export);
            AddWrap("R Фаза 2 Q1, вар", enumArchTechParamType.ReactiveL2Q1);
            AddWrap("R Фаза 2 Q2, вар", enumArchTechParamType.ReactiveL2Q2);
            AddWrap("R Фаза 2 Q3, ВАр", enumArchTechParamType.ReactiveL2Q3);
            AddWrap("R Фаза 2 Q4, ВАр", enumArchTechParamType.ReactiveL2Q4);
            AddWrap("R+ Фаза 3, вар", enumArchTechParamType.ReactiveL3Import);
            AddWrap("R- Фаза 3, вар", enumArchTechParamType.ReactiveL3Export);
            AddWrap("R Фаза 3 Q1, вар", enumArchTechParamType.ReactiveL3Q1);
            AddWrap("R Фаза 3 Q2, вар", enumArchTechParamType.ReactiveL3Q2);
            AddWrap("R Фаза 3 Q3, вар", enumArchTechParamType.ReactiveL3Q3);
            AddWrap("R Фаза 3 Q4, вар", enumArchTechParamType.ReactiveL3Q4);
            AddWrap("S+, ВА", enumArchTechParamType.TotalAggImport);
            AddWrap("S-, ВА", enumArchTechParamType.TotalAggExport);
            AddWrap("S Q1, ВА", enumArchTechParamType.TotalAggQ1);
            AddWrap("S Q2, ВА", enumArchTechParamType.TotalAggQ2);
            AddWrap("S Q3, ВА", enumArchTechParamType.TotalAggQ3);
            AddWrap("S Q4, ВА", enumArchTechParamType.TotalAggQ4);
            AddWrap("S+ Фаза 1, ВА", enumArchTechParamType.TotalL1Import);
            AddWrap("S- Фаза 1, ВА", enumArchTechParamType.TotalL1Export);
            AddWrap("S Фаза 1 Q1, ВА", enumArchTechParamType.TotalL1Q1);
            AddWrap("S Фаза 1 Q2, ВА", enumArchTechParamType.TotalL1Q2);
            AddWrap("S Фаза 1 Q3, ВА", enumArchTechParamType.TotalL1Q3);
            AddWrap("S Фаза 1 Q4, ВА", enumArchTechParamType.TotalL1Q4);
            AddWrap("S+ Фаза 2, ВА", enumArchTechParamType.TotalL2Import);
            AddWrap("S- Фаза 2, ВА", enumArchTechParamType.TotalL2Export);
            AddWrap("S Фаза 2 Q1, ВА", enumArchTechParamType.TotalL2Q1);
            AddWrap("S Фаза 2 Q2, ВА", enumArchTechParamType.TotalL2Q2);
            AddWrap("S Фаза 2 Q3, ВА", enumArchTechParamType.TotalL2Q3);
            AddWrap("S Фаза 2 Q4, ВА", enumArchTechParamType.TotalL2Q4);
            AddWrap("S+ Фаза 3, ВА", enumArchTechParamType.TotalL3Import);
            AddWrap("S- Фаза 3, ВА", enumArchTechParamType.TotalL3Export);
            AddWrap("S Фаза 3 Q1, ВА", enumArchTechParamType.TotalL3Q1);
            AddWrap("S Фаза 3 Q2, ВА", enumArchTechParamType.TotalL3Q2);
            AddWrap("S Фаза 3 Q3, ВА", enumArchTechParamType.TotalL3Q3);
            AddWrap("S Фаза 3 Q4, ВА", enumArchTechParamType.TotalL3Q4);
            AddWrap("F, Hz", enumArchTechParamType.Frequency);
            AddWrap("U нулевого провода, V", enumArchTechParamType.Voltage0);

            AddWrap("U Фаза 1, В", enumArchTechParamType.VoltageL1);
            AddWrap("U Фаза 2, В", enumArchTechParamType.VoltageL2);
            AddWrap("U Фаза 3, В", enumArchTechParamType.VoltageL3);
            
            AddWrap("I нулевого провода, A", enumArchTechParamType.Current0);

            AddWrap("I Фаза 1, A", enumArchTechParamType.CurrentL1);
            AddWrap("I Фаза 2, A", enumArchTechParamType.CurrentL2);
            AddWrap("I Фаза 3, A", enumArchTechParamType.CurrentL3);
            AddWrap("Дифференциальный I, A", enumArchTechParamType.CurrentDif);

            AddWrap("Cos(Ф)", enumArchTechParamType.CosFi);

            AddWrap("Cos(Ф) Фаза 1", enumArchTechParamType.CosFiL1);
            AddWrap("Cos(Ф) Фаза 2", enumArchTechParamType.CosFiL2);
            AddWrap("Cos(Ф) Фаза 3", enumArchTechParamType.CosFiL3);
            AddWrap("U Межфазное 1-2, В", enumArchTechParamType.VoltageL12);
            AddWrap("U Межфазное 1-3, В", enumArchTechParamType.VoltageL13);
            AddWrap("U Межфазное 2-3, В", enumArchTechParamType.VoltageL23);

            
            AddWrap("Угол между векторами напряжений 1-2 фаз", enumArchTechParamType.AnglePhaseVoltageL12);
            AddWrap("Угол между векторами напряжений 1-3 фаз", enumArchTechParamType.AnglePhaseVoltageL13);
            AddWrap("Угол между векторами напряжений 2-3 фаз", enumArchTechParamType.AnglePhaseVoltageL23);

            AddWrap("Угол сдвига между векторами тока и напряжения Фаза 1", enumArchTechParamType.AngleUIL1);
            AddWrap("Угол сдвига между векторами тока и напряжения Фаза 2", enumArchTechParamType.AngleUIL2);
            AddWrap("Угол сдвига между векторами тока и напряжения Фаза 3", enumArchTechParamType.AngleUIL3);


            AddWrap("Мгновенный квадрант Фаза 1", enumArchTechParamType.QuadrantL1);
            AddWrap("Мгновенный квадрант Фаза 2",enumArchTechParamType.QuadrantL2);
            AddWrap("Мгновенный квадрант Фаза 3",enumArchTechParamType.QuadrantL3);
            AddWrap("Мгновенный квадрант",enumArchTechParamType.QuadrantAgg);

            AddWrap("Угол сдвига фазы напряжения Фаза 1",enumArchTechParamType.AnglePhaseUL1);
            AddWrap("Угол сдвига фазы напряжения Фаза 2",enumArchTechParamType.AnglePhaseUL2);
            AddWrap("Угол сдвига фазы напряжения Фаза 3",enumArchTechParamType.AnglePhaseUL3);

            AddWrap("Угол сдвига фазы тока Фаза 1",enumArchTechParamType.AnglePhaseIL1);
            AddWrap("Угол сдвига фазы тока Фаза 2",enumArchTechParamType.AnglePhaseIL2);
            AddWrap("Угол сдвига фазы тока Фаза 3", enumArchTechParamType.AnglePhaseIL3);

            // активная прием+отдача сумма и по фазам
            AddWrap("(A+)+(A-), Вт",enumArchTechParamType.ActiveSummAgg);
            AddWrap("(A+)+(A-) Фаза 1, Вт",enumArchTechParamType.ActiveSummL1);
            AddWrap("(A+)+(A-) Фаза 2, Вт",enumArchTechParamType.ActiveSummL2);
            AddWrap("(A+)+(A-) Фаза 3, Вт",enumArchTechParamType.ActiveSummL3);

            // реактивная прием+отдача сумма и по фазам
            AddWrap("(R+)+(R-), вар",enumArchTechParamType.ReactiveSummAgg);
            AddWrap("(R+)+(R-) Фаза 1, вар",enumArchTechParamType.ReactiveSummL1);
            AddWrap("(R+)+(R-) Фаза 2, вар",enumArchTechParamType.ReactiveSummL2);
            AddWrap("(R+)+(R-) Фаза 3, вар",enumArchTechParamType.ReactiveSummL3);


            // реактивная прием-отдача сумма и по фазам
            AddWrap("(R+)-(R-), вар",enumArchTechParamType.ReactiveDeltaAgg);
            AddWrap("(R+)-(R-) Фаза 1, вар",enumArchTechParamType.ReactiveDeltaL1);
            AddWrap("(R+)-(R-) Фаза 2, вар",enumArchTechParamType.ReactiveDeltaL2);
            AddWrap("(R+)-(R-) Фаза 3, вар",enumArchTechParamType.ReactiveDeltaL3);

            // cos(fi)- отдача, сумма фаз
            AddWrap("Cos(Ф) экспорт",enumArchTechParamType.CosFiExport);
            // cos(fi)-, фаза 1
            AddWrap("Cos(Ф) экспорт Фаза 1",enumArchTechParamType.CosFiL1Export);
            // cos(fi)-, фаза 2
            AddWrap("Cos(Ф) экспорт Фаза 2",enumArchTechParamType.CosFiL2Export);
            // cos(fi)-, фаза 3
            AddWrap("Cos(Ф) экспорт Фаза 3",enumArchTechParamType.CosFiL3Export);

            // активная прием-отдача сумма и по фазам
            AddWrap("(A+)-(A-), Вт",enumArchTechParamType.ActiveDeltaAgg);
            AddWrap("(A+)-(A-) Фаза 1, Вт",enumArchTechParamType.ActiveDeltaL1);
            AddWrap("(A+)-(A-) Фаза 2, Вт",enumArchTechParamType.ActiveDeltaL2);
            AddWrap("(A+)-(A-) Фаза 3, Вт",enumArchTechParamType.ActiveDeltaL3);
            

        }

        public enumArchTechParamTypeConverter()
            : base(typeof(enumArchTechParamType))
        {
            LocalizeEnum();
        }
        
        public enumArchTechParamTypeConverter(Type type)
            : base(type)
        {
            LocalizeEnum();
        }
    }


    public class enumenumChanelTypeConverter : EnumARMTypeConverterBase
    {
        void LocalizeEnum()
        {
            AddWrap("АП", enumChanelType.AI);
            AddWrap("АО", enumChanelType.AO);
            AddWrap("РП", enumChanelType.RI);
            AddWrap("РО", enumChanelType.RO);

            AddWrap("АП Тариф 1", enumChanelType.T1AI);
            AddWrap("АО Тариф 1", enumChanelType.T1AO);
            AddWrap("РП Тариф 1", enumChanelType.T1RI);
            AddWrap("РО Тариф 1", enumChanelType.T1RO);

            AddWrap("АП Тариф 2", enumChanelType.T2AI);
            AddWrap("АО Тариф 2", enumChanelType.T2AO);
            AddWrap("РП Тариф 2", enumChanelType.T2RI);
            AddWrap("РО Тариф 2", enumChanelType.T2RO);

            AddWrap("АП Тариф 3", enumChanelType.T3AI);
            AddWrap("АО Тариф 3", enumChanelType.T3AO);
            AddWrap("РП Тариф 3", enumChanelType.T3RI);
            AddWrap("РО Тариф 3", enumChanelType.T3RO);
        }

        public enumenumChanelTypeConverter()
            : base(typeof(enumChanelType))
        {
            LocalizeEnum();
        }

        public enumenumChanelTypeConverter(Type type)
            : base(type)
        {
            LocalizeEnum();
        }
    }


    public class enumFormulsObjectTypeTypeConverter : EnumARMTypeConverterBase
    {
        void LocalizeEnum()
        {
            AddWrap("Уровень 1", enumFormulsObjectType.Dict_HierLev1);
            AddWrap("Уровень 2", enumFormulsObjectType.Dict_HierLev2);
            AddWrap("Уровень 3", enumFormulsObjectType.Dict_HierLev3);
            AddWrap("ПС", enumFormulsObjectType.Dict_PS);
            AddWrap("ТИ", enumFormulsObjectType.Info_TI);
        }

        public enumFormulsObjectTypeTypeConverter()
            : base(typeof(enumFormulsObjectType))
        {
            LocalizeEnum();
        }

        public enumFormulsObjectTypeTypeConverter(Type type)
            : base(type)
        {
            LocalizeEnum();
        }
    }

    public class EnumExportExcelAdapterTypeTypeConverter : EnumARMTypeConverterBase
    {
        void LocalizeEnum()
        {
            AddWrap("HTML", TExportExcelAdapterType.toHTML);
            AddWrap("PDF", TExportExcelAdapterType.toPDF);
            AddWrap("XLS", TExportExcelAdapterType.toXLS);
            AddWrap("XLSx", TExportExcelAdapterType.toXLSx);
        }

        public EnumExportExcelAdapterTypeTypeConverter()
            : base(typeof(TExportExcelAdapterType))
        {
            LocalizeEnum();
        }

        public EnumExportExcelAdapterTypeTypeConverter(Type type)
            : base(type)
        {
            LocalizeEnum();
        }
    }


    public class EnumenumTypeOfMeasuringTypeConverter : EnumARMTypeConverterBase
    {
        void LocalizeEnum()
        {
            AddWrap("Активная мощность, кВт", enumTypeOfMeasuring.Active);
            AddWrap("Реактивная мощность, квар", enumTypeOfMeasuring.Reactive);
        }

        public EnumenumTypeOfMeasuringTypeConverter()
            : base(typeof(enumTypeOfMeasuring))
        {
            LocalizeEnum();
        }

        public EnumenumTypeOfMeasuringTypeConverter(Type type)
            : base(type)
        {
            LocalizeEnum();
        }
    }

    
    public class enumGroupTPPowerReportModeTypeConverter : EnumARMTypeConverterBase
    {
        void LocalizeEnum()
        {
            AddWrap("Расчет по открытым данным", EnumGroupTPPowerReportMode.OpenPeriod);
            AddWrap("Расчет по закрытым данным", EnumGroupTPPowerReportMode.ClosedCurrPeriod);
            AddWrap("Расчет по закрытым данным, с дорасчетами в последующие периоды", EnumGroupTPPowerReportMode.ClosedNextPeriod);
            AddWrap("Расчет по закрытым данным, с дорасчетами за предыдущие периоды", EnumGroupTPPowerReportMode.ClosedPrevPeriod);
        }

        public enumGroupTPPowerReportModeTypeConverter()
            : base(typeof(EnumGroupTPPowerReportMode))
        {
            LocalizeEnum();
        }

        public enumGroupTPPowerReportModeTypeConverter(Type type)
            : base(type)
        {
            LocalizeEnum();
        }
    }
    
    

    public class enumReportTypeTypeConverter : EnumARMTypeConverterBase
    {
        void LocalizeEnum()
        {
            AddWrap("Контроль скачков и провалов напряжения", enumReportType.ReportVoltageControl);
            AddWrap("Отчет о замене средств учета", enumReportType.ReportReplacementOfMeters);
            AddWrap("Контроль частоты сети и перегрузок по току", enumReportType.ReportCurrentControl);
            AddWrap("Перечень отключений/ограничений", enumReportType.ReportOffOnListMeters);
            AddWrap("Перечень приборов учета и концентраторов, не выходивших на связь", enumReportType.ReportMetersLink);
            AddWrap("Суточный отчет (профиль) по ПУ", enumReportType.DailyPrifile);
            AddWrap("Суточный отчет (профиль мощности) по ПУ", enumReportType.DailyPrifilePower);
            AddWrap("Суточный отчет (профиль электроэнергии) по ПУ", enumReportType.DailyPrifileEnergy);
            AddWrap("Профиль значений энергии за интервал", enumReportType.PeriodPrifileEnergySingleTI);
            AddWrap("Мгновенные значения тока", enumReportType.TechQualityCurrentPeriodSingleTI);
            AddWrap("Мгновенные значения напряжения", enumReportType.TechQualityVoltagePeriodSingleTI);
            AddWrap("Коэффициент мощности (косинус фи)", enumReportType.TechQualityCosFiPeriodSingleTI);
            AddWrap("Действующее значение полной мощности", enumReportType.TechQualityTotalL2PeriodSingleTI);
            AddWrap("Частота", enumReportType.TechQualityFrequencyPeriodSingleTI);
            AddWrap("Отчет по профилям ПУ", enumReportType.DailyProfileSingleTI);
            AddWrap("Ведомость опроса ПУ", enumReportType.StatementRequestSinglePS);
            AddWrap("Фактическая мощность по договорам", enumReportType.FactPowerByContracts);
            AddWrap("Ведомость контроля мощности", enumReportType.StatementPowerControl);
            AddWrap("Ведомость контроля мощности (С учетом перерасчета)", enumReportType.StatementPowerControlWithRecalculate);
        }

        public enumReportTypeTypeConverter()
            : base(typeof(enumBusRelation))
        {
            LocalizeEnum();
        }

        public enumReportTypeTypeConverter(Type type)
            : base(type)
        {
            LocalizeEnum();
        }
    }

    
    public class enumTypeHierarchyTypeConverter : EnumARMTypeConverterBase
    {
        void LocalizeEnum()
        {
            AddWrap("Уровень 1", enumTypeHierarchy.Dict_HierLev1);
            AddWrap("Уровень 2", enumTypeHierarchy.Dict_HierLev2);
            AddWrap("Уровень 3", enumTypeHierarchy.Dict_HierLev3);
            AddWrap("ПС", enumTypeHierarchy.Dict_PS);
            AddWrap("ТИ", enumTypeHierarchy.Info_TI);
            AddWrap("Сечение", enumTypeHierarchy.Section);
            AddWrap("КА", enumTypeHierarchy.Info_ContrTI);
            AddWrap("ПС КА", enumTypeHierarchy.Dict_Contr_PS);
            AddWrap("Точка поставки", enumTypeHierarchy.Info_TP);
            AddWrap("Формула ТИ ФСК в ТП", enumTypeHierarchy.Formula_TP_OurSide);
            AddWrap("Формула ТИ КА в ТП", enumTypeHierarchy.Formula_TP_CA);
            AddWrap("Формула", enumTypeHierarchy.Formula);
            AddWrap("Предприятие КА", enumTypeHierarchy.Dict_ContrObject);
            AddWrap("ЕНЭС", enumTypeHierarchy.Dict_HierLev0);
            AddWrap("Ценовая зона", enumTypeHierarchy.PriceZone);
            AddWrap("Субъекты территорий", enumTypeHierarchy.ObjectTerritory);
            AddWrap("Прямой потребитель", enumTypeHierarchy.Dict_DirectConsumer);
            AddWrap("Интегральное значение", enumTypeHierarchy.Info_Integral);
            AddWrap("РЭС", enumTypeHierarchy.Dict_RES);
            AddWrap("Юридическое лицо", enumTypeHierarchy.JuridicalPerson);
            AddWrap("Договор юр. лица", enumTypeHierarchy.Dict_JuridicalPersons_Contract);
            AddWrap("Бытовой абонент", enumTypeHierarchy.BitAbonent);
            AddWrap("Мониторинг автосбора ", enumTypeHierarchy.MonitoringAutoSbor);
            AddWrap("Концентратор", enumTypeHierarchy.Concentrator);
            AddWrap("Мониторинг 61968", enumTypeHierarchy.Monitoring61968);
            AddWrap("Площадка сбора по 61968", enumTypeHierarchy.Slave61968System);
            AddWrap("Мгновенное значение", enumTypeHierarchy.ArchTechQuality);
            AddWrap("Нагрузка трансформатора", enumTypeHierarchy.LoadTransformer);
            AddWrap("Узел в дереве свободной иерархии (без привязки к объекту)", enumTypeHierarchy.Node);
            AddWrap("Трансформатор расчетный", enumTypeHierarchy.TransformatorT);
//            AddWrap("", enumTypeHierarchy.BusSystem);
//            AddWrap("", enumTypeHierarchy.DistributingArrangement);
            AddWrap("УСПД", enumTypeHierarchy.USPD);
            AddWrap("Узел OPC UA", enumTypeHierarchy.UANode);
            AddWrap("Сервер OPC UA", enumTypeHierarchy.UAServer);
        }

        public enumTypeHierarchyTypeConverter()
            : base(typeof(enumTypeHierarchy))
        {
            LocalizeEnum();
        }

        public enumTypeHierarchyTypeConverter(Type type)
            : base(type)
        {
            LocalizeEnum();
        }
    }

    public class GroupTPPowerReportModeTypeConverter : EnumARMTypeConverterBase
    {
        private void LocalizeEnum()
        {
            AddWrap("Расчет по открытым данным", GroupTPPowerReportMode.OpenPeriod);
            AddWrap("Расчет по закрытым данным", GroupTPPowerReportMode.ClosedCurrPeriod);
            AddWrap("Расчет по закрытым данным, с дорасчетами в последующие периоды",
                GroupTPPowerReportMode.ClosedNextPeriod);
            AddWrap("Расчет по закрытым данным, с дорасчетами за предыдущие периоды",
                GroupTPPowerReportMode.ClosedPrevPeriod);
            AddWrap("Не определено", GroupTPPowerReportMode.NullValue);
        }

        public GroupTPPowerReportModeTypeConverter()
            : base(typeof (GroupTPPowerReportMode))
        {
            LocalizeEnum();
        }

        public GroupTPPowerReportModeTypeConverter(Type type)
            : base(type)
        {
            LocalizeEnum();
        }
    }

    public class EnumGroupTPPowerReportModeTypeConverter : EnumARMTypeConverterBase
    {
        void LocalizeEnum()
        {
            AddWrap("Расчет по открытым данным", EnumGroupTPPowerReportMode.OpenPeriod);
            AddWrap("Расчет по закрытым данным", EnumGroupTPPowerReportMode.ClosedCurrPeriod);
            AddWrap("Расчет по закрытым данным, с дорасчетами в последующие периоды", EnumGroupTPPowerReportMode.ClosedNextPeriod);
            AddWrap("Расчет по закрытым данным, с дорасчетами за предыдущие периоды", EnumGroupTPPowerReportMode.ClosedPrevPeriod);
        }

        public EnumGroupTPPowerReportModeTypeConverter()
            : base(typeof(EnumGroupTPPowerReportMode))
        {
            LocalizeEnum();
        }

        public EnumGroupTPPowerReportModeTypeConverter(Type type)
            : base(type)
        {
            LocalizeEnum();
        }
    }

    public class enumBusRelationTypeConverter : EnumARMTypeConverterBase
    {
        void LocalizeEnum()
        {
            AddWrap("Отношения нет", enumBusRelation.None);
            AddWrap("Расчетная сторона", enumBusRelation.PPI_Station);
            AddWrap("Контрольная сторона", enumBusRelation.PPI_OurSide);
            AddWrap("Относительно станций", enumBusRelation.PSI_Station);
            AddWrap("Относительно шин", enumBusRelation.PSI_OurSide);
            AddWrap("Сторона МРСК", enumBusRelation.OurSideOnly);
            AddWrap("Сторона КА", enumBusRelation.ContrSideOnly);
            AddWrap("Все по ПСИ", enumBusRelation.PSI_All);
        }

        public enumBusRelationTypeConverter()
            : base(typeof(enumBusRelation))
        {
            LocalizeEnum();
        }

        public enumBusRelationTypeConverter(Type type)
            : base(type)
        {
            LocalizeEnum();
        }
    }
    
    public class enumAlarmTypeTypeConverter : EnumARMTypeConverterBase
    {
        void LocalizeEnum()
        {
            AddWrap("Отсутствуют", enumAlarmType.None);
            AddWrap("Балансы ПС", enumAlarmType.BalancePS);
            AddWrap("Формулы", enumAlarmType.Formula);
            AddWrap("Подчиненные системы", enumAlarmType.Master61968SlaveSystems);
            AddWrap("ТИ", enumAlarmType.TI);
            AddWrap("Пользователи", enumAlarmType.Users);
            AddWrap("ПС", enumAlarmType.Users);
            AddWrap("Универсальные балансы", enumAlarmType.BalanceFreeHierarchy);
        }

        public enumAlarmTypeTypeConverter()
            : base(typeof(enumAlarmType))
        {
            LocalizeEnum();
        }

        public enumAlarmTypeTypeConverter(Type type)
            : base(type)
        {
            LocalizeEnum();
        }
    }

    public class enumObjectTypeForNameTypeConverter : EnumARMTypeConverterBase
    {
        void LocalizeEnum()
        {
            AddWrap("Баланс уровня 0", enumObjectTypeForName.Balance_HierLev0);
            AddWrap("Баланс уровня 3", enumObjectTypeForName.Balance_HierLev3);
            AddWrap("Баланс ПС", enumObjectTypeForName.Balance_PS);
            AddWrap("Формула", enumObjectTypeForName.Formula);
            AddWrap("Уровень 1", enumObjectTypeForName.HierLev1);
            AddWrap("Уровень 2", enumObjectTypeForName.HierLev2);
            AddWrap("Уровень 3", enumObjectTypeForName.HierLev3);
            AddWrap("ПС", enumObjectTypeForName.PS);
            AddWrap("ТИ", enumObjectTypeForName.TI);
            AddWrap("Пользователь", enumObjectTypeForName.User);
            AddWrap("Сечение", enumObjectTypeForName.Section);
        }

        public enumObjectTypeForNameTypeConverter()
            : base(typeof(enumObjectTypeForName))
        {
            LocalizeEnum();
        }

        public enumObjectTypeForNameTypeConverter(Type type)
            : base(type)
        {
            LocalizeEnum();
        }
    }

    public class enumManualReadRequestTypeTypeConverter : EnumARMTypeConverterBase
    {
        void LocalizeEnum()
        {
            AddWrap("Интегралы", enumManualReadRequestType.Integrals);
            AddWrap("Мгновенные значения", enumManualReadRequestType.QualityValue);
            AddWrap("Тарифное расписание", enumManualReadRequestType.TarifSchedule);
            AddWrap("Профиль", enumManualReadRequestType.Profile);
            AddWrap("Журнал событий", enumManualReadRequestType.JournalEvents);
        }

        public enumManualReadRequestTypeTypeConverter()
            : base(typeof(enumManualReadRequestType))
        {
            LocalizeEnum();
        }

        public enumManualReadRequestTypeTypeConverter(Type type)
            : base(type)
        {
            LocalizeEnum();
        }
    }

    public class enumManualReadRequestPriorityTypeConverter : EnumARMTypeConverterBase
    {
        void LocalizeEnum()
        {
            AddWrap("Обычный", enumManualReadRequestPriority.Normal);
            AddWrap("Высокий", enumManualReadRequestPriority.Hight);
        }

        public enumManualReadRequestPriorityTypeConverter()
            : base(typeof(enumManualReadRequestPriority))
        {
            LocalizeEnum();
        }

        public enumManualReadRequestPriorityTypeConverter(Type type)
            : base(type)
        {
            LocalizeEnum();
        }
    }

    public class enumManualUspdRequestTypeTypeConverter : EnumARMTypeConverterBase
    {
        void LocalizeEnum()
        {
            AddWrap("Регламентный опрос", enumManualUspdRequestType.Reglaments);
            AddWrap("Опрос перечня доступных ПУ", enumManualUspdRequestType.AccessMeters);
        }

        public enumManualUspdRequestTypeTypeConverter()
            : base(typeof(enumManualUspdRequestType))
        {
            LocalizeEnum();
        }

        public enumManualUspdRequestTypeTypeConverter(Type type)
            : base(type)
        {
            LocalizeEnum();
        }
    }

    
}
