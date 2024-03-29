﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DMS_BOL.Validation_Classes
{
    public class ValidateDoctorServices
    {
        public int DS_ID { get; set; }

        [Required(ErrorMessage ="*")]
        [Display(Name ="Doctor ID")]
        [RegularExpression("^[0-9]+$", ErrorMessage = "Invalid Doctor ID")]
        public int? DS_DoctorID { get; set; }

        [Required(ErrorMessage = "*")]
        [Display(Name = "Services ID")]
        [RegularExpression("^[0-9]+$", ErrorMessage = "Invalid Services ID")]
        public List<int?> DS_ServicesID { get; set; }

        public DateTime? DS_CreatedOn { get; set; }

        [Required(ErrorMessage = "Provide Created By")]
        [Display(Name = "Created By")]
        [RegularExpression("^[0-9]+$", ErrorMessage = "Invalid Created By")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid Created By")]
        public int? DS_CreatedBy { get; set; }

        public DateTime? DS_UpdatedOn { get; set; }

        [Required(ErrorMessage = "Provide Updated By")]
        [Display(Name = "Updated By")]
        [RegularExpression("^[0-9]+$", ErrorMessage = "Invalid Updated By")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid Updated By")]
        public int? DS_UpdatedBy { get; set; }
        public bool? DS_IsActive { get; set; }
        public bool? DS_IsArchive { get; set; }

        public virtual tblDoctor tblDoctor { get; set; }
        public virtual tblService tblService { get; set; }
    }
}
