using System;
using System.Collections.Generic;
using System.Text;
using PieterP.ScoreSheet.Model.Database;
using PieterP.Shared.Cells;

namespace PieterP.ScoreSheet.ViewModels.Settings {
    public class LiveUpdatesViewModel {
        public LiveUpdatesViewModel() {
            this.EnableLiveUpdates = DatabaseManager.Current.Settings.EnableLiveUpdates;
            this.EnableLiveUpdatesForSuperOnly = DatabaseManager.Current.Settings.EnableLiveUpdatesForSuperOnly;
            this.IsMatchTrackingPrevented = Cell.Derived(DatabaseManager.Current.Settings.FollowAway, this.EnableLiveUpdates, this.EnableLiveUpdatesForSuperOnly, 
                (fa, elu, so) => fa && (!elu || so));
        }
        public Cell<bool> EnableLiveUpdates { get; private set; }
        public Cell<bool> EnableLiveUpdatesForSuperOnly { get; private set; }
        public Cell<bool> IsMatchTrackingPrevented { get; private set; }
    }
}
