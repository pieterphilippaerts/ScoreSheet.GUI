using System;
using System.Collections.Generic;
using System.Text;

namespace PieterP.Shared.Interfaces {
    public interface ITimerService {
        event Action<ITimerService> Tick;
        void Start(TimeSpan interval);
        void Stop();
    }
}
