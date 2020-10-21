using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.ViewModels.Commands.Bases;
using PieterP.ScoreSheet.ViewModels.Score;
using PieterP.ScoreSheet.ViewModels.Wizards;
using System.Collections.Generic;

namespace PieterP.ScoreSheet.ViewModels.Commands
{
    /// <summary>
    /// This command is used for exporting active matches.
    /// </summary>
    internal class ExportActiveMatchesCommand : OpenWizardCommand<SelectMatchesViewModel>
    {
        public ExportActiveMatchesCommand(MainWindowViewModel mainWindowViewModel) : base(mainWindowViewModel)
        {
            _mainWindowViewModel.ActiveMatches.CollectionChanged += RaiseCanExecuteChanged;
        }

        /// <inheritdoc/>
        protected override SelectMatchesViewModel CreatePanelViewModel() => new SelectMatchesViewModel(
            _wizardViewModel,
            _mainWindowViewModel.ActiveMatches,
            Strings.Wizard_Export,
            Strings.Wizard_ExportDesc,
            Strings.Wizard_ExportMessage,
            ExportTypes.FileExport,
            ContinueWithSelectedMatches);

        /// <inheritdoc/>
        public override bool CanExecute(object _) => _mainWindowViewModel.ActiveMatches.Count > 0;

        /// <summary>
        /// Upon continuing the export process, change the current panel of the wizard to select the export type.
        /// </summary>
        /// <param name="selectedMatches">The selected matches to export.</param>
        private void ContinueWithSelectedMatches(IEnumerable<CompetitiveMatchViewModel> selectedMatches)
        {
            _wizardViewModel.CurrentPanel.Value = new SelectExportTypeViewModel(_wizardViewModel, selectedMatches);
        }
    }
}
