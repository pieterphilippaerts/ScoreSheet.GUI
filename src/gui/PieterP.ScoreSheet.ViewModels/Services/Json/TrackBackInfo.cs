using System;
using System.Collections.Generic;
using System.Text;
using PieterP.ScoreSheet.Model.Information;

namespace PieterP.ScoreSheet.ViewModels.Services.Json {
    public class TrackBackInfo {
        public Guid? UniqueId { get; set; }
        public int SuccessfulUploads { get; set; }
        public string? LatestUploadClubId { get; set; }
        public string? ClubId { get; set; }
        public string? AppVersion { get; set; }
        public string? OsVersion { get; set; }
        public string? OsVersionString { get; set; }
        public string? ScreenResolution { get; set; }
        public bool? HasMultipleScreens { get; set; }
        public long? MemorySize { get; set; }
        public string? CpuType { get; set; }
        public string? Architecture { get; set; }
        //public IEnumerable<Model.Information.DriveInfo>? Drives { get; set; }
        public RuntimeInformation? Runtimes { get; set; }
        public string? ActiveCulture { get; set; }
        public bool PrintsDirectly { get; set; }
        public bool SecondScreenEnabled { get; set; }
        public bool LiveUpdatesEnabled { get; set; }
        public bool FollowAwayEnabled { get;set;}
        public bool WebServiceEnabled { get; set; }
    }
}
