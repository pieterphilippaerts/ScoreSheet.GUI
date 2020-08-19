using System;
using System.Collections.Generic;
using System.Text;

namespace PieterP.ScoreSheet.Model.Database.MatchSystems {
    public class MatchSystemFactory {
        public MatchSystemFactory(IEnumerable<MatchSystem> systems) {
            _systems = new Dictionary<int, MatchSystem>();
            if (systems != null) {
                foreach (var s in systems) {
                    _systems[s.Id] = s;
                }
            }
        }

        public IEnumerable<MatchSystem> Systems {
            get {
                return _systems.Values;
            }
        }

        public MatchSystem? this[int id] {
            get {
                if (_systems.TryGetValue(id, out var ms))
                    return ms;
                return null;
            }
        }

        private Dictionary<int, MatchSystem> _systems;
    }
}
