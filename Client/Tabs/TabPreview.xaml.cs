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

namespace Proryv.AskueARM2.Client.Visual
{
    /// <summary>
    /// Interaction logic for TabPreview.xaml
    /// </summary>
    public partial class TabPreview : UserControl
    {
        public TabPreview()
        {
            InitializeComponent();
        }

        FrameworkElement frame = null;

        public FrameworkElement Frame
        {
            get { return frame; }
            set
            {
                frame = value;
                if (value == null) return;

                value.Visibility = Visibility.Visible;
                content.Source = frame.GetPreview(200, 200);
                value.Visibility = Visibility.Hidden;
            }
        }
    }
}
