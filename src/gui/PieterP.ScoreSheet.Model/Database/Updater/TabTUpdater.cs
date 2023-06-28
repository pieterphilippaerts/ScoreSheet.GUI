using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PieterP.ScoreSheet.Connector;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model.Database.Entities;
using PieterP.ScoreSheet.Model.Database.Enums;
using PieterP.ScoreSheet.Model.Database.MatchSystems;
using PieterP.Shared;
using PieterP.Shared.Services;
using static PieterP.ScoreSheet.Localization.Errors;
using static PieterP.ScoreSheet.Localization.Strings;

namespace PieterP.ScoreSheet.Model.Database.Updater {
    public class TabTUpdater {
        private bool _hasDownloadedExtendedDivisions;

        public TabTUpdater() {
            UpdateProgress += (str, error) => Logger.Log(error ? LogType.Exception : LogType.Informational, str);
            _hasDownloadedExtendedDivisions = false;
        }

        public async Task<bool> UpdateClubs(CancellationToken cancellationToken) {
            UpdateProgress?.Invoke(TabTUpdater_BeginClubUpdate, false);

            var connectorFactory = ServiceLocator.Resolve<IConnectorFactory>();
            var connectorResult = await connectorFactory.Create(true);
            if (connectorResult.Connector == null)
                return false;
            var connector = connectorResult.Connector;
            if (connector.IsAnonymous)
                UpdateProgress?.Invoke(TabTUpdater_Warning, true);

            var newClubs = new List<Club>();
            try {
                var clubs = await connector.GetClubsAsync(await connector.GetActiveSeason());
                foreach (var club in clubs) {
                    if (cancellationToken.IsCancellationRequested) {
                        return Cancelled();
                    }
                    if (club.UniqueIndex != null && club.UniqueIndex.Length > 0) {
                        var newClub = new Club();
                        newClub.UniqueIndex = club.UniqueIndex;
                        newClub.Province = (Province)club.Region;
                        newClub.Name = club.Name;
                        newClub.LongName = club.LongName;
                        newClub.Venues = club.Venues.Select(v => v.ToString()).ToList();
                        if (newClub.Name == null || newClub.Name.Length == 0)
                            newClub.Name = newClub.LongName;
                        if (newClub.LongName == null || newClub.LongName.Length == 0)
                            newClub.LongName = newClub.Name;
                        newClubs.Add(newClub);
                        Logger.Log(LogType.Debug, Safe.Format(TabTUpdater_AddedClub, newClub.Name, newClub.UniqueIndex));
                    } else {
                        Logger.Log(LogType.Warning, TabTUpdater_NoUniqueId);
                    }
                }
            } catch (Exception e) {
                Logger.Log(e);
                return false;
            }

            if (newClubs.Count == 0) {
                Logger.Log(LogType.Warning, TabTUpdater_NoClubs);
                return false;
            } else {
                if (cancellationToken.IsCancellationRequested) {
                    return Cancelled();
                }
                DatabaseManager.Current.Clubs.Update(newClubs);
                UpdateProgress?.Invoke(TabTUpdater_ClubsUpdated, false);
                LogStats(connector.Statistics);
                return true;
            }
        }
        private bool Cancelled() {
            UpdateProgress?.Invoke(TabTUpdater_Canceled, true);
            return false;
        }
        private async Task<bool> UpdatePlayerCategories(IConnector connector, TabTSeason season) {
            var categories = await connector.GetPlayerCategoriesAsync(season);
            if (categories == null || categories.Count() == 0)
                return false;

            var data = new List<PlayerCategory>();
            foreach(var cat in categories) {
                data.Add(new PlayerCategory() {
                    UniqueIndex = cat.UniqueIndex,
                    Name = cat.Name,
                    ShortName = cat.ShortName,
                    RankingCategory = cat.RankingCategory,
                    Sex = cat.Sex,
                    MinimumAge = cat.MinimumAge,
                    MaximumAge = cat.MaximumAge,
                    IsGroup = cat.IsGroup,
                    GroupMembers = cat.GroupMembers
                });
            }
            DatabaseManager.Current.PlayerCategories.Update(data);
            return true;
        }
        public async Task<bool> UpdateMatches(Club club, CancellationToken cancellationToken) {
            // first update clubs; this is to avoid that we have an invalid list of clubs
            // (can happen at the start of a competition year when clubs are added or removed)
            await UpdateClubs(cancellationToken); // continue, even if this fails
            
            UpdateProgress?.Invoke(TabTUpdater_BeginMatchUpdate, false);

            var connectorFactory = ServiceLocator.Resolve<IConnectorFactory>();
            var connectorResult = await connectorFactory.Create(true);
            var connector = connectorResult.Connector;
            if (connector == null)
                return false;
            if (connector.IsAnonymous)
                UpdateProgress?.Invoke(TabTUpdater_Warning, true);

            bool everythingOk = true;

            // get the active season
            var season = await connector.GetActiveSeason();
            DatabaseManager.Current.Settings.CurrentSeason.Value = new Season() { Id = season.Id, Name = season.Name };
            UpdateProgress?.Invoke(Safe.Format(TabTUpdater_DownloadingSeason, season.Name), false);

            // get all the player categories for this season
            if (!await UpdatePlayerCategories(connector, season)) {
                everythingOk = false;
                UpdateProgress?.Invoke(Safe.Format(TabTUpdater_MembersListError, 0), true); // TODO
            }
            if (cancellationToken.IsCancellationRequested) {
                return Cancelled();
            }

            // get the divisions
            var divisions = new List<TabTDivision>();
            var divList = new List<TabTDivisionRegion> { TabTDivisionRegion.Super, TabTDivisionRegion.National };
            if (club.Province != null) {
                divList.Add(club.Province.Value.ToDivisionLevel());
                if (club.Province.Value.IsFlemish())
                    divList.Add(TabTDivisionRegion.RegionalVTTL);
                else if (club.Province.Value.IsWalloon())
                    divList.Add(TabTDivisionRegion.RegionalIWB);
            }
            foreach (var div in divList) {
                if (cancellationToken.IsCancellationRequested) {
                    return Cancelled();
                }
                try {
                    divisions.AddRange(await connector.GetDivisions(div, season));
                } catch (Exception e) {
                    Logger.Log(e);
                    UpdateProgress?.Invoke(Safe.Format(TabTUpdater_DivisionError,  div.ToString()), true);
                }
            }
            UpdateProgress?.Invoke(TabTUpdater_DivisionsDownloaded, false);

            if (cancellationToken.IsCancellationRequested) {
                return Cancelled();
            }
            var teams = await connector.GetTeams(club.UniqueIndex!, season);
            UpdateProgress?.Invoke(Safe.Format(TabTUpdater_TeamsDownloaded, club.UniqueIndex), false);

            // standaard alle clubs uit dezelfde provincie toevoegen
            foreach (var ce in DatabaseManager.Current.Clubs) {
                if (ce.Province == club.Province) {
                    if (cancellationToken.IsCancellationRequested) {
                        return Cancelled();
                    }
                    if (!await RefreshMemberList(connector, ce.UniqueIndex!, DatabaseManager.Current.PlayerCategories.Default, season)) {
                        everythingOk = false;
                        UpdateProgress?.Invoke(Safe.Format(TabTUpdater_MembersListError, ce.UniqueIndex), true);
                    }
                }
            }
            UpdateProgress?.Invoke(TabTUpdater_MemberListsDownloaded, false);

            var newMatchList = new List<MatchStartInfo>();
            foreach (var team in teams) {
                if (cancellationToken.IsCancellationRequested) {
                    return Cancelled();
                }
                team.Team = team.Team?.Trim(); 
                var matches = await connector.GetMatches(club.UniqueIndex!, team, season);
                UpdateProgress?.Invoke(Safe.Format(TabTUpdater_MatchDetailsDownloaded, team.Team, team.DivisionName), false);
                bool hasWarned = false;
                foreach (var match in matches) {
                    if (cancellationToken.IsCancellationRequested) {
                        return Cancelled();
                    }
                    if (!await RefreshMemberList(connector, match.HomeClub, team.DivisionCategory, season)) {
                        everythingOk = false;
                        UpdateProgress?.Invoke(Safe.Format(TabTUpdater_MembersListError, match.HomeClub), true);
                    }
                    if (cancellationToken.IsCancellationRequested) {
                        return Cancelled();
                    }
                    if (!await RefreshMemberList(connector, match.AwayClub, team.DivisionCategory, season)) {
                        everythingOk = false;
                        UpdateProgress?.Invoke(Safe.Format(TabTUpdater_MembersListError, match.AwayClub), true);
                    }
                    var m = await CreateMatch(connector, club, team, match, divisions, season);
                    if (m != null) {
                        newMatchList.Add(m);
                    } else {
                        everythingOk = false;
                        if (!hasWarned) {
                            UpdateProgress?.Invoke(Safe.Format(TabTUpdater_MatchDetailsError, team.Team), true);
                            hasWarned = true;
                        }
                    }
                }
            }
            if (cancellationToken.IsCancellationRequested) {
                return Cancelled();
            }
            DatabaseManager.Current.Members.Save(); // the entire database has been updated; save now
            DatabaseManager.Current.MatchStartInfo.Update(newMatchList);

            if (!teams.Any()) {
                UpdateProgress?.Invoke(TabTUpdater_NoTeams, true);
                everythingOk = false;
            } else {
                UpdateProgress?.Invoke(TabTUpdater_Finished, false);
                if (everythingOk)
                    UpdateProgress?.Invoke(TabTUpdater_UpdateSucceeded, false);
                else
                    UpdateProgress?.Invoke(TabTUpdater_UpdateFailed, true);
            }
            LogStats(connector.Statistics);
            return everythingOk;
        }
        private async Task<MatchStartInfo?> CreateMatch(IConnector connector, Club club, TabTTeam team, TabTMatch match, IList<TabTDivision> divisions, TabTSeason season) {
            var ms = ServiceLocator.Resolve<MatchSystemFactory>()[team.MatchType];
            if (ms == null) {
                Logger.Log(LogType.Exception, Safe.Format(TabTUpdater_MatchSystemNotFound, team.MatchType, match.MatchId, team.Team, team.DivisionId));
                return null;
            }
            var newMatch = new MatchStartInfo();
            newMatch.IsOfficial = true;
            newMatch.MatchSystemId = ms.Id;
            newMatch.AwayClub = match.AwayClub;
            newMatch.AwayTeam = match.AwayTeam.Trim();  // team names in super have a trailing space
            newMatch.HomeClub= match.HomeClub;
            newMatch.HomeTeam = match.HomeTeam.Trim();  // team names in super have a trailing space
            newMatch.MatchId = match.MatchId;
            newMatch.Series = team.DivisionName;

            if (match.VenueId != null) {
                Club? vc = null;
                if (match.VenueClub != null && match.VenueClub.Length > 0) {
                    vc = DatabaseManager.Current.Clubs[match.VenueClub];
                }
                if (vc == null)
                    vc = club;
                if (club.Venues != null && match.VenueId > 0 && match.VenueId <= club.Venues.Count) {
                    newMatch.Venue = club.Venues[match.VenueId.Value - 1];
                }
            } 
            if (newMatch.Venue == null) {
                newMatch.Venue = club.Venues.FirstOrDefault();
            }
            
            var wn = match.WeekName;
            if (match.DateSpecified) {
                newMatch.Date = match.Date.ToFormattedDate();
                newMatch.WeekStart = match.Date.FindStartOfWeek();
            } else {
                if (match.HomeClub != "-" && match.AwayClub != "-") {
                    UpdateProgress?.Invoke(Safe.Format(TabTUpdater_InvalidMatchDate, team.Team), true);
                }
                newMatch.WeekStart = await GuessWeek(connector, team.DivisionId, season, match.WeekName);
                if (newMatch.WeekStart == null) {
                    UpdateProgress?.Invoke(Safe.Format(TabTUpdater_NoWeek, team.Team, team.DivisionName), true);
                    UpdateProgress?.Invoke(Safe.Format(TabTUpdater_InvalidDateInfo, match.HomeTeam, match.AwayTeam, match.MatchId), true);
                } else if (match.HomeClub == "-" || match.AwayClub == "-") {
                    UpdateProgress?.Invoke(Safe.Format(TabTUpdater_MatchPlanned, match.HomeTeam, match.AwayTeam, match.MatchId, newMatch.WeekStart.Value.ToString("D")), false);
                }
            }
            if (match.TimeSpecified)
                newMatch.StartHour = match.Time.ToFormattedTime();
            var div = divisions.Where(d => d.Id == team.DivisionId).FirstOrDefault();
            if (div == null) {
                if (await DownloadAllDivisions(connector, divisions, season))
                    div = divisions.Where(d => d.Id == team.DivisionId).FirstOrDefault(); // try again, onw with all divisions downloaded
            }

            if (div != null) {
                newMatch.Region = (Region)div.Region;
                newMatch.Level = newMatch.Region.Value.ToLevel();
                newMatch.PlayerCategory = div.PlayerCategory;

                // parse series; we can't get this directly from the webservice
                int startFrom = -1;
                if (div.Name.StartsWith("Afdeling ") || div.Name.StartsWith("Division ")) {
                    startFrom = 9;
                } else if (div.Name.StartsWith("Reeks ") || div.Name.StartsWith("Série ") || div.Name.StartsWith("Serie ")) {
                    startFrom = 6;
                }
                if (startFrom >= 0 && newMatch.Level != Level.Super) {
                    int seriesEnd = div.Name.IndexOf(' ', startFrom);
                    if (seriesEnd != -1) {
                        newMatch.Series = div.Name.Substring(startFrom, seriesEnd - startFrom);
                    }
                } else {
                    newMatch.Series = string.Empty;
                }

                // parse men/women, interclub/super/youth/cup/veterans
                // we can't get this information directly from the webservice
                GuessTypeInfo(newMatch, div.PlayerCategory, div.Name);
            } else {
                UpdateProgress?.Invoke(Safe.Format(TabTUpdater_CannotFindDivision, match.MatchId), true);
                Logger.Log(LogType.Exception, Safe.Format(TabTUpdater_CannotFindDivisionWMatchWTeam,  team.DivisionId, match.MatchId,  team.Team));
            }            

            return newMatch;
        }
        private async Task<bool> DownloadAllDivisions(IConnector connector, IList<TabTDivision> divisions, TabTSeason season) {
            if (_hasDownloadedExtendedDivisions)
                return false;
            try {
                divisions.AddRange(await connector.GetDivisions(null, season));
            } catch (Exception e) {
                Logger.Log(e);
                UpdateProgress?.Invoke(TabTUpdater_AdditionalDivisionError, true);
                return false;
            }
            UpdateProgress?.Invoke(TabTUpdater_AdditionalDivisionsDownloaded, false);
            _hasDownloadedExtendedDivisions = true;
            return true;
        }
        private async Task<MatchStartInfo?> CreateMatch(IConnector connector, TabTMatch match, Division division, TabTSeason season) {
            var ms = ServiceLocator.Resolve<MatchSystemFactory>()[division.MatchSystemId];
            if (ms == null) {
                //Logger.Log(LogType.Exception, Safe.Format(TabTUpdater_MatchSystemNotFound, team.MatchType, match.MatchId, team.Team, team.DivisionId));
                return null;
            }
            var newMatch = new MatchStartInfo();
            newMatch.IsOfficial = true;
            newMatch.MatchSystemId = ms.Id;
            newMatch.AwayClub = match.AwayClub;
            newMatch.AwayTeam = match.AwayTeam.Trim();  // team names in super have a trailing space
            newMatch.HomeClub = match.HomeClub;
            newMatch.HomeTeam = match.HomeTeam.Trim();  // team names in super have a trailing space
            newMatch.MatchId = match.MatchId;
            newMatch.Series = division.Name;

            var club = DatabaseManager.Current.Clubs[match.HomeClub];
            if (match.VenueId != null) {
                Club? vc = null;
                if (match.VenueClub != null && match.VenueClub.Length > 0) {
                    vc = DatabaseManager.Current.Clubs[match.VenueClub];
                }
                if (vc == null)
                    vc = club;
                if (club?.Venues != null && match.VenueId > 0 && match.VenueId <= club.Venues.Count) {
                    newMatch.Venue = club.Venues[match.VenueId.Value - 1];
                }
            }
            if (newMatch.Venue == null) {
                newMatch.Venue = club?.Venues.FirstOrDefault();
            }

            var wn = match.WeekName;
            if (match.DateSpecified) {
                newMatch.Date = match.Date.ToFormattedDate();
                newMatch.WeekStart = match.Date.FindStartOfWeek();
            } else {
                if (match.HomeClub != "-" && match.AwayClub != "-") {
                    //UpdateProgress?.Invoke(Safe.Format(TabTUpdater_InvalidMatchDate, team.Team), true);
                }
                newMatch.WeekStart = await GuessWeek(connector, division.Id, season, match.WeekName);
                if (newMatch.WeekStart == null) {
                    //UpdateProgress?.Invoke(Safe.Format(TabTUpdater_NoWeek, team.Team, division.Name), true);
                    //UpdateProgress?.Invoke(Safe.Format(TabTUpdater_InvalidDateInfo, match.HomeTeam, match.AwayTeam), true);
                } else if (match.HomeClub == "-" || match.AwayClub == "-") {
                    //UpdateProgress?.Invoke(Safe.Format(TabTUpdater_MatchPlanned, match.HomeTeam, match.AwayTeam, newMatch.WeekStart.Value.ToString("D")), false);
                }
            }
            if (match.TimeSpecified)
                newMatch.StartHour = match.Time.ToFormattedTime();

            newMatch.Region = division.Region;
            newMatch.Level = newMatch.Region.Value.ToLevel();
            newMatch.PlayerCategory = division.PlayerCategory;

            // parse series; we can't get this directly from the webservice
            int startFrom = -1;
            if (division.Name.StartsWith("Afdeling ") || division.Name.StartsWith("Division ")) {
                startFrom = 9;
            } else if (division.Name.StartsWith("Reeks ") || division.Name.StartsWith("Série ") || division.Name.StartsWith("Serie ")) {
                startFrom = 6;
            }
            if (startFrom >= 0 && newMatch.Level != Level.Super) {
                int seriesEnd = division.Name.IndexOf(' ', startFrom);
                if (seriesEnd != -1) {
                    newMatch.Series = division.Name.Substring(startFrom, seriesEnd - startFrom);
                }
            } else {
                newMatch.Series = string.Empty;
            }

            // parse men/women, interclub/super/youth/cup/veterans
            // we can't get this information directly from the webservice
            GuessTypeInfo(newMatch, division.PlayerCategory, division.Name);

            return newMatch;
        }

