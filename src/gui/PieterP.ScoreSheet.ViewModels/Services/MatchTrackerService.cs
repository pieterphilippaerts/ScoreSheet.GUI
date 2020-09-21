using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using PieterP.ScoreSheet.Connector;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.Model.Database.Entities;
using PieterP.ScoreSheet.Model.Interfaces;
using PieterP.Shared;
using PieterP.Shared.Cells;
using PieterP.Shared.Interfaces;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.ViewModels.Services {
    public class MatchTrackerService : IDisposable {
        public MatchTrackerService() {
            _timer = ServiceLocator.Resolve<ITimerService>();
            this.AwayMatches = new ObservableCollection<AwayMatchInfo>();
            this.TrackCount = Cell.Create(0);

            DatabaseManager.Current.Settings.HomeClubId.ValueChanged += Initialize;
            DatabaseManager.Current.Settings.FollowAway.ValueChanged += Initialize;
            DatabaseManager.Current.Settings.EnableLiveUpdates.ValueChanged += Initialize;
            DatabaseManager.Current.Settings.EnableLiveUpdatesForSuperOnly.ValueChanged += Initialize;
            Initialize();
        }

        private void Initialize() {
            this.AwayMatches.Clear();
            _matches = null;
            string? homeClub = DatabaseManager.Current.Settings.HomeClubId.Value;
            if (homeClub != null && homeClub != "") {
                var date = DateTime.Now.Date;
//#if DEBUG
//                date = new DateTime(2020, 9, 12);
//#endif
                _matches = DatabaseManager.Current.MatchStartInfo.GetMatchesAtDate(date, false, false)
                    .Where(m => m.HomeClub != homeClub && !MatchStartInfo.IsByeIndex(m.HomeClub)).ToList();
                if (DatabaseManager.Current.Settings.FollowAway.Value && DatabaseManager.Current.Settings.EnableLiveUpdates.Value && !DatabaseManager.Current.Settings.EnableLiveUpdatesForSuperOnly.Value) {
                    lock (_timer) {
                        if (_isStarted || (_matches != null && _matches.Count == 0))
                            return;
                        _timer.Tick += Timer_Tick;
                        _timer.Start(new TimeSpan(0, 5, 0));
                        _isStarted = true;
                    }
                    Timer_Tick(_timer);
                } else {
                    lock (_timer) {
                        if (_isStarted)
                            _timer.Stop();
                        _isStarted = false;
                    }
                }
            }
            this.TrackCount.Value = _matches?.Count ?? 0;
        }

        public ObservableCollection<AwayMatchInfo> AwayMatches { get; private set; }
        public Cell<int> TrackCount { get; private set; }

        private async void Timer_Tick(ITimerService obj) {
            if (_matches == null || _matches.Count == 0)
                return;
            string? home = DatabaseManager.Current.Settings.HomeClubId.Value;
            if (home == null)
                return;

            if (!ServiceLocator.Resolve<INetworkAvailabilityService>().IsNetworkAvailable.Value)
                return; // no network connection available

            var connector = ServiceLocator.Resolve<IConnector>();
            connector.SetDefaultCredentials(DatabaseManager.Current.Settings.TabTUsername.Value, DatabaseManager.Current.Settings.TabTPassword.Value);
            
            foreach (var m in _matches) {
                if (m.MatchId != null) {
                    try {
                        var match = await connector.GetMatchDetails(home, m.MatchId);
                        if (match != null && match.Details != null) {
                            var am = AwayMatches.Where(c => c.MatchId == m.MatchId).FirstOrDefault();
                            if (am == null) {
                                am = new AwayMatchInfo(m.MatchId, m.HomeTeam ?? Strings.MatchTracker_Home, m.HomeClub ?? "", m.AwayTeam ?? Strings.MatchTracker_Away, m.AwayClub ?? "");
                                AwayMatches.Add(am);
                            }
                            am.HomeMatchesWon.Value = match.Details.HomeScore;
                            am.AwayMatchesWon.Value = match.Details.AwayScore;
                        }
                    } catch (Exception e) {
                        Logger.Log(e);
                    }
                }
            }
        }

        public void Dispose() {
            _timer.Stop();
        }

        private ITimerService _timer;
        private bool _isStarted;
        private IList<MatchStartInfo>? _matches;
    }

    public class AwayMatchInfo {
        public AwayMatchInfo(string matchId, string homeTeam, string homeClub, string awayTeam, string awayClub) {
            this.MatchId = matchId;
            this.HomeTeam = homeTeam;
            this.HomeClubId = homeClub;
            this.AwayTeam = awayTeam;
            this.AwayClubId = awayClub;
            this.HomeMatchesWon = Cell.Create(0);
            this.AwayMatchesWon = Cell.Create(0);
        }
        public string MatchId { get; private set; }
        public string HomeTeam { get; private set; }
        public string HomeClubId { get; private set; }
        public string AwayTeam { get; private set; }
        public string AwayClubId { get; private set; }
        public Cell<int> HomeMatchesWon { get; private set; }
        public Cell<int> AwayMatchesWon { get; private set; }
    }
}

