using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DMS_BOL.Validation_Classes
{
    public class ValidateDoctor : ValidateUsersProfiles
    {
        [Required(ErrorMessage = "*")]
        [Display(Name = "Doctor Specialization")]
        public string DoctorSpecialization { get; set; }

        [Required(ErrorMessage = "*")]
        [Display(Name = "Year Of Experience")]
        public string DoctorYearsOfExperience { get; set; }

        [Required(ErrorMessage = "*")]
        [Display(Name = "Work Phone Number")]
        [RegularExpression(@"03[0-9]{2}(?!1234567)(?!1111111)(?!7654321)[0-9]{7}", ErrorMessage = "Invalid Phone Number")]
        public string DoctorWorkPhoneNumber { get; set; }

        [Display(Name = "*")]
        public string DoctorAwardsAndAchievements { get; set; }

        [Required(ErrorMessage = "*")]
        [Display(Name = "About Me")]
        public string DoctorAboutMe { get; set; }

        [Required(ErrorMessage = "*")]
        [Display(Name = "Is Profile Completed")]
        public bool? D_IsProfileCompleted { get; set; }

        public int? DoctorSatisfactionRate { get; set; }

        [Required(ErrorMessage = "*")]
        [Display(Name = "Response Time")]
        public int? DoctorResponseTime { get; set; }

        public virtual ICollection<tblAppointment> tblAppointments { get; set; }
        public virtual ICollection<tblDiagnostic> tblDiagnostics { get; set; }
        public virtual ICollection<tblDoctorReview> tblDoctorReviews { get; set; }
        public virtual ICollection<tblDoctorWorkExperience> tblDoctorWorkExperiences { get; set; }
        public virtual ICollection<tblOfflineConsultaionDetail> tblOfflineConsultaionDetails { get; set; }
        public virtual ICollection<tblOnlineConsultaionDetail> tblOnlineConsultaionDetails { get; set; }
    }
}
