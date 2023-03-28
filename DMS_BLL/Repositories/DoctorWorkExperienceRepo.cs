﻿using System;
using DMS_BOL;
using DMS_DAL.DBLayer;
using DMS_BOL.Validation_Classes;
using System.Collections.Generic;

namespace DMS_BLL.Repositories
{
    public class DoctorWorkExperienceRepo
    {
        private DoctorWorkExperienceDb wexRepo;

        public DoctorWorkExperienceRepo()
        {
            wexRepo = new DoctorWorkExperienceDb();
        }

        public bool InsertDoctorWorkExperience(ValidateDoctorWorkExperience model)
        {
            try
            {
                if (model != null)
                {
                    bool reas = false;
                    foreach (var HospitalInfo in model.HospitalInfos)
                    {
                        tblDoctorWorkExperience doctorService = new tblDoctorWorkExperience()
                        {
                            WEX_DoctorID = model.WEX_DoctorID,
                            WEX_HospitalName = HospitalInfo.WEX_HospitalName,
                            WEX_Designation = HospitalInfo.WEX_Designation,
                            WEX_FromDate = HospitalInfo.WEX_FromDate,
                            WEX_ToDate = HospitalInfo.WEX_ToDate,
                            WEX_IsWorking = HospitalInfo.WEX_IsWorking,                           
                            WEX_CreatedBy = model.WEX_CreatedBy,
                        };
                        reas = wexRepo.InsertDoctorWorkExperiences(doctorService);
                    }
                    if (reas)
                        return true;
                    return false;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        public bool UpdateDoctorWorkExperience(ValidateDoctorWorkExperience model)
        {
            try
            {
                if (model != null)
                {
                    bool reas = false;
                    foreach (var HospitalInfo in model.HospitalInfos)
                    {
                        var modelServices = GetDoctorWorkExperienceByID(HospitalInfo.HospitalID);
                        modelServices.WEX_ID = HospitalInfo.HospitalID;
                        modelServices.WEX_DoctorID = model.WEX_DoctorID;
                        modelServices.WEX_HospitalName = HospitalInfo.WEX_HospitalName;
                        modelServices.WEX_Designation = HospitalInfo.WEX_Designation;
                        modelServices.WEX_FromDate = HospitalInfo.WEX_FromDate;
                        modelServices.WEX_ToDate = HospitalInfo.WEX_ToDate;
                        modelServices.WEX_IsWorking = HospitalInfo.WEX_IsWorking;                           
                        modelServices.WEX_UpdatedBy = model.WEX_UpdatedBy;
                        reas = wexRepo.UpdateDoctorWorkExperiences(modelServices);
                    }
                    if (reas)
                        return true;
                    return false;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public tblDoctorWorkExperience GetDoctorWorkExperienceByID(int modelId)
        {
            if (modelId > 0)
                return wexRepo.GetDoctorWorkExperienceByID(modelId);
            return null;
        }
        
        public IEnumerable<tblDoctorWorkExperience> GetDoctorAllWorkExperiencesByID(int modelId)
        {
            if (modelId > 0)
                return wexRepo.GetDoctorAllWorkExperiencesByID(modelId);
            return null;
        }

        public bool InActiveDoctorWorkExperience(int ExpID)
        {
            try
            {
                if (ExpID > 0)
                {
                    var reas = wexRepo.InActiveDoctorWorkExperience(ExpID);
                    if (reas)
                        return true;
                    return false;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool ReActiveDoctorWorkExperience(int ExpID)
        {
            try
            {
                if (ExpID > 0)
                {
                    var reas = wexRepo.ReActiveDoctorWorkExperience(ExpID);
                    if (reas)
                        return true;
                    return false;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
