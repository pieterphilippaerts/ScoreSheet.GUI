using PieterP.ScoreSheet.Localization;
using PieterP.Shared;
using PieterP.Shared.Cells;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PieterP.ScoreSheet.ViewModels.Score {
    public class SetScoreViewModel {
        public SetScoreViewModel() {
            IsValid = Cell.Create(true);
            ScoreLeft = Cell.Create<string?>(string.Empty);
            ScoreRight = Cell.Create<string?>(string.Empty);

            ScoreLeft.ValueChanged += Score_ValueChanged;

        }

        private void Score_ValueChanged() {
            IsValid.Value = IsNaturalNumberOrEmpty(ScoreLeft.Value) == null
                && IsNaturalNumberOrEmpty(ScoreLeft.Value) == null;
        }
        public static ICollection? IsNaturalNumberOrEmpty(string? input) {
            if (input == null || input.Length == 0)
                return null;
            if (!int.TryParse(input, out var result) || result < 0)
                return new string[] { Strings.SetScoreViewModel_NaturalNumber };
            return null;
        }


        public Cell<string?> ScoreLeft { get; set; }
        public Cell<string?> ScoreRight { get; set; }
        public Cell<bool> IsValid { get; set; }

    }
}
