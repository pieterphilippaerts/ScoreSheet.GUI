using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
    /// Interaction logic for UpdatingFromInternetPanel.xaml
    /// </summary>
    public partial class UpdatingFromInternetPanel : UserControl {
        public UpdatingFromInternetPanel() {
            InitializeComponent();
            //this.DataContextChanged += UpdatingFromInternetPanel_DataContextChanged;
            
        }
        private void UpdatingFromInternetPanel_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var col = TheListBox.GetValue(ListBox.ItemsSourceProperty) as INotifyCollectionChanged;
            if (col != null) {
                col.CollectionChanged += Col_CollectionChanged;
            }
        }
        private void Col_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (TheListBox.SelectedItem == null && TheListBox.Items.Count > 0)
                TheListBox.ScrollIntoView(TheListBox.Items[TheListBox.Items.Count - 1]);
        }

        private void TheListBox_TargetUpdated(object sender, DataTransferEventArgs e) {
            var col = TheListBox.GetValue(ListBox.ItemsSourceProperty) as INotifyCollectionChanged;
            if (col != null) {
                col.CollectionChanged += Col_CollectionChanged;
            }
        }
    }
}
