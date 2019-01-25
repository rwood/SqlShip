//using System;
//using System.Text.RegularExpressions;
//
//namespace SqlShip.Extensions
//{
//    public static class DateTimeExtensions
//    {
//        private static readonly Regex IntDateFormat =
//            new Regex(@"(?<year>\d\d\d\d)(?<month>\d\d)(?<day>\d\d)", RegexOptions.Compiled);
//
//        public static int ToIntDate(this DateTime date)
//        {
//            return int.Parse(date.ToString("yyyyMMdd"));
//        }
//
//        public static DateTime ToDateTime(this int intDate)
//        {
//            var m = IntDateFormat.Match(intDate.ToString());
//            if (!m.Success)
//                return DateTime.MinValue;
//            var year = int.Parse(m.Groups["year"].Value);
//            var month = int.Parse(m.Groups["month"].Value);
//            var day = int.Parse(m.Groups["day"].Value);
//            return new DateTime(year, month, day);
//        }
//    }
//}