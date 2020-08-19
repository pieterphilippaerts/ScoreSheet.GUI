using System;
using System.Collections.Generic;
using System.Text;
using PieterP.Shared;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.ViewModels.Notifications {
    public class ShowDialogNotification : Notification {
        public ShowDialogNotification(object o) {
            this.Contents = o;
        }
        public object Contents { get; private set; }
        public bool Result { get; set; }
    }
}