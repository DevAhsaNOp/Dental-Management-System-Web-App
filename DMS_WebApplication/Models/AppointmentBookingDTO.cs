using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DMS_WebApplication.Models
{
    public class AppointmentBookingDTO
    {
        public int? OfcId { get; set; }
        public int? OcdId { get; set; }
        public int DoctorId { get; set; }
        public int PatientId { get; set; }
        public string DoctorName { get; set; }
        public bool IsProfileVerified { get; set; }
        public string AppointmentType { get; set; }
        public string DoctorSpeciality { get; set; }
        public string DoctorProfileImage { get; set; }
        public string AppointmentLocation { get; set; }
        public decimal AppointmentCharges { get; set; }
        public DateTime AppointmentDateTime { get; set; }
        public List<string> DoctorAvailableDays { get; set; }
        public List<IGrouping<string, DaysWithTimings>> DoctorAvailableDaysWithTimings { get; set; }
    }
}