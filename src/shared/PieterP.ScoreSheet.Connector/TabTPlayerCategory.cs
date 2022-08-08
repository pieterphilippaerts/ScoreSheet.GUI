using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PieterP.ScoreSheet.Connector {
    public class TabTPlayerCategory {
        public TabTPlayerCategory(int uniqueIndex, string name, string shortName, int rankingCategory, string sex, int? minimumAge, int? maximumAge) {
            this.UniqueIndex = uniqueIndex;
            this.Name = name;
            this.ShortName = shortName;
            this.RankingCategory = rankingCategory;
            this.Sex = sex;
            this.MinimumAge = minimumAge;
            this.MaximumAge = maximumAge;
        }
        public int UniqueIndex { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public int RankingCategory { get; set; }
        public string Sex { get; set; }
        public int? MinimumAge { get; set; }
        public int? MaximumAge { get; set; }
        public override string ToString() => Name;
    }
}