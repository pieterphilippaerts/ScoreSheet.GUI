using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PieterP.ScoreSheet.Model.Database.Entities;

namespace PieterP.ScoreSheet.Model.Database {
    public class MemberDatabase : AbstractDatabase<List<MemberList>> {
        public MemberDatabase() : base("members.ssjs") {
            _index = new Dictionary<string, List<MemberList>>();
            foreach (var ml in Database) {
                if (ml.ClubId != null && ml.ClubId.Length > 0 && ml.Entries != null) {
                    List<MemberList> clist;
                    if (!_index.TryGetValue(ml.ClubId, out clist)) {
                        clist = new List<MemberList>();
                        _index[ml.ClubId] = clist;
                    }
                    clist.Add(ml);
                }
            }
        }

        public bool ContainsKey(string clubId, int category) {
            if (_index.TryGetValue(clubId, out var l)) {
                if (l.Any(e => e.Category == category))
                    return true;
            }
            return false;
        }
        public MemberList? this[string clubId, int category] {
            get {
                if (clubId == null)
                    return null;
                if (_index.TryGetValue(clubId, out var l)) {
                    return l.FirstOrDefault(e => e.Category == category);
                }
                return null;
            }
        }
        internal virtual void Update(MemberList list, bool save = true) {
            if (list == null)
                return;
            var old = this[list.ClubId!, list.Category!.Value];
            if (old == null) {
                Database.Add(list);
                List<MemberList> il;
                if (!_index.TryGetValue(list.ClubId!, out il)) {
                    il = new List<MemberList>();
                    _index[list.ClubId!] = il;
                }
                il.Add(list);
            } else {
                var il = _index[list.ClubId!];
                il.Remove(old);
                Database.Remove(old);
                il.Add(list);
                Database.Add(list);
            }
            RaiseDataUpdated();
            if (save)
                Save();
        }

        private Dictionary<string, List<MemberList>> _index;
    }
}
