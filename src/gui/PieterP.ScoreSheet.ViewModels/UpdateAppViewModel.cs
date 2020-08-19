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
            this.Close = new CloseDialogCommand(true);
            service.CheckForUpdate();
        }

        public ICommand Close { get; private set; }
        public Cell<UpdateStatus> Status { get; private set; }
    }
}
