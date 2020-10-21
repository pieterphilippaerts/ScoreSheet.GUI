using PieterP.ScoreSheet.ViewModels.Notifications;
using PieterP.ScoreSheet.ViewModels.Wizards;
using PieterP.Shared.Services;
using System;
using System.Collections.Specialized;
using System.Windows.Input;

namespace PieterP.ScoreSheet.ViewModels.Commands.Bases
{
    /// <summary>
    /// This base class can be used for commands that open a wizard.
    /// </summary>
    /// <typeparam name="T">A type that extends <see cref="WizardPanelViewModel"/>. The view model to be used by the panel in the wizard.</typeparam>
    internal class OpenWizardCommand<T> : OpenDialogNotificationCommand<T> where T : WizardPanelViewModel
    {
        protected WizardViewModel _wizardViewModel;

        public OpenWizardCommand(MainWindowViewModel mainWindowViewModel): base (mainWindowViewModel) { }

        /// <summary>
        /// Executes the command by creating the view model for the panel and raising the wizard notification.
        /// </summary>
        /// <param name="_">A paramater.</param>
        public sealed override void Execute(object _)
        {
            _wizardViewModel = new WizardViewModel();
            _panelViewModel = CreatePanelViewModel();
            _wizardViewModel.CurrentPanel.Value = _panelViewModel;

            ShowDialogNotification(_wizardViewModel);
        }
    }
}
