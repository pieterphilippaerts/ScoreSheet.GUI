﻿/*
 *   Code gratuitously stolen from my dear friend Frédéric Vogels
 */

namespace PieterP.Shared.Cells {
    public class ConcreteCell<T> : Cell<T>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="initialValue">
        /// Cell's initial value.
        /// </param>
        public ConcreteCell( T initialValue = default( T ) )
            : base( initialValue )
        {
            // NOP
        }

        /// <summary>
        /// Value of the cell.
        /// </summary>
        public override T Value
        {
            get
            {
                return base.Value;
            }
            set
            {
                if ( !Util.AreEqual( base.Value, value ) )
                {
                    base.Value = value;
                    NotifyObservers();
                }
            }
        }

        protected void InternalSetValue(T value) {
            base.Value = value;
        }


        public override string ToString()
        {
            var stringRepresentation = this.Value != null ? this.Value.ToString() : "null";

            return $"CELL[{stringRepresentation}]";
        }

        public override void Refresh()
        {
            // NOP
        }
    }
}
