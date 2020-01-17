using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using Infragistics.Controls.Menus;
using Infragistics.Windows.DataPresenter;
using Infragistics.Windows.DataPresenter.Events;
using Proryv.AskueARM2.Client.ServiceReference.FreeHierarchyService;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.Visual.Common.FreeHierarchy;
using Proryv.ElectroARM.Controls.Controls.Dialog.Primitives;
using Proryv.ElectroARM.Controls.Controls.Tabs;
using Proryv.AskueARM2.Server.DBAccess.Public.Common;
using Xceed.Wpf.DataGrid;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Proryv.AskueARM2.Client.Visual.Common;
using Xceed.Wpf.DataGrid.Views;
using System.Collections;
using Proryv.AskueARM2.Client.Visual.Common.Configuration;
using ActiproSoftware.Windows.Controls.Docking;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using ActiproSoftware.Windows.Controls.Docking.Serialization;
using System.Windows.Threading;
using System.Globalization;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.Visual.Common.GlobalDictionary;
using System.Windows.Data;
using Proryv.AskueARM2.Client.Visual.DataTariff;
using System.Reflection;
using Northwoods.GoXam;
using System.Threading;
using Microsoft.Win32;
using Proryv.AskueARM2.Both.VisualCompHelpers;
using ActiproSoftware.Products.Docking;
using Infragistics.Windows;
using Infragistics.Windows.OutlookBar;
using Proryv.AskueARM2.Client.ServiceReference.Common;
using Proryv.ElectroARM.Controls.Common;
using Proryv.ElectroARM.Controls.Controls.FreeHierarchyTree;
using Proryv.ElectroARM.Controls.Controls.Menu;
using Action = System.Action;
using CompressUtility = Proryv.AskueARM2.Both.VisualCompHelpers.CompressUtility;
using InfoBit_Abonents_List = Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service.InfoBit_Abonents_List;
using Proryv.ElectroARM.Controls.Controls.FreeHierarchyTree.TreeSelector;
using Proryv.ElectroARM.Controls.Controls.Orgstructure;
using EnumUnitDigit = Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service.EnumUnitDigit;

namespace Proryv.AskueARM2.Client.Visual
{
    /// <summary>
    /// Расширения для визуалки
    /// </summary>
    public static class VisualEx
    {
        public static void Init()
        {
            SR.SetCustomString(SRName.UICommandMakeFloatingWindowText.ToString(), "Плавающее");
            //SR.SetCustomString(SRName.UICommandAutoHideWindowText.ToString(), "Спрятать"); начиная с версии 2011.2 отсутствует
            SR.SetCustomString(SRName.UICommandCloseWindowText.ToString(), "Закрыть");
            SR.SetCustomString(SRName.UICommandMakeDockedWindowText.ToString(), "Закрепить");
            SR.SetCustomString(SRName.UICommandMakeDocumentWindowText.ToString(), "Документ");
            SR.SetCustomString(SRName.UICommandOpenOptionsMenuText.ToString(), "Опции");
            SR.SetCustomString(SRName.UICommandToggleWindowAutoHideStateText.ToString(), "Показать/Спрятать");
            SR.SetCustomString(SRName.UITabbedMdiContainerCloseButtonToolTip.ToString(), "Закрыть");
            SR.SetCustomString(SRName.UIToolWindowContainerAutoHideButtonToolTip.ToString(), "Показать/Спрятать");
            SR.SetCustomString(SRName.UIToolWindowContainerOptionsButtonToolTip.ToString(), "Опции");
            SR.SetCustomString(SRName.UICommandMoveToNewHorizontalContainerText.ToString(), "Создать горизонтальную панель");
            SR.SetCustomString(SRName.UICommandMoveToNewVerticalContainerText.ToString(), "Создать вертикальную панель");
            SR.SetCustomString(SRName.UICommandMoveToNextContainerText.ToString(), "Переместить в следующую панель");
            SR.SetCustomString(SRName.UICommandMoveToPreviousContainerText.ToString(), "Переместить в предыдущую панель");
            SR.SetCustomString(SRName.UITabbedMdiContainerDocumentsButtonToolTip.ToString(), "Активные файлы");
            try
            {
                Licenser.LicenseKey = Xceed.Wpf.DataGrid.Licenser.LicenseKey = "DGP60PE4ATRBAEK6BBA";
                //"DGP599M4YTTUK1N3BCA";
                //"DGP565F4DTTBKEA3BBA";
                //"DGP559M4NBZAAEA3BCA";
                //"DGP539M4ATGK41Y5BCA";
                //"DGP52P1WATRAKMK9BJA";
                //"DGP512F4PB74XMD2BJA";
                //"DGP50JMWYTBWG1J5U2A";//"DGP45C14ATZUX7A2UXA";//"DGP43LM4PTUTKWP0NCA";//"DGP42PEWPTT7BUP385A";
                //более не используется Xceed.Silverlight.ListBox.Licenser.LicenseKey = "LBW20N141BHK7J5LB3A";
            }
            catch
            {
            }
            Northwoods.GoXam.Diagram.LicenseKey = "1";
            ChartButton.Helper = new ChartHelper();

            //pack://application:,,,/System.Activities.Presentation;V4.0.0.0;31bf3856ad364e35;component/themes/icons.xaml
            // стандартные иконки для Workflow            
            System.Action resourceAction = () =>
                                           {
                                               var dict = new ResourceDictionary { Source = new Uri("pack://application:,,,/System.Activities.Presentation;component/themes/icons.xaml") };
                                               var dict2 = new ResourceDictionary();
                                               foreach (var d in dict.Keys)
                                                   dict2[d.ToString().ToLower()] = dict[d];
                                               Application.Current.Resources.MergedDictionaries.Add(dict2);
                                           };

#if !DEBUG
            try
            {

                resourceAction();
            }
            catch (Exception)
            {

            }

#endif

            try
            {
                // для повышения производительности WorkFlow
                Registry.SetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\.NETFramework\\v4.0.0.0\\System.Activities.Presentation",
                    "DisableValidateOnModelItemChanged", 1);
            }
            catch (Exception)
            {
                // возможен вылет из за отсутствия прав
            }
        }


        #region Методы расширения

        /// <summary>
        /// Найти родителя в дереве UI по его типу
        /// </summary>
        /// <param name="element">Элемент, относительно которого искать</param>
        /// <returns>Найденный родитель или NULL</returns>
        public static T FindParent<T>(this FrameworkElement element, bool useHimself = true) where T : class
        {
            if (element == null) return null;

            if (useHimself && element is T) return element as T;

            do
            {
                var temp = element.Parent as FrameworkElement;
                if (temp == null)
                {
                    temp = element.TemplatedParent as FrameworkElement;
                    if (temp == null)
                    {
                        if (element is TreeViewItem) temp = System.Windows.Controls.TreeView.ItemsControlFromItemContainer(element);
                        if (element is ListViewItem) temp = ListView.ItemsControlFromItemContainer(element);
                        if (element is DataCell) temp = DataGridControl.GetDataGridContext(element as DataCell).DataGridControl;
                        if (element is ColumnManagerCell) temp = DataGridControl.GetDataGridContext(element as ColumnManagerCell).DataGridControl;
                        if (element is Node) temp = (element as Node).Panel as FrameworkElement;

                        if (temp == null)
                        {
                            temp = VisualTreeHelper.GetParent(element) as FrameworkElement;
                        }
                    }
                }
                element = temp;
            } while ((element != null) && (!(element is T)));
            return element as T;
        }

        /// <summary>
        /// Расширение для совместимости
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="element"></param>
        /// <returns></returns>
        public static T FindParrent<T>(FrameworkElement element) where T : class
        {
            return element.FindParent<T>();
        }


