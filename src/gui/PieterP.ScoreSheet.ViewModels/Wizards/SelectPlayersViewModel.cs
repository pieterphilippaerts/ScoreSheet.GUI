using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.Model.Database.Entities;
using PieterP.ScoreSheet.Model.Database.Enums;
using PieterP.ScoreSheet.ViewModels.Notifications;
using PieterP.ScoreSheet.ViewModels.Score;
using PieterP.ScoreSheet.ViewModels.Score.Validations;
using PieterP.Shared;
using PieterP.Shared.Cells;
using PieterP.Shared.Services;
using static PieterP.ScoreSheet.Localization.Errors;
using static PieterP.ScoreSheet.Localization.Strings;

namespace PieterP.ScoreSheet.ViewModels.Wizards {
    public class SelectPlayersViewModel : WizardPanelViewModel {
        public SelectPlayersViewModel(WizardViewModel parent, TeamInfo team, IReadOnlyList<SelectedMemberInfo> players, SelectedMemberInfo chiefReferee, SelectedMemberInfo roomCommissioner) : base(parent) {
            _team = team;
            this.ShowAllPlayers = team.ParentMatch.PlayerCategory != PlayerCategories.Default;
            this.Team = team.Name.Value;
            this.Filter = Cell.Create("");
            _allAvailableMembers = DatabaseManager.Current.Members[team.ClubId.Value, team.ParentMatch.PlayerCategory]?.Entries?.OrderBy(e => e.Position).ToList();
            if (this.ShowAllPlayers) {
                _allMembers = DatabaseManager.Current.Members[team.ClubId.Value, PlayerCategories.Default]?.Entries?.OrderBy(e => e.Position).ToList();
                if (_allMembers == null)
                    this.ShowAllPlayers = false;
            }
            this.ChiefReferee = CreatePlayerVM(chiefReferee, false);
            this.RoomCommissioner = CreatePlayerVM(roomCommissioner, false);
            this.Players = new List<SelectedPlayerViewModel>();
            for (int i = 0; i < team.ParentMatch.MatchSystem.PlayerCount; i++) {
                var vm = CreatePlayerVM(players[i], false);
                vm.IsWO.Value = players[i].IsWO;
                vm.IsWO.ValueChanged += () => OnWO(vm);
                vm.SelectedPlayer.ValueChanged += () => OnPlayerSelected(vm);
                this.Players.Add(vm);
            }

            this.IsRelevant = Cell.Create(true);
            this.AvailablePlayers = Cell.Create(FilterMembers());
            this.Filter.ValueChanged += () => this.AvailablePlayers.Value = FilterMembers();
            this.Select = new RelayCommand(OnSelect);
            this.SwitchToRelevant = new RelayCommand(() => this.IsRelevant.Value = true);
            this.SwitchToAll = new RelayCommand(() => this.IsRelevant.Value = false);
            this.IsRelevant.ValueChanged += () => this.AvailablePlayers.Value = FilterMembers();

            // set the initial rank index
            this.SelectedPlayer = Cell.Create<MemberListItem?>(this.AvailablePlayers.Value?.FirstOrDefault());
            var components = team.Name.Value.Split(' ');
            if (components.Length > 0 && components[components.Length - 1].Length == 1) {
                var teamIndex = components[components.Length - 1].ToUpper()[0];
                if (char.IsLetter(teamIndex)) { // can be different if the club has more than 26 teams
                    int playerIndex = (teamIndex - 'A') * 4 + 1;
                    var searchRank = _allAvailableMembers?.Where(m => m.RankIndex >= playerIndex).FirstOrDefault()?.Ranking;
                    if (searchRank != null) {
                        var rankItem = this.AvailablePlayers.Value?.Where(m => !m.IsSelectable && m.Caption == searchRank).FirstOrDefault();
                        this.SelectedPlayer.Value = rankItem;
                    }
                }
            }
        }
        private Member? FindMember(string compNum) {
            Member? member = null;
            if (int.TryParse(compNum, out var cn)) {
                member = _allAvailableMembers.FirstOrDefault(m => m.ComputerNumber == cn);
                if (this.ShowAllPlayers && member == null)
                    member = _allMembers.FirstOrDefault(m => m.ComputerNumber == cn);
            }
            return member;
        }
        private SelectedPlayerViewModel CreatePlayerVM(SelectedMemberInfo selectedMember, bool showRanking) {
            if (int.TryParse(selectedMember.ComputerNumber, out var cn)) {
                var member = _allAvailableMembers.FirstOrDefault(m => m.ComputerNumber == cn);
                if (this.ShowAllPlayers && member == null)
                    member = _allMembers.FirstOrDefault(m => m.ComputerNumber == cn);
                if (member != null)
                    return new SelectedPlayerViewModel(member, showRanking);
            }
            return new SelectedPlayerViewModel(selectedMember.Name, showRanking);
        }
        private void OnWO(SelectedPlayerViewModel vm) {
            if (vm.IsWO.Value) {
                foreach (var ovm in this.Players) {
                    if (ovm != vm)
                        ovm.IsWO.Value = false;
                }
            }
        }
        private void OnPlayerSelected(SelectedPlayerViewModel vm) {
            if (vm.SelectedPlayer.Value == null)
                return;
            foreach (var ovm in this.Players) {
                if (ovm != vm && ovm.SelectedPlayer.Value == vm.SelectedPlayer.Value)
                    ovm.SelectedPlayer.Value = null;
            }
        }
        private void OnSelect() {
            if (_team.ParentMatch.Level.Value.Id != Level.Super) { // super kan doen wat ze willen :-)
                bool problem = false;
                int prevIndex = -1;
                foreach (var p in this.Players) {
                    int? idx = p.SelectedPlayer.Value?.RankIndex;
                    if (idx != null) {
                        if (idx.Value < prevIndex) {
                            problem = true;
                            break;
                        }
                        prevIndex = idx.Value;
                    }
                }
                if (problem) {
                    var n = new ShowMessageNotification(Wizard_RankWarning, NotificationTypes.Exclamation, NotificationButtons.YesNo);
                    NotificationManager.Current.Raise(n);
                    if (n.Result == false) {
                        return;
                    }
                }
            }
            NotificationManager.Current.Raise(new CloseDialogNotification(true));
        }
        private IEnumerable<MemberListItem>? FilterMembers() {
            if (_allAvailableMembers == null || !_allAvailableMembers.Any())
                return null;

            var filter = Filter.Value;
            IEnumerable<Member>? members = _allAvailableMembers;
            bool showRanks = true;
            if (!this.IsRelevant.Value) {
                members = _allMembers;
                showRanks = false;
            }
            if (filter != null && filter.Length > 0) {
                var filters = filter.ToLower().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                members = members.Where(c => filters.All(f => (c.Firstname != null && c.Firstname.ToLower().Contains(f)) || (c.Lastname != null && c.Lastname.ToLower().Contains(f)) || (c.Ranking != null && c.Ranking.ToLower().Contains(f))));
            }
            var memList = new List<MemberListItem>();
            string? currentRank = null;
            if (members != null) {
                foreach (var member in members) {
                    if (showRanks) {
                        var memRank = GetRank(member.Ranking);
                        if (currentRank != memRank) {
                            memList.Add(new MemberListItem(memRank));
                            currentRank = memRank;
                        }
                    }
                    memList.Add(new MemberListItem(member));
                }
            }
            return memList;
        }

