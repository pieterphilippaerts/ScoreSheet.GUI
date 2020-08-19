using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.Model.Database.Entities;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.ViewModels.Services {
    public class MemberListUpdater : IDisposable {
        public MemberListUpdater() {
            var matches = DatabaseManager.Current.MatchStartInfo.GetMatchesAtDate(DateTime.Now, true, false);
            if (matches.Count() > 0)
                StartUpdate(matches);

        }
        private async void StartUpdate(IEnumerable<MatchStartInfo> matches) {
            foreach (var match in matches) {
                if (match.PlayerCategory != null) {
                    if (match.HomeClub != null) {
                        await Refresh(match.HomeClub, match.PlayerCategory.Value);
                    }
                    if (match.AwayClub != null) {
                        await Refresh(match.AwayClub, match.PlayerCategory.Value);
                    }
                }
            }
            DatabaseManager.Current.Members.Save();
        }
        private async Task Refresh(string clubId, int category) {
            if (await DatabaseManager.Current.RefreshMemberList(clubId, category)) {
                Logger.Log(LogType.Debug, Safe.Format(Strings.MemberList_AutoUpdateSucceeded, clubId));
            } else {
                Logger.Log(LogType.Warning, Safe.Format(Strings.MemberList_AutoUpdateFailed, clubId));
            }
        }

        public void Dispose() {
            //
        }
    }
}
