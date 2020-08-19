using System;
using System.Collections.Generic;
using System.Text;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model.Database.MatchSystems;

namespace PieterP.ScoreSheet.ViewModels.Score.MatchSystems {
    public class Youth2v2MatchSystem : VMMatchSystem {
        public override void Initialize(TeamInfo home, TeamInfo away, IList<MatchInfo> matches) {
            var h = home.Players;
            var a = away.Players;
            // init teams
            for (int i = 1; i <= 2; i++) {
                h.Add(new SinglePlayerInfo(home, i.ToString()));
                a.Add(new SinglePlayerInfo(away, i.ToString()));
            }
            h.Add(new DoublePlayerInfo(home, DoublePlayerOption.Permutation12));
            a.Add(new DoublePlayerInfo(away, DoublePlayerOption.Permutation12));

            // init matches
            matches.Add(new MatchInfo(1, h[1], a[0]));
            matches.Add(new MatchInfo(2, h[0], a[1]));
            matches.Add(new MatchInfo(3, h[2], a[2]));
            matches.Add(new MatchInfo(4, h[1], a[1]));
            matches.Add(new MatchInfo(5, h[0], a[0]));
        }

        public override int Id => 6;
        public override int SetCount => 3;
        public override int PointCount => 11;
        public override int PlayerCount => 2;
        public override int MatchCount => 5;
        public override string Name => Strings.MatchSystem_Youth;
        public override bool IsCompetitive => true;
    }
}
