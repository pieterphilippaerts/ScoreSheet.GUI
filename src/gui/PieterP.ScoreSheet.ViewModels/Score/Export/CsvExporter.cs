using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.Model.Database.Entities;
using PieterP.Shared;
using PieterP.Shared.Services;
using static PieterP.ScoreSheet.Localization.Errors;

namespace PieterP.ScoreSheet.ViewModels.Score.Export {
    public class CsvExporter {
        public string Export(CompetitiveMatchViewModel matchVm, bool partial = false) {
            return Export(new CompetitiveMatchViewModel[] { matchVm }, partial);
        }
        public string Export(IEnumerable<CompetitiveMatchViewModel> matchVms, bool partial = false) {
            var sb = new StringBuilder();
            Export(matchVms, sb, partial);
            return sb.ToString();
        }
        public void Export(CompetitiveMatchViewModel matchVm, string file) {
            Export(new CompetitiveMatchViewModel[] { matchVm }, file);
        }
        public void Export(IEnumerable<CompetitiveMatchViewModel> matchVms, string file) {
            var sb = new StringBuilder();
            Export(matchVms, sb);
            using (var tw = new StreamWriter(file)) {
                tw.Write(sb.ToString());
            }
        }
        public void Export(IEnumerable<CompetitiveMatchViewModel> matchVms, StringBuilder output, bool partial = false) {
            output.AppendLine("C;Match result exported by Scoresheet, Copyright (c) PieterP.be");
            foreach (var match in matchVms) {
                Export(match, output, partial);
            }
        }
        public void Export(CompetitiveMatchViewModel matchVm, StringBuilder output, bool partial = false) {
            var db = DatabaseManager.Current;
            // interclub
            output.Append("I;");
            // season id
            if (db.Settings.CurrentSeason.Value == null) {
                Logger.Log(LogType.Warning, CsvExporter_InvalidSeasonId);
            } else {
                output.Append(db.Settings.CurrentSeason.Value.Id);
            }
            output.Append(';');
            if (matchVm.MatchId.Value == "") {
                Logger.Log(LogType.Warning, CsvExporter_NoMatchId);
            } else {
                output.Append(matchVm.MatchId.Value);
            }
            output.Append(';');
            if (matchVm.HomeTeam.ClubId.Value == "") {
                Logger.Log(LogType.Warning, CsvExporter_NoClubIdHome);
            } else {
                output.Append(ClubIndex(matchVm.HomeTeam.ClubId.Value));
            }
            output.Append(';');
            if (matchVm.AwayTeam.ClubId.Value == "") {
                Logger.Log(LogType.Warning, CsvExporter_NoClubIdAway);
            } else {
                output.Append(ClubIndex(matchVm.AwayTeam.ClubId.Value));
            }
            output.Append(';');
            // home players
            ExportTeam(output, matchVm, matchVm.HomeTeam );
            output.Append(';');
            // away players
            ExportTeam(output, matchVm, matchVm.AwayTeam);
            output.Append(';');
            // score
            bool isDataComplete = true;
            if (MatchStartInfo.IsByeIndex(matchVm.HomeTeam.ClubId.Value) || MatchStartInfo.IsByeIndex(matchVm.AwayTeam.ClubId.Value)) {
                // only output results for bye
                output.Append($"U{ matchVm.Score.HomeMatchesWon.Value }-{ matchVm.Score.AwayMatchesWon.Value }");
            } else {
                output.Append('P');
                bool first = true;
                foreach (var m in matchVm.Matches) {
                    if (first) {
                        first = false;
                    } else {
                        output.Append(',');
                    }
                    var result = matchVm.Score.WhoWon(m.Sets, matchVm.MatchSystem);
                    if (partial && (result.Result == WonResult.Empty || result.Result == WonResult.Error)) {
                        output.Append('?');
                        isDataComplete = false;
                    } else {
                        output.Append(ToIndividualMatchResults(matchVm, result.Result, m.Sets));
                    }
                }
            }

            // check that partial is allowed (i.e., at least one match is not yet filled in)
            if (isDataComplete) {
                partial = false; // override the setting if all data is filled in
            }

            if (!partial) {
                // all following columns are optional and only make sense when there are matches that have been played
                bool emptyMatch = matchVm.HomeTeam.Forfeit.Value || matchVm.AwayTeam.Forfeit.Value || matchVm.HomeTeam.IsBye.Value || matchVm.AwayTeam.IsBye.Value;
                output.Append(';');
                // responsibles

                if (emptyMatch) {
                    output.Append('-');
                } else if (!IsNumber(matchVm.HomeCaptain.ComputerNumber.Value)) {
                    Logger.Log(LogType.Warning, CsvExporter_InvalidIdHomeCaptain);
                    output.Append('-');
                } else {
                    output.Append(matchVm.HomeCaptain.ComputerNumber.Value);
                }
                output.Append(',');
                if (emptyMatch) {
                    output.Append('-');
                } else if (!IsNumber(matchVm.AwayCaptain.ComputerNumber.Value)) {
                    Logger.Log(LogType.Warning, CsvExporter_InvalidIdAwayCaptain);
                    output.Append('-');
                } else {
                    output.Append(matchVm.AwayCaptain.ComputerNumber.Value);
                }
                output.Append(',');
                if (emptyMatch) {
                    output.Append('-');
                } else if (!IsNumber(matchVm.ChiefReferee.ComputerNumber.Value)) {
                    Logger.Log(LogType.Warning, CsvExporter_InvalidIdChiefReferee);
                    output.Append('-');
                } else {
                    output.Append(matchVm.ChiefReferee.ComputerNumber.Value);
                }
                output.Append(',');
                if (emptyMatch) {
                    output.Append('-');
                } else if (matchVm.RoomCommissioner.ComputerNumber.Value == "") {
                    output.Append('-');
                } else if (!IsNumber(matchVm.RoomCommissioner.ComputerNumber.Value)) {
                    Logger.Log(LogType.Warning, CsvExporter_InvalidIdRoomCommissioner);
                    output.Append('-');
                } else {
                    output.Append(matchVm.RoomCommissioner.ComputerNumber.Value);
                }
                output.Append(';');
                var startHour = matchVm.StartHour.Value.ToTime();
                var endHour = matchVm.EndHour.Value.ToTime();
                if (startHour != null && endHour != null) {
                    output.Append($"{ startHour.Value.ToString("HH:mm") },{ endHour.Value.ToString("HH:mm") }");
                }
                if (matchVm.Comments.Value.Length > 0) {
                    output.Append(';');
                    output.Append('"');
                    output.Append(matchVm.Comments.Value.Replace(@"\", @"\\").Replace("\r\n", @"\n").Replace('"', '\''));
                    output.Append('"');
                }
            }
            output.Append("\r\n");
#if DEBUG
            Logger.Log(LogType.Debug, "Exported CSV:\r\n" + output.ToString());
#endif
        }
        private string ToIndividualMatchResults(CompetitiveMatchViewModel matchVm, WonResult matchResult, IList<SetInfo> sets) {
            // return individual match result (for CSV upload)
            switch (matchResult) {
                case WonResult.Neither:
                    return "-";
                case WonResult.HomeWithWO:
                    return "9";
                case WonResult.AwayWithWO:
                    return "8";
                case WonResult.Home:
                case WonResult.Away:
                case WonResult.HomeWithFF:
                case WonResult.AwayWithFF:
                    var sb = new StringBuilder();
                    bool first = true;
                    int setsWon = 0, setsLost = 0;
                    for (int i = 0; i < sets.Count; i++) {
                        if (matchVm.Score.IsEmpty(sets[i]))
                            break;
                        if (first) {
                            first = false;
                        } else {
                            sb.Append('/');
                        }
                        var setResult = matchVm.Score.WhoWon(i, sets[i]);
                        switch (setResult) {
                            case WonResult.Home:
                                sb.Append(sets[i].RightScore.Value);
                                setsWon++;
                                break;
                            case WonResult.Away:
                                sb.Append('-');
                                sb.Append(sets[i].LeftScore.Value);
                                setsLost++;
                                break;
                            case WonResult.HomeWithFF:
                                sb.Append('0');
                                setsWon++;
                                while (setsWon < matchVm.MatchSystem.SetCount) {
                                    sb.Append("/0");
                                    setsWon++;
                                }
                                break;
                            case WonResult.AwayWithFF:
                                sb.Append("-0");
                                setsLost++;
                                while (setsLost < matchVm.MatchSystem.SetCount) {
                                    sb.Append("/-0");
                                    setsLost++;
                                }
                                break;
                        }
                    }
                    return sb.ToString();
            }
            return "";
        }
        private void ExportTeam(StringBuilder sb, CompetitiveMatchViewModel matchVm, TeamInfo team) {
            bool first = true;
            foreach (var pi in team.Players) {
                var spi = pi as SinglePlayerInfo;
                if (spi != null) {
                    string cn = spi.ComputerNumber.Value;
                    if (cn == "" && spi.Optional) // In SUPER hoeft 4e man niet ingevuld te zijn
                        continue;
                    if (first) {
                        first = false;
                    } else {
                        sb.Append(',');
                    }
                    if (cn != "?" && cn != "-" && !IsNumber(cn)) {
                        Logger.Log(LogType.Warning, Safe.Format(CsvExporter_InvalidIdPlayer, spi.ComputerNumber.Value, spi.Name.Value, team.Name.Value));
                    } else {
                        if (matchVm.Score.IsPlayerWO(spi)) {
                            sb.Append('-');
                        }
                        sb.Append(ToValidIndex(cn));
                        var sp = team.Players.Where(p => (p is SubstitutePlayerInfo) && ((SubstitutePlayerInfo)p).TransferablePlayer == spi).Cast<SubstitutePlayerInfo>();
                        foreach (var subpi in sp) {
                            if (subpi.SelectedTransferMatch.Value != null) {
                                sb.Append('+');
                                sb.Append(subpi.SelectedTransferMatch.Value.Position.ToString());
                            }
                        }
                    }
                } else {
                    var dpi = pi as DoublePlayerInfo;
                    if (dpi != null) {
                        if (first) {
                            first = false;
                        } else {
                            sb.Append(',');
                        }
                        sb.Append(ToValidDouble(dpi.SelectedOption.Value.Name));
                    }
                }
            }
        }
        private string? ClubIndex(string? c) {
            if (c == null)
                return null;
            if (MatchStartInfo.IsByeIndex(c))
                return "bye";
            else if (c.StartsWith("Vl-B"))
                return "VLB" + c.Substring(4); // sending Vl-B to the server is probably OK too, but the TabT specification clearly mentions VLB
            else
                return c;
        }
        private bool IsNumber(string input) {
            if (input == null || input.Length == 0)
                return false;
            return int.TryParse(input, out var result);
        }
        private string ToValidIndex(string idx) {
            if (/*idx == "?" ||*/ idx == "-")
                return "?";
            else
                return idx;
        }
        private string ToValidDouble(string doubleTeam) {
            return doubleTeam.Replace('/', '-').Replace('\\', '-').Replace('+', '-');
        }
    }
}
