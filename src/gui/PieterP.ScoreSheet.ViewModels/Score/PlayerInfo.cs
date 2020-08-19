using System;
using System.Collections.Generic;
using System.Text;
using PieterP.Shared.Cells;

namespace PieterP.ScoreSheet.ViewModels.Score {
    public abstract class PlayerInfo {
        public PlayerInfo(TeamInfo team) {
            this.ParentTeam = team;
            this.IndividualWins = Cell.Create("");
        }
        // nothing to see here
        public abstract PlayerInfoTypes PlayerInfoType { get; }
        public Cell<string> IndividualWins { get; private set; }
        public TeamInfo ParentTeam { get; private set; }

        protected void RaiseDataChanged() {
            DataChanged?.Invoke();
        }
        public event Action DataChanged;
    }
}
