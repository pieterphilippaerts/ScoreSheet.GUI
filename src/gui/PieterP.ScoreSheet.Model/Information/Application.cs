using System;
using System.Collections.Generic;
using System.Text;
using PieterP.Shared.Interfaces;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.Model.Information {
    public static partial class Application {
        public static Version Version { 
            get {
                var timer = ServiceLocator.Resolve<ITimerService>();
                if (timer != null) {
                    return timer.GetType().Assembly.GetName().Version; // get the version of PieterP.ScoreSheet.GUI
                }
                return typeof(Application).Assembly.GetName().Version; // fallback; get the version of PieterP.ScoreSheet.Model
            }
        }
        public static DateTime ExpiryTime {
            get {
                return new DateTime(2020, 9, 1); // expires at the start of season 2020-2021
            }
        }
        public static string BuildType {
            get {
#if DEBUG
                return "debug";
#else
                return "release";
#endif
            }
        }
        public static string BuildPlatform {
            get {
#if NETFRAMEWORK
                return "netfx";
#else
                return "core";
#endif 
            }
        }
    }
}
