using System;
using System.Collections.Generic;
using System.Text;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model.Database.Enums;
using PieterP.Shared.Cells;

namespace PieterP.ScoreSheet.ViewModels.Score {
    public class LevelInfo {
        public LevelInfo(string name, Level id) {
            this.Name = Cell.Create(name);
            this.Id = id;
        }
        public Cell<string> Name { get; set; }
        public Level Id { get; set; }

        public static IList<LevelInfo> DefaultList {
            get {
                return new LevelInfo[] {
                    new LevelInfo(Strings.LevelInfo_Super, Level.Super),
                    new LevelInfo(Strings.LevelInfo_National, Level.National),
                    new LevelInfo(Strings.LevelInfo_Regional, Level.Regional),
                    new LevelInfo(Strings.LevelInfo_Provincial, Level.Provincial)
                };
            }
        }
    }
}
