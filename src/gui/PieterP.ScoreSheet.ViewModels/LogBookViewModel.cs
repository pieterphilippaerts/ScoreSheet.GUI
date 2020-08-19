using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using PieterP.Shared.Cells;
using PieterP.Shared.Services;
using static PieterP.Shared.Services.Logger;

namespace PieterP.ScoreSheet.ViewModels {
    public class LogBookViewModel {
        public LogBookViewModel(bool debug) {
            this.IsDebug = debug;
            this.ShowError = Cell.Create(true, ResetFilter);
            this.ShowWarning = Cell.Create(true, ResetFilter);
            this.ShowInformation = Cell.Create(true, ResetFilter);
            this.ShowDebug = Cell.Create(false, ResetFilter);
            this.Items = new ObservableCollection<LogEntry>();
            var logger = ServiceLocator.Resolve<Logger>();
            ResetFilter();
        }
        private void ResetFilter() {
            this.Items.Clear();
            var logger = ServiceLocator.Resolve<Logger>();
            foreach (var e in logger.Entries.Where(m =>
                (this.ShowError.Value && m.Type == LogType.Exception)
                || (this.ShowWarning.Value && m.Type == LogType.Warning)
                || (this.ShowInformation.Value && m.Type == LogType.Informational)
                || (this.ShowDebug.Value && m.Type == LogType.Debug)
            )) {
                this.Items.Add(e);
            }
        }

        public ObservableCollection<LogEntry> Items { get; private set; }
        public Cell<bool> ShowError { get; private set; }
        public Cell<bool> ShowWarning { get; private set; }
        public Cell<bool> ShowInformation { get; private set; }
        public Cell<bool> ShowDebug { get; private set; }
        public bool IsDebug { get; private set; }
    }
}
