using DMS_BOL.Validation_Classes;
using DMS_BOL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_DAL.DBLayer
{
    public class DoctorApprovalDb
    {
        private dmswebapp_dentalDBEntities _context;

        public DoctorApprovalDb()
        {
            _context = new dmswebapp_dentalDBEntities();
        }

        public bool InsertDoctorApproved(int DoctorID)
        {
            try
            {
                if (DoctorID > 0)
                {
                    var allAdmins = _context.tblAdmins.Select(x => x.A_ID).ToList();
                    if (allAdmins.Count > 0)
                    {
                        foreach (var item in allAdmins)
                        {
                            var model = new tblDoctorApproved();
                            model.N_IsApproved = false;
                            model.N_IsArchive = false;
                            model.N_IsRead = false;
                            model.N_AdminID = item;
                            model.N_DoctorID = DoctorID;
                            model.N_CreatedBy = true;
                            model.N_CreateadOn = DateTime.Now;
                            _context.tblDoctorApproveds.Add(model);
                            Save();
                        }
                    }
                    var allSuperAdmins = _context.tblSuperAdmins.Select(x => x.SA_ID).ToList();
                    if (allSuperAdmins.Count > 0)
                    {
                        foreach (var item in allSuperAdmins)
                        {
                            var model = new tblDoctorApproved();
                            model.N_IsApproved = false;
                            model.N_IsArchive = false;
                            model.N_IsRead = false;
                            model.N_SuperAdminID = item;
                            model.N_DoctorID = DoctorID;
                            model.N_CreatedBy = true;
                            model.N_CreateadOn = DateTime.Now;
                            _context.tblDoctorApproveds.Add(model);
                            Save();
                        }
                    }
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool UpdateDoctorApproved(tblDoctorApproved model)
        {
            try
            {
                if (model != null)
                {
                    model.N_UpdatedOn = DateTime.Now;
                    _context.Entry(model).State = System.Data.Entity.EntityState.Modified;
                    Save();
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public tblDoctorApproved GetDoctorApprovedByID(int id)
        {
            try
            {
                if (id > 0)
                {
                    var reas = _context.tblDoctorApproveds.Where(x => x.N_ID == id).FirstOrDefault();
                    if (reas != null)
                        return reas;
                    else
                        return null;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        public bool CheckIsDoctorApprovedByID(int id)
        {
            try
            {
                if (id > 0)
                {
                    var reas = _context.tblDoctorApproveds.Any(x => x.N_DoctorID == id && x.N_IsApproved == true);
                    if (reas)
                        return true;
                    else
                        return false;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ValidateDoctorApproval GetDoctorApprovedById(int id)
        {
            try
            {
                if (id > 0)
                {
                    var reas = _context.tblDoctorApproveds.Where(x => x.N_ID == id).Select(a => new ValidateDoctorApproval()
                    {
                        N_ID = a.N_ID,
                        N_DoctorID = a.N_DoctorID,
                        N_IsRead = a.N_IsRead.Value,
                        N_IsApproved = a.N_IsApproved.Value,
                        N_CreateadOn = a.N_CreateadOn,
                        N_CreatedBy = a.N_CreatedBy,
                        tblDoctor = a.tblDoctor
                    }).FirstOrDefault();
                    if (reas != null)
                        return reas;
                    else
                        return null;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool ChangeDANotificationToReadForAD(int AdminID)
        {
            var reas = _context.tblDoctorApproveds.Where(a => a.N_IsRead == false && a.N_AdminID == AdminID).ToList();
            if (reas.Count > 0)
            {
                foreach (var notification in reas)
                {
                    try
                    {
                        notification.N_IsRead = true;
                        _context.Entry(notification).State = System.Data.Entity.EntityState.Modified;
                        _context.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        return false;
                        throw ex;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        
        public bool ChangeDANotificationToReadForSAD(int SuperAdminID)
        {
            var reas = _context.tblDoctorApproveds.Where(a => a.N_IsRead == false && a.N_SuperAdminID == SuperAdminID).ToList();
            if (reas.Count > 0)
            {
                foreach (var notification in reas)
                {
                    try
                    {
                        notification.N_IsRead = true;
                        _context.Entry(notification).State = System.Data.Entity.EntityState.Modified;
                        _context.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        return false;
                        throw ex;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        void Save()
        {
            _context.SaveChanges();
        }

        public IEnumerable<ValidateNotification> GetAllDoctorApprovalRequestForAD(int AdminID)
        {
            var reas = _context.tblDoctorApproveds.Where(a => a.N_AdminID == AdminID && a.N_IsApproved == false).OrderByDescending(a => a.N_CreateadOn).Select(x => new ValidateNotification() { Title = "Approval Request", Description = x.tblDoctor.D_FirstName + " " + x.tblDoctor.D_LastName + " is waiting for our <b>Doctor Profile</b> approval!", CreatedOn = x.N_CreateadOn, IsRead = x.N_IsRead, IsApproved = x.N_IsApproved.Value, DoctorInfo = x.tblDoctor, DoctorID = x.N_DoctorID.Value.ToString(), NotificationID = x.N_ID }).ToList();
            return reas;
        }
        
        public IEnumerable<ValidateNotification> GetAllDoctorApprovalRequestForSAD(int SuperAdminID)
        {
            var reas = _context.tblDoctorApproveds.Where(a => a.N_SuperAdminID == SuperAdminID && a.N_IsApproved == false).OrderByDescending(a => a.N_CreateadOn).Select(x => new ValidateNotification() { Title = "Approval Request", Description = x.tblDoctor.D_FirstName + " " + x.tblDoctor.D_LastName + " is waiting for our <b>Doctor Profile</b> approval!", CreatedOn = x.N_CreateadOn, IsRead = x.N_IsRead, IsApproved = x.N_IsApproved.Value, DoctorInfo = x.tblDoctor, DoctorID = x.N_DoctorID.Value.ToString(), NotificationID = x.N_ID }).ToList();
            return reas;
        }

        public int GetCountDoctorApprovalRequestForAD(int AdminID)
        {
            var reas = _context.tblDoctorApproveds.Where(a => a.N_AdminID == AdminID && a.N_IsRead == false).ToList().Count;
            return reas;
        }
        
        public int GetCountDoctorApprovalRequestForSAD(int SuperAdminID)
        {
            var reas = _context.tblDoctorApproveds.Where(a => a.N_SuperAdminID == SuperAdminID && a.N_IsRead == false).ToList().Count;
            return reas;
        }
    }
}
