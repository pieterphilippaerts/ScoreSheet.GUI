using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using PieterP.ScoreSheet.ViewModels.Services;
using PieterP.Shared.Cells;

namespace PieterP.ScoreSheet.ViewModels.Settings {
    public class SettingsViewModel {
        public SettingsViewModel(MainWindowViewModel parent) {
            this.Close = new CloseDialogCommand(true);
            var tracker = parent.GetService<MatchTrackerService>();
            
            this.Panels = new object[] { 
                new LanguagesViewModel(),
                new DefaultValuesViewModel(),
                new PrintDefaultsViewModel(),
                new UploadViewModel(),
                new AutoUploadViewModel(),
                new SecondScreenViewModel(),
                new ScoreVisualizationViewModel(),
                new LayoutViewModel(),
                new LiveUpdatesViewModel(),
                new AwayMatchesViewModel(tracker?.TrackCount),
                new WebServiceViewModel(),
#if LIMBURG_FREETIME_SUPPORT
                new LimburgViewModel(),
#endif
                new VariousViewModel(),
                new StartupViewModel()
            };
            this.SelectedPanel = Cell.Create(this.Panels.FirstOrDefault());
        }

        public ICommand Close { get; private set; }
        public IList<object> Panels { get; private set; }
        public Cell<object> SelectedPanel { get; private set; }
    }
}
