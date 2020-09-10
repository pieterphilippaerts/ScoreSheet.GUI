using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PieterP.ScoreSheet.Model.Interfaces {
    /// <summary>
    /// Represents a service that raises events whenever the network becomes available
    /// </summary>
    public interface INetworkAvailabilityService {
        event Action? NetworkAvailable;
        void TriggerManually();
    }
}
