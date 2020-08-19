using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using PieterP.ScoreSheet.ViewModels.Notifications;
using PieterP.Shared;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.ViewModels {
    public class SaveViewModel {
        public SaveViewModel(IList<SelectedMatchInfo> matches) {
            this.Matches = matches;
            this.Cancel = new CloseDialogCommand();
            this.Discard = new RelayCommand(() => NotificationManager.Current.Raise(new CloseDialogNotification(true)));
            this.Save = new RelayCommand(() => {
                this.SaveMatches = this.Matches;
                NotificationManager.Current.Raise(new CloseDialogNotification(true));
            });
        }
        public IList<SelectedMatchInfo> Matches { get; private set; }
        public IList<SelectedMatchInfo> SaveMatches { get; private set; }
        public ICommand Save { get; private set; }
        public ICommand Discard { get; private set; }
        public ICommand Cancel { get; private set; }
    }
}
