using System;
using System.Collections.Generic;
using System.Text;

namespace PieterP.ScoreSheet.Model.Database.Entities {
    public class MemberList {
        public string? ClubId { get; set; }
        public int? Category { get; set; }
        public DateTime? LastUpdated { get; set; }
        public List<Member>? Entries { get; set; }

    }
}
