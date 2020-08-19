using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.Model.Database.Entities;
using PieterP.ScoreSheet.ViewModels.Notifications;
using PieterP.Shared;
using PieterP.Shared.Cells;
using PieterP.Shared.Services;
using static PieterP.ScoreSheet.Localization.Errors;
using static PieterP.ScoreSheet.Localization.Strings;

namespace PieterP.ScoreSheet.ViewModels.Wizards {
    public class NewMatchdayViewModel : WizardPanelViewModel {
        public NewMatchdayViewModel(WizardViewModel parent) : base(parent) {
            var entries = new ObservableCollection<TreeEntry>();
            var dates = DatabaseManager.Current.MatchStartInfo.GetMatchDates().OrderBy(d => d);
            TreeEntry? selectedNode = null;
            TimeSpan curSpan = TimeSpan.MaxValue;
            var now = DateTime.Now.Date;
            foreach (var d in dates) {
                var entry = new TreeEntry(d.ToString("D"), d);
                entries.Add(entry);

                var ts = d.Date.Subtract(now).Duration();
                if (ts < curSpan) {
                    curSpan = ts;
                    selectedNode = entry;
                }
            }
            var orphans = DatabaseManager.Current.MatchStartInfo.GetOrphanMatches();
            if (orphans.Any()) {
                var orphanEntries = new ObservableCollection<TreeEntry>();
                foreach (var o in orphans) {
                    orphanEntries.Add(new TreeEntry($"{ o.HomeTeam } - { o.AwayTeam } ({ o.MatchId })", o));
                }
                entries.Add(new TreeEntry(Wizard_UnknownMatchDate) { Children = orphanEntries, IsValidSelection = false });
            }
            this.Entries = entries;
            this.SelectedEntry = Cell.Create<TreeEntry?>(selectedNode);
            this.SelectedEntry.ValueChanged += () => { this.Load.RaiseCanExecuteChanged(); };
            this.LoadByes = DatabaseManager.Current.Settings.LoadByes;
            this.LoadAwayMatches = DatabaseManager.Current.Settings.LoadAwayMatches;
            this.Load = new RelayCommand(OnLoad, () => this.SelectedEntry.Value?.IsValidSelection ?? false);
            this.Matches = Enumerable.Empty<MatchStartInfo>();
        }

        private void OnLoad() {
            var selected = SelectedEntry.Value;
            if (selected == null || selected.Value == null)
                return;
            switch (selected.Value) {
                case MatchStartInfo m:
                    this.Matches = new MatchStartInfo[] { m };
                    break;
                case DateTime d:
                    this.Matches = DatabaseManager.Current.MatchStartInfo.GetMatchesAtDate(d, !LoadAwayMatches.Value, LoadByes.Value);
                    break;
                default:
                    Logger.Log(LogType.Exception, Wizard_InvalidSelection); // bug?
                    return;
            }
            if (this.Matches.Count() == 0) {
                NotificationManager.Current.Raise(new ShowMessageNotification(Wizard_NoMatches, NotificationTypes.Exclamation));
            } else {
                NotificationManager.Current.Raise(new CloseDialogNotification(true));
            }
        }

        public IEnumerable<MatchStartInfo> Matches { get; private set; }

        public ObservableCollection<TreeEntry> Entries { get; private set; }
        public Cell<TreeEntry?> SelectedEntry { get; private set; }
        public Cell<bool> LoadByes { get; private set; }
        public Cell<bool> LoadAwayMatches { get; private set; }

        public RelayCommand<object> Load { get; private set; }

        public override string Title => Wizard_NewMatchDay;
        public override string Description => Wizard_NewMatchDayDesc;
    }

    public class TreeEntry {
        public TreeEntry(string name, object value = null) {
            this.Name = name;
            this.Value = value;
            this.IsValidSelection = true;
        }
        public string Name { get; set; }
        public object? Value { get; set; }
        public ObservableCollection<TreeEntry>? Children { get; set; }
        public bool IsValidSelection { get; set; }
    }
}
