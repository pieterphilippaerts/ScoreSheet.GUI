using System;
using System.Collections.Generic;
using System.Text;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model.Database.MatchSystems;
using PieterP.ScoreSheet.ViewModels.Templates;

namespace PieterP.ScoreSheet.ViewModels.Score.MatchSystems {
    public class FreeTimeMatchSystem : VMMatchSystem {
        public override void Initialize(TeamInfo home, TeamInfo away, IList<MatchInfo> matches) {
            var h = home.Players;
            var a = away.Players;
            // init teams
            for (int i = 1; i <= 3; i++) {
                h.Add(new SinglePlayerInfo(home, i.ToString()));
                a.Add(new SinglePlayerInfo(away, i.ToString()));
            }
            // init matches
            matches.Add(new MatchInfo(1, new PlayerInfo[] { h[0], h[1] }, new PlayerInfo[] { a[0], a[1] }));
            matches.Add(new MatchInfo(2, h[2], a[2]));
            matches.Add(new MatchInfo(3, h[0], a[1]));
            matches.Add(new MatchInfo(4, h[1], a[0]));
            matches.Add(new MatchInfo(5, new PlayerInfo[] { h[0], h[2] }, new PlayerInfo[] { a[0], a[2] }));
            matches.Add(new MatchInfo(6, h[1], a[1]));
            matches.Add(new MatchInfo(7, h[2], a[0]));
            matches.Add(new MatchInfo(8, h[0], a[2]));
            matches.Add(new MatchInfo(9, new PlayerInfo[] { h[1], h[2] }, new PlayerInfo[] { a[1], a[2] }));
            matches.Add(new MatchInfo(10, h[0], a[0]));
            matches.Add(new MatchInfo(11, h[2], a[1]));
            matches.Add(new MatchInfo(12, h[1], a[2]));
        }

        public override int Id => 7;
        public override int SetCount => 3;
        public override int PointCount => 11;
        public override int PlayerCount => 3;
        public override int MatchCount => 12;
        public override string Name => Strings.MatchSystem_FreeTime;
        public override bool IsCompetitive => false;

        public override IOrientationInfo GenerateTemplate(CompetitiveMatchViewModel matchVm) {
            return new UnofficialScoreTemplate(matchVm);
        }
    }
}
