using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using PieterP.ScoreSheet.Model.Database.Entities;
using PieterP.Shared.Cells;

namespace PieterP.ScoreSheet.ViewModels.Score {
    public class TeamInfo {
        public TeamInfo(CompetitiveMatchViewModel match, Action onDataChanged) {
            this.ParentMatch = match;
            this.Name = Cell.Create("", onDataChanged);
            this.Forfeit = Cell.Create(false, onDataChanged);
            this.ClubId = Cell.Create("", onDataChanged);
            var c = new ObservableCollection<PlayerInfo>();
            c.CollectionChanged += (s, e) => {
                foreach (var item in e.NewItems) {
                    var pi = item as PlayerInfo;
                    if (pi != null) {
                        pi.DataChanged += () => onDataChanged();
                    }
                    var spi = item as SinglePlayerInfo;
                    if (spi != null) {
                        spi.Captain.ValueChanged += () => {
                            if (spi.Captain.Value)
                                this.Captain.Value = spi;
                            else if (!this.Players.Any(p => p is SinglePlayerInfo && ((SinglePlayerInfo)p).Captain.Value))
                                this.Captain.Value = null;
                        };
                    }
                }
            };
            this.Players = c;
            this.IsBye = Cell.Derived(this.ClubId, cid => MatchStartInfo.IsByeIndex(cid));
            this.Captain = Cell.Create<SinglePlayerInfo?>(null);
            this.Forfeit.ValueChanged += Forfeit_ValueChanged;
        }
        private void Forfeit_ValueChanged() {
            if (!this.Forfeit.Value)
                return;
            foreach (var player in Players) {
                var spi = player as SinglePlayerInfo;
                if (spi != null && spi.ComputerNumber.Value.Length == 0) {
                    spi.ComputerNumber.Value = "?";
                }
            }
        }

        public Cell<string> Name { get; private set; }
        public Cell<bool> Forfeit { get; private set; }
        public Cell<string> ClubId { get; private set; }
        public IList<PlayerInfo> Players { get; private set; }
        public Cell<bool> IsBye { get; private set; }
        public CompetitiveMatchViewModel ParentMatch { get; private set; }
        public Cell<SinglePlayerInfo?> Captain { get; private set; }
    }
}