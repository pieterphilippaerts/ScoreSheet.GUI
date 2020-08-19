using System;
using System.Collections.Generic;
using System.Text;

namespace PieterP.Shared.Interfaces {
    public interface IWindowHandle {
        public IntPtr Handle { get; }
    }
    public interface IMainWindowHandle : IWindowHandle { }
    public interface IActiveWindowHandle  : IWindowHandle { }
}
