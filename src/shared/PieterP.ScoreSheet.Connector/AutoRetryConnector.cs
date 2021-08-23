using PieterP.ScoreSheet.Localization;
using PieterP.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PieterP.ScoreSheet.Connector {
    /// <summary>
    /// Wraps another IConnector (by default a TabTConnector) and adds retry
    /// functionality if the call fails for some reason...
    /// </summary>
    public class AutoRetryConnector : IConnector {
        public AutoRetryConnector() : this(new TabTConnector(), 2) { }
        public AutoRetryConnector(IConnector internalConnector, int retryCount) {
            _internalConnector = internalConnector;
            _retryCount = retryCount;
        }
        private IConnector _internalConnector;
        private int _retryCount;

        private async Task<T> TryDownload<T>(Func<Task<T>> call) {
            int tries = 1;
            while (true) {
                try {
                    return await call();
                } catch (Exception e) {
                    if (tries < _retryCount && ShouldRetry(e)) {
                        Logger.Log(LogType.Warning, Safe.Format(Errors.TabT_ServiceException, e.Message));
                        Logger.LogDebug(e.ToString()); // log the full exception details
                    } else {
                        throw;
                    }
                }
                tries++;
            }
        }
        private bool ShouldRetry(Exception e) {
            if (e is FaultException fe) {
                return fe.Code?.Name == "22" ? false : true; // error code 22 => invalid credentials
            }
            // TODO: add other specific cases when we should not retry multiple times?
            
            return true; // retry is the default
        }
        public Task<TabTSeason> GetActiveSeason() => TryDownload(() => _internalConnector.GetActiveSeason());
        public Task<IEnumerable<TabTClub>> GetClubsAsync(TabTSeason? season = null) => TryDownload(() => _internalConnector.GetClubsAsync(season));
        public Task<IEnumerable<TabTDivision>> GetDivisions(TabTDivisionRegion? level, TabTSeason? season = null) => TryDownload(() => _internalConnector.GetDivisions(level, season));
        public Task<TabTMatch?> GetMatchDetails(string club, string matchId, TabTSeason? season = null) => TryDownload(() => _internalConnector.GetMatchDetails(club, matchId, season));
        public Task<IEnumerable<TabTMatch>> GetMatches(string clubId, TabTTeam team, TabTSeason? season = null) => TryDownload(() => _internalConnector.GetMatches(clubId, team, season));
        public Task<IEnumerable<TabTMatch>> GetMatches(int divisionId, string? weekName = null, TabTSeason? season = null) => TryDownload(() => _internalConnector.GetMatches(divisionId, weekName, season));
        public Task<IEnumerable<TabTMatchSystem>> GetMatchSystemsAsync() => TryDownload(() => _internalConnector.GetMatchSystemsAsync());
        public Task<IEnumerable<TabTMember>> GetMembers(string clubId, int category, bool extendedInfo = false, TabTSeason? season = null) => TryDownload(() => _internalConnector.GetMembers(clubId, category, extendedInfo, season));
        public Task<IEnumerable<TabTSeason>> GetSeasonsAsync() => TryDownload(() => _internalConnector.GetSeasonsAsync());
        public Task<IEnumerable<TabTTeam>> GetTeams(string clubId, TabTSeason? season = null) => TryDownload(() => _internalConnector.GetTeams(clubId, season));
        public Task<(TabTErrorCode ErrorCode, TabTInfo? Info)> TestAsync() => TryDownload(() => _internalConnector.TestAsync()); // do not auto retry this
        public Task<(TabTErrorCode, IEnumerable<string>)> UploadAsync(string csv) => _internalConnector.UploadAsync(csv); // do not auto retry this
        public void SetDefaultCredentials(string? username, string? password) => _internalConnector.SetDefaultCredentials(username, password);
        public bool IsAnonymous => _internalConnector.IsAnonymous;
        public IDictionary<string, int> Statistics => _internalConnector.Statistics;
    }
}
