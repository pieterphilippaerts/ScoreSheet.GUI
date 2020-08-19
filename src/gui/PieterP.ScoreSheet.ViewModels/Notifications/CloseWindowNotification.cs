using System;
using System.Collections.Generic;
using System.Text;
using PieterP.Shared;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.ViewModels.Notifications {
    public class CloseWindowNotification : Notification {
        public CloseWindowNotification(object window) {
            this.Window = window;
        }
        public object Window { get; set; }
    }
}