        private void GuessTypeInfo(PlayerCategory playerCategory, out bool veterans, out bool youth, out bool women, out bool men) {
            youth = false;
            if (playerCategory.MaximumAge != null && playerCategory.MaximumAge <= 21)
                youth = true;
            veterans = false;
            if (playerCategory.MinimumAge != null && playerCategory.MinimumAge >= 39)
                veterans = true;
            women = false;
            men = false;
            if (playerCategory.Sex == "F")
                women = true;
            else
                men = true;
        }
        private void GuessGroupInfo(PlayerCategory playerCategory, out bool veterans, out bool youth, out bool women, out bool men) {
            var groups = playerCategory.GroupMembers?.Split(',');
            if (groups != null) {
                veterans = true;
                youth = true;
                women = true;
                men = true;
                // make sure all subcategories agree on which type of category this is
                foreach (var groupStr in groups) {
                    if (int.TryParse(groupStr, out int group)) {
                        var cat = DatabaseManager.Current.PlayerCategories[group];
                        if (cat != null) {
                            bool v, y, m, w;
                            GuessTypeInfo(cat, out v, out y, out w, out m);
                            veterans &= v;
                            youth &= y;
                            women &= w;
                            men &= m;
                        }
                    }
                }
            } else { // invalid data
                veterans = false;
                youth = false;
                women = false;
                men = false;
            }
        }
        private void GuessTypeInfo(MatchStartInfo match, int category, string? divisionTitle) {
            divisionTitle = divisionTitle?.ToLower();
            bool super = match.Level == Level.Super;
            bool cup = divisionTitle?.Contains("beker") ?? false;
            bool youth = false, veterans = false, men = false, women = false;

            var playerCategory = DatabaseManager.Current.PlayerCategories[category];
            if (playerCategory != null) {
                if (playerCategory.IsGroup) {
                    GuessGroupInfo(playerCategory, out veterans, out youth, out women, out men);
                } else {
                    GuessTypeInfo(playerCategory, out veterans, out youth, out women, out men);
                }
            }
            //switch (category) {
            //    // hack omdat er nog wat fouten in de online database zitten
            //    // (geen MaximumAge en MinimumAge ingevuld bij bepaalde categorieen)
            //    case 3:
            //    case 4:
            //    case 41:
            //    case 42:
            //    case 43:
            //    case 44:
            //        veterans = (category == 3 || category == 4);
            //        youth = (category == 41 || category == 42);
            //        women = (category == 44 || category == 42 || category == 4);
            //        men = !women;
            //        break;

            //    default:
            //}
            bool interclub = !super && !cup && !youth && !veterans;
            match.Men = men;
            match.Women = women;
            match.Interclub = interclub;
            match.Super = super;
            match.Cup = cup;
            match.Youth = youth;
            match.Veterans = veterans;
        }

