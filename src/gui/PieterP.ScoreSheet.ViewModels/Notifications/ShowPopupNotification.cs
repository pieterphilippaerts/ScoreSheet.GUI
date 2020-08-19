using System;
using System.Collections.Generic;
using System.Text;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.ViewModels.Notifications {
    public class ShowPopupNotification : Notification {
        
        
        public object Context { get; set; }
        public object Owner { get; set; }
        public bool PlaceUnderOwner { get; set; }

    }
}