using System;
using System.Collections.Generic;

namespace Timenut.Lab.Utils
{
    static class DateUtils
    {
        static Dictionary<DayOfWeek, string> preset =
            new Dictionary<DayOfWeek, string>()
            {
                { DayOfWeek.Sunday, "일" },
                { DayOfWeek.Monday, "월" },
                { DayOfWeek.Tuesday, "화" },
                { DayOfWeek.Wednesday, "수" },
                { DayOfWeek.Thursday, "목" },
                { DayOfWeek.Friday, "금" },
                { DayOfWeek.Saturday, "토" }
            };

        public static string GetDayOfWeekName(this DateTime date)
        {
            return preset[date.DayOfWeek];
        }
    }
}
