using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using PieterP.Shared.Cells;

namespace PieterP.ScoreSheet.ViewModels.Wizards {
    public class WizardViewModel {
        public WizardViewModel() {
            this.CurrentPanel = Cell.Create<WizardPanelViewModel?>(null);
        }

        public Cell<WizardPanelViewModel?> CurrentPanel { get; private set; }
    }
}
