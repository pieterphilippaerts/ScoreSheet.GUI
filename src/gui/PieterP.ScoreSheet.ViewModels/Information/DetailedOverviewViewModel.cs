using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using PieterP.ScoreSheet.ViewModels.Interfaces;
using PieterP.ScoreSheet.ViewModels.Score;
using PieterP.ScoreSheet.ViewModels.Services;
using PieterP.Shared;
using PieterP.Shared.Cells;
using PieterP.Shared.Interfaces;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.ViewModels.Information {
    public class DetailedOverviewViewModel : ICanBeOrchestrated {
        public DetailedOverviewViewModel(ObservableCollection<CompetitiveMatchViewModel> activeMatches, ObservableCollection<AwayMatchInfo> awayMatches) {
            _activeMatches = activeMatches;
            _awayMatches = awayMatches;
            this.Details = Cell.Create<object?>(null);
            this.Titles = Cell.Create<List<AbstractTitle>?>(null);
            _activeIndex = -1;
            _activeMatches.CollectionChanged += (s, e) => { if (this.Details.Value == null) UpdateScreen(); };
            _awayMatches.CollectionChanged += (s, e) => { if (this.Details.Value == null) UpdateScreen(); };
            UpdateScreen();
            this.TitleCount = Cell.Derived(this.Titles, v => {
                if (v == null)
                    return 0;
                else
                    return v.Count;
            });
        }

        public void UpdateScreen() {
            _activeIndex++;
            var titles = new List<AbstractTitle>();
            foreach (var am in _activeMatches) {
                titles.Add(new MatchTitle() {
                    HomeTeam = am.HomeTeam.Name,
                    AwayTeam = am.AwayTeam.Name,
                    HomeScore = am.Score.HomeMatchesWon,
                    AwayScore = am.Score.AwayMatchesWon,
                    IsActive = false
                });
            }
            if (_awayMatches.Count > 0) {
                titles.Add(new AwayTitle() {
                    IsActive = false
                });
            }
            if (_activeIndex >= _activeMatches.Count) {
                _activeIndex = -1;
                if (_awayMatches.Count > 0) {
                    this.Details.Value = new OverviewAwayMatchInfo(_awayMatches);
                    titles[titles.Count - 1].IsActive = true;
                } else {
                    if (_activeMatches.Count > 0) {
                        _activeIndex = 0;
                        this.Details.Value = new DetailedMatchInfo(_activeMatches[_activeIndex]);
                        titles[_activeIndex].IsActive = true;
                    } else {
                        this.Details.Value = null;
                    }
                }
            } else {
                this.Details.Value = new DetailedMatchInfo(_activeMatches[_activeIndex]);
                titles[_activeIndex].IsActive = true;
            }
            this.Titles.Value = titles;

            if (_activeIndex >= 0 && _activeIndex < _activeMatches.Count) {
                // dit moeten we doen omdat er anders een bug is dat het second screen de namen van de spelers niet update
                // als die aangepast zijn
                foreach (var m in _activeMatches[_activeIndex].Matches) {
                    m.HomePlayers.NotifyObservers();
                    m.AwayPlayers.NotifyObservers();
                }
            }
        }

        public void Dispose() {
            // nothing to do
        }

        public Cell<List<AbstractTitle>?> Titles { get; }
        public Cell<object?> Details { get; }
        public Cell<int> TitleCount { get; }

        private ObservableCollection<CompetitiveMatchViewModel> _activeMatches;
        private ObservableCollection<AwayMatchInfo> _awayMatches;
        private int _activeIndex;
    }

    public class AbstractTitle {
        public bool IsActive { get; set; }
    }
    public class MatchTitle : AbstractTitle { 
        public Cell<string> HomeTeam { get; set; }
        public Cell<string> AwayTeam { get; set; }
        public Cell<int> HomeScore { get; set; }
        public Cell<int> AwayScore { get; set; }
    }
    public class AwayTitle : AbstractTitle {
    }

    public class DetailedMatchInfo : OverviewMatchInfo {
        public DetailedMatchInfo(CompetitiveMatchViewModel activeMatch) : base(activeMatch) {
            this.Matches = activeMatch.Matches;
        }
        public IList<MatchInfo> Matches { get; private set; }
    }
    public class OverviewAwayMatchInfo {
        public OverviewAwayMatchInfo(ObservableCollection<AwayMatchInfo> awayMatches) {
            this.AwayMatches = awayMatches.Select(am => new OverviewMatchInfo(am));
        }
        public IEnumerable<OverviewMatchInfo> AwayMatches { get; private set; }
    }
}
