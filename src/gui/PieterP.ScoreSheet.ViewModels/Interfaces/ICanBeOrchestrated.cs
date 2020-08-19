using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PieterP.ScoreSheet.ViewModels.Interfaces {
    public interface ICanBeOrchestrated : IDisposable {
        void UpdateScreen();
    }
}
