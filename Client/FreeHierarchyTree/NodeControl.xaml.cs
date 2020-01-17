using System;
using System.Collections.Generic;
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
using System.Windows.Threading;
using Infragistics.Controls.Menus;
using Proryv.AskueARM2.Client.Visual;
using Proryv.AskueARM2.Client.Visual.Common.FreeHierarchy;

namespace Proryv.ElectroARM.Controls.Controls.FreeHierarchyTree
{
    /// <summary>
    /// Логика взаимодействия для NodeControl.xaml
    /// </summary>
    public partial class NodeControl 
    {
        public NodeControl()
        {
            InitializeComponent();

            _initVisualTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(7), DispatcherPriority.Input, OnInitVisualTimerTick, Dispatcher);
            _initVisualTimer.Stop();
        }

        private void OnNodeDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext == null) return;

            _initVisualTimer.Start();
        }

        private void OnInitVisualTimerTick(object sender, EventArgs e)
        {
            _initVisualTimer.Stop();

            var nodeContext = DataContext as XamDataTreeNodeDataContext;
            if (nodeContext == null) return;

            var parentTree = nodeContext.Node.NodeLayout.Tree.Tag as FreeHierarchyTree;

            //Content = new FreeItem(nodeContext.Data as FreeHierarchyTreeItem, parentTree);
        }

        private readonly DispatcherTimer _initVisualTimer;

        private void OnNodeUnloaded(object sender, RoutedEventArgs e)
        {
            _initVisualTimer.Stop();

            var disposable = Content as IDisposable;
            if (disposable!=null) disposable.Dispose();
        }
    }
}
