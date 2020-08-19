#if DEBUG
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.Connector {
    public class DebugConnector : IConnector {
        public DebugConnector() {
            //
        }
        public void SetDefaultCredentials(string? username, string? password) {
            if (username == null || username.Length == 0 || password == null || password.Length == 0) {
                _credentials = null;
            } else {
                _credentials = new DebugService.CredentialsType();
                _credentials.Account = username;
                _credentials.Password = password;
            }
        }

        public Task<TabTMatch> GetMatchDetails(string club, string matchId, TabTSeason? season = null) {
            return null;
        }
        #region Information
        public async Task<(TabTErrorCode ErrorCode, TabTInfo? Info)> TestAsync() {
            Count(nameof(_fetcher.TestAsync));
            var testRequest = new DebugService.TestRequest();
            testRequest.Credentials = _credentials;

            DebugService.TestResponse1 testResponse;
            try {
                Count(nameof(_fetcher.TestAsync));
                testResponse = await _fetcher.TestAsync(testRequest);
            } catch (Exception e) {
                Logger.Log(e);
                return (TabTErrorCode.NetworkError, null);
            }

            // CULTURE
            var culture = testResponse.TestResponse.Language.ToCulture();
            Version apiVersion;
            if (Version.TryParse(testResponse.TestResponse.ApiVersion, out var result)) {
                apiVersion = result;
                Logger.Log(LogType.Debug, $"Using API version { apiVersion.ToString() }");
            } else {
                apiVersion = new Version(0, 0, 0);
                Logger.Log(LogType.Warning, $"Cannot parse version type received from web service: { testResponse.TestResponse.ApiVersion }");
            }

            // DATABASE NAME
            var databaseName = testResponse.TestResponse.Database;
            if (databaseName == null || databaseName.Length == 0) {
                Logger.Log(LogType.Warning, "The name of the database used by TabT is empty.");
            } else {
                Logger.Log(LogType.Debug, $"The name of the TabT database is '{ databaseName }'");
            }

            // QUOTA
            long allowedQuota;
            if (long.TryParse(testResponse.TestResponse.AllowedQuota, out var allowed)) {
                allowedQuota = allowed;
            } else {
                allowedQuota = -1;
                Logger.Log(LogType.Warning, $"could not parse allowed quota: { testResponse.TestResponse.AllowedQuota }");
            }
            long currentQuota;
            if (long.TryParse(testResponse.TestResponse.CurrentQuota, out var current)) {
                currentQuota = current;
            } else {
                currentQuota = -1;
                Logger.Log(LogType.Warning, $"could not parse current quota: { testResponse.TestResponse.CurrentQuota }");
            }
            if (currentQuota != -1 && allowedQuota != -1) {
                Logger.Log(LogType.Informational, $"Currently, { currentQuota } quota is used out of a total of { allowedQuota }.");
            }

            // IS VALID?
            var validAccount = testResponse.TestResponse.IsValidAccount;
            if (!validAccount) {
                return (TabTErrorCode.InvalidCredentials, null);
            }
            return (TabTErrorCode.NoError, new TabTInfo(apiVersion, allowedQuota, currentQuota, databaseName, validAccount, culture));
        }
        #endregion

        #region Seasons
        private List<TabTSeason>? _seasons;
        public async Task<IEnumerable<TabTSeason>> GetSeasonsAsync() {
            if (_seasons != null)
                return _seasons;

            var seasonsRequest = new DebugService.GetSeasonsRequest();
            seasonsRequest.Credentials = _credentials;
            Count(nameof(_fetcher.GetSeasonsAsync));
            var seasonsResponse = await _fetcher.GetSeasonsAsync(seasonsRequest);

            var entries = seasonsResponse?.GetSeasonsResponse?.SeasonEntries;
            if (entries == null || entries.Length == 0) {
                Logger.Log(LogType.Warning, "The list of seasons is empty.");
                return Enumerable.Empty<TabTSeason>();
            }
            _seasons = new List<TabTSeason>();
            foreach (var s in entries) {
                if (int.TryParse(s.Season, out var sid)) {
                    _seasons.Add(new TabTSeason(sid, s.Name, s.IsCurrent));
                } else {
                    Logger.Log(LogType.Warning, $"Invalid season ID: '{ s.Season }' for season '{ s.Name }'");
                }
            }
            return _seasons;
        }
        public async Task<TabTSeason> GetActiveSeason() {
            var seasons = await GetSeasonsAsync();
            return seasons.Where(c => c.IsCurrent).Single();
        }
        #endregion

        #region Upload
        public async Task<(TabTErrorCode, IEnumerable<string>)> UploadAsync(string csv) {
            var request = new DebugService.UploadRequest();
            request.Credentials = _credentials;
            request.Data = csv;
            DebugService.UploadResponse1 response;
            try {
                Count(nameof(_fetcher.UploadAsync));
                response = await _fetcher.UploadAsync(request);
            } catch (Exception e) {
                Logger.Log(e);
                return (TabTErrorCode.NetworkError, new string[] { e.Message });
            }
            if (!response.UploadResponse.Result && response.UploadResponse.ErrorLines != null) {
                return (TabTErrorCode.DataError, response.UploadResponse.ErrorLines);
            }
            return (TabTErrorCode.NoError, Enumerable.Empty<string>());
        }
        #endregion

        #region GetClubs
        public async Task<IEnumerable<TabTClub>> GetClubsAsync(TabTSeason? season = null) {
            Count(nameof(_fetcher.GetClubsAsync));
            var request = new DebugService.GetClubs();
            request.Credentials = _credentials;
            if (season != null)
                request.Season = season.Id.ToString();
            var ret = await _fetcher.GetClubsAsync(request);
            if (ret?.GetClubsResponse?.ClubEntries == null || ret.GetClubsResponse.ClubEntries.Length == 0)
                return Enumerable.Empty<TabTClub>();

            var retList = new List<TabTClub>();
            foreach (var club in ret.GetClubsResponse.ClubEntries) {
                var venues = new List<TabTVenue>();
                if (club.VenueEntries != null) {
                    foreach (var venue in club.VenueEntries) {
                        if (int.TryParse(venue.Id, out var venueId) && int.TryParse(venue.ClubVenue, out var clubVenueId)) {
                            venues.Add(new TabTVenue(venueId, clubVenueId, venue.Name, venue.Street, venue.Town, venue.Phone, venue.Comment));
                        } else {
                            Logger.Log(LogType.Warning, $"Invalid VenuId/ClubVenueId found for club { club.Name } ({ club.UniqueIndex })");
                        }
                    }
                }
                if (int.TryParse(club.Category, out var category)) {
                    retList.Add(new TabTClub(club.Name, club.LongName, category, club.CategoryName, club.UniqueIndex, venues));
                } else {
                    Logger.Log(LogType.Warning, $"Invalid Category found for club { club.Name } ({ club.UniqueIndex })");
                }
            }
            return retList;
        }
        #endregion

        #region GetClubTeams
        public async Task<IEnumerable<TabTTeam>> GetTeams(string clubId, TabTSeason? season = null) {
            Count(nameof(_fetcher.GetClubTeamsAsync));
            var request = new DebugService.GetClubTeamsRequest();
            request.Club = clubId;
            request.Credentials = _credentials;
            if (season != null) {
                request.Season = season.Id.ToString();
            }
            var ret = await _fetcher.GetClubTeamsAsync(request);
            if (ret?.GetClubTeamsResponse?.TeamEntries == null || ret.GetClubTeamsResponse.TeamEntries.Length == 0)
                return Enumerable.Empty<TabTTeam>();

            var retList = new List<TabTTeam>();
            foreach (var team in ret.GetClubTeamsResponse.TeamEntries) {
                if (int.TryParse(team.DivisionCategory, out var divisionCategory)
                        && int.TryParse(team.DivisionId, out var divisionId)
                        && int.TryParse(team.MatchType, out var matchType)
                        && team.TeamId != null && team.TeamId.Length > 0) {
                    retList.Add(new TabTTeam(divisionCategory, divisionId, team.DivisionName ?? "", team.Team ?? "", team.TeamId, matchType));
                } else {
                    Logger.Log(LogType.Warning, $"Invalid team data found for one of the teams in club { clubId }");
                }
            }
            return retList;
        }
        #endregion

        #region GetDivisionRanking
        // TODO; we don't need that right now for ScoreSheet
        #endregion

        #region GetMatches
        public Task<IEnumerable<TabTMatch>> GetMatches(string clubId, TabTTeam team, TabTSeason? season = null) {
            return GetMatches(clubId, team, null, null, season);
        }
        public Task<IEnumerable<TabTMatch>> GetMatches(int divisionId, string? weekName = null, TabTSeason? season = null) {
            return GetMatches(null, null, divisionId, weekName, season);
        }
        private async Task<IEnumerable<TabTMatch>> GetMatches(string? clubId, TabTTeam? team, int? divisionId, string? weekName, TabTSeason? season = null) {
            Count(nameof(_fetcher.GetMatchesAsync));
            var request = new DebugService.GetMatchesRequest();
            request.Club = clubId;
            request.Credentials = _credentials;
            if (season != null)
                request.Season = season.Id.ToString();

            request.DivisionCategory = team?.DivisionCategory.ToString();
            request.DivisionId = divisionId?.ToString() ?? team?.DivisionId.ToString();
            request.Team = team?.Team;
            request.WeekName = weekName;
            var ret = await _fetcher.GetMatchesAsync(request);
            if (ret?.GetMatchesResponse?.TeamMatchesEntries == null || ret.GetMatchesResponse.TeamMatchesEntries.Length == 0)
                return Enumerable.Empty<TabTMatch>();

            var retList = new List<TabTMatch>();
            foreach (var match in ret.GetMatchesResponse.TeamMatchesEntries) {
                int? venue = null;
                if (match.Venue != null) {
                    int v;
                    if (int.TryParse(match.Venue, out v))
                        venue = v;
                    else
                        Logger.Log(LogType.Warning, $"Unable to parse match information for match { match.MatchUniqueId }");
                }
                retList.Add(new TabTMatch(match.AwayTeam, match.Date, match.DateSpecified, match.HomeTeam,
                    match.MatchId, match.Score, match.Time, match.TimeSpecified, match.WeekName, match.HomeClub, match.AwayClub, venue, match.VenueClub));
            }
            return retList;
        }
        #endregion

        #region GetMembers
        public async Task<IEnumerable<TabTMember>> GetMembers(string clubId, int category, bool extendedInfo = false, TabTSeason? season = null) {
            Count(nameof(_fetcher.GetMembersAsync));
            var request = new DebugService.GetMembersRequest();
            request.Club = clubId;
            request.Credentials = _credentials;
            if (season != null)
                request.Season = season.Id.ToString();
            request.PlayerCategory = category.ToString();
            if (extendedInfo && _credentials != null) {
                request.ExtendedInformation = true;
                request.ExtendedInformationSpecified = true;
            }
            var ret = await _fetcher.GetMembersAsync(request);
            if (ret?.GetMembersResponse?.MemberEntries == null || ret.GetMembersResponse.MemberEntries.Length == 0)
                return Enumerable.Empty<TabTMember>();

            var retList = new List<TabTMember>();
            var t = CultureInfo.InvariantCulture.TextInfo;
            foreach (var member in ret.GetMembersResponse.MemberEntries) {
                if (int.TryParse(member.Position, out var position)
                        && int.TryParse(member.UniqueIndex, out var vttlIndex)) {
                    int? rankingIndex = null;
                    if (member.RankingIndex != null && member.RankingIndex.Length > 0 && int.TryParse(member.RankingIndex, out var res)) {
                        rankingIndex = res;
                    }
                    retList.Add(new TabTMember(t.ToTitleCase(member.FirstName.ToLowerInvariant()),
                        t.ToTitleCase(member.LastName.ToLowerInvariant()),
                        vttlIndex,
                        position,
                        member.Ranking,
                        rankingIndex,
                        member.Status));
                } else {
                    Logger.Log(LogType.Warning, $"Unable to parse member information for { member.FirstName } { member.LastName }");
                }
            }
            return retList;
        }
        #endregion

        #region GetDivisions
        public async Task<IEnumerable<TabTDivision>> GetDivisions(TabTDivisionRegion? level, TabTSeason? season = null) {
            Count(nameof(_fetcher.GetDivisionsAsync));
            var request = new DebugService.GetDivisions();
            request.Credentials = _credentials;
            if (season != null)
                request.Season = season.Id.ToString();
            //request.ShowDivisionName = DebugService.ShowDivisionNameType.yes;
            //request.ShowDivisionNameSpecified = true;
            if (level != null) {
                request.Level = ((int)level).ToString();
            }
            var ret = await _fetcher.GetDivisionsAsync(request);
            if (ret?.GetDivisionsResponse?.DivisionEntries == null || ret.GetDivisionsResponse.DivisionEntries.Length == 0)
                return Enumerable.Empty<TabTDivision>();
            var retList = new List<TabTDivision>();
            foreach (var division in ret.GetDivisionsResponse.DivisionEntries) {
                if (int.TryParse(division.DivisionId, out var divisionId)
                        && int.TryParse(division.DivisionCategory, out var divisionCategory)
                        && int.TryParse(division.Level, out var divLevel)
                        && int.TryParse(division.MatchType, out var matchType)) {
                    retList.Add(new TabTDivision(divisionId, division.DivisionName, divisionCategory, divLevel, matchType));
                }
            }
            return retList;
        }
        #endregion

        #region GetMatchSystems
        public async Task<IEnumerable<TabTMatchSystem>> GetMatchSystemsAsync() {
            Count(nameof(_fetcher.GetMatchSystemsAsync));
            var request = new DebugService.GetMatchSystems();
            request.Credentials = _credentials;
            var ret = await _fetcher.GetMatchSystemsAsync(request);
            if (ret?.GetMatchSystemsResponse?.MatchSystemEntries == null) {
                return Enumerable.Empty<TabTMatchSystem>();
            }
            var systems = new List<TabTMatchSystem>();
            foreach (var matchSystem in ret.GetMatchSystemsResponse.MatchSystemEntries) {
                if (int.TryParse(matchSystem.UniqueIndex, out var uniqueIndex)
                        && int.TryParse(matchSystem.SingleMatchCount, out var singleMatchCount)
                        && int.TryParse(matchSystem.DoubleMatchCount, out var doubleMatchCount)
                        && int.TryParse(matchSystem.PointCount, out var pointCount)
                        && int.TryParse(matchSystem.SetCount, out var setCount)
                        && int.TryParse(matchSystem.SubstituteCount, out var substituteCount)
                        && int.TryParse(matchSystem.TeamMatchCount, out var teamMatchCount)) {

                    var matchDefs = new List<TabTMatchDefinition>();
                    if (matchSystem.TeamMatchDefinitionEntries != null && matchSystem.TeamMatchDefinitionEntries.Length > 0) {
                        foreach (var def in matchSystem.TeamMatchDefinitionEntries) {
                            if (int.TryParse(def.Position, out var position)
                                    && int.TryParse(def.Type, out var type)
                                    && int.TryParse(def.HomePlayerIndex, out var homePlayerIndex)
                                    && int.TryParse(def.AwayPlayerIndex, out var awayPlayerIndex)) {
                                matchDefs.Add(new TabTMatchDefinition(position, type, homePlayerIndex, awayPlayerIndex, def.AllowSubstituteSpecified && def.AllowSubstitute));
                            } else {
                                Logger.Log(LogType.Warning, $"Could not parse one of the MatchSystem MatchDefinitions that was returned, for MatchDefinition at position '{ def.Position }'");
                            }
                        }
                    } else {
                        Logger.Log(LogType.Warning, $"No MatchDefinitions found in the MatchSystem '{ matchSystem.Name }' ({ matchSystem.UniqueIndex })");
                    }
                    systems.Add(new TabTMatchSystem(uniqueIndex, matchSystem.Name, singleMatchCount, doubleMatchCount, matchSystem.ForcedDoubleTeams, pointCount, setCount, substituteCount, teamMatchCount, matchDefs));
                } else {
                    Logger.Log(LogType.Warning, $"Could not parse one of the MatchSystem values that was returned, for MatchSystem '{ matchSystem.Name }' ({ matchSystem.UniqueIndex })");
                }
            }
            return systems;
        }
        #endregion

        #region Statistics
        private Dictionary<string, int> _statistics = new Dictionary<string, int>();
        private void Count(string wsdlCall) {
            if (_statistics.ContainsKey(wsdlCall)) {
                _statistics[wsdlCall] = _statistics[wsdlCall] + 1;
            } else {
                _statistics[wsdlCall] = 1;
            }
        }

        public IDictionary<string, int> Statistics {
            get {
                return _statistics;
            }
        }

        public bool IsAnonymous => _credentials == null;
        #endregion

        private DebugService.TabTAPI_PortTypeClient _fetcher = new DebugService.TabTAPI_PortTypeClient();
        private DebugService.CredentialsType? _credentials;
    }
}
#endif