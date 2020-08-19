using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PieterP.ScoreSheet.Connector;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.Model.Database.Enums;

namespace PieterP.ScoreSheet.Model.Information {
    public class RegionFinder : IRegionFinder {
        public bool IsVttl {
            get {
                var dbm = DatabaseManager.Current;
                var homeClubId = dbm.Settings.HomeClubId.Value;
                if (homeClubId != null) {
                    var  club =  dbm.Clubs[homeClubId];
                    if (club != null) {
                        var province = club.Province;
                        if (province != null) {
                            switch (province) {
                                case Province.Aftt:
                                case Province.Hainaut:
                                case Province.Liege:
                                case Province.Luxemburg:
                                case Province.Namur:
                                case Province.WalloonBrabantBrussels:
                                    return false;
                            }
                        }
                    }
                }
                return true;
            }
        }
    }
}
