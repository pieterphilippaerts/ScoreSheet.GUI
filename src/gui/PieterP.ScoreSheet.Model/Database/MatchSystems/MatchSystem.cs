using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PieterP.ScoreSheet.Model.Database.Entities;

namespace PieterP.ScoreSheet.Model.Database.MatchSystems {
    public abstract class MatchSystem {
        public abstract int Id { get; }
        public abstract int SetCount { get; }
        public abstract int PointCount { get; }
        public abstract int PlayerCount { get; }
        public abstract int MatchCount { get; }
        public abstract string Name { get; }
        public abstract bool IsCompetitive { get; }
    }
}