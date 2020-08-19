using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PieterP.ScoreSheet.Model.Database.MatchSystems;
using PieterP.Shared.Cells;

namespace PieterP.ScoreSheet.ViewModels.Score {
    public class ScoreCalculator {
        public ScoreCalculator(CompetitiveMatchViewModel vm) {
            _viewModel = vm;
            this.HomeMatchesWon = Cell.Create(0);
            this.AwayMatchesWon = Cell.Create(0);
            this.Result = Cell.Create(Winner.Incomplete);
            //this.ContainsSuspectSemantics = Cell.Create(false);
        }

        private void AssertEmptyMatch(int homeWins, int awayWins) {
            for (int i = 0; i < _viewModel.Matches.Count; i++) {
                var m = _viewModel.Matches[i];
                foreach (var s in m.Sets) {
                    if (IsEmpty(s))
                        s.IsValid.Value = true;
                    else
                        s.IsValid.Value = false;
                }
                if (i == _viewModel.Matches.Count - 1) {
                    m.HomeSets.Value = "";
                    m.AwaySets.Value = "";
                    m.HomeMatches.Value = homeWins.ToString();
                    m.AwayMatches.Value = awayWins.ToString();
                } else {
                    m.HomeSets.Value = "";
                    m.AwaySets.Value = "";
                    m.HomeMatches.Value = "";
                    m.AwayMatches.Value = "";
                }
            }
        }

        public bool IsPlayerWO(SinglePlayerInfo spi) {
            if (spi.ParentTeam.Forfeit.Value)
                return true;
            int matchesPlayer = 0;
            foreach (var m in _viewModel.Matches) {
                if (m.HomePlayers.Value.Contains(spi)) {
                    matchesPlayer++;
                    var result = WhoWon(0, m.Sets[0]);
                    if (result != WonResult.AwayWithWO && result != WonResult.Neither)
                        return false;
                }
                if (m.AwayPlayers.Value.Contains(spi)) {
                    matchesPlayer++;
                    var result = WhoWon(0, m.Sets[0]);
                    if (result != WonResult.HomeWithWO && result != WonResult.Neither)
                        return false;
                }
            }
            // als er wedstrijden gespeeld zijn en die waren altijd WO, dan is de speler WO
            // als de speler geen wedstrijden gespeeld heeft, is het waarschijnlijk een wisselspeler in super
            return matchesPlayer != 0;
        }

