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
    
    public partial class tblOfflineConsultaionDetail
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tblOfflineConsultaionDetail()
        {
            this.tblAppointments = new HashSet<tblAppointment>();
        }
    
        public int OFCD_ID { get; set; }
        public int OFCD_DoctorID { get; set; }
        public string OFCD_HospitalName { get; set; }
        public string OFCD_HospitalPhoneNumber { get; set; }
        public int OFCD_HospitalAddressID { get; set; }
        public Nullable<System.TimeSpan> OFCD_MondayStartTime { get; set; }
        public Nullable<System.TimeSpan> OFCD_MondayEndTime { get; set; }
        public Nullable<System.TimeSpan> OFCD_TuesdayStartTime { get; set; }
        public Nullable<System.TimeSpan> OFCD_TuesdayEndTime { get; set; }
        public Nullable<System.TimeSpan> OFCD_WednesdayStartTime { get; set; }
        public Nullable<System.TimeSpan> OFCD_WednesdayEndTime { get; set; }
        public Nullable<System.TimeSpan> OFCD_ThursdayStartTime { get; set; }
        public Nullable<System.TimeSpan> OFCD_ThursdayEndTime { get; set; }
        public Nullable<System.TimeSpan> OFCD_FridayStartTime { get; set; }
        public Nullable<System.TimeSpan> OFCD_FridayEndTime { get; set; }
        public Nullable<System.TimeSpan> OFCD_SaturdayStartTime { get; set; }
        public Nullable<System.TimeSpan> OFCD_SaturdayEndTime { get; set; }
        public Nullable<System.TimeSpan> OFCD_SundayStartTime { get; set; }
        public Nullable<System.TimeSpan> OFCD_SundayEndTime { get; set; }
        public decimal OFCD_Charges { get; set; }
        public Nullable<bool> OFCD_IsActive { get; set; }
        public Nullable<bool> OFCD_IsArchive { get; set; }
        public Nullable<int> OFCD_CreatedBy { get; set; }
        public Nullable<System.DateTime> OFCD_CreatedOn { get; set; }
        public Nullable<int> OFCD_UpdatedBy { get; set; }
        public Nullable<System.DateTime> OFCD_UpdatedOn { get; set; }
    
        public virtual tblAddress tblAddress { get; set; }
        public virtual tblDoctor tblDoctor { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblAppointment> tblAppointments { get; set; }
    }
}
