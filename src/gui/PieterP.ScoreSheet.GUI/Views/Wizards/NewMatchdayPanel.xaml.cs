using System;
using System.Collections.Generic;
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

namespace PieterP.ScoreSheet.GUI.Views.Wizards {
    /// <summary>
    /// Interaction logic for NewMatchdayPanel.xaml
    /// </summary>
    public partial class NewMatchdayPanel : UserControl {
        public NewMatchdayPanel() {
            InitializeComponent();
        }

        private void TreeViewSelectedItemChanged(object sender, RoutedEventArgs e) {
            var item = sender as TreeViewItem;
            if (item != null) {
                item.BringIntoView();
                e.Handled = true;
            }
        }
    }
}