using DMS_BOL.Validation_Classes;
using DMS_BOL;
using DMS_DAL.DBLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting.Contexts;

namespace DMS_BLL.Repositories
{
    public class DoctorApprovalRepo
    {
        private DoctorApprovalDb dbObj;

        public DoctorApprovalRepo()
        {
            dbObj = new DoctorApprovalDb();
        }

        public bool InsertDoctorApproved(int DoctorID)
        {
            try
            {
                if (DoctorID > 0)
                {
                    var reas = dbObj.InsertDoctorApproved(DoctorID);
                    if (reas)
                        return reas;
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

        public bool UpdateDoctorApproved(tblDoctorApproved model)
        {
            try
            {
                if (model != null && model.N_ID > 0)
                {
                    var reasObj = dbObj.GetDoctorApprovedByID(model.N_ID);
                    if (reasObj != null)
                    {
                        reasObj.N_IsApproved = model.N_IsApproved;
                        reasObj.N_IsRead = model.N_IsRead;
                        reasObj.N_IsArchive = model.N_IsArchive;
                        reasObj.N_UpdatedBy = model.N_UpdatedBy;
                        var reas = dbObj.UpdateDoctorApproved(reasObj);
                        if (reas)
                            return reas;
                        else
                            return false;
                    }
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
        
        public bool ChangeDANotificationToReadForAD(int AdminID)
        {
            try
            {
                if (AdminID > 0)
                {
                    var reas = dbObj.ChangeDANotificationToReadForAD(AdminID);
                    if (reas)
                        return reas;
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
        
        public bool ChangeDANotificationToReadForSAD(int SuperAdminID)
        {
            try
            {
                if (SuperAdminID > 0)
                {
                    var reas = dbObj.ChangeDANotificationToReadForSAD(SuperAdminID);
                    if (reas)
                        return reas;
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

        public ValidateDoctorApproval GetDoctorApprovalById(int id)
        {
            try
            {
                if (id > 0)
                {
                    var reas = dbObj.GetDoctorApprovedById(id);
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

        public IEnumerable<ValidateNotification> GetAllDoctorApprovalRequestForAD(int AdminID)
        {
            try
            {
                if (AdminID > 0)
                {
                    var reas = dbObj.GetAllDoctorApprovalRequestForAD(AdminID);
                    foreach (var item in reas)
                        item.Time = item.CreatedOn.ToLongTimeString();

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
        
        public IEnumerable<ValidateNotification> GetAllDoctorApprovalRequestForSAD(int SuperAdminID)
        {
            try
            {
                if (SuperAdminID > 0)
                {
                    var reas = dbObj.GetAllDoctorApprovalRequestForSAD(SuperAdminID);
                    foreach (var item in reas)
                        item.Time = item.CreatedOn.ToLongTimeString();

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

        public int GetCountDoctorApprovalRequestForAD(int AdminID)
        {
            try
            {
                if (AdminID > 0)
                {
                    var reas = dbObj.GetCountDoctorApprovalRequestForAD(AdminID);
                    if (reas > 0)
                        return reas;
                    else
                        return 0;
                }
                else
                    return 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        public int GetCountDoctorApprovalRequestForSAD(int SuperAdminID)
        {
            try
            {
                if (SuperAdminID > 0)
                {
                    var reas = dbObj.GetCountDoctorApprovalRequestForSAD(SuperAdminID);
                    if (reas > 0)
                        return reas;
                    else
                        return 0;
                }
                else
                    return 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool IsDoctorProfileApproved(int DoctorID)
        {
            if (DoctorID > 0)
            {
                var reas = dbObj.CheckIsDoctorApprovedByID(DoctorID);
                if (reas)
                    return true;
                else return false;
            }
            else return false;
        }
    }
}
