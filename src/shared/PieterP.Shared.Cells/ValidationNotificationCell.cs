using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace PieterP.Shared.Cells {
    public class ValidationNotificationCell<T> : ConcreteCell<T>, INotifyDataErrorInfo {
        /// <summary>
        /// Creates a validated cell that implements the INotifyDataErrorInfo interface.
        /// </summary>
        /// <param name="initialValue">The initial value of the cell.</param>
        /// <param name="validator">A function that performs the validation. It returns a list of errors, or <b>null</b> if there are no errors.</param>
        public ValidationNotificationCell(T initialValue, params Func<T, ICollection?>[] validators)
            : base(initialValue) {
            if (validators == null)
                _validators = new Func<T, ICollection?>[0];
            else
                _validators = validators;
            Validate();
        }

        public override T Value {
            get {
                return base.Value;
            }
            set {
                base.Value = value;
                Validate();
                
            }
        }
        private void Validate() {
            var hadErrors = HasErrors;
            ArrayList? allErrors = null;
            foreach(var validator in _validators) {
                var errors = validator.Invoke(Value);
                if (errors != null && errors.Count > 0) {
                    if (allErrors == null)
                        allErrors = new ArrayList();
                    allErrors.AddRange(errors);
                }
            }
            _errors = allErrors;
            HasErrors = _errors != null;
            if (HasErrors || hadErrors) {
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs("Value"));
            }
        }

        public bool HasErrors { get; private set; }

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public IEnumerable? GetErrors(string propertyName) {
            return _errors;
        }

        private readonly Func<T, ICollection?>[] _validators;
        private IEnumerable? _errors;
    }
}