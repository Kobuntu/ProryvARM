using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.Common;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.Visual.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Proryv.AskueARM2.Client.Visual.Formulas.BalanceEditor.Data
{
    /// <summary>
    /// TODO пока не работает
    /// </summary>
    public class BalanceFreeHierarchyViewModel
    {
        public byte BalanceFreeHierarchyTypeID { get; set; }

        private readonly IFreeHierarchyObject _hierarchyObject;
        public IFreeHierarchyObject HierarchyObject
        {
            get
            {
                return _hierarchyObject;
            }
        }

        private readonly Info_Balance_FreeHierarchy_List _balance;
        public Info_Balance_FreeHierarchy_List Balance
        {
            get
            {
                return _balance;
            }
        }

        public bool IsNew;

        public RangeObservableCollection<BalanceFreeHierarchySectionRow> Source { get; private set; }

        private FrameworkElement BalanceLayout;



        public BalanceFreeHierarchyViewModel(IFreeHierarchyObject hierarchyObject, Info_Balance_FreeHierarchy_List balance)
        {
            _hierarchyObject = hierarchyObject;
            _balance = balance;
            Source = new RangeObservableCollection<BalanceFreeHierarchySectionRow>();
            if (_balance == null)
            {
                //Создание нового
                byte defaultBalanceType = 0;
                _balance = new ServiceReference.ARM_20_Service.Info_Balance_FreeHierarchy_List
                {
                    HierarchyObject = hierarchyObject,
                    BalanceFreeHierarchyType_ID = defaultBalanceType,
                    User_ID = Manager.User.User_ID,
                    DispatchDateTime = DateTime.Now.DateTimeToWCFDateTime(),
                };

                IsNew = true;
            }
            else
            {
                BalanceFreeHierarchyTypeID = balance.BalanceFreeHierarchyType_ID;
            }

            //Подгрузка всех подразделов
            _loadSubsectionsTask = LoadSubsectionsTask();
        }

        #region Обновление баланса в редакторе

        public IFreeHierarchyObject UpdateSource(List<Info_Balance_FreeHierarchy_Description> descriptions, Dispatcher dispatcher, 
            Action onFieldChanged, out bool isAdded)
        {
            
            isAdded = false;

            //List<Info_Balance_FreeHierarchy_Description> descriptions = null;

            if (_balance != null && descriptions == null)
            {
                descriptions = ARM_Service.BL_GetFreeHierarchyBalanceDescritions(_balance.BalanceFreeHierarchy_UN);
            }

            var sections = ARM_Service.BL_GetFreeHierarchyBalanceSections(BalanceFreeHierarchyTypeID);

            if (sections == null || sections.Count == 0)
            {
                Manager.UI.ShowMessage("Не описаны разделы для данного типа баланса!");
                return null;
            }

            var source = new List<BalanceFreeHierarchySectionRow>();
            var isExistsDescription = descriptions != null && descriptions.Count > 0;
            var tps = EnumClientServiceDictionary.GetTps();
            var isFirst = true;
            var isFirstRow = true;

            IFreeHierarchyObject result = null;

            foreach (var section in sections.Where(s => s.UseInTotalResult))
            {
                var row = new BalanceFreeHierarchySectionRow(section, dispatcher);
                if (isFirst)
                {
                    row.IsSelected = true;
                    isFirst = false;
                }

                if (isExistsDescription)
                {
                    //Если есть описание данного баланса
                    foreach (var description in descriptions.Where(d => string.Equals(d.BalanceFreeHierarchySection_UN, section.BalanceFreeHierarchySection_UN)).OrderBy(d => d.SortNumber))
                    {
                        var isIntegral = false;
                        IFreeHierarchyObject hierarchyObject = null;
                        if (description.TI_ID.HasValue)
                        {
                            TInfo_TI ti;
                            if (!EnumClientServiceDictionary.TIHierarchyList.TryGetValue(description.TI_ID.Value, out ti) || ti == null) continue;
                            hierarchyObject = ti;
                        }
                        if (description.IntegralTI_ID.HasValue)
                        {
                            isIntegral = true;
                            TInfo_TI ti;
                            if (!EnumClientServiceDictionary.TIHierarchyList.TryGetValue(description.IntegralTI_ID.Value, out ti) || ti == null) continue;
                            hierarchyObject = ti;
                        }
                        else if (description.TP_ID.HasValue && tps != null)
                        {
                            TPoint tp;
                            if (!tps.TryGetValue(description.TP_ID.Value, out tp) || tp == null) continue;
                            hierarchyObject = tp;
                        }
                        else if (!string.IsNullOrEmpty(description.Formula_UN) && EnumClientServiceDictionary.FormulasList != null)
                        {
                            var formula = EnumClientServiceDictionary.FormulasList[description.Formula_UN];
                            hierarchyObject = formula;
                        }
                        else if (!string.IsNullOrEmpty(description.OurFormula_UN))
                        {
                            TFormulaForSection formula;
                            if (!EnumClientServiceDictionary.FormulaFsk.TryGetValue(description.OurFormula_UN, out formula) || formula == null) continue;
                            hierarchyObject = formula;
                        }
                        else if (!string.IsNullOrEmpty(description.ContrFormula_UN))
                        {
                            TFormulaForSection formula;
                            if (!EnumClientServiceDictionary.FormulaCA.TryGetValue(description.ContrFormula_UN, out formula) || formula == null) continue;
                            hierarchyObject = formula;
                        }
                        else if (description.PTransformator_ID.HasValue)
                        {
                            var transformator = EnumClientServiceDictionary.GetOrAddTransformator(description.PTransformator_ID.Value, null);
                            hierarchyObject = transformator;
                        }
                        else if (description.PReactor_ID.HasValue)
                        {
                            var reactor = EnumClientServiceDictionary.GetOrAddReactor(description.PReactor_ID.Value, null);
                            hierarchyObject = reactor;
                        }
                        else if (description.Section_ID.HasValue)
                        {
                            var hierarhicalSections = EnumClientServiceDictionary.GetSections();
                            TSection hierarhicalSection;
                            if (hierarhicalSections != null && hierarhicalSections.TryGetValue(description.Section_ID.Value, out hierarhicalSection))
                            {
                                hierarchyObject = hierarhicalSection;
                            }
                        }
                        else if (!string.IsNullOrEmpty(description.FormulaConstant_UN))
                        {
                            var formulaConstant = EnumClientServiceDictionary.FormulaConstantDictionary[description.FormulaConstant_UN];
                            hierarchyObject = formulaConstant;
                        }

                        if (hierarchyObject == null) continue;

                        var itemRows = FindDescriptions(description, row, dispatcher);
                        if (itemRows != null)
                        {
                            itemRows.Add(new BalanceFreeHierarchyItemRow(hierarchyObject, description.ChannelType, description.Coef ?? 1, onFieldChanged,
                                description.SortNumber, itemRows, isIntegral, (EnumDataSourceType?)description.DataSource_ID));
                        }

                        //Раскрываем дерево на первом объекте баланса
                        if (isFirstRow)
                        {
                            //fhTree.Dispatcher.BeginInvoke((Action)(() =>
                            //{
                            //    var freeHierTreeId = FormulaEditor_Frame.FindTreeByObjectType(hierarchyObject);
                            //    fhTree.ReloadTree(freeHierTreeId, hierarchyObject: hierarchyObject);
                            //}), DispatcherPriority.Background);

                            result = hierarchyObject;

                            isAdded = true;
                            isFirstRow = false;
                        }
                    }
                }

                source.Add(row);
            }

            dispatcher.BeginInvoke((Action)(()=> Source.AddRange(source)));

            //if (!isAdded)
            //{
            //    Task.Factory.StartNew(() =>
            //    {
            //        Thread.Sleep(100);
            //        //Раскрываем дерево на родителе баланса
            //        fhTree.Dispatcher.BeginInvoke((Action)(() =>
            //           fhTree.ExpandAndSelect(_hierarchyObject, true)), DispatcherPriority.ContextIdle);
            //    });
            //}

            return result;
        }

        private ObservableCollection<BalanceFreeHierarchyItemRow> FindDescriptions(Info_Balance_FreeHierarchy_Description description, 
            BaseBalanceFreeHierarchyRow hierarchyRow, Dispatcher dispatcher)
        {
            string balanceFreeHierarchySubsectionUN;
            if (!string.IsNullOrEmpty(description.BalanceFreeHierarchySubsection_UN))
            {
                balanceFreeHierarchySubsectionUN = description.BalanceFreeHierarchySubsection_UN;
                description.BalanceFreeHierarchySubsection_UN = null;
            }
            else if (!string.IsNullOrEmpty(description.BalanceFreeHierarchySubsection2_UN))
            {
                balanceFreeHierarchySubsectionUN = description.BalanceFreeHierarchySubsection2_UN;
                description.BalanceFreeHierarchySubsection2_UN = null;
            }
            else if (!string.IsNullOrEmpty(description.BalanceFreeHierarchySubsection3_UN))
            {
                balanceFreeHierarchySubsectionUN = description.BalanceFreeHierarchySubsection3_UN;
                description.BalanceFreeHierarchySubsection3_UN = null;
            }
            else
            {
                return hierarchyRow.Descriptions;
            }

            var subsectionRow = hierarchyRow.SubsectionRows.FirstOrDefault(s => Equals(s.Subsection.BalanceFreeHierarchySubsection_UN, balanceFreeHierarchySubsectionUN));
            if (subsectionRow == null && Subsections != null)
            {
                var subsection = Subsections.FirstOrDefault(s => string.Equals(s.BalanceFreeHierarchySubsection_UN, balanceFreeHierarchySubsectionUN));
                if (subsection != null) subsectionRow = new BalanceFreeHierarchySubsectionRow(subsection, hierarchyRow, dispatcher);
                hierarchyRow.SubsectionRows.Add(subsectionRow);
            }

            return subsectionRow == null ? hierarchyRow.Descriptions : FindDescriptions(description, subsectionRow, dispatcher);
        }

        private Task<ObservableCollection<Dict_Balance_FreeHierarchy_Subsection>> _loadSubsectionsTask;

        private ObservableCollection<Dict_Balance_FreeHierarchy_Subsection> _subsections;
        private ObservableCollection<Dict_Balance_FreeHierarchy_Subsection> Subsections
        {
            get
            {
                if (_subsections != null) return _subsections;

                try
                {
                    _subsections = _loadSubsectionsTask.Result;
                }
                catch (AggregateException aex)
                {
                    Manager.UI.ShowMessage(aex.ToException().Message);
                }
                catch (Exception ex)
                {
                    Manager.UI.ShowMessage(ex.Message);
                }

                //Перезапускаем задачу на подгрузку подразделов
                if (_subsections != null) return _subsections;

                _loadSubsectionsTask = LoadSubsectionsTask();
                try
                {
                    _subsections = _loadSubsectionsTask.Result;
                }
                catch
                {
                }

                return _subsections;
            }
        }

        private Task<ObservableCollection<ServiceReference.ARM_20_Service.Dict_Balance_FreeHierarchy_Subsection>> LoadSubsectionsTask()
        {
            return Task<ObservableCollection<Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service.Dict_Balance_FreeHierarchy_Subsection>>.Factory.StartNew(() =>
            {
                ObservableCollection<Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service.Dict_Balance_FreeHierarchy_Subsection> subsections;
                try
                {
                    var request = ServiceFactory.ArmServiceInvokeSync<Tuple<string, List<Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service.Dict_Balance_FreeHierarchy_Subsection>>>("BL_GetFreeHierarchyBalanceSubsections", _hierarchyObject.Type, _hierarchyObject.Id);
                    if (request != null)
                    {
                        subsections = new ObservableCollection<Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service.Dict_Balance_FreeHierarchy_Subsection>(request.Item2);
                        _balance.BalanceFreeHierarchyObject_UN = request.Item1;
                    }
                    else
                    {
                        subsections = new ObservableCollection<Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service.Dict_Balance_FreeHierarchy_Subsection>();
                    }
                }
                catch (Exception ex)
                {
                    Manager.UI.ShowMessage("Ошибка подгрузки подразделов :" + ex.Message);
                    return null;
                }

                //lvBalance.Dispatcher.BeginInvoke((Action)(() =>
                //{
                //    lvBalance.Resources["Subsections"] = subsections;
                //    lvBalance.Resources["BalanceFreeHierarchyObjectUn"] = _balance.BalanceFreeHierarchyObject_UN;

                //}));

                return subsections;
            });
        }

        #endregion

        //private BalanceFreeHierarchyCommand _updateCommand;
        //public BalanceFreeHierarchyCommand UpdateCommand
        //{
        //    get
        //    {
        //        return _updateCommand ??
        //         (_updateCommand = new BalanceFreeHierarchyCommand(obj =>
        //         {
        //             Phone phone = new Phone();
        //             Phones.Insert(0, phone);
        //             SelectedPhone = phone;
        //         }));
        //    }
        //}

        //private bool CanUpdateBalance(object parameter)
        //{
        //    return true;
        //}
    }
}
