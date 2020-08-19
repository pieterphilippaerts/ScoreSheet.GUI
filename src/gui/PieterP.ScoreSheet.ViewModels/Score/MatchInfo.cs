using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PieterP.ScoreSheet.Model;
using PieterP.Shared;
using PieterP.Shared.Cells;

namespace PieterP.ScoreSheet.ViewModels.Score {
    public class MatchInfo {
        private MatchInfo(int position, int setCount, SubstitutePlayerInfo? homeSubstitute = null, SubstitutePlayerInfo? awaySubstitute = null) {
            this.Position = position;
            this.AllowSubstitution = false;
            if (homeSubstitute != null) {
                this.AllowSubstitution = true;
                _homeSubstitute = homeSubstitute;
                homeSubstitute.SelectedTransferMatch.ValueChanged += OnTransfer;
            }
            if (awaySubstitute != null) {
                this.AllowSubstitution = true;
                _awaySubstitute = awaySubstitute;
                awaySubstitute.SelectedTransferMatch.ValueChanged += OnTransfer;
            }

            this.Sets = new List<SetInfo>();
            for (int i = 0; i < setCount; i++) {
                this.Sets.Add(new SetInfo(RaiseDataChanged));
            }
            this.HomeSets = Cell.Create("");
            this.AwaySets = Cell.Create("");
            this.HomeMatches = Cell.Create("");
            this.AwayMatches = Cell.Create("");
            this.HomePlayers = new InterceptedCell<IList<PlayerInfo>>(new List<PlayerInfo>());
            this.HomePlayers.GetValue += HomePlayers_GetValue;
            this.AwayPlayers = new InterceptedCell<IList<PlayerInfo>>(new List<PlayerInfo>());
            this.AwayPlayers.GetValue += AwayPlayers_GetValue;
        }

        private void AwayPlayers_GetValue(InterceptedEventArgs<IList<PlayerInfo>> obj) {
            if (_awaySubstitute != null && _awaySubstitute.SelectedTransferMatch.Value == this) {
                obj.Value = new List<PlayerInfo>() { _awaySubstitute.TransferablePlayer };
            }
        }
        private void HomePlayers_GetValue(InterceptedEventArgs<IList<PlayerInfo>> obj) {
            if (_homeSubstitute != null && _homeSubstitute.SelectedTransferMatch.Value == this) {
                obj.Value = new List<PlayerInfo>() { _homeSubstitute.TransferablePlayer };
            }
        }

        private void OnTransfer() {
            this.HomePlayers.NotifyObservers();
            this.AwayPlayers.NotifyObservers();
        }

        public MatchInfo(int position, IEnumerable<PlayerInfo> homePlayers, IEnumerable<PlayerInfo> awayPlayers, int setCount = 5, SubstitutePlayerInfo? homeSubstitute = null, SubstitutePlayerInfo? awaySubstitute = null) : this(position, setCount, homeSubstitute, awaySubstitute) {
            this.HomePlayers.Value.AddRange(homePlayers);
            this.AwayPlayers.Value.AddRange(awayPlayers);
        }
        public MatchInfo(int position, PlayerInfo homePlayer, PlayerInfo awayPlayer, int setCount = 5, SubstitutePlayerInfo? homeSubstitute = null, SubstitutePlayerInfo? awaySubstitute = null) : this(position, setCount, homeSubstitute, awaySubstitute) {
            this.HomePlayers.Value.Add(homePlayer);
            this.AwayPlayers.Value.Add(awayPlayer);
        }
        public int Position { get; }
        public InterceptedCell<IList<PlayerInfo>> HomePlayers { get; set; }
        public InterceptedCell<IList<PlayerInfo>> AwayPlayers { get; set; }
        public IList<SetInfo> Sets { get; set; }
        public Cell<string> HomeSets { get; set; }
        public Cell<string> AwaySets { get; set; }
        public Cell<string> HomeMatches { get; set; }
        public Cell<string> AwayMatches { get; set; }
        public bool IsEmpty {
            get {
                return Sets.All(s => s.IsEmpty.Value);
            }
        }
        public bool AllowSubstitution { get; set; }

        private SubstitutePlayerInfo _homeSubstitute;
        private SubstitutePlayerInfo _awaySubstitute;

        protected void RaiseDataChanged() {
            DataChanged?.Invoke();
        }
        public event Action DataChanged;

    }
}
