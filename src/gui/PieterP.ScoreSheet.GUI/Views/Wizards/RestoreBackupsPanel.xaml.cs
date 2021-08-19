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
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model.Database.Entities;
using PieterP.ScoreSheet.ViewModels.Wizards;
using PieterP.Shared.Cells;

namespace PieterP.ScoreSheet.GUI.Views.Wizards {
    /// <summary>
    /// Interaction logic for RestoreBackupsPanel.xaml
    /// </summary>
    public partial class RestoreBackupsPanel : UserControl {
        private RestoreBackupsViewModel _viewModel;

        public RestoreBackupsPanel() {
            InitializeComponent();
            TheListBox.Focus();
        }

        #region Properties

        /// <summary>
        /// The current RestoreBackupsViewModel.
        /// </summary>
        private RestoreBackupsViewModel ViewModel => _viewModel ??= DataContext as RestoreBackupsViewModel;

        #endregion

        #region Event handlers

        /// <summary>
        /// Event handler for when the target of the list box is updated.
        /// Ensures that all matches are selected in the list box.
        /// </summary>
        /// <param name="_">Ignored.</param>
        /// <param name="__">Ignored.</param>
        private void TheListBox_TargetUpdated(object _, DataTransferEventArgs __) => EnsureAllMatchesSelected();

        /// <summary>
        /// Event handler for when the selection of the list box has changed.
        /// Updates the view model based on the selection changes.
        /// </summary>
        /// <param name="_">Ignored.</param>
        /// <param name="__">Ignored.</param>
        private void TheListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ViewModel.EnsureMatchesAreSetToDestroy(e.RemovedItems.Cast<Match>().ToList());
            ViewModel.EnsureMatchesAreSetToRestore(e.AddedItems.Cast<Match>().ToList());
        }

        #endregion

        #region Functions

        /// <summary>
        /// Ensures that all the matches in the list box are selected.
        /// </summary>
        /// <remarks>This has a side effect that the selection changed event is invoked for each added item, which in their turn updates the view model.</remarks>
        private void EnsureAllMatchesSelected()
        {
            foreach (var i in TheListBox.Items)
            {
                if (!TheListBox.SelectedItems.Contains(i))
                    TheListBox.SelectedItems.Add(i);
            }
        }

        #endregion
    }
}
