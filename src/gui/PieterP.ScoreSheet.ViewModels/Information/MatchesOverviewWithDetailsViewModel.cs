using PieterP.ScoreSheet.ViewModels.Interfaces;
using PieterP.ScoreSheet.ViewModels.Score;
using PieterP.ScoreSheet.ViewModels.Services;
using PieterP.Shared.Cells;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PieterP.ScoreSheet.ViewModels.Information {
    public class MatchesOverviewWithDetailsViewModel : ICanBeOrchestrated {
        public MatchesOverviewWithDetailsViewModel(ObservableCollection<CompetitiveMatchViewModel> activeMatches, ObservableCollection<AwayMatchInfo> awayMatches) {
            this.Rows = Cell.Create(1);
            this.Columns = Cell.Create(1);
            this.Matches = new ObservableCollection<OverviewWithDetailsMatchInfo>();
            this.Matches.CollectionChanged += Matches_CollectionChanged;
            _activeMatches = activeMatches;
            _activeMatches.CollectionChanged += (a, b) => Refresh();
            _awayMatches = awayMatches;
            _awayMatches.CollectionChanged += (a, b) => Refresh();
            Refresh();
        }

        private void Refresh() {
            this.Matches.Clear();
            foreach (var m in _activeMatches) {
                this.Matches.Add(new OverviewWithDetailsMatchInfo(m));
            }
            foreach (var m in _awayMatches) {
                this.Matches.Add(new OverviewWithDetailsMatchInfo(m));
            }
        }

        private void Matches_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            var mc = this.Matches.Count;
            if (mc <= 3) {
                this.Columns.Value = 1;
                this.Rows.Value = mc;
            } else if (mc == 4) {
                this.Columns.Value = 2;
                this.Rows.Value = 2;
            } else if (mc <= 6) {
                this.Columns.Value = 2;
                this.Rows.Value = 3;
            } else if (mc <= 9) {
                this.Columns.Value = 3;
                this.Rows.Value = 3;
            } else if (mc <= 12) {
                this.Columns.Value = 3;
                this.Rows.Value = 4;
            } else {
                var v = (int)Math.Ceiling(Math.Sqrt(mc));
                this.Columns.Value = v;
                this.Rows.Value = v;
            }
        }

        public void Dispose() {
            // nothing to do
        }
        public void UpdateScreen() {
            // nothing to do
        }

        public Cell<int> Rows { get; private set; }
        public Cell<int> Columns { get; private set; }
        public ObservableCollection<OverviewWithDetailsMatchInfo> Matches { get; private set; }
        private ObservableCollection<CompetitiveMatchViewModel> _activeMatches;
        private ObservableCollection<AwayMatchInfo> _awayMatches;
    }
    public class OverviewWithDetailsMatchInfo {
        public OverviewWithDetailsMatchInfo(AwayMatchInfo awayMatch) {
            this.MatchId = Cell.Create(awayMatch.MatchId);
            this.HomeTeam = Cell.Create(awayMatch.HomeTeam);
            this.HomeClubId = Cell.Create(awayMatch.HomeClubId);
            this.AwayTeam = Cell.Create(awayMatch.AwayTeam);
            this.AwayClubId = Cell.Create(awayMatch.AwayClubId);
            this.HomeMatchesWon = awayMatch.HomeMatchesWon;
            this.AwayMatchesWon = awayMatch.AwayMatchesWon;
            this.HomePlayers = Cell.Create<IList<SinglePlayerInfo>>(new List<SinglePlayerInfo>());
            this.AwayPlayers = Cell.Create<IList<SinglePlayerInfo>>(new List<SinglePlayerInfo>());
        }
        public OverviewWithDetailsMatchInfo(CompetitiveMatchViewModel activeMatch) {
            this.MatchId = activeMatch.MatchId;
            this.HomeTeam = activeMatch.HomeTeam.Name;
            this.HomeClubId = activeMatch.HomeTeam.ClubId;
            this.AwayTeam = activeMatch.AwayTeam.Name;
            this.AwayClubId = activeMatch.AwayTeam.ClubId;
            this.HomeMatchesWon = activeMatch.Score.HomeMatchesWon;
            this.AwayMatchesWon = activeMatch.Score.AwayMatchesWon;
            this.HomePlayers = Cell.Create<IList<SinglePlayerInfo>>(activeMatch.HomeTeam.Players.OfType<SinglePlayerInfo>().ToList());
            this.AwayPlayers = Cell.Create<IList<SinglePlayerInfo>>(activeMatch.AwayTeam.Players.OfType<SinglePlayerInfo>().ToList());

            foreach (var hp in this.HomePlayers.Value) {
                hp.ComputerNumber.ValueChanged += () => this.HomePlayers.NotifyObservers();
                hp.IndividualWins.ValueChanged += () => this.HomePlayers.NotifyObservers();
                hp.Ranking.ValueChanged += () => this.HomePlayers.NotifyObservers();
            }
            foreach (var ap in this.AwayPlayers.Value) {
                ap.ComputerNumber.ValueChanged += () => this.AwayPlayers.NotifyObservers();
                ap.IndividualWins.ValueChanged += () => this.AwayPlayers.NotifyObservers();
                ap.Ranking.ValueChanged += () => this.AwayPlayers.NotifyObservers();
            }
        }
        public Cell<string> MatchId { get; private set; }
        public Cell<string> HomeTeam { get; private set; }
        public Cell<string> HomeClubId { get; private set; }
        public Cell<string> AwayTeam { get; private set; }
        public Cell<string> AwayClubId { get; private set; }
        public Cell<int> HomeMatchesWon { get; private set; }
        public Cell<int> AwayMatchesWon { get; private set; }
        public Cell<IList<SinglePlayerInfo>> HomePlayers { get; private set;}
        public Cell<IList<SinglePlayerInfo>> AwayPlayers { get; private set; }
    }
}
