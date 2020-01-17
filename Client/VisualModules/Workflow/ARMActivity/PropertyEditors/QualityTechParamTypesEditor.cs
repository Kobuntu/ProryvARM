using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using System.Activities.Presentation.PropertyEditing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Reflection;

namespace Proryv.Workflow.Activity.ARM
{
    public class ParamItem
    {
        public enumArchTechParamType ParamId { get; set; }
        public string ParamName { get; set; }
        public bool IsChecked { get; set; }
    }


    public class ConvertBinding : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            List<enumArchTechParamType> v = value as List<enumArchTechParamType>;

            string s;
            if (v == null)
                s = "Не определено";
            else
                s = "Выбрано параметров : " + v.Count.ToString();

            return s;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    class TechParamTypesEditor : DialogPropertyValueEditor
    {
        public TechParamTypesEditor()
        {
            this.InlineEditorTemplate = new DataTemplate();

            FrameworkElementFactory stack = new FrameworkElementFactory(typeof(StackPanel));
            stack.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

            // button
            FrameworkElementFactory editModeSwitch = new FrameworkElementFactory(typeof(EditModeSwitchButton));
            editModeSwitch.SetValue(EditModeSwitchButton.TargetEditModeProperty, PropertyContainerEditMode.Dialog);
            stack.AppendChild(editModeSwitch);
            // text
            FrameworkElementFactory label = new FrameworkElementFactory(typeof(Label));

            ConvertBinding convertBind = new ConvertBinding();

            Binding labelBinding = new Binding("Value");
            labelBinding.Converter = new ConvertBinding();
            label.SetValue(Label.ContentProperty, labelBinding);

            label.SetValue(Label.MaxWidthProperty, 300.0);
            stack.AppendChild(label);

            this.InlineEditorTemplate.VisualTree = stack;
        }

        public override void ShowDialog(PropertyValue propertyValue, IInputElement commandSource)
        {
            List<enumArchTechParamType> propvalue = (List<enumArchTechParamType>)propertyValue.Value;
            if (propvalue == null)
                propvalue = new List<enumArchTechParamType>();

            Type T = typeof(enumArchTechParamType);
            List<ParamItem> ParamItemList = new List<ParamItem>();

            enumArchTechParamTypeConverter enumconv = new enumArchTechParamTypeConverter();

            string PName;
            foreach (FieldInfo fi in T.GetFields())
            {
                if (!Enum.IsDefined(T, fi.Name)) continue;

                enumArchTechParamType val = (enumArchTechParamType)Enum.Parse(T, fi.Name);
                PName = enumconv.GetDescriprion(val);

                bool found = propvalue.IndexOf(val) > -1;

                ParamItemList.Add(new ParamItem() { ParamId = val, ParamName = PName, IsChecked = found });
            }

            DialogSetTechQualityLastValues dialogcontent = new DialogSetTechQualityLastValues();
            dialogcontent.ListQuality.ItemsSource = ParamItemList;
            if (dialogcontent.ShowOkCancel())
            {
                List<enumArchTechParamType> Result = new List<enumArchTechParamType>();

                foreach (ParamItem p in ParamItemList)
                {
                    if (p.IsChecked)
                        Result.Add(p.ParamId);
                }

                propertyValue.Value = Result;
            }
        }
    }
}
