using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PieterP.ScoreSheet.Launcher.Database {
    public class DatabaseManager {
        private static DatabaseManager _manager = new DatabaseManager();

        public static DatabaseManager Current {
            get {
                return _manager;
            }
        }
        public DatabaseManager() {
            //
        }
        public LaunchSettingsDatabase LaunchSettings {
            get {
                if (_launchSettings == null)
                    _launchSettings = new LaunchSettingsDatabase();
                return _launchSettings;
            }
        }
        private LaunchSettingsDatabase _launchSettings;

        public CultureDatabase CultureSettings {
            get {
                if (_defaultCulture == null)
                    _defaultCulture = new CultureDatabase();
                return _defaultCulture;
            }
        }
        private CultureDatabase _defaultCulture;


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
        private string _basePath;

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
        private string _profilesPath;

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
        private string _versionsPath;
    }
}
