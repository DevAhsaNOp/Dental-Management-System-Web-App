﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class dmswebapp_dentalDBEntities : DbContext
    {
        public dmswebapp_dentalDBEntities()
            : base("name=dmswebapp_dentalDBEntities")
        {
            this.Configuration.LazyLoadingEnabled = false;
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<sysdiagram> sysdiagrams { get; set; }
        public virtual DbSet<tblAddress> tblAddresses { get; set; }
        public virtual DbSet<tblAdmin> tblAdmins { get; set; }
        public virtual DbSet<tblAppointment> tblAppointments { get; set; }
        public virtual DbSet<tblAppointmentDetail> tblAppointmentDetails { get; set; }
        public virtual DbSet<tblCity> tblCities { get; set; }
        public virtual DbSet<tblCountry> tblCountries { get; set; }
        public virtual DbSet<tblDiagnostic> tblDiagnostics { get; set; }
        public virtual DbSet<tblDoctor> tblDoctors { get; set; }
        public virtual DbSet<tblDoctorApproved> tblDoctorApproveds { get; set; }
        public virtual DbSet<tblDoctorReview> tblDoctorReviews { get; set; }
        public virtual DbSet<tblDoctorService> tblDoctorServices { get; set; }
        public virtual DbSet<tblDoctorWorkExperience> tblDoctorWorkExperiences { get; set; }
        public virtual DbSet<tblMessage> tblMessages { get; set; }
        public virtual DbSet<tblMessageDetail> tblMessageDetails { get; set; }
        public virtual DbSet<tblOfflineConsultaionDetail> tblOfflineConsultaionDetails { get; set; }
        public virtual DbSet<tblOnlineConsultaionDetail> tblOnlineConsultaionDetails { get; set; }
        public virtual DbSet<tblOTP> tblOTPs { get; set; }
        public virtual DbSet<tblPatient> tblPatients { get; set; }
        public virtual DbSet<tblPatientTest> tblPatientTests { get; set; }
        public virtual DbSet<tblRole> tblRoles { get; set; }
        public virtual DbSet<tblService> tblServices { get; set; }
        public virtual DbSet<tblState> tblStates { get; set; }
        public virtual DbSet<tblSuperAdmin> tblSuperAdmins { get; set; }
        public virtual DbSet<tblZone> tblZones { get; set; }
    }
}
