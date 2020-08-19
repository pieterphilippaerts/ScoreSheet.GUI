using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using PieterP.ScoreSheet.ViewModels.Notifications;
using PieterP.Shared;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.ViewModels {
    public class WelcomeViewModel {
        public WelcomeViewModel() {
            this.NewMatchday = new RelayCommand(() => {
                this.Choice = WelcomeChoices.NewMatchday;
                Close();
            });
            this.NewCustomMatch = new RelayCommand(() => {
                this.Choice = WelcomeChoices.NewCustomMatch;
                Close();
            });
            this.OpenCustomMatch = new RelayCommand(() => {
                this.Choice = WelcomeChoices.OpenCustomMatch;
                Close();
            });
            this.UpdateDatabase = new RelayCommand(() => {
                this.Choice = WelcomeChoices.UpdateDatabase;
                Close();
            });
        }
        public ICommand NewMatchday { get; private set; }
        public ICommand NewCustomMatch { get; private set; }
        public ICommand OpenCustomMatch { get; private set; }
        public ICommand UpdateDatabase { get; private set; }
        public WelcomeChoices Choice { get; private set; }
        private void Close() {
            NotificationManager.Current.Raise(new CloseDialogNotification(true));
        }
    }
    public enum WelcomeChoices : int { 
        NewMatchday,
        NewCustomMatch,
        OpenCustomMatch,
        UpdateDatabase
    }

}
