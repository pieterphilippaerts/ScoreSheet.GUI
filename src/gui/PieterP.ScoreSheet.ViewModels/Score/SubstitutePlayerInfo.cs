using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using PieterP.ScoreSheet.Localization;
using PieterP.Shared.Cells;

namespace PieterP.ScoreSheet.ViewModels.Score {
    public class SubstitutePlayerInfo : PlayerInfo {
        public SubstitutePlayerInfo(TeamInfo team, SinglePlayerInfo transferablePlayer) : base(team) {
            this.TransferablePlayer = transferablePlayer;
            this.SelectedTransferMatch = Cell.Create<MatchInfo?>(null, RaiseDataChanged);
            this.AvailableTransferMatches = new ObservableCollection<SubstituteMatchInfo>();
            this.AvailableTransferMatches.Add(new SubstituteMatchInfo(Strings.SubstitutePlayerInfo_NoSubstitute, null));
            foreach (var tm in team.ParentMatch.Matches.Where(m => m.AllowSubstitution)) {
                this.AvailableTransferMatches.Add(new SubstituteMatchInfo(Safe.Format(Strings.SubstitutePlayerInfo_Substitute, tm.Position), tm));
            }
            team.ParentMatch.Matches.CollectionChanged += Matches_CollectionChanged;
        }

        private void Matches_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (e.OldItems != null) {
                foreach (var o in e.OldItems) {
                    var smi = this.AvailableTransferMatches.Where(c => c.Match == o).FirstOrDefault();
                    if (smi != null) {
                        this.AvailableTransferMatches.Remove(smi);
                    }
                }
            }
            if (e.NewItems != null) {
                foreach (var o in e.NewItems) {
                    var mi = o as MatchInfo;
                    if (mi != null && mi.AllowSubstitution) {
                        this.AvailableTransferMatches.Add(new SubstituteMatchInfo(Safe.Format(Strings.SubstitutePlayerInfo_Substitute, mi.Position), mi));
                    }
                }
            }
        }

        public ObservableCollection<SubstituteMatchInfo> AvailableTransferMatches { get; private set; }
        public Cell<MatchInfo?> SelectedTransferMatch { get; private set; }
        public SinglePlayerInfo TransferablePlayer { get; private set; }

        public override PlayerInfoTypes PlayerInfoType {
            get {
                return PlayerInfoTypes.Transfer;
            }
        }

        public class SubstituteMatchInfo {
            public SubstituteMatchInfo(string name, MatchInfo? match) {
                this.Name = name;
                this.Match = match;
            }
            public string Name { get; set; }
            public MatchInfo? Match { get; set; }
        }
    }
}
