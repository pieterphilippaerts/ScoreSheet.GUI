/*
 *   Code gratuitously stolen from my dear friend Frédéric Vogels
 */

using System;
using System.Diagnostics;

namespace PieterP.Shared.Cells
{
    public abstract class Var {
        // needed for serialization
        public abstract object? Serialize();
        public abstract void Deserialize(object? input);
    }
    [DebuggerDisplay( "{Value}" )]
    public class Var<T> : Var, IVar<T>
    {
        private T value;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="initialValue">
        /// Var's initial value.
        /// </param>
        public Var( T initialValue = default( T ) )
        {
            this.value = initialValue;
        }

        /// <summary>
        /// Value of the Var.
        /// </summary>
        public virtual T Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
            }
        }

        public override string ToString()
        {
            var stringRepresentation = value != null ? value.ToString() : "null";

            return $"VAR[{stringRepresentation}]";
        }

        public override bool Equals( object obj )
        {
            throw new NotImplementedException( "Equals is not implemented for Vars" );
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException( "GetHashCode is not implemented for Vars" );
        }

        public override object? Serialize() {
            var v = this.Value as Var;
            if (v == null)
                return this.Value;
            else
                return v.Serialize();
        }

        public override void Deserialize(object? input) {
            var v = this.Value as Var;
            if (v == null) {
                this.Value = (T)input;
            } else {
                v.Deserialize(input);
            }
        }
    }
}
