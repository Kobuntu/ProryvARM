using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.Common;
using Proryv.AskueARM2.Client.ServiceReference.FreeHierarchyService;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.ServiceReference.UAService;
using Proryv.AskueARM2.Client.Visual.Common.GlobalDictionary;
using RadarSoft.RadarCube.WPF;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Proryv.AskueARM2.Client.ServiceReference.Data;
using Proryv.AskueARM2.Client.ServiceReference.Service.Interfaces;
using Dict_JuridicalPersons_Contract = Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service.Dict_JuridicalPersons_Contract;
using EnumFreeHierarchyItemType = Proryv.AskueARM2.Client.ServiceReference.FreeHierarchyService.EnumFreeHierarchyItemType;
using enumTypeHierarchy = Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service.enumTypeHierarchy;
using Hard_USPD = Proryv.AskueARM2.Client.ServiceReference.FreeHierarchyService.Hard_USPD;
using Proryv.AskueARM2.Client.Visual.Common.Interfaces;

namespace Proryv.AskueARM2.Client.Visual.Common.FreeHierarchy
{
    using Dict_DistributingArrangement = ServiceReference.ARM_20_Service.Dict_DistributingArrangement;

    [DataContract]
    public partial class FreeHierarchyTreeItem : IDisposable, IFindableItemWithPath, INotifyPropertyChanged, IKey,
        IUpdateVisual, ITreeDescriptor
    {
        public FreeHierarchyTreeDescriptor Descriptor { get; set; }

        [DataMember] public int? SortNumber;

        public readonly int HierLevel;

        public FreeHierarchyTreeItem(FreeHierarchyTreeDescriptor descriptor, FreeHierarchyTreeItem parent, bool isSelected = false,
            string nodeName = "")
        {
            Descriptor = descriptor;
            Parent = parent;
            _isSelected = isSelected;
            _nodeName = nodeName;
            if (parent != null)
            {
                HierLevel = parent.HierLevel + 1;
            }

            _pathSynk = new object();
            Opacity = 1;

            _children = new RangeObservableCollection<FreeHierarchyTreeItem>(new FreeHierarchyTreeItemComparer());
        }

        public FreeHierarchyTreeItem(FreeHierarchyTreeDescriptor descriptor, IFreeHierarchyObject freeHierarchyObject,
            bool isSelected = false, string nodeName = "",
            int freeHierItemID = 0, FreeHierarchyTreeItem parent = null, bool includeObjectChildren = false,
            bool isHideTi = false,
            UserRightsForTreeObject nodeRights = null,
            EnumFreeHierarchyItemType freeHierItemType = EnumFreeHierarchyItemType.Error,
            bool isChildrenInitializet = false, bool notLoaded = false) : this(descriptor, parent, isSelected, nodeName)
        {
            FreeHierItem_ID = freeHierItemID;
            IncludeObjectChildren = includeObjectChildren;
            IsHideTi = isHideTi;
            NodeRights = nodeRights;

            _hierObject = freeHierarchyObject;
            IsChildrenInitializet = isChildrenInitializet;

            if (_hierObject != null)
            {
                _hierObject.GetNodeRight = GetNodeRight;
            }

            FreeHierItemType = freeHierItemType;
            NotLoaded = notLoaded;
        }

        [DataMember] private bool _isSelected;

        /// <summary>
        /// Выбран объект в дереве или нет
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected == value 
                    || !IsSelectableByPermissibleSettings
                    || (Descriptor != null && Equals(Descriptor.PreviousSelected, this))) return;

                _isSelected = value;
                if (Descriptor != null)
                {
                    if (Descriptor.IsSelectSingle)
                    {
                        if (Descriptor.PreviousSelected != null)
                        {
                            var ps = Descriptor.PreviousSelected;
                            Descriptor.PreviousSelected = null;
                            ps.IsSelected = false;
                        }

                        if (value) Descriptor.PreviousSelected = this;
                    }

                    if ((Parent == null || Parent.IsExpandedFromVisual) && PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("IsSelected"));
                    }

                    Descriptor.RemoveAddSelected(_isSelected, this);

                    if (Parent != null)
                    {
                        if (_isSelected)
                        {
                            Parent.SelectedChildrenCount++;
                        }
                        else
                        {
                            Parent.SelectedChildrenCount--;
                        }

                        Descriptor.ProcessParentCheckboxes(new Dictionary<int, HashSet<int>> { { Parent.HierLevel, new HashSet<int> { Parent.FreeHierItem_ID } } });

                        //Parent.UpdateParentCheckBoxStyles(); 
                    }

