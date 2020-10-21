using PieterP.ScoreSheet.ViewModels.Commands.Bases;
using PieterP.ScoreSheet.ViewModels.Settings;

namespace PieterP.ScoreSheet.ViewModels.Commands
{
    /// <summary>
    /// This command is used for opening the settings dialog.
    /// </summary>
    internal class OpenSettingsCommand : OpenDialogNotificationCommand<SettingsViewModel>
    {
        public OpenSettingsCommand(MainWindowViewModel mainWindowViewModel) : base(mainWindowViewModel) { }

        /// <inheritdoc/>
        protected override SettingsViewModel CreatePanelViewModel() => new SettingsViewModel(_mainWindowViewModel);
    }
}
