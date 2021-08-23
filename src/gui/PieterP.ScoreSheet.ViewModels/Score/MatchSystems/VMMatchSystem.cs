using System;
using System.Collections.Generic;
using System.Text;
using PieterP.ScoreSheet.Connector;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.Model.Database.Enums;
using PieterP.ScoreSheet.Model.Database.MatchSystems;
using PieterP.ScoreSheet.ViewModels.Score.Validations;
using PieterP.ScoreSheet.ViewModels.Templates;
using PieterP.Shared.Services;
using static PieterP.ScoreSheet.Localization.Strings;

namespace PieterP.ScoreSheet.ViewModels.Score.MatchSystems {
    public abstract class VMMatchSystem : MatchSystem{
        public abstract void Initialize(TeamInfo home, TeamInfo away, IList<MatchInfo> matches);
        public virtual IEnumerable<Validation> GetValidations(CompetitiveMatchViewModel matchVm) {
            // default validations for all match systems
            var validations = new List<Validation>() {
                new AddressValidation(),
                //new ChiefRefereeValidValidation(),
                new ClubIdValidation(MatchSystem_TheHomeTeam, vm => vm.HomeTeam.ClubId),
                new ClubIdValidation(MatchSystem_TheAwayTeam, vm => vm.AwayTeam.ClubId),
                new DateValidation(),
                new EndHourValidation(),
                new GenderValidation(),
                new LevelValidation(),
                new MatchIdValidation(),
                new MatchTypeValidation(),
                new PersonNameValidation(MatchSystem_TheChiefReferee, vm => vm.ChiefReferee.Name),
                new PersonNameValidation(MatchSystem_TheHomeCaptain, vm => vm.HomeCaptain.Name),
                new PersonNameValidation(MatchSystem_TheAwayCaptain, vm => vm.AwayCaptain.Name),
                new PersonNumberValidation(MatchSystem_TheChiefReferee, vm => vm.ChiefReferee.ComputerNumber),
                new PersonNumberValidation(MatchSystem_TheHomeCaptain, vm => vm.HomeCaptain.ComputerNumber),
                new PersonNumberValidation(MatchSystem_TheAwayCaptain, vm => vm.AwayCaptain.ComputerNumber),
                new RoomCommissionerNameValidation(),
                new RoomCommissionerNumberValidation(),
                new ScoreValidation(),
                new SeriesValidation(),
                new StartHourValidation(),
                new TeamIndexValidation(MatchSystem_TheHomeTeam, vm => vm.HomeTeam),
                new TeamIndexValidation(MatchSystem_TheAwayTeam, vm => vm.AwayTeam),
                new TeamNameValidation(MatchSystem_TheHomeTeam, vm => vm.HomeTeam.Name),
                new TeamNameValidation(MatchSystem_TheAwayTeam, vm => vm.AwayTeam.Name),
                new TeamWOValidation(MatchSystem_TheHomeTeam, vm => vm.HomeTeam),
                new TeamWOValidation(MatchSystem_TheAwayTeam, vm => vm.AwayTeam),
                new CaptainNotWOValidation(MatchSystem_TheHomeTeam, vm => vm.HomeTeam),
                new CaptainNotWOValidation(MatchSystem_TheAwayTeam, vm => vm.AwayTeam),
            };
            for (int i = 0; i < matchVm.HomeTeam.Players.Count; i++) {
                var hplayer = matchVm.HomeTeam.Players[i] as SinglePlayerInfo;
                if (hplayer != null) {
                    validations.Add(new PlayerValidation(Safe.Format(MatchSystem_HomePlayer, i + 1), vm => hplayer));
                }
            }
            for (int i = 0; i < matchVm.AwayTeam.Players.Count; i++) {
                var aplayer = matchVm.AwayTeam.Players[i] as SinglePlayerInfo;
                if (aplayer != null) {
                    validations.Add(new PlayerValidation(Safe.Format(MatchSystem_AwayPlayer, i + 1), vm => aplayer));
                }
            }
            return validations;
        }
        public virtual IOrientationInfo GenerateTemplate(CompetitiveMatchViewModel matchVm) {
            string clubId;
            if (matchVm.HomeTeam.IsBye.Value) {
                clubId = matchVm.AwayTeam.ClubId.Value;
            } else {
                clubId = matchVm.HomeTeam.ClubId.Value;
            }
            var club = DatabaseManager.Current.Clubs[clubId];
            bool isVttl;
            if (club == null) { // club not found
                var finder = ServiceLocator.Resolve<IRegionFinder>();
                isVttl = finder.IsVttl;
            } else {
                isVttl = club.Province.HasValue && club.Province.Value.IsFlemish();
            }            
            if (isVttl) {
                return new VttlScoreTemplate(matchVm);
            } else {
                return new AfttScoreTemplate(matchVm);
            }
        }
    }
}
