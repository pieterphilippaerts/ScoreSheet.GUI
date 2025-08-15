using System;
using System.Collections.Generic;
using System.Text;

namespace PieterP.ScoreSheet.ViewModels.Services.Json {
    public class UpdateInfo {
        public string LatestVersion { get; set; }
        public string? InstallFileUrl { get; set; }
        public string? Hash { get; set; }
        public int? Size { get; set; }
        public string? DatabaseCompatibilityVersion { get; set; } // minimum application version whose database is compatible with this ScoreSheet version; if a database update must be forced after an app update, DatabaseCompatibilityVersion will be equal to the updated app version
        public TranslatedString ExtraInfo { get; set; }
        public UpdateAction[] PostUnzipActions { get; set; }
    }
}