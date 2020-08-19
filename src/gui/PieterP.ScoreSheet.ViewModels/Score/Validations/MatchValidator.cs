using System;
using System.Collections.Generic;
using System.Text;
using PieterP.ScoreSheet.ViewModels.Score.MatchSystems;
using PieterP.Shared.Services;
using static PieterP.ScoreSheet.Localization.Errors;

namespace PieterP.ScoreSheet.ViewModels.Score.Validations {
    public class MatchValidator {
        public IList<string>? Validate(CompetitiveMatchViewModel matchVm) {
            var errors = new List<string>();
            var validations = new List<Validation>();            
            var vmms = matchVm.MatchSystem as VMMatchSystem;
            if (vmms != null) {
                validations.AddRange(vmms.GetValidations(matchVm));
            }
            foreach (var validation in validations) {
                try {
                    string? error = validation.Run(matchVm);
                    if (error != null) {
                        errors.Add(error);
                    }
                } catch (Exception e) {
                    Logger.Log(e);
                }
            }
            if (errors.Count == 0)
                return null;
            return errors;
        }
    }
}
