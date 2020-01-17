using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Infragistics.Controls.Menus;
using Proryv.AskueARM2.Both.VisualCompHelpers;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.FreeHierarchyService;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.Visual;
using Proryv.AskueARM2.Client.Visual.Common;
using Proryv.AskueARM2.Client.Visual.Common.FreeHierarchy;
using Proryv.AskueARM2.Client.Visual.Common.GlobalDictionary;
using Proryv.AskueARM2.Client.Visual.Formulas;
using RadarSoft.RadarCube.WPF;
using Action = System.Action;
using DictTariffs_Zone = Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service.DictTariffs_Zone;
using System.Diagnostics;
using Proryv.ElectroARM.Controls.Controls.Tabs;

namespace Proryv.ElectroARM.Controls.Controls.FreeHierarchyTree
{
    /// <summary>
    /// Interaction logic for FreeHierarchyTree_ObjectSelector.xaml
    /// </summary>
    public partial class FreeHierarchyTree_ObjectSelector : UserControl
    {
        private List<enumTypeHierarchy> _selectTypesFilter;
        public IFreeHierarchyObject SelectedItem;
        public string ChannelType;


        /// <summary>
        /// Выбор каналов или объектов из дерева FreeHierarchy 
        /// </summary>
        /// <param name="selectTypesFilter">Можно выбирать только элементы заданных типов</param>
        /// <param name="selectedItem">Текущий выделенный элемент</param>
        /// <param name="channelType">Тип канала выбранного </param>
        /// <param name="channelsIsSelectable">Можно ли выбирать каналы  </param>
        public FreeHierarchyTree_ObjectSelector(List<enumTypeHierarchy> selectTypesFilter = null, IFreeHierarchyObject selectedItem = null, string channelType = null, bool channelsIsSelectable = true)
        {
            _selectTypesFilter = selectTypesFilter;
            SelectedItem = selectedItem;
            ChannelType = channelType;
            ChannelsIsSelectable = channelsIsSelectable;
            this.Loaded += OnLoaded;
            InitializeComponent();

        }

        public static readonly DependencyProperty ChannelsIsSelectableProperty = DependencyProperty.Register(
            "ChannelsIsSelectable", typeof(bool), typeof(FreeHierarchyTree_ObjectSelector), new PropertyMetadata(default(bool)));

        public bool ChannelsIsSelectable
        {
            get { return (bool)GetValue(ChannelsIsSelectableProperty); }
            set { SetValue(ChannelsIsSelectableProperty, value); }
        }

        public EventHandler<FreeHierarchyTree_ObjectSelectorSelectObjectEventArgs> OnSelectObject;

        public class FreeHierarchyTree_ObjectSelectorSelectObjectEventArgs : EventArgs
        {
            /// <summary>
            /// Выбранный объект
            /// </summary>
            public IFreeHierarchyObject SelectedObject { get; set; }

            /// <summary>
            /// Канал если выбран
            /// </summary>
            public string ChannelType { get; set; }

        }

        void SelectObject(FreeHierarchyTree_ObjectSelectorSelectObjectEventArgs obj)
        {
            this.ChannelType = obj.ChannelType;
            this.SelectedItem = obj.SelectedObject;

            if (this.OnSelectObject != null)
            {
                this.OnSelectObject.Invoke(this, obj);
            }

        }



        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (_selectTypesFilter == null || !_selectTypesFilter.Any())
            {
                _selectTypesFilter = Enum.GetValues(typeof(enumTypeHierarchy)).OfType<enumTypeHierarchy>().ToList();
            }

            int treeType = GlobalFreeHierarchyDictionary.TreeTypeStandartPS;
            if (SelectedItem != null)
            {

                switch (SelectedItem.Type)
                {
                    case enumTypeHierarchy.Info_TI:
                        treeType = GlobalFreeHierarchyDictionary.TreeTypeStandartPS;
                        break;
                    case enumTypeHierarchy.Dict_PS:
                        treeType = GlobalFreeHierarchyDictionary.TreeTypeStandartPS;
                        break;
                    case enumTypeHierarchy.Formula:
                        treeType = GlobalFreeHierarchyDictionary.TreeTypeStandartTIFormula;
                        break;
                    case enumTypeHierarchy.Section:
                        treeType = GlobalFreeHierarchyDictionary.TreeTypeStandartSections;

                        break;
                    case enumTypeHierarchy.FormulaConstant:
                        treeType = GlobalFreeHierarchyDictionary.TreeTypeStandartTIFormula;
                        break;
                    case enumTypeHierarchy.Info_TP:
                        treeType = GlobalFreeHierarchyDictionary.TreeTypeStandartGroupTP;
                        break;
                    default:
                        treeType = GlobalFreeHierarchyDictionary.TreeTypeStandartPS;
                        break;
                }

                //TI_tree.WaitLoadTreeAndInit(() => { TI_tree.ExpandAndSelect(SelectedItem, false); });
                ////   TI_tree.ReloadTree(treeType);
                //// TI_tree.ExpandAndSelect(SelectedItem, false);
                ////TI_tree.


                //TI_tree.ExpandAndSelect(SelectedItem, false);






            }
            TI_tree.InitialTree_ID = treeType;
            TI_tree.OnTreeDataLoaded += OnTreeDataLoaded;

