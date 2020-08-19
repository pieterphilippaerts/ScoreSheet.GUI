using System;
using System.Collections.Generic;
using System.Text;

namespace PieterP.ScoreSheet.ViewModels.Score {
    public class DoublePlayerOption {
        public DoublePlayerOption(string name) {
            this.Name = name;
        }
        public string Name { get; private set; }

        public static IEnumerable<DoublePlayerOption> Permutation123 {
            get {
                return new DoublePlayerOption[] {
                    new DoublePlayerOption("1/2" ),
                    new DoublePlayerOption("1/3" ),
                    new DoublePlayerOption("2/3" )
                };
            }
        }
        public static IEnumerable<DoublePlayerOption> Permutation12 {
            get {
                return new DoublePlayerOption[] {
                    new DoublePlayerOption("1/2" )
                };
            }
        }
    }
}
