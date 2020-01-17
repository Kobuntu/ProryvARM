using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Proryv.AskueARM2.Both.VisualCompHelpers.TClasses;
using Proryv.AskueARM2.Both.VisualCompHelpers.XamDataPresenter.DataTableExtention;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Server.DBAccess.Public.Common;

namespace Proryv.AskueARM2.Both.VisualCompHelpers.XamDataPresenter
{
	public static partial class ArchivesToXamDataGrid
	{
		public static DataTableEx ExecuteFormulasValueExpand(FormulasResult formulasResult, ArchivesValues_List2 archivesList,
			IVisualDataRequestObjectsNames getNameInterface, int doublePrecision, DateTime dtStart, DateTime dtEnd, enumTimeDiscreteType discreteType, string timeZoneId,
			ArchivesTPValueAllChannels archivesTpList = null, SectionIntegralActsTotalResults? archivesSection = null,
			bool isSumTiForChart = false, FormulaConstantArchives constantArchives = null, ArchTariffIntegralsTIs cumulativeIntegrals = null,
		    ForecastConsumptionResult fResult = null, List<TRowObjectForGrid> listTiViewModel = null, bool isDetailOv = true,
            BalanceFreeHierarchyResults balanceFreeHierarchy = null)
		{
			if (archivesList == null && formulasResult == null && archivesTpList == null && archivesSection == null
				&& constantArchives == null && cumulativeIntegrals == null && fResult == null && balanceFreeHierarchy == null) return null;

#if DEBUG
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif

            var dts = getNameInterface.GetDateTimeListForPeriod(dtStart, dtEnd, discreteType, timeZoneId.GeTimeZoneInfoById());
			var numbersValues = dts.Count;
			//Расчетный ли это профиль
			var deltaDiscrete = (int)discreteType + 1;
            
            //-------------Таблица для представления данных----------------------------
            var userTable = new DataTableEx();
		    userTable.BeginLoadData();
		    try
		    {
		        userTable.Columns.Add("EventDateTime", typeof(DateTime));

		        #region Создаем колонки в пользовательском представлениее

		        var vType = typeof(IFValue);
		        var k = 0;

		        #region ------------Перебираем точки---------------------------------------------

                var tiColumnValues = userTable.AddColumnsTi(ref k, archivesList, isSumTiForChart, isDetailOv);

                #endregion

                #region ------------ТП---------------------------------------------

                var tpColumnValues = userTable.AddColumnsTp(ref k, archivesTpList, listTiViewModel);

          //      var tpArchives = new List<Tuple<TPoint, Dictionary<byte, List<TPointValue>>>>();

		        //if (archivesTpList != null)
		        //{
		            
		        //}

		        #endregion

		        #region ------------Перебираем формулы---------------------------------------------

		        var formulaArchives = new List<Tuple<IFormulaInfo, TFormulasResult, List<TFormulaConsist>>>();
		        if (formulasResult != null && formulasResult.Result_Values != null)
		        {
		            foreach (var formulas in formulasResult.Result_Values
		                .Select(f => new Tuple<IFormulaInfo, TFormulasResult, List<TFormulaConsist>>(f.GetFormulaInfo(getNameInterface),
		                        f.Result_Values.FirstOrDefault(), f.FormulaConsist))
		                .Where(f => f.Item1 != null)
		                .OrderBy(f => f.Item1.FormulaName))
		            {
                        var fVal = formulas.Item2;
		                if (fVal != null && fVal.Val_List != null && fVal.Val_List.Count > 0)
		                {
                            var col = new DataColumn("F_VALUE" + k, vType)
		                    {
		                        ReadOnly = true,
                            };

		                    userTable.Columns.Add(col);
		                    k++;
		                }

		                //------------ТИ которые входят в формулу-------------------------------------------------------
		                if (formulasResult.IsValuesAllTIEnabled && formulas.Item3 != null)
		                {
		                    foreach (var archivesValue in formulas.Item3)
		                    {
		                        if (archivesValue.Val_List == null) continue;

		                        //var hierObj =
		                        //    getNameInterface.GetHierarchyDbTreeObject(archivesValue.TI_Ch_ID.TI_ID.ToString(),
		                        //        archivesValue.TypeTIinFormula);

		                        var col = new DataColumn("F_VALUE" + k, vType)
		                        {
		                            ReadOnly = true,
                                };

		                        userTable.Columns.Add(col);
		                        k++;
		                    }
		                }

		                formulaArchives.Add(formulas);
		            }
		        }

		        #endregion

		        #region ------------Перебираем константы---------------------------------------------

		        if (constantArchives != null && constantArchives.FormulaConstantDict != null)
		        {
		            foreach (var formulaConstant in constantArchives.FormulaConstantDict.Values)
		            {
		                if (formulaConstant.ArchiveValues != null && formulaConstant.ArchiveValues.Count > 0)
		                {
		                    var col = new DataColumn("F_VALUE" + k, vType)
		                    {
		                        //Caption = "Конст. -" + formulaConstant.ConstantaParams.FormulaConstantName,
		                        ReadOnly = true,
                            };

		                    userTable.Columns.Add(col);
		                    //namesFormulas.Add(new GridTitle
		                    //{
		                    //    //TitleString = "Конст. -" + formulaConstant.ConstantaParams.FormulaConstantName,
		                    //    TitleString = "Конст. -" + formulaConstant.ConstantaParams.FormulaConstantName
		                    //});

		                    k++;
		                }
		            }
		        }

		        #endregion

		        #region ------------Перебираем сечения---------------------------------------------

		        if (archivesSection != null && archivesSection.Value.Total_Result != null &&
		            archivesSection.Value.Total_Result.Count > 0)
		        {
		            foreach (var sectionPair in archivesSection.Value.Total_Result)
		            {
		                var section = sectionPair.Value;
		                if (section.TotalVals != null && section.TotalVals.Count > 0)
		                {
		                    var sectionName = getNameInterface.GetSectionName(sectionPair.Key);

		                    var col = new DataColumn("F_VALUE" + k, vType)
		                    {
		                        //Caption = "Сеч. -" + sectionName,
		                        ReadOnly = true,
                            };

		                    userTable.Columns.Add(col);
		                    //namesFormulas.Add(new GridTitle
		                    //{
		                    //    //TitleString = "Сеч. -" + sectionName,
		                    //    TitleString = "Сеч. -" + sectionName
		                    //});

		                    k++;
		                }
		            }
		        }

		        #endregion

		        #region------------Перебираем накопительные интегралы------------------------------

		        if (cumulativeIntegrals != null && cumulativeIntegrals.IntegralsValue30orHour != null)
		        {
                    var nType = typeof(IConvertible);

		            foreach (var integral in cumulativeIntegrals.IntegralsValue30orHour)
		            {
		                if (integral.Cumulate_Val_List == null || integral.Cumulate_Val_List.Count == 0) continue;

                        //var tiName = getNameInterface.GetTIName(integral.TI_Ch_ID.TI_ID, integral.TI_Ch_ID.IsCA);
		                //var psName = getNameInterface.GetPSName(integral.PS_ID, integral.TI_Ch_ID.IsCA);

		                var col = new DataColumn("F_VALUE" + k, nType)
		                {
		                    //Caption = tiName +
		                    //          getNameInterface.GetChanelTypeNameByID(integral.TI_Ch_ID.TI_ID,
		                    //              integral.TI_Ch_ID.ChannelType, false,
		                    //              cumulativeIntegrals.DTStart, cumulativeIntegrals.DTEnd) + " \n" +
		                    //          psName,
		                    ReadOnly = true,
                        };

		                userTable.Columns.Add(col);
		                k++;
		            }
		        }

                #endregion

		        #region------------Прогнозирование------------------------------

		        if (fResult != null && fResult.Result_Values != null)
		        {
		            var hierarchyObject =
		                getNameInterface.GetHierarchyDbTreeObject(fResult.ID.ID, fResult.ID.TypeHierarchy);

		            if (hierarchyObject != null)
		            {
		                foreach (var archive in fResult.Result_Values)
		                {
                            if (archive.Value == null) continue;

		                    var col = new DataColumn("F_VALUE" + k, vType)
		                    {
		                        Caption = hierarchyObject.Name + " " + getNameInterface.GetChanelTypeNameByID(archive.Key.Channel, false),
		                        ReadOnly = true,
		                    };

		                    userTable.Columns.Add(col);
		                    k++;
		                }
		            }
		        }

                #endregion

                #region ------------Перебираем балансы---------------------------------------------

                var balanceColumnValues = userTable.AddColumnsBalances(ref k, balanceFreeHierarchy);

                #endregion

                #endregion

#if DEBUG
                sw.Stop();
                Console.WriteLine("Создаем колонки в пользовательском представлениее - > {0} млс", sw.ElapsedMilliseconds);
                sw.Restart();
#endif

                #region Наполняем поставщик

                if (archivesList != null)
		        {
		            userTable.Description = new DataTableDescription
		            {
		                DtStart = archivesList.DTStart,
		                DiscreteType = archivesList.DiscreteType,
		                TimeZoneId = archivesList.TimeZoneId,
		            };
		        }
		        else
		        {
		            userTable.Description = null;
		        }

		        for (var i = 0; i < numbersValues; i++)
		        {
		            k = 0;

		            //--------колонки в промежуточную таблицу-----------
		            var row = userTable.NewRow() as DataRowEx;
		            var dt = dts[i];

		            row["EventDateTime"] = dt;
		            //row["Time"] = dt.TimeOfDay;

		            //-------пишем в промежуточную таблицу значения--------

		            //TVALUES_DB mainChannelVal = null;

                    #region ТИ

                    userTable.PopulateRowsTi(ref k, tiColumnValues, numbersValues, isSumTiForChart, row, i);

                    #endregion

                    #region ТП

                    userTable.PopulateRowsTp(ref k, tpColumnValues, row, i);

                    #endregion

                    #region Формулы

                    if (formulasResult != null && formulasResult.Result_Values != null)
		            {
		                foreach (var formulas in formulaArchives)
		                {
		                    enumTypeHierarchy typeHierarchy;
		                    switch (formulas.Item1.FormulasTable)
		                    {
		                        case enumFormulasTable.Info_Formula_Description:
		                            typeHierarchy = enumTypeHierarchy.Formula;
		                            break;
		                        case enumFormulasTable.Info_TP2_OurSide_Formula_Description:
		                            typeHierarchy = enumTypeHierarchy.Formula_TP_OurSide;
		                            break;
		                        case enumFormulasTable.Info_TP2_Contr_Formula_Description:
		                            typeHierarchy = enumTypeHierarchy.Formula_TP_CA;
		                            break;
		                        default:
		                            typeHierarchy = enumTypeHierarchy.Unknown;
		                            break;
		                    }

		                    var id = new ID_Hierarchy
		                    {
		                        ID = formulas.Item1.Formula_UN,
		                        TypeHierarchy = typeHierarchy
		                    };

		                    var fVal = formulas.Item2;
		                    if (fVal != null && fVal.Val_List != null && fVal.Val_List.Count > 0)
		                    {
		                        var v = fVal.Val_List.ElementAtOrDefault(i);
		                        if (v != null)
		                        {
		                            var useMeasureModule =
                                        string.IsNullOrEmpty(fVal.MeasureUnit_UN)
                                        &&
                                        (string.Equals(fVal.MeasureQuantityType_UN,"EnergyUnit") ||
		                                string.Equals(fVal.MeasureQuantityType_UN, "PowerUnit"));

                                    //row["F_VALUE" + k] = v;
                                    row.SetValue("F_VALUE" + k, i, 0, id, null, fVal.MeasureUnit_UN, v, useMeasureModule);
		                        }

		                        k++;
		                    }

		                    if (formulasResult.IsValuesAllTIEnabled && formulas.Item3 != null)
		                    {
		                        foreach (var archivesValue in formulas.Item3)
		                        {
		                            if (archivesValue.Val_List == null) continue;

		                            var v = archivesValue.Val_List.ElementAtOrDefault(i);
		                            if (v != null)
		                            {
                                        if (archivesValue.Id == null)
                                        {
                                            row["F_VALUE" + k] = new TVALUES_DB
                                            {
                                                F_VALUE = v.F_VALUE,
                                                F_FLAG = v.F_FLAG,
                                            };
                                        }
                                        else
                                        {
                                            row.SetValue("F_VALUE" + k, i, archivesValue.ChannelType, archivesValue.Id, null, null, v);
                                        }
                                    }

		                            k++;
		                        }
		                    }
		                }
		            }

                    #endregion

                    #region Константы

                    if (constantArchives != null && constantArchives.FormulaConstantDict != null)
                    {
                        foreach (var formulaConstant in constantArchives.FormulaConstantDict.Values)
                        {
                            if (formulaConstant.ArchiveValues != null)
                            {
                                var v = formulaConstant.ArchiveValues.ElementAtOrDefault(i);
                                if (v != null)
                                {
                                    row.SetValue("F_VALUE" + k, i, 0, new ID_Hierarchy
                                    {
                                        ID = formulaConstant.ConstantaParams.FormulaConstant_UN,
                                        TypeHierarchy = enumTypeHierarchy.FormulaConstant
                                    }, null, null, v);
                                }

                                k++;
                            }
                        }
                    }

                    #endregion

                    #region Сечения

                    if (archivesSection != null && archivesSection.Value.Total_Result != null &&
		                archivesSection.Value.Total_Result.Count > 0)
		            {
		                foreach (var sectionPair in archivesSection.Value.Total_Result)
		                {
		                    var section = sectionPair.Value;
		                    List<TVALUES_DB> values;
		                    if (section.TotalVals != null && section.TotalVals.Count > 0 &&
		                        section.TotalVals.TryGetValue(enumInputType.Saldo, out values))
		                    {
		                        var v = values.ElementAtOrDefault(i);
		                        if (v != null)
		                        {
                                    //row["F_VALUE" + k] = v;
                                    row.SetValue("F_VALUE" + k, i, 0, new ID_Hierarchy
                                    {
                                        ID = sectionPair.Key.ToString(),
                                        TypeHierarchy = enumTypeHierarchy.Section
                                    }, null, null, v);
                                }

		                        k++;
		                    }
		                }
		            }

		            #endregion

		            #region------------Перебираем накопительные интегралы------------------------------

		            if (cumulativeIntegrals != null && cumulativeIntegrals.IntegralsValue30orHour != null)
		            {
		                var unitDigitCoeff = (double) cumulativeIntegrals.UnitDigit;

		                foreach (var integral in cumulativeIntegrals.IntegralsValue30orHour)
		                {
		                    if (integral.Cumulate_Val_List == null || integral.Cumulate_Val_List.Count == 0) continue;

		                    var val = integral.Cumulate_Val_List.ElementAtOrDefault(i);
		                    if (val != null)
		                    {
                                var v = new TVal
                                {
                                    F_FLAG = val.F_FLAG,
                                    F_VALUE = val.Value / unitDigitCoeff,
                                };

                                //var v = val.Value / unitDigitCoeff;

                                //------------ТИ ФСК-----------------------------

                                row.SetValue("F_VALUE" + k, i, integral.TI_Ch_ID.ChannelType, new ID_Hierarchy
                                {
                                    ID = integral.TI_Ch_ID.TI_ID.ToString(),
                                    TypeHierarchy = enumTypeHierarchy.Info_TI
                                }, integral.TI_Ch_ID.DataSourceType, null, v);
                                
                                //row["F_VALUE" + k] = v;
		                        //if (integral.TI_Ch_ID.ChannelType < 3 && v > 0)
		                        //{
		                        //    mainChannelVal = v;
		                        //}
		                    }

		                    k++;
		                }
		            }

                    #endregion

                    #region------------Прогнозирование------------------------------

                    if (fResult != null && fResult.Result_Values != null)
		            {
		                var hierarchyObject =
		                    getNameInterface.GetHierarchyDbTreeObject(fResult.ID.ID, fResult.ID.TypeHierarchy);

		                if (hierarchyObject != null)
		                {
		                    var unitDigitCoeff = (double)fResult.UnitDigit;

                            foreach (var archive in fResult.Result_Values)
                            {
                                if (archive.Value == null) continue;

                                var val = archive.Value.ElementAtOrDefault(i);
		                        if (val != null)
		                        {
		                            var v = new TVALUES_DB
		                            {
		                                F_FLAG = val.F_FLAG,
		                                F_VALUE = val.F_VALUE / unitDigitCoeff,
		                            };

                                    //------------ТИ ФСК-----------------------------
                                    //row["F_VALUE" + k] = v;

                                    row.SetValue("F_VALUE" + k, i, archive.Key.Channel, new ID_Hierarchy
                                    {
                                        ID = archive.Key.ID,
                                        TypeHierarchy = archive.Key.TypeHierarchy,
                                    }, null, null, v);
                                }

		                        k++;
		                    }
		                }
		            }

                    #endregion

                    #region Балансы

                    userTable.PopulateRowsBalances(ref k, balanceColumnValues, row, i);

                    #endregion

                    //      if (consumptionSchedule != null && mainChannelVal != null && mainChannelVal.F_VALUE != null &&
                    //    mainChannelVal.F_FLAG.HasFlag(VALUES_FLAG_DB.ConsumptionScheduleOverflow))
                    //{
                    //    //Ищем значение типового графика для данной получасовки
                    //    var cs_val =
                    //        consumptionSchedule.ConsumptionScheduleValues.FirstOrDefault(
                    //            cs => cs.TotalDay == (dt.Date - dtStart.Date).TotalDays && cs.TotalNumberPerDay ==
                    //                  dt.TimeOfDay.TotalMinutes / (30 * deltaDiscrete));

                    //    if (cs_val != null && cs_val.F_VALUE.HasValue)
                    //    {
                    //        double delta = 0;
                    //        if (mainChannelVal.F_VALUE > cs_val.MAX_VALUE)
                    //        {
                    //            delta = mainChannelVal.F_VALUE - cs_val.F_VALUE.Value;
                    //        }
                    //        else
                    //        {
                    //            delta = mainChannelVal.F_VALUE - cs_val.F_VALUE.Value;
                    //        }

                    //        row["ConsumptionSchedule"] = delta;
                    //        row["ConsumptionSchedulePercent"] = delta / mainChannelVal.F_VALUE * 100;
                    //    }
                    //}

                    userTable.Rows.Add(row);
		        }

		        #endregion
		    }
            finally
		    {
		        userTable.EndLoadData();
		        userTable.AcceptChanges();
            }

#if DEBUG
            sw.Stop();
            Console.WriteLine("ExecuteFormulasValueExpand - > {0} млс", sw.ElapsedMilliseconds);
#endif

            return userTable;
		}

