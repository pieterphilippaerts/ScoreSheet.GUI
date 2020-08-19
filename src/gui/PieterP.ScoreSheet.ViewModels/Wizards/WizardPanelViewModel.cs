using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace PieterP.ScoreSheet.ViewModels.Wizards {
    public abstract class WizardPanelViewModel {
        public WizardPanelViewModel(WizardViewModel parent, ICommand? cancel = null) {
            this.Parent = parent;
            if (cancel == null)
                cancel = new CloseDialogCommand();
            Cancel = cancel;
        }
        public abstract string Title { get; }
        public abstract string Description { get; }
        public WizardViewModel Parent { get; private set; }
        public ICommand Cancel { get; private set; }
    }
}
