using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.Model.Database.Enums;
using PieterP.Shared.Cells;

namespace PieterP.ScoreSheet.ViewModels.Settings {
    public class ScoreVisualizationViewModel {
        public ScoreVisualizationViewModel() {
            this.AvailableVisualizations = new ScoreVisualizationInfo[] {
                new ScoreVisualizationInfo() {
                    VisualizationType = ScoreVisualizations.Default ,
                    Name = Strings.ScoreVisualization_Default,
                    Description = Strings.ScoreVisualization_DefaultDesc
                },
                new ScoreVisualizationInfo() {
                    VisualizationType = ScoreVisualizations.Compact,
                    Name = Strings.ScoreVisualization_Compact,
                    Description = Strings.ScoreVisualization_CompactDesc
                },
                new ScoreVisualizationInfo() {
                    VisualizationType = ScoreVisualizations.CompactDetailed,
                    Name = Strings.ScoreVisualization_CompactDetailed,
                     Description = Strings.ScoreVisualization_CompactDetailedDesc
                }
            };
            this.OverviewVisualization = Cell.Create(
                this.AvailableVisualizations.FirstOrDefault(c => c.VisualizationType == DatabaseManager.Current.Settings.OverviewVisualization.Value), 
                () => { DatabaseManager.Current.Settings.OverviewVisualization.Value = this.OverviewVisualization.Value.VisualizationType; }
                );
            this.SecondScreenVisualization = Cell.Create(
                this.AvailableVisualizations.FirstOrDefault(c => c.VisualizationType == DatabaseManager.Current.Settings.SecondScreenVisualization.Value),
                () => { DatabaseManager.Current.Settings.SecondScreenVisualization.Value = this.SecondScreenVisualization.Value.VisualizationType; }
                );
        }

        public IList<ScoreVisualizationInfo> AvailableVisualizations { get; private set; }
        public Cell<ScoreVisualizationInfo> OverviewVisualization { get; private set; }
        public Cell<ScoreVisualizationInfo> SecondScreenVisualization { get; private set; }
    }
    public class ScoreVisualizationInfo {
        public ScoreVisualizations VisualizationType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}