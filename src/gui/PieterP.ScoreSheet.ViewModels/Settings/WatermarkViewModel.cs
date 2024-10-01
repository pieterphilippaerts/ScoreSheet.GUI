using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.Model.Information;
using PieterP.Shared.Cells;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PieterP.ScoreSheet.ViewModels.Settings {
    public class WatermarkViewModel {
        public WatermarkViewModel() {
            this.ShowWatermark = DatabaseManager.Current.Settings.ShowWatermark;
            this.WatermarkSize = DatabaseManager.Current.Settings.WatermarkSize;
            this.WatermarkOpacity = DatabaseManager.Current.Settings.WatermarkOpacity;
        }
        public Cell<bool> ShowWatermark { get; private set; }
        public Cell<int> WatermarkSize { get; private set; }
        public Cell<double> WatermarkOpacity { get; private set; }
    }
}
