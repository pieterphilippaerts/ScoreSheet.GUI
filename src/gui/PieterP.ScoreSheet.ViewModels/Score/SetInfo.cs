using System;
using System.Collections.Generic;
using System.Text;
using PieterP.Shared.Cells;

namespace PieterP.ScoreSheet.ViewModels.Score {
    public class SetInfo {
        public SetInfo(Action onDataChanged) {
            this.IsValid = Cell.Create(true);
            this.LeftScore = Cell.Create("", onDataChanged);
            this.RightScore = Cell.Create("", onDataChanged);
            this.IsEmpty = Cell.Derived(this.LeftScore, this.RightScore, (l, r) => l == "" && r == "");
        }
        public Cell<bool> IsValid { get; set; }
        public Cell<string> LeftScore { get; set; }
        public Cell<string> RightScore { get; set; }
        public Cell<bool> IsEmpty { get; private set; }
    }
}
