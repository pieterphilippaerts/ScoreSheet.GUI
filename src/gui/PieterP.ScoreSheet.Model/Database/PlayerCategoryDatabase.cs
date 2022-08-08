using PieterP.ScoreSheet.Model.Database.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PieterP.ScoreSheet.Model.Database {
    public class PlayerCategoryDatabase : AbstractDatabase<List<PlayerCategory>>, IEnumerable<PlayerCategory> {
        public PlayerCategoryDatabase() : base("playercats.ssjs") {
            this.DataUpdated += Reset;
        }

        private void Reset(AbstractDatabase<List<PlayerCategory>> source) {
            _men = null;
            _women = null;
        }

        protected override void Initialize() {
            _index = new Dictionary<int, PlayerCategory>();
            foreach (var c in Database) {
                if (c.UniqueIndex != null) {
                    _index[c.UniqueIndex.Value] = c;
                }
            }
        }

        public bool ContainsKey(int categoryId) {
            return _index!.ContainsKey(categoryId);
        }

        public IEnumerator<PlayerCategory> GetEnumerator() {
            return Database.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        public int Default => Men;
        private int? _men;
        public int Men {
            get {
                if (_men == null) {
                    _men = this.FirstOrDefault(cat => (cat.MinimumAge ?? 0) <= 0 && (cat.MaximumAge ?? 100) >= 100 && cat.Sex == "*")?.UniqueIndex;
                    if (_men == null)
                        _men = this.FirstOrDefault(cat => (cat.MinimumAge ?? 0) <= 0 && (cat.MaximumAge ?? 100) >= 100 && cat.Sex == "M")?.UniqueIndex;
                }
                return _men ?? -1;
            }
        }
        private int? _women;
        public int Women {
            get {
                if (_women == null) {
                    _women = this.FirstOrDefault(cat => (cat.MinimumAge ?? 0) <= 0 && (cat.MaximumAge ?? 100) >= 100 && cat.Sex == "F")?.UniqueIndex;
                }
                return _women ?? -1;
            }
        }

        public PlayerCategory? this[int categoryId] {
            get {
                if (_index!.TryGetValue(categoryId, out var c))
                    return c;
                return null;
            }
        }        

        private Dictionary<int, PlayerCategory>? _index;
    }
}