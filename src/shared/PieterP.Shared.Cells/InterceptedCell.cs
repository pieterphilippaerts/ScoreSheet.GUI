using System;
using System.Collections.Generic;
using System.Text;

namespace PieterP.Shared.Cells {
    public class InterceptedCell<T> : ConcreteCell<T> {
        public InterceptedCell(T initialValue)
            : base(initialValue) {
        }
        public override T Value {
            get {
                var ea = new InterceptedEventArgs<T>();
                ea.Value = base.Value;
                GetValue?.Invoke(ea);
                return ea.Value;
            }
            set {
                var ea = new InterceptedEventArgs<T>();
                ea.Value = value;
                SetValue?.Invoke(ea);
                base.Value = ea.Value;
            }
        }
        public event Action<InterceptedEventArgs<T>> SetValue;
        public event Action<InterceptedEventArgs<T>> GetValue;
    }
    public class InterceptedEventArgs<T> { 
        public T Value { get; set; }
    }
}
