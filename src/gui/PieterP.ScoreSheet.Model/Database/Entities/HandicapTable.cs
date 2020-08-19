using System;
using System.Collections.Generic;
using System.Text;
using PieterP.ScoreSheet.Localization;

namespace PieterP.ScoreSheet.Model.Database.Entities {
    public abstract class HandicapTable {
        public HandicapTable(string name, int id) {
            this.Name = name;
            this.Id = id;
        }

        public int Id { get; private set; }
        public string Name { get; private set; }

        public abstract int Calculate(int rank1, int rank2);
        public virtual int Calculate(string rank1, string rank2) {
            return Calculate(RankToIndex(rank1), RankToIndex(rank2));
        }

        public static int RankToIndex(string rank) {
            rank = rank.ToUpper();
            if (rank == "NG")
                rank = "NC";
            if (rank.StartsWith("A"))
                rank = "A";
            for (int i = 0; i < RankList.Length; i++) {
                if (RankList[i] == rank)
                    return i;
            }
            return -1;
        }
        public static string IndexToRank(int index) {
            return RankList[index];
        }
        protected static string[] RankList = new string[] { "A", "B0", "B2", "B4", "B6", "C0", "C2", "C4", "C6", "D0", "D2", "D4", "D6", "E0", "E2", "E4", "E6", "NC" };

        public static HandicapTable Men {
            get {
                return new OfficialMenHandicapTable();
            }
        }
        public static HandicapTable Women {
            get {
                return new OfficialWomenHandicapTable();
            }
        }
    }
    public abstract class OfficialHandicapTable : HandicapTable {
        public OfficialHandicapTable(string name, int id, int[] list) : base(name, id) {
            _list = list;
        }
        public override int Calculate(int thuis, int uit) {
            if (thuis == uit)
                return 0;
            if (thuis == -1 || uit == -1)
                return 0;
            int ret = (uit - thuis);
            if (ret < 0) {
                if (-ret >= _list.Length)
                    return _list[_list.Length - 1];
                else
                    return _list[-ret];
            } else {
                if (ret >= _list.Length)
                    return -_list[_list.Length - 1];
                else
                    return -_list[ret];
            }
        }
        private int[] _list;
    }
    public class OfficialMenHandicapTable : OfficialHandicapTable {
        public OfficialMenHandicapTable()
            : base(Strings.Handicap_Men, 1, HandicapIncrementsMen) {
        }
        private static int[] HandicapIncrementsMen = new int[] { 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9 };
    }
    public class OfficialWomenHandicapTable : OfficialHandicapTable {
        public OfficialWomenHandicapTable()
            : base(Strings.Handicap_Women, 2, HandicapIncrementWomen) {
        }
        private static int[] HandicapIncrementWomen = new int[] { 0, 1, 2, 2, 3, 4, 4, 5, 6, 6, 7, 8, 8, 9 };
    }
}
