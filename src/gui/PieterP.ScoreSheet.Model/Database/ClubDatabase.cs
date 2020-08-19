using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using PieterP.ScoreSheet.Model.Database.Entities;
using PieterP.ScoreSheet.Model.Database.Enums;

namespace PieterP.ScoreSheet.Model.Database {
    public class ClubDatabase : AbstractDatabase<List<Club>>, IEnumerable<Club> {
        public ClubDatabase() : base("clubs.ssjs") { }
        
        protected override void Initialize() {
            _index = new Dictionary<string, Club>();
            _provIndex = new Dictionary<Province, List<Club>>();
            foreach (var c in Database) {
                if (c.UniqueIndex != null && c.UniqueIndex.Length > 0) {
                    _index[c.UniqueIndex] = c;

                    if (c.Province != null) {
                        List<Club> cl;
                        if (!_provIndex.TryGetValue(c.Province.Value, out cl)) {
                            cl = new List<Club>();
                            _provIndex[c.Province.Value] = cl;
                        }
                        cl.Add(c);
                    }
                }
            }
        }

        public bool ContainsKey(string clubId) {
            return _index!.ContainsKey(clubId);
        }

        public IEnumerator<Club> GetEnumerator() {
            return Database.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        public Club? this[string clubId] { 
            get {
                if (_index!.TryGetValue(clubId, out var c))
                    return c;
                return null;
            }
        }
        public IEnumerable<Club> ByProvince(Province province) {
            if (_provIndex!.TryGetValue(province, out var clubs)) {
                return clubs;
            }
            return Enumerable.Empty<Club>();
        }

        private Dictionary<string, Club>? _index;
        private Dictionary<Province, List<Club>>? _provIndex;
    }
}
