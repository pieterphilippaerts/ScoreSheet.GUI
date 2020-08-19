using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace PieterP.ScoreSheet.Connector {
    public class TabTInfo {
        public TabTInfo(Version apiVersion, long allowedQuota, long currentQuota, string? database, bool validAccount, CultureInfo culture) {
            this.ApiVersion = apiVersion;
            this.AllowedQuota = allowedQuota;
            this.CurrentQuota = currentQuota;
            this.DatabaseName = database;
            this.ValidAccount = validAccount;
            this.Culture = culture;
        }
        public long AllowedQuota { get; private set; }
        public long CurrentQuota { get; private set; }
        public Version ApiVersion { get; private set; }
        public string? DatabaseName { get; private set; }
        public bool ValidAccount { get; private set; }
        public CultureInfo Culture { get; private set; }
    }
}
