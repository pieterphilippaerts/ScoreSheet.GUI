using System;
using System.Collections.Generic;
using System.Text;

namespace PieterP.ScoreSheet.ViewModels.Services.Json {
    public class UpdateAction {
        public UpdateActionTypes Type { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; } // only used for MoveDirectory and MoveFile
    }
    public enum UpdateActionTypes : int {
        ExecuteFileNonBlocking = 0,
        ExecuteFileBlocking = 1,
        MoveDirectory = 2,
        MoveFile = 3,
        DeleteDirectory = 4,
        DeleteFile = 5,
        RedirectToUrl = 6
    }
}