using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PieterP.ScoreSheet.Model.Database.Entities;
using PieterP.Shared;

namespace PieterP.ScoreSheet.Model.Database {
    public class MatchDatabase {
        public MatchDatabase(string path) {
            _databasePath = path;
        }
        public Match? this[string? id] {
            get {
                if (id == null)
                    return null; 
                var sanitized = id.SanitizeForFilename();
                if (sanitized == null)
                    return null;
                string file = Path.Combine(_databasePath, sanitized + ".ssjs");
                return Match.FromFile(file);
            }
            set {
                var sanitized = id.SanitizeForFilename();
                if (sanitized == null)
                    return;
                string file = Path.Combine(_databasePath, sanitized + ".ssjs");
                if (value == null) {
                    if (File.Exists(file)) {
                        File.Delete(file);
                    }
                    return;
                }
                Match.ToFile(value, file);
            }
        }
        public IEnumerable<string> MatchIds { 
            get {
                return Directory.GetFiles(_databasePath, "*.ssjs").Select(r => {
                        string filename = new FileInfo(r).Name;
                        return filename.Substring(0, filename.Length - 5);
                    });
            }
        }

        private string _databasePath;
    }
}
