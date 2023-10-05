using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace DMS_WebApplication.Models
{
    public static class DayWithTimingGenerator
    {
        public static List<IGrouping<string, DaysWithTimings>> GetNext14DaysWithTimings(List<Details> daysWithTimings, out List<string> AvailableDays)
        {
            int count = 0;
            int TotalDays = 14;
            var currentDate = DateTime.Now.Date;
            var next14DaysWithTimings = new List<DaysWithTimings>();

            while (count < TotalDays)
            {
                var dayOfWeek = currentDate.DayOfWeek;
                var matchingDetail = daysWithTimings.Find(detail =>
                    detail.FieldName.Equals($"{dayOfWeek}StartTime", StringComparison.OrdinalIgnoreCase));

                if (matchingDetail != null && TimeSpan.TryParse(matchingDetail.Value, out TimeSpan startTime))
                {
                    var endTimeDetail = daysWithTimings.Find(detail =>
                        detail.FieldName.Equals($"{dayOfWeek}EndTime", StringComparison.OrdinalIgnoreCase));

                    if (endTimeDetail != null && TimeSpan.TryParse(endTimeDetail.Value, out TimeSpan endTime))
                    {
                        var currentTime = currentDate.Date.Add(startTime);
                        while (currentTime <= currentDate.Date.Add(endTime))
                        {
                            next14DaysWithTimings.Add(new DaysWithTimings() { Day = currentTime.Date.ToString("dddd, dd MMM"), Timings = currentTime.ToString("hh:mm tt") });
                            currentTime = currentTime.AddMinutes(20);
                        }
                        count++;
                    }
                }

                currentDate = currentDate.AddDays(1);
            }

            AvailableDays = next14DaysWithTimings.Select(item => item.Day).Distinct().ToList();

            return next14DaysWithTimings.GroupBy(item => item.Day).ToList();
        }
    }
}