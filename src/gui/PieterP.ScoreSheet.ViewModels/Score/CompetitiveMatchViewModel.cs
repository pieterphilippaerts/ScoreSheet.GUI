using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.Model.Database.Entities;
using PieterP.ScoreSheet.Model.Database.Enums;
using PieterP.ScoreSheet.Model.Database.MatchSystems;
using PieterP.ScoreSheet.ViewModels.Interfaces;
using PieterP.ScoreSheet.ViewModels.Notifications;
using PieterP.ScoreSheet.ViewModels.Score.Export;
using PieterP.ScoreSheet.ViewModels.Score.MatchSystems;
using PieterP.ScoreSheet.ViewModels.Score.Validations;
using PieterP.ScoreSheet.ViewModels.Wizards;
using PieterP.Shared;
using PieterP.Shared.Cells;
using PieterP.Shared.Services;
using LevelEnum = PieterP.ScoreSheet.Model.Database.Enums.Level;
using ModPersonInfo = PieterP.ScoreSheet.Model.Database.Entities.PersonInfo;
using ModPlayerInfo = PieterP.ScoreSheet.Model.Database.Entities.PlayerInfo;

namespace PieterP.ScoreSheet.ViewModels.Score {
    public class CompetitiveMatchViewModel : IZoomable {
        public CompetitiveMatchViewModel(MatchSystem system) {
            Initialize(system);
            ScoreChanged();
        }
        public CompetitiveMatchViewModel(Match fullMatch, MatchStartInfo? officialData, string? fromFile = null) {
            // Check if the saved data is still consistent with the downloaded official data
            if (officialData != null) {
                bool ok = true;
                if (officialData.MatchSystemId != fullMatch.MatchSystemId) // check for inconsistency w.r.t. the match system (e.g., 4v4, 3v3, 2v2, ...)
                    ok = false;
                if (officialData.PlayerCategory != fullMatch.PlayerCategory) // check for inconsistency w.r.t. the player category (e.g., male, female, youth, ...)
                    ok = false;
                // other inconsistencies can be fixed manually by the user, so we ignore them here
                if (!ok) { 
                    // ERROR: the match information does not correspond (anymore) with the official data
                    // This can happen if the official data has changed (e.g., they changed the match system)
                    // We 'fix' this by ignoring the saved data and creating a new score sheet
                    Initialize(officialData);
                    return;
                }
            }
            Initialize(fullMatch);
            this.IsInitializing = true;
            
            // apply settings
            var defAddress = DatabaseManager.Current.Settings.DefaultAddress.Value;
            if (defAddress.Length > 0)
                this.Address.Value = defAddress;

            this.Article632.Value = DatabaseManager.Current.Settings.DefaultTwoByTwo.Value && (fullMatch.Level != LevelEnum.Super || !DatabaseManager.Current.Settings.DefaultTwoByTwoExceptSuper.Value);

            this.EndHour.Value = fullMatch.EndHour ?? "";
            this.Article632.Value = fullMatch.Article632 ?? this.Article632.Value;
            this.Comments.Value = fullMatch.Comments ?? "";
            this.UniqueId = fullMatch.UniqueId ?? this.UniqueId; // UniqueId can only be null if the user manually changed the file with the JSON data
            this.HomeTeam.Forfeit.Value = fullMatch.HomeTeamForfeit;
            if (fullMatch.HomePlayers != null) {
                for (int i = 0; i < fullMatch.HomePlayers.Count; i++) {
                    Initialize(this.HomeTeam.Players[i], fullMatch.HomePlayers[i]);
                }
            }
            this.AwayTeam.Forfeit.Value = fullMatch.AwayTeamForfeit;
            if (fullMatch.AwayPlayers != null) {
                for (int i = 0; i < fullMatch.AwayPlayers.Count; i++) {
                    Initialize(this.AwayTeam.Players[i], fullMatch.AwayPlayers[i]);
                }
            }
            if (fullMatch.Results != null) {
                for (int i = 0; i < fullMatch.Results.Count; i++) {
                    Initialize(this.Matches[i], fullMatch.Results[i]);
                }
            }
            if (fullMatch.HomeDoubles != null && fullMatch.HomeDoubles.Count > 0) {
                var ret = this.HomeTeam.Players.Where(c => c is DoublePlayerInfo).Cast<DoublePlayerInfo>().ToList();
                for (int i = 0; i < fullMatch.HomeDoubles.Count; i++) {
                    ret[i].SelectedOption.Value = ret[i].AvailableOptions.Where(c => c.Name == fullMatch.HomeDoubles[i]).FirstOrDefault();
                }
            }
            if (fullMatch.AwayDoubles != null && fullMatch.AwayDoubles.Count > 0) {
                var ret = this.AwayTeam.Players.Where(c => c is DoublePlayerInfo).Cast<DoublePlayerInfo>().ToList();
                for (int i = 0; i < fullMatch.AwayDoubles.Count; i++) {
                    ret[i].SelectedOption.Value = ret[i].AvailableOptions.Where(c => c.Name == fullMatch.AwayDoubles[i]).FirstOrDefault();
                }
            }
            if (fullMatch.HomeSubstitutes != null && fullMatch.HomeSubstitutes.Count > 0) {
                var ret = this.HomeTeam.Players.Where(c => c is SubstitutePlayerInfo).Cast<SubstitutePlayerInfo>().ToList();
                for (int i = 0; i < fullMatch.HomeSubstitutes.Count; i++) {
                    ret[i].SelectedTransferMatch.Value = Matches.Where(m => m.Position == fullMatch.HomeSubstitutes[i]).FirstOrDefault();
                }
            }
            if (fullMatch.AwaySubstitutes != null && fullMatch.AwaySubstitutes.Count > 0) {
                var ret = this.AwayTeam.Players.Where(c => c is SubstitutePlayerInfo).Cast<SubstitutePlayerInfo>().ToList();
                for (int i = 0; i < fullMatch.AwaySubstitutes.Count; i++) {
                    ret[i].SelectedTransferMatch.Value = Matches.Where(m => m.Position == fullMatch.AwaySubstitutes[i]).FirstOrDefault();
                }
            }
            Initialize(this.ChiefReferee, fullMatch.ChiefReferee);
            Initialize(this.HomeCaptain, fullMatch.HomeCaptain);
            Initialize(this.AwayCaptain, fullMatch.AwayCaptain);
            Initialize(this.RoomCommissioner, fullMatch.RoomCommissioner);
            this.DisablePartialUpload.Value = fullMatch.DisablePartialUpload ?? false;
            this.MatchStatus.ValueChanged += () => {
                if (this.MatchStatus.Value == ViewModels.Score.MatchStatus.Uploaded)
                    this.DisablePartialUpload.Value = true;
            };
            this.Filename.Value = fromFile;
            this.IsInitializing = false;
            ScoreChanged();
        }

