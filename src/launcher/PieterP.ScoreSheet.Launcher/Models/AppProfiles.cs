using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PieterP.ScoreSheet.Launcher.Database;

namespace PieterP.ScoreSheet.Launcher.Models {
    public class AppProfiles : IEnumerable<AppProfile> {
        public AppProfiles() {
            _profiles = new List<AppProfile>();
            var vd = Directory.GetDirectories(DatabaseManager.Current.ProfilesPath);
            foreach (var d in vd) {
                var av = AppProfile.Create(d);
                if (av != null)
                    _profiles.Add(av);
            }
        }



        public static AppProfiles All {
            get {
                if (_all == null)
                    _all = new AppProfiles();
                return _all;
            }
        }

        public IEnumerator<AppProfile> GetEnumerator() => _profiles.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _profiles.GetEnumerator();

        private List<AppProfile> _profiles;
        private static AppProfiles _all;
    }
}
