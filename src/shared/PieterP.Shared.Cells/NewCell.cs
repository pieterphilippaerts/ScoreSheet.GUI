//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Text;

//namespace PieterP.Shared.Cells {
//    public static class NewCell {
//        // Create
//        // Derive
//        // Observe
//        // Convert
//        // Validate
//        // ReadOnly
//        // Constant
//        // Async
//    }

//    public class NewCell<T> : INotifyPropertyChanged, INotifyPropertyChanging {
//        public NewCell(T initialValue = default(T)) {
//            _value = initialValue;
//        }

//        public virtual T Value {
//            get {
//                return _value;
//            }
//            set {
//                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Value)));
//                _value = value;
//                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
//            }
//        }

//        public override string ToString() {
//            return $"Cell[{ (_value == null ? "null" : _value.ToString()) }]";
//        }

//        public override bool Equals(object? obj) {
//            var co = obj as NewCell<T>;
//            if (co == null)
//                return false;
//            if (_value == null)
//                return co._value == null;
//            else
//                return _value.Equals(co);
//        }

//        public override int GetHashCode() {
//            int vh = 0;
//            if (_value != null)
//                vh = ShiftAndWrap(_value.GetHashCode(), 16);
//            return base.GetHashCode() ^ vh;

//            int ShiftAndWrap(int value, int positions) {
//                positions = positions & 0x1F;
//                // Save the existing bit pattern, but interpret it as an unsigned integer.
//                uint number = BitConverter.ToUInt32(BitConverter.GetBytes(value), 0);
//                // Preserve the bits to be discarded.
//                uint wrapped = number >> (32 - positions);
//                // Shift and wrap the discarded bits.
//                return BitConverter.ToInt32(BitConverter.GetBytes((number << positions) | wrapped), 0);
//            }
//        }

//        private T _value;

//        public event PropertyChangedEventHandler? PropertyChanged;
//        public event PropertyChangingEventHandler? PropertyChanging;
//    }

//    public class NewDerived<T> : NewCell<T> {
//        // meerdere naar een waarde
//    }

//    public class NewObserverCel<T> : NewCell<T> {
//        // observeert INotifyPropertyChanged

//    }

//    public class NewValidatingCell<T> : NewCell<T> {
//        // IDataErrorInfo, INotifyDataErrorInfo
//    }

//    public class NewConstantCell<T> : NewCell<T> { 
    
//    }

//    public class NewReadOnlyCell<T> : NewCell<T> {

//    }

//    public interface IReadOnlyCell<T> : INotifyPropertyChanged {
//        T Value { get; }
//    }
//}
