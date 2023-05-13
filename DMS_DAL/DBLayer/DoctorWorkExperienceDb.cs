using System;
using DMS_BOL;
using System.Data.Entity;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using DMS_BOL.Validation_Classes;

namespace DMS_DAL.DBLayer
{
    public class DoctorWorkExperienceDb
    {
        private dmswebapp_dentalDBEntities _context;

        public DoctorWorkExperienceDb()
        {
            _context = new dmswebapp_dentalDBEntities();
        }

        public bool InsertDoctorWorkExperiences(tblDoctorWorkExperience model)
        {
            try
            {
                model.WEX_IsActive = true;
                model.WEX_CreatedOn = DateTime.Now;
                model.WEX_UpdatedOn = null;
                model.WEX_UpdatedBy = null;
                model.WEX_IsArchive = false;
                _context.tblDoctorWorkExperiences.Add(model);
                Save();
                if (model.WEX_ID > 0)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool UpdateDoctorWorkExperiences(tblDoctorWorkExperience model)
        {
            try
            {
                model.WEX_IsActive = true;
                model.WEX_CreatedOn = GetDoctorWorkExperienceByID(model.WEX_ID).WEX_CreatedOn;
                model.WEX_CreatedBy = GetDoctorWorkExperienceByID(model.WEX_ID).WEX_CreatedBy;
                model.WEX_UpdatedOn = DateTime.Now;
                model.WEX_IsArchive = false;
                _context.Entry(model).State = EntityState.Modified;
                Save();
                if (model.WEX_ID > 0)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<tblDoctorWorkExperience> GetDoctorAllWorkExperiencesByID(int modelId)
        {
            return _context.tblDoctorWorkExperiences.Where(x => x.WEX_DoctorID == modelId && x.WEX_IsActive == true).ToList();
        }

        public tblDoctorWorkExperience GetDoctorWorkExperienceByID(int modelId)
        {
            return _context.tblDoctorWorkExperiences.Find(modelId);
        }

        public bool InActiveDoctorWorkExperience(int ExpID)
        {
            try
            {
                var model = GetDoctorWorkExperienceByID(ExpID);
                model.WEX_IsActive = false;
                model.WEX_UpdatedOn = DateTime.Now;
                model.WEX_IsArchive = true;
                _context.Entry(model).State = EntityState.Modified;
                Save();
                if (model.WEX_ID > 0)
                    return true;
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
                var model = GetDoctorWorkExperienceByID(ExpID);
                model.WEX_IsActive = true;
                model.WEX_UpdatedOn = DateTime.Now;
                model.WEX_IsArchive = false;
                _context.Entry(model).State = EntityState.Modified;
                Save();
                if (model.WEX_ID > 0)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
