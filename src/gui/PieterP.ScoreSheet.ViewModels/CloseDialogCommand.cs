using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using PieterP.ScoreSheet.ViewModels.Notifications;
using PieterP.Shared;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.ViewModels {
    public class CloseDialogCommand : ICommand {
        public event EventHandler CanExecuteChanged;

        public CloseDialogCommand(bool returnValue = false) {
            _returnValue = returnValue;
        }

        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) {
            NotificationManager.Current.Raise(new CloseDialogNotification(_returnValue));
        }
        private bool _returnValue;
    }
}
