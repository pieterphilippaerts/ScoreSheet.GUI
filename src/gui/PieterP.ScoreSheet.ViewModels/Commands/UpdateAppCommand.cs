using PieterP.ScoreSheet.ViewModels.Commands.Bases;

namespace PieterP.ScoreSheet.ViewModels.Commands
{
    /// <summary>
    /// This command is used for opening the application update window.
    /// </summary>
    internal class UpdateAppCommand : OpenDialogNotificationCommand<UpdateAppViewModel>
    {
        public UpdateAppCommand() : base(null) { }

        /// <inheritdoc/>
        protected override UpdateAppViewModel CreatePanelViewModel() => new UpdateAppViewModel();
    }
}
