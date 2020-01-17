using Proryv.AskueARM2.Both.VisualCompHelpers;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Proryv.ElectroARM.Controls.Controls.FreeHierarchyTree.TreeSelector
{
    /// <summary>
    /// Логика взаимодействия для FreeHierarchyTreeSelectedObjectsControl.xaml
    /// </summary>
    public partial class FreeHierarchyTreeSelectedObjectsControl : INotifyPropertyChanged
    {
        private string _test;
        public string Text
        {
            set
            {
                if (string.Equals(_test, value)) return;

                _test = value;
                _PropertyChanged("Text");
            }
            get { return _test; }
        }

        public FreeHierarchyTreeSelectedObjectsControl()
        {
            InitializeComponent();
            Init(dontUseRightClick: true);
        }

        protected override UserControl GetPopupControl()
        {
            var tree = this.FindParent<FreeHierarchyTree>();
            if (tree == null) return null;

            return new FreeHierarchyTreeSelectedObjectsPopup(tree.GetDescriptor());
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void _PropertyChanged(string property)
        {
            if (PropertyChanged != null && !DesignerProperties.GetIsInDesignMode(this))
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        #endregion
    }
}
