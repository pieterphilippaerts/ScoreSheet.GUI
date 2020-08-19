using System;
using System.Collections.Generic;
using System.Text;
using PieterP.Shared.Cells;

namespace PieterP.ScoreSheet.ViewModels.Score {
    public class PersonInfo {
        public PersonInfo(Action? onDataChanged) {
            this.Name = Cell.Create("", onDataChanged);
            this.ComputerNumber = Cell.Create("", onDataChanged);
            this.ClubName = Cell.Create("", onDataChanged);
            this.ClubId = Cell.Create("", onDataChanged);
        }
        public Cell<string> Name { get; set; }
        public Cell<string> ComputerNumber { get; set; }
        public Cell<string> ClubName { get; set; }
        public Cell<string> ClubId { get; set; }
    }
}
