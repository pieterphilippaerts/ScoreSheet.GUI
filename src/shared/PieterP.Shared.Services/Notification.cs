using System;
using System.Collections.Generic;
using System.Text;

namespace PieterP.Shared.Services {
    public abstract class Notification {
        public event Action<Notification> BeforeRaise;
        public event Action<Notification> AfterRaise;
        internal void OnBeforeRaise() {
            this.BeforeRaise?.Invoke(this);
        }
        internal void OnAfterRaise() {
            this.AfterRaise?.Invoke(this);
        }
    }
}
