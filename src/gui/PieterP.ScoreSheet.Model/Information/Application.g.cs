﻿using System;

// Make sure you have installed T4Executer
// https://marketplace.visualstudio.com/items?itemName=TimMaes.ttexecuter
namespace PieterP.ScoreSheet.Model.Information {
    public static partial class Application {
        public static DateTime CompilationTimestamp { 
			get { 
				return new DateTime(637650001934265773L, DateTimeKind.Utc); 
			}
		}
    }
}