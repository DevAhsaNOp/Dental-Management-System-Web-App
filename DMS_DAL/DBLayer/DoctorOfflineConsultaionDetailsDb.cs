using DMS_BOL;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace DMS_DAL.DBLayer
{
    public class DoctorOfflineConsultaionDetailsDb
    {
        private dentalDBEntities _context;

        public DoctorOfflineConsultaionDetailsDb()
        {
            _context = new dentalDBEntities();
        }

        public bool InsertOfflineConsultaionDetails(tblOfflineConsultaionDetail model)
        {
            try
            {
                model.OFCD_IsActive = true;
                model.OFCD_CreatedOn = DateTime.Now;
                model.OFCD_UpdatedOn = null;
                model.OFCD_UpdatedBy = null;
                model.OFCD_IsArchive = false;
                _context.tblOfflineConsultaionDetails.Add(model);
                Save();
                if (model.OFCD_ID > 0)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool UpdateOfflineConsultaionDetails(tblOfflineConsultaionDetail model)
        {
            try
            {
                model.OFCD_IsActive = true;
                model.OFCD_CreatedOn = GetOfflineConsultaionDetailsByID(model.OFCD_ID).OFCD_CreatedOn;
                model.OFCD_CreatedBy = GetOfflineConsultaionDetailsByID(model.OFCD_ID).OFCD_CreatedBy;
                model.OFCD_UpdatedOn = DateTime.Now;
                model.OFCD_IsArchive = false;
                _context.Entry(model).State = EntityState.Modified;
                Save();
                if (model.OFCD_ID > 0)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public tblOfflineConsultaionDetail GetOfflineConsultaionDetailsByID(int modelId)
        {
            return _context.tblOfflineConsultaionDetails.Find(modelId);
        }

        public IEnumerable<tblOfflineConsultaionDetail> GetDoctorAllOfflineConsultaionDetailsByID(int modelId)
        {
            var reas = _context.tblOfflineConsultaionDetails.Where(x => x.OFCD_DoctorID == modelId && x.OFCD_IsActive == true).ToList();
            foreach (var item in reas)
            {
                item.tblAddress = _context.tblAddresses.Find(item.OFCD_HospitalAddressID);
                item.tblAddress.tblCity = _context.tblCities.Find(item.tblAddress.AddressCity);
                item.tblAddress.tblState = _context.tblStates.Find(item.tblAddress.AddressState);
                item.tblAddress.tblZone = _context.tblZones.Find(item.tblAddress.AddressZone);
            }
            return reas;
        }

        public bool InActiveOfflineConsultaionDetails(int OfcdID)
        {
            try
            {
                var model = GetOfflineConsultaionDetailsByID(OfcdID);
                model.OFCD_IsActive = false;
                model.OFCD_UpdatedOn = DateTime.Now;
                model.OFCD_IsArchive = true;
                _context.Entry(model).State = EntityState.Modified;
                Save();
                if (model.OFCD_ID > 0)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool ReActiveOfflineConsultaionDetails(int OfcdID)
        {
            try
            {
                var model = GetOfflineConsultaionDetailsByID(OfcdID);
                model.OFCD_IsActive = true;
                model.OFCD_UpdatedOn = DateTime.Now;
                model.OFCD_IsArchive = false;
                _context.Entry(model).State = EntityState.Modified;
                Save();
                if (model.OFCD_ID > 0)
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
