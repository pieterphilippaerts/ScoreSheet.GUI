using System;
using System.Collections.Generic;
using System.Text;
using PieterP.Shared.Cells;

namespace PieterP.ScoreSheet.GUI.ViewModels {
    public class ZoomableViewModel {
        public ZoomableViewModel(object originalDataContext, Cell<float>? zoomLevel) {
            this.OriginalDataContext = originalDataContext;
            this.ZoomLevel = zoomLevel ?? Cell.Create(1f);
        }
        public object OriginalDataContext { get; private set; }
        public Cell<float> ZoomLevel { get; private set; }
    }
}
