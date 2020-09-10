using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using PieterP.ScoreSheet.Localization;
using HandicapTable = PieterP.ScoreSheet.Model.Database.Entities.HandicapTable;
using PieterP.ScoreSheet.Model.Database.Enums;
using PieterP.ScoreSheet.ViewModels.Score;

namespace PieterP.ScoreSheet.ViewModels.Templates {
    public abstract class RefereeLayout {
        public abstract string Name { get; }
        public abstract RefereeLayoutOptions Id { get; }
        public abstract RefereeTemplate CreateTemplate(CompetitiveMatchViewModel competitiveMatchInfo, IList<MatchInfo> matches, HandicapTable? handicap, bool tuutTuut);
        public virtual IEnumerable<object> CreateDocuments(CompetitiveMatchViewModel competitiveMatchInfo, HandicapTable? handicap) {
            var matches = competitiveMatchInfo.Matches;
            var list = new List<object>();
            for (int i = 0; i < matches.Count; i += 8) {
                list.Add(CreateTemplate(competitiveMatchInfo, matches.Skip(i).Take(8).ToList(), handicap, matches.Count == 16));
            }
            return list;
        }

        public static IEnumerable<RefereeLayout> All {
            get {
                return new RefereeLayout[] { 
                    new DefaultRefereeLayout(),
                    new VerticalRefereeLayout(), 
                    new PerTableRefereeLayout() 
                };
            }
        }
        private class DefaultRefereeLayout : RefereeLayout {
            public override string Name => Strings.RefereeLayout_2by4;
            public override RefereeLayoutOptions Id => RefereeLayoutOptions.Default;
            public override RefereeTemplate CreateTemplate(CompetitiveMatchViewModel competitiveMatchInfo, IList<MatchInfo> matches, HandicapTable? handicap, bool tuutTuut) => new RefereeDefaultTemplate(competitiveMatchInfo, matches, handicap, tuutTuut);
        }
        private class VerticalRefereeLayout : RefereeLayout {
            public override string Name => Strings.RefereeLayout_1by8;
            public override RefereeLayoutOptions Id => RefereeLayoutOptions.Vertical;
            public override RefereeTemplate CreateTemplate(CompetitiveMatchViewModel competitiveMatchInfo, IList<MatchInfo> matches, HandicapTable? handicap, bool tuutTuut) => new RefereeHorizontalTemplate(competitiveMatchInfo, matches, handicap, tuutTuut);
        }
        private class PerTableRefereeLayout : RefereeLayout {
            public override string Name => Strings.RefereeLayout_PerTable;
            public override RefereeLayoutOptions Id => RefereeLayoutOptions.PerTable;
            public override RefereeTemplate CreateTemplate(CompetitiveMatchViewModel competitiveMatchInfo, IList<MatchInfo> matches, HandicapTable? handicap, bool tuutTuut) => new RefereeTableTemplate(competitiveMatchInfo, matches, handicap, tuutTuut);
            public override IEnumerable<object> CreateDocuments(CompetitiveMatchViewModel competitiveMatchInfo, HandicapTable? handicap) {
                var matches = competitiveMatchInfo.Matches;
                if (matches.Count <= 8)
                    return base.CreateDocuments(competitiveMatchInfo, handicap);

                var firstTable = Enumerable.Range(0, matches.Count).Where(r => (r % 2) == 0).Select(r => matches[r]).ToList();
                var secondTable = Enumerable.Range(0, matches.Count).Where(r => (r % 2) == 1).Select(r => matches[r]).ToList();

                var list = new List<object>();
                for (int i = 0; i < firstTable.Count; i += 8) {
                    list.Add(CreateTemplate(competitiveMatchInfo, firstTable.Skip(i).Take(8).ToList(), handicap, matches.Count == 16));
                }
                for (int i = 0; i < secondTable.Count; i += 8) {
                    list.Add(CreateTemplate(competitiveMatchInfo, secondTable.Skip(i).Take(8).ToList(), handicap, matches.Count == 16));
                }
                return list;
            }
        }
    }
}
