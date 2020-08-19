using System;
using System.Collections.Generic;
using System.Text;
using SIODI = System.IO.DriveInfo;

namespace PieterP.ScoreSheet.Model.Information {
    public class System {
        public static long TotalMemory {
            get {
                var memStatus = new NativeMethods.MEMORYSTATUSEX();
                if (NativeMethods.GlobalMemoryStatusEx(memStatus)) {
                    return memStatus.ullTotalPhys;
                }
                return 0;
            }
        }
        public static IEnumerable<DriveInfo> Drives {
            get {
                var ret = new List<DriveInfo>();
                foreach (var d in SIODI.GetDrives()) {
                    try {
                        var di = new DriveInfo();
                        di.Root = d.RootDirectory.FullName;
                        di.TotalSize = d.TotalSize;
                        di.FreeSize = d.TotalFreeSpace;
                        ret.Add(di);
                    } catch { /* could be an empty CD-rom drive... */ }
                }
                return ret;
            }
        }
    }
    public class DriveInfo { 
        public string? Root { get; set; }
        public long TotalSize { get; set; }
        public long FreeSize { get; set; }
    }
}