	    public static DataTableEx ExecuteValidateValue(FormulasResult formulasResult, ArchivesValidate_List archivesList,
	        ArchivesTPValueAllChannels archivesTpList, IVisualDataRequestObjectsNames getNameInterface, List<DateTime> dts)
	    {
	        if ((archivesList == null || archivesList.Result_Values == null) &&
	            (formulasResult == null || formulasResult.Result_Values == null) &&
	            (archivesTpList == null || archivesTpList.Result_Values == null)) return null;

	        int countColumn;
	        DateTime dtStart, dtEnd;
	        enumTimeDiscreteType discreteType;

	        if (archivesList != null && archivesList.Result_Values != null && archivesList.Result_Values.Count > 0)
	        {
	            dtStart = archivesList.DTStart;
	            dtEnd = archivesList.DTEnd;
	            countColumn = archivesList.NumbersValues;
	            discreteType = archivesList.DiscreteType;
	        }
	        else if (formulasResult!=null && formulasResult.Result_Values != null && formulasResult.Result_Values.Count > 0 &&
	                 formulasResult.Result_Values[0].Result_Values != null &&
	                 formulasResult.Result_Values[0].Result_Values.Count > 0)
	        {
	            dtStart = formulasResult.DTStart;
	            dtEnd = formulasResult.DTEnd;
	            countColumn = formulasResult.NumbersValues;
	            discreteType = formulasResult.DiscreteType;
	        }
	        else if (archivesTpList != null && archivesTpList.Result_Values != null)
	        {
	            dtStart = archivesTpList.DTStart;
	            dtEnd = archivesTpList.DTEnd;
	            countColumn = archivesTpList.NumbersValues;
	            discreteType = archivesTpList.DiscreteType;
	        }
	        else
	        {
	            return null;
	        }

	        string labelFormat;
	        if (discreteType == enumTimeDiscreteType.DBHours || discreteType == enumTimeDiscreteType.DBHalfHours)
	        {
	            labelFormat = "dd.MM.yy\nHH:mm";
	        }
	        else
	        {
	            labelFormat = "dd.MM.yy";
	        }

	        var userTable = new DataTableEx()
            {
                FieldForSearch = "NameTI",
            };

            userTable.BeginLoadData();

            try
	        {
	            #region -------------Таблица для представления данных----------------------------

	            userTable.Columns.Add("NameTI", typeof(IFreeHierarchyObject));
	            userTable.Columns.Add("Channel", typeof(object));
	            userTable.Columns.Add("Parent", typeof(IFreeHierarchyObject));
	            userTable.Columns.Add("DataSource", typeof(string));

	            var fType = typeof(VALUES_FLAG_DB);

                userTable.Columns.Add(new DataColumn("TotalFlag", fType)
                {
                    Caption = "Итого",
                });

	            var i = 0;
	            foreach (var dt in dts)
	            {
	                userTable.Columns.Add(new DataColumn("Valid" + i, fType)
	                {
	                    Caption = dt.ToString(labelFormat),
	                });
	                i++;
	            }

	            #endregion

	            #region ------------Перебираем ТИ в архиве---------------------------------------

	            if (archivesList != null && archivesList.Result_Values != null)
	            {
	                userTable.Description = new DataTableDescription
	                {
	                    DtStart = archivesList.DTStart,
	                    DiscreteType = archivesList.DiscreteType,
	                    TimeZoneId = archivesList.TimeZoneId,
	                };

	                //сортировка
	                foreach (var pair in archivesList.Result_Values
	                    .Select(v => new Tuple<TPSHierarchy, TArchivesValidate, TInfo_TI>(
	                        EnumClientServiceDictionary.DetailPSList[v.TI_Validate.PS_ID], v,
	                        v.TI_Validate.TI_Ch_ID.IsCA
	                            ? EnumClientServiceDictionary.TICAList[v.TI_Validate.TI_Ch_ID.TI_ID]
	                            : EnumClientServiceDictionary.TIHierarchyList[v.TI_Validate.TI_Ch_ID.TI_ID]))
	                    .Where(v => v.Item1 != null && v.Item3 != null && v.Item2.TI_Validate != null)
	                    .OrderBy(v => v.Item2.TI_Validate.TI_Ch_ID.IsCA)
	                    .ThenBy(v => v.Item1.Name)
	                    .ThenBy(v => v.Item2.TI_Validate.PS_ID)
	                    .ThenBy(v => v.Item3.Name)
	                    .ThenBy(v => v.Item2.TI_Validate.TI_Ch_ID.TI_ID)
	                    .ThenBy(v => v.Item2.TI_Validate.TI_Ch_ID.ChannelType % 10)
	                    .ThenBy(v => v.Item2.TI_Validate.TI_Ch_ID.ChannelType))
	                {
	                    var archiveValidateValue = pair.Item2;
	                    var validate = archiveValidateValue.TI_Validate;
	                    //--------колонки в промежуточную таблицу-----------
	                    var row = userTable.NewRow() as DataRowEx;
	                    if (validate != null)
	                    {
                            row["NameTI"] = pair.Item3;
	                        row["Channel"] = new TTariffPeriodID
	                        {
	                            ChannelType = validate.TI_Ch_ID.ChannelType,
	                            IsOV = false,
	                            TI_ID = validate.TI_Ch_ID.TI_ID,
	                            StartDateTime = dtStart,
	                            FinishDateTime = dtEnd,
	                        };
	                        row["Parent"] = pair.Item1;
	                        row["DataSource"] = EnumClientServiceDictionary
	                            .DataSourceTypeList
	                            .FirstOrDefault(v =>
	                                EqualityComparer<EnumDataSourceType?>.Default.Equals(validate.TI_Ch_ID.DataSourceType,
	                                    v.Key))
	                            .Value;

	                        var id = new ID_Hierarchy
	                        {
	                            ID = validate.TI_Ch_ID.TI_ID.ToString(),
	                            TypeHierarchy =
	                                validate.TI_Ch_ID.IsCA ? enumTypeHierarchy.Info_ContrTI : enumTypeHierarchy.Info_TI
	                        };

                            row["TotalFlag"] = validate.Total_Flag;

                            if (validate.F_FLAG_List != null)
	                        {
	                            for (var j = 0; j < countColumn; j++)
	                            {
	                                var flag = validate.F_FLAG_List.ElementAtOrDefault(j);
	                                row.SetValue("Valid" + j, j, validate.TI_Ch_ID.ChannelType, id, validate.TI_Ch_ID.DataSourceType, null, flag);
	                            }
	                        }
	                    }

	                    userTable.Rows.Add(row);
	                    validate = archiveValidateValue.ContrTI_Validate;
	                    if (validate != null)
	                    {
	                        row = userTable.NewRow() as DataRowEx;
                            row["NameTI"] = "ТИ КА -" + getNameInterface.GetHierarchyDbTreeObject(
	                                            validate.TI_Ch_ID.TI_ID.ToString(),
	                                            validate.TI_Ch_ID.IsCA
	                                                ? enumTypeHierarchy.Info_ContrTI
	                                                : enumTypeHierarchy.Info_TI);
	                        row["Parent"] = EnumClientServiceDictionary.DetailContrPSList[validate.PS_ID];
	                        row["Channel"] = validate.TI_Ch_ID.ChannelType;
	                        if (validate.F_FLAG_List != null)
	                        {
	                            for (var j = 0; j < countColumn; j++)
	                            {
	                                var flag = validate.F_FLAG_List.ElementAtOrDefault(j);
	                                row["Valid" + j] = flag;
	                            }
	                        }

	                        row["TotalFlag"] = validate.Total_Flag;
	                        userTable.Rows.Add(row);
	                    }
	                }
	            }

	            #endregion

	            #region------------Перебираем формулы---------------------------------------------

	            var tps = EnumClientServiceDictionary.GetTps();
	            if (tps == null) return userTable;

	            if (formulasResult != null && formulasResult.Result_Values != null)
	            {
	                if (userTable.Description == null)
	                {
	                    userTable.Description = new DataTableDescription
	                    {
	                        DtStart = formulasResult.DTStart,
	                        DiscreteType = formulasResult.DiscreteType,
	                        TimeZoneId = formulasResult.TimeZoneId,
	                    };
	                }

	                foreach (var pair in formulasResult.Result_Values
	                    .Where(fr => fr.Result_Values != null && fr.Result_Values.Count > 0)
	                    .Select(fr =>

	                        new Tuple<IFreeHierarchyObject, IFormulaInfo, TFormulaResultsList>(fr.TP_CH_ID != null
	                                ? (IFreeHierarchyObject) tps[fr.TP_CH_ID.TP_ID]
	                                : EnumClientServiceDictionary.DetailPSList[fr.PS_ID],
	                            fr.GetFormulaInfo(getNameInterface),
	                            fr))
	                    .OrderBy(p => p.Item1 == null ? "Формулы" : p.Item1.Name)
	                    .ThenBy(p => p.Item2 == null ? string.Empty : p.Item2.FormulaName))
	                {
	                    var formula = pair.Item3;

	                    var formulaResult = formula.Result_Values[0];
	                    var row = userTable.NewRow() as DataRowEx;
                        row["NameTI"] = pair.Item2;
	                    row["Channel"] = null;
	                    row["Parent"] = pair.Item1;

	                    var totalFlag = VALUES_FLAG_DB.None;

	                    enumTypeHierarchy typeHierarchy;
	                    switch (pair.Item2.FormulasTable)
	                    {
	                        case enumFormulasTable.Info_Formula_Description:
	                            typeHierarchy = enumTypeHierarchy.Formula;
	                            break;
	                        case enumFormulasTable.Info_TP2_OurSide_Formula_Description:
	                            typeHierarchy = enumTypeHierarchy.Formula_TP_OurSide;
	                            break;
	                        case enumFormulasTable.Info_TP2_Contr_Formula_Description:
	                            typeHierarchy = enumTypeHierarchy.Formula_TP_CA;
	                            break;
	                        default:
	                            typeHierarchy = enumTypeHierarchy.Unknown;
	                            break;
	                    }

	                    var id = new ID_Hierarchy
	                    {
	                        ID = formulaResult.FormulaId,
	                        TypeHierarchy = typeHierarchy,
	                    };

                        if (formulaResult.Val_List != null && formulaResult.Val_List.Count > 0)
                        {
                            for (int j = 0; j < countColumn; j++)
                            {
                                var v = formulaResult.Val_List.ElementAt(j);
                                if (v != null)
                                {
                                    var flag = v.F_FLAG;
                                    row.SetValue("Valid" + j, j, 0, id, null, null, flag);
                                    totalFlag = totalFlag.CompareAndReturnMostBadStatus(flag);
                                }

                                //row.SetIndex(description, j);
                            }
                        }
	                    row["TotalFlag"] = totalFlag.AccomulateTotalStatistic();
	                    userTable.Rows.Add(row);
	                }
	            }

	            #endregion

	            #region ------------Перебираем ТП в архиве---------------------------------------

	            if (archivesTpList == null || archivesTpList.Result_Values == null) return userTable;

	            var sections = EnumClientServiceDictionary.GetSections();
	            if (sections == null) return userTable;

	            if (userTable.Description == null)
	            {
	                userTable.Description = new DataTableDescription
	                {
	                    DtStart = archivesTpList.DTStart,
	                    DiscreteType = archivesTpList.DiscreteType,
	                    TimeZoneId = archivesTpList.TimeZoneId,
	                };
	            }

                foreach (var pair in archivesTpList.Result_Values
                    .Select(a =>
                    {
                        TPoint tp;
                        tps.TryGetValue(a.Key, out tp);

                        TSection section;
                        sections.TryGetValue(tp.Section_ID, out section);

                        return new Tuple<TSection, TPoint, List<TArchivesTPValueAllChannels>>(section, tp, a.Value);
                    })
                    .Where(p => p.Item1 != null && p.Item2 != null && p.Item3 != null)
                    .OrderBy(p => p.Item1.SectionName)
                    .ThenBy(p => p.Item2.StringName))
                {
                    foreach (var tpArchive in pair.Item3)
                    {
                        if (tpArchive.Val_List == null) continue;

                        Dictionary<byte, List<TPointValue>> tpVals;
                        if (!tpArchive.Val_List.TryGetValue(tpArchive.IsMoneyOurSide, out tpVals) || tpVals == null ||
                            tpVals.Count <= 0) continue;

                        foreach (var valueByChannel in tpVals)
                        {
                            var row = userTable.NewRow() as DataRowEx;
                            row["NameTI"] = pair.Item2;
                            row["Channel"] = valueByChannel.Key;
                            row["Parent"] = pair.Item1;
                            var totalFlag = VALUES_FLAG_DB.None;

                            var id = new ID_Hierarchy
                            {
                                ID = pair.Item2.TP_ID.ToString(),
                                TypeHierarchy = enumTypeHierarchy.Info_TP,
                            };

                            if (valueByChannel.Value != null && valueByChannel.Value.Count > 0)
                            {
                                for (var j = 0; j < countColumn; j++)
                                {
                                    var v = valueByChannel.Value.ElementAtOrDefault(j);
                                    if (v != null)
                                    {
                                        var flag = v.F_FLAG;
                                        row.SetValue("Valid" + j, j, valueByChannel.Key, id, null, null, flag);
                                        totalFlag = totalFlag.CompareAndReturnMostBadStatus(flag);

                                        //row.SetIndex(description, j);
                                    }
                                }
                            }

                            row["TotalFlag"] = totalFlag.AccomulateTotalStatistic();
                            userTable.Rows.Add(row);
                        }
                    }
	            }

	            #endregion

	        }
	        finally
	        {
	            userTable.EndLoadData();
	            userTable.AcceptChanges();
	        }

	        return userTable;
	    }
	}
}
