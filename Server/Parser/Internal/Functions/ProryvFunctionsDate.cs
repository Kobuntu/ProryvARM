using System;

namespace Proryv.Servers.Calculation.Parser.Internal.Functions
{
    public class ProryvFunctionsDate
    {
        #region DateDiff
        /// <summary>
        /// Returns a number of time intervals between two specified dates.
        /// </summary>
        public static TimeSpan DateDiff(DateTime date1, DateTime date2)
        {
            return date1.Subtract(date2);
        }

        /// <summary>
        /// Returns a number of time intervals between two specified dates.
        /// </summary>
        public static TimeSpan? DateDiff(DateTime? date1, DateTime? date2)
        {
            if (date1.HasValue && date2.HasValue)
                return date1.Value.Subtract(date2.Value);
            else
                return null;
        }
        #endregion

        #region Year
        /// <summary>
        /// Returns the year from a date and returns it as a integer value.
        /// </summary>
        public static Int64 Year(DateTime date)
        {
            return date.Year;
        }

        /// <summary>
        /// Returns the year from a date and returns it as a integer value.
        /// </summary>
        public static Int64 Year(DateTime? date)
        {
            if (date.HasValue)
                return date.Value.Year;
            else
                return 0;
        }
        #endregion

        #region Month
        /// <summary>
        /// Returns the month from a date and returns it as a integer value.
        /// </summary>
        public static Int64 Month(DateTime date)
        {
            return date.Month;
        }

        /// <summary>
        /// Returns the month from a date and returns it as a integer value.
        /// </summary>
        public static Int64 Month(DateTime? date)
        {
            if (date.HasValue)
                return date.Value.Month;
            else
                return 0;
        }
        #endregion

        #region Hour
        /// <summary>
        /// Returns the hour portion from a date and returns it as a integer value.
        /// </summary>
        public static Int64 Hour(DateTime date)
        {
            return date.Hour;
        }

        /// <summary>
        /// Returns the hour portion from a date and returns it as a integer value.
        /// </summary>
        public static Int64 Hour(DateTime? date)
        {
            if (date.HasValue)
                return date.Value.Hour;
            else
                return 0;
        }
        #endregion

        #region Minute
        /// <summary>
        /// Returns the minutes portion from a date and returns it as a integer value.
        /// </summary>
        public static Int64 Minute(DateTime date)
        {
            return date.Minute;
        }

        /// <summary>
        /// Returns the minutes portion from a date and returns it as a integer value.
        /// </summary>
        public static Int64 Minute(DateTime? date)
        {
            if (date.HasValue)
                return date.Value.Minute;
            else
                return 0;
        }
        #endregion

        #region Second
        /// <summary>
        /// Returns the seconds portion from a date and returns it as a integer value.
        /// </summary>
        public static Int64 Second(DateTime date)
        {
            return date.Second;
        }

        /// <summary>
        /// Returns the seconds portion from a date and returns it as a integer value.
        /// </summary>
        public static Int64 Second(DateTime? date)
        {
            if (date.HasValue)
                return date.Value.Second;
            else
                return date.Value.Second;
        }
        #endregion

        #region Day
        /// <summary>
        /// Returns the day from a date and returns it as a integer value.
        /// </summary>
        public static Int64 Day(DateTime date)
        {
            return date.Day;
        }

        /// <summary>
        /// Returns the day from a date and returns it as a integer value.
        /// </summary>
        public static Int64 Day(DateTime? date)
        {
            if (date.HasValue)
                return date.Value.Day;
            else
                return 0;
        }
        #endregion

        #region DayOfWeek
        /// <summary>
        /// Returns the day of the week.
        /// </summary>
        public static string DayOfWeek(DateTime date)
        {
            return Func.DayOfWeekToStr.DayOfWeek(date);
        }

        /// <summary>
        /// Returns the day of the week.
        /// </summary>
        public static string DayOfWeek(DateTime? date)
        {
            if (date.HasValue)
                return Func.DayOfWeekToStr.DayOfWeek(date.Value);
            else
                return string.Empty;
        }

        /// <summary>
        /// Returns the day of the week.
        /// </summary>
        public static string DayOfWeek(DateTime date, bool localized)
        {
            return Func.DayOfWeekToStr.DayOfWeek(date, localized);
        }

        /// <summary>
        /// Returns the day of the week.
        /// </summary>
        public static string DayOfWeek(DateTime? date, bool localized)
        {
            if (date.HasValue)
                return Func.DayOfWeekToStr.DayOfWeek(date.Value, localized);
            else
                return string.Empty;
        }

        /// <summary>
        /// Returns the day of the week.
        /// </summary>
        public static string DayOfWeek(DateTime date, string culture)
        {
            return Func.DayOfWeekToStr.DayOfWeek(date, culture);
        }

        /// <summary>
        /// Returns the day of the week.
        /// </summary>
        public static string DayOfWeek(DateTime? date, string culture)
        {
            if (date.HasValue)
                return Func.DayOfWeekToStr.DayOfWeek(date.Value, culture);
            else
                return string.Empty;
        }

