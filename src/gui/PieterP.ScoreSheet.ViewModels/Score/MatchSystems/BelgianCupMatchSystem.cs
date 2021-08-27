using System;
using System.Collections.Generic;
using System.Text;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model.Database.MatchSystems;

namespace PieterP.ScoreSheet.ViewModels.Score.MatchSystems {
    public class BelgianCupMatchSystem : VMMatchSystem {
        public override void Initialize(TeamInfo home, TeamInfo away, IList<MatchInfo> matches) {
            var h = home.Players;
            var a = away.Players;
            // init teams
            for (int i = 1; i <= 3; i++) {
                h.Add(new SinglePlayerInfo(home, i.ToString()));
                a.Add(new SinglePlayerInfo(away, i.ToString()));
            }
            h.Add(new DoublePlayerInfo(home, DoublePlayerOption.Permutation123));
            a.Add(new DoublePlayerInfo(away, DoublePlayerOption.Permutation123));
            // init matches
            matches.Add(new MatchInfo(1, h[0], a[1]));
            matches.Add(new MatchInfo(2, h[1], a[0]));
            matches.Add(new MatchInfo(3, h[2], a[2]));
            matches.Add(new MatchInfo(4, h[3], a[3]));
            matches.Add(new MatchInfo(5, h[0], a[0]));
            matches.Add(new MatchInfo(6, h[2], a[1]));
            matches.Add(new MatchInfo(7, h[1], a[2]));
        }

        public override int Id => 5;
        public override int SetCount => 3;
        public override int PointCount => 11;
        public override int PlayerCount => 3;
        public override int MatchCount => 7;
        public override int SingleMatchCount => 6;
        public override int DoubleMatchCount => 1;
        public override string Name => Strings.MatchSystem_BelgianCup;
        public override bool IsCompetitive => true;
    }
}