        private string GetRank(string? r) {
            if (r == null || r.Length == 0)
                return "?";
            if (r.StartsWith("A"))
                return "A";
            return r;
        }

        public Cell<MemberListItem?> SelectedPlayer { get; private set; }
        public Cell<IEnumerable<MemberListItem>?> AvailablePlayers { get; private set; }
        public string Team { get; private set; }
        public List<SelectedPlayerViewModel> Players { get; private set; }
        public SelectedPlayerViewModel ChiefReferee { get; private set; }
        public SelectedPlayerViewModel RoomCommissioner { get; private set; }
        public Cell<string> Filter { get; private set; }
        public ICommand Select { get; private set; }
        public Cell<bool> IsRelevant { get; private set; }
        public bool ShowAllPlayers { get; private set; }

        public ICommand SwitchToRelevant { get; private set; }
        public ICommand SwitchToAll { get; private set; }
        public MemberListItem InitialScrollToElement { get; private set; }
        public override string Title => Wizard_SelectPlayers;

        public override string Description => Wizard_SelectPlayersDesc;
        private TeamInfo _team;
        private IEnumerable<Member>? _allAvailableMembers;
        private IEnumerable<Member>? _allMembers;
    }
    public class SelectedMemberInfo {
        public SelectedMemberInfo() {
            this.Name = "";
        }
        public SelectedMemberInfo(string name, string cn) {
            this.Name = name;
            this.ComputerNumber = cn;
        }
        public string Name { get; set; }
        public string ComputerNumber { get; set; }
        public bool IsWO { get; set; }
    }
    public class MemberListItem {
        public MemberListItem(string caption) {
            this.Caption = caption;
            this.IsSelectable = false;
            this.Member = null;
        }
        public MemberListItem(Member member) {
            this.Member = member;
            this.Caption = $"{ member.Position }. { member.Lastname?.ToUpper() } { member.Firstname }";
            if (member.Status != null) {
                if (member.Status.Value == PlayerStatus.RecreantReserve) {
                    this.Caption = this.Caption + $" ({ Strings.PlayerType_RecreationalReserve })";
                } else if (member.Status.Value == PlayerStatus.FreeTime) {
                    this.Caption = this.Caption + $" ({ Strings.PlayerType_FreeTime })";
                }
            }
            this.IsSelectable = true;
        }
        public string Caption { get; private set; }
        public bool IsSelectable { get; private set; }
        public Member? Member { get; set; }
    }