        public void Refresh() {
            var individualWins = new Dictionary<PlayerInfo, int>();
            var woWins = new Dictionary<PlayerInfo, int>();
            var system = _viewModel.MatchSystem;
            int totalHomeWins = 0, totalAwayWins = 0;
            bool hasErrors = false, hasIncomplete = false;
            if (_viewModel.HomeTeam.IsBye.Value || _viewModel.AwayTeam.IsBye.Value) {
                totalHomeWins = 0;
                totalAwayWins = 0;
                hasErrors = false;
                hasIncomplete = false;
                AssertEmptyMatch(totalHomeWins, totalAwayWins);
            } else if (_viewModel.HomeTeam.Forfeit.Value || _viewModel.AwayTeam.Forfeit.Value) {
                totalHomeWins = _viewModel.HomeTeam.Forfeit.Value ? 0 : system.MatchCount;
                totalAwayWins = _viewModel.HomeTeam.Forfeit.Value ? system.MatchCount : 0;
                hasErrors = false;
                hasIncomplete = false;
                AssertEmptyMatch(totalHomeWins, totalAwayWins);
            } else {
                foreach (var m in _viewModel.Matches) {
                    var (matchResult, setsLeft, setsRight) = WhoWon(m.Sets, system);
                    if (matchResult == WonResult.Empty) {
                        foreach (var s in m.Sets) {
                            s.IsValid.Value = true;
                        }
                        m.AwayMatches.Value = "";
                        m.AwaySets.Value = "";
                        m.HomeMatches.Value = "";
                        m.HomeSets.Value = "";
                        hasIncomplete = true;
                    } else {
                        if (matchResult != WonResult.Error) {
                            m.HomeSets.Value = setsLeft.ToString();
                            m.AwaySets.Value = setsRight.ToString();
                            if (matchResult == WonResult.Home || matchResult == WonResult.HomeWithFF || matchResult == WonResult.HomeWithWO) {
                                if (matchResult == WonResult.HomeWithWO)
                                    AddWinner(woWins, m.HomePlayers.Value);
                                else
                                    AddWinner(individualWins, m.HomePlayers.Value);
                                totalHomeWins++;
                            } else if (matchResult == WonResult.Away || matchResult == WonResult.AwayWithFF || matchResult == WonResult.AwayWithWO) {
                                if (matchResult == WonResult.AwayWithWO)
                                    AddWinner(woWins, m.AwayPlayers.Value);
                                else
                                    AddWinner(individualWins, m.AwayPlayers.Value);
                                totalAwayWins++;
                            }
                            m.HomeMatches.Value = totalHomeWins.ToString();
                            m.AwayMatches.Value = totalAwayWins.ToString();
                        } else {
                            m.HomeSets.Value = "";
                            m.AwaySets.Value = "";
                            m.HomeMatches.Value = "";
                            m.AwayMatches.Value = "";
                            hasErrors = true;
                        }
                    }
                }
            }
            // update overall results
            //this.ContainsSuspectSemantics.Value = HasSuspectSemantics();
            this.HomeMatchesWon.Value = totalHomeWins;
            this.AwayMatchesWon.Value = totalAwayWins;
            if (hasErrors)
                this.Result.Value = Winner.Error;
            else if (hasIncomplete)
                this.Result.Value = Winner.Incomplete;
            else if (totalHomeWins == totalAwayWins)
                this.Result.Value = Winner.Draw;
            else
                this.Result.Value = totalHomeWins > totalAwayWins ? Winner.Home : Winner.Away;
            // update individual player results
            var allPlayers = _viewModel.HomeTeam.Players.Concat(_viewModel.AwayTeam.Players);
            foreach (var pi in allPlayers) {
                int wins, wo;
                if (!individualWins.TryGetValue(pi, out wins))
                    wins = 0;
                if (!woWins.TryGetValue(pi, out wo))
                    wo = 0;
                string result = wins.ToString();
                if (wo > 0) {
                    if (wo == 1) {
                        result = result + "+WO";
                    } else {
                        result = result + $"+{ wo.ToString() }WO";
                    }
                }
                var spi = pi as SinglePlayerInfo;
                if (spi != null && IsPlayerWO(spi)) {
                    pi.IndividualWins.Value = "WO";
                } else {
                    pi.IndividualWins.Value = result;
                }
            }
        }
        //private bool HasSuspectSemantics() {
        //    foreach (var m in _viewModel.Matches) {
        //        if (m.Sets[0].LeftScore.Value.ToLower() == "ff" || m.Sets[0].RightScore.Value.ToLower() == "ff")
        //            return true;
        //        foreach (var s in m.Sets.Skip(1)) {
        //            if (s.LeftScore.Value.ToLower() == "wo" || s.RightScore.Value.ToLower() == "wo")
        //                return true;
        //        }
        //    }
        //    return false;
        //}
        private void AddWinner(Dictionary<PlayerInfo, int> dict, IList<PlayerInfo> players) {
            if (players.Count != 1)
                return; // only add score for individual matches
            int val;
            if (!dict.TryGetValue(players[0], out val)) {
                val = 0;
            }
            dict[players[0]] = val + 1;
        }
        
