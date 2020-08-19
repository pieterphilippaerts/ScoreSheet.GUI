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
using PieterP.ScoreSheet.Localization;
using PieterP.Shared.Cells;

namespace PieterP.ScoreSheet.GUI.Views.Wizards {
    /// <summary>
    /// Interaction logic for RestoreBackupsPanel.xaml
    /// </summary>
    public partial class RestoreBackupsPanel : UserControl {
        public RestoreBackupsPanel() {
            this.RestoreText = Cell.Create("#");
            InitializeComponent();
            TheListBox.Focus();
        }

        private void TheListBox_TargetUpdated(object sender, DataTransferEventArgs e) {
            foreach (var i in TheListBox.Items) {
                if (!TheListBox.SelectedItems.Contains(i))
                    TheListBox.SelectedItems.Add(i);
            }
        }

        private void TheListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            int count = TheListBox.SelectedItems.Count;
            if (count == 0) {
                RestoreText.Value = Strings.RestoreBackup_RestoreNothing;
            } else if (count == 1) {
                RestoreText.Value = Strings.RestoreBackup_RestoreOne;
            } else {
                RestoreText.Value = Safe.Format(Strings.RestoreBackup_RestoreMultiple, count);
            }
        }

        public Cell<string> RestoreText { get; private set; }
    }
}
