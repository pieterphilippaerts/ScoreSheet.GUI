using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PieterP.ScoreSheet.Launcher.Database;

namespace PieterP.ScoreSheet.Launcher.Models {
    public class AppVersions : IEnumerable<AppVersion> {
        public AppVersions() {
            _versions = new List<AppVersion>();
            var vd = Directory.GetDirectories(DatabaseManager.Current.VersionsPath);
            foreach (var d in vd) {
                var av = AppVersion.Create(d);
                if (av != null)
                    _versions.Add(av);
            }
        }



        public static AppVersions All {
            get {
                if (_all == null)
                    _all = new AppVersions();
                return _all;
            }
        }

        public IEnumerator<AppVersion> GetEnumerator() => _versions.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _versions.GetEnumerator();

        private List<AppVersion> _versions;
        private static AppVersions _all;
    }
}