        private async Task<DateTime?> GuessWeek(IConnector connector, int divisionId, TabTSeason season, string weekName) {
            var matches = await connector.GetMatches(divisionId, season, weekName);
            var dates = new Dictionary<DateTime, int>();
            // count which week starts are most common
            foreach (var m in matches) {
                if (m.DateSpecified) {
                    var d = m.Date.FindStartOfWeek();
                    if (dates.TryGetValue(d, out int value)) {
                        dates[d] = value + 1;
                    } else {
                        dates[d] = 1;
                    }
                }
            }
            var keys = dates.Keys.OrderByDescending(k => dates[k]).ToList();
            if (keys.Count() > 0) { // make sure there are (non-bye) matches in this week
                var max = dates[keys[0]];
                var maxDates = keys.Where(k => dates[k] == max); // filter out all the dates that have the maximum number of matchcounts
                if (maxDates.Count() == 1) { // if we have only one date that sticks out as the most likely, use this one
                    return maxDates.First();
                } // else: there are multiple dates that are equally likely; in this case, return null
            }
            return null;
        }
        public async Task<bool> RefreshMemberList(string clubId, int category) {
            var connectorFactory = ServiceLocator.Resolve<IConnectorFactory>();
            var connectorResult = await connectorFactory.Create(true, false);
            var connector = connectorResult.Connector;
            if (connector == null)
                return false;
            return await RefreshMemberList(connector, clubId, category, await connector.GetActiveSeason());
        }
        private async Task<bool> RefreshMemberList(IConnector connector, string clubId, int category, TabTSeason season) {
            var ml = DatabaseManager.Current.Members[clubId, category];
            if (clubId == "-" || ml != null && ml.LastUpdated != null && (DateTime.Now.Date - ml.LastUpdated.Value.Date) < UpdateInterval) {
                // do not download the same member list multiple times per day
                return true;
            }

            try {
                var members = await connector.GetMembers(clubId, category, season);
                var list = new MemberList();
                list.ClubId = clubId;
                list.Category = category;
                list.LastUpdated = DateTime.Now;
                list.Entries = new List<Member>();
                members = FixMembers(members);
                foreach (var m in members) {
                    var newMember = new Member();
                    newMember.ComputerNumber = m.VttlIndex;
                    newMember.Firstname = m.Firstname;
                    newMember.Lastname = m.Lastname;
                    newMember.Position = m.Position;
                    newMember.RankIndex = m.RankIndex;
                    newMember.Ranking = m.Ranking;
                    newMember.Status = ToPlayerStatus(m.Status);
                    list.Entries.Add(newMember);
                }
                DatabaseManager.Current.Members.Update(list, false);
            } catch (Exception e) {
                Logger.Log(e);
                return false;
            }
            var club = DatabaseManager.Current.Clubs[clubId];
            string clubName = clubId;
            if (club != null) {
                clubName = $"{ club.Name } ({ clubName })";
            }
            UpdateProgress?.Invoke(Safe.Format(TabTUpdater_MemberListDownloaded, clubName), false);
            return true;

            static IEnumerable<TabTMember> FixMembers(IEnumerable<TabTMember> members) {
                if (members == null)
                    return Enumerable.Empty<TabTMember>();

                var activeMembers = members.Where(c => c.Status == "A" /* actief */
                || c.Status == "V"  /* recreant reserve */
                || c.Status == "S"  /* super */
                || c.Status == "T"  /* dubbele aansluiting (super) */
                || c.Status == "O"  /* half seizoen */
                || c.Status == "P" /* vrijetijdsspeler */
                || c.Status == "D" /* dubbele aansluiting (super dames) */).ToList();
                if (activeMembers.Count == 0) {
                    return members; // weird
                } else {
                    for (int i = 0; i < activeMembers.Count; i++) {
                        activeMembers[i].Position = i + 1; // fix player positions if we've filtered out non-active players
                    }
                }
                return activeMembers;
            }

            static PlayerStatus ToPlayerStatus(string status) {
                return status switch
                {
                    "V" => PlayerStatus.RecreantReserve,
                    "S" => PlayerStatus.Super,
                    "T" => PlayerStatus.DoubleAffiliationSuper,
                    "O" => PlayerStatus.MidSeason,
                    "P" => PlayerStatus.FreeTime,
                    _ => PlayerStatus.Active,
                };
            }
        }

