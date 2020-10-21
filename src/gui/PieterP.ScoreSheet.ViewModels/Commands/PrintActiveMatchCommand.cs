using PieterP.ScoreSheet.ViewModels.Commands.Bases;
using PieterP.ScoreSheet.ViewModels.Wizards;

namespace PieterP.ScoreSheet.ViewModels.Commands
{
    /// <summary>
    /// This command is used for printing the active match in the main view model.
    /// </summary>
    internal class PrintActiveMatchCommand: OpenWizardCommand<PrintViewModel>
    {
        public PrintActiveMatchCommand(MainWindowViewModel mainWindowViewModel): base (mainWindowViewModel)
        {
            mainWindowViewModel.CurrentScreen.ValueChanged += RaiseCanExecuteChanged;
        }

        /// <inheritdoc/>
        public override bool CanExecute(object _) => _mainWindowViewModel.CurrentScreen.Value == _mainWindowViewModel.MatchContainer;

        protected override PrintViewModel CreatePanelViewModel() => new PrintViewModel(_wizardViewModel, _mainWindowViewModel.MatchContainer.ActiveMatch.Value!);
    }
}
