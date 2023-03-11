using DMS_BOL;
using DMS_BOL.Validation_Classes;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace DMS_WebApplication.Controllers
{
    public class DoctorController : Controller
    {
        private HttpClient _httpClient;
        string _BaseURL = "https://dmswepapi.azurewebsites.net/api/";
        AccountController acObj = new AccountController();

        public DoctorController()
        {
            _httpClient = new HttpClient();
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ProfileComplete()
        {

            var AllStates = acObj.GetAllState();
            var states = new List<SelectListItem>();
            foreach (var item in AllStates)
            {
                states.Add(new SelectListItem() { Text = item.StateName, Value = item.StateID.ToString() });
            }
            ViewBag.State = states;
            var AllService = GetAllServices();
            var service = new List<SelectListItem>();
            foreach (var item in AllService)
            {
                service.Add(new SelectListItem() { Text = item.S_Name, Value = item.S_ID.ToString() });
            }
            ViewBag.Service = service;
            ValidateDoctor doctor = new ValidateDoctor();
            return View(doctor);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ProfileComplete(ValidateDoctor doctor)
        {
            return View(doctor);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult InsertExp(ValidateDoctorHospitalInfo doctor)
        {
            var reas = Session["ExperienceList"];
            var ExperienceList = new List<ValidateDoctorHospitalInfo>();
            if (reas == null)
            {
                ExperienceList.Add(new ValidateDoctorHospitalInfo()
                {
                    HospitalID = doctor.HospitalID <= 0 ? 1 : doctor.HospitalID,
                    WEX_Designation = doctor.WEX_Designation,
                    WEX_FromDate = doctor.WEX_FromDate,
                    WEX_HospitalName = doctor.WEX_HospitalName,
                    WEX_IsWorking = doctor.WEX_IsWorking,
                    WEX_ToDate = doctor.WEX_ToDate,
                });
            }
            else
            {
                var data = (List<ValidateDoctorHospitalInfo>)Session["ExperienceList"];
                if (data == null || data.Count <= 0)
                {
                    ExperienceList.Add(new ValidateDoctorHospitalInfo()
                    {
                        HospitalID = doctor.HospitalID <= 0 ? 1 : doctor.HospitalID,
                        WEX_Designation = doctor.WEX_Designation,
                        WEX_FromDate = doctor.WEX_FromDate,
                        WEX_HospitalName = doctor.WEX_HospitalName,
                        WEX_IsWorking = doctor.WEX_IsWorking,
                        WEX_ToDate = doctor.WEX_ToDate,
                    });
                }
                else
                {
                    var ID = data.Max(x => x.HospitalID) + 1;
                    ExperienceList = data;
                    ExperienceList.Add(new ValidateDoctorHospitalInfo()
                    {
                        HospitalID =  ID,
                        WEX_Designation = doctor.WEX_Designation,
                        WEX_FromDate = doctor.WEX_FromDate,
                        WEX_HospitalName = doctor.WEX_HospitalName,
                        WEX_IsWorking = doctor.WEX_IsWorking,
                        WEX_ToDate = doctor.WEX_ToDate,
                    });
                }

            }
            Session["ExperienceList"] = ExperienceList;
            return PartialView("_ShowExperience", ExperienceList);
        }
        
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DeleteExp(int ExpID)
        {
            var reas = Session["ExperienceList"];
            var ExperienceList = new List<ValidateDoctorHospitalInfo>();
            if (ExpID <= 0)
            {
                var data = (List<ValidateDoctorHospitalInfo>)Session["ExperienceList"];
                ExperienceList = data;
                Session["ExperienceList"] = ExperienceList;
                return PartialView("_ShowExperience", ExperienceList);
            }
            else
            {
                if (reas == null)
                {
                    return PartialView("_ShowExperience", ExperienceList);
                }
                else
                {
                    var data = (List<ValidateDoctorHospitalInfo>)Session["ExperienceList"];
                    var ExpItem = data.Where(x => x.HospitalID == ExpID).FirstOrDefault();
                    data.Remove(ExpItem);
                    ExperienceList = data;
                    Session["ExperienceList"] = ExperienceList;
                    return PartialView("_ShowExperience", ExperienceList);
                }
            }
        }
        
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult InsertOfflineConsultation(ValidateDoctorHospitalInfo doctor)
        {
            var reas = Session["ExperienceList"];
            var ExperienceList = new List<ValidateDoctorHospitalInfo>();
            if (reas == null)
            {
                ExperienceList.Add(new ValidateDoctorHospitalInfo()
                {
                    HospitalID = doctor.HospitalID <= 0 ? 1 : doctor.HospitalID,
                    WEX_Designation = doctor.WEX_Designation,
                    WEX_FromDate = doctor.WEX_FromDate,
                    WEX_HospitalName = doctor.WEX_HospitalName,
                    WEX_IsWorking = doctor.WEX_IsWorking,
                    WEX_ToDate = doctor.WEX_ToDate,
                });
            }
            else
            {
                var data = (List<ValidateDoctorHospitalInfo>)Session["ExperienceList"];
                if (data == null || data.Count <= 0)
                {
                    ExperienceList.Add(new ValidateDoctorHospitalInfo()
                    {
                        HospitalID = doctor.HospitalID <= 0 ? 1 : doctor.HospitalID,
                        WEX_Designation = doctor.WEX_Designation,
                        WEX_FromDate = doctor.WEX_FromDate,
                        WEX_HospitalName = doctor.WEX_HospitalName,
                        WEX_IsWorking = doctor.WEX_IsWorking,
                        WEX_ToDate = doctor.WEX_ToDate,
                    });
                }
                else
                {
                    var ID = data.Max(x => x.HospitalID) + 1;
                    ExperienceList = data;
                    ExperienceList.Add(new ValidateDoctorHospitalInfo()
                    {
                        HospitalID =  ID,
                        WEX_Designation = doctor.WEX_Designation,
                        WEX_FromDate = doctor.WEX_FromDate,
                        WEX_HospitalName = doctor.WEX_HospitalName,
                        WEX_IsWorking = doctor.WEX_IsWorking,
                        WEX_ToDate = doctor.WEX_ToDate,
                    });
                }

            }
            Session["ExperienceList"] = ExperienceList;
            return PartialView("_ShowExperience", ExperienceList);
        }
        
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DeleteOfflineConsultation(int ExpID)
        {
            var reas = Session["ExperienceList"];
            var ExperienceList = new List<ValidateDoctorHospitalInfo>();
            if (ExpID <= 0)
            {
                var data = (List<ValidateDoctorHospitalInfo>)Session["ExperienceList"];
                ExperienceList = data;
                Session["ExperienceList"] = ExperienceList;
                return PartialView("_ShowExperience", ExperienceList);
            }
            else
            {
                if (reas == null)
                {
                    return PartialView("_ShowExperience", ExperienceList);
                }
                else
                {
                    var data = (List<ValidateDoctorHospitalInfo>)Session["ExperienceList"];
                    var ExpItem = data.Where(x => x.HospitalID == ExpID).FirstOrDefault();
                    data.Remove(ExpItem);
                    ExperienceList = data;
                    Session["ExperienceList"] = ExperienceList;
                    return PartialView("_ShowExperience", ExperienceList);
                }
            }
        }

        public IEnumerable<tblService> GetAllServices()
        {
            try
            {
                var response = _httpClient.GetAsync(_BaseURL + "Get/AllServices");
                var reas = response.GetAwaiter().GetResult();
                response.Wait();
                using (HttpContent content = reas.Content)
                {
                    var json = JObject.Parse(content.ReadAsStringAsync().Result);
                    var StatusCode = JsonConvert.DeserializeObject<int>(json["StatusCode"].ToString());
                    if (StatusCode == 200)
                    {
                        IEnumerable<tblService> Services = JsonConvert.DeserializeObject<IEnumerable<tblService>>(json["Datalist"].ToString());
                        return Services;
                    }
                    else
                        return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}