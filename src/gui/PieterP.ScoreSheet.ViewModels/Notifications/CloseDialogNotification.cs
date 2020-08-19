using System;
using System.Collections.Generic;
using System.Text;
using PieterP.Shared;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.ViewModels.Notifications {
    public class CloseDialogNotification : Notification {
        public CloseDialogNotification(bool result) {
            this.Result = result;
        }
        public bool Result { get; set; }
    }
}
