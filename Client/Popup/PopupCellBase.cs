using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Proryv.AskueARM2.Client.Visual
{
    public class PopupCellBase : PopupBase
    {
        public PopupCellBase() : base()
        {
            Init();
            Loaded += new RoutedEventHandler(PopupableCell_Loaded);
        }

        void PopupableCell_Loaded(object sender, RoutedEventArgs e)
        {
            Content = CellContentTemplate.LoadContent();
            Loaded -= PopupableCell_Loaded;
        }        

        public DataTemplate CellContentTemplate { get; set; }
    }
}