        public CompetitiveMatchViewModel(MatchStartInfo matchInfo) {
            Initialize(matchInfo);
            ScoreChanged();
        }
        protected void Initialize(PersonInfo vmPerson, ModPersonInfo? modPerson) {
            if (modPerson == null)
                return;
            vmPerson.ClubId.Value = modPerson.ClubId ?? "";
            vmPerson.ClubName.Value = modPerson.ClubName ?? "";
            vmPerson.ComputerNumber.Value = modPerson.ComputerNumber ?? "";
            vmPerson.Name.Value = modPerson.Name ?? "";
        }
        protected void Initialize(MatchInfo vmResult, MatchResult? modResult) {
            if (modResult == null || modResult.Sets == null)
                return;
            for (int i = 0; i < modResult.Sets.Count; i++) {
                vmResult.Sets[i].LeftScore.Value = modResult.Sets[i].LeftScore ?? "";
                vmResult.Sets[i].RightScore.Value = modResult.Sets[i].RightScore ?? "";
            }
        }
        protected void Initialize(PlayerInfo vmPlayer, ModPlayerInfo? modPlayer) {
            var spi = vmPlayer as SinglePlayerInfo;
            if (modPlayer == null || spi == null)
                return;
            spi.Captain.Value = modPlayer.Captain ?? false;
            spi.ComputerNumber.Value = modPlayer.ComputerNumber ?? "";
            spi.Index.Value = modPlayer.Index ?? "";
            spi.Name.Value = modPlayer.Name ?? "";
            spi.Ranking.Value = modPlayer.Ranking ?? "";
            spi.StrengthListPosition.Value = modPlayer.Position ?? "";
        }
        protected void Initialize(MatchStartInfo matchInfo) {
            MatchSystem? system = null;
            if (matchInfo.MatchSystemId != null) {
                system = ServiceLocator.Resolve<MatchSystemFactory>()[matchInfo.MatchSystemId.Value];
            }
            if (system == null) {
                throw new NotSupportedException(Safe.Format(Errors.CompetitiveMatch_MatchSystemNotFound, matchInfo.MatchSystemId));
            }
            Initialize(system);
            this.IsInitializing = true;
            this.PlayerCategory = matchInfo.PlayerCategory ?? DatabaseManager.Current.PlayerCategories.Default;
            this.AwayTeam.ClubId.Value = matchInfo.AwayClub ?? "";
            this.AwayTeam.Name.Value = matchInfo.AwayTeam ?? "";
            this.Cup.Value = matchInfo.Cup ?? false;
            this.Date.Value = matchInfo.Date ?? "";
            this.HomeTeam.ClubId.Value = matchInfo.HomeClub ?? "";
            this.HomeTeam.Name.Value = matchInfo.HomeTeam ?? "";
            this.Interclub.Value = matchInfo.Interclub ?? false;
            this.MatchId.Value = matchInfo.MatchId ?? "";
            this.Men.Value = matchInfo.Men ?? false;
            this.Series.Value = matchInfo.Series ?? "";
            this.StartHour.Value = matchInfo.StartHour ?? "";
            this.Super.Value = matchInfo.Super ?? false;
            if (DatabaseManager.Current.Settings.DefaultAddress.Value != "") {
                this.Address.Value = DatabaseManager.Current.Settings.DefaultAddress.Value;
            } else {
                this.Address.Value = matchInfo.Venue ?? "";
            }
            this.Veterans.Value = matchInfo.Veterans ?? false;
            this.Women.Value = matchInfo.Women ?? false;
            this.Youth.Value = matchInfo.Youth ?? false;
            var level = this.AvailableLevels.Where(l => l.Id == matchInfo.Level).SingleOrDefault();
            if (level != null)
                this.Level.Value = level;
            this.Article632.Value = (matchInfo.Level == LevelEnum.Super ? DatabaseManager.Current.Settings.DefaultTwoByTwo.Value && !DatabaseManager.Current.Settings.DefaultTwoByTwoExceptSuper.Value : DatabaseManager.Current.Settings.DefaultTwoByTwo.Value);
            if (this.HomeTeam.ClubId.Value == "-") {
                foreach (var pi in this.HomeTeam.Players) {
                    var spi = pi as SinglePlayerInfo;
                    if (spi != null) {
                        spi.ComputerNumber.Value = "?";
                    }
                }
            }
            if (this.AwayTeam.ClubId.Value == "-") {
                foreach (var pi in this.AwayTeam.Players) {
                    var spi = pi as SinglePlayerInfo;
                    if (spi != null) {
                        spi.ComputerNumber.Value = "?";
                    }
                }
            }
            this.UniqueId = matchInfo.MatchId ?? UniqueId;
            this.IsOfficial.Value = matchInfo.IsOfficial ?? false;
            this.IsInitializing = false;
        }
        protected void Initialize(MatchSystem system) {
            this.IsInitializing = true;
            this.Score = new ScoreCalculator(this);
            this.Score.Result.ValueChanged += () => {
                if (this.Score.Result.Value != Winner.Error && this.Score.Result.Value != Winner.Incomplete && this.EndHour.Value.Length == 0 && !(this.HomeTeam.IsBye.Value || this.AwayTeam.IsBye.Value))
                    this.EndHour.Value = DateTime.Now.ToString("HH:mm");
            };
            this.PlayerCategory = DatabaseManager.Current.PlayerCategories.Men; // we default to 'men' when constructing from a MatchSystem
            this.Men = CreateCell(true);
            this.Women = CreateCell(false);
            CreateXorGroup(this.Men, this.Women);
            this.Men.ValueChanged += MenWomen_ValueChanged; // this must be done after CreateXorGroup!
            this.Women.ValueChanged += MenWomen_ValueChanged; // this must be done after CreateXorGroup!
            this.HomeTeam = new TeamInfo(this, DataChanged);
            this.HomeTeam.Forfeit.ValueChanged += () => ScoreChanged();
            this.HomeTeam.Captain.ValueChanged += () => SetCaptain(this.HomeCaptain, this.HomeTeam.Captain.Value);
            this.AwayTeam = new TeamInfo(this, DataChanged);
            this.AwayTeam.Forfeit.ValueChanged += () => ScoreChanged();
            this.AwayTeam.Captain.ValueChanged += () => SetCaptain(this.AwayCaptain, this.AwayTeam.Captain.Value);
            this.Address = CreateCell("");
            this.Date = CreateCell("");
            this.StartHour = CreateCell("");
            this.EndHour = CreateCell("");
            this.MatchId = CreateCell("");
            this.Series = CreateCell("");
            this.ChiefReferee = new PersonInfo(DataChanged);
            this.HomeCaptain = new PersonInfo(DataChanged);
            this.AwayCaptain = new PersonInfo(DataChanged);
            this.Comments = CreateCell("");
            this.IsOfficial = CreateCell(false);
            this.MatchSystem = (VMMatchSystem)system;
            this.Matches = new ObservableCollection<MatchInfo>();
            this.Matches.CollectionChanged += (s, e) => {
                foreach (var item in e.NewItems) {
                    var mi = item as MatchInfo;
                    if (mi != null) {
                        mi.DataChanged += () => ScoreChanged();
                        mi.DataChanged += () => DataChanged();
                    }
                }
            };
            var vmsystem = system as VMMatchSystem;
            vmsystem?.Initialize(this.HomeTeam, this.AwayTeam, this.Matches);
            this.Dirty = Cell.Create(false, RefreshMatchStatus);
            this.Filename = Cell.Create<string?>(null);
            this.MatchDataUnprotected = Cell.Derived(DatabaseManager.Current.Settings.ProtectMatchInfo, this.IsOfficial, (protect, official) => !protect || !official);
            UniqueId = Guid.NewGuid().ToString("D");

            this.AvailableLevels = LevelInfo.DefaultList;
            this.RoomCommissioner = new PersonInfo(DataChanged);
            this.Level = CreateCell(this.AvailableLevels.Last());
            this.Article632 = CreateCell(true);
            this.Interclub = CreateCell(true);
            this.Super = CreateCell(false);
            this.Cup = CreateCell(false);
            this.Youth = CreateCell(false);
            this.Veterans = CreateCell(false);
            CreateXorGroup(this.Interclub, this.Super, this.Cup, this.Youth, this.Veterans);
            this.MustBePlayed = Cell.Derived(this.HomeTeam.IsBye, this.HomeTeam.Forfeit, this.AwayTeam.IsBye,this.AwayTeam.Forfeit, (hb, hff, ab, aff) => !(hb || hff || ab || aff));
            this.MatchStatus = Cell.Create(ViewModels.Score.MatchStatus.None);
            this.UploadStatus = Cell.Create(ViewModels.Score.UploadStatus.None, RefreshMatchStatus);
            this.DisablePartialUpload = CreateCell(false);
            this.Watermark = Cell.Derived(this.HomeTeam.Name, c => {
                if (!string.IsNullOrWhiteSpace(c)) {
                    var components = c.Split(' ');
                    if (components.Length > 0 && components[components.Length - 1].Length == 1) {
                        return components[components.Length - 1].ToUpper();
                    }
                }
                return "";
            });
            this.IsInitializing = false;
        }

