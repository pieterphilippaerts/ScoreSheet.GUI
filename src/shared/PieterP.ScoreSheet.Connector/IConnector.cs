using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PieterP.ScoreSheet.Connector {
    public interface IConnector {
        bool IsAnonymous { get; }
        void SetDefaultCredentials(string? username, string? password);
        Task<(TabTErrorCode ErrorCode, TabTInfo? Info)> TestAsync();
        Task<IEnumerable<TabTSeason>> GetSeasonsAsync();
        Task<TabTSeason> GetActiveSeason();
        Task<(TabTErrorCode, IEnumerable<string>)> UploadAsync(string csv);
        Task<IEnumerable<TabTClub>> GetClubsAsync(TabTSeason? season);
        Task<IEnumerable<TabTTeam>> GetTeams(string clubId, TabTSeason? season);
        Task<IEnumerable<TabTMatch>> GetMatches(string clubId, TabTTeam team, TabTSeason? season);
        Task<IEnumerable<TabTMatch>> GetMatches(int divisionId, TabTSeason? season, string? weekName = null);
        Task<TabTMatch?> GetMatchDetails(string club, string matchId, TabTSeason? season);
        Task<IEnumerable<TabTMember>> GetMembers(string clubId, int category, TabTSeason? season, bool extendedInfo = false);
        Task<IEnumerable<TabTDivision>> GetDivisions(TabTDivisionRegion? level, TabTSeason? season);
        Task<IEnumerable<TabTMatchSystem>> GetMatchSystemsAsync();
        Task<IEnumerable<TabTPlayerCategory>> GetPlayerCategoriesAsync(TabTSeason? season);
        IDictionary<string, int> Statistics { get; }
    }
    public interface IConnectorFactory {
        Task<(IConnector? Connector, TabTErrorCode ErrorCode)> Create(bool allowAnonymous = false, bool showUi = true);
    }
}