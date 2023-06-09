﻿using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.Model.Database.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using static PieterP.ScoreSheet.Localization.Errors;

namespace PieterP.ScoreSheet.ViewModels.Score.Validations {
    public class RoomCommissionerNameValidation : Validation {
        public override string? Run(CompetitiveMatchViewModel matchVm) {
            if (matchVm.Level.Value.Id != Model.Database.Enums.Level.Super || matchVm.PlayerCategory != DatabaseManager.Current.PlayerCategories.Men)  // only required in super men (starting from competition year 2020-2021)
                return null;
            if (matchVm.RoomCommissioner.Name.Value == "")
                return Validation_NoRoomCommissionerName;
            return null;
        }
    }
}