        private void MenWomen_ValueChanged() {
            if (this.Men.Value) {
                this.PlayerCategory = DatabaseManager.Current.PlayerCategories.Men;
            } else {
                this.PlayerCategory = DatabaseManager.Current.PlayerCategories.Women;
            }
        }

        private void CreateXorGroup(params Cell<bool>[] cells) {
            foreach(INotifyPropertyChanged cell in cells) {
                cell.PropertyChanged += (sender, e) => {
                    if (this.IsInitializing)
                        return;
                    var senderCell = sender as Cell<bool>;
                    if (senderCell != null && senderCell.Value) {
                        foreach(var otherCell in cells) {
                            if(otherCell != senderCell) {
                                otherCell.Value = false;
                            }
                        }
                    }
                };
            }
        }

        private void RefreshMatchStatus() {
            if (this.UploadStatus.Value == ViewModels.Score.UploadStatus.Failed) {
                this.MatchStatus.Value = ViewModels.Score.MatchStatus.UploadError;
            } else {
                if (this.Dirty.Value) {
                    this.MatchStatus.Value = ViewModels.Score.MatchStatus.Dirty;
                } else {
                    if (this.IsOfficial.Value) {
                        if (this.IsCompetitive) {
                            this.MatchStatus.Value = ViewModels.Score.MatchStatus.Uploaded;
                        } else {
                            this.MatchStatus.Value = ViewModels.Score.MatchStatus.Emailed;
                        }
                    } else {
                        this.MatchStatus.Value = ViewModels.Score.MatchStatus.Saved;
                    }
                }
            }
        }

