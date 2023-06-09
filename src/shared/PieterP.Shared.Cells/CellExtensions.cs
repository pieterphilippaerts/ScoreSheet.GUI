﻿/*
 *   Code gratuitously stolen from my dear friend Frédéric Vogels
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PieterP.Shared.Cells {
    public static class CellExtensions
    {
        public static Cell<R> Map<T, R>( this Cell<T> cell, Func<T, R> func )
        {
            return Cell.Derived( cell, func );
        }

        public static Cell<bool> Negate( this Cell<bool> cell )
        {
            return cell.Map( x => !x );
        }
    }
}
