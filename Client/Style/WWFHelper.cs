using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Diagnostics;
using System.Windows.Controls;
using System.Activities.Presentation.Toolbox;

namespace Proryv.AskueARM2.Client.Visual
{
    public partial class WWFHelper
    {
        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var cp = sender as ContentPresenter;
            var tc = cp.DataContext as ToolboxCategory;
            if (tc != null) cp.Content = tc.CategoryName;
        }
    }
}
