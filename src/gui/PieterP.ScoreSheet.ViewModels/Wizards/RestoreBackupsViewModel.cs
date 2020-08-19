using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.Model.Database.Entities;
using PieterP.ScoreSheet.ViewModels.Notifications;
using PieterP.Shared;
using PieterP.Shared.Services;
using static PieterP.ScoreSheet.Localization.Errors;
using static PieterP.ScoreSheet.Localization.Strings;

namespace PieterP.ScoreSheet.ViewModels.Wizards {
    public class RestoreBackupsViewModel : WizardPanelViewModel {
        public RestoreBackupsViewModel(WizardViewModel parent) : base(parent) {
            this.Restore = new RelayCommand<object>(OnRestore);
            this.FoundMatches = new ObservableCollection<Match>();
            foreach (var id in DatabaseManager.Current.MatchBackup.MatchIds) {
                var match = DatabaseManager.Current.MatchBackup[id];
                if (match == null) { // probably illegal json
                    DatabaseManager.Current.MatchBackup[id] = null; // delete it
                } else {
                    if (match.UniqueId != id)
                        match.UniqueId = id; // weird.. invalid json? modified file?
                    this.FoundMatches.Add(match);
                }
            }
        }
        public void OnRestore(object o) {
            var list = o as IEnumerable<object>;
            if (list != null) {
                this.SelectedMatches = list.Cast<Match>().ToList();
                NotificationManager.Current.Raise(new CloseDialogNotification(true));
            }
        }

        public ObservableCollection<Match> FoundMatches { get; private set; }
        public IEnumerable<Match> SelectedMatches { get; private set; }
        public RelayCommand<object> Restore { get; private set; }

        public override string Title => Wizard_Restore;

        public override string Description => Wizard_RestoreDesc;
    }
}