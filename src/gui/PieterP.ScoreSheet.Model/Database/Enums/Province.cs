using System;
using System.Collections.Generic;
using System.Text;
using PieterP.ScoreSheet.Connector;

namespace PieterP.ScoreSheet.Model.Database.Enums {
    public enum Province : int {
        FlemishBrabantBrussels = 2,
        WalloonBrabantBrussels = 3,
        Antwerp = 4,
        EastFlanders = 5,
        WestFlanders = 6,
        Limburg = 7,
        Hainaut = 8,
        Luxemburg = 9,
        Liege = 10,
        Namur = 11,
        Vttl = 12,
        Aftt = 14
    }
    public static class ProvinceExtensions {
        public static bool IsFlemish(this Province prov) {
            return prov == Province.FlemishBrabantBrussels
                || prov == Province.Antwerp
                || prov == Province.EastFlanders
                || prov == Province.WestFlanders
                || prov == Province.Limburg
                || prov == Province.Vttl;
        }
        public static bool IsWalloon(this Province prov) {
            return prov == Province.WalloonBrabantBrussels
                || prov == Province.Hainaut
                || prov == Province.Luxemburg
                || prov == Province.Liege
                || prov == Province.Namur
                || prov == Province.Aftt;
        }
        public static TabTDivisionRegion ToDivisionLevel(this Province prov) {
            switch (prov) {
                case Province.FlemishBrabantBrussels:
                    return TabTDivisionRegion.VlaamsBrabant;
                case Province.WalloonBrabantBrussels:
                    return TabTDivisionRegion.BruxellesBrabantWallon;
                case Province.Antwerp:
                    return TabTDivisionRegion.Antwerpen;
                case Province.EastFlanders:
                    return TabTDivisionRegion.OostVlaanderen;
                case Province.WestFlanders:
                    return TabTDivisionRegion.WestVlaanderen;
                case Province.Limburg:
                    return TabTDivisionRegion.Limburg;
                case Province.Hainaut:
                    return TabTDivisionRegion.Hainaut;
                case Province.Luxemburg:
                    return TabTDivisionRegion.Luxembourg;
                case Province.Liege:
                    return TabTDivisionRegion.Liege;
                case Province.Namur:
                    return TabTDivisionRegion.Namur;
                case Province.Vttl:
                    return TabTDivisionRegion.RegionalVTTL;
                case Province.Aftt:
                    return TabTDivisionRegion.RegionalIWB;
                default:
                    return TabTDivisionRegion.National;

            }
        }
    }
}
