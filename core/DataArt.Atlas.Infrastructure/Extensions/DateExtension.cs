#region License
// =================================================================================================
// Copyright 2018 DataArt, Inc.
// -------------------------------------------------------------------------------------------------
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this work except in compliance with the License.
// You may obtain a copy of the License in the LICENSE file, or at:
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// =================================================================================================
#endregion
using System;
using System.Globalization;
using DataArt.Atlas.Infrastructure.Exceptions;

namespace DataArt.Atlas.Infrastructure.Extensions
{
    public static class DateExtension
    {
        public static readonly string DateFormatErrorMessage = $"Invalid date, date format should be '{DateFormat}'";
        private const string DateFormat = "yyyy-MM-dd";

        public static string ToIso8601(this DateTimeOffset dto)
        {
            var format = dto.Offset == TimeSpan.Zero
                ? "yyyy-MM-ddTHH:mm:ss.fffffffZ"
                : "yyyy-MM-ddTHH:mm:ss.fffffffzzz";

            return dto.ToString(format, CultureInfo.InvariantCulture);
        }

        public static DateTimeOffset ToBritishTime(this DateTimeOffset date)
        {
            return TimeZoneInfo.ConvertTime(date, TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"));
        }

        public static string ToDateString(this DateTime date)
        {
            return date.ToString(DateFormat);
        }

        public static string ToDateString(this DateTime? date)
        {
            return date.HasValue ? ToDateString(date.Value) : null;
        }

        public static string ToTimeString(this TimeSpan time)
        {
            return time.ToString(@"hh\:mm");
        }

        public static DateTime ToBritishDate(this DateTimeOffset date)
        {
            return date.ToBritishTime().Date;
        }

        public static bool TryParseDate(string date, out DateTime result)
        {
            return DateTime.TryParseExact(date, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
        }

        public static DateTime ParseDate(string date)
        {
            DateTime result;

            if (!TryParseDate(date, out result))
            {
                throw new ApiValidationException(DateFormatErrorMessage);
            }

            return result;
        }

        public static string ToFilenameFormat(this DateTimeOffset dto)
        {
            return dto.UtcDateTime.ToString("yyyyMMddTHHmmssZ", CultureInfo.InvariantCulture);
        }
    }
}
