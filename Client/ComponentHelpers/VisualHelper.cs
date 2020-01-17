using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Xceed.Wpf.DataGrid;
using System.Windows.Controls;

namespace Proryv.AskueARM2.Both.VisualCompHelpers
{
    using Northwoods.GoXam;
    using System.Windows.Media;

    public static class VisualHelper
    {
        /// <summary>
        /// Найти родителя в дереве UI по его типу
        /// </summary>
        /// <param name="T">Тип искомого родителя</param>
        /// <param name="element">Элемент, относительно которого искать</param>
        /// <returns>Найденный родитель или NULL</returns>
        public static T FindParent<T>(this FrameworkElement element) where T : class
        {
            if (element == null) return null;
            if (element is T) return element as T;
            FrameworkElement temp = null;
            do
            {
                temp = element.Parent as FrameworkElement;
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


        public static List<T> GetLogicalChildCollection<T>(this object parent) where T : DependencyObject
        {
            List<T> logicalCollection = new List<T>();
            GetLogicalChildCollection(parent as DependencyObject, logicalCollection);
            return logicalCollection;
        }

        private static void GetLogicalChildCollection<T>(DependencyObject parent, List<T> logicalCollection) where T : DependencyObject
        {
            IEnumerable children = LogicalTreeHelper.GetChildren(parent);
            foreach (object child in children)
            {
                if (child is DependencyObject)
                {
                    DependencyObject depChild = child as DependencyObject;
                    if (child is T)
                    {
                        logicalCollection.Add(child as T);
                    }
                    GetLogicalChildCollection(depChild, logicalCollection);
                }
            }
        }

        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        /// <summary>
        /// Найти дитя по родителю по типу
        /// </summary>
        /// <typeparam name="T">Тип дитя</typeparam>
        /// <param name="depObj">Родитель</param>
        /// <returns>Найденный дитя ли NULL</returns>
        public static T GetChildOfType<T>(this DependencyObject depObj)
            where T : DependencyObject
        {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;

                //foreach (T childOfChild in FindVisualChildren<T>(child))
                //{
                //    result = (childOfChild as T) ?? GetChildOfType<T>(childOfChild);
                //    if (result != null) return result;
                //}
            }

            return null;
        }

        /// <summary>
        ///  Найти дитя по родителю по типу 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Visual GetDescendantByType(Visual element, Type type)
        {
            if (element == null) return null;
            if (element.GetType() == type) return element;
            Visual foundElement = null;
            if (element is FrameworkElement)
                (element as FrameworkElement).ApplyTemplate();
            for (int i = 0;
                i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                Visual visual = VisualTreeHelper.GetChild(element, i) as Visual;
                foundElement = GetDescendantByType(visual, type);
                if (foundElement != null)
                    break;
            }
            return foundElement;
        }

        #region Кнопка экспорта в Excel

        public static readonly DependencyProperty IsExportToExcelEnabledProperty = DependencyProperty.RegisterAttached(
            "IsExportToExcelEnabled", typeof(bool), typeof(VisualHelper), new PropertyMetadata(true));

        public static void SetIsExportToExcelEnabled(UIElement element, bool value)
        {
            element.SetValue(IsExportToExcelEnabledProperty, value);
        }

        public static bool GetIsExportToExcelEnabled(UIElement element)
        {
            return (bool)element.GetValue(IsExportToExcelEnabledProperty);
        }

        #endregion

        public static string prepareFile(string filter, Action<string> onError)
        {
            return prepareFile(filter, string.Empty, onError);
        }
        /// <summary>
        /// Внимание : после этой процедуры файл на диске остается с нулевым размером , при сохранении нужно его удалять 
        /// </summary>
        /// <param name="ext"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string prepareFile(string ext, string fileName, Action<string> onError)
        {
            using (System.Windows.Forms.SaveFileDialog saveDialog = new System.Windows.Forms.SaveFileDialog())
            {
                saveDialog.DefaultExt = FileAdapter.RemoveBadChar(ext).ToLower();
                saveDialog.Filter = ext.ToUpper() + "-документ (*." + ext.ToLower() + ")|*." + ext.ToLower();
                saveDialog.FilterIndex = 1;
                saveDialog.FileName = (String.IsNullOrEmpty(fileName) ? "Exported" : FileAdapter.RemoveBadChar(fileName)) + "." + saveDialog.DefaultExt;
                saveDialog.OverwritePrompt = true;
                saveDialog.AddExtension = true;
                saveDialog.SupportMultiDottedExtensions = true;

                //TODO сделать чтобы диалог отображался поверх Popup

                if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        if (FileAdapter.VerifyFileIsInUse(saveDialog.FileName, out fileName))
                        {
                            return fileName;
                        }
                        else
                        {
                            throw new Exception("Не удается перезаписать файл!\n Возможно он уже открыт. Закройте его.");
                        }
                    }
                    catch (Exception ex)
                    {
                        if (onError != null) onError(ex.Message);
                        return null;
                    }
                }
                return null;
            }
        }
    }
}
