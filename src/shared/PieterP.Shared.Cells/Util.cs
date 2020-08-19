/*
 *   Code gratuitously stolen from my dear friend Frédéric Vogels
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PieterP.Shared.Cells {
    public class Util
    {
        public static bool AreEqual<T>( T x, T y )
        {
            if ( x is null )
            {
                return y is null;
            }
            else
            {
                return x.Equals( y );
            }
        }
    }
}
