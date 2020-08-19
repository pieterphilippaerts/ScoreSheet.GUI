using System;
using System.Collections.Generic;
using System.Text;

namespace PieterP.ScoreSheet.Model.Information {
    public class Keyboard {
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