using System;
using System.Collections.Generic;
using System.Text;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.ViewModels.Interfaces;
using PieterP.ScoreSheet.ViewModels.Score.Optimizations;
using PieterP.Shared.Cells;

namespace PieterP.ScoreSheet.ViewModels.Score {
    public class MatchContainerViewModel : IZoomable {
        public MatchContainerViewModel() {
            _watermark = new RelayCell<string>();
            this.ActiveMatch = Cell.Create<CompetitiveMatchViewModel?>(null, () => {
                _watermark.ChangeTarget(this.ActiveMatch?.Value?.Watermark);
            });
        }

        public Cell<CompetitiveMatchViewModel?> ActiveMatch { get; private set; }

        private RelayCell<string> _watermark;
        public Cell<string> Watermark => _watermark;
        public Cell<bool> ShowWatermark => DatabaseManager.Current.Settings.ShowWatermark;
        public Cell<int> WatermarkSize => DatabaseManager.Current.Settings.WatermarkSize;
        public Cell<double> WatermarkOpacity => DatabaseManager.Current.Settings.WatermarkOpacity;
    }
}
