using System;
using System.Collections.Generic;
using System.Text;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model.Database.MatchSystems;

namespace PieterP.ScoreSheet.ViewModels.Score.MatchSystems {
    public class SuperMatchSystem : VMMatchSystem {
        public override void Initialize(TeamInfo home, TeamInfo away, IList<MatchInfo> matches) {
            var h = home.Players;
            var a = away.Players;
            var hn = new string[] { "A", "B", "C", "V" };
            var an = new string[] { "X", "Y", "Z", "V" };
            // init teams
            for (int i = 1; i <= 4; i++) {
                h.Add(new SinglePlayerInfo(home, hn[i - 1], i == 4));
                a.Add(new SinglePlayerInfo(away, an[i - 1], i == 4));
            }
            var hspi = new SubstitutePlayerInfo(home, (h[3] as SinglePlayerInfo)!);
            h.Add(hspi);
            var aspi = new SubstitutePlayerInfo(away, (a[3] as SinglePlayerInfo)!);
            a.Add(aspi);
            // init matches
            matches.Add(new MatchInfo(1, h[0], a[1]));
            matches.Add(new MatchInfo(2, h[1], a[0]));
            matches.Add(new MatchInfo(3, h[2], a[2]));
            matches.Add(new MatchInfo(4, h[0], a[0], homeSubstitute: hspi, awaySubstitute: aspi));
            matches.Add(new MatchInfo(5, h[2], a[1], homeSubstitute: hspi, awaySubstitute: aspi));
            matches.Add(new MatchInfo(6, h[1], a[2], homeSubstitute: hspi, awaySubstitute: aspi));
        }

        public override int Id => 8;
        public override int SetCount => 3;
        public override int PointCount => 11;
        public override int PlayerCount => 4;
        public override int MatchCount => 6;
        public override string Name => Strings.MatchSystem_Super;
        public override bool IsCompetitive => true;
    }
}
