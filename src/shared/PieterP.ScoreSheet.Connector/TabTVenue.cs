using System;
using System.Collections.Generic;
using System.Text;

namespace PieterP.ScoreSheet.Connector {
    public class TabTVenue {
        public TabTVenue(int id, int clubId, string name, string street, string town, string phone, string comment) {
            this.Id = id;
            this.ClubId = clubId;
            this.Name = name;
            this.Street = street;
            this.Town = town;
            this.Phone = phone;
            this.Comment = comment;
        }

        public int Id { get; private set; }
        public int ClubId { get; private set; } /* the venue ID within the club */
        public string Name { get; private set; }
        public string Street { get; private set; }
        public string Town { get; private set; }
        public string Phone { get; private set; }
        public string Comment { get; private set; }

        public override string ToString() {
            return string.Format("{1}{0}{2}{0}{3}", Environment.NewLine, this.Name, this.Street, this.Town);
        }
    }
}