using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.ViewModels.Notifications;
using PieterP.Shared;
using PieterP.Shared.Cells;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.ViewModels.Settings {
    public class VariousViewModel {
        public VariousViewModel() {
            this.HideNavigation = DatabaseManager.Current.Settings.HideNavigation;
            this.StartFullScreen = DatabaseManager.Current.Settings.StartFullScreen;
            this.TurnOnCapsLock = DatabaseManager.Current.Settings.TurnOnCapsLock;
            this.ShowResultsInNavigation = DatabaseManager.Current.Settings.ShowResultsInNavigation;
            this.ClearCache = new RelayCommand(OnClear);
        }
        private void OnClear() {
            var not = new ShowMessageNotification(Strings.Various_DeleteWarning, NotificationTypes.Exclamation, NotificationButtons.YesNo);
            NotificationManager.Current.Raise(not);
            if (not.Result == true) {
                foreach (var mid in DatabaseManager.Current.OfficialMatches.MatchIds) {
                    DatabaseManager.Current.OfficialMatches[mid] = null;
                }
            }
        }
        public Cell<bool> HideNavigation { get; private set; }
        public Cell<bool> StartFullScreen { get; private set; }
        public Cell<bool> TurnOnCapsLock { get; private set; }
        public Cell<bool> ShowResultsInNavigation { get; private set; }
        public ICommand ClearCache { get; private set; }
    }
}
