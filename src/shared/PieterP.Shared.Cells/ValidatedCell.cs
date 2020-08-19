/*
 *   Code gratuitously stolen from my dear friend Frédéric Vogels
 */

using System;

namespace PieterP.Shared.Cells {
    public class ValidatedCell<T> : ConcreteCell<T> {
        private readonly Func<T, bool>? validator;
        private readonly Func<T, T>? corrector;

        /// <summary>
        /// Validates the value with the given validator and throws an exception is the value is not valid.
        /// </summary>
        public ValidatedCell(T initialValue, Func<T, bool> validator)
            : base(initialValue) {
            this.validator = validator;
        }
        /// <summary>
        /// Corrects the value with the given corrector, if necessary.
        /// </summary>
        public ValidatedCell(T initialValue, Func<T, T> corrector)
           : base(initialValue) {
            this.corrector = corrector;
        }

        public override T Value {
            get {
                return base.Value;
            }
            set {
                if (corrector != null) {
                    base.Value = corrector(value);
                } else if (!validator(value)) {
                    throw new ArgumentException("Invalid value");
                } else {
                    base.Value = value;
                }
            }
        }
    }
}