using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PieterP.ScoreSheet.Launcher.Database;

namespace PieterP.ScoreSheet.Launcher.Models {
    public class AppProfile {
        private AppProfile(DirectoryInfo di) {
            this.Name = di.Name;
            this.CreatedOn = di.CreationTime;
            this.FullPath = di.FullName;
        }
        private AppProfile(string name) {
            this.Name = name;
            this.CreatedOn = DateTime.Now;
            this.FullPath = Path.Combine(DatabaseManager.Current.ProfilesPath, name);
        }
        public static AppProfile Create(string path) {
            var di = new DirectoryInfo(path);
            if (!di.Exists)
                return null;
            return new AppProfile(di);
        }
        public static AppProfile CreateNew(string name) {
            if (name == null || name == "")
                return null;
            return new AppProfile(name);
        }

        public string Name { get; private set; }
        public string FullPath { get; private set; }
        public DateTime CreatedOn { get; private set; }
    }
}
