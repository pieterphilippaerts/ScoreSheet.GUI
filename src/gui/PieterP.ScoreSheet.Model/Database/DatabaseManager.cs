using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PieterP.ScoreSheet.Model.Database.Entities;
using PieterP.ScoreSheet.Model.Database.Updater;
using PieterP.ScoreSheet.Model.Information;
using PieterP.Shared;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.Model.Database {
    public class DatabaseManager {
        public static DatabaseManager Current { 
            get {
                return ServiceLocator.Resolve<DatabaseManager>();
            }
        }
        public DatabaseManager(string profile) {
            ActiveProfile = profile;
        }
        public SettingsDatabase Settings { 
            get {
                return _settings.Value;
            }
        }
        private Lazy<SettingsDatabase> _settings = new Lazy<SettingsDatabase>();

        public MemberDatabase Members {
            get {
                return _members.Value;
            }
        }
        private Lazy<MemberDatabase> _members = new Lazy<MemberDatabase>();

        public ClubDatabase Clubs {
            get {
                return _clubs.Value;
            }
        }
        private Lazy<ClubDatabase> _clubs = new Lazy<ClubDatabase>();

        public MatchStartInfoDatabase MatchStartInfo {
            get {
                return _matchStartInfo.Value;
            }
        }
        private Lazy<MatchStartInfoDatabase> _matchStartInfo = new Lazy<MatchStartInfoDatabase>();

        public MatchDatabase OfficialMatches {
            get {
                return _officialMatches.Value;
            }
        }
        private Lazy<MatchDatabase> _officialMatches = new Lazy<MatchDatabase>(() => new MatchDatabase(DatabaseManager.Current.OfficialMatchesPath));

        public MatchDatabase MatchBackup {
            get {
                return _matchBackup.Value;
            }
        }
        private Lazy<MatchDatabase> _matchBackup = new Lazy<MatchDatabase>(() => new MatchDatabase(DatabaseManager.Current.BackupPath));

        public CultureDatabase CultureSettings{
            get {
                return _defaultCulture.Value;
            }
        }
        private Lazy<CultureDatabase> _defaultCulture = new Lazy<CultureDatabase>(() => new CultureDatabase());
        
        public HandicapDatabase Handicap {
            get {
                return _handicap.Value;
            }
        }
        private Lazy<HandicapDatabase> _handicap = new Lazy<HandicapDatabase>(() => new HandicapDatabase());

        public void Export(string file) {
            var mule = new ImportExportMule() {
                HomeClub = Settings.HomeClub.Value,
                HomeClubId = Settings.HomeClubId.Value,
                CurrentSeason = Settings.CurrentSeason.Value,
                Members = Members.Database,
                Clubs = Clubs.Database,
                Matches = MatchStartInfo.Database
            };
            File.WriteAllText(file, DataSerializer.Serialize(mule));
        }
        private class ImportExportMule { 
            public string? HomeClub { get; set; }
            public string? HomeClubId { get; set; }
            public Season? CurrentSeason { get; set; }
            public List<MemberList>? Members { get; set; }
            public List<Club>? Clubs { get; set; }
            public List<MatchStartInfo>? Matches { get; set; }
        }
        public void Import(string file) {
            if (!File.Exists(file))
                throw new FileNotFoundException();
            var mule = DataSerializer.Deserialize<ImportExportMule>(File.ReadAllText(file));            
            if (mule.Members == null || mule.Clubs == null || mule.Matches == null) {
                throw new InvalidDataException("Invalid or missing data in the file.");
            }
            if (mule.HomeClub != null && mule.HomeClubId != null) {
                Settings.HomeClub.Value = mule.HomeClub;
                Settings.HomeClubId.Value = mule.HomeClubId;
            }
            if (mule.CurrentSeason != null) {
                Settings.CurrentSeason.Value = mule.CurrentSeason;
            }
            Members.Update(mule.Members);
            Clubs.Update(mule.Clubs);
            MatchStartInfo.Update(mule.Matches);
        }
        public Task<bool> UpdateClubs() {
            cancellationTokenSource = new CancellationTokenSource();
            var updater = new TabTUpdater();
            return updater.UpdateClubs(cancellationTokenSource.Token);
        }
        public Task<bool> UpdateMatches(Club club, Action<string, bool>? progressCallback = null) {
            cancellationTokenSource = new CancellationTokenSource();
            var updater = new TabTUpdater();
            if (progressCallback != null)
                updater.UpdateProgress += progressCallback;
            return updater.UpdateMatches(club, cancellationTokenSource.Token);
        }
        public Task<bool> RefreshMemberList(string clubId, int category) {
            var updater = new TabTUpdater();
            return updater.RefreshMemberList(clubId, category);
        }
        public void CancelUpdate() {
            if (cancellationTokenSource != null) {
                cancellationTokenSource.Cancel();
            }
        }
        private CancellationTokenSource? cancellationTokenSource;

#region General properties
        public string BasePath {
            get {
                if (_basePath == null) {
                    _basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ScoreSheet");
                    if (!Directory.Exists(_basePath))
                        Directory.CreateDirectory(_basePath);
                }
                return _basePath;
            }
        }
        private string? _basePath;

        public string ProfilesPath {
            get {
                if (_profilesPath == null) {
                    _profilesPath = Path.Combine(BasePath, "profiles");
                    if (!Directory.Exists(_profilesPath))
                        Directory.CreateDirectory(_profilesPath);
                }
                return _profilesPath;
            }
        }
        private string? _profilesPath;

        public string ActiveProfilePath {
            get {
                if (_activeProfilePath == null) {
                    _activeProfilePath = Path.Combine(ProfilesPath, ActiveProfile);
                    if (!Directory.Exists(_activeProfilePath))
                        Directory.CreateDirectory(_activeProfilePath);
                }
                return _activeProfilePath;
            }
        }
        private string? _activeProfilePath;

        public string OfficialMatchesPath {
            get {
                if (_officialMatchesPath == null) {
                    var season = Application.DefaultSeasonId;
                    if (Settings.CurrentSeason.Value != null) {
                        season = Settings.CurrentSeason.Value.Id;
                    }
                    _officialMatchesPath = Path.Combine(ActiveProfilePath, "official", season.ToString());
                    if (!Directory.Exists(_officialMatchesPath))
                        Directory.CreateDirectory(_officialMatchesPath);
                }
                return _officialMatchesPath;
            }
        }
        private string? _officialMatchesPath;

        public string BackupPath {
            get {
                if (_backupPath == null) {
                    _backupPath = Path.Combine(ActiveProfilePath, "backup");
                    if (!Directory.Exists(_backupPath))
                        Directory.CreateDirectory(_backupPath);
                }
                return _backupPath;
            }
        }
        private string? _backupPath;

        public string WwwPath {
            get {
                if (_wwwPath == null) {
                    _wwwPath = Path.Combine(BasePath, "www");
                    if (!Directory.Exists(_wwwPath))
                        Directory.CreateDirectory(_wwwPath);
                }
                return _wwwPath;
            }
        }
        private string? _wwwPath;

        public string TempPath {
            get {
                if (_tempPath == null) {
                    _tempPath = Path.Combine(ActiveProfilePath, "temp");
                    if (!Directory.Exists(_tempPath))
                        Directory.CreateDirectory(_tempPath);
                }
                return _tempPath;
            }
        }
        private string? _tempPath;

        public string VersionsPath {
            get {
                if (_versionsPath == null) {
                    _versionsPath = Path.Combine(BasePath, "versions");
                    if (!Directory.Exists(_versionsPath))
                        Directory.CreateDirectory(_versionsPath);
                }
                return _versionsPath;
            }
        }
        private string? _versionsPath;

        public string ActiveProfile {
            get {
                return _activeProfile;
            }
            set {
                if (value != null && value.Length > 0 && !Path.GetInvalidPathChars().Any(c => value.Contains(c)))
                    _activeProfile = value;
            }
        }
        private string _activeProfile = "default";

        public string GetTempFilename(string extension) {
            if (!extension.StartsWith(".") && extension.Length > 0)
                extension = "." + extension;
            var path = Path.Combine(TempPath, Guid.NewGuid().ToString("N") + extension);
            while (File.Exists(path)) {
                path = Path.Combine(TempPath, Guid.NewGuid().ToString("N") + extension);
            }
            return path;
        }
#endregion
    }
}
