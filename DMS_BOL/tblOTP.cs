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
    
    public partial class tblOTP
    {
        public int OT_ID { get; set; }
        public string OT_OTP { get; set; }
        public string OT_UsersEmail { get; set; }
        public Nullable<bool> OT_IsActive { get; set; }
        public Nullable<bool> OT_IsArchive { get; set; }
        public Nullable<System.DateTime> OT_CreatedOn { get; set; }
        public Nullable<System.DateTime> OT_UpdatedOn { get; set; }
    }
}
