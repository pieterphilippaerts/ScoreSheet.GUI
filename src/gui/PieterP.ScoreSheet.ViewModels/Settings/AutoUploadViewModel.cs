using System;
using System.Collections.Generic;
using System.Text;
using PieterP.ScoreSheet.Model.Database;
using PieterP.Shared.Cells;

namespace PieterP.ScoreSheet.ViewModels.Settings {
    public class AutoUploadViewModel {
        public AutoUploadViewModel() {
            this.EnableAutoUpload = DatabaseManager.Current.Settings.EnableAutoUpload;
        }
        public Cell<bool> EnableAutoUpload { get; private set; }
    }
}
