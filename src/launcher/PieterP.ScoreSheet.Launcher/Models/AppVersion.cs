using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace PieterP.ScoreSheet.Launcher.Models {
    public class AppVersion {
        private AppVersion(string path) {
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException();
            var di = new DirectoryInfo(path);
            this.Version = new Version(di.Name);
            this.InstalledOn = di.CreationTime;
            var fis = di.GetFiles("scoresheet.exe", SearchOption.AllDirectories);
            if (fis == null || fis.Length == 0 || fis[0] == null)
                this.ExePath = Path.Combine(path, "scoresheet.exe");
            else
                this.ExePath = fis[0].FullName;
            this.IsExeAvailable = File.Exists(this.ExePath);
        }
        public static AppVersion Create(string path) {
            try {
                return new AppVersion(path);
            } catch { }
            return null;
        }

        public bool Run(AppProfile profile = null, bool debug = false) {
            try {
                var args = "launched";
                if (profile != null) {
                    args = args + " profile=\"" + profile.Name + "\"";
                }
                if (debug) {
                    args = args + " debug";
                }

                var startInfo = new ProcessStartInfo();
                startInfo.UseShellExecute = false;
                startInfo.FileName = this.ExePath;
                if (args.Length > 0)
                    startInfo.Arguments = args;
                Process.Start(startInfo);
                return true;
            } catch {
                return false;
            }
        }

        public Version Version { get; private set; }
        public string ExePath { get; private set; }
        public bool IsExeAvailable { get; private set; }
        public DateTime InstalledOn { get; private set; }
    }
}
