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
using PieterP.Shared.Cells;

namespace PieterP.ScoreSheet.GUI.Views.Wizards {
    /// <summary>
    /// Interaction logic for SelectMatchesPanel.xaml
    /// </summary>
    public partial class SelectMatchesPanel : UserControl {
        public SelectMatchesPanel() {
            this.ExportCount = Cell.Create(0);
            InitializeComponent();
            TheListBox.Focus();
        }
        private void TheListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ExportCount.Value = TheListBox.SelectedItems.Count;
            TheExportButton.IsEnabled = ExportCount.Value > 0;
        }

        public Cell<int> ExportCount { get; private set; }
    }
}
