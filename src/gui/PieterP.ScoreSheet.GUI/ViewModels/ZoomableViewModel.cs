using System;
using System.Collections.Generic;
using System.Text;
using PieterP.ScoreSheet.ViewModels.Interfaces;
using PieterP.Shared.Cells;

namespace PieterP.ScoreSheet.GUI.ViewModels {
    public class ZoomableViewModel {
        public ZoomableViewModel(IZoomable originalDataContext, Cell<float>? zoomLevel) {
            this.OriginalDataContext = originalDataContext;
            this.ZoomLevel = zoomLevel ?? Cell.Create(1f);
        }
        public IZoomable OriginalDataContext { get; private set; }
        public Cell<float> ZoomLevel { get; private set; }
        public Cell<string> Watermark => OriginalDataContext.Watermark;
        public Cell<bool> ShowWatermark => OriginalDataContext.ShowWatermark;
        public Cell<int> WatermarkSize => OriginalDataContext.WatermarkSize;
        public Cell<double> WatermarkOpacity => OriginalDataContext.WatermarkOpacity;

    }
}
