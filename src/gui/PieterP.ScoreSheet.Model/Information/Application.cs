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
                return new DateTime(2025, 9, 1); // expires at the start of season 2021-2022
                // if you're updating this, also update the DefaultSeasonId below
            }
        }
        public static int DefaultSeasonId {
            get {
                return 25; // the ID of season 2023-2024
            }
        }
        public static int DefaultMenCategoryId {
            get {
                return 37; // since season 2022-2023; can normally be detected automatically, unless the user doesn't enter a username/password
            }
        }
        public static int DefaultWomenCategoryId {
            get {
                return 38; // since season 2022-2023; can normally be detected automatically, unless the user doesn't enter a username/password
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