        /// <summary>
        /// Updates the captain of the team based on the currently selected captain.
        /// </summary>
        /// <param name="team">The team to update the captain for.</param>
        private void UpdateTeamCaptain(TeamInfo team)
        {
            var captain = team.Players.OfType<SinglePlayerInfo>().FirstOrDefault(p => p.Captain.Value);
            switch (team)
            {
                case TeamInfo ti when ti == HomeTeam:
                    SetCaptain(HomeCaptain, captain);
                    break;
                case TeamInfo ti when ti == AwayTeam:
                    SetCaptain(AwayCaptain, captain);
                    break;
            }
        }

        private void SetCaptain(PersonInfo dest, SinglePlayerInfo? spi) {
            if (spi == null || dest == null)
                return;
            dest.Name.Value = spi.Name.Value;
            dest.ComputerNumber.Value = spi.ComputerNumber.Value;
            dest.ClubId.Value = spi.ParentTeam.ClubId.Value;
            dest.ClubName.Value = DatabaseManager.Current.Clubs[dest.ClubId.Value]?.Name ?? "";
        }
        protected async void DataChanged() {
            if (IsInitializing)
                return;

            // always wait a second before saving; intermediate DataChanged events are ignored
            lock (this) {
                if (_isSaveScheduled)
                    return;

                _isSaveScheduled = true;
            }
            await Task.Delay(1000);
            try {
                if (UniqueId != null && UniqueId.Length > 0) {
                    var exporter = new MatchExporter();
                    if (IsOfficial.Value) {
                        DatabaseManager.Current.OfficialMatches[UniqueId] = exporter.ToMatch(this);
                        Logger.Log(LogType.Debug, Safe.Format(Strings.CompetitiveMatch_MatchSaved, DateTime.Now.ToString("HH:mm:ss")));
                    } else {
                        DatabaseManager.Current.MatchBackup[UniqueId] = exporter.ToMatch(this);
                        Logger.Log(LogType.Debug, Safe.Format(Strings.CompetitiveMatch_MatchBackedUp, DateTime.Now.ToString("HH:mm:ss")));
                    }
                    Dirty.Value = true;
                }
            } catch (Exception e) {
                Logger.Log(e);
            } finally {
                lock (this) {
                    _isSaveScheduled = false;
                }
            }
        }
        public void Close() {
            if (!IsOfficial.Value) {
                DatabaseManager.Current.MatchBackup[UniqueId] = null; // remove backup
            }
        }
        protected void ScoreChanged() {
            if (!this.IsInitializing) {
                this.Score.Refresh();
            }
        }

