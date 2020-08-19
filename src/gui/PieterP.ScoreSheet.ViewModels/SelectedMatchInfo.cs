using System;
using System.Collections.Generic;
using System.Text;
using PieterP.ScoreSheet.ViewModels.Score;

namespace PieterP.ScoreSheet.ViewModels {
    public class SelectedMatchInfo {
        public Winner Result { get; set; }
        public int HomeMatchesWon { get; set; }
        public int AwayMatchesWon { get; set; }
        public TeamInfo HomeTeam { get; set; }
        public TeamInfo AwayTeam { get; set; }
        public string MatchId { get; set; }
        public bool HasValidationErrors { get; set; }
        public IList<string>? ValidationErrors { get; set; }
        public CompetitiveMatchViewModel MatchVm { get; set; }
        public bool IsSelected { get; set; }
        public bool ShowUploadMessage { get; set; }
        public bool ShowNonCompetitiveMessage { get; set; }
    }
}
