using System;
using System.Collections.Generic;
using System.Text;
using PieterP.ScoreSheet.Model.Database.Enums;

namespace PieterP.ScoreSheet.Model.Database.Entities {
    public class Settings {
        public Guid? UniqueId { get; set; }
        public int? SuccessfulUploads { get; set; }
        public string? LatestUploadClubId { get; set; }
        public string? HomeClub { get; set; }
        public string? HomeClubId { get; set; }
        public float? ZoomLevel { get; set; }
        public bool? HideNavigation { get; set; }

        public string? Password { get; set; }

        public string? TabTUsername { get; set; }
        public string? TabTPassword { get; set; }

        public string? DefaultAddress { get; set; }
        public bool? DefaultTwoByTwo { get; set; }
        public bool? DefaultTwoByTwoExceptSuper { get; set; }

        public bool? StartFullScreen { get; set; }
        public bool? TurnOnCapsLock { get; set; }
        
        public bool? ProtectMatchInfo { get; set; }
        public Season? CurrentSeason { get; set; }
        public string? LastOpenSaveDirectory { get; set; }

        public bool? EnableLiveUpdates { get; set; }
        public bool? EnableLiveUpdatesForSuperOnly { get; set; }
        public bool? FollowAway { get; set; }
        public bool? UpdateSettingsOnStart { get; set; }

        public bool? EnableSecondScreen { get; set; }
        public bool? ChooseScreenAutomatically { get; set; }
        public bool? ChooseScreenManually { get; set; }
        public string? SelectedScreen { get; set; }
        public bool? PrintDirect { get; set; }
        public bool? PrintViaAdobe { get; set; }
        public bool? PrintSponsors { get; set; }
        public string? AdobePath { get; set; }
        public RefereeLayoutOptions? DefaultRefereeLayoutOption { get; set; }
        public bool? EnableJsonService { get; set; }
        public string? JsonServiceHost { get; set; }
        public int? JsonServicePort { get; set; }
        public string? ThemePath { get; set; }
        public string? SelectedBackgroundColor { get; set; }
        public string? SelectedTextBoxColor { get; set; }
        public string? SelectedTextColor { get; set; }
        public string? SelectedErrorBackgroundColor { get; set; }
        public string? SelectedErrorTextColor { get; set; }
        public bool? ShowResultsInNavigation { get; set; }

        public string? ActiveCulture { get; set; }
        public bool? EnableAutoUpload { get; set; }
        public bool? LoadByes { get; set; }
        public bool? LoadAwayMatches { get; set; }
        public string? LatestInstalledVersion { get; set; }
        public ScoreVisualizations? OverviewVisualization { get; set; }
        public ScoreVisualizations? SecondScreenVisualization { get; set; }

        public string? SmtpHost { get; set; }
        public int? SmtpPort { get; set; }
        public string? SmtpUsername { get; set; }
        public string? SmtpPassword { get; set; }
        public bool? SmtpUseStartTls { get; set; }
        public string? FreeTimeMailFrom { get; set; }
        public string? FreeTimeMailTo { get; set; }
        public bool? ClubResponsibleInCC { get; set; }        
    }
}