        public (WonResult Result, int HomeSets, int AwaySets) WhoWon(IList<SetInfo> sets, MatchSystem system) {
            int setsLeft = 0, setsRight = 0;
            if (_viewModel.HomeTeam.Forfeit.Value) {
                return (WonResult.AwayWithWO, setsLeft, setsRight);
            } else if (_viewModel.AwayTeam.Forfeit.Value) {
                return (WonResult.HomeWithWO, setsLeft, setsRight);
            }
            if (IsEmpty(sets)) {
                return (WonResult.Empty, setsLeft, setsRight);
            } else {
                var matchResult = WonResult.Empty;
                for (int i = 0; i < sets.Count; i++) {
                    var s = sets[i];
                    if (matchResult != WonResult.Empty) {
                        if (IsEmpty(s)) {
                            s.IsValid.Value = true;
                        } else {
                            s.IsValid.Value = false;
                            matchResult = WonResult.Error;
                        }
                    } else {
                        var whoWonSet = WhoWon(i, s);
                        switch (whoWonSet) {
                            case WonResult.Error:
                                s.IsValid.Value = false;
                                matchResult = WonResult.Error;
                                break;
                            case WonResult.Home:
                            case WonResult.HomeWithFF:
                            case WonResult.HomeWithWO:
                                // if home won the set...
                                if (setsLeft < system.SetCount) { // increase the home set count
                                    setsLeft++;
                                    if (setsLeft == system.SetCount || whoWonSet == WonResult.HomeWithFF || whoWonSet == WonResult.HomeWithWO)
                                        matchResult = whoWonSet;
                                    s.IsValid.Value = true;
                                } else { // unless we have played too many sets
                                    s.IsValid.Value = false;
                                    matchResult = WonResult.Error;
                                }
                                break;
                            case WonResult.Away:
                            case WonResult.AwayWithFF:
                            case WonResult.AwayWithWO:
                                // if away won the set...
                                if (setsRight < system.SetCount) { // increase the away set count
                                    setsRight++;
                                    if (setsRight == system.SetCount || whoWonSet == WonResult.AwayWithFF || whoWonSet == WonResult.AwayWithWO)
                                        matchResult = whoWonSet;
                                    s.IsValid.Value = true;
                                } else { // unless we have played too many sets
                                    s.IsValid.Value = false;
                                    matchResult = WonResult.Error;
                                }
                                break;
                            case WonResult.Neither:
                                s.IsValid.Value = true;
                                matchResult = WonResult.Neither;
                                break;
                        }
                    }
                }
                if (matchResult == WonResult.HomeWithWO) {
                    setsLeft = system.SetCount;
                    setsRight = 0;
                } else if (matchResult == WonResult.AwayWithWO) {
                    setsRight = system.SetCount;
                    setsLeft = 0;
                }
                return (matchResult, setsLeft, setsRight);
            }
        }
        public WonResult WhoWon(int setIndex, SetInfo set) {            
            if (_viewModel.HomeTeam.Forfeit.Value) {
                return WonResult.AwayWithWO;
            }
            if (_viewModel.AwayTeam.Forfeit.Value) {
                return WonResult.HomeWithWO;
            }

            string left = set.LeftScore.Value.ToUpper(), right = set.RightScore.Value.ToUpper();

            if (setIndex > 0 && (left == "WO" || right == "WO"))
                return WonResult.Error;

            if (left == "WO" && right == "WO")
                return WonResult.Neither;

            if (left == "FF" || left == "WO") {
                if (right.Length > 0)
                    return WonResult.Error;
                else
                    return left == "FF" ? WonResult.AwayWithFF : WonResult.AwayWithWO;
            }

            if (right == "FF" || right == "WO") {
                if (left.Length > 0)
                    return WonResult.Error;
                else
                    return right == "FF" ? WonResult.HomeWithFF : WonResult.HomeWithWO;
            }

            var system = _viewModel.MatchSystem;
            int home, away;
            if (!int.TryParse(left, out home) || !int.TryParse(right, out away))
                return WonResult.Error;
            if (home < 0 || away < 0)
                return WonResult.Error;
            if (home > away) {
                if (home < system.PointCount || (home > system.PointCount && home - away != 2) || (home - away < 2))
                    return WonResult.Error;
                else
                    return WonResult.Home;
            } else {
                if (away < system.PointCount || (away > system.PointCount && away - home != 2) || (away - home < 2))
                    return WonResult.Error;
                else
                    return WonResult.Away;
            }
        }
        public bool IsEmpty(IEnumerable<SetInfo> sets) {
            return sets.All(s => IsEmpty(s));
        }
        public bool IsEmpty(SetInfo set) {
            return set.LeftScore.Value == "" && set.RightScore.Value == "";
        }

        private CompetitiveMatchViewModel _viewModel;

        public Cell<Winner> Result { get; private set; }
        public Cell<int> HomeMatchesWon { get; private set; }
        public Cell<int> AwayMatchesWon { get; private set; }
        //public Cell<bool> ContainsSuspectSemantics { get; private set; }
    }
    public enum WonResult {
        Empty,
        Home,
        HomeWithFF,
        HomeWithWO,
        Away,
        AwayWithFF,
        AwayWithWO,
        Neither,
        Error
    }
    public enum Winner { 
        Home,
        Away,
        Draw,
        Error,
        Incomplete
    }
}
