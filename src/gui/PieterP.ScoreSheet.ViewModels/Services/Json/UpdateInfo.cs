using System;
using System.Collections.Generic;
using System.Text;

namespace PieterP.ScoreSheet.ViewModels.Services.Json {
    public class UpdateInfo {
        public string LatestVersion { get; set; }
        public string? InstallFileUrl { get; set; }
        public string? Hash { get; set; }
        public int? Size { get; set; }
        public TranslatedString ExtraInfo { get; set; }
        public UpdateAction[] PostUnzipActions { get; set; }
    }
}