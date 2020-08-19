using System;
using System.Collections.Generic;
using System.Text;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.ViewModels.Notifications {
    public class ColorDialogNotification : Notification {
        public ColorDialogNotification() {
            this.CustomColors = new List<string>();
        }
        public string? InitialColor { get; set; }
        public IList<string> CustomColors { get; private set; }
        public string? SelectedColor { get; set; }
    }
}