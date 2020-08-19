using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.Model.Database.Entities;
using PieterP.Shared.Cells;
using static PieterP.ScoreSheet.Localization.Errors;
using static PieterP.ScoreSheet.Localization.Strings;

namespace PieterP.ScoreSheet.ViewModels.Wizards {
    public class ChooseHandicapViewModel : WizardPanelViewModel {
        public ChooseHandicapViewModel(WizardViewModel parent, Action<HandicapTable?> onContinue) : base(parent) {
            _onContinue = onContinue;
            this.UseHandicap = Cell.Create(false);
            this.Tables = DatabaseManager.Current.Handicap.Tables;
            this.SelectedTable = Cell.Create<HandicapTable?>(null);
            this.Continue = new RelayCommand(OnContinue);
        }
        private void OnContinue() {
            if (UseHandicap.Value)
                _onContinue(SelectedTable.Value);
            else
                _onContinue(null);
        }

        public Cell<bool> UseHandicap { get; private set; }
        public IEnumerable<HandicapTable> Tables { get; private set; }
        public Cell<HandicapTable?> SelectedTable { get; private set; }
        public ICommand Continue { get; private set; }

        public override string Title => Wizard_MatchType;
        public override string Description => Wizard_MatchTypeDesc;

        private Action<HandicapTable?> _onContinue;
    }
}
