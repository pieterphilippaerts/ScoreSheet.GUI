using PieterP.ScoreSheet.ViewModels.Commands.Bases;
using System.Linq;

namespace PieterP.ScoreSheet.ViewModels.Commands
{
    /// <summary>
    /// This command handles the request to close all matches.
    /// </summary>
    class CloseAllCommand : BaseMainWindowCommand
    {
        public CloseAllCommand(MainWindowViewModel mainWindowViewModel) : base(mainWindowViewModel)
        {
            mainWindowViewModel.ActiveMatches.CollectionChanged += RaiseCanExecuteChanged;
        }

        /// <inheritdoc/>
        public override bool CanExecute(object _) => _mainWindowViewModel.ActiveMatches.Count > 0;

        /// <inheritdoc/>
        public override void Execute(object _) => _mainWindowViewModel.CloseMatches(_mainWindowViewModel.ActiveMatches.ToList());
    }
}
