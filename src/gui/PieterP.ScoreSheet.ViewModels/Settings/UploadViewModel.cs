using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using PieterP.ScoreSheet.Connector;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.Model.Information;
using PieterP.ScoreSheet.ViewModels.Notifications;
using PieterP.Shared;
using PieterP.Shared.Cells;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.ViewModels.Settings {
    public class UploadViewModel {
        public UploadViewModel() {
            this.Username = DatabaseManager.Current.Settings.TabTUsername;
            this.Password = DatabaseManager.Current.Settings.TabTPassword;
            this.Test = new RelayCommand(OnTest);
            //this.IsCapsLockOn = Keyboard.CapsLockOn;
        }

        private async void OnTest() {
            if (this.Username.Value == "") {
                NotificationManager.Current.Raise(new ShowMessageNotification(Strings.Various_NoUsername, NotificationTypes.Exclamation));
            } else if (this.Password.Value == "") {
                NotificationManager.Current.Raise(new ShowMessageNotification(Strings.Various_NoPassword, NotificationTypes.Exclamation));
            } else {
                var connector = ServiceLocator.Resolve<IConnector>();
                connector.SetDefaultCredentials(this.Username.Value, this.Password.Value);
                var result = await connector.TestAsync();
                ShowMessageNotification message;
                if (result.ErrorCode == TabTErrorCode.InvalidCredentials || (result.Info != null && !result.Info.ValidAccount)) {
                    message = new ShowMessageNotification(Strings.Various_InvalidLogin, NotificationTypes.Exclamation);
                } else if (result.ErrorCode == TabTErrorCode.NoError) {
                    message = new ShowMessageNotification(Strings.Various_ValidLogin, NotificationTypes.Informational);
                } else {
                    message = new ShowMessageNotification(Strings.Various_NetworkError, NotificationTypes.Error);
                }
                NotificationManager.Current.Raise(message);
            }
        }

        public Cell<string> Username { get; set; }
        public Cell<string> Password { get; set; }
        public ICommand Test { get; set; }
//        public Cell<bool> IsCapsLockOn { get; }
    }
}