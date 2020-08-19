using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PieterP.Shared.Services {
    public class NotificationManager {
        public static NotificationManager Current {
            get {
                return ServiceLocator.Resolve<NotificationManager>();
            }
        }
        public void RegisterFor<T>(Action<T> callback) where T : Notification {
            if (callback == null)
                return;

            var t = typeof(T);
            List<object> l;
            lock (_callbacks) {
                if (!_callbacks.TryGetValue(t, out l)) {
                    l = new List<object>();
                    _callbacks[t] = l;
                }
                l.Add(callback);
            }
        }
        public void Raise<T>(T notification) where T : Notification {
            var t = notification.GetType();

            List<object> l;
            lock (_callbacks) {
                if (!_callbacks.TryGetValue(t, out l)) {
                    return; // nothing to do
                }
                l = l.ToList(); // make a copy (thread safe enumeration)
            }
            notification.OnBeforeRaise();
            foreach (Action<T>? action in l) {
                action?.Invoke(notification);
            }
            notification.OnAfterRaise();
        }
        private Dictionary<Type, List<object>> _callbacks = new Dictionary<Type, List<object>>();

    }
}
