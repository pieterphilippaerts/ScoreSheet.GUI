using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PieterP.ScoreSheet.Model.Database.Entities;

namespace PieterP.ScoreSheet.Model.Database {
    public class HandicapDatabase {
        public HandicapDatabase() {
            this.Tables = new HandicapTable[] { new OfficialMenHandicapTable(), new OfficialWomenHandicapTable() };
        }
        
        public IEnumerable<HandicapTable> Tables { get; private set; }
        public HandicapTable? this[int id] {
            get {
                return this.Tables.FirstOrDefault(c => c.Id == id);
            }
        }
    }
}
