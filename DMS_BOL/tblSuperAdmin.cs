//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DMS_BOL
{
    using System;
    using System.Collections.Generic;
    
    public partial class tblSuperAdmin
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tblSuperAdmin()
        {
            this.tblDoctorApproveds = new HashSet<tblDoctorApproved>();
        }
    
        public int SA_ID { get; set; }
        public string SA_FirstName { get; set; }
        public string SA_LastName { get; set; }
        public string SA_PhoneNumber { get; set; }
        public string SA_Gender { get; set; }
        public Nullable<int> SA_AddressID { get; set; }
        public Nullable<int> SA_RoleID { get; set; }
        public string SA_ProfileImage { get; set; }
        public string SA_Email { get; set; }
        public string SA_Password { get; set; }
        public Nullable<int> SA_CreatedBy { get; set; }
        public Nullable<System.DateTime> SA_CreatedOn { get; set; }
        public Nullable<int> SA_UpdatedBy { get; set; }
        public Nullable<System.DateTime> SA_UpdatedOn { get; set; }
        public Nullable<bool> SA_IsActive { get; set; }
        public Nullable<bool> SA_IsArchive { get; set; }
        public string SA_OTP { get; set; }
        public Nullable<bool> SA_Verified { get; set; }
    
        public virtual tblAddress tblAddress { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblDoctorApproved> tblDoctorApproveds { get; set; }
        public virtual tblRole tblRole { get; set; }
    }
}
