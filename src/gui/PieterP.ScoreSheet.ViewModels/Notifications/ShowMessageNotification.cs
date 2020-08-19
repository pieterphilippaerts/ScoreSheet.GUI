using System;
using System.Collections.Generic;
using System.Text;
using PieterP.Shared;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.ViewModels.Notifications {
    public class ShowMessageNotification : Notification {
        public ShowMessageNotification(string message, NotificationTypes type = NotificationTypes.Informational, NotificationButtons buttons = NotificationButtons.OK) {
            this.Message = message;
            this.Type = type;
            this.Buttons = buttons;
        }
        public string Message { get; private set; }
        public NotificationTypes Type { get; private set; }
        public NotificationButtons Buttons{ get; private set; }
        public bool? Result { get; set; }
    }
    public enum NotificationTypes : int { 
        Informational = 0x40,
        Question = 0x20,
        Exclamation = 0x30,
        Error = 0x10
    }
    public enum NotificationButtons : int {
        OK = 0,
        OKCancel = 1,
        YesNoCancel = 3,
        YesNo = 4
    }
}
