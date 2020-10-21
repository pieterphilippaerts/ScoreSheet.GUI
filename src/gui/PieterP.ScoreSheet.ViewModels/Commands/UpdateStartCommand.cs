using PieterP.ScoreSheet.ViewModels.Commands.Bases;
using PieterP.ScoreSheet.ViewModels.Wizards;

namespace PieterP.ScoreSheet.ViewModels.Commands
{
    /// <summary>
    /// This command handles the start command for updating.
    /// </summary>
    internal class UpdateStartCommand : OpenWizardCommand<UpdateStartViewModel>
    {
        public UpdateStartCommand() : base(null) { }

        /// <inheritdoc/>
        protected override UpdateStartViewModel CreatePanelViewModel() => new UpdateStartViewModel(_wizardViewModel);
    }
}
