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

            // Populate the player view models.
            ChiefReferee = CreatePlayerVM(chiefReferee, false);
            RoomCommissioner = CreatePlayerVM(roomCommissioner, false);
            Players = ConvertMembersToSelectedPlayerVm(players);

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

        /// <summary>
        /// Converts a set of SelectedMemberInfo instanced into a list of SelectedPlayerViewModel instances.
        /// </summary>
        /// <param name="selectedMemberInfos">The set of SelectedMemberInfo instances.</param>
        /// <returns>A list of SelectedPlayerViewModel instances.</returns>
        private List<SelectedPlayerViewModel> ConvertMembersToSelectedPlayerVm(IEnumerable<SelectedMemberInfo> selectedMemberInfos)
        {
            return selectedMemberInfos.Select(memberInfo =>
            {
                var playerViewModel = CreatePlayerVM(memberInfo, false);
                playerViewModel.IsWO.Value = memberInfo.IsWO;
                playerViewModel.IsWO.ValueChanged += () => OnWOChanged(playerViewModel);
                playerViewModel.IsCaptain.Value = memberInfo.IsCaptain;
                playerViewModel.IsCaptain.ValueChanged += () => OnCaptainChanged(playerViewModel);
                playerViewModel.SelectedPlayer.ValueChanged += () => OnPlayerSelected(playerViewModel);
                return playerViewModel;
            }).ToList();
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

        /// <summary>
        /// Appends the player to the next available spot in the team.
        /// </summary>
        /// <param name="memberListItem">The related member list item to add the player from.</param>
        public void AppendPlayer(MemberListItem memberListItem)
        {
            var firstAvailablePlayer = Players.FirstOrDefault(p => p.SelectedPlayer.Value == null);
            firstAvailablePlayer?.SetMember(memberListItem.Member);
        }

        /// <summary>
        /// Called when the WO value of a SelectedPlayerViewModel changes.
        /// </summary>
        /// <param name="vm">The SelectedPlayerViewModel instance that changed.</param>
        private void OnWOChanged(SelectedPlayerViewModel vm) {
            if (vm.IsWO.Value) {
                foreach (var ovm in this.Players) {
                    if (ovm != vm)
                        ovm.IsWO.Value = false;
                }

                // Ensure that when a player is set as WO, the player can't be the captain.
                vm.IsCaptain.Value = false;
            }
        }

        /// <summary>
        /// Called when the IsCaptain value of a SelectedPlayerViewModel changes.
        /// </summary>
        /// <param name="vm">The SelectedPlayerViewModel instance that changed.</param>
        private void OnCaptainChanged(SelectedPlayerViewModel vm)
        {
            // When a player is set as the captain, the other player should be unset as captain.
            if (vm.IsCaptain.Value)
            {
                // We do a FindAll here to ensure that a single captain is selected.
                Players.FindAll(p => p != vm && p.IsCaptain.Value).ForEach(p => p.IsCaptain.Value = false);
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
            Name = string.Empty;
        }
        public SelectedMemberInfo(string name, string cn) {
            Name = name;
            ComputerNumber = cn;
        }
        public string Name { get; set; }
        public string ComputerNumber { get; set; }
        public bool IsWO { get; set; }
        public bool IsCaptain { get; set; }
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

    /// <summary>
    /// Represents the view model of a selected player in a match.
    /// </summary>
    public class SelectedPlayerViewModel {

        private string _placeholder;
        private bool _includeRanking;

        #region Constructors

        /// <summary>
        /// Creates a new instance of the view model.
        /// </summary>
        /// <param name="selected">The selected member.</param>
        /// <param name="includeRanking">Whether the ranking of the member should be included in the player name.</param>
        public SelectedPlayerViewModel(Member selected, bool includeRanking = true) : this(string.Empty, includeRanking) {
            SelectedPlayer.Value = selected;
        }

        /// <summary>
        /// Creates a new instance of hte view model.
        /// </summary>
        /// <param name="placeholder">The placeholder to display when there is no selected player.</param>
        /// <param name="includeRanking">Whether the ranking of the member should be included in the player name.</param>
        public SelectedPlayerViewModel(string placeholder, bool includeRanking = true) {
            _placeholder = placeholder ?? "";
            _includeRanking = includeRanking;

            IsWO = Cell.Create(false);
            IsCaptain = Cell.Create(false);
            SelectedPlayer = Cell.Create<Member?>(null);
            IsEmpty = Cell.Derived(this.SelectedPlayer, sp => sp == null);
            Drop = new RelayCommand<MemberListItem?>(OnDrop);
            Clear = new RelayCommand(OnClear);
            SelectedPlayerName = Cell.Derived(this.SelectedPlayer, sp => {
                if (sp == null)
                    return _placeholder;
                else
                    return $"{ sp.Lastname?.ToUpper() } { sp.Firstname }" + (_includeRanking ? $" ({ sp.Ranking })" : "");
            });
        }

        #endregion Constructors

        #region Command handlers

        /// <summary>
        /// Sets the dropped member as the selected player.
        /// </summary>
        /// <param name="parameter">The list item containing the dropped member.</param>
        private void OnDrop(MemberListItem? parameter) {
            if (parameter != null)
                SetMember(parameter.Member);
        }

        /// <summary>
        /// Clears the state of the view model.
        /// </summary>
        private void OnClear() {
            SelectedPlayer.Value = null;
            IsWO.Value = false;
            IsCaptain.Value = false;
        }

        #endregion Command handlers

        #region Functions

        /// <summary>
        /// Sets the member as the selected player.
        /// </summary>
        /// <param name="member">The member to set.</param>
        public void SetMember(Member? member)
        {
            SelectedPlayer.Value = member;
        }

        #endregion

        #region Cells

        /// <summary>
        /// The name of the selected player.
        /// </summary>
        public Cell<string> SelectedPlayerName { get; private set; }

        /// <summary>
        /// Whether the view model has no selected player.
        /// </summary>
        public Cell<bool> IsEmpty { get; private set; }

        /// <summary>
        /// Whether the player is walk-over.
        /// </summary>
        public Cell<bool> IsWO { get; private set; }

        /// <summary>
        /// Whether the player is the captain of the team in the match.
        /// </summary>
        public Cell<bool> IsCaptain { get; private set; }

        /// <summary>
        /// The selected player.
        /// </summary>
        public Cell<Member?> SelectedPlayer { get; private set; }

        #endregion Cells

        #region Commands

        /// <summary>
        /// The command for when a MemberListItem is dropped on the view model.
        /// </summary>
        public ICommand Drop { get; private set; }

        /// <summary>
        /// The command for when the view model should be cleared.
        /// </summary>
        public ICommand Clear { get; private set; }

        #endregion Commands
    }
}
