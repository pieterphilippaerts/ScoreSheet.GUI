using PieterP.Shared.Cells;
using System;
using System.Collections.Generic;
using System.Text;

namespace PieterP.ScoreSheet.ViewModels.Interfaces {
    public interface IZoomable {
        // nothing here
        public Cell<string> Watermark { get; }
        public Cell<bool> ShowWatermark { get; }
        public Cell<int> WatermarkSize { get; }
        public Cell<double> WatermarkOpacity {get;}
    }
}