        /// <summary>
        /// Returns the day of the week.
        /// </summary>
        public static string DayOfWeek(DateTime date, string culture, bool upperCase)
        {
            return Func.DayOfWeekToStr.DayOfWeek(date, culture, upperCase);
        }

        /// <summary>
        /// Returns the day of the week.
        /// </summary>
        public static string DayOfWeek(DateTime? date, string culture, bool upperCase)
        {
            if (date.HasValue)
                return Func.DayOfWeekToStr.DayOfWeek(date.Value, culture, upperCase);
            else
                return string.Empty;
        }
        #endregion

        #region MonthName
        /// <summary>
        /// Returns the name of the month.
        /// </summary>
        public static string MonthName(DateTime date)
        {
            return Func.MonthToStr.MonthName(date);
        }

        /// <summary>
        /// Returns the name of the month.
        /// </summary>
        public static string MonthName(DateTime? date)
        {
            if (date.HasValue)
                return Func.MonthToStr.MonthName(date.Value);
            else
                return string.Empty;
        }

        /// <summary>
        /// Returns the name of the month.
        /// </summary>
        public static string MonthName(DateTime date, bool localized)
        {
            return Func.MonthToStr.MonthName(date, localized);
        }

        /// <summary>
        /// Returns the name of the month.
        /// </summary>
        public static string MonthName(DateTime? date, bool localized)
        {
            if (date.HasValue)
                return Func.MonthToStr.MonthName(date.Value, localized);
            else
                return string.Empty;
        }

        /// <summary>
        /// Returns the name of the month.
        /// </summary>
        public static string MonthName(DateTime date, string culture)
        {
            return Func.MonthToStr.MonthName(date, culture);
        }

        /// <summary>
        /// Returns the name of the month.
        /// </summary>
        public static string MonthName(DateTime? date, string culture)
        {
            if (date.HasValue)
                return Func.MonthToStr.MonthName(date.Value, culture);
            else
                return string.Empty;
        }

        /// <summary>
        /// Returns the name of the month.
        /// </summary>
        public static string MonthName(DateTime date, string culture, bool upperCase)
        {
            return Func.MonthToStr.MonthName(date, culture, upperCase);
        }

        /// <summary>
        /// Returns the name of the month.
        /// </summary>
        public static string MonthName(DateTime? date, string culture, bool upperCase)
        {
            if (date.HasValue)
                return Func.MonthToStr.MonthName(date.Value, culture, upperCase);
            else
                return string.Empty;
        }
        #endregion

        #region DayOfYear
        /// <summary>
        /// Returns the day of the year.
        /// </summary>
        public static long DayOfYear(DateTime date)
        {
            return (long)date.DayOfYear;
        }

        /// <summary>
        /// Returns the day of the year.
        /// </summary>
        public static long DayOfYear(DateTime? date)
        {
            if (date.HasValue)
                return (long)date.Value.DayOfYear;
            else
                return 0;
        }
        #endregion

        #region DateSerial
        /// <summary>
        /// Returns the DateTime value for the specified year, month, and day.
        /// </summary>
        public static DateTime DateSerial(long year, long month, long day)
        {
            return new DateTime((int)year, (int)month, (int)day);
        }
        #endregion

        #region TimeSerial
        /// <summary>
        /// Returns the TimeValue value for a specified number of hours, minutes, and seconds.
        /// </summary>
        public static TimeSpan TimeSerial(long hours, long minutes, long seconds)
        {
            return new TimeSpan((int)hours, (int)minutes, (int)seconds);
        }
        #endregion

        #region DaysInMonth
        /// <summary>
        /// Returns the number of days in the specified month and year.
        /// </summary>
        public static long DaysInMonth(long year, long month)
        {
            return (long)DateTime.DaysInMonth((int)year, (int)month);
        }

        /// <summary>
        /// Returns the number of days in the specified month and year.
        /// </summary>
        public static long DaysInMonth(DateTime date)
        {
            return (long)DateTime.DaysInMonth(date.Year, date.Month);
        }

        /// <summary>
        /// Returns the number of days in the specified month and year.
        /// </summary>
        public static long DaysInMonth(DateTime? date)
        {
            if (date.HasValue)
                return (long)DateTime.DaysInMonth(date.Value.Year, date.Value.Month);
            else
                return 0;
        }
        #endregion

        #region DaysInYear
        /// <summary>
        /// Returns the number of days in the specified year.
        /// </summary>
        public static long DaysInYear(long year)
        {
            return (long)(DateTime.IsLeapYear((int)year) ? 366 : 365);
        }

        /// <summary>
        /// Returns the number of days in the specified year.
        /// </summary>
        public static long DaysInYear(DateTime date)
        {
            return (long)(DateTime.IsLeapYear(date.Year) ? 366 : 365);
        }

        /// <summary>
        /// Returns the number of days in the specified year.
        /// </summary>
        public static long DaysInYear(DateTime? date)
        {
            if (date.HasValue)
                return (long)(DateTime.IsLeapYear(date.Value.Year) ? 366 : 365);
            else
                return 0;
        }
        #endregion
               
    }
}