                    Descriptor.RaiseSelectedChanged();
                }
            }
        }

        #region Выдление объекта на дереве

        private bool _isSelectedNode;

        /// <summary>
        /// Объект выделен в дереве
        /// </summary>
        public bool IsSelectedNode
        {
            get { return _isSelectedNode; }
            set
            {
                if (_isSelectedNode == value) return;

                _isSelectedNode = value;

                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("IsSelectedNode"));
            }
        }

        [DataMember] private bool _isSelectedChildren;

        /// <summary>
        /// Признак того, что выбран хоть один дочерний. 
        /// </summary>
        public bool IsSelectedChildren
        {
            get { return _isSelectedChildren; }
            set
            {
                if (_isSelectedChildren == value) return;

                _isSelectedChildren = value;

                //UpdateParentCheckBoxStyles();
            }
        }

        /// <summary>
        /// Объект доступен для выбора исходя из его тип, по настройкам дерева IPermissibleForSelectObjects
        /// </summary>
        public bool IsSelectableByPermissibleSettings;

        #endregion

        private bool? _hasSeeDbObjectsRight;

        public bool HasSeeDbObjectsRight
        {
            get
            {
                if (_hasSeeDbObjectsRight.HasValue) return _hasSeeDbObjectsRight.Value;

                _hasSeeDbObjectsRight = Manager.User.IsAssentRight(HierObject, EnumObjectRightType.SeeDbObjects);
                return _hasSeeDbObjectsRight.Value;
            }
        }

        private Visibility _visibility = Visibility.Visible;

        [DataMember]
        public Visibility Visibility
        {
            get { return _visibility; }
            set
            {
                if (_visibility != value)
                {
                    _visibility = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("Visibility"));
                }
            }
        }

        /// <summary>
        /// Право данного пользователя на этот узел(объект если описан как объект)
        /// </summary>
        [DataMember] public UserRightsForTreeObject NodeRights;

        /// <summary>
        /// Статус ТИ вложенных объектов
        /// </summary>
        [DataMember] public EnumTIStatus TIStatus;

        private EnumSelectedManyCheckBoxStyle? _checkBoxStyle;

        public EnumSelectedManyCheckBoxStyle? CheckBoxStyle
        {
            get { return _checkBoxStyle; }
            set
            {
                if (_checkBoxStyle == value) return;

                _checkBoxStyle = value;
                if (PropertyChanged != null
                    && (Parent == null || Parent.IsExpandedFromVisual))
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("CheckBoxStyle"));
                }
            }
        }

        private FreeHierarchyTreeItem _parent;

        /// <summary>
        /// Родитель
        /// </summary>
        public FreeHierarchyTreeItem Parent
        {
            get { return _parent; }
            set
            {
                _parent = value;
                if (_parent != null)
                {
                    TreePathKey = (_parent.TreePathKey ?? string.Empty) + "/" + ((ushort)_parent.FreeHierItemType) +
                                  "," + _parent.GetKey;
                }
            }
        }


        /// <summary>
        /// Идентификатор узла
        /// </summary>
        [DataMember] public int FreeHierItem_ID;

        [DataMember] private long? _id;

        public long Id
        {
            get
            {
                if (_id.HasValue) return _id.Value;
                int treeHashId = 0;
                if (Descriptor != null) treeHashId = Descriptor.TreeHashId;
                _id = (long)FreeHierItem_ID << 32 | treeHashId;

                return _id.Value;
            }
        }

        /// <summary>
        /// Изменяем порядок элемента в дереве FH
        /// </summary>
        /// <param name="index"></param>
        public void SetTreeItemSortNumber(int Tree_ID, int FreeHierItem_ID, int Index, int? ParentFreeHierItemId)
        {

            FreeHierarchyService.SetTreeItemSortNumber(Manager.User.User_ID,
                new List<SetTreeItemSortNumberDataRequest>()
                {
                    new SetTreeItemSortNumberDataRequest()
                    {
                        FreeHierItemId = FreeHierItem_ID,
                        TreeId = Tree_ID,
                        NewIndex = Index,
                        ParentFreeHierItemId = ParentFreeHierItemId

                    }
                });
        }

        [DataMember] private string _stringName;

        /// <summary>
        /// Название
        /// </summary>
        public string StringName
        {
            get
            {

                if (_stringName != null) return _stringName;

                switch (FreeHierItemType)
                {
                    case EnumFreeHierarchyItemType.Error:
                        _stringName = "Ошибка";
                        break;
                    case EnumFreeHierarchyItemType.Node:
                    case EnumFreeHierarchyItemType.CommonDocuments:
                        _stringName = _nodeName ?? "Ошибка";
                        break;
                    case EnumFreeHierarchyItemType.Formula:
                    case EnumFreeHierarchyItemType.HierLev1:
                    case EnumFreeHierarchyItemType.HierLev2:
                    case EnumFreeHierarchyItemType.HierLev3:
                    case EnumFreeHierarchyItemType.PS:
                    case EnumFreeHierarchyItemType.Section:
                    case EnumFreeHierarchyItemType.TI:
                    case EnumFreeHierarchyItemType.TP:
                    case EnumFreeHierarchyItemType.Contract:
                    case EnumFreeHierarchyItemType.DirectConsumer:
                    case EnumFreeHierarchyItemType.JuridicalPerson:
                    case EnumFreeHierarchyItemType.BusSystem:
                    case EnumFreeHierarchyItemType.DistributingArrangement:
                    case EnumFreeHierarchyItemType.USPD:
                    case EnumFreeHierarchyItemType.UANode:
                    case EnumFreeHierarchyItemType.UAServer:
                    case EnumFreeHierarchyItemType.FormulaConstant:
                    case EnumFreeHierarchyItemType.ForecastObject:
                    case EnumFreeHierarchyItemType.OurFormula:
                    case EnumFreeHierarchyItemType.OurFormulaDescription:
                    case EnumFreeHierarchyItemType.XMLSystem:
                    case EnumFreeHierarchyItemType.CAFormula:
                    case EnumFreeHierarchyItemType.CAFormulaDescription:
                    case EnumFreeHierarchyItemType.OldTelescopeTreeNode:
                        _stringName = HierObject != null ? HierObject.Name : "Ошибка";
                        break;
                    default:
                        _stringName = _nodeName ?? "Ошибка";
                        break;
                }


                if (FIASNode != null)
                    return FIASNode.StringName;


                return _stringName;
            }
            set
            {
                _stringName = value;

                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("StringName"));
            }
        }

        public string Name
        {
            get { return StringName; }
        }

        /// <summary>
        /// Название узла в дереве
        /// </summary>
        private readonly string _nodeName;

        /// <summary>
        /// Получить объкт который представляет узел
        /// </summary>
        public object ItemObject
        {
            get
            {
                switch (FreeHierItemType)
                {
                    case EnumFreeHierarchyItemType.Error:
                        return this;
                    case EnumFreeHierarchyItemType.CommonDocuments:
                        return this;
                    case EnumFreeHierarchyItemType.Formula:
                    case EnumFreeHierarchyItemType.Node:
                    case EnumFreeHierarchyItemType.HierLev1:
                    case EnumFreeHierarchyItemType.HierLev2:
                    case EnumFreeHierarchyItemType.HierLev3:
                    case EnumFreeHierarchyItemType.PS:
                    case EnumFreeHierarchyItemType.Section:
                    case EnumFreeHierarchyItemType.TI:
                    case EnumFreeHierarchyItemType.TP:
                    case EnumFreeHierarchyItemType.Contract:
                    case EnumFreeHierarchyItemType.DirectConsumer:
                    case EnumFreeHierarchyItemType.JuridicalPerson:
                    case EnumFreeHierarchyItemType.BusSystem:
                    case EnumFreeHierarchyItemType.DistributingArrangement:
                    case EnumFreeHierarchyItemType.USPD:
                    case EnumFreeHierarchyItemType.UANode:
                    case EnumFreeHierarchyItemType.UAServer:
                    case EnumFreeHierarchyItemType.PTransformator:
                    case EnumFreeHierarchyItemType.Reactor:
                    case EnumFreeHierarchyItemType.FormulaConstant:
                    case EnumFreeHierarchyItemType.ForecastObject:
                    case EnumFreeHierarchyItemType.OurFormula:
                    case EnumFreeHierarchyItemType.OldTelescopeTreeNode:
                        return HierObject != null ? (object)HierObject : this;
                    case EnumFreeHierarchyItemType.XMLSystem:
                        return XMLSystem != null ? (object)XMLSystem : this;
                    //return OurFormula != null ? (object)OurFormula : this;
                    case EnumFreeHierarchyItemType.CAFormula:
                        return CAFormula != null ? (object)CAFormula : this;
                    case EnumFreeHierarchyItemType.OurFormulaDescription:
                        return OurFormulaDescription != null ? (object)OurFormulaDescription : this;
                    case EnumFreeHierarchyItemType.CAFormulaDescription:
                        return CAFormulaDescription != null ? (object)CAFormulaDescription : this;
                    case EnumFreeHierarchyItemType.FiasFullAddress:
                        return FIASNode != null ? (object)FIASNode : this;
                    default:
                        return this;
                }

            }
            set { throw new Exception(string.Format("Старое ItemObject: {0}, новое: {1}", ItemObject, value)); }

        }

        //Для совместимости со старыми деревьями
        public object Item
        {
            get { return ItemObject; }
        }

        private EnumFreeHierarchyItemType? _freeHierItemType;

        /// <summary>
        /// Тип узла
        /// </summary>
        [DataMember] public EnumFreeHierarchyItemType FreeHierItemType
        {
            get
            {
                return _freeHierItemType ?? EnumFreeHierarchyItemType.Error;
            }
            set
            {
                if (_freeHierItemType == value) return;

                _freeHierItemType = value;

                //Проверяем, доступен ли объект для выбора
                if (_freeHierItemType != EnumFreeHierarchyItemType.Error
                    && Descriptor != null && Descriptor.PermissibleForSelectObjects != null
                    && Descriptor.PermissibleForSelectObjects.PermissibleForSelectObjects != null
                    && !Descriptor.PermissibleForSelectObjects.PermissibleForSelectObjects.Contains(_freeHierItemType.Value))
                {
                    IsSelectableByPermissibleSettings = false;
                }
                else
                {
                    IsSelectableByPermissibleSettings = true;
                }
            }
        }

        private IFreeHierarchyObject _hierObject;

        /// <summary>
        /// Идентификатор уровня иерархии
        /// </summary>
        public IFreeHierarchyObject HierObject
        {
            get { return _hierObject; }
            set
            {
                if (ReferenceEquals(_hierObject, value)) return;

                _hierObject = value;
                if (_hierObject != null)
                {
                    _hierObject.GetNodeRight = GetNodeRight;
                }

                _hashCode = -1;

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("HierObject"));
                }
            }
        }

        public Proryv.AskueARM2.Client.ServiceReference.DeclaratorService.TFIASNode FIASNode;

        [DataMember] public TUANode UANode;

        /// <summary>
        /// XML Система
        /// </summary>
        [DataMember] public Expl_XML_System_ID_List XMLSystem;

        /// <summary>
        /// Формула (наша сторона)
        /// </summary>
        //public TFormulaForSection OurFormula;

        /// <summary>
        /// Формула (КА)
        /// </summary>
        [DataMember] public TFormulaForSection CAFormula;

        /// <summary>
        /// Компонент формулы (наша сторона)
        /// </summary>
        [DataMember] public Info_OurSide_Formula_Description_Ex OurFormulaDescription;

        /// <summary>
        /// Компонент формулы (КА)
        /// </summary>
        [DataMember] public Info_Contr_Formula_Description_Ex CAFormulaDescription;

        /// <summary>
        /// Распределительное устройство
        /// </summary>
        [DataMember] public Dict_DistributingArrangement DistributingArrangement;

        /// <summary>
        /// должны ли быть отображены дочерение объекты сущности , к примеру ТИ для ТП
        /// </summary>
        [DataMember] public bool IncludeObjectChildren;

        [DataMember] public bool IsHideTi;

        /// <summary>
        /// Включать ТИ без подгрузки из БД (например в модуле малых ТИ, там ТИ подгружаются заранее)
        /// </summary>
        [DataMember] public bool IncludeTiAnyway;

        private static readonly List<EnumFreeHierarchyItemType> SortPriority = new List<EnumFreeHierarchyItemType>
        {
            EnumFreeHierarchyItemType.HierLev1,
            EnumFreeHierarchyItemType.HierLev2,
            EnumFreeHierarchyItemType.HierLev3,
            EnumFreeHierarchyItemType.PS,
            EnumFreeHierarchyItemType.Formula
        };

        /// <summary>
        /// Была полная инициализация дочерних объектов
        /// </summary>
        public volatile bool IsChildrenInitializet;

        /// <summary>
        /// Была инициализация объектов без догрузки с сервера
        /// </summary>
        public volatile bool IsLocalChildrenInitializet;

        [DataMember] public bool IsUaNodesInitializet;
        [DataMember] public bool IsFiasNodesInitializet;

        [DataMember] public volatile bool IsTisNodesInitializet;

        [DataMember] public volatile bool IsFreeHierLoadedInitializet;

        [DataMember] public bool IsTransformatorsAndReactorsInitializet;

        [DataMember] public bool IsUspdAndE422Initializet;

        private readonly object _syncLock = new object();

        public bool HasChildren
        {
            get
            {
                //lock (_syncLock)
                {
                    return !IsChildrenInitializet || _isExpandProcessed || (_children != null && _children.Count > 0);
                }
            }
        }

        //(!IsTisNodesInitializet && FreeHierItemType!=EnumFreeHierarchyItemType.TI)

        private readonly RangeObservableCollection<FreeHierarchyTreeItem> _children;

        [DataMember]
        public RangeObservableCollection<FreeHierarchyTreeItem> Children
        {
            get
            {
                //lock (_syncLock)
                {
                    return _children;
                }
            }
        }



        /// <summary>
        /// Это свойство только для работы в визуалке
        /// </summary>
        public RangeObservableCollection<FreeHierarchyTreeItem> ChildrenVisual { get; private set; }


        //public Action<FreeHierarchyTreeItem> UpdateItemContent;

        /// <summary>
        /// Уведомляем визуалку  об изменениях
        /// </summary>
        public void UpdateVisual()
        {
            if (PropertyChanged != null)
            {
                //if (UpdateItemContent != null) UpdateItemContent(this);

                UpdateTis(EnumTIStatus.All);
            }
        }

        private bool expanded = false;
        [DataMember] private volatile bool _isExpandProcessed;

        public bool IsExpandProcessed
        {
            get { return _isExpandProcessed; }
            set
            {
                if (_isExpandProcessed == value) return;

                _isExpandProcessed = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("IsExpandProcessed"));

                    //if (!value)
                    //{
                    //    PropertyChanged(this, new PropertyChangedEventArgs("HasChildren"));
                    //}
                }
            }
        }

        public bool IsExpandedFromVisual;

        public void UpdateVisualUIChildren(DispatcherObject dispatcherObject, Action uiThreadAction)
        {
            if (dispatcherObject == null) dispatcherObject = Application.Current;

            //if (dispatcherObject.)
            {
                dispatcherObject.Dispatcher.BeginInvoke((Action)(() =>
               {
                   if (PropertyChanged != null)
                       PropertyChanged(this, new PropertyChangedEventArgs("HasChildren"));

                   IsExpandProcessed = false;

                   if (uiThreadAction != null) uiThreadAction();
               }));
            }
        }

        public string ToSerializedProryvObject()
        {
            return (new ProryvObject()
            {
                ObjectID = (ItemObject as IKey).GetKey,
                TableName = FreeHierItemType.ToString(),
                Value = ItemObject.ToString()
            }).Serialize();
        }

        public void Dispose()
        {
            try
            {
                //lock (_syncLock)
                {
                    if (_children != null && _children.Count > 0)
                    {
                        foreach (var ch in _children)
                        {
                            ch.Dispose();
                        }

                        _children.Clear(true);

                        //Children = null;
                    }
                }

                Parent = null;

                if (_hierObject != null)
                {
                    _hierObject.GetNodeRight = null;
                    _hierObject = null;
                }

                if (Descriptor != null)
                {
                    Descriptor = null;
                }

                //if (_updateParentStyleTimer != null)
                //{
                //    _updateParentStyleTimer.Tick += OnUpdateParentCallback;
                //}

                //UpdateItemContent = null;
            }
            catch (Exception ex)
            {
                Manager.UI.ShowMessage(ex.Message);
            }
        }

        public object GetItemForSearch()
        {
            //return this;
            return ItemObject;
        }

        public object GetItemForTemplate
        {
            get { return this; }
        }

        public IEnumerable GetChildren()
        {
            //TODO временное решение
            if (IsChildrenInitializet) return Children;

            LoadDynamicChildren(isLoadFromServer: false);
            return Children;
        }

        public override string ToString()
        {
            return StringName;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void PropertyChangedCall(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Смена стиля для выделенного объекта

        private DispatcherTimer _updateParentStyleTimer;

        public void UpdateParentCheckBoxStyles()
        {
            //if (Parent == null || Parent.IsExpandedFromVisual)
            //{
            //    if (_updateParentStyleTimer == null)
            //    {
            //        _updateParentStyleTimer = new DispatcherTimer();
            //        _updateParentStyleTimer.Tick += OnUpdateParentCallback;
            //        _updateParentStyleTimer.Interval = TimeSpan.FromMilliseconds(100);
            //    }

            //    _updateParentStyleTimer.Start();
            //}
            //else
            //{
                //UpdateCheckBoxChildrenStyles();
                if (Parent != null) Parent.UpdateParentCheckBoxStyles();
            //}
        }

        //private void OnUpdateParentCallback(object sender, EventArgs e)
        //{
        //    if (_updateParentStyleTimer != null)
        //    {
        //        _updateParentStyleTimer.Stop();
        //    }

        //    UpdateCheckBoxChildrenStyles();
        //    if (Parent != null) Parent.UpdateParentCheckBoxStyles();
        //}

        public void UpdateCheckBoxChildrenStyles()
        {

            //Тут определяем по дочерним, идет от низа к верху
            if (IsChildrenInitializet || IsUaNodesInitializet || IsFreeHierLoadedInitializet)
            {
                if (_children == null || _children.Count == 0) //TODO Проверить подгруженность
                {
                    CheckBoxStyle = EnumSelectedManyCheckBoxStyle.None;
                }
                else
                {
                    //Есть пометка что выбран хоть один дочерний
                    bool haveAll = false, haveNone = false;
                    foreach (var c in _children)
                    {
                        if (c.CheckBoxStyle == EnumSelectedManyCheckBoxStyle.PartSelected)
                        {
                            CheckBoxStyle = EnumSelectedManyCheckBoxStyle.PartSelected;
                            return;
                        }

                        if ((c.IsSelectableByPermissibleSettings && !c.IsSelected) 
                            || (!c.IsSelectableByPermissibleSettings && c.CheckBoxStyle == EnumSelectedManyCheckBoxStyle.None))
                        {
                            if (!haveNone) haveNone = true;
                        }
                        else if (!haveAll)
                        {
                            haveAll = true;
                        }
                    }

                    if (haveNone)
                    {
                        if (haveAll)
                        {
                            CheckBoxStyle = EnumSelectedManyCheckBoxStyle.PartSelected;
                        }
                        else
                        {
                            CheckBoxStyle = EnumSelectedManyCheckBoxStyle.None;
                        }
                    }
                    else if (haveAll)
                    {
                        CheckBoxStyle = EnumSelectedManyCheckBoxStyle.AllSelected;
                    }
                    else
                    {
                        CheckBoxStyle = EnumSelectedManyCheckBoxStyle.PartSelected;
                    }
                }
            }
            else if (IsSelectedChildren)
            {
                CheckBoxStyle = EnumSelectedManyCheckBoxStyle.AllSelected;
            }
            else
            {
                CheckBoxStyle = EnumSelectedManyCheckBoxStyle.None;
            }
        }

        #endregion

        /// <summary>
        /// Используется для внутренних нужд
        /// </summary>
        private int? USPD_ID;

        public void SetUSPD_ID(int value)
        {
            USPD_ID = value;
        }

        public int? GetUSPD_ID()
        {
            return USPD_ID;
        }

        private string _key;

        public string GetKey
        {
            get
            {
                if (!string.IsNullOrEmpty(_key)) return _key;

                if (HierObject != null)
                {
                    _key = HierObject.StringId ?? HierObject.Id.ToString();
                }
                else
                {
                    var keyable = ItemObject as IKey;
                    if (keyable != null && !ReferenceEquals(keyable, this))
                    {
                        _key = keyable.GetKey;
                    }
                    else
                    {
                        _key = FreeHierItem_ID.ToString();
                    }
                }

                return _key;
            }
        }

        public Stack GetParents()
        {
            var stack = new Stack();
            FreeHierarchyTreeItem item = this;
            while (item != null)
            {
                stack.Push(item);
                item = item.Parent;
            }

            return stack;
        }

        /// <summary>
        /// Возвращаем права узла и родителей
        /// </summary>
        /// <returns></returns>
        public UserRightsForTreeObject GetNodeRight()
        {
            return NodeRights;
        }

        private readonly object _pathSynk;
        private string _toRootPath;

        /// <summary>
        /// Путь у началу дерева
        /// </summary>
        public string ToRootPath
        {
            get
            {
                lock (_pathSynk)
                {
                    if (!string.IsNullOrEmpty(_toRootPath))
                    {

                        return _toRootPath;
                    }
                }

                if (!_isRootRequested)
                {
                    Task.Factory.StartNew(UpdateToRootPath);
                }

                return string.Empty;
            }
            set
            {
                lock (_pathSynk)
                {
                    _toRootPath = value;
                }

                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                    (Action)(() =>
                   {
                       if (PropertyChanged != null)
                           PropertyChanged(this, new PropertyChangedEventArgs("ToRootPath"));
                   }));
            }
        }

        private volatile bool _isRootRequested;

        private void UpdateToRootPath()
        {
            try
            {
                var toRootpath = new StringBuilder();
                BuildToRootPath(toRootpath, Parent);
                if (toRootpath.Length > 0)
                {
                    ToRootPath = toRootpath.ToString();
                }
            }
            finally
            {
                _isRootRequested = false;
            }
        }

        private void BuildToRootPath(StringBuilder toRootPath, FreeHierarchyTreeItem parent)
        {
            if (parent == null) return;
            toRootPath.Insert(0, "\\" + parent.StringName);
            BuildToRootPath(toRootPath, parent.Parent);
        }

        public void AddChildren(FreeHierarchyTreeItem newItem)
        {
            //lock (_syncLock)
            {
                //не обновляется в дереве, поэтому пересоздаем _uiChildren
                _children.Add(newItem);

                //_children = new RangeObservableCollection<FreeHierarchyTreeItem>(Children,
                //new FreeHierarchyTreeItemComparer());
            }

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                PropertyChangedCall("IsExpandProcessed");
                // PropertyChangedCall("HasChildren");
            }));
        }

        public void RemoveChildren(FreeHierarchyTreeItem item)
        {
            //lock (_syncLock)
            {
                _children.Remove(item);

                //_children = new RangeObservableCollection<FreeHierarchyTreeItem>(_children,
                //new FreeHierarchyTreeItemComparer());
            }


            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                PropertyChangedCall("IsExpandProcessed");
                PropertyChangedCall("HasChildren");
            }));
        }

        /// <summary>
        /// очистка дочерних узлов 
        /// при добвлении сечения на объект его дочерние не нужны
        /// </summary>
        /// <param name="newItem"></param>
        public void ClearChildren()
        {
            //lock (_syncLock)
            {
                if (_children != null)
                {
                    _children.Clear(true);
                }

                ResetFlags();
            }
        }

        private void ResetFlags()
        {
            IsChildrenInitializet = false;
            IsTisNodesInitializet = false;
            IsFiasNodesInitializet = false;
            IsTransformatorsAndReactorsInitializet = false;
            IsUspdAndE422Initializet = false;
            IsUaNodesInitializet = false;
        }

        public int Getlevel()
        {
            int result = 0;
            try
            {
                FreeHierarchyTreeItem item = this.Parent;
                while (item != null)
                {
                    result++;
                    item = item.Parent;

                }
            }
            catch
            {
            }

            return result;
        }

        public void ReloadUaNodeBranch(Queue<long> uaNodes)
        {
            if (uaNodes == null || uaNodes.Count == 0) return;

            LoadDynamicChildren();

            if (Children == null || Children.Count == 0) return;

            var uaNodeId = uaNodes.Dequeue();
            var nextNode = Children.FirstOrDefault(ch => ch.FreeHierItemType == EnumFreeHierarchyItemType.UANode
                                                         && (ch.HierObject as TUANode).UANodeId == uaNodeId);

            if (nextNode != null)
            {
                nextNode.ReloadUaNodeBranch(uaNodes);
            }
        }

        /// <summary>
        /// Уникальный путь узла в дереве
        /// </summary>
        public string TreePathKey;

        private int? _nodeIconID;

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            var p = obj as FreeHierarchyTreeItem;
            if (p != null)
            {
                

                if (_hashCode.HasValue && p._hashCode.HasValue)
                {
                    return _hashCode == p._hashCode;
                }

                if (HierObject != null)
                {
                    return Equals(HierObject, p.HierObject);
                }

                //Для поиска ItemObject должен быть переписан метод Equals
                return p.FreeHierItemType == FreeHierItemType && p.FreeHierItem_ID == FreeHierItem_ID;
            }
            else
            {
                //Поиск объекта по дереву
                var idType = obj as IDHierarchy;
                if (idType != null)
                {
                    //По дереву свободной иерархии
                    if (FreeHierItem_ID >= 0 && idType.FreeHierItemId.HasValue)
                    {
                        return FreeHierItem_ID == idType.FreeHierItemId.Value;
                    }

                    if (HierObject != null)
                    {
                        //Должен совпадать тип
                        if (HierObject.Type != idType.TypeHierarchy) return false;

                        if (idType.ID > 0)
                        {
                            return HierObject.Id == idType.ID;
                        }

                        return string.Equals(HierObject.StringId, idType.StringId);
                    }
                }
            }

            return Equals(HierObject, obj);
        }

        /// <summary>
        /// Для ускорения сравнивания
        /// </summary>
        /// <param name="treeItem"></param>
        /// <returns></returns>
        public bool Equals(FreeHierarchyTreeItem treeItem)
        {
            if (treeItem == null
                || (!string.IsNullOrEmpty(TreePathKey) && !string.IsNullOrEmpty(treeItem.TreePathKey)
                                                       && string.Compare(TreePathKey, treeItem.TreePathKey,
                                                           StringComparison.Ordinal) != 0)) return false;

            if (HierObject != null && treeItem.HierObject != null)
            {
                return HierObject.Id == treeItem.HierObject.Id && HierObject.Type == treeItem.HierObject.Type;
            }

            return FreeHierItemType == treeItem.FreeHierItemType;
        }


        private long? _hashCode;

        public override int GetHashCode()
        {
            if (_hashCode.HasValue) return (int)_hashCode.Value;

            if (FreeHierItem_ID >= 0 && !string.IsNullOrEmpty(TreePathKey))
            {
                _hashCode = TreePathKey.GetHashCode();
            }
            else if (HierObject != null)
            {
                var type = (int)HierObject.Type;

                type = (type << 24) | (type >> (32 - 24));

                if (HierObject.Id > 0 || string.IsNullOrEmpty(HierObject.StringId))
                {
                    _hashCode = HierObject.Id ^ type;
                }
                else
                {
                    _hashCode = HierObject.StringId.GetHashCode() ^ type;
                }
            }
            else
            {
                _hashCode = FreeHierItem_ID ^ ((ushort)FreeHierItemType * 397);
            }

            return (int)_hashCode.Value;
        }


        private string _stringId;
        public string StringId
        {
            get
            {
                if (!string.IsNullOrEmpty(_stringId)) return _stringId;

                if (HierObject == null) return string.Empty;


                _stringId = (HierObject.Id > 0 && !HierarchyObjectHelper.IsStringId(this.FreeHierItemType) ? HierObject.Id.ToString() : HierObject.StringId) + ","
                                                                                                 + (int)HierObject.Type + ","
                                                                                                 + (FreeHierItem_ID >= 0 ? FreeHierItem_ID.ToString() : "");


                return _stringId;
            }
        }

        /// <summary>
        /// возвращает FreeHierarchyItemID, по которому будут проверяться права
        /// если узел реальный - возвращается его ИД, если нет - ИД родителя
        /// т.к. родитель тоже может быть виртуальным находим FreeHierarchyItemID первого реального родительского объекта
        /// </summary>
        /// <returns></returns>
        public int GetFirstRealParentFreeHierarchyItemID()
        {
            int result = FreeHierItem_ID;

            if (FreeHierItem_ID >= 0 || Parent == null) return result;

            FreeHierarchyTreeItem parent = Parent;
            int level = 0;
            while (parent != null && level < 20)
            {
                level++;
                result = parent.FreeHierItem_ID;
                if (result > 0)
                    break;
                parent = parent.Parent;
            }

            return result;
        }

        public int? NodeIcon_ID
        {
            get
            {
                //объекты кэшируютс, поэтому сбрасываем ..но... если открыть 2 дерева разных рядом  то ..
                if (Descriptor != null && Descriptor.Tree != null && Descriptor.Tree_ID <= 0)
                {
                    _nodeIconID = null;

                }

                return _nodeIconID;
            }
            set { _nodeIconID = value; }
        }

        public double Opacity { get; set; }

        public void OnVisualExpanded(bool isDeclaratorMainTree, DispatcherObject dispatcherObject)
        {
            if (!IsExpandProcessed)
            {

                //иначе не обновляется после добавления чего- либо
                if (isDeclaratorMainTree)
                {
                    IsTisNodesInitializet = false;
                    IsFiasNodesInitializet = false;
                    IsTransformatorsAndReactorsInitializet = false;
                    IsUspdAndE422Initializet = false;
                    IsUaNodesInitializet = false;

                    //em.ReloadChildren(true, false, true);
                }

                var isHideTp = false;
                if (Descriptor != null)
                {
                    isHideTp = Descriptor.IsHideTp;
                }

                //здесь надо обновлять визуалку
                LoadDynamicChildren(IsHideTi, dispatcherObject, isHideTp: isHideTp);
            }

            ChildrenVisual = Children;
        }

        public void OnVisualCollapsed()
        {
            ChildrenVisual = null;
            //ClearChildren();
            //GC.Collect();
        }

        /// <summary>
        /// Признак того, что этот объект еще не загружен в дерево
        /// Нужно чтобы выделять объекты без необходимости реальной прогрузки дочерних
        /// </summary>
        public bool NotLoaded;

        #region Выбор объекта и дочерних

        public int SelectedChildrenCount;

        /// <summary>
        /// Основная процедура выбора объектов
        /// </summary>
        /// <param name="isSelect">Выбрать/снять</param>
        /// <param name="isRecursive">Продолжать на дочерние объекты</param>
        /// <param name="itemType">Тип объектов для выбора</param>
        /// <returns></returns>
        public void SelectUnselect(bool isSelect, 
            List<FreeHierarchyTreeItem> itemsForPrepare,
            Dictionary<int, HashSet<int>> itemsByLevForUpdateChekbox,
            bool isRecursive = false,
            EnumFreeHierarchyItemType? itemType = null, int? maxlevel = null, List<FreeHierarchyTreeItem> selectedItems = null)
        {
            if (isRecursive && HasChildren)
            {
                SelectChildren(isSelect, itemsForPrepare, itemsByLevForUpdateChekbox, isRecursive, itemType, 
                    maxlevel:maxlevel, selectedItems: selectedItems);
            }

            //Выбираем сам объект, если нужно
            if (SelectHimself(isSelect, false, itemType, selectedItems) && Parent!=null)
            {
                HashSet<int> itemsForUpdateChekbox;
                if (!itemsByLevForUpdateChekbox.TryGetValue(Parent.HierLevel, out itemsForUpdateChekbox))
                {
                    itemsForUpdateChekbox = new HashSet<int>();
                    itemsByLevForUpdateChekbox[Parent.HierLevel] = itemsForUpdateChekbox;
                }

                if (itemsForUpdateChekbox.Add(Parent.FreeHierItem_ID))
                {
                    //Это первый дочерний у своего родителя, сбрасываем счетчик выбранных у родителя
                    //Parent.SelectedChildrenCount = 0;
                }

                if (isSelect) Parent.SelectedChildrenCount++;
                else Parent.SelectedChildrenCount--;
            }
        }

        /// <summary>
        /// Ставим/снимаем галочку с самого объекта
        /// </summary>
        /// <param name="isSelect">Снять/поставить</param>
        /// <param name="isUpdatePropertyAnyway">обновить состояние в даже если оно равно предыдущему</param>
        /// <param name="itemType">тип объекта для выделения</param>
        /// <returns>состояние изменилось удачно</returns>
        private bool SelectHimself(bool isSelect, bool isUpdatePropertyAnyway,
            EnumFreeHierarchyItemType? itemType = null, List<FreeHierarchyTreeItem> selectedItems = null)
        {
            if (_isSelected == isSelect && !isUpdatePropertyAnyway) return false;

            if (!IsSelectableByPermissibleSettings || (itemType.HasValue && FreeHierItemType != itemType.Value)) return false;

            _isSelected = isSelect;

            if (PropertyChanged != null && (Parent == null || Parent.IsExpandedFromVisual))
            {
                if (selectedItems != null)
                {
                    selectedItems.Add(this);
                }
                else
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("IsSelected"));
                }
            }

            #region Работаем с коллекцией выбранных

            if (Descriptor != null)
            {
                if (isSelect)
                {
                    //Добавляем в коллекцию выбранных
                    Descriptor.SelectedItems[FreeHierItem_ID] = this;
                }
                else
                {
                    //Удаляем из коллекции
                    Descriptor.SelectedItems.Remove(FreeHierItem_ID);
                }
            }

            #endregion

            return true;
        }

        internal void SelectChildren(bool isSelect, List<FreeHierarchyTreeItem> itemsForPrepare,
            Dictionary<int, HashSet<int>> itemsByLevForUpdateChekbox,
            bool isRecursive = false,
            EnumFreeHierarchyItemType? itemType = null, 
            bool isUpdatePropertyAnyway = false,
            int currLevel = 0, 
            int? maxlevel = null, List<FreeHierarchyTreeItem> selectedItems = null)
        {
            if (maxlevel.HasValue && maxlevel.Value <= currLevel) return;

            if (_isSelectedChildren != isSelect)
            {
                _isSelectedChildren = isSelect;
            }

            var noSelect = false; //Пока не выбирать дочерние, будут выбраны после загрузки

            if (isSelect
                && !IsChildrenInitializet //но помечено, что они не подгружены
                && itemsForPrepare != null)
            {
                //if (IncludeObjectChildren || DynamicLoadedParent.Contains(FreeHierItemType))
                {
                    itemsForPrepare.Add(this);
                    noSelect = true;
                }
                //else
                //{
                  //  LoadDynamicChildren(isLoadFromServer: true);
                //}
            }

            if (_children != null)
            {
                bool isSomeChildChanged = false;
                //SelectedChildrenCount = 0;

                foreach (var child in _children)
                {
                    if (!noSelect && child.SelectHimself(isSelect, isUpdatePropertyAnyway, itemType, selectedItems))
                    {
                        if (isSelect) SelectedChildrenCount++;
                        else SelectedChildrenCount--;

                        if (!isSomeChildChanged) isSomeChildChanged = true;
                    }

                    if (isRecursive && child.HasChildren)
                    {
                        child.SelectChildren(isSelect, itemsForPrepare, itemsByLevForUpdateChekbox, isRecursive, itemType, 
                            currLevel: currLevel + 1, maxlevel: maxlevel, selectedItems: selectedItems);
                    }
                }

                if (isSomeChildChanged)
                {
                    HashSet<int> itemsForUpdateChekbox;
                    if (!itemsByLevForUpdateChekbox.TryGetValue(HierLevel, out itemsForUpdateChekbox))
                    {
                        itemsForUpdateChekbox = new HashSet<int>();
                        itemsByLevForUpdateChekbox[HierLevel] = itemsForUpdateChekbox;
                    }

                    itemsForUpdateChekbox.Add(FreeHierItem_ID);
                }
            }

            //UpdateParentCheckBoxStyles();
        }

        public void RaiseIsSelectedChanged()
        {
            if (PropertyChanged != null && (Parent == null || Parent.IsExpandedFromVisual))
            {
                PropertyChanged(this, new PropertyChangedEventArgs("IsSelected"));
            }
        }


        #endregion
    }
}