        /// <summary>
        /// Найти близжайший элемент в визуальном дереве к destination типа T 
        /// </summary>
        /// <typeparam name="T">тип искомого элемента</typeparam>
        /// <param name="element">откуда начинаем искать</param>
        /// <param name="destination">до куда искать</param>
        /// <param name="closerObj"></param>
        /// <returns></returns>
        public static T FindCloserVisualChild<T>(this DependencyObject element, object destination, T closerObj = null) where T : class
        {

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var item = VisualTreeHelper.GetChild(element, i);
                if (item is T) closerObj = item as T;
                if (item.Equals(destination)) return closerObj as T;
                if (item != null)
                {
                    var found = FindCloserVisualChild<T>(item, destination, closerObj);
                    if (found != null) return found;
                }
            }
            return null;
        }
        public static T FindCloserLogicalChild<T>(this DependencyObject element, object destination, T closerObj = null) where T : class
        {

            foreach (var item in LogicalTreeHelper.GetChildren(element))
            {
                if (item is T) closerObj = item as T;
                if (item.Equals(destination)) return closerObj as T;
                if (item != null)
                {
                    if (item as DependencyObject != null)
                    {
                        var found = FindCloserLogicalChild<T>(item as DependencyObject, destination, closerObj);
                        if (found != null) return found;
                    }

                }
            }


            return null;
        }
        /// <summary>
        /// Получить скриншот элемента
        /// </summary>
        /// <param name="element">Элемент</param>
        /// <param name="width">Ширина скриншота</param>
        /// <param name="height">Высота скриншота</param>
        /// <returns>Image со скриншотом</returns>
        public static RenderTargetBitmap GetPreview(this FrameworkElement element, int width, int height)
        {
            if (element == null) return null;

            var bmp = new RenderTargetBitmap((int)element.ActualWidth, (int)element.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(element);

            var drawingVisual = new DrawingVisual();

            using (var drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawImage(bmp, new Rect(0, 0, width, height));
            }

            var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(drawingVisual);
            return rtb;
        }

        /// <summary>
        /// Запустить анимацию для элемента
        /// </summary>
        /// <param name="element">Элемент</param>
        /// <param name="resourceName">Имя анимации</param>
        public static void StartStoryboard(this FrameworkElement element, string resourceName)
        {
            var sb = element.Resources[resourceName] as Storyboard;
            if (sb == null) return;

            try
            {
                sb.Begin();
            }
            catch
            {
            }
        }

        /// <summary>
        /// Запустить анимацию для элемента (с выполнением метода после ее завершения)
        /// </summary>
        /// <param name="element">Элемент</param>
        /// <param name="resourceName">Имя анимации</param>
        /// <param name="completeMethod">Метод, который выполнится после завершения анимации</param>
        public static void StartStoryboard(this FrameworkElement element, string resourceName, Action completeMethod)
        {
            if (element != null)
            {
                var sb = element.Resources[resourceName] as Storyboard;
                if (sb == null)
                {
                    Manager.UI.ShowMessage(resourceName + " не найдено!");
                    return;
                }

                EventHandler eh = null;
                eh = new EventHandler(delegate (object sender, EventArgs args)
                                      {
                                          if (completeMethod != null) completeMethod();
                                          sb.Completed -= eh;
                                      });
                sb.Completed += eh;
                element.BeginStoryboard(sb);
            }
        }

        /// <summary>
        /// Найти дочерний элемент в логическом дереве элемента
        /// </summary>
        /// <param name="element">Элемент</param>
        /// <param name="name">Имя искомого дочернего элемента</param>z
        /// <returns>Найденный элемент или NULL</returns>
        public static FrameworkElement FindLogicalChild(this FrameworkElement element, string name)
        {
            if (element == null) return null;

            var elements = new List<FrameworkElement>();

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var item = VisualTreeHelper.GetChild(element, i) as FrameworkElement;
                if (item != null)
                {
                    if (item.Name == name) return item;
                    elements.Add(item);
                }
            }

            if (elements.Count == 0) return null;
            FrameworkElement found = null;

            elements.ForEach(item =>
            {
                found = FindLogicalChild(item, name);
                if (found != null) return;
            });

            return found;
        }



        /// <summary>
        /// Найти дочерний элемент в логическом дереве элемента
        /// </summary>
        /// <typeparam name="T">Тип дочернего элемента</typeparam>
        /// <param name="element">Элемент</param>
        /// <returns>Найденный элемент или NULL</returns>
        public static T FindLogicalChild<T>(this DependencyObject element) where T : class
        {
            if (element == null) return null;

            var elements = new List<DependencyObject>();

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var item = VisualTreeHelper.GetChild(element, i);
                if (item is T) return item as T;
                if (item != null) elements.Add(item);
            }

            if (elements.Count == 0) return null;
            T found = null;

            foreach (var item in elements)
            {
                var data = FindLogicalChild<T>(item);
                if (data != null)
                    found = data;
            }

            return found;

            //for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            //{
            //    var item = VisualTreeHelper.GetChild(element, i);
            //    if (item is T) return item as T;
            //    if (item != null)
            //    {
            //        var found = FindLogicalChild<T>(item);
            //        if (found != null) return found;
            //    }
            //}
            //return null;
        }

        /// <summary>
        /// Создает копию детальной конфигурации XCeed Grid
        /// </summary>
        /// <param name="source">Исходная детальная конфигурация</param>
        /// <returns>Клонированная детальная конфигурация</returns>
        public static DetailConfiguration Clone(this DetailConfiguration source)
        {
            var dc = new DetailConfiguration();
            dc.Title = source.Title;
            dc.RelationName = source.RelationName;
            dc.AutoCreateColumns = source.AutoCreateColumns;
            dc.AutoCreateDetailConfigurations = source.AutoCreateDetailConfigurations;
            dc.Visible = source.Visible;
            int fcc = TableView.GetFixedColumnCount(source);
            if (fcc > 0) TableView.SetFixedColumnCount(dc, fcc);
            dc.UseDefaultHeadersFooters = source.UseDefaultHeadersFooters;
            foreach (DataTemplate dt in source.Headers) dc.Headers.Add(dt);
            foreach (var col in source.Columns)
            {
                ColumnBase newcol = null;
                if (col is Column) newcol = new Column();
                if (col is UnboundColumn) newcol = new UnboundColumn();
                newcol.Width = col.Width;
                newcol.FieldName = col.FieldName;
                newcol.Title = col.Title;
                newcol.ReadOnly = col.ReadOnly;
                newcol.CellContentTemplate = col.CellContentTemplate;
                newcol.TitleTemplate = col.TitleTemplate;
                newcol.CellContentTemplateSelector = col.CellContentTemplateSelector;
                newcol.CellEditor = col.CellEditor;
                newcol.CellEditorDisplayConditions = col.CellEditorDisplayConditions;
                newcol.VisiblePosition = col.VisiblePosition;
                newcol.Visible = col.Visible;
                if (col is Column)
                {
                    newcol.AllowAutoFilter = col.AllowAutoFilter;
                    newcol.AllowGroup = col.AllowGroup;
                    newcol.AllowSort = col.AllowSort;
                    newcol.GroupValueTemplate = col.GroupValueTemplate;
                }
                dc.Columns.Add(newcol);
            }
            foreach (var child in source.DetailConfigurations)
                dc.DetailConfigurations.Add(child.Clone());
            return dc;
        }

        /// <summary>
        /// Создает копию описателя детальной конфигурации
        /// </summary>
        /// <param name="source">Исходный описатель</param>
        /// <returns>Клонированный описатель</returns>
        public static PropertyDetailDescription Clone(this PropertyDetailDescription source)
        {
            var dd = new PropertyDetailDescription();
            dd.RelationName = source.RelationName;
            dd.AutoFilterMode = source.AutoFilterMode;
            dd.DefaultCalculateDistinctValues = source.DefaultCalculateDistinctValues;
            dd.DistinctValuesConstraint = source.DistinctValuesConstraint;
            foreach (PropertyDetailDescription child in source.DetailDescriptions)
                dd.DetailDescriptions.Add(child.Clone());
            return dd;
        }

        /// <summary>
        /// Открыть и зарегистрировать Popup
        /// </summary>
        /// <param name="popup">Popup</param>
        /// <param name="closeAll">Закрывать ли при этом все остальные Popup'ы</param>
        public static void OpenAndRegister(this Popup popup, bool closeAll)
        {
            PopupBase.CloseAllPopups();
            if (closeAll) Manager.UI.CloseAllPopups();
            if (!popup.IsOpen) popup.IsOpen = true;
            lock (WorkPage.popups)
            {
                WorkPage.popups.Add(popup);
            }
        }

        public static void ClosePopup(this FrameworkElement element)
        {
            var parent = element.FindParent<Popup>();
            if (parent != null) parent.IsOpen = false;
        }

        /// <summary>
        /// Открыть локальный модальный диалог в модуле
        /// </summary>
        /// <param name="module">Модуль</param>
        /// <param name="content">Содержимое модального диалога</param>
        /// <param name="title">Заголовок модального окна</param>
        /// <param name="isOpenAsSingleModal">Открывать как единственно допустимое модальное окно</param>
        /// <param name="isNotCloseOnEscapeButton">Не закрывать окно по клавише Escape</param>
        public static void ShowLocalModal(this FrameworkElement module, FrameworkElement content, string title, bool isOpenAsSingleModal = false, bool isNotCloseOnEscapeButton = false)
        {
            if (content == null) return;

            var topPanel = module.FindName("topPanel") as ContentControl;
            if (topPanel == null)
            {
                Manager.UI.ShowGlobalModal(content, title, isOpenAsSingleModal, isNotCloseOnEscapeButton);
            }
            else
            {
                PopupBase.CloseAllPopups();
                Manager.UI.CloseAllPopups();
                var panel = topPanel.Parent as Panel;
                foreach (FrameworkElement child in panel.Children)
                    if ((child != topPanel) && (child.Name != "topMessage") && (child.Name != "topWaiter")) (child as FrameworkElement).IsEnabled = false;
                topPanel.Content = new ModalDialog(false, isOpenAsSingleModal) { DialogContent = content, Caption = title, IsNotCloseOnEscapeButton = isNotCloseOnEscapeButton };
            }
        }

        /// <summary>
        /// Открыть локальный модальный диалог в модуле
        /// </summary>
        /// <param name="module">Модуль</param>
        /// <param name="content">Содержимое модального диалога</param>
        /// <param name="title">Заголовок модального окна</param>
        public static void ShowModal(this FrameworkElement module, FrameworkElement content, string title)
        {
            ContentControl topPanel;

            var managerContainer = module as UIManagerContainer;
            if (managerContainer != null)
            {
                topPanel = WaitPanel.FindAndAddContentControl(managerContainer);
            }
            else
            {
                topPanel = module.FindName("topPanel") as ContentControl ?? module.FindName("topWaiter") as ContentControl;
                if (topPanel != null) PopupBase.CloseAllPopups();
            }

            if (topPanel == null) return;

            var panel = topPanel.Parent as Panel;
            if (panel != null)
            {
                foreach (FrameworkElement child in panel.Children)
                {
                    if (child != topPanel && child.Name != "topMessage" && child.Name != "topWaiter")
                    {
                        child.IsEnabled = false;
                    }
                }
            }

            topPanel.Content = new ModalDialog(content, title);
        }

        /// <summary>
        /// Открыть локальный модальный диалог в модуле
        /// </summary>
        /// <param name="module">Модуль</param>
        /// <param name="message">Содержимое модального диалога</param>
        /// <param name="yesMethod">Событи на нажатие кнопки да</param>
        public static void ShowYesNoModal(this FrameworkElement module, string message, Action yesMethod)
        {
            var content = new YesNoDialog(message, yesMethod);
            ShowModal(module, content, "");
        }

        public static void CloseXamModal(this FrameworkElement module, params object[] args)
        {
            var parrentFrame = module.FindParent<UIManagerContainer>();
            if (parrentFrame == null) return;

            //var resultableContent = module.FindParent<IModalCompleted>() as IModalResult;
            //var args = resultableContent != null ? resultableContent.GetResult() : null;

            var modalCompleted = module.FindParent<IModalCompleted>();
            if (modalCompleted != null) modalCompleted.OnClosedModalAction(args);

            parrentFrame.CloseLocalModal(null, module);
        }


        public static void CloseModal(this FrameworkElement dialog, params object[] args)
        {
            CloseModal(dialog, null, args);
        }

        public static void CloseModal(this FrameworkElement dialog, Action onClose, params object[] args)
        {
            var md = dialog.FindParent<ModalDialog>();
            if (md == null) return;

            var topPanel = md.Parent as ContentControl;
            if (topPanel == null) return;

            (topPanel.Content as FrameworkElement)
                .StartStoryboard("FadeOutAnimation", delegate
                {
                    if (onClose != null) onClose();

                    var panel = topPanel.Parent as Panel;
                    if (panel == null) return;

                    foreach (FrameworkElement child in panel.Children)
                    {
                        if (child.Name != "topMessage" && child.Name != "topWaiter")
                            child.IsEnabled = true;
                    }

                    var modalCompleted = topPanel.FindParent<IModalCompleted>();
                    if (modalCompleted != null) modalCompleted.OnClosedModalAction(args);

                    md.Dispose();
                    topPanel.Content = null;
                });
        }

        /// <summary>
        /// Закрыть локальный модальный диалог
        /// </summary>
        /// <param name="dialog">Диалог</param>
        /// <param name="completeMethod">Метод, который необходимо выполнить после закрытия диалога</param>
        public static void CloseLocalModal(this FrameworkElement dialog, Action completeMethod)
        {
            try
            {
                var fe = dialog.FindTrueIModule();
                if (fe == null)
                {
                    Manager.UI.CloseGlobalModal(completeMethod);
                    return;
                }

                var topPanel = fe.FindName("topPanel") as ContentControl;
                if (topPanel == null || topPanel.Content == null)
                {
                    var uiContainer = dialog as UIManagerContainer;
                    if (uiContainer != null)
                    {
                        uiContainer.CloseLocalModal(completeMethod, null);
                        return;
                    }

                    Manager.UI.CloseGlobalModal(completeMethod);
                    return;
                }

                var md = dialog.FindParent<ModalDialog>();
                if (md != null)
                {
                    var cc = md.Parent as ContentControl;
                    if (cc != null)
                    {
                        if (cc.Name == "topPanel" && cc != topPanel) topPanel = cc;
                    }
                }

                (topPanel.Content as FrameworkElement).StartStoryboard("ExitWarningPanelAnimation",
                    delegate ()
                    {
                        var panel = topPanel.Parent as Panel;

                        foreach (FrameworkElement child in panel.Children)
                            if (!Equals(child, topPanel) && child.Name != "topMessage" &&
                                child.Name != "topWaiter")
                                child.IsEnabled = true;

                        var d = topPanel.Content as IDisposable;
                        topPanel.Content = null;

                        if (completeMethod != null)
                        {
                            Application.Current.Dispatcher.Invoke(completeMethod);
                        }

                        if (d != null) d.Dispose();
                    });

            }
            catch (Exception)
            {
            }
        }

        public static void ExpandAndSelect(this ItemContainerGenerator generator, Stack chain)
        {
            generator.ExpandAndSelect(chain, false);
        }

        /// <summary>
        /// Раскрыть и выделить в дереве объект
        /// </summary>
        /// <param name="generator">Генератор элементов дерева 1-ого уровня</param>
        /// <param name="chain">Путь искомого объекта (в обратном порядке)</param>
        /// <param name="isExpandLast">Раскрывать последнюю найденную ноду или нет</param>
        public static void ExpandAndSelect(this ItemContainerGenerator generator, Stack chain, bool isExpandLast)
        {
            EventHandler stChanged = null;
            Action expandAndSelect = delegate
                                     {
                                         var item = chain.Pop();
                                         var tvi = generator.ContainerFromItem(item) as TreeViewItem;
                                         if (tvi == null)
                                         {
                                             return;
                                         }
                                         if (chain.Count == 0)
                                         {
                                             tvi.IsSelected = true;
                                             tvi.BringIntoView();
                                             if (isExpandLast) tvi.IsExpanded = true;
                                             return;
                                         }
                                         tvi.ItemContainerGenerator.ExpandAndSelect(chain, isExpandLast);
                                         if (!tvi.IsExpanded) tvi.IsExpanded = true;
                                     };
            stChanged = new EventHandler(delegate
                                         {
                                             if (generator.Status == GeneratorStatus.ContainersGenerated)
                                             {
                                                 generator.StatusChanged -= stChanged;
                                                 expandAndSelect();
                                             }
                                         });
            if (generator.Status != GeneratorStatus.ContainersGenerated)
                generator.StatusChanged += stChanged;
            else expandAndSelect();
        }

        public static void ExpandAndSelectJuridical(this ItemsControl tree, object ti_obj)
        {
            TInfo_TI ti_item = ti_obj as TInfo_TI;
            if (ti_item == null)
            {
                IKey ti_key = ti_obj as IKey;
                if (ti_key != null)
                {
                    int id;
                    if (!int.TryParse(ti_key.GetKey, out id) || !EnumClientServiceDictionary.TIHierarchyList.TryGetValue(id, out ti_item, onException: Manager.UI.ShowMessage))
                    {
                        return; //Ошибка, такого быть не должно, но чтобы не падало выйдем
                    }
                }
            }
        }


        public static void ExpandAndSelectTI(this ItemsControl tree, object ti_obj)
        {
            if (ti_obj != null)
            {
                TInfo_TI ti_item = null;
                if (ti_obj is TInfo_TI)
                {
                    ti_item = ti_obj as TInfo_TI;
                }
                if (ti_obj is THierarchyDbObject)
                {
                    EnumClientServiceDictionary.TIHierarchyList.TryGetValue((ti_obj as THierarchyDbObject).Id, out ti_item, onException: Manager.UI.ShowMessage);
                }

                if (ti_item != null)
                {
                    TPSHierarchy ps;

                    if (EnumClientServiceDictionary.DetailPSList.TryGetValue(ti_item.PS_ID, out ps, onException: Manager.UI.ShowMessage) && ps != null)
                    {
                        var psFounded = new Dictionary<object, Stack>();

                        FindBar.scanNode(psFounded, tree.ItemsSource, ps.HierarchyObject, new Stack(), false, isFindOnlySingle:true);
                        ContinueExpand(ti_item, psFounded, tree);
                    }
                }
            }
        }

        private static void ContinueExpand(TInfo_TI ti_item, Dictionary<object, Stack> psFounded, ItemsControl tree)
        {
            FindBar.TItoSelect = ti_item;
            if (psFounded.Count == 0) return;

            FindBar.PSFound = psFounded.Values.First();

            var psObject = FindBar.PSFound.ToArray().LastOrDefault();

            if (psObject is KeyValuePair<ID_TypeHierarchy, DataTariffTreeItem> && ti_item != null)
            {
                DataTariffTreeItem psItem = ((KeyValuePair<ID_TypeHierarchy, DataTariffTreeItem>)psObject).Value;

                Action<Stack> after = (s) =>
                                      {
                                          s.Push(psItem.Children.FirstOrDefault(t => t.Key.ID == ti_item.TI_ID));
                                          s = new Stack(s);
                                          tree.ItemContainerGenerator.ExpandAndSelect(s, true);
                                      };

                Stack st = new Stack(FindBar.PSFound.Clone() as Stack);
                if (!psItem.VerifyInitializedTI())
                {
                    tree.RunAsync(psItem.LoadTIforPS, delegate
                                                      {
                                                          psItem.UpdateChildrenProperty();
                                                          after(st);
                                                      });
                }
                else
                {
                    after(st);
                }

            }
            else
            {
                var psItem = psObject as IFindableItem;
                IFindableItem ti = null;
                foreach (var i in psItem.GetChildren())
                {
                    ti = i as IFindableItem;
                    break;
                }
                if ((ti != null) && (ti.GetItemForSearch() != FindBar.DummyTI)) FindBar.SelectTI(tree as System.Windows.Controls.TreeView, psItem);
                else tree.ItemContainerGenerator.ExpandAndSelect(FindBar.PSFound.Clone() as Stack, true);
            }
        }

        
        public static CancellationTokenSource ShowWaitPanel(FrameworkElement element)
        {
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            Task.Factory.StartNew(arg =>
            {
                Thread.Sleep(38);

                var t = (CancellationToken) arg;

                try
                {
                    if (t.IsCancellationRequested) return;

                    WaitPanel.Show(element, isHideElement: false, isDisableElement: true);
                }
                catch
                {
                }
            }, token, token);

            return tokenSource;
        }

        public static void ScrollNodeIntoView(this UIElement itemsControl, UIElement item)
        {
            if (item == null) return;

            var pos = item.TranslatePoint(new Point(0, 0), itemsControl);

            var sv = itemsControl.FindLogicalChild<ScrollViewer>();// VisualTreeHelper.GetChild(itemsControl, 0) as ScrollViewer;
            if (sv != null)
            {
                double vo = sv.VerticalOffset + pos.Y - sv.ViewportHeight / 2;
                sv.ScrollToVerticalOffset(vo);
            }
        }

        /// <summary>
        /// В одном потоке разворачиваем объекты (без подгрузки), возвращаем выбранный объект
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="chain"></param>
        /// <param name="isExpandLast"></param>
        /// <param name="isSelect"></param>
        /// <returns></returns>
        public static XamDataTreeNode ExpandAndSelectXamTreeSync(this XamDataTree tree, ConcurrentStack<object> chain, bool isExpandLast, bool isSelect = true)
        {
            XamDataTreeNode result = null;
            tree.SelectionSettings.SelectedNodes.Clear();
            Action<XamDataTreeNodesCollection> expandAndSelect = null;
            expandAndSelect = delegate (XamDataTreeNodesCollection list)
            {
                object item;
                if (!chain.TryPop(out item)) return;
                var find = list.FirstOrDefault(n => Equals(n.Data, item));
                if (find == null)
                {
                    return;
                }

                if (chain.Count == 0)
                {
                    if (isExpandLast) find.IsExpanded = true;
                    result = find;
                    if (isSelect)
                    {
                        find.IsSelected = true;
                    }
                    tree.SelectionSettings.SelectedNodes.Add(find);
                    tree.ScrollNodeIntoView(find);
                }
                else
                {
                    find.IsExpanded = true;
                    expandAndSelect(find.Nodes);
                }
            };
            expandAndSelect(tree.Nodes);
            return result;
        }

        
        public static string PackString(this object x)
        {
            if (x == null) return "NULL";
            if (x is string)
            {
                if (x.ToString() == "") return "NULL";
            }
            return x.ToString();
        }

        public static void RunAsync(this FrameworkElement element, System.Action startMethod, System.Action completeMethod)
        {
            var async = new AsyncOperation(startMethod, completeMethod, element);
            async.Start();
        }

        public static void RunAsync<T>(this FrameworkElement element, RunAsyncDelegate<T> startMethod, System.Action<T> completeMethod, params object[] arg) where T : class
        {
            AsyncFunctions.RunAsync(startMethod, completeMethod, element, arg); //WorkPage.CurrentPage
        }

        public static Task RunAsync<T>(this FrameworkElement element, Func<T> startMethod, System.Action<T> completeMethod, CancellationToken cancellationToken = default(CancellationToken),
            TaskCreationOptions creationOptions = TaskCreationOptions.None, DispatcherPriority priority = DispatcherPriority.Normal)
            where T : class
        {
            return AsyncFunctions.RunAsync(startMethod, completeMethod, element, cancellationToken, creationOptions, priority);
        }

        public static Task RunAsync(this FrameworkElement element, Action startMethod)
        {
            return AsyncFunctions.RunAsync(startMethod, element);
        }

        public static void SaveTreeItemSelected(this System.Windows.Controls.TreeView tree, ref string shortcut)
        {
            if (tree.SelectedItem != null)
            {
                object i = null;
                if (tree.SelectedItem is KeyValuePair<ID_TypeHierarchy, DataSectionTreeItem>)
                {
                    var ds = (KeyValuePair<ID_TypeHierarchy, DataSectionTreeItem>)tree.SelectedItem;
                    i = ds.Value.GetItemForSearch();
                }
                else
                {
                    if (tree.SelectedItem is KeyValuePair<ID_TypeHierarchy, DataCATreeItem>)
                    {
                        var ds = (KeyValuePair<ID_TypeHierarchy, DataCATreeItem>)tree.SelectedItem;
                        i = ds.Value.GetItemForSearch();
                    }
                    else i = (tree.SelectedItem as IFindableItem).GetItemForSearch();
                }
                SaveSelected(i, ref shortcut);
            }
        }

        public static void SaveSelected(object i, ref string shortcut)
        {
            var item = i as THierarchyDbObject;
            if (item != null)
                shortcut += ";ItemSelectedId=" + item.Id + ";ItemSelectedType=" + item.Type.ToString();
            var sect = i as TSection;
            if (sect != null)
                shortcut += ";ItemSelectedId=" + sect.Section_ID + ";ItemSelectedType=Section";
            var tp = i as TPoint;
            if (tp != null)
                shortcut += ";ItemSelectedId=" + tp.TP_ID + ";ItemSelectedType=TP";
            var ti = i as TInfo_TI;
            if (ti != null)
                shortcut += ";ItemSelectedId=" + ti.TI_ID + ";ItemSelectedType=" + (!ti.IsCA ? enumTypeHierarchy.Info_TI.ToString() : enumTypeHierarchy.Info_ContrTI.ToString());
            var cobj = i as TContrObjectHierarchy;
            if (cobj != null)
                shortcut += ";ItemSelectedId=" + cobj.Id + ";ItemSelectedType=ContrObject";
            var cps = i as TContrPSHierarchy;
            if (cps != null)
                shortcut += ";ItemSelectedId=" + cps.Id + ";ItemSelectedType=ContrPS";
            var fs = i as TFormulaForSection;
            if (fs != null)
                shortcut += ";ItemSelectedId=" + fs.Formula_UN + ";ItemSelectedType=" + (fs.TP_Ch_ID.IsMoneyOurSide ? "OurFormula" : "CAFormula");
            var ab = i as InfoBit_Abonents_List;
            if (ab != null)
                shortcut += ";ItemSelectedId=" + ab.BitAbonent_ID + ";ItemSelectedType=Abonent";
        }

        public static object ReadTreeItemSelected(this Dictionary<string, string> dict, System.Windows.Controls.TreeView tree)
        {
            object obj = ReadSelected(dict);
            if (obj != null)
            {
                var founded = new Dictionary<object, Stack>();
                FindBar.scanNode(founded, tree.ItemsSource, obj, new Stack(), false, isFindOnlySingle:true);
                if (founded.Count > 0)
                    tree.ItemContainerGenerator.ExpandAndSelect(founded.First().Value.Clone() as Stack);
            }
            return obj;
        }

        public static object ReadSelected(Dictionary<string, string> dict)
        {
            object obj = null;
            if (dict.ContainsKey("ItemSelectedId"))
            {
                var type = dict["ItemSelectedType"];
                int id = int.Parse(dict["ItemSelectedId"]);
                switch (type)
                {
                    case "Section":
                        obj = GlobalSectionsDictionary.SectionsList[id];
                        break;
                    case "TP":
                        obj = EnumClientServiceDictionary.GetTps()[id];
                        break;
                    case "ContrObject":
                        obj = EnumClientServiceDictionary.ContrObjects[id];
                        break;
                    case "ContrPS":
                        obj = EnumClientServiceDictionary.DetailContrPSList[id];
                        break;
                    case "OurFormula":
                        obj = EnumClientServiceDictionary.FormulaFsk[dict["ItemSelectedId"]];
                        break;
                    case "CAFormula":
                        obj = EnumClientServiceDictionary.FormulaFsk[dict["ItemSelectedId"]];
                        break;
                    case "Abonent":
                        obj = EnumClientServiceDictionary.GetAbonent(id);
                        break;
                    default:
                        switch ((enumTypeHierarchy)Enum.Parse(typeof(enumTypeHierarchy), type))
                        {
                            case enumTypeHierarchy.Dict_HierLev1:
                                obj = EnumClientServiceDictionary.HierLev1List[id];
                                break;
                            case enumTypeHierarchy.Dict_HierLev2:
                                obj = EnumClientServiceDictionary.HierLev2List[id];
                                break;
                            case enumTypeHierarchy.Dict_HierLev3:
                                obj = EnumClientServiceDictionary.HierLev3List[id];
                                break;
                            case enumTypeHierarchy.Dict_PS:
                                TPSHierarchy ps;
                                if (EnumClientServiceDictionary.DetailPSList.TryGetValue(id, out ps, onException: Manager.UI.ShowMessage) && ps != null)
                                {
                                    obj = ps.HierarchyObject;
                                }
                                break;
                            case enumTypeHierarchy.Info_TI:
                                obj = EnumClientServiceDictionary.TIHierarchyList[id];
                                break;
                            case enumTypeHierarchy.Info_ContrTI:
                                obj = EnumClientServiceDictionary.TICAList[id];
                                break;
                        }
                        break;
                }
            }
            return obj;
        }

        public static void ClearItems(this ItemsControl itemsControl, bool isDisposeItems = true)
        {
            if (itemsControl != null)
            {
                try
                {
                    if (itemsControl.Items != null)
                    {
                        DisposeItemsDataTemplate(itemsControl, isDisposeItems);
                        //itemsControl.Items.Clear();
                    }

                    BindingOperations.ClearBinding(itemsControl, ItemsControl.ItemsSourceProperty);
                    itemsControl.ClearValue(ItemsControl.ItemsSourceProperty);

                    itemsControl.ItemsSource = null;
                }
                catch (Exception
#if DEBUG
                    ex
#endif
                    )
                {
#if DEBUG
                    Console.WriteLine("ClearItems: {0} ", ex.Message);
#endif
                }
            }
        }

        public static void DisposeItemsDataTemplate(this ItemsControl itemsControl, bool isDisposeItems)
        {
            for (int count = 0; count < itemsControl.Items.Count; count++)
            {
                var container = itemsControl.ItemContainerGenerator.ContainerFromIndex(count);
                if (container != null)
                {
                    var dic = container as ItemsControl;
                    if (dic != null && dic.Items != null)
                    {
                        //Очищаем дочерние объекты
                        DisposeItemsDataTemplate(dic, isDisposeItems);
                    }
                    
                    var userControl = container.GetVisualDescendent<UserControl>();
                    var controlToPotentiallyDispose = userControl as IDisposable;
                    if (controlToPotentiallyDispose != null)
                    {
                        controlToPotentiallyDispose.Dispose();
                    }
                }

                if (isDisposeItems)
                {
                    var item = itemsControl.Items[count] as IDisposable;
                    if (item !=null)
                    {
                        item.Dispose();
                    }
                }
            }
        }

        public static void DisposeDataPresenterTemplate(this DataPresenterBase presenterBase)
        {
            foreach (var record in presenterBase.Records)
            {
                var recpresenter = DataRecordPresenter.FromRecord(record);
                if (recpresenter != null)
                {
                    var fe = Utilities.GetDescendantFromType(recpresenter, typeof(FrameworkElement), true);
                    if (fe == null) continue;

                    var controlToPotentiallyDispose = fe as IDisposable;
                    if (controlToPotentiallyDispose != null)
                    {
                        controlToPotentiallyDispose.Dispose();
                    }

                    if (record.ParentDataRecord != null)
                    {
                        var disposable = record.ParentDataRecord.DataItem as IDisposable;
                        if (disposable != null)
                        {
                            disposable.Dispose();
                        }
                    }
                }

                if (record.HasChildren)
                {
                    var gbr = record as GroupByRecord;
                    if (gbr != null && gbr.ChildRecords.Count > 0)
                    {
                        foreach (var gbrChildRecord in gbr.ChildRecords)
                        {
                            var dr = gbrChildRecord as DataRecord;
                            if (dr == null) continue;

                            var crecpresenter = DataRecordPresenter.FromRecord(dr);
                            if (crecpresenter == null) continue;

                            var fe = Utilities.GetDescendantFromType(crecpresenter, typeof(FrameworkElement), true);
                            if (fe == null) continue;

                            var controlToPotentiallyDispose = fe as IDisposable;
                            if (controlToPotentiallyDispose != null)
                            {
                                controlToPotentiallyDispose.Dispose();
                            }

                            if (dr.DataItem != null)
                            {
                                var disposable = dr.DataItem as IDisposable;
                                if (disposable != null)
                                {
                                    disposable.Dispose();
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void DisposeChildren(this IEnumerable<IDisposable> ItemsSource)
        {
            if (ItemsSource != null)
            {
                foreach (IDisposable item in ItemsSource)
                {
                    item.Dispose();
                    RemoveSourceFromValueChangedEventManager(item);
                }
            }
        }

        public static void ClearAllSelection(this DataGridContext context)
        {
            context.CurrentItem = null;
            context.SelectedItems.Clear();
            context.SelectedCellRanges.Clear();
            context.SelectedItemRanges.Clear();
            foreach (var c in context.GetChildContexts()) c.ClearAllSelection();
        }

        #endregion

        #region XamGridSettingsProperty implementation

        /// <summary>
        /// Присоединенное св-во для Infragistics.XamDataGrid
        /// Включает сохранение и восстановление ширин столбцов и их порядка
        /// </summary>
        public static readonly DependencyProperty UseMeasureConverterProperty = DependencyProperty.RegisterAttached(
            "UseMeasureConverter", typeof(bool), typeof(VisualEx), new PropertyMetadata(false));

        public static void SetUseMeasureConverter(UIElement element, bool value)
        {
            element.SetValue(UseMeasureConverterProperty, value);
        }

        public static bool GetUseMeasureConverter(UIElement element)
        {
            return (bool)element.GetValue(UseMeasureConverterProperty);
        }

        #endregion

        #region IsExcessScrollDisabledProperty implementation

        /// <summary>
        /// Присоединенное св-во для TreeView
        /// Избавляет дерево от нежелательного скроллинга влево при выделении элемента
        /// </summary>
        public static readonly DependencyProperty IsExcessScrollDisabledProperty = DependencyProperty.RegisterAttached(
            "IsExcessScrollDisabled", typeof(Boolean), typeof(VisualEx), new PropertyMetadata(new PropertyChangedCallback(excessScrollDisabledPropertyChangedCallback)));

        public static void SetIsExcessScrollDisabled(UIElement element, Boolean value)
        {
            element.SetValue(IsExcessScrollDisabledProperty, value);
        }

        public static Boolean GetIsExcessScrollDisabled(UIElement element)
        {
            return (Boolean)element.GetValue(IsExcessScrollDisabledProperty);
        }

        private static void excessScrollDisabledPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tree = d as System.Windows.Controls.TreeView;
            if (tree != null)
            {
                if ((bool)e.NewValue)
                    tree.SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(TreeView_SelectedItemChanged);
                else
                    tree.SelectedItemChanged -= TreeView_SelectedItemChanged;
            }
        }

        private static void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var tree = sender as System.Windows.Controls.TreeView;
            var sv = tree.FindLogicalChild<ScrollViewer>();
            if (sv == null) return;

            sv.ScrollChanged += new ScrollChangedEventHandler(sv_ScrollChanged);
        }

        private static void sv_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var sv = sender as ScrollViewer;
            if (sv == null) return;

            sv.ScrollToLeftEnd();
            sv.ScrollChanged -= sv_ScrollChanged;
        }

        #endregion

        #region XamGridSettingsProperty implementation

        /// <summary>
        /// Присоединенное св-во для Infragistics.XamDataGrid
        /// Включает сохранение и восстановление ширин столбцов и их порядка
        /// </summary>
        public static readonly DependencyProperty XamGridSettingsProperty = DependencyProperty.RegisterAttached(
            "XamGridSettings", typeof(string), typeof(VisualEx), new PropertyMetadata(new PropertyChangedCallback(XamgridSettingsPropertyChangedCallback)));

        public static void SetXamGridSettings(UIElement element, string value)
        {
            element.SetValue(XamGridSettingsProperty, value);
        }

        public static string GetXamGridSettings(UIElement element)
        {
            return element.GetValue(XamGridSettingsProperty) as string;
        }

        private static void XamgridSettingsPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((e.NewValue as string).Length > 0)
            {
                if (!Manager.IsDesignMode)
                {
                    var grid = d as XamDataGrid;
                    grid.Loaded += new RoutedEventHandler(Xamgrid_Loaded);
                }
            }
        }

        private static void Xamgrid_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var grid = sender as XamDataGrid;
                var conf = GetXamGridSettings(grid);
                if (Manager.Config.XamGridSettings.ContainsKey(conf))
                {
                    var conf_str = Manager.Config.XamGridSettings[conf];
                    byte[] utf8ByteString = Encoding.UTF8.GetBytes(conf_str);
                    byte[] unicodeByteString = Encoding.Convert(Encoding.UTF8, Encoding.Unicode, utf8ByteString);
                    conf_str = Encoding.Unicode.GetString(unicodeByteString);
                    grid.LoadCustomizations(conf_str);
                }
                else
                {
                    if (Manager.Config.XamGridSettings.ContainsKey(conf + "_unicode"))
                    {
                        var conf_str = Manager.Config.XamGridSettings[conf + "_unicode"];
                        grid.LoadCustomizations(conf_str);
                    }
                }
                grid.FieldPositionChanged += GridOnFieldPositionChanged;
                grid.IsVisibleChanged += GridOnIsVisibleChanged;
            }
            catch (Exception)
            {
            }
        }

        private static void saveGridLayout(object sender)
        {
            var grid = sender as XamDataGrid;
            var conf = GetXamGridSettings(grid);
            var strm = new MemoryStream();
            grid.SaveCustomizations(strm);
            strm.Position = 0;
            var sr = new StreamReader(strm, Encoding.Unicode);
            var conf_str = sr.ReadToEnd();
            Manager.Config.XamGridSettings[conf + "_unicode"] = conf_str;
            if (Manager.Config.XamGridSettings.ContainsKey(conf))
                Manager.Config.XamGridSettings.Remove(conf);
        }

        private static void GridOnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            saveGridLayout(sender);
        }

        private static void GridOnFieldPositionChanged(object sender, FieldPositionChangedEventArgs fieldPositionChangedEventArgs)
        {
            saveGridLayout(sender);
        }

        #endregion

        #region GridSettings

        /// <summary>
        /// Присоединенное св-во для Xceed.DataGridControl
        /// Включает сохранение и восстановление ширин столбцов и их порядка
        /// </summary>
        public static readonly DependencyProperty GridSettingsProperty = DependencyProperty.RegisterAttached(
            "GridSettings", typeof(string), typeof(VisualEx), new PropertyMetadata(new PropertyChangedCallback(gridSettingsPropertyChangedCallback)));

        public static void SetGridSettings(UIElement element, string value)
        {
            element.SetValue(GridSettingsProperty, value);
        }

        public static string GetGridSettings(UIElement element)
        {
            return element.GetValue(GridSettingsProperty) as string;
        }

        private static void gridSettingsPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (Manager.IsDesignMode) return;

            var settings = e.NewValue as string;
            if (string.IsNullOrEmpty(settings) || settings.Length <= 0) return;

            var grid = d as DataGridControl;
            if (grid == null) return;

            if (!e.NewValue.ToString().EndsWith("FREE"))
            {
                if (e.NewValue.ToString().EndsWith("_IGOR"))
                    grid_Loaded(grid, null);
                else
                    grid.Loaded += new RoutedEventHandler(grid_Loaded);
            }
        }

        public static void SetColumnsTriggers(DataGridControl grid)
        {
            setColumns(grid, "main", grid.Columns);
            setDetails(grid, grid.DetailConfigurations);
            grid.Unloaded += grid_Unloaded;
        }

        private static void grid_Loaded(object sender, RoutedEventArgs e)
        {
            SetColumnsTriggers(sender as DataGridControl);
        }

        private static void grid_Unloaded(object sender, RoutedEventArgs e)
        {
            var grid = sender as DataGridControl;
            var colsForRemove = ColumnDictionary.Where(c => Equals(c.Value.Item1, grid))
                .Select(c => c.Key)
                .ToList();

            foreach (var col in colsForRemove)
            {
                if (widthProp != null) widthProp.RemoveValueChanged(col, width_Changed);
                if (orderProp != null) orderProp.RemoveValueChanged(col, order_Changed);
                if (visibilityProp != null) visibilityProp.RemoveValueChanged(col, visibility_Changed);

                ColumnDictionary.Remove(col);
            }

            colsForRemove.Clear();
        }

        private static void setDetails(DataGridControl grid, DetailConfigurationCollection details)
        {
            foreach (var detail in details)
            {
                setColumns(grid, detail.RelationName, detail.Columns);
                setDetails(grid, detail.DetailConfigurations);
                if (detail.Headers.Count != 0) continue;
                detail.Headers.Add(ResourceTemplatesLoader.ColumnManagerRowHeaders);
                detail.UseDefaultHeadersFooters = false;
            }
        }

        private static DependencyPropertyDescriptor widthProp, orderProp, visibilityProp;

        private static void setColumns(DataGridControl grid, string relation, ColumnCollection columns)
        {
            if (widthProp == null)
                widthProp = DependencyPropertyDescriptor.FromProperty(Column.WidthProperty, typeof(Column));
            if (orderProp == null)
                orderProp = DependencyPropertyDescriptor.FromProperty(Column.VisiblePositionProperty, typeof(Column));
            if (visibilityProp == null)
                visibilityProp = DependencyPropertyDescriptor.FromProperty(Column.VisibleProperty, typeof(Column));

            var set = grid.GetValue(GridSettingsProperty).ToString();
            var positions = new Dictionary<UnboundColumn, int>();
            foreach (var uc in columns.OfType<UnboundColumn>()) positions[uc] = uc.VisiblePosition;
            foreach (var colBase in columns)
            {
                var col = colBase as Column;
                if (col == null || ColumnDictionary.ContainsKey(col)) continue;

                var colConfig = Manager.Config.GetGridColumnConfig(set, relation, col.FieldName);
                if (double.IsNaN(colConfig.Width))
                {
                    colConfig.Width = col.Width;
                    colConfig.Order = col.VisiblePosition;
                    colConfig.IsVisible = col.Visible;
                }
                else
                {
                    col.Width = colConfig.Width;
                    col.VisiblePosition = colConfig.Order;
                    col.Visible = colConfig.IsVisible ?? true;
                }
                ColumnDictionary[col] = new Tuple<DataGridControl, GridColumnConfig>(grid, colConfig);
                widthProp.AddValueChanged(col, width_Changed);
                orderProp.AddValueChanged(col, order_Changed);
                visibilityProp.AddValueChanged(col, visibility_Changed);
            }
            foreach (var uc in positions) uc.Key.VisiblePosition = uc.Value;
        }

        private static readonly Dictionary<Column, Tuple<DataGridControl, GridColumnConfig>> ColumnDictionary =
            new Dictionary<Column, Tuple<DataGridControl, GridColumnConfig>>();

        private static void width_Changed(object sender, EventArgs e)
        {
            var c = sender as Column;
            if (c == null) return;

            Tuple<DataGridControl, GridColumnConfig> dg;
            if (ColumnDictionary.TryGetValue(c, out dg)) dg.Item2.Width = c.Width;
        }

        private static void order_Changed(object sender, EventArgs e)
        {
            var c = sender as Column;
            if (c == null) return;

            Tuple<DataGridControl, GridColumnConfig> dg;
            if (ColumnDictionary.TryGetValue(c, out dg)) dg.Item2.Order = c.VisiblePosition;
        }

        private static void visibility_Changed(object sender, EventArgs e)
        {
            var c = sender as Column;
            if (c == null) return;

            Tuple<DataGridControl, GridColumnConfig> dg;
            if (ColumnDictionary.TryGetValue(c, out dg)) dg.Item2.IsVisible = c.Visible;
        }


        #endregion

        #region ToolWindowSettingsProperty implementation

        public static readonly DependencyProperty ToolWindowSettingsProperty = DependencyProperty.RegisterAttached(
            "ToolWindowSettings", typeof(string), typeof(VisualEx), new PropertyMetadata(new PropertyChangedCallback(toolWindowSettingsPropertyChangedCallback)));

        public static void SetToolWindowSettings(UIElement element, string value)
        {
            if ((element != null) && (value != null))
                element.SetValue(ToolWindowSettingsProperty, value);
        }

        public static string GetToolWindowSettings(UIElement element)
        {
            if (element != null)
                return element.GetValue(ToolWindowSettingsProperty) as string;
            else return "";
        }

        private static void toolWindowSettingsPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tws = e.NewValue as string;
            if (tws != null)
            {
                if (!Manager.IsDesignMode)
                {
                    var tw = d as FrameworkElement;
                    if (Manager.Config.ToolWindowConfig.ContainsKey(tws))
                    {
                        var size = Manager.Config.ToolWindowConfig[tws];
                        if (size.Width == 0 || double.IsNaN(size.Width))
                            size.Width = 10;
                        if (size.Height == 0 || double.IsNaN(size.Height))
                            size.Height = 10;
                        DockSite.SetControlSize(tw, size);
                    }
                    tw.SizeChanged += new SizeChangedEventHandler(tw_SizeChanged);
                }
            }
        }

        private static void tw_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var set = (sender as FrameworkElement).GetValue(ToolWindowSettingsProperty).ToString();
            Manager.Config.ToolWindowConfig[set] = DockSite.GetControlSize(sender as FrameworkElement);
        }

        #endregion

        #region IsTopElementProperty implementation

        public static readonly DependencyProperty IsTopElementProperty = DependencyProperty.RegisterAttached(
            "IsTopElement", typeof(Boolean), typeof(VisualEx), new PropertyMetadata(new PropertyChangedCallback(topElementPropertyChangedCallback)));

        public static void SetIsTopElement(UIElement element, Boolean value)
        {
            element.SetValue(IsTopElementProperty, value);
        }

        public static Boolean GetIsTopElement(UIElement element)
        {
            return (Boolean)element.GetValue(IsExcessScrollDisabledProperty);
        }

        private static void topElementPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var top = d as FrameworkElement;
            if ((bool)e.NewValue)
                top.IsVisibleChanged += new DependencyPropertyChangedEventHandler(top_IsVisibleChanged);
            else top.IsVisibleChanged -= top_IsVisibleChanged;
        }

        public static int visibleCount = 0;

        public static void top_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                if (WorkPage.ActiveHeader != null && WorkPage.ActiveHeader.ArmModule != null)
                {
                    var wb = WorkPage.ActiveHeader.ArmModule.FindLogicalChild<WebBrowser>();
                    if (wb != null)
                    {
                        wb.Visibility = Visibility.Hidden;
                        visibleCount++;
                    }
                    else
                    {
                        var wh = WorkPage.ActiveHeader.ArmModule.FindLogicalChild<WindowsFormsHost>();
                        if (wh != null)
                        {
                            wh.Visibility = Visibility.Hidden;
                            visibleCount++;
                        }
                    }
                }
            }
            else
            {
                if (WorkPage.ActiveHeader != null)
                {
                    var wb = WorkPage.ActiveHeader.ArmModule.FindLogicalChild<WebBrowser>();
                    if (wb != null)
                    {
                        visibleCount--;
                        if (visibleCount <= 0) wb.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        var wh = WorkPage.ActiveHeader.ArmModule.FindLogicalChild<WindowsFormsHost>();
                        if (wh != null)
                        {
                            visibleCount--;
                            if (visibleCount <= 0) wh.Visibility = Visibility.Visible;
                        }
                    }
                }
            }
        }

        #endregion

        #region DockingSettingsProperty implementation

        public static readonly DependencyProperty DockingSettingsProperty = DependencyProperty.RegisterAttached(
            "DockingSettings", typeof(string), typeof(VisualEx), new PropertyMetadata(new PropertyChangedCallback(dockingSettingsPropertyChangedCallback)));

        public static void SetDockingSettings(UIElement element, string value)
        {
            if ((element != null) && (value != null))
                element.SetValue(DockingSettingsProperty, value);
        }

        public static string GetDockingSettings(UIElement element)
        {
            if (element != null)
                return element.GetValue(DockingSettingsProperty) as string;
            else return "";
        }

        private static void dockingSettingsPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var set = e.NewValue as string;
            if ((set != null) && e.OldValue == null && !set.StartsWith("~") && !Manager.IsDesignMode)
            {
                var dock = d as DockSite;
                if (dock != null)
                {
                    dock.Loaded += dock_Loaded;
                    //dock.Unloaded += dockUnloaded;
                }
            }
        }

        private static void dockUnloaded(object sender, RoutedEventArgs e)
        {
            var dock = (sender as FrameworkElement).FindParent<DockSite>();
            if (dock != null)
            {
                dock.Loaded -= dock_Loaded;
                dock.Unloaded -= dockUnloaded;

                if (Manager.Config != null)
                {
                    save(sender);
                }
            }
        }

        private static void dock_Loaded(object sender, RoutedEventArgs e)
        {
            var dock = sender as DockSite;
            if (dock == null) return;

            dock.Loaded -= dock_Loaded;
            dock.Unloaded += dockUnloaded;
            //dock.Unloaded -= dockUnloaded;

            var set = GetDockingSettings(dock);

            if (set == null) return;

            var values = set.Split(new string[] { "~" }, StringSplitOptions.RemoveEmptyEntries);
            var v0 = values[0];
            if (Manager.Config.IsHorizontalTrees && values.Length > 1) v0 += "_h";

            var isLoadedCorrectly = false;
            string dc;
            if (Manager.Config.DockingConfig.TryGetValue(v0, out dc))
            {



                //dock.Dispatcher.BeginInvoke((Action)(() =>
                //{
#if DEBUG

                    var swl = new System.Diagnostics.Stopwatch();
                    swl.Start();
#endif
                    try
                    {

                        var ds = new DockSiteLayoutSerializer
                        {
                            SerializationBehavior = DockSiteSerializationBehavior.All,
                            DocumentWindowDeserializationBehavior = DockingWindowDeserializationBehavior.Discard,
                            ToolWindowDeserializationBehavior = DockingWindowDeserializationBehavior.Discard,
                        };


                        ds.LoadFromString(dc, dock);

                    }
                    catch
                    {
                        //Manager.UI.ShowMessage(ex.Message);
                    }
#if DEBUG

                    swl.Stop();
                    Console.WriteLine("dock_Loaded: ds.LoadFromString {0} млс", swl.ElapsedMilliseconds);
#endif
                //}), DispatcherPriority.Send);

                isLoadedCorrectly = true;
            }

            if (!isLoadedCorrectly)
            {
                var v1 = values.LastOrDefault();
                if (!string.IsNullOrEmpty(v1) && v1.IndexOf(".layout") >= 0)
                {
                    v1 = v1.Replace("%", Manager.Config.IsHorizontalTrees ? "horizontal" : "vertical").Replace("ARM_20", "ElectroARM");
                    try
                    {
                        var uri = new Uri("pack://application:,,,/" + v1);
                        var serializer = new DockSiteLayoutSerializer
                        {
                            SerializationBehavior = DockSiteSerializationBehavior.All,
                            DocumentWindowDeserializationBehavior = DockingWindowDeserializationBehavior.Discard,
                            ToolWindowDeserializationBehavior = DockingWindowDeserializationBehavior.LazyLoad,
                        };
                        var resource = Application.GetResourceStream(uri);

                        serializer.LoadFromStream(resource.Stream, dock);
                    }
                    catch (Exception ex)
                    {
                        Manager.UI.ShowMessage(ex.Message);
                    }
                }
            }

#if DEBUG

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif

            //scanDockSite(dock.Content as FrameworkElement);
            //SetDockingSettings(dock, "~" + values[0]);

#if DEBUG
            sw.Stop();
            Console.WriteLine("dock_Loaded: SetDockingSettings {0} млс", sw.ElapsedMilliseconds);
#endif
        }

        private static void scanDockSite(FrameworkElement content)
        {
            var sc = content as SplitContainer;
            if (sc != null)
            {
                sc.SizeChanged += new SizeChangedEventHandler(panel_SizeChanged);
                foreach (var pane in sc.Children) scanDockSite(pane);
                return;
            }
            var tabbedHost = content as TabbedMdiHost;
            if (tabbedHost != null)
            {
                scanDockSite(tabbedHost.Content as FrameworkElement);
                return;
            }
            var tabbedCont = content as TabbedMdiContainer;
            if (tabbedCont != null)
            {
                //tabbedCont.SizeChanged += new SizeChangedEventHandler(panel_SizeChanged);
                tabbedCont.SelectionChanged += (sender, args) =>
                                               {
                                                   save(sender);
                                               };
                foreach (var pane in tabbedCont.Items) scanDockSite(pane as FrameworkElement);
                return;
            }
            var twc = content as ToolWindowContainer;
            if (twc != null)
            {
                twc.SelectionChanged += (sender, args) =>
                                        {
                                            save(sender);
                                        };
                twc.SizeChanged += new SizeChangedEventHandler(panel_SizeChanged);
                foreach (var pane in twc.Items) scanDockSite(pane as FrameworkElement);
                return;
            }
            var tw = content as ToolWindow;
            if (tw != null)
            {
                tw.SizeChanged += new SizeChangedEventHandler(panel_SizeChanged);
                return;
            }
        }

        private static void save(object sender)
        {
            var fe = sender as FrameworkElement;
            if (fe == null) return;

            var dock = fe.FindParent<DockSite>();
            if (dock == null) return;

            if (!(dock.Tag is bool))
            {
                dock.Tag = true;
                //dock.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                //    new Action(delegate
                //    {
                if (Manager.Config == null || Manager.Config.DockingConfig == null) return;

                try
                {
                    var set = dock.GetValue(VisualEx.DockingSettingsProperty) as string;
                    if (set != null && !string.IsNullOrEmpty(set))
                    {
                        set = set.Substring(1);

                        var ls = new DockSiteLayoutSerializer
                        {
                            SerializationBehavior = DockSiteSerializationBehavior.All,
                            DocumentWindowDeserializationBehavior = DockingWindowDeserializationBehavior.Discard,
                            ToolWindowDeserializationBehavior = DockingWindowDeserializationBehavior.Discard,
                        }.SaveToString(dock);

                        Manager.Config.DockingConfig[set] = ls;
                    }
                }
                finally
                {
                    dock.Tag = null;
                }

                //}));
            }

        }

        private static void panel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //save(sender);
        }

        #endregion

        #region ResizableControlProperty implementation

        private const int border_offset = 7;

        public static readonly DependencyProperty ResizableControlProperty = DependencyProperty.RegisterAttached(
            "ResizableControl", typeof(ResizeHelper), typeof(VisualEx), new PropertyMetadata(new PropertyChangedCallback(resizableControlPropertyChangedCallback)));

        public static void SetResizableControl(FrameworkElement element, ResizeHelper value)
        {
            if ((element != null) && (value != null))
                element.SetValue(ResizableControlProperty, value);
        }

        public static ResizeHelper GetResizableControl(FrameworkElement element)
        {
            if (element != null)
                return element.GetValue(ResizableControlProperty) as ResizeHelper;
            else return null;
        }

        private static void resizableControlPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var fe = d as FrameworkElement;
            fe.PreviewMouseDown += new MouseButtonEventHandler(fe_PreviewMouseDown);
            fe.PreviewMouseMove += new MouseEventHandler(fe_PreviewMouseMove);
            fe.PreviewMouseUp += new MouseButtonEventHandler(fe_PreviewMouseUp);
        }

        private static void fe_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            var element = sender as FrameworkElement;
            var rh = GetResizableControl(element);
            if ((rh.isLeft != null) || (rh.isTop != null))
            {
                element.ReleaseMouseCapture();
                rh.isLeft = rh.isTop = null;
            }
        }

        private static double checkNegative(double value)
        {
            if (value < 1) return 5;
            else return value;
        }

        private static void fe_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            var element = sender as FrameworkElement;
            var rh = GetResizableControl(element);
            if ((rh.isLeft != null) || (rh.isTop != null))
            {
                var pos = element.PointToScreen(e.GetPosition(element));
                if (rh.isLeft == false) element.Width = checkNegative(element.ActualWidth + pos.X - rh.old.X);
                if (rh.isTop == false)
                {
                    if (!rh.isRigthDown) element.MinHeight = checkNegative(element.ActualHeight + pos.Y - rh.old.Y);
                    else element.Height = checkNegative(element.ActualHeight + pos.Y - rh.old.Y);
                }
                if (rh.isLeft == true) element.Width = checkNegative(element.ActualWidth - pos.X + rh.old.X);
                if (rh.isTop == true) element.Height = checkNegative(element.ActualHeight - pos.Y + rh.old.Y);
                rh.old = pos;
            }
            bool? _isLeft = null, _isTop = null;
            var _pos = e.GetPosition(element);
            if (!rh.isRigthDown)
            {
                if (_pos.X < border_offset) _isLeft = true;
            }
            else
            {
                if (_pos.X > element.ActualWidth - border_offset) _isLeft = false;
            }
            {
                if (_pos.Y > element.ActualHeight - border_offset) _isTop = false;
            }
            if ((_isLeft != null) && (_isTop == null))
            {
                e.MouseDevice.SetCursor(Cursors.SizeWE);
                return;
            }
            if ((_isTop != null) && (_isLeft == null))
            {
                e.MouseDevice.SetCursor(Cursors.SizeNS);
                return;
            }
            if (((_isLeft == true) && (_isTop == true)) || ((_isLeft == false) && (_isTop == false)))
            {
                e.MouseDevice.SetCursor(Cursors.SizeNWSE);
                return;
            }
            if (((_isLeft == true) && (_isTop == false)) || ((_isLeft == false) && (_isTop == true)))
            {
                e.MouseDevice.SetCursor(Cursors.SizeNESW);
                return;
            }
        }

        private static void fe_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var element = sender as FrameworkElement;
            var rh = GetResizableControl(element);
            var pos = e.GetPosition(element);
            if (!rh.isRigthDown)
            {
                if (pos.X < border_offset) rh.isLeft = true;
            }
            else
            {
                if (pos.X > element.ActualWidth - border_offset) rh.isLeft = false;
            }
            if (pos.Y > element.ActualHeight - border_offset)
            {
                rh.isTop = false;
            }
            if ((rh.isLeft != null) || (rh.isTop != null))
            {
                try
                {
                    rh.old = element.PointToScreen(e.GetPosition(element));
                    element.CaptureMouse();
                }
                catch (Exception)
                {
                }
            }
        }

        public static void MakeResizable(this FrameworkElement element)
        {
            SetResizableControl(element, new ResizeHelper());
        }

        public static void MakeResizable(this FrameworkElement element, bool isRightDown)
        {
            SetResizableControl(element, new ResizeHelper() { isRigthDown = isRightDown });
        }

        #endregion


        #region OutlookBarSettingsProperty Implementation


        public static readonly DependencyProperty OutlookBarPinnedProperty = DependencyProperty.RegisterAttached(
            "OutlookBarPinned", typeof(bool), typeof(VisualEx), new PropertyMetadata(new PropertyChangedCallback(OutlookBarPinnedChanged)));

        private static void OutlookBarPinnedChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var bar = dependencyObject as XamOutlookBar;
            if (bar != null)
            {
                var but = bar.FindLogicalChild<ToggleButton>();
                if (but != null)
                {
                    but.IsChecked = dependencyPropertyChangedEventArgs.NewValue is bool ? (bool)dependencyPropertyChangedEventArgs.NewValue : false;
                }

            }
        }

        public static void SetOutlookBarPinned(UIElement element, bool value)
        {
            element.SetValue(OutlookBarPinnedProperty, value);
        }

        public static bool GetOutlookBarPinned(UIElement element)
        {
            return element.GetValue(OutlookBarPinnedProperty) is bool ? (bool)element.GetValue(OutlookBarPinnedProperty) : false;
        }


        public static readonly DependencyProperty OutlookBarSettingsProperty = DependencyProperty.RegisterAttached(
            "OutlookBarSettings", typeof(string), typeof(VisualEx), new PropertyMetadata(new PropertyChangedCallback(OutlookBarSettingsPropertyChanged)));

        private static void OutlookBarSettingsPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var bar = dependencyObject as XamOutlookBar;
            if (bar != null)
            {
                bar.Loaded += BarOnLoaded;
            }
        }

        private static void BarOnLoaded(object o, RoutedEventArgs routedEventArgs)
        {
            if (!Manager.IsDesignMode)
            {
                var bar = o as XamOutlookBar;
                if (bar != null)
                {


                    RestoreBarSettings(bar, GetOutlookBarSettings(bar));
                    //  bar.SelectedGroupChanged += Bar_ParamChanged;
                    bar.NavigationPaneMinimized += Bar_ParamChanged;
                    bar.NavigationPaneExpanded += Bar_ParamChanged;
                    bar.SizeChanged += Bar_ParamChanged;


                }
            }

        }

        private static void Bar_ParamChanged(object sender, RoutedEventArgs e)
        {

            var t = sender as XamOutlookBar;
            var sch = (e as SizeChangedEventArgs);
            if (sch != null)
            {
                if (sch.HeightChanged && !sch.WidthChanged) return;
                // если высота изменилась не учитываем
            }
            SaveBarSettings(t, GetOutlookBarSettings(t));


        }

        public static void SetOutlookBarSettings(UIElement element, string value)
        {
            element.SetValue(OutlookBarSettingsProperty, value);
        }

        public static string GetOutlookBarSettings(UIElement element)
        {
            return element.GetValue(OutlookBarSettingsProperty) as string;
        }

        static void SaveBarSettings(XamOutlookBar bar, string settingString)
        {
            if (bar.SelectedGroup == null) return;


            var ispin = bar.FindLogicalChild<ToggleButton>().IsChecked.GetValueOrDefault(false);
            var OutlookBarSettingsCollection = new OutlookBarSettingsCollection()
            {
                IsMinimized = bar.IsMinimized,
                Width = bar.Width,
                GroupHeaderName = bar.SelectedGroup.Header.ToString(),
                IsPinned = ispin
            };
            Task.Factory.StartNew(() =>
            {

                Manager.Config.OutlookBarSettings[settingString] = OutlookBarSettingsCollection;
                Manager.SaveConfig();
            });

        }

        static void RestoreBarSettings(XamOutlookBar bar, string settingString)
        {
            if (Manager.Config.OutlookBarSettings == null)
            {
                Manager.Config.OutlookBarSettings = new Dictionary<string, OutlookBarSettingsCollection>();
            }

            if (bar == null || string.IsNullOrEmpty(settingString) || OutlookBarPinnedProperty == null) return;

            OutlookBarSettingsCollection par;
            if (Manager.Config.OutlookBarSettings.TryGetValue(settingString, out par) && par!=null)
            {
                bar.IsMinimized = par.IsMinimized;
                bar.Width = par.Width;
                bar.SelectedGroup = bar.Groups.FirstOrDefault(i => i.Header.ToString() == par.GroupHeaderName);
                bar.SetValue(OutlookBarPinnedProperty, par.IsPinned);
            }
        }

        #endregion

        public static void WalkDictionary(ResourceDictionary resources)
        {
            foreach (DictionaryEntry entry in resources)
            {
            }
            foreach (ResourceDictionary rd in resources.MergedDictionaries)
                WalkDictionary(rd);
        }

        public static WrapPanel GetWrapPanel(params object[] elements)
        {
            var wp = new WrapPanel();
            foreach (var element in elements)
            {
                if (element is string) wp.Children.Add(WarningPanel.GetTextBox(element.ToString()));
                else wp.Children.Add(element as UIElement);
            }
            return wp;
        }

        public static void FixedColumns(this DataGridControl grid, int count)
        {
            (grid.View as TableView).FixedColumnCount = count;
        }

        public static void UpdateAlarmCount()
        {
            new System.Threading.Thread(new System.Threading.ThreadStart(delegate()
            {
                try
                {
                    int alarmCount = ARM_Service.ALARM_Get_CurrentAlarms_Count(Manager.User.User_ID);
                    Manager.UI.Alarm(alarmCount);
                }
                catch (Exception)
                {
                }
            })).Start();
        }

        public static Window WinHost = null;

        public static void SetWindowTitle(Page page)
        {
            if (Manager.User != null)
            {
                string title = "";
                try
                {
                    //может упасть на некоторых машинах клиента
                    title = string.Format("АРМ ЭНЕРГЕТИКА  (Пользователь: {0}; База данных: {1}; Версия: {2})",
                        Manager.User.UserFullName, Manager.User.DBName, Assembly.GetExecutingAssembly().GetName().Version.ToString());
                    if (!string.IsNullOrEmpty(title))
                    {
                        if (WinHost != null) WinHost.Title = title;
                        page.WindowTitle = title;
                    }
                }
                catch (Exception ex)
                {
                    ARM_Service.ReportClientException(ex.Message, ex.StackTrace, title, "");

                }
            }
        }

        public static void DoEvents()
        {
            Thread.Sleep(5);
            DispatcherFrame f = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                (SendOrPostCallback)delegate (object arg)
                                    {
                                        DispatcherFrame fr = arg as DispatcherFrame;
                                        fr.Continue = false;
                                    }, f);
            Dispatcher.PushFrame(f);
        }

        public static FrameworkElement FindTrueIModule(this FrameworkElement fe)
        {
            if (fe == null) return null;

            var pe = fe.Parent as FrameworkElement;

            if (pe == null)
            {
                pe = fe.TemplatedParent as FrameworkElement;
            }

            if (pe == null) return null;

            var parent = pe.FindParent<IModule>() as FrameworkElement;
            if (parent == null) return null;

            if (parent is UIManagerContainer) return FindTrueIModule(parent);

            return parent;
        }

        public static FrameworkElement FindTrueIModuleFromPopup(this Popup popup)
        {
            if (popup == null) return null;

            var pt = popup.PlacementTarget as FrameworkElement;
            if (pt == null) return null;

            var parent = pt.FindTrueIModule();
            if (parent != null) return parent.FindLogicalChild<UIManagerContainer>();

            popup = pt.FindParent<System.Windows.Controls.Primitives.Popup>();
            return popup.FindTrueIModuleFromPopup();
        }

        public static UIManagerContainer FindModuleContainer(this FrameworkElement fe)
        {
            if (fe == null) return null;

            var pe = fe.Parent as FrameworkElement;
            if (pe == null) return null;

            var parent = pe.FindTrueIModule();
            if (parent == null) return null;

            if (parent is UIManagerContainer) return FindModuleContainer(parent);

            return parent.FindLogicalChild<UIManagerContainer>();
        }


        /// <summary>
        /// Очистка фильтра
        /// </summary>
        /// <param name="Table">XCeed грид</param>
        public static void ClearColumnFilter(this DataGridControl Table)
        {
            DataGridContext rootContext = DataGridControl.GetDataGridContext(Table);
            if (rootContext == null) return;

            rootContext.ClearColumnFilter();
        }

        /// <summary>
        /// Очистка фильтра
        /// </summary>
        /// <param name="rootContext"></param>
        private static void ClearColumnFilter(this DataGridContext rootContext)
        {
            if (rootContext != null)
            {
                if (rootContext.AutoFilterValues != null && rootContext.Columns != null)
                {
                    int i;
                    foreach (var col in rootContext.Columns)
                    {
                        if (col != null && col.AllowAutoFilter && !String.IsNullOrEmpty(col.FieldName))
                        {
                            string fName;
                            i = col.FieldName.IndexOf(".");
                            if (i > 0)
                            {
                                fName = col.FieldName.Substring(0, i);
                            }
                            else
                            {
                                fName = col.FieldName;
                            }

                            IList filter;
                            if (rootContext.AutoFilterValues.TryGetValue(fName, out filter) && filter != null)
                            {
                                filter.Clear();
                            }
                        }
                    }
                }

                DetailConfigurationCollection DetailConfigurations = rootContext.DetailConfigurations;

                //Теперь убирвем фильтры на дочерних объектах
                if (rootContext.Items != null && DetailConfigurations != null)
                {
                    // заполнение таблицы
                    for (int j = 0; j < rootContext.Items.Count; j++)
                    {
                        object item = rootContext.Items.GetItemAt(j);

                        if (item != null)
                        {
                            foreach (var detail in DetailConfigurations)
                            {
                                var childContext = rootContext.GetChildContext(item, detail.RelationName);
                                ClearColumnFilter(childContext);
                            }
                        }
                    }
                }
            }
        }


        public static FrameworkElement ToControl(this IFreeHierarchyObject freeHierarchyObject, bool isStandartImageVisible = true)
        {
            if (freeHierarchyObject == null) return null;

            switch (freeHierarchyObject.Type)
            {
                case enumTypeHierarchy.Dict_Contr_PS:
                    return new ContrPS
                    {
                        DataContext = freeHierarchyObject,
                    };
                case enumTypeHierarchy.Info_ContrTI:
                    return new ContrTI
                    {
                        DataContext = freeHierarchyObject,
                    };
                case enumTypeHierarchy.Dict_JuridicalPersons_Contract:
                    return new JuridicalPersonsContract
                    {
                        DataContext = freeHierarchyObject,
                    };

                case enumTypeHierarchy.Formula:
                case enumTypeHierarchy.Formula_TP_OurSide:
                case enumTypeHierarchy.Formula_TP_CA:
                    return new Formula
                    {
                        IsStandartImageVisible = isStandartImageVisible,
                        DataContext = freeHierarchyObject,
                    };
                case enumTypeHierarchy.LinkedFormula:
                    return new LinkedFormulasTpControl
                    {
                        DataContext = freeHierarchyObject,
                    };
                case enumTypeHierarchy.Node:
                    return new FreeHier
                    {
                        DataContext = freeHierarchyObject,
                    };
                
                case enumTypeHierarchy.PTransformator:
                    return new Transformator
                    {
                        DataContext = freeHierarchyObject,
                    };
                case enumTypeHierarchy.Reactor:
                    return new Reactor
                    {
                        DataContext = freeHierarchyObject,
                    };
                case enumTypeHierarchy.FormulaConstant:
                    return new FormulaConstantControl
                    {
                        DataContext = freeHierarchyObject,
                    };
                case enumTypeHierarchy.ForecastObject:
                    return new ForecastObjectControl
                    {
                        DataContext = freeHierarchyObject,
                    };
                case enumTypeHierarchy.OldTelescopeTreeNode:
                    return new OldTelescopeTreeNode
                    {
                        DataContext = freeHierarchyObject,
                    };
                case enumTypeHierarchy.Dict_HierLev1:
                case enumTypeHierarchy.Dict_HierLev2:
                case enumTypeHierarchy.Dict_HierLev3:
                case enumTypeHierarchy.Dict_PS:
                case enumTypeHierarchy.Info_TI:
                case enumTypeHierarchy.Info_TP:
                    return new HierObject
                    {
                        IsStandartImageVisible = isStandartImageVisible,
                        DataContext = freeHierarchyObject,
                    };
               
                case enumTypeHierarchy.Section:
                    return new Section
                    {
                        DataContext = freeHierarchyObject
                    };
                
                case enumTypeHierarchy.Dict_DirectConsumer:
                    return new DirectConsumer
                    {
                        DataContext = freeHierarchyObject
                    };
                case enumTypeHierarchy.JuridicalPerson:
                    return new JuridicalPerson
                    {
                        DataContext = freeHierarchyObject
                    };
                case enumTypeHierarchy.USPD:
                    return new USPD
                    {
                        DataContext = freeHierarchyObject
                    };
                case enumTypeHierarchy.E422:
                    return new E422Control
                    {
                        DataContext = freeHierarchyObject
                    };
                case enumTypeHierarchy.Concentrator:
                    return new Concentrator
                    {
                        DataContext = freeHierarchyObject
                    };
                case enumTypeHierarchy.UANode:
                    return new UANode
                    {
                        DataContext = freeHierarchyObject
                    };
                case enumTypeHierarchy.UniversalBalance:
                    return new BalanceFreeHierarchyControl
                    {
                        DataContext = freeHierarchyObject
                    };
                case enumTypeHierarchy.UAServer:
                    return new UaServerControl
                    {
                        DataContext = freeHierarchyObject
                    };

                default:
                    return new TextBlock
                    {
                        Text = freeHierarchyObject.Name
                    };
            }
        }


        public static void SaveStreamToFile(this MemoryStream stream, string fileName, string ext, StringBuilder errors = null, bool isDecompress = true)
        {
            var saveFileDelegate = (Action)(() =>
                                                {
                                                    if (stream == null || stream.Length == 0) return;

                                                    if (isDecompress)
                                                    {
                                                        var decompressed = CompressUtility.DecompressGZip(stream);
                                                        AskueARM2.Both.VisualCompHelpers.FileAdapter.SaveFile(decompressed, fileName, ext, Manager.UI.ShowMessage, (message, fn) => Manager.UI.ShowYesNoDialog(message, () =>
                                                        {
                                                            CommonEx.OpenSavedFile(fn);
                                                        }));
                                                    }
                                                    else
                                                    {
                                                        AskueARM2.Both.VisualCompHelpers.FileAdapter.SaveFile(stream, fileName, ext, Manager.UI.ShowMessage, (message, fn) => Manager.UI.ShowYesNoDialog(message, () =>
                                                        {
                                                            CommonEx.OpenSavedFile(fn);
                                                        }));
                                                    }
                                                });

            if (errors != null && errors.Length > 0)
            {
                Manager.UI.ShowMessage(errors.ToString(), saveFileDelegate);
            }
            else
            {
                saveFileDelegate();
            }
        }

        public static void RemoveSourceFromValueChangedEventManager(object source)
        {
            if (source == null) return;
            try
            {
                // Remove the source from the ValueChangedEventManager. 
                Assembly assembly = Assembly.GetAssembly(typeof(FrameworkElement));
                Type type = assembly.GetType("MS.Internal.Data.ValueChangedEventManager");
                PropertyInfo propertyInfo = type.GetProperty("CurrentManager",
                    BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo currentManagerGetter = propertyInfo.GetGetMethod(true);
                object manager = currentManagerGetter.Invoke(null, null);
                MethodInfo remove = type.GetMethod("Remove", BindingFlags.NonPublic |
                                                             BindingFlags.Instance);
                remove.Invoke(manager, new[] { source });
                // The code above removes the instances of ValueChangedRecord from the 
                // WeakEventTable, but they are still rooted by the property descriptors of 
                // the source object. We need to clean them out of the property descriptors 
                // as well, to allow them to be garbage collected. (Which is necessary 
                // because they contain a hard reference to the source, which is what we 
                // really want garbage collected.) 
                FieldInfo valueChangedHandlersInfo = typeof(PropertyDescriptor).GetField
                    ("valueChangedHandlers", BindingFlags.Instance | BindingFlags.NonPublic);
                PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(source);
                foreach (PropertyDescriptor pd in pdc)
                {
                    Hashtable changeHandlers =
                        (Hashtable)valueChangedHandlersInfo.GetValue(pd);
                    if (changeHandlers != null)
                    {
                        changeHandlers.Remove(source);
                    }
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Выполнение запроса к серверу в отдельном потоке с обработкой результата в UI потоке
        /// </summary>
        /// <typeparam name="T">Тип результата которые возвращается в UI поток</typeparam>
        /// <param name="element">UI элемент который блокируется на момент выполнения отдельного потока</param>
        /// <param name="methodName">Название вызываемой на сервере процедуры</param>
        /// <param name="onCompleteRunUiMethod">Метода обрабатывающий результат. В UI потоке</param>
        /// <param name="serviceType">Тип сервиса к которому выполняется запрос</param>
        /// <param name="args">Аргументы передаваемы на сервис</param>
        public static void RunBackgroundAsync<T>(this FrameworkElement element, string methodName, Action<T> onCompleteRunUiMethod, EnumServiceType serviceType,
            params object[] args) where T : class
        {
            ContentControl topPanel = null;
            ContentControl innerPanel = null;
            UIManagerContainer managerContainer = null;

            var timer = new DispatcherTimer(TimeSpan.FromMilliseconds(200), DispatcherPriority.Background,
                (sender, eventArgs) =>
                {
                    var t = sender as DispatcherTimer;
                    if (t != null)
                    {
                        t.Stop();
                    }

                    managerContainer = element as UIManagerContainer ?? element.FindParent<UIManagerContainer>();
                    if (managerContainer != null)
                    {
                        topPanel = WaitPanel.FindAndAddContentControl(managerContainer);
                        if (topPanel != null)
                        {
                            if (topPanel.Content != null && !(topPanel.Content is BusyIndicator))
                            {
                                innerPanel = topPanel;
                                innerPanel.IsEnabled = false;
                                topPanel = WaitPanel.AddNewContentControl(managerContainer);
                            }


                            if (topPanel != null && topPanel.Content == null)
                            {
                                topPanel.Content = new BusyIndicator();
                            }
                        }

                        managerContainer.IsEnabled = false;
                    }
                    else
                    {
                        element.IsEnabled = false;
                    }
                }, element.Dispatcher);

            

            Task.Factory.StartNew(() =>
            {
                T result;
                string exMessage = null;
                try
                {
                    switch (serviceType)
                    {
                        case EnumServiceType.ArmService:
                            result = ServiceFactory.ArmServiceInvokeSync<T>(methodName, args);
                            break;
                        case EnumServiceType.FreeHierarchy:
                            result = ServiceFactory.FreeHierarchyInvokeSync<T>(methodName, args);
                            break;
                        case EnumServiceType.StimulReport:
                            result = ServiceFactory.StimulReportInvokeSync<T>(methodName, args);
                            break;
                        default:
                            result = null;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    exMessage = ex.Message;
                    result = null;
                }

                element.Dispatcher.BeginInvoke((Action)(
                    () =>
                    {
                        timer.Stop();

                        if (topPanel != null && topPanel.Content is BusyIndicator)
                        {
                            topPanel.Content = null;
                            if (managerContainer != null)
                            {
                                managerContainer.Children.Remove(topPanel);
                                try
                                {
                                    managerContainer.UnregisterName(topPanel.Name);
                                }
                                catch
                                {
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(exMessage))
                        {
                            Manager.UI.ShowLocalMessage(exMessage, managerContainer);
                        }

                        if (onCompleteRunUiMethod != null)
                        {
                            try
                            {
                                onCompleteRunUiMethod(result);
                            }
                            catch (Exception ex)
                            {
                                Manager.UI.ShowLocalMessage(ex.Message, managerContainer);
                            }
                        }

                        //onCompleteRunUiMethod = null;

                        if (innerPanel != null) innerPanel.IsEnabled = true;

                        if (managerContainer != null) managerContainer.IsEnabled = true;
                        else element.IsEnabled = true;

                    }), DispatcherPriority.Send);
            });
        }

        /// <summary>
        /// Выполнение запроса к серверу в отдельном потоке с обработкой результата в UI потоке, с выводом окна "Подождите"
        /// </summary>
        /// <typeparam name="T">Тип результата которые возвращается в UI поток</typeparam>
        /// <param name="element">UI элемент который блокируется на момент выполнения отдельного потока</param>
        /// <param name="methodName">Название вызываемой на сервере процедуры</param>
        /// <param name="onCompleteRunUiMethod">Метода обрабатывающий результат. В UI потоке</param>
        /// <param name="serviceType">Тип сервиса к которому выполняется запрос</param>
        /// <param name="args">Аргументы передаваемы на сервис</param>
        public static void RunBackgroundAsync<T>(this FrameworkElement element, string methodName, Action<T, object[]> onCompleteRunUiMethod, EnumServiceType serviceType,
            params object[] args) where T : class
        {
            ContentControl topPanel = null;
            ContentControl innerPanel = null;
            var managerContainer = element as UIManagerContainer ?? element.FindParent<UIManagerContainer>();
            if (managerContainer != null)
            {
                topPanel = WaitPanel.FindAndAddContentControl(managerContainer);
                if (topPanel != null)
                {
                    if (topPanel.Content != null && !(topPanel.Content is BusyIndicator))
                    {
                        innerPanel = topPanel;
                        innerPanel.IsEnabled = false;
                        topPanel = WaitPanel.AddNewContentControl(managerContainer);
                    }


                    if (topPanel != null && topPanel.Content == null)
                    {
                        topPanel.Content = new BusyIndicator();
                    }
                }
                managerContainer.IsEnabled = false;
            }
            else
            {
                element.IsEnabled = false;
            }

            Task.Factory.StartNew(() =>
            {
                T result;
                string exMessage = null;
                try
                {
                    switch (serviceType)
                    {
                        case EnumServiceType.ArmService:
                            result = ServiceFactory.ArmServiceInvokeSync<T>(methodName, args);
                            break;
                        case EnumServiceType.FreeHierarchy:
                            result = ServiceFactory.FreeHierarchyInvokeSync<T>(methodName, args);
                            break;
                        case EnumServiceType.StimulReport:
                            result = ServiceFactory.StimulReportInvokeSync<T>(methodName, args);
                            break;
                        default:
                            result = null;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    exMessage = ex.Message;
                    result = null;
                }

                element.Dispatcher.BeginInvoke((Action)(
                    () =>
                    {
                        if (topPanel != null && topPanel.Content is BusyIndicator)
                        {
                            topPanel.Content = null;
                            if (managerContainer != null)
                            {
                                managerContainer.Children.Remove(topPanel);
                                managerContainer.UnregisterName(topPanel.Name);
                            }
                        }

                        if (!string.IsNullOrEmpty(exMessage))
                        {
                            Manager.UI.ShowLocalMessage(exMessage, managerContainer);
                        }

                        if (onCompleteRunUiMethod != null)
                        {
                            try
                            {
                                onCompleteRunUiMethod(result, args);
                            }
                            catch (Exception ex)
                            {
                                Manager.UI.ShowLocalMessage(ex.Message, managerContainer);
                            }
                        }

                        //onCompleteRunUiMethod = null;

                        if (innerPanel != null) innerPanel.IsEnabled = true;

                        if (managerContainer != null) managerContainer.IsEnabled = true;
                        else element.IsEnabled = true;

                    }), DispatcherPriority.Send);
            });
        }

        /// <summary>
        /// Выполнение запроса к серверу в отдельном потоке с обработкой результата в UI потоке
        /// </summary>
        /// <param name="element">UI элемент который блокируется на момент выполнения отдельного потока</param>
        /// <param name="methodName">Название вызываемой на сервере процедуры</param>
        /// <param name="onCompleteRunUiMethod">Метода обрабатывающий результат. В UI потоке</param>
        /// <param name="isContinueOnException">Выполнять onCompleteRunUiMethod метод если сервис вернет ошибку</param>
        /// <param name="serviceType">Тип вызываемого сервиса</param>
        /// <param name="args">Аргументы передаваемы на сервис</param>
        public static void RunBackgroundAsync(this FrameworkElement element, string methodName, Action onCompleteRunUiMethod,
           bool isContinueOnException, EnumServiceType serviceType, params object[] args)
        {
            element.IsEnabled = false;
            Task.Factory.StartNew(() =>
            {
                try
                {
                    switch (serviceType)
                    {
                        case EnumServiceType.ArmService:
                            ServiceFactory.ArmServiceInvokeSync<Exception>(methodName, args);
                            break;
                        case EnumServiceType.FreeHierarchy:
                            ServiceFactory.FreeHierarchyInvokeSync<Exception>(methodName, args);
                            break;
                        case EnumServiceType.StimulReport:
                            ServiceFactory.StimulReportInvokeSync<Exception>(methodName, args);
                            break;
                        default:
                            return;
                    }

                    isContinueOnException = true;
                }
                catch (Exception ex)
                {
                    Manager.UI.ShowMessage(ex.Message);
                }

                element.Dispatcher.BeginInvoke((Action)(
                    () =>
                    {
                        if (isContinueOnException && onCompleteRunUiMethod != null)
                        {
                            try
                            {
                                onCompleteRunUiMethod();
                            }
                            catch (Exception ex)
                            {
                                Manager.UI.ShowMessage(ex.Message);
                            }
                        }

                        element.IsEnabled = true;

                    }), DispatcherPriority.Send);
            });
        }


        public static void RunBackgroundAsync<T>(this FrameworkElement element, Task<T> task,
            Action<T> onCompleteRunUiMethod, Func<Exception, string> exceptionConverter = null)
        {
            ContentControl topPanel = null;
            ContentControl innerPanel = null;
            var managerContainer = element as UIManagerContainer ?? element.FindParent<UIManagerContainer>();
            if (managerContainer != null)
            {
                topPanel = WaitPanel.FindAndAddContentControl(managerContainer);
                if (topPanel != null)
                {
                    if (topPanel.Content != null && !(topPanel.Content is BusyIndicator))
                    {
                        innerPanel = topPanel;
                        innerPanel.IsEnabled = false;
                        topPanel = WaitPanel.AddNewContentControl(managerContainer);
                    }


                    if (topPanel != null && topPanel.Content == null)
                    {
                        topPanel.Content = new BusyIndicator();
                    }
                }

                managerContainer.IsEnabled = false;
            }
            else
            {
                element.IsEnabled = false;
            }

            Task.Factory.StartNew(() =>
            {
                T result;
                string exMessage = null;
                try
                {
                    result = task.Result;
                }
                catch (AggregateException aex)
                {
                    if (exceptionConverter != null) exMessage = exceptionConverter(aex);
                    else
                    {
                        foreach (var ex in aex.Flatten().InnerExceptions)
                        {
                            exMessage += ex.Message;
                        }
                    }

                    result = default(T);
                }
                catch (Exception ex)
                {
                    if (exceptionConverter != null) exMessage = exceptionConverter(ex);
                    else
                    {
                        exMessage = ex.Message;
                    }

                    result = default(T);
                }

                element.Dispatcher.BeginInvoke((Action) (
                    () =>
                    {
                        if (topPanel != null && topPanel.Content is BusyIndicator)
                        {
                            topPanel.Content = null;
                            if (managerContainer != null)
                            {
                                managerContainer.Children.Remove(topPanel);
                                try
                                {
                                    managerContainer.UnregisterName(topPanel.Name);
                                }
                                catch
                                {
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(exMessage))
                        {
                            Manager.UI.ShowLocalMessage(exMessage, managerContainer);
                        }

                        if (onCompleteRunUiMethod != null)
                        {
                            try
                            {
                                onCompleteRunUiMethod(result);
                            }
                            catch (Exception ex)
                            {
                                Manager.UI.ShowLocalMessage(ex.Message, managerContainer);
                            }
                        }

                        //onCompleteRunUiMethod = null;

                        if (innerPanel != null) innerPanel.IsEnabled = true;

                        if (managerContainer != null) managerContainer.IsEnabled = true;
                        else element.IsEnabled = true;

                    }), DispatcherPriority.Send);
            });
        }

        public static T FindAllElementsInHostCoordinates<T>(Point intersectingPoint, UIElement subTree, UIElement rootElement) where T : class
        {
            var count = VisualTreeHelper.GetChildrenCount(subTree);

            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(subTree, i) as FrameworkElement;

                if (child == null) continue;

                var gt = child.TransformToVisual(rootElement);
                var offset = gt.Transform(new Point(0, 0));
                var elementBounds = new Rect(offset.X, offset.Y, child.ActualWidth, child.ActualHeight);

                if (IsInBounds(intersectingPoint, elementBounds))
                {
                    var t = child as T;
                    if (t != null) return t;
                }

                {
                    var t = FindAllElementsInHostCoordinates<T>(intersectingPoint, child, rootElement);
                    if (t != null) return t;
                }
            }

            return null;
        }

        private static bool IsInBounds(Point point, Rect bounds)
        {
            return point.X > bounds.Left && point.X < bounds.Right &&
                   point.Y < bounds.Bottom && point.Y > bounds.Top;
        }

        /// <summary>
        /// Выставляем на форме сохраненные начальные значения
        /// </summary>
        /// <param name="module">Модуль</param>
        /// <param name="settingsCompressed">Сжатые настройки</param>
        /// <param name="rightFilter">Фильтр для построения дерева</param>
        /// <param name="useBase64"></param>
        public static bool SetModuleSettings(FrameworkElement module, byte[] settingsCompressed, EnumModuleFilter? rightFilter = null,
            bool useBase64 = false, bool isSelectHierarchyTreeObjectFromPreviousSelected = true)
        {
            if (module == null || settingsCompressed == null) return false;

            string settings;
            try
            {
                settings = CompressUtility.Unzip(settingsCompressed, useBase64);
            }
            catch (Exception ex)
            {
                Manager.UI.ShowMessage(ex.Message);
                return false;
            }

            if (string.IsNullOrEmpty(settings)) return false;

            var dict = CommonEx.ParseSettings(settings);

            string param;
            if (dict.TryGetValue("dateStart", out param))
            {
                DateTime dt;
                if (DateTime.TryParse(param, new CultureInfo("en-US", true), DateTimeStyles.None, out dt))
                {
                    var dateStart = module.FindName("dateStart") as Xceed.Wpf.Controls.DatePicker;
                    if (dateStart != null)
                    {
                        dateStart.SelectedDate = dt;
                    }
                    else
                    {
                        var my = module.FindName("monthYear") as MonthYear;
                        if (my != null)
                        {
                            my.SelectedDate = dt;
                        }
                        else
                        {
                            var tmy = module.FindName("twoMonthYear") as TwoMonthYear;
                            if (tmy != null)
                            {
                                tmy.dpStart.SelectedDate = dt;
                            }
                        }
                    }
                }
            }

            if (dict.TryGetValue("dateEnd", out param))
            {
                DateTime dt;
                if (DateTime.TryParse(param, new CultureInfo("en-US", true), DateTimeStyles.None, out dt))
                {
                    var dateEnd = module.FindName("dateEnd") as Xceed.Wpf.Controls.DatePicker;
                    if (dateEnd != null)
                    {
                        dateEnd.SelectedDate = dt;
                    }
                    else
                    {
                        var tmy = module.FindName("twoMonthYear") as TwoMonthYear;
                        if (tmy != null)
                        {
                            tmy.dpFinish.SelectedDate = dt;
                        }
                    }
                }
            }

            foreach (var paramName in new[] {"TimeSelectStart", "TimeSelectEnd"})
            {
                if (dict.TryGetValue(paramName, out param))
                {
                    var fe = module.FindName(paramName);
                    if (fe != null)
                    {
                        TimeSpan ts;
                        TimeSpan.TryParse(param, new CultureInfo("en-US", true), out ts);

                        var tsc = fe as TimeSpanComboBox;
                        if (tsc != null)
                        {
                            tsc.SelectedTime = ts;
                        }
                        else
                        {
                            var timeStart = fe as ComboBox;
                            if (timeStart != null)
                            {
                                try
                                {
                                    timeStart.SelectedValue = ts;
                                }
                                catch
                                {
                                }
                            }
                        }
                    }
                }
            }

            if (isSelectHierarchyTreeObjectFromPreviousSelected && dict.TryGetValue("tree", out param) && !string.IsNullOrEmpty(param))
            {
                var dm = module as IDeviceManage;
                if (dm == null || !dm.IsInitializet)
                {
                    var tree = module.FindName("tree") as FreeHierarchyTree;
                    if (tree != null)
                    {
                        short versionNumber = 0;
                        string svn;
                        if (dict.TryGetValue("treeSelectedSetVersionNumber", out svn) && !string.IsNullOrEmpty(svn))
                        {
                            short.TryParse(svn, out versionNumber);
                        }

                        object sets = null;
                        if (versionNumber < 2)
                        {
                            sets = param.Split('■').Take(1000).ToList();
                        }
                        else if (versionNumber == 2)
                        {
                            sets = param;
                        }

                        tree.SetItemsForSelection(sets, versionNumber);
                        //tree.LoadTypes(rightFilter);
                    }
                }
            }

            if (dict.TryGetValue("cbDiscreteType", out param) && !string.IsNullOrEmpty(param))
            {
                var cbDiscreteType = module.FindName("cbDiscreteType") as ComboBox;
                if (cbDiscreteType != null && cbDiscreteType.ItemsSource != null)
                {
                    enumTimeDiscreteType discreteType;
                    if (Enum.TryParse(param, out discreteType))
                    {
                        try
                        {
                            cbDiscreteType.SelectedValue = discreteType;
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
            }

            if (dict.TryGetValue("cbTimeZones", out param) && !string.IsNullOrEmpty(param))
            {
                var cbTimeZones = module.FindName("cbTimeZones") as ComboBox;
                if (cbTimeZones != null && cbTimeZones.ItemsSource != null)
                {
                    try
                    {
                        cbTimeZones.SelectedValue = param;
                    }
                    catch (Exception)
                    {

                    }
                }
            }

            var moduleSettings = module as IModuleSettings;
            if (moduleSettings == null) return true;

            if (dict.TryGetValue("SelectedUnitDigit", out param))
            {
                EnumUnitDigit unitDigit;
                if (Enum.TryParse(param, out unitDigit) && Enum.IsDefined(typeof(EnumUnitDigit), unitDigit))
                {
                    moduleSettings.SelectedUnitDigit = unitDigit;
                }
            }

            if (dict.TryGetValue("SelectedUnitDigitIntegrals", out param))
            {
                EnumUnitDigit unitDigit;
                if (Enum.TryParse(param, out unitDigit) && Enum.IsDefined(typeof(EnumUnitDigit), unitDigit))
                {
                    moduleSettings.SelectedUnitDigitIntegrals = unitDigit;
                }
            }

            if (dict.TryGetValue("DiscreteType", out param))
            {
                enumTimeDiscreteType discreteType;
                if (Enum.TryParse(param, out discreteType) && Enum.IsDefined(typeof(enumTimeDiscreteType), discreteType))
                {
                    moduleSettings.DiscreteType = discreteType;
                }
            }

            moduleSettings.SetSettings(dict);
            return true;
        }

        /// <summary>
        /// Читаем настройки формы, сжимаем.
        /// </summary>
        /// <param name="module">Модуль</param>
        /// <returns>Настройки в сжатом виде</returns>
        public static byte[] GetModuleSettings(FrameworkElement module)
        {
            if (module == null) return null;

            var result = new StringBuilder();

            var dateStart = module.FindName("dateStart") as Xceed.Wpf.Controls.DatePicker;
            if (dateStart != null && dateStart.SelectedDate.HasValue)
            {
                result.Append("dateStart").Append("═").Append(dateStart.SelectedDate.Value.ToString(new CultureInfo("en-US", true))).Append("¤");
            }
            else
            {
                var my = module.FindName("monthYear") as MonthYear;
                if (my != null)
                {
                    result.Append("dateStart").Append("═").Append(my.SelectedDate.Value.ToString(new CultureInfo("en-US", true))).Append("¤");
                }
                else
                {
                    var tmy = module.FindName("twoMonthYear") as TwoMonthYear;
                    if (tmy != null)
                    {
                        if (tmy.dpStart.SelectedDate.HasValue)
                        {
                            result.Append("dateStart").Append("═").Append(tmy.dpStart.SelectedDate.Value.ToString(new CultureInfo("en-US", true))).Append("¤");
                        }

                        if (tmy.dpFinish.SelectedDate.HasValue)
                        {
                            result.Append("dateEnd").Append("═").Append(tmy.dpFinish.SelectedDate.Value.ToString(new CultureInfo("en-US", true))).Append("¤");
                        }
                    }
                }
            }

            var dateEnd = module.FindName("dateEnd") as Xceed.Wpf.Controls.DatePicker;
            if (dateEnd != null && dateEnd.SelectedDate.HasValue)
            {
                result.Append("dateEnd").Append("═").Append(dateEnd.SelectedDate.Value.ToString(new CultureInfo("en-US", true))).Append("¤");
            }

            var timeStart = module.FindName("TimeSelectStart") as ComboBox;
            if (timeStart != null && timeStart.SelectedValue is TimeSpan)
            {
                result.Append("TimeSelectStart").Append("═").Append(((TimeSpan)timeStart.SelectedValue).ToString("c", new CultureInfo("en-US", true))).Append("¤");
            }
            else
            {
                var timeSpanStart = module.FindName("TimeSelectStart") as TimeSpanComboBox;
                if (timeSpanStart != null && timeSpanStart.SelectedTime.HasValue)
                {
                    result.Append("TimeSelectStart").Append("═").Append((timeSpanStart.SelectedTime.Value).ToString("c", new CultureInfo("en-US", true))).Append("¤");
                }
            }

            var timeEnd = module.FindName("TimeSelectEnd") as ComboBox;
            if (timeEnd != null && timeEnd.SelectedValue is TimeSpan)
            {
                result.Append("TimeSelectEnd").Append("═").Append(((TimeSpan)timeEnd.SelectedValue).ToString("c", new CultureInfo("en-US", true))).Append("¤");
            }
            else
            {
                var timeSpanEnd = module.FindName("TimeSelectEnd") as TimeSpanComboBox;
                if (timeSpanEnd != null && timeSpanEnd.SelectedTime.HasValue)
                {
                    result.Append("TimeSelectEnd").Append("═").Append((timeSpanEnd.SelectedTime.Value).ToString("c", new CultureInfo("en-US", true))).Append("¤");
                }
            }

            var tree = module.FindName("tree") as FreeHierarchyTree;
            if (tree != null)
            {
                var sets = tree.GetSelectedToSet();

                result.Append("treeSelectedSetVersionNumber").Append("═").Append(2).Append("¤");
                result.Append("tree").Append("═").Append(sets).Append("¤");
            }

            var cbDiscreteType = module.FindName("cbDiscreteType") as ComboBox;
            if (cbDiscreteType != null && cbDiscreteType.SelectedValue is enumTimeDiscreteType)
            {
                result.Append("cbDiscreteType").Append("═").Append(((enumTimeDiscreteType)cbDiscreteType.SelectedValue)).Append("¤");
            }

            var cbTimeZones = module.FindName("cbTimeZones") as ComboBox;
            if (cbTimeZones != null && cbTimeZones.SelectedValue is string)
            {
                result.Append("cbTimeZones").Append("═").Append(((string)cbTimeZones.SelectedValue)).Append("¤");
            }
            
            var moduleSettings = module as IModuleSettings;
            if (moduleSettings != null)
            {
                result.Append("SelectedUnitDigit").Append("═").Append(moduleSettings.SelectedUnitDigit).Append("¤");
                result.Append("SelectedUnitDigitIntegrals").Append("═").Append(moduleSettings.SelectedUnitDigitIntegrals).Append("¤");
                result.Append("DiscreteType").Append("═").Append(moduleSettings.DiscreteType).Append("¤");

                moduleSettings.GetSettings(result);
            }

            return result.Length == 0 ? null : CompressUtility.Zip(result.ToString());
        }

        public static T GetVisualParent<T>(this DependencyObject child) where T : class
        {
            while ((child != null) && !(child is T))
            {
                child = VisualTreeHelper.GetParent(child);
            }
            return child as T;
        }

        public static T GetParentDataContext<T>(this FrameworkElement child) where T : class
        {
            while (child != null && child.DataContext!=null && !(child.DataContext is T))
            {
                child = VisualTreeHelper.GetParent(child) as FrameworkElement;
            }

            if (child == null) return null;

            return child.DataContext as T;
        }

        /// <summary>
        /// Регистрируем все кнтролы из optionPanel для чтения и сохранения настроек
        /// </summary>
        public static void RegisterNamesFromChildDown(FrameworkElement parent, FrameworkElement child)
        {
            if (parent == null || child == null) return;

            var children = LogicalTreeHelper.GetChildren(child);
            foreach (var c in children)
            {
                var cc = c as FrameworkElement;
                if (cc == null) continue;

                if (!string.IsNullOrEmpty(cc.Name) && parent.FindName(cc.Name) == null)
                {
                    parent.RegisterName(cc.Name, cc);
                }

                RegisterNamesFromChildDown(parent, cc);
            }
        }

        public static void SetWatermark(DatePicker dp)
        {
            var fiTextBox = typeof(DatePicker).GetField("_textBox", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fiTextBox == null) return;

            var dateTextBox = fiTextBox.GetValue(dp) as DatePickerTextBox;
            if (dateTextBox == null) return;

            var piWatermark = dateTextBox.GetType().GetProperty("Watermark", BindingFlags.Instance | BindingFlags.NonPublic);
            if (piWatermark == null) return;

            piWatermark.SetValue(dateTextBox, "Месяц, год", null);

        }

        public static FrameworkElement FindParent(FrameworkElement element, Type parentType)
        {
            return element.FindParent<FrameworkElement>();
        }
        
        #region Кнопки сернуть/развернуть

        public static readonly DependencyProperty IsCollapseEnabledProperty = DependencyProperty.RegisterAttached(
            "IsCollapseEnabled", typeof(bool), typeof(VisualEx));

        public static void SetIsCollapseEnabled(UIElement element, bool value)
        {
            element.SetValue(IsCollapseEnabledProperty, value);
        }

        public static bool GetIsCollapseEnabled(UIElement element)
        {
            return (bool)element.GetValue(IsCollapseEnabledProperty);
        }

        #endregion

        #region Кнопки повернуть на 90

        public static readonly DependencyProperty IsPivotEnabledProperty = DependencyProperty.RegisterAttached(
            "IsPivotEnabled", typeof(bool), typeof(VisualEx));

        public static void SetIsPivotEnabled(UIElement element, bool value)
        {
            element.SetValue(IsPivotEnabledProperty, value);
        }

        public static bool GetIsPivotEnabled(UIElement element)
        {
            return (bool)element.GetValue(IsPivotEnabledProperty);
        }

        #endregion

        #region Кнопки настройки итогов для грида

        public static readonly DependencyProperty IsSummaryOptionsEnabledProperty = DependencyProperty.RegisterAttached(
            "IsSummaryOptionsEnabled", typeof(bool), typeof(VisualEx));

        public static void SetIsSummaryOptionsEnabled(UIElement element, bool value)
        {
            element.SetValue(IsSummaryOptionsEnabledProperty, value);
        }

        public static bool GetIsSummaryOptionsEnabled(UIElement element)
        {
            return (bool)element.GetValue(IsSummaryOptionsEnabledProperty);
        }

        #endregion

        #region Расширение для Button


        public static readonly DependencyProperty ModuleDrawingBrushNameProperty = DependencyProperty.RegisterAttached(
            "ModuleDrawingBrushName", typeof(string), typeof(VisualEx));

        public static void SetModuleDrawingBrushName(DependencyObject element, string value)
        {
            element.SetValue(ModuleDrawingBrushNameProperty, value);
        }

        public static string GetModuleDrawingBrushName(DependencyObject element)
        {
            return element.GetValue(ModuleDrawingBrushNameProperty) as string;
        }

        public static string GetSenderDrawingBrushName(object sender)
        {
            var fe = sender as DependencyObject;
            if (fe != null)
            {
                try
                {
                    return GetModuleDrawingBrushName(fe);
                }
                catch
                {
                    //Обрабатывать ошибку не нужно
                }
            }

            return null;
        }

        #endregion

        public static bool TryGetUserControl(string folderName, string pageName, out UserControl control)
        {
            control = null;

            try
            {
                var elementType = Type.GetType("Proryv.ElectroARM.Controls.UI." + folderName + "." + pageName, true,
                    true);
                control = Activator.CreateInstance(elementType) as UserControl;
            }
            catch (Exception ex)
            {
                Manager.UI.ShowMessage("Ошибка чтения логотипа: " + ex.Message);
            }

            return control != null;
        }

        public static List<UserControl> GetUserControlsInFolder(string folderName)
        {
            var userControls = new List<UserControl>();
            var assembly = typeof(LoginPage).Assembly;
            var resourceName = assembly.GetName().Name + ".g.resources";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new System.Resources.ResourceReader(stream))
            {
                var firstInner = folderName + '/';

                foreach (System.Collections.DictionaryEntry entry in reader)
                {
                    var key = entry.Key.ToString();
                    if (!key.EndsWith(".baml")) continue;

                    var pFrom = key.IndexOf(firstInner) + firstInner.Length;
                    if (pFrom < 0) continue;

                    var pTo = key.LastIndexOf('/');
                    if (pTo < 0) continue;

                    var subFolderName = key.Substring(pFrom, pTo - pFrom);

                    if (string.Equals(subFolderName, "common", StringComparison.InvariantCultureIgnoreCase) ||
                        string.Equals(subFolderName, "old", StringComparison.InvariantCultureIgnoreCase)) continue;

                    try
                    {
                        var elementType = Type.GetType("Proryv.ElectroARM.Controls." + folderName + "." + subFolderName + "." + "LoginPage",
                            true,
                            true);
                        userControls.Add(Activator.CreateInstance(elementType) as UserControl);
                    }
                    catch (Exception ex)
                    {
                        Manager.UI.ShowMessage("Ошибка чтения логотипа: " + ex.Message);
                    }

                }
            }
            return userControls;
        }

    }

    public class ResizeHelper
    {
        internal bool? isLeft = null, isTop = null;
        internal Point old;
        internal bool isRigthDown = true;
    }

    public class THierarchyDbTreeObjectComparer : IComparer
    {
        public THierarchyDbTreeObjectComparer()
        {
        }

        public int Compare(object x, object y)
        {
            if ((x is THierarchyDbTreeObject) && (y is THierarchyDbTreeObject))
            {
                THierarchyDbTreeObject xH = x as THierarchyDbTreeObject;
                THierarchyDbTreeObject yH = y as THierarchyDbTreeObject;

                if (xH.Type > yH.Type) return 1;
                if (xH.Type < yH.Type) return -1;
                if (xH.Id > yH.Id) return 1;
                if (xH.Id < yH.Id) return -1;

                return 0;
            }

            throw new ArgumentException();
        }
    }

    /// <summary>
    /// Для сравнения и нахождения уникальных
    /// </summary>
    public class THierarchyDbTreeObjectEqualityComparer : IEqualityComparer<THierarchyDbTreeObject>
    {
        public bool Equals(THierarchyDbTreeObject x, THierarchyDbTreeObject y)
        {
            return ((x.Id == y.Id) && (x.ParentId == y.ParentId));
        }

        public int GetHashCode(THierarchyDbTreeObject obj)
        {
            string s = String.Format("{0}{1}", obj.Id, obj.ParentId);
            return s.GetHashCode();
        }
    }

    /// <summary>
    /// Для возврата результатов из модального диалога
    /// </summary>
    public interface IModalCompleted
    {
        void OnClosedModalAction(params object[] args);
    }

    /// <summary>
    /// для того чтобы из основного окна можно было запросить результат модального окна
    /// </summary>
    public interface IModalResult
    {
        object[] GetResult();
    }

}
