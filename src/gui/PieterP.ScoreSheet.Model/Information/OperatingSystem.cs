using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace PieterP.ScoreSheet.Model.Information {
    public class OperatingSystem {
        public static bool Is64bit {
            get {
                if (IntPtr.Size == 8)
                    return true; // we're running as a 64-bit process
                // are we a 32-bit process on 64-bit windows?
                using (var p = Process.GetCurrentProcess()) {
                    bool retVal;
                    if (!NativeMethods.IsWow64Process(p.Handle, out retVal)) {
                        return false;
                    }
                    return retVal;
                }
            }
        }
        public static Version Version {
            get {
                return Environment.OSVersion.Version;
            }
        }
        public static string ServicePack { 
            get {
                if (Environment.OSVersion.ServicePack != null && Environment.OSVersion.ServicePack != string.Empty) {
                    return Environment.OSVersion.ServicePack;
                }
                return "no service pack";
            }
        }
        public static string Name {
            get {
                switch (Environment.OSVersion.Platform) {
                    case PlatformID.Win32S:
                        return "Windows 3.1";
                    case PlatformID.Win32Windows:
                        switch (Environment.OSVersion.Version.Minor) {
                            case 0:
                                return "Windows 95";
                            case 10:
                                return "Windows 98";
                            case 90:
                                return "Windows ME";
                        }
                        break;

                    case PlatformID.Win32NT:
                        switch (Environment.OSVersion.Version.Major) {
                            case 3:
                                return "Windows NT 3.51";
                            case 4:
                                return "Windows NT 4.0";
                            case 5:
                                switch (Environment.OSVersion.Version.Minor) {
                                    case 0:
                                        return "Windows 2000";
                                    case 1:
                                        return "Windows XP";
                                    case 2:
                                        return "Windows 2003";
                                }
                                break;

                            case 6:
                                switch (Environment.OSVersion.Version.Minor) {
                                    case 0:
                                        return "Windows Vista"; // or 2008 server
                                    case 1:
                                        return "Windows 7"; // or 2008 server r2
                                    case 2:
                                        return "Windows 8"; // or 2012 server
                                    case 3:
                                        return "Windows 8.1"; // or 2012 server r2
                                }
                                break;
                            case 10:  //this will only show up if the application has a manifest file allowing W10, otherwise a 6.2 version will be used
                                if (Environment.OSVersion.Version.Build < 22000)
                                    return "Windows 10";
                                return "Windows 11";
                        }
                        break;

                    case PlatformID.WinCE:
                        return "Windows CE";
                }
                return "Unknown";
            }
        }
    }
}
