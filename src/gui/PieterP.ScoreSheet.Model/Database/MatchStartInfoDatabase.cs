using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PieterP.ScoreSheet.Model.Database.Entities;
using PieterP.Shared;

namespace PieterP.ScoreSheet.Model.Database {
    public class MatchStartInfoDatabase : AbstractDatabase<List<MatchStartInfo>> {
        public MatchStartInfoDatabase() : base("matchinfo.ssjs") {
        }
        protected override void Initialize() {
            base.Initialize();
            _idIndex = new Dictionary<string, MatchStartInfo>();
            _dateIndex = new Dictionary<DateTime, List<MatchStartInfo>>();
            _byes = new Dictionary<DateTime, List<MatchStartInfo>>();
            _orphans = new List<MatchStartInfo>();
            foreach (var c in Database) {
                // fill index by ID
                if (c.MatchId != null && c.MatchId.Length > 0)
                    _idIndex[c.MatchId] = c;
                var date = c.Date.ToDate();
                if (date != null) {
                    // fill index by date
                    List<MatchStartInfo> mad;
                    if (!_dateIndex.TryGetValue(date.Value, out mad)) {
                        mad = new List<MatchStartInfo>();
                        _dateIndex[date.Value] = mad;
                    }
                    mad.Add(c);
                } else if (c.WeekStart != null) {
                    // fill index of byes (by weekstart)
                    List<MatchStartInfo> bm;
                    if (!_byes.TryGetValue(c.WeekStart.Value, out bm)) {
                        bm = new List<MatchStartInfo>();
                        _byes[c.WeekStart.Value] = bm;
                    }
                    bm.Add(c);
                } else {
                    // add to list of matches that have no date and for which no weekstart could be calculated
                    // we have no idea when these matches should be scheduled!
                    _orphans.Add(c);
                }
            }
            // right now, it is possible that there's a bye-match for a given week
            // but that there's no other (normal) match during that week.
            // this means the bye match won't show up in GetMatchDates() list,
            // because this list only shows the dates of the regular matches.
            // in addition, it doesn't show up in the orphan matches either, because
            // the bye match has a start week.
            // we fix this by looping over all the bye's and add them to the orphan
            // match list if there is no corresponding day in GetMatchDates() for which
            // the bye would show up
            foreach (var byeDate in _byes.Keys) {
                // check whether the key has a corresponding date in _dateIndex
                bool found = false;
                for (int i = 0; i < 7; i++) {
                    if (_dateIndex.ContainsKey(byeDate.AddDays(i))) {
                        found = true;
                        break;
                    }
                }
                if (!found) {
                    // we found orphan matches
                    _orphans.AddRange(_byes[byeDate]);
                }
            }
        }

        public bool ContainsKey(string matchId) {
            return _idIndex.ContainsKey(matchId);
        }
        public MatchStartInfo? this[string matchId] {
            get {
                if (_idIndex.TryGetValue(matchId, out var m))
                    return m;
                return null;
            }
        }
        public IEnumerable<MatchStartInfo> GetMatchesAtDate(DateTime date, bool homeOnly, bool includeBye) {
            string? homeClub = DatabaseManager.Current.Settings.HomeClubId.Value;
            if (homeClub == null)
                return Enumerable.Empty<MatchStartInfo>();
            
            List<MatchStartInfo> ret;
            if (_dateIndex.TryGetValue(date, out var list)) {
                ret = list.Where(m => !homeOnly || m.HomeClub == homeClub).ToList();
            } else {
                ret = new List<MatchStartInfo>();
            }
            if (includeBye) {
                if (_byes.TryGetValue(date.FindStartOfWeek(), out var byeList)) {
                    ret.AddRange(byeList);
                }
            }
            return ret;
        }
        public IEnumerable<DateTime> GetMatchDates() {
            return _dateIndex.Keys;
        }
        public IEnumerable<MatchStartInfo> GetOrphanMatches() {
            return _orphans;
        }

        private Dictionary<string, MatchStartInfo> _idIndex;
        private Dictionary<DateTime, List<MatchStartInfo>> _dateIndex;
        private Dictionary<DateTime, List<MatchStartInfo>> _byes;
        private List<MatchStartInfo> _orphans;
    }
}
