using PieterP.ScoreSheet.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace PieterP.ScoreSheet.GUI.Services {
    /// <summary>
    /// Represents a service that raises events whenever the network becomes available
    /// </summary>
    public class NetworkAvailabilityService : INetworkAvailabilityService {
        public NetworkAvailabilityService() {
            NetworkChange.NetworkAvailabilityChanged += (s, e) => {
                if (e.IsAvailable)
                    TriggerManually();
            };
        }

        public event Action? NetworkAvailable;

        public void TriggerManually() {
            NetworkAvailable?.Invoke();
        }
    }
}
