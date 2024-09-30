using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using PieterP.ScoreSheet.ViewModels.Services;
using PieterP.Shared.Cells;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.ViewModels {
    public class UpdateAppViewModel {
        public UpdateAppViewModel() {
            var service = ServiceLocator.Resolve<AppUpdateService>();
            this.Status = service.Status;
            this.TotalBytes = service.TotalBytes;
            this.BytesDownloaded = service.BytesDownloaded;
            this.Close = new CloseDialogCommand(true);
            service.CheckForUpdate();
        }

        public ICommand Close { get; }
        public Cell<UpdateStatus> Status { get; }
        public Cell<int> TotalBytes { get; }
        public Cell<int> BytesDownloaded { get; }
    }
}
