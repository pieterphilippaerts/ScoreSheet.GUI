using System;
using System.Collections.Generic;
using System.Text;
using PieterP.ScoreSheet.Model.Database;
using PieterP.Shared.Cells;

namespace PieterP.ScoreSheet.ViewModels.Information {
    public class SecondScreenWindowViewModel : IDisposable {
        public SecondScreenWindowViewModel(MainWindowViewModel mainVm, int left, int top, int width, int height) {
            this.Orchestrator = new InformationOrchestratorViewModel(DatabaseManager.Current.Settings.SecondScreenVisualization, mainVm.ActiveMatches, mainVm.AwayMatches);
            this.Left = Cell.Create(left);
            this.Top = Cell.Create(top);
            this.Width = Cell.Create(width);
            this.Height = Cell.Create(height);
        }
        public Cell<int> Left { get; set; }
        public Cell<int> Top { get; set; }
        public Cell<int> Width { get; set; }
        public Cell<int> Height { get; set; }
        public InformationOrchestratorViewModel Orchestrator { get; set; }

        public void Dispose() {
            if (this.Orchestrator != null) {
                this.Orchestrator.Dispose();
            }
        }
    }
}