        public async Task<IList<Division>?> GetDivisions(Region level) {
            var connectorFactory = ServiceLocator.Resolve<IConnectorFactory>();
            var connectorResult = await connectorFactory.Create(true);
            var connector = connectorResult.Connector;
            if (connector == null)
                return null;

            var divisionList = new List<Division>();
            try {                
                var divisions = await connector.GetDivisions((TabTDivisionRegion)level, await connector.GetActiveSeason());
                foreach (var division in divisions) {
                    var d = new Division();
                    d.Id = division.Id;
                    d.Name = division.Name;
                    d.MatchSystemId = division.MatchSystemId;
                    d.PlayerCategory = division.PlayerCategory;
                    d.Region = (Region)division.Region;
                    divisionList.Add(d);
                }
            } catch (Exception e) {
                Logger.Log(e);
                return null;
            }

            if (divisionList.Count == 0) {
                return null;
            } else {
                LogStats(connector.Statistics);
                return divisionList;
            }
        }
        public async Task<IList<MatchStartInfo>?> GetDivisionMatches(Division division) {
            var connectorFactory = ServiceLocator.Resolve<IConnectorFactory>();
            var connectorResult = await connectorFactory.Create(true);
            var connector = connectorResult.Connector;
            if (connector == null)
                return null;

            var matchList = new List<MatchStartInfo>();
            try {
                var season = await connector.GetActiveSeason();
                var matches = await connector.GetMatches(division.Id, season);
                foreach (var match in matches) {
                    var club = DatabaseManager.Current.Clubs[match.HomeClub];
                    var m = await CreateMatch(connector, match, division, season);
                    if (m != null)
                        matchList.Add(m);
                }
            } catch (Exception e) {
                Logger.Log(e);
                return null;
            }

            if (matchList.Count == 0) {
                return null;
            } else {
                LogStats(connector.Statistics);
                return matchList;
            }
        }

        private void LogStats(IDictionary<string, int> stats) {
            var output = string.Join(",", stats.Keys.Select(k => $"{ k } ({ stats[k] }x)"));
            Logger.Log(LogType.Debug, Safe.Format(TabTUpdater_Stats, output));
        }

        public event Action<string, bool> UpdateProgress;

        private static readonly TimeSpan UpdateInterval = new TimeSpan(12, 0, 0); // do not update member lists that are younger than 12 hours
    }
}