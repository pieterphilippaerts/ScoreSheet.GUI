using System;
using System.Collections.Generic;
using System.Text;
using PieterP.ScoreSheet.Model.Database;
using PieterP.Shared.Cells;

namespace PieterP.ScoreSheet.ViewModels.Settings {
    public class AwayMatchesViewModel {
        public AwayMatchesViewModel(Cell<int>? trackCount) {
            this.FollowAway = DatabaseManager.Current.Settings.FollowAway;
            if (trackCount == null)
                trackCount = Cell.Create(0);
            this.TrackCount = trackCount;
            this.IsLiveUpdatesEnabled = Cell.Derived(DatabaseManager.Current.Settings.FollowAway, DatabaseManager.Current.Settings.EnableLiveUpdates, DatabaseManager.Current.Settings.EnableLiveUpdatesForSuperOnly, (fa, lu, so) => !fa || (lu && !so));
        }
        public Cell<bool> FollowAway { get; private set; }
        public Cell<int> TrackCount { get; private set; }
        public Cell<bool> IsLiveUpdatesEnabled { get; private set; }
    }
}
