using PieterP.ScoreSheet.ViewModels.Notifications;
using PieterP.ScoreSheet.ViewModels.Wizards;
using PieterP.Shared.Services;
using System;

namespace PieterP.ScoreSheet.ViewModels.Commands.Bases
{
    /// <summary>
    /// This base class can be used for commands that open notification dialog.
    /// </summary>
    internal class OpenDialogNotificationCommand<T> : BaseMainWindowCommand
    {
        protected T _panelViewModel;

        public OpenDialogNotificationCommand(MainWindowViewModel mainWindowViewModel) : base(mainWindowViewModel) { }

        /// <summary>
        /// The action to be executed when the dialog is confirmed.
        /// </summary>
        protected Action<ShowDialogNotification> ProcessResultAction { get; set; }

        /// <inheritdoc/>
        public override void Execute(object _)
        {
            _panelViewModel = CreatePanelViewModel();
            ShowDialogNotification(_panelViewModel!);
        }

        /// <summary>
        /// Function executed when the command is executed and the view model for the panel should be created.
        /// </summary>
        /// <returns>An instance of a <see cref="WizardPanelViewModel">panel view model</see>.</returns>
        protected virtual T CreatePanelViewModel() => throw new NotImplementedException();

        /// <summary>
        /// Opens the dialog notification for the given view model.
        /// </summary>
        /// <param name="viewModel">The view model to present.</param>
        protected void ShowDialogNotification(object viewModel)
        {
            var n = new ShowDialogNotification(viewModel);
            NotificationManager.Current.Raise(n);

            if (n.Result)
                ProcessResultAction?.Invoke(n);
        }
    }
}
