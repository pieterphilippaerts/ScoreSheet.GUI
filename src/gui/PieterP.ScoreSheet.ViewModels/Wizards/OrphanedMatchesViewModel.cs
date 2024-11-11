using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.Model.Database.Entities;
using PieterP.ScoreSheet.ViewModels.Notifications;
using PieterP.Shared.Cells;
using PieterP.Shared.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static PieterP.ScoreSheet.Localization.Errors;
using static PieterP.ScoreSheet.Localization.Strings;
using PieterP.ScoreSheet.Model;

namespace PieterP.ScoreSheet.ViewModels.Wizards {
    public class OrphanedMatchesViewModel : WizardPanelViewModel {
        public OrphanedMatchesViewModel(WizardViewModel parent, IEnumerable<MatchStartInfo> orphanedMatches) : base(parent) {
            var currentSeason = DateTime.Now.Year;
            if (DateTime.Now.Month <= 7)
                currentSeason--;
            var start = new DateTime(currentSeason, 09, 01);
            var end = new DateTime(currentSeason + 1, 06, 30);
            
            this.Ok = new RelayCommand(this.OnClose);
            this.OrphanedMatches = orphanedMatches.Select(c => new OrphanedMatchViewModel(c, start, end)).ToList();            
        }

        public ICommand Ok { get; }
        public void OnClose() { 
            if (OrphanedMatches.Any(c => c.WeekStart.Value == null)) {
                var message = new ShowMessageNotification(OrphanedMatches_NotComplete, NotificationTypes.Question, NotificationButtons.YesNo);
                NotificationManager.Current.Raise(message);
                if (message.Result != true)
                    return;
                NotificationManager.Current.Raise(new ShowMessageNotification(OrphanedMatches_AvailabilityInfo, NotificationTypes.Informational, NotificationButtons.OK));                
            }
            // save dates
            foreach(var omvm in OrphanedMatches) {
                if (omvm.WeekStart.Value != null && omvm.WeekStart.Value.Value != null) {
                    omvm.Info.WeekStart = omvm.WeekStart.Value.Value.FindStartOfWeek();
                }
            }
            var season = DatabaseManager.Current.Settings.CurrentSeason.Value?.Id ?? 0;
            DatabaseManager.Current.MatchStartInfo.RefreshAndSave();
            DatabaseManager.Current.Settings.CustomStartWeeks.Value = OrphanedMatches.Where(c => c.WeekStart.Value != null && c.WeekStart.Value.Value != null).Select(c => new CustomStartWeek() {
                SeasonId = season,
                MatchId = c.Info.MatchId,
                StartWeek = c.WeekStart.Value!.Value
            }).ToArray();

            NotificationManager.Current.Raise(new CloseDialogNotification(true));            
        }

        public IEnumerable<OrphanedMatchViewModel> OrphanedMatches { get; private set; }

        public override string Title => Wizard_OrphanedMatches;

        public override string Description => Wizard_OrphanedMatchesDesc;
    }
    public class OrphanedMatchViewModel {
        public OrphanedMatchViewModel(MatchStartInfo startInfo, DateTime start, DateTime end) {
            this.Info = startInfo;
            
            // Do we have an initial value for the date? (sto
            var initial = this.Info.WeekStart;
            var season = DatabaseManager.Current.Settings.CurrentSeason.Value?.Id ?? 0;
            var csw = DatabaseManager.Current.Settings.CustomStartWeeks.Value?.FirstOrDefault(c => c.SeasonId == season && c.MatchId == startInfo.MatchId);
            if (csw != null)
                initial = csw.StartWeek;

            this.WeekStart = Cell.Create(initial);
            this.DateStart = start;
            this.DateEnd = end;
        }
        public MatchStartInfo Info { get; }
        public Cell<DateTime?> WeekStart { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; }
    }
}