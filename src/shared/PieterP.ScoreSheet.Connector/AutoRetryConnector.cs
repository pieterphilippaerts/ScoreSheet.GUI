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
                return fe.Code == null ? true : false; // if we get a specific TabT error, do not retry
            }
            /* 
               TabT Error Codes
                2 => DivisionId not numeric
                3 => DivisionId invalid
                4 => WeekNAme not valid
                5 => Unable to process ranking for division
                6 => Season not numeric
                7 => Season not valid
                8 => Unexpected database error
                9 => Club is invalid
                10 => Club has no team in division
                11 => Club has no team in division
                12 => Club parameter required
                13 => Required parameter missing
                14 => Club has no team
                15 => DivisionCategory not numeric
                16 => DivisionCategory invalid
                17 => Club is empty
                18 => Level not numeric
                19 => Level invalid
                20 => WeekName invalid
                21 => ShowDivisionNAme not valid
                22 => No permission to upload data
                23 => Missing upload data
                24 => Club category not numeric
                25 => Club category not valid
                26 => Missing search criteria
                27 => No permission to see extended information
                28 => Member unique index not valid
                29 => Player category not numeric
                30 => Player category invalid
                31 => RankingSystem not valid
                32 => YearDateFrom not valid
                33 => YearDateTo not valid
                34 => Quota exceeded
                35 => Tournament serie index missing
                36 => Tournament serie index not numeric
                37 => Tournament serie index invalid
                39 => Tournament unique index not numeric
                40 => Tournament unique index invalid
                41 => Missing player
                42 => Player unique index not numeric
                43 => Player unique index invalid
                44 => Too many players
                45 => Invalid player (same player twice)
                46 => Player already registered in tournament
                47 => No permission to register tournament players
                48 => Player not registered
                49 => Tournament unique index not numeric
                50 => Tournament unique index invalid
                51 => Missing UniqueIndex or Club
                56 => Tournament unique index missing
                57 => Tournament unique index missing
            */
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
