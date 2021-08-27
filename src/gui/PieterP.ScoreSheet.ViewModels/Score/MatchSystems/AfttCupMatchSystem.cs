using PieterP.ScoreSheet.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PieterP.ScoreSheet.ViewModels.Score.MatchSystems {
    public class AfttCupMatchSystem : VMMatchSystem {
        public override void Initialize(TeamInfo home, TeamInfo away, IList<MatchInfo> matches) {
            var h = home.Players;
            var a = away.Players;
            // init teams
            for (int i = 1; i <= 4; i++) {
                h.Add(new SinglePlayerInfo(home, i.ToString()));
                a.Add(new SinglePlayerInfo(away, i.ToString()));
            }
            h.Add(new DoublePlayerInfo(home, DoublePlayerOption.Permutation1234));
            a.Add(new DoublePlayerInfo(away, DoublePlayerOption.Permutation1234));
            // init matches
            matches.Add(new MatchInfo(1, h[0], a[0], 7));
            matches.Add(new MatchInfo(2, h[1], a[1], 7));
            matches.Add(new MatchInfo(3, h[4], a[4], 7));
            matches.Add(new MatchInfo(4, h[0], a[1], 7));
            matches.Add(new MatchInfo(5, h[1], a[0], 7));
        }

        public override int Id => -2;
        public override int SetCount => 4;
        public override int PointCount => 11;
        public override int PlayerCount => 2;
        public override int MatchCount => 5;
        public override int SingleMatchCount => 4;
        public override int DoubleMatchCount => 1;
        public override string Name => Strings.MatchSystem_AfttCup;
        public override bool IsCompetitive => false;
    }
}
