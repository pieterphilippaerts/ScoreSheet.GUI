using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using PieterP.ScoreSheet.Model.Database.Entities;
using PieterP.ScoreSheet.Model.Database.Enums;
using PieterP.ScoreSheet.Model.Information;
using PieterP.Shared;
using PieterP.Shared.Cells;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.Model.Database {
    public class SettingsDatabase : AbstractDatabase<Settings> {
        public SettingsDatabase() : base("settings.ssjs") {
            this.UniqueId = CreateCell(Database.UniqueId, value => Database.UniqueId = value);
            this.SuccessfulUploads = CreateCell(Database.SuccessfulUploads ?? 0, value => Database.SuccessfulUploads = value);
            this.LatestUploadClubId = CreateCell(Database.LatestUploadClubId ?? Database.HomeClubId, value => Database.LatestUploadClubId = value);
            this.HomeClub = CreateCell(Database.HomeClub, value => Database.HomeClub = value);
            this.HomeClubId = CreateCell(Database.HomeClubId, value => Database.HomeClubId = value);
            this.ZoomLevel = CreateCell<float>(Database.ZoomLevel ?? 1f, value => Database.ZoomLevel = value, v => {
                if (v < 0.75f)
                    return 0.75f;
                if (v > 4f)
                    return 4f;
                return v;
            });
            this.Password = CreateCell<string>(Decrypt(Database.Password) ?? "", value => Database.Password = Encrypt(value));
            this.TabTUsername = CreateCell(Database.TabTUsername ?? "", value => Database.TabTUsername = value);
            this.TabTPassword = CreateCell(Decrypt(Database.TabTPassword) ?? "", value => Database.TabTPassword = Encrypt(value));
            this.DefaultAddress = CreateCell(Database.DefaultAddress ?? "", value => Database.DefaultAddress = value);
            this.DefaultTwoByTwo = CreateCell<bool>(Database.DefaultTwoByTwo ?? true, value => Database.DefaultTwoByTwo = value);
            this.DefaultTwoByTwoExceptSuper = CreateCell<bool>(Database.DefaultTwoByTwoExceptSuper ?? true, value => Database.DefaultTwoByTwoExceptSuper = value);
            this.StartFullScreen = CreateCell<bool>(Database.StartFullScreen ?? false, value => Database.StartFullScreen = value);
            this.TurnOnCapsLock = CreateCell<bool>(Database.TurnOnCapsLock ?? false, value => Database.TurnOnCapsLock = value);
            this.ProtectMatchInfo = CreateCell<bool>(Database.ProtectMatchInfo ?? true, value => Database.ProtectMatchInfo = value);
            this.CurrentSeason = CreateCell<Season?>(Database.CurrentSeason, value => Database.CurrentSeason = value);
            string? lastDir = Database.LastOpenSaveDirectory;
            if (lastDir == null || !Directory.Exists(lastDir))
                lastDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            this.LastOpenSaveDirectory = CreateCell<string>(lastDir, value => Database.LastOpenSaveDirectory = value);
            this.HideNavigation = CreateCell<bool>(Database.HideNavigation ?? false, value => Database.HideNavigation = value);
            this.EnableLiveUpdates = CreateCell<bool>(Database.EnableLiveUpdates ?? true, value => Database.EnableLiveUpdates = value);
            this.EnableLiveUpdatesForSuperOnly = CreateCell<bool>(Database.EnableLiveUpdatesForSuperOnly ?? false, value => Database.EnableLiveUpdatesForSuperOnly = value);
            this.FollowAway = CreateCell<bool>(Database.FollowAway ?? true, value => Database.FollowAway = value);
            string appVer = Application.Version.ToString(3);
            this.UpdateSettingsOnStart = CreateCell<bool>(Database.UpdateSettingsOnStart ?? true, value => Database.UpdateSettingsOnStart = value);
            this.EnableSecondScreen = CreateCell<bool>(Database.EnableSecondScreen ?? false, value => Database.EnableSecondScreen = value);
            this.ChooseScreenAutomatically = CreateCell<bool>(Database.ChooseScreenAutomatically ?? true, value => Database.ChooseScreenAutomatically = value);
            this.ChooseScreenManually = CreateCell<bool>(Database.ChooseScreenManually ?? false, value => Database.ChooseScreenManually = value);
            this.SelectedScreen = CreateCell<string?>(Database.SelectedScreen, value => Database.SelectedScreen = value);
            this.PrintDirect = CreateCell<bool>(Database.PrintDirect ?? true, value => Database.PrintDirect = value);
            this.PrintViaAdobe = CreateCell<bool>(Database.PrintViaAdobe ?? false, value => Database.PrintViaAdobe = value);
            this.PrintSponsors = CreateCell<bool>(Database.PrintSponsors ?? true, value => Database.PrintSponsors = value);
            this.AdobePath = CreateCell<string>(Database.AdobePath ?? "", value => Database.AdobePath = value);
            this.DefaultRefereeLayoutOption = CreateCell(Database.DefaultRefereeLayoutOption ?? RefereeLayoutOptions.Default, value => Database.DefaultRefereeLayoutOption = value);
            this.EnableJsonService = CreateCell<bool>(Database.EnableJsonService ?? false, value => Database.EnableJsonService = value);
            this.JsonServiceHost = CreateCell<string>(Database.JsonServiceHost ?? "127.0.0.1", value => Database.JsonServiceHost = value);
            this.JsonServicePort = CreateCell<int>(Database.JsonServicePort ?? 6221, value => Database.JsonServicePort = value);
            this.ThemePath = CreateCell<string>(Database.ThemePath ?? "/Themes/DefaultTheme.xaml", value => Database.ThemePath = value);
            this.SelectedBackgroundColor = CreateCell<string>(Database.SelectedBackgroundColor ?? "#C2D69B", value => Database.SelectedBackgroundColor = value);
            this.SelectedTextBoxColor = CreateCell<string>(Database.SelectedTextBoxColor ?? "#D1E5AA", value => Database.SelectedTextBoxColor = value);
            this.SelectedTextColor = CreateCell<string>(Database.SelectedTextColor ?? "#365F91", value => Database.SelectedTextColor = value);
            this.SelectedErrorBackgroundColor = CreateCell<string>(Database.SelectedErrorBackgroundColor ?? "#E5B8B7", value => Database.SelectedErrorBackgroundColor = value);
            this.SelectedErrorTextColor = CreateCell<string>(Database.SelectedErrorTextColor ?? "#FF0000", value => Database.SelectedErrorTextColor = value);
            this.ActiveCulture = CreateCell<string>(Database.ActiveCulture ?? DatabaseManager.Current.CultureSettings.DefaultCulture, value => Database.ActiveCulture = value);
            this.EnableAutoUpload = CreateCell<bool>(Database.EnableAutoUpload ?? true, value => Database.EnableAutoUpload = value);
            this.LoadAwayMatches = CreateCell<bool>(Database.LoadAwayMatches ?? false, value => Database.LoadAwayMatches = value);
            this.LoadByes = CreateCell<bool>(Database.LoadByes ?? false, value => Database.LoadByes = value);
            this.ShowResultsInNavigation = CreateCell<bool>(Database.ShowResultsInNavigation ?? true, value => Database.ShowResultsInNavigation = value);
            Version lv;
            if (Database.LatestInstalledVersion == null || !Version.TryParse(Database.LatestInstalledVersion, out lv) || lv < Application.Version) {
                lv = Application.Version;
#if !DEBUG
                Database.LatestInstalledVersion = lv.ToString(3);
                Save();
#endif
            }
            this.LatestInstalledVersion = CreateCell<Version>(lv, value => Database.LatestInstalledVersion = value.ToString(3));
            this.OverviewVisualization = CreateCell(Database.OverviewVisualization ?? ScoreVisualizations.Default, value => Database.OverviewVisualization = value);
            this.SecondScreenVisualization = CreateCell(Database.SecondScreenVisualization ?? ScoreVisualizations.Default, value => Database.SecondScreenVisualization = value);

            this.SmtpHost = CreateCell(Database.SmtpHost ?? "", value => Database.SmtpHost = value);
            this.SmtpPort = CreateCell(Database.SmtpPort ?? 587, value => Database.SmtpPort = value);
            this.SmtpUsername = CreateCell(Database.SmtpUsername ?? "", value => Database.SmtpUsername = value);
            this.SmtpPassword = CreateCell<string>(Decrypt(Database.SmtpPassword) ?? "", value => Database.SmtpPassword = Encrypt(value));
            this.SmtpUseStartTls = CreateCell(Database.SmtpUseStartTls ?? true, value => Database.SmtpUseStartTls = value);
            this.FreeTimeMailFrom = CreateCell(Database.FreeTimeMailFrom ?? "", value => Database.FreeTimeMailFrom = value);
            this.FreeTimeMailTo = CreateCell(Database.FreeTimeMailTo ?? "", value => Database.FreeTimeMailTo = value);
            this.ClubResponsibleInCC = CreateCell(Database.ClubResponsibleInCC ?? false, value => Database.ClubResponsibleInCC = value);
            this.UseHandicap = Cell.Create(false); // do not store this in the settings; always reset to false on startup

            this.ShowWatermark = CreateCell(Database.ShowWatermark ?? false, value => Database.ShowWatermark = value);
            this.WatermarkSize = CreateCell(Database.WatermarkSize ?? 20, value => Database.WatermarkSize = value, c => {
                if (c < 1)
                    return 1;
                if (c > 30)
                    return 30;
                return c;
            });
            this.WatermarkOpacity = CreateCell(Database.WatermarkOpacity ?? 0.02, value => Database.WatermarkOpacity = value, c => {
                if (c < 0.01)
                    return 0.01;
                if (c > 1)
                    return 1;
                return c;
            });
        }
        private Cell<T> CreateCell<T>(T initial, Action<T> setCallback, Func<T, T>? validator = null) {
            var cell = new SettingsCell<T>(initial, setCallback, validator);
            cell.ValueChanged += () => Save();
            return cell;
        }
        private string? Encrypt(string? input) {
            if (input == null || input.Length == 0)
                return null;
            byte[] b = Encoding.UTF8.GetBytes(input);
            byte[] pb = ProtectedData.Protect(b, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(pb);
        }
        private string? Decrypt(string? input) {
            if (input == null || input.Length == 0)
                return null;
            try {
                byte[] pb = Convert.FromBase64String(input);
                byte[] b = ProtectedData.Unprotect(pb, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(b);
            } catch (Exception e) {
                Logger.Log(e);
                return null;
            }
        }
        public Cell<bool> UseHandicap { get; private set; }
        public Cell<Guid?> UniqueId { get; private set; }
        public Cell<int> SuccessfulUploads { get; private set; }
        public Cell<string?> LatestUploadClubId { get; private set; }
        public Cell<string> LastOpenSaveDirectory { get; private set; }
        public Cell<string?> HomeClub { get; private set; }
        public Cell<string?> HomeClubId { get; private set; }
        public Cell<float> ZoomLevel { get; private set; }
        public Cell<string> Password { get; private set; }
        public Cell<string> TabTUsername { get; private set; }
        public Cell<string> TabTPassword { get; private set; }
        public Cell<string> DefaultAddress { get; private set; }
        public Cell<bool> DefaultTwoByTwo { get; private set; }
        public Cell<bool> DefaultTwoByTwoExceptSuper { get; private set; }
        public Cell<bool> StartFullScreen { get; private set; }
        public Cell<bool> TurnOnCapsLock { get; private set; }
        public Cell<bool> ProtectMatchInfo { get; private set; }        
        public Cell<Season?> CurrentSeason { get; private set; }
        public Cell<bool> HideNavigation { get; private set; }
        public Cell<bool> EnableLiveUpdates { get; private set; }
        public Cell<bool> EnableLiveUpdatesForSuperOnly { get; private set; }
        public Cell<bool> FollowAway { get; private set; }
        public Cell<bool> UpdateSettingsOnStart  { get; private set; }
        public Cell<bool> EnableSecondScreen { get; private set; }
        public Cell<bool> ChooseScreenAutomatically { get; private set; }
        public Cell<bool> ChooseScreenManually { get; private set; }
        public Cell<string?> SelectedScreen { get; private set; }
        public Cell<bool> PrintDirect { get; private set; }
        public Cell<bool> PrintViaAdobe { get; private set; }
        public Cell<bool> PrintSponsors { get; private set; }
        public Cell<string> AdobePath { get; private set; }
        public Cell<RefereeLayoutOptions> DefaultRefereeLayoutOption { get; private set; }
        public Cell<bool> ShowResultsInNavigation { get; private set; }
        public Cell<bool> EnableJsonService { get; private set; }
        public Cell<int> JsonServicePort { get; private set; }
        public Cell<string> JsonServiceHost { get; private set; }
        public Cell<string> ThemePath { get; private set; }
        public Cell<string> SelectedBackgroundColor { get; private set; }
        public Cell<string> SelectedTextBoxColor { get; private set; }
        public Cell<string> SelectedTextColor { get; private set; }
        public Cell<string> SelectedErrorBackgroundColor { get; private set; }
        public Cell<string> SelectedErrorTextColor { get; private set; }
        public Cell<string> ActiveCulture { get; private set; }
        public Cell<bool> EnableAutoUpload { get; private set; }
        public Cell<bool> LoadByes { get; private set; }
        public Cell<bool> LoadAwayMatches { get; private set; }
        public Cell<Version> LatestInstalledVersion { get; private set; }
        public Cell<ScoreVisualizations> OverviewVisualization { get; private set; }
        public Cell<ScoreVisualizations> SecondScreenVisualization { get; private set; }
        public Cell<string> SmtpHost { get; private set; }
        public Cell<int> SmtpPort { get; private set; }
        public Cell<string> SmtpUsername { get; private set; }
        public Cell<string> SmtpPassword { get; private set; }
        public Cell<bool> SmtpUseStartTls { get; private set; }
        public Cell<string> FreeTimeMailFrom { get; private set; }
        public Cell<string> FreeTimeMailTo { get; private set; }
        public Cell<bool> ClubResponsibleInCC { get; private set; }
        public Cell<bool> ShowWatermark { get; private set; }
        public Cell<int> WatermarkSize { get; private set; }
        public Cell<double> WatermarkOpacity { get; private set; }

        private class SettingsCell<T> : ConcreteCell<T> {
            public SettingsCell(T initial, Action<T> setCallback, Func<T, T>? validator = null) : base(initial) {
                _setCallback = setCallback;
                _validator = validator;
            }

            public override T Value {
                get {
                    return base.Value;
                }
                set {
                    if (_validator != null)
                        value = _validator(value);
                    InternalSetValue(value);
                    _setCallback(value);
                    NotifyObservers();
                }
            }

            private Action<T> _setCallback;
            private Func<T, T>? _validator;
        }
    }
}