    public class SelectedPlayerViewModel {
        public SelectedPlayerViewModel(Member selected, bool includeRanking = true) : this("", includeRanking) {
            this.SelectedPlayer.Value = selected;
        }
        public SelectedPlayerViewModel(string placeholder, bool includeRanking = true) {
            _placeholder = placeholder ?? "";
            _includeRanking = includeRanking;
            this.IsWO = Cell.Create(false);
            this.SelectedPlayer = Cell.Create<Member?>(null);
            this.IsEmpty = Cell.Derived(this.SelectedPlayer, sp => sp == null);
            this.Drop = new RelayCommand<MemberListItem?>(OnDrop);
            this.Clear = new RelayCommand(OnClear);
            this.SelectedPlayerName = Cell.Derived(this.SelectedPlayer, sp => {
                if (sp == null)
                    return _placeholder;
                else
                    return $"{ sp.Lastname?.ToUpper() } { sp.Firstname }" + (_includeRanking ? $" ({ sp.Ranking })" : "");
            });
        }
        private void OnDrop(MemberListItem? parameter) {
            if (parameter != null)
                this.SelectedPlayer.Value = parameter.Member;
        }
        private void OnClear() {
            this.SelectedPlayer.Value = null;
            this.IsWO.Value = false;
        }
        public Cell<string> SelectedPlayerName { get; private set; }
        public Cell<bool> IsEmpty { get; private set; }
        public Cell<bool> IsWO { get; private set; }
        public Cell<Member?> SelectedPlayer { get; private set; }
        public ICommand Drop { get; private set; }
        public ICommand SetWO { get; private set; }
        public ICommand Clear { get; private set; }

        private string _placeholder;
        private bool _includeRanking;
    }
}
