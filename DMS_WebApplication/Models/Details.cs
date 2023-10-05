using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DMS_WebApplication.Models
{
    public class Details
    {
        public string FieldName { get; set; }
        public string Value { get; set; }
    }

    public class DaysWithTimings
    {
        public string Day { get; set; }
        public string Timings { get; set; }
    }
}