            TI_tree.OuterSelector.SelectionTypesFilter = _selectTypesFilter;



            TI_tree.LoadTypes(EnumModuleFilter.ViewCommercialData | EnumModuleFilter.ViewDataSection);
            TI_tree.Hide_SetsManager();



        }

        //private XamDataTreeNode selected;

        private void OnTreeDataLoaded(object sender, EventArgs eventArgs)
        {
            if (TI_tree.OnTreeDataLoaded != null) TI_tree.OnTreeDataLoaded -= OnTreeDataLoaded;
            TI_tree.SelectedChanged += TI_tree_SelectedChanged;
            TI_tree.ExpandAndSelect(SelectedItem, true);
        }

        private void TI_tree_SelectedChanged()
        {

            TI_tree.SelectedChanged -= TI_tree_SelectedChanged;


            //Подсветим канал который выбран
            if (!string.IsNullOrEmpty(ChannelType))
            {

                TI_tree.Dispatcher.BeginInvoke(
                    new Action(delegate
                    {
                        try
                        {
                            if (TI_tree.SelectedNodes.Any())
                            {
                                var feControl = TI_tree.SelectedNodes.FirstOrDefault();

                                if (feControl != null)
                                {
                                    var sp = feControl.Control.FindLogicalChild<StackPanel>();
                                    if (sp != null)
                                    {
                                        var sp2 = sp.Children.OfType<object>().LastOrDefault(m => m is ContentControl) as ContentControl;
                                        if (sp2 != null && sp2.Content != null)
                                        {
                                            var sp3 = sp2.Content as StackPanel;
                                            if (sp3 != null)
                                            {
                                                var sp4 =
                                                    sp3.Children.OfType<object>()
                                                        .LastOrDefault(m => m is Grid) as Grid;
                                                if (sp4 != null)
                                                {
                                                    var sp5 =
                                                        sp4.Children.OfType<object>()
                                                            .LastOrDefault(m => m is StackPanel) as StackPanel;
                                                    if (sp5 != null)
                                                        foreach (var bChild in sp5.Children.OfType<Button>())
                                                        {
                                                            if (bChild.Tag != null && bChild.Tag.ToString() == ChannelType)
                                                            {
                                                                // bChild.Background = new SolidColorBrush() { Color = Colors.Yellow };
                                                                //bChild.Foreground = new SolidColorBrush() { Color = Colors.Yellow };
                                                                var index = sp5.Children.IndexOf(bChild);
                                                                sp5.Children.Remove(bChild);
                                                                sp5.Children.Insert(index, new Border()
                                                                {
                                                                    Child = bChild,
                                                                    BorderBrush = new SolidColorBrush() { Color = Colors.Yellow },
                                                                    BorderThickness = new Thickness(5)
                                                                });
                                                                break;
                                                                //bChild.BorderThickness = new Thickness(5);
                                                                // bChild.Visibility = Visibility.Collapsed;
                                                            }
                                                        }
                                                }
                                            }

                                        }
                                    }
                                    //foreach (var button in buttons)
                                    //{
                                    //    if (button.Tag != null && button.Tag.ToString() == ChannelType)
                                    //    {
                                    //        button.BorderBrush = new SolidColorBrush() { Color = Colors.Yellow };
                                    //    }
                                    //}
                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                        }


                    }), DispatcherPriority.Loaded);





            }

        }

        protected RoutedEventHandler itemExpanded;
        protected void OnTreeItemExpanded(object sender, RoutedEventArgs e)
        {
            var tvi = e.OriginalSource as TreeViewItem;
            //var item = tvi.DataContext as SelectableTreeItem;
            //item.Expanded(tvi, TI_tree);
        }







        private void butTI_Add_Click(object sender, RoutedEventArgs e)
        {
            var but = sender as Button;
            var treeItem = but.DataContext as FreeHierarchyTreeItem;
            if (treeItem == null) return;

            var obj = treeItem.Item as IFreeHierarchyObject;
            if (obj != null)
            {
                if (but.Tag != null)
                    SelectObject(new FreeHierarchyTree_ObjectSelectorSelectObjectEventArgs()
                    {
                        ChannelType = but.Tag.ToString(),
                        SelectedObject = obj
                    });
                else
                {
                    SelectObject(new FreeHierarchyTree_ObjectSelectorSelectObjectEventArgs()
                    {
                        ChannelType = string.Empty,
                        SelectedObject = obj
                    });
                }


            }
        }

        private void butContr_Add_Click(object sender, RoutedEventArgs e)
        {
            var but = sender as Button;
            var obj = (but.DataContext as CA_TreeItem).Item as IFreeHierarchyObject;
            if (obj != null)
            {

                SelectObject(new FreeHierarchyTree_ObjectSelectorSelectObjectEventArgs()
                {
                    ChannelType = but.Tag.ToString(),
                    SelectedObject = obj
                });

            }
        }

        private void butSection_Add_Click(object sender, RoutedEventArgs e)
        {
            var but = sender as FrameworkElement;

            //Section
            IFreeHierarchyObject obj = but.DataContext as IFreeHierarchyObject;

            var treeItem = but.DataContext as FreeHierarchyTreeItem;

            if (treeItem == null && obj == null) return;

            if (obj == null)
                obj = treeItem.Item as IFreeHierarchyObject;
            if (obj != null)
            {
                SelectObject(new FreeHierarchyTree_ObjectSelectorSelectObjectEventArgs()
                {
                    ChannelType = but.Tag.ToString(),
                    SelectedObject = obj
                });

            }
        }

        private void butFormula_Add_Click(object sender, RoutedEventArgs e)
        {
            var formula = (sender as Button).DataContext as FreeHierarchyTreeItem;

            if (formula != null)
            {

                var fh = formula.Item as IFreeHierarchyObject;
                if (fh != null)
                    SelectObject(new FreeHierarchyTree_ObjectSelectorSelectObjectEventArgs()
                    {
                        //ChannelType = Convert.ToByte(but.Tag),
                        SelectedObject = fh
                    });

            }
        }



        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (itemExpanded == null)
            {
                itemExpanded = OnTreeItemExpanded;
                TI_tree.AddHandler(TreeViewItem.ExpandedEvent, itemExpanded);
            }

        }

        protected void setChannelVisible(object sender)
        {
            var fe = sender as FrameworkElement;
            var ti = fe.DataContext as SelectableTreeItem;
            if (ti == null) return;

            var isVis = Visibility.Visible;
            var t = EnumClientServiceDictionary.TIHierarchyList[ti.Item.Id];
            if (t != null)
            {
                var mask = t.AbsentChannelsMask;
                if (mask != null)
                {
                    var ch = Convert.ToByte(fe.Tag) - 1;
                    if (((1 << ch) | mask.Value) == mask.Value) isVis = Visibility.Collapsed;
                }
            }
            fe.Visibility = isVis;
        }

        protected void TIButton_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            setChannelVisible(sender);
        }

        protected void TIButton_Loaded(object sender, RoutedEventArgs e)
        {
            setChannelVisible(sender);
        }



        private void butDelClick(object sender, RoutedEventArgs e)
        {
            //if (current == null) return;

            //current.Item = null;
            itemExpanded = null;

        }

        public void Dispose()
        {
            TI_tree.Dispose();
        }


        private void btn_SelectTechParamsClick(object sender, RoutedEventArgs e)
        {
            var module = VisualEx.FindParent<IModalCompleted>(this);
            var uc = module as UserControl;
            //if (uc == null) return;

            Manager.UI.CloseLocalModal(() => { }, this);
            var tiId = ((sender as Button).DataContext as FreeHierarchyTreeItem).HierObject.Id;
            var tiFh = ((sender as Button).DataContext as FreeHierarchyTreeItem).HierObject;
            var d = new TechQualityParams(false, tiId, tiFh);
            d.OnSelectObject += OnSelectObject;
            Manager.UI.ShowLocalModal(d, "Выберите тип мгновенного", this.Parent as FrameworkElement);
        }

    }
}
