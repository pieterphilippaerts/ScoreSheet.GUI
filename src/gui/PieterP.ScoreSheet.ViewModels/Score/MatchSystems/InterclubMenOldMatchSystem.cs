using System;
using System.Collections.Generic;
using System.Text;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model.Database.MatchSystems;

namespace PieterP.ScoreSheet.ViewModels.Score.MatchSystems {
    public class InterclubMenOldMatchSystem : VMMatchSystem {
        public override void Initialize(TeamInfo home, TeamInfo away, IList<MatchInfo> matches) {
            var h = home.Players;
            var a = away.Players;
            // init teams
            for (int i = 1; i <= 4; i++) {
                h.Add(new SinglePlayerInfo(home, i.ToString()));
                a.Add(new SinglePlayerInfo(away, i.ToString()));
            }
            // init matches
            matches.Add(new MatchInfo(1, h[3], a[1], 3));
            matches.Add(new MatchInfo(2, h[2], a[0], 3));
            matches.Add(new MatchInfo(3, h[1], a[3], 3));
            matches.Add(new MatchInfo(4, h[0], a[2], 3));
            matches.Add(new MatchInfo(5, h[3], a[0], 3));
            matches.Add(new MatchInfo(6, h[2], a[1], 3));
            matches.Add(new MatchInfo(7, h[1], a[2], 3));
            matches.Add(new MatchInfo(8, h[0], a[3], 3));
            matches.Add(new MatchInfo(9, h[3], a[2], 3));
            matches.Add(new MatchInfo(10, h[2], a[3], 3));
            matches.Add(new MatchInfo(11, h[1], a[0], 3));
            matches.Add(new MatchInfo(12, h[0], a[1], 3));
            matches.Add(new MatchInfo(13, h[3], a[3], 3));
            matches.Add(new MatchInfo(14, h[2], a[2], 3));
            matches.Add(new MatchInfo(15, h[1], a[1], 3));
            matches.Add(new MatchInfo(16, h[0], a[0], 3));
        }

        public override int Id => 1;
        public override int SetCount => 2;
        public override int PointCount => 21;
        public override int PlayerCount => 4;
        public override int MatchCount => 16;
        public override string Name => Strings.MatchSystem_InterclubMenOld;
        public override bool IsCompetitive => true;
    }
}