        public void BrowseForPlayers(TeamInfo team) {
            var wiz = new WizardViewModel();
            var selectPlayers = new SelectPlayersViewModel(wiz, team, team.Players.Select(p => {
                    var spi = p as SinglePlayerInfo;
                    if (spi != null) {
                        return new SelectedMemberInfo() {
                            Name = spi.Name.Value,
                            ComputerNumber = spi.ComputerNumber.Value,
                            IsWO = Score.IsPlayerWO(spi),
                            IsCaptain = spi.Captain.Value
                        };
                    }
                    return null;
                }).Where(c => c != null).ToList(),
                new SelectedMemberInfo(ChiefReferee.Name.Value, ChiefReferee.ComputerNumber.Value),
                new SelectedMemberInfo(RoomCommissioner.Name.Value, RoomCommissioner.ComputerNumber.Value));
            wiz.CurrentPanel.Value = selectPlayers;

            var n = new ShowDialogNotification(wiz);
            NotificationManager.Current.Raise(n);

            if (n.Result) {
                FillPlayerInfo(selectPlayers, team);
            }
        }

        protected void FillPlayerInfo(SelectPlayersViewModel selectPlayers, TeamInfo team) {
            FillPerson(selectPlayers.ChiefReferee, this.ChiefReferee, team);
            FillPerson(selectPlayers.RoomCommissioner, this.RoomCommissioner, team);
            for (int i = 0; i < selectPlayers.Players.Count; i++) {
                FillPlayerInfo(selectPlayers.Players[i], team.Players[i]);
            }

            UpdateTeamCaptain(team);
        }
        protected void FillPerson(SelectedPlayerViewModel selected, PersonInfo dest, TeamInfo team) {
            var member = selected.SelectedPlayer.Value;
            if (member == null && selected.SelectedPlayerName.Value != "")
                return;
            if (member == null) {
                dest.ClubId.Value = "";
                dest.ClubName.Value = "";
                dest.Name.Value = "";
                dest.ComputerNumber.Value = "";
            } else {
                dest.ClubId.Value = team.ClubId.Value;
                dest.ClubName.Value = DatabaseManager.Current.Clubs[team.ClubId.Value]?.Name ?? "";
                dest.Name.Value = $"{ member.Lastname?.ToUpper() } { member.Firstname }";
                dest.ComputerNumber.Value = member.ComputerNumber?.ToString() ?? "";
            }
        }
        protected void FillPlayerInfo(SelectedPlayerViewModel selected, PlayerInfo dest) {
            var member = selected.SelectedPlayer.Value;
            if (member == null && selected.SelectedPlayerName.Value != "")
                return;
            var spi = dest as SinglePlayerInfo;
            if (spi == null)
                return;
            if (member == null) {
                spi.Name.Value = "";
                spi.ComputerNumber.Value = "";
                spi.Index.Value = "";
                spi.Ranking.Value = "";
                spi.StrengthListPosition.Value = "";
                spi.Captain.Value = false;
            } else {
                spi.Name.Value = $"{ member.Lastname?.ToUpper() } { member.Firstname }";
                spi.ComputerNumber.Value = member.ComputerNumber?.ToString() ?? "";
                spi.Index.Value = member.RankIndex?.ToString() ?? "";
                spi.Ranking.Value = member.Ranking ?? "";
                spi.StrengthListPosition.Value = member.Position?.ToString() ?? "";
                spi.Captain.Value = selected.IsCaptain.Value;
            }
            if (selected.IsWO.Value && member != null) {
                var match = dest.ParentTeam.ParentMatch;
                bool home = dest.ParentTeam == match.HomeTeam;
                foreach (var w in match.Matches) {
                    if (w.HomePlayers.Value.Contains(spi) && w.Sets[0].LeftScore.Value.Length == 0)
                        w.Sets[0].LeftScore.Value = "wo";
                    else if (w.AwayPlayers.Value.Contains(spi) && w.Sets[0].RightScore.Value.Length == 0)
                        w.Sets[0].RightScore.Value = "wo";
                }
            } else if (member == null || !selected.IsWO.Value) {
                var match = dest.ParentTeam.ParentMatch;
                bool home = dest.ParentTeam == match.HomeTeam;
                foreach (var w in match.Matches) {
                    if (w.HomePlayers.Value.Contains(spi) && w.Sets[0].LeftScore.Value == "wo")
                        w.Sets[0].LeftScore.Value = "";
                    else if (w.AwayPlayers.Value.Contains(spi) && w.Sets[0].RightScore.Value == "wo")
                        w.Sets[0].RightScore.Value = "";
                }
            }
        }

