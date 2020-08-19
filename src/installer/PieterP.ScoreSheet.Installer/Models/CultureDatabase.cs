using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PieterP.ScoreSheet.Installer.Models {
    public class CultureDatabase {
        public CultureDatabase() {
            _defaultCulture = SupportedCultures.First();
        }

        public string DefaultCulture {
            get {
                return _defaultCulture;
            }
            set {
                if (value == null)
                    return;
                var dc = value.ToLower();
                foreach (var c in SupportedCultures) {
                    if (c.ToLower() == dc) {
                        Save(c);
                        return;
                    }
                }
            }
        }
        private string _defaultCulture;
        private void Save(string culture) {
            var file = Path.Combine(DatabaseManager.Current.BasePath, "culture.ssjs");
            using (var sw = new StreamWriter(file)) {
                sw.WriteLine($"{{ \"DefaultCulture\": \"{ culture }\" }}");
            }
        }

        public IEnumerable<string> SupportedCultures {
            get {
                return new string[] {
                    "en-US",
                    "nl-BE",
                    "fr-BE",
                    //"de-DE",
                };
            }
        }
    }
}
