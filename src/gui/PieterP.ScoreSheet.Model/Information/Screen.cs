using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PieterP.ScoreSheet.Model.Information {
    public class Screen {
        private Screen(IntPtr handle) {
            this.Handle = handle;

            var mi = new NativeMethods.MONITORINFOEX();
            mi.Size = Marshal.SizeOf(mi);
            _succes = NativeMethods.GetMonitorInfo(handle, ref mi);
            if (_succes) {
                this.DeviceName = mi.DeviceName;
                this.Bounds = new Rectangle(mi.Monitor.left, mi.Monitor.top, mi.Monitor.right - mi.Monitor.left, mi.Monitor.bottom - mi.Monitor.top);
                this.Primary = (mi.Flags & NativeMethods.MONITORINFOF_PRIMARY) == NativeMethods.MONITORINFOF_PRIMARY;
                this.WorkingArea = new Rectangle(mi.WorkArea.left, mi.WorkArea.top, mi.WorkArea.right - mi.WorkArea.left, mi.WorkArea.bottom - mi.WorkArea.top);
            }
        }

        private IntPtr Handle { get; set; }
        public string DeviceName { get; private set; }
        public Rectangle Bounds { get; private set; }
        public Rectangle WorkingArea { get; private set; }
        public bool Primary { get; private set; }
        private bool _succes;

        public static Screen PrimaryScreen { 
            get {
                var allScreens = AllScreens;
                if (allScreens.Where(s => s.Primary).Count() > 0)
                    return allScreens.Where(s => s.Primary).First();
                return allScreens.First();
            }
        }
        public static List<Screen> AllScreens {
            get {
                var screens = new List<Screen>();
                NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
                    delegate (IntPtr hMonitor, IntPtr hdcMonitor, ref NativeMethods.Rect lprcMonitor, IntPtr dwData) {
                        var s = new Screen(hMonitor);
                        if (s._succes)
                            screens.Add(s);
                        return true;
                    }, IntPtr.Zero);
                return screens;
            }
        }
        public static Screen? FromHandle(IntPtr handle) {
            var s = new Screen(NativeMethods.MonitorFromWindow(handle, NativeMethods.MONITOR_DEFAULTTONEAREST));
            if (s._succes)
                return s;
            return null;
        }
        public static void LockDesktopUpdate() {
            NativeMethods.LockWindowUpdate(NativeMethods.GetDesktopWindow());
        }
        public static void UnlockDesktopUpdate() {
            NativeMethods.LockWindowUpdate(IntPtr.Zero);
        }
    }
}
