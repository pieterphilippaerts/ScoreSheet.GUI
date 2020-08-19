using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PieterP.Shared.Cells;

namespace PieterP.ScoreSheet.ViewModels.Score {
    public class DoublePlayerInfo : PlayerInfo {
        public DoublePlayerInfo(TeamInfo team, IEnumerable<DoublePlayerOption> options, DoublePlayerOption? selected = null) : base(team) {
            this.AvailableOptions = options;
            if (selected == null)
                this.SelectedOption = Cell.Create(this.AvailableOptions.First(), RaiseDataChanged);
            else
                this.SelectedOption = Cell.Create(selected, RaiseDataChanged);
        }
        public IEnumerable<DoublePlayerOption> AvailableOptions { get; private set; }
        public Cell<DoublePlayerOption> SelectedOption { get; private set; }
        public override PlayerInfoTypes PlayerInfoType => PlayerInfoTypes.Double;
    }
}
