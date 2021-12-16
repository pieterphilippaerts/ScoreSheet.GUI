using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PieterP.ScoreSheet.Model;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.Model.Database.Enums;
using PieterP.Shared;
using static PieterP.ScoreSheet.Localization.Errors;

namespace PieterP.ScoreSheet.ViewModels.Score.Validations {
    public class RoomCommissionerNumberValidation : Validation {
        public override string? Run(CompetitiveMatchViewModel matchVm) {
            if (matchVm.RoomCommissioner.ComputerNumber.Value != "") {
                if (!matchVm.RoomCommissioner.ComputerNumber.Value.IsNumber())
                    return Validation_InvalidRoomCommissionerId;
                if (int.TryParse(matchVm.RoomCommissioner.ComputerNumber.Value, out var cn)) {
                    var clubMembers = DatabaseManager.Current.Members[matchVm.HomeTeam.ClubId.Value, PlayerCategories.Default];
                    if (clubMembers != null) {
                        if (!clubMembers.Entries.Any(m => m.ComputerNumber == cn))
                            return Validation_WrongRoomCommissionerId;
                    }
                }
            } else {
                if (matchVm.Level.Value.Id != Model.Database.Enums.Level.Super || matchVm.PlayerCategory != PlayerCategories.Men) // only required in super (starting from competition year 2020-2021)
                    return null;
                return Validation_NoRoomCommissionerId;
            }
            return null;
        }
    }
}
