using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PieterP.ScoreSheet.Model.Database.Entities {
    public class PlayerCategory {
        public int? UniqueIndex { get; set; }
        public string? Name { get; set; }
        public string? ShortName { get; set; }
        public int? RankingCategory { get; set; }
        public string? Sex { get; set; }
        public int? MinimumAge { get; set; }
        public int? MaximumAge { get; set; }
    }
}
