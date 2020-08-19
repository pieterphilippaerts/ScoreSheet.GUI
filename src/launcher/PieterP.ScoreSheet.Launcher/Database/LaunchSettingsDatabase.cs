using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PieterP.ScoreSheet.Launcher.Database {
    public class LaunchSettingsDatabase : AbstractDatabase<LaunchSettings> {
        public LaunchSettingsDatabase() : base("launcher.ssjs") { 
            //
        }

        public string DefaultProfile {
            get {
                return Database.DefaultProfile;
            }
            set {
                Database.DefaultProfile = value;
                Save();
            }
        }
        public Version DefaultVersion {
            get {
                try {
                    if (Database.DefaultVersion == null || Database.DefaultVersion.Length == 0)
                        return null;
                    return new Version(Database.DefaultVersion);
                } catch {
                    return null;
                }
            }
            set {
                Database.DefaultVersion = value?.ToString(3);
                Save();
            }
        }
        public bool SkipLaunchChecks {
            get {
                return Database.SkipLaunchChecks ?? false;
            }
            set {
                Database.SkipLaunchChecks = value;
                Save();
            }
        }
    }
    public class LaunchSettings { 
        public string DefaultProfile { get; set; }
        public bool? SkipLaunchChecks { get; set; }
        public string DefaultVersion { get; set; }
    }
}
