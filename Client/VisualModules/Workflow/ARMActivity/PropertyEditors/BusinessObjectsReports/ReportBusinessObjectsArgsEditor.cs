using System;
using System.Activities;
using System.Activities.Expressions;
using System.Activities.Presentation.Converters;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.PropertyEditing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Proryv.AskueARM2.Both.VisualCompHelpers;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.Interfaces;
using Proryv.AskueARM2.Client.ServiceReference.Service.Extensions;
using Proryv.AskueARM2.Client.ServiceReference.StimulReportService;
using Proryv.AskueARM2.Client.Visual.Common;
using Proryv.AskueARM2.Client.Visual.Common.Modules;

namespace Proryv.Workflow.Activity.ARM.PropertyEditors.BusinessObjectsReports
{
    internal class ReportBusinessObjectsArgsEditor : DialogPropertyValueEditor
    {
        public class ConvertEditor : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                var propVal = value as string;
                if (propVal != null)
                {
                    if (propVal == string.Empty) return "<Объекты не соответствуют типу отчета!>";

                    MultiPsSelectedArgs args = null;
                    try
                    {
                        args = propVal.DeserializeFromString<MultiPsSelectedArgs>();
                    }
                    catch (Exception ex)
                    {
                        
                    }

                    if (args == null) return "<Ошибка десериализации>";

                    var pss = args.PSList;
                    if (pss != null && pss.Count > 0)
                    {
                        var n = new StringBuilder();
                        foreach (var id in pss.Take(3))
                        {
                            if (id.ID == -1)
                            {
                                n.Append("<Все >");
                                break;
                            }

                            var hierObject = id.ToHierarchyObject();
                            if (hierObject == null) continue;
                            n.Append(hierObject.Name).Append(", ");
                            if (n.Length > 400) break;
                        }

                        if (n.Length > 2) n.Remove(n.Length - 2, 1);

                        if (pss.Count > 3) n.Append("...");
                        return n.ToString();

                    }

                    //Manager.UI.ShowMessage("Выберите объекты!");
                    return "< Объекты не выбраны, или не нужны для данного отчета >";
                }
                return "";
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                return new InArgument<object[]>(value as object[]);
            }
        }

        public ReportBusinessObjectsArgsEditor()
        {
            this.InlineEditorTemplate = new DataTemplate();

            FrameworkElementFactory stack = new FrameworkElementFactory(typeof(StackPanel));
            stack.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

            // button
            FrameworkElementFactory editModeSwitch = new FrameworkElementFactory(typeof(EditModeSwitchButton));
            editModeSwitch.SetValue(EditModeSwitchButton.TargetEditModeProperty, PropertyContainerEditMode.Dialog);
            stack.AppendChild(editModeSwitch);
            // text
            FrameworkElementFactory textBox = new FrameworkElementFactory(typeof(TextBox));

            Binding textBoxBinding = new Binding("Value") {Converter = new ConvertEditor(), Mode = BindingMode.OneWay, };
            textBox.SetValue(TextBox.TextProperty, textBoxBinding);
            textBox.SetValue(TextBoxBase.IsReadOnlyProperty, true);

            //textBox.SetValue(Label.MaxWidthProperty, 300.0);
            stack.AppendChild(textBox);

            this. InlineEditorTemplate.VisualTree = stack;
        }

        public override void ShowDialog(PropertyValue propertyValue, IInputElement commandSource)
        {
            ModelPropertyEntryToOwnerActivityConverter ownerActivityConverter = new ModelPropertyEntryToOwnerActivityConverter();
            ModelItem activityItem = ownerActivityConverter.Convert(propertyValue.ParentProperty, typeof(ModelItem), false, null) as ModelItem;

            //var av = activityItem.GetCurrentValue() as SendBusinessObjectsReportToEmail;
            //if (av == null) return;

            string reportUn = string.Empty;
            var av = activityItem.GetCurrentValue();
            if (av == null) return;

            var rv = av.GetType().GetProperty("Report_id").GetValue(av, null);
            if (rv != null)
            {
                var rvl = rv as InArgument<string>;
                if (rvl == null) return;
                var literal = rvl.Expression as Literal<string>;
                if (literal == null) return;
                reportUn = literal.Value;
            }


            //if (av.Report_id != null)
            //{
            //    var literal = av.Report_id.Expression as Literal<string>;
            //    if (literal == null) return;
            //    reportUn = literal.Value;
            //}

            if (string.IsNullOrEmpty(reportUn))
            {
                Manager.UI.ShowMessage("Сначала необходимо выбрать отчет!");
                return;
            }

            var fe = commandSource as FrameworkElement;
            if (fe!=null)
            {
                _owner = fe.FindParent<IModule>();
            }

            Manager.UI.RunAsync(report =>
            {
                string businessObjectName = string.Empty;
                try
                {
                    businessObjectName = ServiceFactory.StimulReportInvokeSync<string>("GetUsedBusinessObjectsNames", reportUn);
                }
                catch (Exception ex)
                {
                    Manager.UI.ShowMessage(ex.Message);
                }

                return businessObjectName;

            }, businessObjectName =>
            {
                if (string.IsNullOrEmpty(businessObjectName))
                {
                    //Это отчет без бизнес объекта
                    Manager.UI.RunAsync(args => ServiceFactory.StimulReportInvokeSync<TReportInfo>("LoadReport", reportUn)
                        , response =>
                        {
                            if (response == null) return;

                            if (response.TreeMode == enumTreeMode.None && !response.IsShowChannelSelector)
                            {
                                //Аргументы не нужны
                                propertyValue.Value = new MultiPsSelectedArgs().SerializeToString<MultiPsSelectedArgs>();
                                return;
                            }

                            string title;
                            var selector = Manager.Modules.CreateModule(ModuleType.MultiPSSelector, out title, response.DateRangeMode, (enumTreeMode)response.TreeMode, response.IsShowChannelSelector);
                            selector.Width = SystemParameters.PrimaryScreenWidth - 50;
                            selector.Height = SystemParameters.PrimaryScreenHeight - 30;
                            ShowMultiPsSelector(propertyValue, selector, title);
                        }, null);
                }
                else
                {
                    string title;
                    var selector = Manager.Modules.CreateModule(ModuleType.MultiPSSelector, out title, businessObjectName);
                    selector.Width = SystemParameters.PrimaryScreenWidth - 50;
                    selector.Height = SystemParameters.PrimaryScreenHeight - 30;
                    ShowMultiPsSelector(propertyValue, selector, title);
                }
            }, reportUn);
        }

        private IModule _owner;

        private void ShowMultiPsSelector(PropertyValue propertyValue, FrameworkElement selector, string title)
        {
            var dialog = new ReportBusinessObjectsArgsDialog();
            dialog.ccMultiPSSelector.Content = selector;
            dialog.Title = title;
            dialog.Owner = _owner as DependencyObject;

            RestoreTreeSettings(selector);

            if (dialog.ShowOkCancel())
            {
                var iArgs = selector as IModuleArgs;
                if (iArgs != null)
                {
                    propertyValue.Value = iArgs.GetArgs().SerializeToString<MultiPsSelectedArgs>();
                    /* todo Доработать
                    Literal<string> literal;
                    if (av.AttachName == null || av.AttachName.Expression == null || (literal = av.AttachName.Expression as Literal<string>) == null || string.IsNullOrEmpty(literal.Value))
                    {
                        av.AttachName = new InArgument<string>(businessObjectName + ".xls");
                    }
                     * */


                    SaveTreeSettings(selector);
                }
            }
        }

        private void RestoreTreeSettings(FrameworkElement selector)
        {
            byte[] settingsCompressed = null;
            if (Manager.Config.ModulesSettingsCompressed != null)
            {
                Manager.Config.ModulesSettingsCompressed.TryGetValue(
                    "MultiPSSelector: Workflow", out settingsCompressed);
            }

            if (settingsCompressed != null)
            {
                var useBase64 = false;
                try
                {
                    string settings;
                    try
                    {
                        settings = CompressUtility.Unzip(settingsCompressed, useBase64);
                    }
                    catch (Exception ex)
                    {
                        Manager.UI.ShowMessage(ex.Message);
                        return;
                    }

                    if (string.IsNullOrEmpty(settings)) return;

                    string param;
                    var dict = AskueARM2.Client.Visual.Common.CommonEx.ParseSettings(settings);
                    if (dict.TryGetValue("tree", out param) && !string.IsNullOrEmpty(param))
                    {
                        var tree = selector.FindName("tree") as IFreeHierarchyTree;
                        if (tree != null)
                        {
                            var sets = param.Split('■').Take(1000).ToList();

                            short versionNumber = 0;
                            string svn;
                            if (dict.TryGetValue("treeSelectedSetVersionNumber", out svn) && !string.IsNullOrEmpty(svn))
                            {
                                short.TryParse(svn, out versionNumber);
                            }

                            tree.SetItemsForSelection(sets, versionNumber);
                            //tree.LoadTypes(rightFilter);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (Manager.UI != null)
                        Manager.UI.ShowMessage("Ошибка восстановления настроек модуля: " + ex.Message);
                }
            }
        }

        private void SaveTreeSettings(FrameworkElement selector)
        {
            var result = new StringBuilder();

            var tree = selector.FindName("tree") as IFreeHierarchyTree;
            if (tree != null)
            {
                try
                {
                    var sets = tree.GetSelectedToSet();

                    result.Append("treeSelectedSetVersionNumber").Append("═").Append(2).Append("¤");

                    result.Append("tree").Append("═").Append(sets).Append("¤");
                }
                catch (Exception ex)
                {
                    if (Manager.UI != null)
                        Manager.UI.ShowMessage("Ошибка сохранения настроек модуля: " + ex.Message);
                }
            }

            var settings = result.Length == 0 ? null : CompressUtility.Zip(result.ToString());

            Manager.Config.SaveModulesSettingsCompressed("MultiPSSelector: Workflow", settings);
        }
    }
}
