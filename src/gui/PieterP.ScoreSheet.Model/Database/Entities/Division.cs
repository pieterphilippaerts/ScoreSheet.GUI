using System;
using System.Collections.Generic;
using System.Text;
using PieterP.ScoreSheet.Model.Database.Enums;

namespace PieterP.ScoreSheet.Model.Database.Entities {
    public class Division {
        public int Id { get; set; }
        public string Name { get; set; }
        public int PlayerCategory { get; set; }
        public Region Region { get; set; }
        public int MatchSystemId { get; set; }
    }
}
