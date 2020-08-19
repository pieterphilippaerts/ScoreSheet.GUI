using System;
using System.Collections.Generic;
using System.Text;
using PieterP.ScoreSheet.ViewModels.Interfaces;
using PieterP.Shared.Cells;

namespace PieterP.ScoreSheet.ViewModels.Score {
    public class MatchContainerViewModel : IZoomable {
        public MatchContainerViewModel() {
            this.ActiveMatch = Cell.Create<CompetitiveMatchViewModel?>(null);
        }
        
        public Cell<CompetitiveMatchViewModel?> ActiveMatch { get; private set; }
    }
}
