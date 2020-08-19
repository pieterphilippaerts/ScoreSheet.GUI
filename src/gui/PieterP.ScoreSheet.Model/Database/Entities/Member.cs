using System;
using System.Collections.Generic;
using System.Text;
using PieterP.ScoreSheet.Localization;

namespace PieterP.ScoreSheet.Model.Database.Entities {
    public class Member {
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public int? RankIndex { get; set; }
        public int? ComputerNumber { get; set; }
        public int? Position { get; set; }
        public string? Ranking {
            get {
                return Localizer.ToLocalizedRanking(_ranking);
            }
            set {
                _ranking = value;
            }
        }
        private string? _ranking;
        public PlayerStatus? Status { get; set; } 
    }
    public enum PlayerStatus : int {
        Active = 1, // A
        RecreantReserve = 2, // V
        Super = 3, // S
        DoubleAffiliationSuper = 4, // T
        MidSeason = 5, // O
        FreeTime = 6 // P
    }
}
