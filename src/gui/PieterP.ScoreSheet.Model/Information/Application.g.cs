﻿using System;

// Make sure you have installed T4Executer
// https://marketplace.visualstudio.com/items?itemName=TimMaes.ttexecuter
namespace PieterP.ScoreSheet.Model.Information {
    public static partial class Application {
        public static DateTime CompilationTimestamp { 
			get { 
				return new DateTime(637356925957882537L, DateTimeKind.Utc); 
			}
		}
    }
}