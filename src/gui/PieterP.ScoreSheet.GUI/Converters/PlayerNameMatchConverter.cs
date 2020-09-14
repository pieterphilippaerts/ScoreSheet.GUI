using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.ViewModels.Score;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PieterP.ScoreSheet.GUI.Converters {
    public class PlayerNameMatchConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var list = value as List<SinglePlayerInfo>;
            if (list == null)
                return "";
            var sb = new StringBuilder();
            foreach (var spi in list) {
                var name = GetName(spi);
                if (name != null) {
                    if (sb.Length > 0)
                        sb.Append(" - ");
                    sb.Append($"{ name } ({ spi.IndividualWins.Value })");
                }
            }
            return sb.ToString();

            string? GetName(SinglePlayerInfo spi) {
                if (int.TryParse(spi.ComputerNumber.Value, out var compNum)) {
                    var member = DatabaseManager.Current.Members.FindMember(spi.ParentTeam.ClubId.Value, compNum);
                    if (member != null) {
                        return $"{member.Lastname?.ToUpper()} {member.Firstname?[0]}.";
                    }
                }
                var name = spi.Name.Value;
                if (name != null && name.Length > 0)
                    return name;
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
