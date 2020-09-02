using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.Model.Database.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PieterP.ScoreSheet.Localization.Errors;

namespace PieterP.ScoreSheet.ViewModels.Score.Validations {
    //public class ChiefRefereeValidValidation : Validation {
    //    public override string? Run(CompetitiveMatchViewModel matchVm) {
    //        if (matchVm.ChiefReferee.ComputerNumber.Value == "" || matchVm.ChiefReferee.Name.Value == "") 
    //            return null; // if the name or number is empty, it will be caught by another validation

    //        // make sure the chief referee is either not linked to the home club, or the same as the home captain 
    //        if (!int.TryParse(matchVm.ChiefReferee.ComputerNumber.Value, out var cn)) {
    //            return null;
    //        }

    //        var clubMembers = DatabaseManager.Current.Members[matchVm.HomeTeam.ClubId.Value, PlayerCategories.Default];
    //        if (clubMembers != null && clubMembers.Entries.Any(m => m.ComputerNumber == cn)) {
    //            // the chief referee is a member of the home team; in this case (s)he must
    //            // be the home captain
    //            if (int.TryParse(matchVm.HomeCaptain.ComputerNumber.Value, out var homeCapt) && homeCapt != cn)
    //                return Validation_ChiefRefereeNotHomeCaptain;
    //        } // else: chief referee is probably an official referee
    //        return null;
    //    }
    //}
}