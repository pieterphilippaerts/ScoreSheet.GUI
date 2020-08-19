using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Threading;
using PieterP.Shared;
using PieterP.Shared.Interfaces;

namespace PieterP.ScoreSheet.GUI.Services {
    public class WpfTimerService : ITimerService {
        public WpfTimerService() {
            _timer = null;
        }

        private void Timer_Tick(object sender, EventArgs e) {
            Tick?.Invoke(this);
        }

        public void Start(TimeSpan interval) {
            if (_timer == null) {
                _timer = new DispatcherTimer();
                _timer.Tick += Timer_Tick;
            }
            _timer.Interval = interval;
            if (!_timer.IsEnabled) {
                _timer.Start();
            }
        }

        public void Stop() {
            _timer?.Stop();
        }

        public event Action<ITimerService> Tick;
        private DispatcherTimer _timer;
    }
}
