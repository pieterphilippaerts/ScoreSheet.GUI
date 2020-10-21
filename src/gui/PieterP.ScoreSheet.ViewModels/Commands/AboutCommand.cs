using PieterP.ScoreSheet.ViewModels.Commands.Bases;

namespace PieterP.ScoreSheet.ViewModels.Commands
{
    /// <summary>
    /// This command is used for opening the about window.
    /// </summary>
    internal class AboutCommand : OpenDialogNotificationCommand<AboutWindowViewModel>
    {
        public AboutCommand() : base(null) { }

        /// <inheritdoc/>
        protected override AboutWindowViewModel CreatePanelViewModel() => new AboutWindowViewModel();
    }
}
