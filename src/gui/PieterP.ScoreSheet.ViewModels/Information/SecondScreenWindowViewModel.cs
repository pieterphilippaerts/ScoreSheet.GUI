using System;
using System.Collections.Generic;
using System.Text;
using PieterP.ScoreSheet.Model.Database;
using PieterP.Shared.Cells;

namespace PieterP.ScoreSheet.ViewModels.Information {
    public class SecondScreenWindowViewModel : IDisposable {
        public SecondScreenWindowViewModel(MainWindowViewModel mainVm) {
            this.Orchestrator = new InformationOrchestratorViewModel(DatabaseManager.Current.Settings.SecondScreenVisualization, mainVm.ActiveMatches, mainVm.AwayMatches);
        }
        public InformationOrchestratorViewModel Orchestrator { get; set; }

        public void Dispose() {
            if (this.Orchestrator != null) {
                this.Orchestrator.Dispose();
            }
        }
    }
}
