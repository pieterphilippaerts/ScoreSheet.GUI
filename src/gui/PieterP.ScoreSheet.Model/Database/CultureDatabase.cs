using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using PieterP.Shared.Cells;

namespace PieterP.ScoreSheet.Model.Database {
    public class CultureDatabase : AbstractDatabase<CultureSettings> {
        public CultureDatabase() : base(Path.Combine(DatabaseManager.Current.BasePath, "culture.ssjs"), true) {
        }

        public string DefaultCulture {
            get {
                if (Database.DefaultCulture != null) {
                    var dc = Database.DefaultCulture.ToLower();
                    foreach (var c in SupportedCultures) {
                        if (c.ToLower() == dc)
                            return c;
                    }
                }
                return SupportedCultures.First();
            }
            set {
                if (value == null)
                    return;
                var dc = value.ToLower();
                foreach (var c in SupportedCultures) {
                    if (c.ToLower() == dc) {
                        Database.DefaultCulture = c;
                        Save();
                        return;
                    }
                }
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
    public class CultureSettings { 
        public string? DefaultCulture { get; set; }
    }
}
