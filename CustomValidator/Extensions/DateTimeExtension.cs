using System;
using System.Data.SqlTypes;

namespace CustomValidator.Extensions
{
    /// <summary>
    /// DateTime extensions.
    /// </summary>
    public static class DateTimeExtension
    {
        /// <summary>
        /// UTC date format string.
        /// </summary>
        public static readonly string UtcDateFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'";

        /// <summary>
        /// Min value for Sql datetime to save in sql db.
        /// </summary>
        public static readonly DateTime SqlDateTimeMinUtc = SqlDateTime.MinValue.Value.AsUtcKind();

        /// <summary>
        /// Gets the UTC datetime format for the date.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string ToUtcFormatString(this DateTime date)
        {
            return date.ToUniversalTime().ToString(UtcDateFormat);
        }

        /// <summary>
        /// Gets the min value for Sql datetime.
        /// </summary>
        public static DateTime ToSqlDateTimeMinUtc(this DateTime date)
        {
            return SqlDateTimeMinUtc;
        }

        /// <summary>
        /// Specifies datetime's kind as UTC.
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        /// <remarks>
        /// Date read from db or parsed from string has its Kind as Unspecified.
        /// Specifying its kind as UTC is needed if date is expected to be UTC.
        /// ToUniversalTime() assumes that the kind is local while converting it and is undesirable.
        /// </remarks>
        public static DateTime AsUtcKind(this DateTime datetime)
        {
            return DateTime.SpecifyKind(datetime, DateTimeKind.Utc);
        }

        #region Métodos
        /// <summary>
        /// Gets the begin of month.
        /// </summary>
        /// <returns>The begin of month.</returns>
        /// <param name="value">The DateTime it self.</param>
        public static DateTime GetBeginOfMonth(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, 1, 0, 0, 0, 0);
        }

        /// <summary>
        /// Gets the end of month.
        /// </summary>
        /// <returns>The end of month.</returns>
        /// <param name="value">The DateTime it self.</param>
        public static DateTime GetEndOfMonth(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, DateTime.DaysInMonth(value.Year, value.Month), 23, 59, 59, 999);
        }

        /// <summary>
        /// Gets the begin of day.
        /// </summary>
        /// <returns>The begin of day.</returns>
        /// <param name="value">The DateTime it self.</param>
        public static DateTime GetBeginOfDay(this DateTime value)
        {
            return value.Date;
        }

        /// <summary>
        /// Gets the end of day.
        /// </summary>
        /// <returns>The end of day.</returns>
        /// <param name="value">The DateTime it self.</param>
        public static DateTime GetEndOfDay(this DateTime value)
        {
            return value.Date.AddDays(1).AddTicks(-1);
        }
        #endregion
    }
}
