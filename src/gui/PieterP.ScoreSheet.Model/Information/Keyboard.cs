using PieterP.Shared.Cells;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Web.UI;

namespace PieterP.ScoreSheet.Model.Information {
    public class Keyboard {
        /*        
        static Keyboard() {
            CapsLockOn = Cell.Create<bool>(IsCapsLockToggled);
            _hookID = SetHook(_proc);
        }
        ~Keyboard() {
            if (_hookID != IntPtr.Zero) {
                NativeMethods.UnhookWindowsHookEx(_hookID);
                _hookID = IntPtr.Zero;
            }
        }
        private static IntPtr SetHook(NativeMethods.LowLevelKeyboardProc proc) {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule) {
                return NativeMethods.SetWindowsHookEx(NativeMethods.WH_KEYBOARD_LL, proc, NativeMethods.GetModuleHandle(curModule.ModuleName), 0);
            }
        }
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode >= 0 && wParam == (IntPtr)NativeMethods.WM_KEYUP) {
                int vkCode = Marshal.ReadInt32(lParam);
                // Check if CapsLock was pressed
                if (vkCode == NativeMethods.VK_CAPITAL) {
                    CapsLockOn.Value = IsCapsLockToggled;
                }
            }
            return NativeMethods.CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

         public static Cell<bool> CapsLockOn { get; }
        private static bool IsCapsLockToggled => (NativeMethods.GetKeyState(NativeMethods.VK_CAPITAL) & NativeMethods.KEY_TOGGLED) == NativeMethods.KEY_TOGGLED;


        private static IntPtr _hookID = IntPtr.Zero;
        private static NativeMethods.LowLevelKeyboardProc _proc = HookCallback;
        */

        public static void SetKey(byte key, bool bState) {
            byte[] keyState = new byte[256];

            NativeMethods.GetKeyboardState(keyState);
            if ((bState && !((keyState[key] & 1) == 1)) ||
                (!bState && (keyState[key] & 1) == 1)) {
                // Simulate a key press
                NativeMethods.keybd_event(key,
                            0x45,
                            NativeMethods.KEYEVENTF_EXTENDEDKEY | 0,
                            0);

                // Simulate a key release
                NativeMethods.keybd_event(key,
                             0x45,
                             NativeMethods.KEYEVENTF_EXTENDEDKEY | NativeMethods.KEYEVENTF_KEYUP,
                             0);
            }
        }
    }
}