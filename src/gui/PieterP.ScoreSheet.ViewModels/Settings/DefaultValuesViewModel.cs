using System;
using System.Collections.Generic;
using System.Text;
using PieterP.ScoreSheet.Model.Database;
using PieterP.Shared.Cells;

namespace PieterP.ScoreSheet.ViewModels.Settings {
    public class DefaultValuesViewModel {
        public DefaultValuesViewModel() {
            this.DefaultAddress = DatabaseManager.Current.Settings.DefaultAddress;
            this.DefaultTwoByTwo = DatabaseManager.Current.Settings.DefaultTwoByTwo;
            this.DefaultTwoByTwoExceptSuper = DatabaseManager.Current.Settings.DefaultTwoByTwoExceptSuper;
        }
        public Cell<string> DefaultAddress { get; set; }
        public Cell<bool> DefaultTwoByTwo { get; set; }
        public Cell<bool> DefaultTwoByTwoExceptSuper { get; set; }
    }
}