        protected Cell<T> CreateCell<T>(T initial) {
            return Cell.Create(initial, DataChanged);
        }
        public bool IsCompetitive {
            get {
                if (!this.MatchSystem.IsCompetitive)
                    return false; // uses a match system that is only used for non-competitive matches
                //// This is a hack for seasons 2020-2021; due to Corona, the free time series use a match system that is normally used
                //// in competitive matches; because of this, we cannot use the match system as the indicator of whether the match is competitive
                //// A better way would be to let the interclub leaders add this information on the competition website, and that
                //// we retrieve the info from the website.
                //if (this.Series.Value.IndexOf('6') >= 0 && this.MatchId.Value.StartsWith("PL/K"))
                //    return false;
                return true;
            }
        }
        public IList<LevelInfo> AvailableLevels { get; set; }
        public PersonInfo RoomCommissioner { get; set; }
        public Cell<bool> Article632 { get; set; }
        public Cell<LevelInfo> Level { get; set; } // afdeling
        public Cell<bool> Men { get; set; }
        public Cell<bool> Women { get; set; }
        public Cell<bool> Interclub { get; set; }
        public Cell<bool> Super { get; set; }
        public Cell<bool> Cup { get; set; }
        public Cell<bool> Youth { get; set; }
        public Cell<bool> Veterans { get; set; }
        public VMMatchSystem MatchSystem { get; private set; }
        public Cell<bool> IsOfficial { get; private set; }
        public Cell<string> Address { get; private set; }
        public TeamInfo AwayTeam { get; private set; }
        public TeamInfo HomeTeam { get; private set; }
        public Cell<string> Date { get; private set; }
        public Cell<string> StartHour { get; private set; }
        public Cell<string> EndHour { get; private set; }
        public Cell<string> MatchId { get; private set; }
        public Cell<string> Series { get; private set; } // reeks
        public PersonInfo ChiefReferee { get; private set; }
        public PersonInfo HomeCaptain { get; private set; }
        public PersonInfo AwayCaptain { get; private set; }
        public Cell<string> Comments { get; private set; }
        public ObservableCollection<MatchInfo> Matches { get; private set; }
        public string UniqueId { get; private set; }
        public int PlayerCategory { get; private set; }
        public Cell<bool> Dirty { get; private set; }
        public Cell<string?> Filename { get; private set; }
        public Cell<bool> MatchDataUnprotected { get; private set; }
        public ScoreCalculator Score { get; private set; }
        protected bool IsInitializing { get; private set; }
        public Cell<bool> MustBePlayed { get; private set; }
        public Cell<MatchStatus> MatchStatus { get; private set; }
        public Cell<UploadStatus> UploadStatus { get; private set; }
        public Cell<bool> DisablePartialUpload { get; private set; }

        public Cell<string> Watermark { get; private set; }
        public Cell<bool> ShowWatermark => DatabaseManager.Current.Settings.ShowWatermark;
        public Cell<int> WatermarkSize => DatabaseManager.Current.Settings.WatermarkSize;
        public Cell<double> WatermarkOpacity => DatabaseManager.Current.Settings.WatermarkOpacity;

        private bool _isSaveScheduled;
    }
    public enum MatchStatus {
        None,
        Saved,
        Uploaded,
        Dirty,
        UploadError,
        Emailed
    }
    public enum UploadStatus {
        None,
        Uploaded,
        Failed
    }
}