using PieterP.ScoreSheet.Model.Interfaces;
using PieterP.Shared.Cells;
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
            _availabilityValue = NetworkInterface.GetIsNetworkAvailable();
            _isAvailable = new ReadonlyManualCell<bool>(() => _availabilityValue);
            NetworkChange.NetworkAvailabilityChanged += (s, e) => {
                TriggerManually();
            };
        }

        public void TriggerManually() {
            _availabilityValue = NetworkInterface.GetIsNetworkAvailable();
            _isAvailable.Refresh();
        }

        public ReadonlyManualCell<bool> IsNetworkAvailable { 
            get {
                return _isAvailable;
            }
        }

        private ReadonlyManualCell<bool> _isAvailable;
        private bool _availabilityValue;
    }
}
