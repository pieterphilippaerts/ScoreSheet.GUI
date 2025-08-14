using System;

// Make sure you have installed T4Executer
// https://marketplace.visualstudio.com/items?itemName=TimMaes.ttexecuter
namespace PieterP.ScoreSheet.Model.Information {
    public static partial class Application {
        public static DateTime CompilationTimestamp { 
			get { 
				return new DateTime(638908104473423228L, DateTimeKind.Utc); 
			}
		}
    }
}