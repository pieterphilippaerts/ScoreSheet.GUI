using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace PieterP.ScoreSheet.Model {
    public static class ExtensionMethods {
        public static void AddRange<T>(this IList<T> list, IEnumerable<T>? items) {
            if (items == null || list == null)
                return;
            foreach (var i in items) {
                list.Add(i);
            }
        }
        public static DateTime? ToDate(this string? input) {
            if (input == null || input.Length == 0)
                return null;
            if (DateTime.TryParseExact(input, new string[] { "dd/MM/yyyy", "d/MM/yyyy", "dd/M/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-MM-yyyy", "dd-M-yyyy", "d-M-yyyy", "dd/MM/yy", "dd-MM-yy" }, new CultureInfo("nl-BE"), DateTimeStyles.None, out var result)) {
                return result;
            }
            if (DateTime.TryParse(input, new CultureInfo("nl-BE"), DateTimeStyles.None, out var second)) {
                return second;
            }
            return null;
        }
        public static string ToFormattedDate(this DateTime date) {
            return date.ToString("dd/MM/yyyy");
        }
        public static string ToFormattedTime(this DateTime date) {
            return date.ToString("HH:mm");
        }
        public static DateTime? ToTime(this string? input) {
            if (input == null || input.Length == 0)
                return null;
            DateTime res;
            if (DateTime.TryParseExact(input, new string[] { "HH:mm", "H:mm", "HHumm", "Humm" }, new CultureInfo("nl-BE"), DateTimeStyles.None, out res))
                return res;
            return null;
        }

        public static DateTime FindStartOfWeek(this DateTime date) {
            int days = (int)date.DayOfWeek - 1;
            if (days == -1) // sunday
                days = 6;
            return date.AddDays(-days).Date;
        }
        public static bool IsNumber(this string? number, bool allowNegative = false) {
            if (number != null && number.Length > 0 && int.TryParse(number, out var n)) {
                return n >= 0 || allowNegative;
            }
            return false;
        }
        public static string? SanitizeForFilename(this string? matchId) {
            if (matchId == null)
                return null;
            char[] invalid = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder();
            foreach (char c in matchId) {
                bool ok = true;
                foreach (char i in invalid) {
                    if (c == i) {
                        ok = false;
                        break;
                    }
                }
                if (ok) {
                    sb.Append(c);
                } else {
                    sb.Append('-');
                }
            }
            if (sb.Length == 0)
                return null;
            return sb.ToString();
        }

    }
}
