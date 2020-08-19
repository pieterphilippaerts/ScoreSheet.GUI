using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using PieterP.ScoreSheet.Model.Database.MatchSystems;
using PieterP.ScoreSheet.ViewModels.Notifications;
using PieterP.Shared;
using PieterP.Shared.Cells;
using PieterP.Shared.Services;
using static PieterP.ScoreSheet.Localization.Errors;
using static PieterP.ScoreSheet.Localization.Strings;

namespace PieterP.ScoreSheet.ViewModels.Wizards {
    public class NewCustomMatchViewModel : WizardPanelViewModel {
        public NewCustomMatchViewModel(WizardViewModel parent, Action<MatchSystem> onContinue) : base(parent) {
            _onContinue = onContinue;
            this.MatchSystems = ServiceLocator.Resolve<MatchSystemFactory>().Systems;
            this.SelectedSystem = Cell.Create<MatchSystem?>(null);
            this.Load = new RelayCommand(OnLoad, () => this.SelectedSystem.Value != null);
            this.SelectedSystem.ValueChanged += this.Load.RaiseCanExecuteChanged;
        }

        private void OnLoad() {
            var selected = SelectedSystem.Value;
            if (selected == null)
                return;
            _onContinue(selected);
        }

        public IEnumerable<MatchSystem> MatchSystems { get; private set; }
        public Cell<MatchSystem?> SelectedSystem { get; private set; }
        public RelayCommand<object> Load { get; private set; }

        public override string Title => Wizard_MatchType;
        public override string Description => Wizard_MatchTypeDesc;

        private Action<MatchSystem> _onContinue;
    }
}
