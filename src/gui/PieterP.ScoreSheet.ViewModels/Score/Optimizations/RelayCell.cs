using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PieterP.Shared.Cells;

#nullable disable
namespace PieterP.ScoreSheet.ViewModels.Score.Optimizations {
    public class RelayCell<T> : Cell<T> {
        public RelayCell() {
            _currentTarget = null;
        }
        public override void Refresh() {
            // NOP
        }
        public void ChangeTarget(Cell<T> relayTo) {
            var org = _currentTarget;
            _currentTarget = relayTo;

            if (org != null)
                ((INotifyPropertyChanged)org).PropertyChanged -= OnChanged;
            if (relayTo != null)
                ((INotifyPropertyChanged)relayTo).PropertyChanged += OnChanged;

            if (org == null || _currentTarget == null || !Util.AreEqual(org.Value, _currentTarget.Value)) {
                NotifyObservers();
            }
        }
        private void OnChanged(object sender, PropertyChangedEventArgs e) {
            NotifyObservers();
        }
        public override T Value {
            get {
                if (_currentTarget == null)
                    return default;
                return _currentTarget.Value;
            }
            set {
                if (_currentTarget != null) {
                    if (!Util.AreEqual(value, _currentTarget.Value)) {
                        _currentTarget.Value = value;
                        NotifyObservers();
                    }
                }
            }
        }
        private Cell<T> _currentTarget;
    }
}