using System;
using System.Collections.Generic;
using System.Text;
using PieterP.Shared;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.ViewModels.Notifications {
    public class ShowWindowNotification : Notification {
        public ShowWindowNotification(object o) {
            this.ViewModel = o;
        }
        public object ViewModel { get; private set; }
        public object Window { get; set; }
    }
}
