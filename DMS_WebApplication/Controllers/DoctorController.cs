using System;
using System.Linq;
using System.Net.Http;
using System.Web.Mvc;
using DMS_BLL.Repositories;
using DMS_BOL.Validation_Classes;
using System.Collections.Generic;
using DMS_WebApplication.Models;
using DMS_BOL;
using System.Drawing;
using System.Web;
using DMS_DAL.DBLayer;
using System.IO;
using System.Web.Security;

namespace DMS_WebApplication.Controllers
{
    public class DoctorController : Controller
    {
        private HttpClient _httpClient;
        AccountController acObj = new AccountController();
        private AddressRepo AddressRepoObj;
        private DoctorsRepo DoctorsRepoObj;
        private DoctorServicesRepo servicesRepoObj;
        private DoctorWorkExperienceRepo WexRepoObj;
        private DoctorOfflineConsultaionDetailsRepo OfcdRepoObj;
        private DoctorOnlineConsultaionDetailsRepo OcdRepoObj;

        public DoctorController()
        {
            _httpClient = new HttpClient();
            AddressRepoObj = new AddressRepo();
            DoctorsRepoObj = new DoctorsRepo();
            servicesRepoObj = new DoctorServicesRepo();
            WexRepoObj = new DoctorWorkExperienceRepo();
            OfcdRepoObj = new DoctorOfflineConsultaionDetailsRepo();
            OcdRepoObj = new DoctorOnlineConsultaionDetailsRepo();
        }

        public static IEnumerable<KeyValuePair<string, string>> ObjectToKeyValuePairs(object obj)
        {
            var properties = obj.GetType().GetProperties();

            foreach (var property in properties)
            {
                yield return new KeyValuePair<string, string>(property.Name, property.GetValue(obj)?.ToString());
            }
        }

        #region **Complete Doctor Profile**

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetTempData()
        {
            var reas = TempData["MsgP"];
            if (reas != null)
                return Json(reas, JsonRequestBehavior.AllowGet);
            else
                return Json(null, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        [CustomAuthorize(Roles = "Admin, SuperAdmin, Doctor")]
        public ActionResult ProfileComplete()
        {

            var AllStates = AddressRepoObj.GetAllState();
            var states = new List<SelectListItem>();
            foreach (var item in AllStates)
            {
                states.Add(new SelectListItem() { Text = item.StateName, Value = item.StateID.ToString() });
            }
            ViewBag.State = states;
            var AllService = servicesRepoObj.GetAllDoctorServices();
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
        [CustomAuthorize(Roles = "Admin, SuperAdmin, Doctor")]
        public ActionResult ProfileComplete(ValidateDoctor doctor)
        {
            try
            {
                if (doctor != null)
                {
                    doctor.UserID = int.Parse(Session["UserID"].ToString());
                    var Docreas = DoctorsRepoObj.GetUserDetailById(doctor.UserID);
                    ValidateDoctor validateDoctor = new ValidateDoctor()
                    {
                        tblAddress = Docreas.tblAddress,
                        UserID = int.Parse(Session["UserID"].ToString()),
                        UserFirstName = Docreas.UserFirstName,
                        UserLastName = Docreas.UserLastName,
                        UserEmail = Docreas.UserEmail,
                        UserPhoneNumber = Docreas.UserPhoneNumber,
                        UserProfileImage = Docreas.UserProfileImage,
                        UserPassword = Docreas.UserPassword,
                        tblRole = Docreas.tblRole,
                        UserIsActive = Docreas.UserIsActive.Value,
                        UserVerified = Docreas.UserVerified.Value,
                        UserOTP = null,
                        UserIsArchive = Docreas.UserIsArchive.Value,
                        UserUpdatedBy = int.Parse(Session["UserID"].ToString()),
                        UserUpdatedOn = Docreas.UserUpdatedOn.Value,
                        UserCreatedBy = Docreas.UserCreatedBy.Value,
                        UserCreatedOn = Docreas.UserCreatedOn.Value,
                        Gender = "Male",
                        D_IsProfileCompleted = true,
                        UserUpdatePhoneNumber = Session["PhoneNumber"].ToString(),
                        UserUpdateEmail = Session["Email"].ToString(),
                        DoctorAboutMe = doctor.DoctorAboutMe,
                        DoctorAwardsAndAchievements = doctor.DoctorAwardsAndAchievements,
                        DoctorResponseTime = doctor.DoctorResponseTime,
                        DoctorSpecialization = doctor.DoctorSpecialization,
                        DoctorWorkPhoneNumber = doctor.DoctorWorkPhoneNumber,
                        DoctorYearsOfExperience = doctor.DoctorYearsOfExperience,
                        AreaID = Docreas.tblAddress.AddressZone.Value,
                        CityID = Docreas.tblAddress.AddressCity.Value,
                        StateID = Docreas.tblAddress.AddressState.Value,
                        CompleteAddress = Docreas.tblAddress.AddressComplete
                    };

                    /*
                     ** Code For Jwt Authorization **
                     * var acc = Session["AccessToken"].ToString();
                     * var client = new HttpClient();
                     * var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:44316/api/Update/Doctor");
                     * request.Headers.Add("Authorization", "Bearer " + acc);
                     * var val = ObjectToKeyValuePairs(doctor);
                     * request.Content = new FormUrlEncodedContent(val);
                     * var response = await client.SendAsync(request);
                     * response.EnsureSuccessStatusCode();
                     * var ereas = await response.Content.ReadAsStringAsync();
                    */

                    if (Session["OFCDList"] != null)
                    {
                        var data = (List<ValidateDoctorOfflineConsultaionDetails>)Session["OFCDList"];
                        if (data != null && data.Count > 0)
                        {
                            foreach (var item in data)
                            {
                                item.OFCD_DoctorID = doctor.UserID;
                                item.OFCD_ID = 0;
                                item.OFCD_CreatedBy = doctor.UserID;
                                OfcdRepoObj.InsertOfflineConsultaionDetail(item);
                            }
                        }
                    }
                    if (Session["OCDList"] != null)
                    {
                        var data = (List<ValidateDoctorOnlineConsultaionDetails>)Session["OCDList"];
                        if (data != null && data.Count > 0)
                        {
                            foreach (var item in data)
                            {
                                item.OCD_DoctorID = doctor.UserID;
                                item.OCD_ID = 0;
                                item.OCD_CreatedBy = doctor.UserID;
                                OcdRepoObj.InsertOnlineConsultaionDetail(item);
                            }
                        }
                    }
                    if (Session["ExperienceList"] != null)
                    {
                        var data = (List<ValidateDoctorHospitalInfo>)Session["ExperienceList"];
                        if (data != null && data.Count > 0)
                        {
                            ValidateDoctorWorkExperience experience = new ValidateDoctorWorkExperience()
                            {
                                WEX_ID = 0,
                                WEX_DoctorID = doctor.UserID,
                                HospitalInfos = data,
                                WEX_CreatedBy = doctor.UserID,
                            };
                            WexRepoObj.InsertDoctorWorkExperience(experience);
                        }
                    }
                    if (doctor.Service.Count > 0 && doctor.Service != null)
                    {
                        var service = new ValidateDoctorServices()
                        {
                            DS_DoctorID = doctor.UserID,
                            DS_ServicesID = doctor.Service,
                            DS_CreatedBy = doctor.UserID,
                        };
                        servicesRepoObj.InsertDoctorServices(service);
                    }

                    var reas = DoctorsRepoObj.UpdateDoctor(validateDoctor);
                    if (reas == 1)
                    {
                        TempData["SuccessMsg"] = "Your profile is completed successfully!";
                        TempData["MsgP"] = "1";
                    }
                    else if (reas == -1)
                    {
                        TempData["ErrorMsg"] = "Email already exists. Please ensure to enter not used email account again!";
                        TempData["MsgP"] = "5";
                    }
                    else if (reas == -2)
                    {
                        TempData["ErrorMsg"] = "Phone Number already exists. Please ensure to enter not used phone number again!";
                        TempData["MsgP"] = "2";
                    }
                    else
                    {
                        TempData["ErrorMsg"] = "Error on completing profile. Please try again later!";
                        TempData["MsgP"] = "3";
                    }
                }
                else
                {
                    TempData["ErrorMsg"] = "Error on completing profile!";
                    TempData["MsgP"] = "4";
                    return RedirectToAction("ProfileComplete");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return RedirectToAction("Index", "Account");
        }

        #endregion

        [AcceptVerbs(HttpVerbs.Get)]
        [CustomAuthorize(Roles = "Admin, SuperAdmin, Doctor")]
        public ActionResult Appointments()
        {
            return View();
        }

        [AcceptVerbs(HttpVerbs.Get)]
        [CustomAuthorize(Roles = "Admin, SuperAdmin, Doctor")]
        public ActionResult Messages()
        {
            return View();
        }

        #region **Manage Experience**

        [AcceptVerbs(HttpVerbs.Post)]
        [CustomAuthorize(Roles = "Admin, SuperAdmin, Doctor")]
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
                    IsActive = true,
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
                        IsActive = true,
                    });
                }
                else
                {
                    var ID = data.Max(x => x.HospitalID) + 1;
                    ExperienceList = data;
                    ExperienceList.Add(new ValidateDoctorHospitalInfo()
                    {
                        HospitalID = ID,
                        WEX_Designation = doctor.WEX_Designation,
                        WEX_FromDate = doctor.WEX_FromDate,
                        WEX_HospitalName = doctor.WEX_HospitalName,
                        WEX_IsWorking = doctor.WEX_IsWorking,
                        WEX_ToDate = doctor.WEX_ToDate,
                        IsActive = true,
                    });
                }

            }
            Session["ExperienceList"] = ExperienceList;
            return PartialView("_ShowExperience", ExperienceList);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [CustomAuthorize(Roles = "Admin, SuperAdmin, Doctor")]
        public ActionResult UpdateExp(ValidateDoctorHospitalInfoForUPdate doctor)
        {
            var data = (List<ValidateDoctorHospitalInfo>)Session["ExperienceList"];
            if (data != null && data.Count > 0 && doctor != null)
            {
                var dataUpdate = data.Where(x => x.HospitalID == doctor.HospitalID).FirstOrDefault();
                foreach (var item in data)
                {
                    if (item.HospitalID == doctor.HospitalID)
                    {
                        item.HospitalID = doctor.HospitalID;
                        item.WEX_Designation = doctor.WEX_Designation;
                        item.WEX_FromDate = DateTime.Parse(doctor.WEX_FromDateU);
                        item.WEX_HospitalName = doctor.WEX_HospitalName;
                        item.WEX_IsWorking = doctor.WEX_IsWorking;
                        if (doctor.WEX_ToDateU == null || doctor.WEX_IsWorking == true)
                            item.WEX_ToDate =  null;
                        else
                            item.WEX_ToDate = DateTime.Parse(doctor.WEX_ToDateU);
                        item.IsActive = true;
                    }
                }
            }
            Session["ExperienceList"] = data;
            return PartialView("_ShowExperience", data);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [CustomAuthorize(Roles = "Admin, SuperAdmin, Doctor")]
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
                    ExpItem.IsActive = false;
                    ExperienceList = data;
                    Session["ExperienceList"] = ExperienceList;
                    return PartialView("_ShowExperience", ExperienceList);
                }
            }
        }

        public ActionResult GetDoctorExperienceInfo(int ExpID)
        {
            if (ExpID > 0)
            {
                var dataExp = (List<ValidateDoctorHospitalInfo>)Session["ExperienceList"];
                var ExpInfo = dataExp.Where(x => x.HospitalID == ExpID).FirstOrDefault();
                if (ExpInfo != null)
                {
                    var data = new ValidateDoctorHospitalInfoForUPdate()
                    {
                        HospitalID = ExpInfo.HospitalID,
                        WEX_FromDateU = ExpInfo.WEX_FromDate.Value.ToString("yyyy-MM"),
                        WEX_ToDateU = ExpInfo.WEX_ToDate == null ? null : ExpInfo.WEX_ToDate.Value.ToString("yyyy-MM"),
                        WEX_Designation = ExpInfo.WEX_Designation,
                        WEX_HospitalName = ExpInfo.WEX_HospitalName,
                        WEX_IsWorking = ExpInfo.WEX_IsWorking,
                        IsActive = ExpInfo.IsActive,                        
                    };
                    if (ExpInfo.WEX_ToDate.HasValue)
                        data.WEX_ToDateU = ExpInfo.WEX_ToDate.Value.ToString("yyyy-MM");
                    else
                        data.WEX_ToDateU = null;
                    return PartialView("_UpdateExperience", data);
                }
                else
                    return Json(false, JsonRequestBehavior.AllowGet);
            }
            else
                return Json(false, JsonRequestBehavior.AllowGet);
        }

        public void GetExperienceInfo(int DoctorID)
        {
            Session["ExperienceList"] = null;
            var ExperienceList = new List<ValidateDoctorHospitalInfo>();
            var ExpInfo = WexRepoObj.GetDoctorAllWorkExperiencesByID(DoctorID);
            if (ExpInfo != null && ExpInfo.Count() > 0)
            {
                foreach (var item in ExpInfo)
                {
                    ExperienceList.Add(new ValidateDoctorHospitalInfo()
                    {
                        HospitalID = item.WEX_ID,
                        WEX_ToDate = item.WEX_ToDate,
                        WEX_FromDate = item.WEX_FromDate,
                        WEX_Designation = item.WEX_Designation,
                        WEX_HospitalName = item.WEX_HospitalName,
                        WEX_IsWorking = item.WEX_IsWorking.Value,
                        IsActive = item.WEX_IsActive.Value,
                    });
                }
            }
            Session["ExperienceList"] = ExperienceList;
        }
              
        public List<ValidateDoctorHospitalInfo> GetHospitalExperienceInfo(int DoctorID)
        {
            var ExperienceList = new List<ValidateDoctorHospitalInfo>();
            var ExpInfo = WexRepoObj.GetDoctorAllWorkExperiencesByID(DoctorID);
            if (ExpInfo != null && ExpInfo.Count() > 0)
            {
                foreach (var item in ExpInfo)
                {
                    ExperienceList.Add(new ValidateDoctorHospitalInfo()
                    {
                        HospitalID = item.WEX_ID,
                        WEX_ToDate = item.WEX_ToDate,
                        WEX_FromDate = item.WEX_FromDate,
                        WEX_Designation = item.WEX_Designation,
                        WEX_HospitalName = item.WEX_HospitalName,
                        WEX_IsWorking = item.WEX_IsWorking.Value,
                        IsActive = item.WEX_IsActive.Value,
                    });
                }
            }
            return ExperienceList;
        }

        public bool DeleteDoctorExp(List<ValidateDoctorHospitalInfo> hospitalInfo)
        {
            try
            {
                if (hospitalInfo != null && hospitalInfo.Count > 0)
                {
                    int NoOfReas = 0;
                    foreach (var item in hospitalInfo)
                    {
                        bool reas = WexRepoObj.InActiveDoctorWorkExperience(item.HospitalID);
                        if (reas)
                            NoOfReas++;
                        else
                            NoOfReas += 0;
                    }
                    if (NoOfReas == hospitalInfo.Count)
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

        #endregion

        #region **Manage Offline Consultation**

        [AcceptVerbs(HttpVerbs.Post)]
        [CustomAuthorize(Roles = "Admin, SuperAdmin, Doctor")]
        public ActionResult InsertOfflineConsultation(ValidateDoctorOfflineConsultaionDetails ofcd)
        {
            var reas = Session["OFCDList"];
            var OFCDList = new List<ValidateDoctorOfflineConsultaionDetails>();
            if (reas == null)
            {
                OFCDList.Add(new ValidateDoctorOfflineConsultaionDetails()
                {
                    OFCD_ID = ofcd.OFCD_ID <= 0 ? 1 : ofcd.OFCD_ID,
                    OFCD_HospitalName = ofcd.OFCD_HospitalName,
                    OFCD_HospitalPhoneNumber = ofcd.OFCD_HospitalPhoneNumber,
                    OFCD_MondayEndTime = ofcd.OFCD_MondayEndTime == null ? null : ofcd.OFCD_MondayEndTime,
                    OFCD_MondayStartTime = ofcd.OFCD_MondayStartTime == null ? null : ofcd.OFCD_MondayStartTime,
                    OFCD_TuesdayEndTime = ofcd.OFCD_TuesdayEndTime == null ? null : ofcd.OFCD_TuesdayEndTime,
                    OFCD_TuesdayStartTime = ofcd.OFCD_TuesdayStartTime == null ? null : ofcd.OFCD_TuesdayStartTime,
                    OFCD_WednesdayEndTime = ofcd.OFCD_WednesdayEndTime == null ? null : ofcd.OFCD_WednesdayEndTime,
                    OFCD_WednesdayStartTime = ofcd.OFCD_WednesdayStartTime == null ? null : ofcd.OFCD_WednesdayStartTime,
                    OFCD_ThursdayEndTime = ofcd.OFCD_ThursdayEndTime == null ? null : ofcd.OFCD_ThursdayEndTime,
                    OFCD_ThursdayStartTime = ofcd.OFCD_ThursdayStartTime == null ? null : ofcd.OFCD_ThursdayStartTime,
                    OFCD_FridayEndTime = ofcd.OFCD_FridayEndTime == null ? null : ofcd.OFCD_FridayEndTime,
                    OFCD_FridayStartTime = ofcd.OFCD_FridayStartTime == null ? null : ofcd.OFCD_FridayStartTime,
                    OFCD_SaturdayEndTime = ofcd.OFCD_SaturdayEndTime == null ? null : ofcd.OFCD_SaturdayEndTime,
                    OFCD_SaturdayStartTime = ofcd.OFCD_SaturdayStartTime == null ? null : ofcd.OFCD_SaturdayStartTime,
                    OFCD_SundayEndTime = ofcd.OFCD_SundayEndTime == null ? null : ofcd.OFCD_SundayEndTime,
                    OFCD_SundayStartTime = ofcd.OFCD_SundayStartTime == null ? null : ofcd.OFCD_SundayStartTime,
                    OFCD_Charges = ofcd.OFCD_Charges,
                    StateID = int.Parse(ofcd.State),
                    CityID = int.Parse(ofcd.City),
                    AreaID = int.Parse(ofcd.Area),
                    State = AddressRepoObj.GetStateById(int.Parse(ofcd.State)).StateName,
                    City = AddressRepoObj.GetCityById(int.Parse(ofcd.City)).CityName,
                    Area = AddressRepoObj.GetZoneById(int.Parse(ofcd.Area)).ZoneName,
                    CompleteAddress = ofcd.CompleteAddress,
                    OFCD_IsActive = true
                });
            }
            else
            {
                var data = (List<ValidateDoctorOfflineConsultaionDetails>)Session["OFCDList"];
                if (data == null || data.Count <= 0)
                {
                    OFCDList.Add(new ValidateDoctorOfflineConsultaionDetails()
                    {
                        OFCD_ID = ofcd.OFCD_ID <= 0 ? 1 : ofcd.OFCD_ID,
                        OFCD_HospitalName = ofcd.OFCD_HospitalName,
                        OFCD_HospitalPhoneNumber = ofcd.OFCD_HospitalPhoneNumber,
                        OFCD_MondayEndTime = ofcd.OFCD_MondayEndTime == null ? null : ofcd.OFCD_MondayEndTime,
                        OFCD_MondayStartTime = ofcd.OFCD_MondayStartTime == null ? null : ofcd.OFCD_MondayStartTime,
                        OFCD_TuesdayEndTime = ofcd.OFCD_TuesdayEndTime == null ? null : ofcd.OFCD_TuesdayEndTime,
                        OFCD_TuesdayStartTime = ofcd.OFCD_TuesdayStartTime == null ? null : ofcd.OFCD_TuesdayStartTime,
                        OFCD_WednesdayEndTime = ofcd.OFCD_WednesdayEndTime == null ? null : ofcd.OFCD_WednesdayEndTime,
                        OFCD_WednesdayStartTime = ofcd.OFCD_WednesdayStartTime == null ? null : ofcd.OFCD_WednesdayStartTime,
                        OFCD_ThursdayEndTime = ofcd.OFCD_ThursdayEndTime == null ? null : ofcd.OFCD_ThursdayEndTime,
                        OFCD_ThursdayStartTime = ofcd.OFCD_ThursdayStartTime == null ? null : ofcd.OFCD_ThursdayStartTime,
                        OFCD_FridayEndTime = ofcd.OFCD_FridayEndTime == null ? null : ofcd.OFCD_FridayEndTime,
                        OFCD_FridayStartTime = ofcd.OFCD_FridayStartTime == null ? null : ofcd.OFCD_FridayStartTime,
                        OFCD_SaturdayEndTime = ofcd.OFCD_SaturdayEndTime == null ? null : ofcd.OFCD_SaturdayEndTime,
                        OFCD_SaturdayStartTime = ofcd.OFCD_SaturdayStartTime == null ? null : ofcd.OFCD_SaturdayStartTime,
                        OFCD_SundayEndTime = ofcd.OFCD_SundayEndTime == null ? null : ofcd.OFCD_SundayEndTime,
                        OFCD_SundayStartTime = ofcd.OFCD_SundayStartTime == null ? null : ofcd.OFCD_SundayStartTime,
                        OFCD_Charges = ofcd.OFCD_Charges,
                        StateID = int.Parse(ofcd.State),
                        CityID = int.Parse(ofcd.City),
                        AreaID = int.Parse(ofcd.Area),
                        State = AddressRepoObj.GetStateById(int.Parse(ofcd.State)).StateName,
                        City = AddressRepoObj.GetCityById(int.Parse(ofcd.City)).CityName,
                        Area = AddressRepoObj.GetZoneById(int.Parse(ofcd.Area)).ZoneName,
                        CompleteAddress = ofcd.CompleteAddress,
                        OFCD_IsActive = true
                    });
                }
                else
                {
                    var ID = data.Max(x => x.OFCD_ID) + 1;
                    OFCDList = data;
                    OFCDList.Add(new ValidateDoctorOfflineConsultaionDetails()
                    {
                        OFCD_ID = ID,
                        OFCD_HospitalName = ofcd.OFCD_HospitalName,
                        OFCD_HospitalPhoneNumber = ofcd.OFCD_HospitalPhoneNumber,
                        OFCD_MondayEndTime = ofcd.OFCD_MondayEndTime == null ? null : ofcd.OFCD_MondayEndTime,
                        OFCD_MondayStartTime = ofcd.OFCD_MondayStartTime == null ? null : ofcd.OFCD_MondayStartTime,
                        OFCD_TuesdayEndTime = ofcd.OFCD_TuesdayEndTime == null ? null : ofcd.OFCD_TuesdayEndTime,
                        OFCD_TuesdayStartTime = ofcd.OFCD_TuesdayStartTime == null ? null : ofcd.OFCD_TuesdayStartTime,
                        OFCD_WednesdayEndTime = ofcd.OFCD_WednesdayEndTime == null ? null : ofcd.OFCD_WednesdayEndTime,
                        OFCD_WednesdayStartTime = ofcd.OFCD_WednesdayStartTime == null ? null : ofcd.OFCD_WednesdayStartTime,
                        OFCD_ThursdayEndTime = ofcd.OFCD_ThursdayEndTime == null ? null : ofcd.OFCD_ThursdayEndTime,
                        OFCD_ThursdayStartTime = ofcd.OFCD_ThursdayStartTime == null ? null : ofcd.OFCD_ThursdayStartTime,
                        OFCD_FridayEndTime = ofcd.OFCD_FridayEndTime == null ? null : ofcd.OFCD_FridayEndTime,
                        OFCD_FridayStartTime = ofcd.OFCD_FridayStartTime == null ? null : ofcd.OFCD_FridayStartTime,
                        OFCD_SaturdayEndTime = ofcd.OFCD_SaturdayEndTime == null ? null : ofcd.OFCD_SaturdayEndTime,
                        OFCD_SaturdayStartTime = ofcd.OFCD_SaturdayStartTime == null ? null : ofcd.OFCD_SaturdayStartTime,
                        OFCD_SundayEndTime = ofcd.OFCD_SundayEndTime == null ? null : ofcd.OFCD_SundayEndTime,
                        OFCD_SundayStartTime = ofcd.OFCD_SundayStartTime == null ? null : ofcd.OFCD_SundayStartTime,
                        OFCD_Charges = ofcd.OFCD_Charges,
                        StateID = int.Parse(ofcd.State),
                        CityID = int.Parse(ofcd.City),
                        AreaID = int.Parse(ofcd.Area),
                        State = AddressRepoObj.GetStateById(int.Parse(ofcd.State)).StateName,
                        City = AddressRepoObj.GetCityById(int.Parse(ofcd.City)).CityName,
                        Area = AddressRepoObj.GetZoneById(int.Parse(ofcd.Area)).ZoneName,
                        CompleteAddress = ofcd.CompleteAddress,
                        OFCD_IsActive = true
                    });
                }
            }
            Session["OFCDList"] = OFCDList;
            return PartialView("_ShowOfflineConsultation", OFCDList);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [CustomAuthorize(Roles = "Admin, SuperAdmin, Doctor")]
        public ActionResult UpdateOfcd(ValidateDoctorOfflineConsultaionDetails doctor)
        {
            var data = (List<ValidateDoctorOfflineConsultaionDetails>)Session["OFCDList"];
            if (data != null && data.Count > 0 && doctor != null)
            {
                var dataUpdate = data.Where(x => x.OFCD_ID == doctor.OFCD_ID).FirstOrDefault();
                foreach (var item in data)
                {
                    if (item.OFCD_ID == doctor.OFCD_ID)
                    {
                        item.OFCD_ID = doctor.OFCD_ID;
                        item.OFCD_HospitalName = doctor.OFCD_HospitalName;
                        item.OFCD_HospitalPhoneNumber = doctor.OFCD_HospitalPhoneNumber;
                        item.OFCD_MondayEndTime = doctor.OFCD_MondayEndTime == null ? null : doctor.OFCD_MondayEndTime;
                        item.OFCD_MondayStartTime = doctor.OFCD_MondayStartTime == null ? null : doctor.OFCD_MondayStartTime;
                        item.OFCD_TuesdayEndTime = doctor.OFCD_TuesdayEndTime == null ? null : doctor.OFCD_TuesdayEndTime;
                        item.OFCD_TuesdayStartTime = doctor.OFCD_TuesdayStartTime == null ? null : doctor.OFCD_TuesdayStartTime;
                        item.OFCD_WednesdayEndTime = doctor.OFCD_WednesdayEndTime == null ? null : doctor.OFCD_WednesdayEndTime;
                        item.OFCD_WednesdayStartTime = doctor.OFCD_WednesdayStartTime == null ? null : doctor.OFCD_WednesdayStartTime;
                        item.OFCD_ThursdayEndTime = doctor.OFCD_ThursdayEndTime == null ? null : doctor.OFCD_ThursdayEndTime;
                        item.OFCD_ThursdayStartTime = doctor.OFCD_ThursdayStartTime == null ? null : doctor.OFCD_ThursdayStartTime;
                        item.OFCD_FridayEndTime = doctor.OFCD_FridayEndTime == null ? null : doctor.OFCD_FridayEndTime;
                        item.OFCD_FridayStartTime = doctor.OFCD_FridayStartTime == null ? null : doctor.OFCD_FridayStartTime;
                        item.OFCD_SaturdayEndTime = doctor.OFCD_SaturdayEndTime == null ? null : doctor.OFCD_SaturdayEndTime;
                        item.OFCD_SaturdayStartTime = doctor.OFCD_SaturdayStartTime == null ? null : doctor.OFCD_SaturdayStartTime;
                        item.OFCD_SundayEndTime = doctor.OFCD_SundayEndTime == null ? null : doctor.OFCD_SundayEndTime;
                        item.OFCD_SundayStartTime = doctor.OFCD_SundayStartTime == null ? null : doctor.OFCD_SundayStartTime;
                        item.OFCD_Charges = doctor.OFCD_Charges;
                        item.StateID = doctor.StateID;
                        item.CityID = doctor.CityID;
                        item.AreaID = doctor.AreaID;
                        item.State = AddressRepoObj.GetStateById(doctor.StateID).StateName;
                        item.City = AddressRepoObj.GetCityById(doctor.CityID).CityName;
                        item.Area = AddressRepoObj.GetZoneById(doctor.AreaID).ZoneName;
                        item.CompleteAddress = doctor.CompleteAddress;
                        item.OFCD_IsActive = true;
                    }
                }
            }
            Session["OFCDList"] = data;
            return PartialView("_ShowOfflineConsultation", data);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [CustomAuthorize(Roles = "Admin, SuperAdmin, Doctor")]
        public ActionResult DeleteOfflineConsultation(int OfcdID)
        {
            var reas = Session["OFCDList"];
            var OFCDList = new List<ValidateDoctorOfflineConsultaionDetails>();
            if (OfcdID <= 0)
            {
                var data = (List<ValidateDoctorOfflineConsultaionDetails>)Session["OFCDList"];
                OFCDList = data;
                Session["OFCDList"] = OFCDList;
                return PartialView("_ShowOfflineConsultation", OFCDList);
            }
            else
            {
                if (reas == null)
                {
                    return PartialView("_ShowOfflineConsultation", OFCDList);
                }
                else
                {
                    var data = (List<ValidateDoctorOfflineConsultaionDetails>)Session["OFCDList"];
                    var OfcdItem = data.Where(x => x.OFCD_ID == OfcdID).FirstOrDefault();
                    OfcdItem.OFCD_IsActive = false;
                    OFCDList = data;
                    Session["OFCDList"] = OFCDList;
                    return PartialView("_ShowOfflineConsultation", OFCDList);
                }
            }
        }

        public ActionResult GetDoctorOfflineConsultationInfo(int OfcdID)
        {
            if (OfcdID > 0)
            {
                var dataOfcd = (List<ValidateDoctorOfflineConsultaionDetails>)Session["OFCDList"];
                var OfcdInfo = dataOfcd.Where(x => x.OFCD_ID == OfcdID).FirstOrDefault();
                if (OfcdInfo != null)
                {
                    var data = new ValidateDoctorOfflineConsultaionDetails()
                    {
                        OFCD_ID = OfcdInfo.OFCD_ID,
                        OFCD_HospitalName = OfcdInfo.OFCD_HospitalName,
                        OFCD_HospitalPhoneNumber = OfcdInfo.OFCD_HospitalPhoneNumber,
                        OFCD_MondayEndTime = OfcdInfo.OFCD_MondayEndTime == null ? null : OfcdInfo.OFCD_MondayEndTime,
                        OFCD_MondayStartTime = OfcdInfo.OFCD_MondayStartTime == null ? null : OfcdInfo.OFCD_MondayStartTime,
                        OFCD_TuesdayEndTime = OfcdInfo.OFCD_TuesdayEndTime == null ? null : OfcdInfo.OFCD_TuesdayEndTime,
                        OFCD_TuesdayStartTime = OfcdInfo.OFCD_TuesdayStartTime == null ? null : OfcdInfo.OFCD_TuesdayStartTime,
                        OFCD_WednesdayEndTime = OfcdInfo.OFCD_WednesdayEndTime == null ? null : OfcdInfo.OFCD_WednesdayEndTime,
                        OFCD_WednesdayStartTime = OfcdInfo.OFCD_WednesdayStartTime == null ? null : OfcdInfo.OFCD_WednesdayStartTime,
                        OFCD_ThursdayEndTime = OfcdInfo.OFCD_ThursdayEndTime == null ? null : OfcdInfo.OFCD_ThursdayEndTime,
                        OFCD_ThursdayStartTime = OfcdInfo.OFCD_ThursdayStartTime == null ? null : OfcdInfo.OFCD_ThursdayStartTime,
                        OFCD_FridayEndTime = OfcdInfo.OFCD_FridayEndTime == null ? null : OfcdInfo.OFCD_FridayEndTime,
                        OFCD_FridayStartTime = OfcdInfo.OFCD_FridayStartTime == null ? null : OfcdInfo.OFCD_FridayStartTime,
                        OFCD_SaturdayEndTime = OfcdInfo.OFCD_SaturdayEndTime == null ? null : OfcdInfo.OFCD_SaturdayEndTime,
                        OFCD_SaturdayStartTime = OfcdInfo.OFCD_SaturdayStartTime == null ? null : OfcdInfo.OFCD_SaturdayStartTime,
                        OFCD_SundayEndTime = OfcdInfo.OFCD_SundayEndTime == null ? null : OfcdInfo.OFCD_SundayEndTime,
                        OFCD_SundayStartTime = OfcdInfo.OFCD_SundayStartTime == null ? null : OfcdInfo.OFCD_SundayStartTime,
                        OFCD_Charges = OfcdInfo.OFCD_Charges,
                        StateID = OfcdInfo.StateID,
                        CityID = OfcdInfo.CityID,
                        AreaID = OfcdInfo.AreaID,
                        State = AddressRepoObj.GetStateById(OfcdInfo.StateID).StateName,
                        City = AddressRepoObj.GetCityById(OfcdInfo.CityID).CityName,
                        Area = AddressRepoObj.GetZoneById(OfcdInfo.AreaID).ZoneName,
                        CompleteAddress = OfcdInfo.CompleteAddress,
                        OFCD_IsActive = true,                        
                    };
                    ViewBag.StateU = AddressRepoObj.GetAllStateDropdown();
                    ViewBag.CityU = AddressRepoObj.GetAllCityByStateDropdown(data.StateID);
                    ViewBag.AreaU = AddressRepoObj.GetAllZoneByCityDropdown(data.CityID);
                    return PartialView("_UpdateOfflineConsultation", data);
                }
                else
                    return Json(false, JsonRequestBehavior.AllowGet);
            }
            else
                return Json(false, JsonRequestBehavior.AllowGet);
        }

        public void GetOfflineConsultation(int DoctorID)
        {
            Session["OFCDList"] = null;
            var OFCDInfo = OfcdRepoObj.GetDoctorAllOfflineConsultaionDetailsByID(DoctorID);
            var OFCDList = new List<ValidateDoctorOfflineConsultaionDetails>();

            if (OFCDInfo != null && OFCDInfo.Count() > 0)
            {
                foreach (var item in OFCDInfo)
                {
                    OFCDList.Add(new ValidateDoctorOfflineConsultaionDetails()
                    {
                        OFCD_ID = item.OFCD_ID,
                        OFCD_HospitalName = item.OFCD_HospitalName,
                        OFCD_HospitalPhoneNumber = item.OFCD_HospitalPhoneNumber,
                        OFCD_MondayEndTime = item.OFCD_MondayEndTime == null ? null : item.OFCD_MondayEndTime,
                        OFCD_MondayStartTime = item.OFCD_MondayStartTime == null ? null : item.OFCD_MondayStartTime,
                        OFCD_TuesdayEndTime = item.OFCD_TuesdayEndTime == null ? null : item.OFCD_TuesdayEndTime,
                        OFCD_TuesdayStartTime = item.OFCD_TuesdayStartTime == null ? null : item.OFCD_TuesdayStartTime,
                        OFCD_WednesdayEndTime = item.OFCD_WednesdayEndTime == null ? null : item.OFCD_WednesdayEndTime,
                        OFCD_WednesdayStartTime = item.OFCD_WednesdayStartTime == null ? null : item.OFCD_WednesdayStartTime,
                        OFCD_ThursdayEndTime = item.OFCD_ThursdayEndTime == null ? null : item.OFCD_ThursdayEndTime,
                        OFCD_ThursdayStartTime = item.OFCD_ThursdayStartTime == null ? null : item.OFCD_ThursdayStartTime,
                        OFCD_FridayEndTime = item.OFCD_FridayEndTime == null ? null : item.OFCD_FridayEndTime,
                        OFCD_FridayStartTime = item.OFCD_FridayStartTime == null ? null : item.OFCD_FridayStartTime,
                        OFCD_SaturdayEndTime = item.OFCD_SaturdayEndTime == null ? null : item.OFCD_SaturdayEndTime,
                        OFCD_SaturdayStartTime = item.OFCD_SaturdayStartTime == null ? null : item.OFCD_SaturdayStartTime,
                        OFCD_SundayEndTime = item.OFCD_SundayEndTime == null ? null : item.OFCD_SundayEndTime,
                        OFCD_SundayStartTime = item.OFCD_SundayStartTime == null ? null : item.OFCD_SundayStartTime,
                        OFCD_Charges = item.OFCD_Charges,
                        State = item.tblAddress.tblState.StateName,
                        City = item.tblAddress.tblCity.CityName,
                        Area = item.tblAddress.tblZone.ZoneName,
                        AreaID = item.tblAddress.tblZone.ZoneID,
                        StateID = item.tblAddress.tblState.StateID,
                        CityID = item.tblAddress.tblCity.CityID,
                        CompleteAddress = item.tblAddress.AddressComplete,
                        OFCD_IsActive = item.OFCD_IsActive,
                    });
                }
            }
            Session["OFCDList"] = OFCDList;
        }
        
        public List<ValidateDoctorOfflineConsultaionDetails> GetHospitalOfflineConsultation(int DoctorID)
        {
            var OFCDInfo = OfcdRepoObj.GetDoctorAllOfflineConsultaionDetailsByID(DoctorID);
            var OFCDList = new List<ValidateDoctorOfflineConsultaionDetails>();

            if (OFCDInfo != null && OFCDInfo.Count() > 0)
            {
                foreach (var item in OFCDInfo)
                {
                    OFCDList.Add(new ValidateDoctorOfflineConsultaionDetails()
                    {
                        OFCD_ID = item.OFCD_ID,
                        OFCD_HospitalName = item.OFCD_HospitalName,
                        OFCD_HospitalPhoneNumber = item.OFCD_HospitalPhoneNumber,
                        OFCD_MondayEndTime = item.OFCD_MondayEndTime == null ? null : item.OFCD_MondayEndTime,
                        OFCD_MondayStartTime = item.OFCD_MondayStartTime == null ? null : item.OFCD_MondayStartTime,
                        OFCD_TuesdayEndTime = item.OFCD_TuesdayEndTime == null ? null : item.OFCD_TuesdayEndTime,
                        OFCD_TuesdayStartTime = item.OFCD_TuesdayStartTime == null ? null : item.OFCD_TuesdayStartTime,
                        OFCD_WednesdayEndTime = item.OFCD_WednesdayEndTime == null ? null : item.OFCD_WednesdayEndTime,
                        OFCD_WednesdayStartTime = item.OFCD_WednesdayStartTime == null ? null : item.OFCD_WednesdayStartTime,
                        OFCD_ThursdayEndTime = item.OFCD_ThursdayEndTime == null ? null : item.OFCD_ThursdayEndTime,
                        OFCD_ThursdayStartTime = item.OFCD_ThursdayStartTime == null ? null : item.OFCD_ThursdayStartTime,
                        OFCD_FridayEndTime = item.OFCD_FridayEndTime == null ? null : item.OFCD_FridayEndTime,
                        OFCD_FridayStartTime = item.OFCD_FridayStartTime == null ? null : item.OFCD_FridayStartTime,
                        OFCD_SaturdayEndTime = item.OFCD_SaturdayEndTime == null ? null : item.OFCD_SaturdayEndTime,
                        OFCD_SaturdayStartTime = item.OFCD_SaturdayStartTime == null ? null : item.OFCD_SaturdayStartTime,
                        OFCD_SundayEndTime = item.OFCD_SundayEndTime == null ? null : item.OFCD_SundayEndTime,
                        OFCD_SundayStartTime = item.OFCD_SundayStartTime == null ? null : item.OFCD_SundayStartTime,
                        OFCD_Charges = item.OFCD_Charges,
                        State = item.tblAddress.tblState.StateName,
                        City = item.tblAddress.tblCity.CityName,
                        Area = item.tblAddress.tblZone.ZoneName,
                        CompleteAddress = item.tblAddress.AddressComplete,
                        OFCD_IsActive = item.OFCD_IsActive,
                    });
                }
            }
            return OFCDList;
        }

        public bool DeleteDoctorOFCD(List<ValidateDoctorOfflineConsultaionDetails> OFCDLst)
        {
            try
            {
                if (OFCDLst != null && OFCDLst.Count > 0)
                {
                    int NoOfReas = 0;
                    foreach (var item in OFCDLst)
                    {
                        bool reas = OfcdRepoObj.InActiveOfflineConsultaionDetail(item.OFCD_ID);
                        if (reas)
                            NoOfReas++;
                        else
                            NoOfReas += 0;
                    }
                    if (NoOfReas == OFCDLst.Count)
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

        #endregion

        #region **Manage Online Consultation**

        [AcceptVerbs(HttpVerbs.Post)]
        [CustomAuthorize(Roles = "Admin, SuperAdmin, Doctor")]
        public ActionResult InsertOnlineConsultation(ValidateDoctorOnlineConsultaionDetails Ocd)
        {
            var reas = Session["OCDList"];
            var OCDList = new List<ValidateDoctorOnlineConsultaionDetails>();
            if (reas == null)
            {
                OCDList.Add(new ValidateDoctorOnlineConsultaionDetails()
                {
                   OCD_ID = Ocd.OCD_ID <= 0 ? 1 : Ocd.OCD_ID,
                   OCD_MondayEndTime = Ocd.OCD_MondayEndTime == null ? null : Ocd.OCD_MondayEndTime,
                   OCD_MondayStartTime = Ocd.OCD_MondayStartTime == null ? null : Ocd.OCD_MondayStartTime,
                   OCD_TuesdayEndTime = Ocd.OCD_TuesdayEndTime == null ? null : Ocd.OCD_TuesdayEndTime,
                   OCD_TuesdayStartTime = Ocd.OCD_TuesdayStartTime == null ? null : Ocd.OCD_TuesdayStartTime,
                   OCD_WednesdayEndTime = Ocd.OCD_WednesdayEndTime == null ? null : Ocd.OCD_WednesdayEndTime,
                   OCD_WednesdayStartTime = Ocd.OCD_WednesdayStartTime == null ? null : Ocd.OCD_WednesdayStartTime,
                   OCD_ThursdayEndTime = Ocd.OCD_ThursdayEndTime == null ? null : Ocd.OCD_ThursdayEndTime,
                   OCD_ThursdayStartTime = Ocd.OCD_ThursdayStartTime == null ? null : Ocd.OCD_ThursdayStartTime,
                   OCD_FridayEndTime = Ocd.OCD_FridayEndTime == null ? null : Ocd.OCD_FridayEndTime,
                   OCD_FridayStartTime = Ocd.OCD_FridayStartTime == null ? null : Ocd.OCD_FridayStartTime,
                   OCD_SaturdayEndTime = Ocd.OCD_SaturdayEndTime == null ? null : Ocd.OCD_SaturdayEndTime,
                   OCD_SaturdayStartTime = Ocd.OCD_SaturdayStartTime == null ? null : Ocd.OCD_SaturdayStartTime,
                   OCD_SundayEndTime = Ocd.OCD_SundayEndTime == null ? null : Ocd.OCD_SundayEndTime,
                   OCD_SundayStartTime = Ocd.OCD_SundayStartTime == null ? null : Ocd.OCD_SundayStartTime,
                   OCD_Charges = Ocd.OCD_Charges,
                   OCD_IsActive = true,
                });
            }
            else
            {
                var data = (List<ValidateDoctorOnlineConsultaionDetails>)Session["OCDList"];
                if (data == null || data.Count <= 0)
                {
                    OCDList.Add(new ValidateDoctorOnlineConsultaionDetails()
                    {
                       OCD_ID = Ocd.OCD_ID <= 0 ? 1 : Ocd.OCD_ID,
                       OCD_MondayEndTime = Ocd.OCD_MondayEndTime == null ? null : Ocd.OCD_MondayEndTime,
                       OCD_MondayStartTime = Ocd.OCD_MondayStartTime == null ? null : Ocd.OCD_MondayStartTime,
                       OCD_TuesdayEndTime = Ocd.OCD_TuesdayEndTime == null ? null : Ocd.OCD_TuesdayEndTime,
                       OCD_TuesdayStartTime = Ocd.OCD_TuesdayStartTime == null ? null : Ocd.OCD_TuesdayStartTime,
                       OCD_WednesdayEndTime = Ocd.OCD_WednesdayEndTime == null ? null : Ocd.OCD_WednesdayEndTime,
                       OCD_WednesdayStartTime = Ocd.OCD_WednesdayStartTime == null ? null : Ocd.OCD_WednesdayStartTime,
                       OCD_ThursdayEndTime = Ocd.OCD_ThursdayEndTime == null ? null : Ocd.OCD_ThursdayEndTime,
                       OCD_ThursdayStartTime = Ocd.OCD_ThursdayStartTime == null ? null : Ocd.OCD_ThursdayStartTime,
                       OCD_FridayEndTime = Ocd.OCD_FridayEndTime == null ? null : Ocd.OCD_FridayEndTime,
                       OCD_FridayStartTime = Ocd.OCD_FridayStartTime == null ? null : Ocd.OCD_FridayStartTime,
                       OCD_SaturdayEndTime = Ocd.OCD_SaturdayEndTime == null ? null : Ocd.OCD_SaturdayEndTime,
                       OCD_SaturdayStartTime = Ocd.OCD_SaturdayStartTime == null ? null : Ocd.OCD_SaturdayStartTime,
                       OCD_SundayEndTime = Ocd.OCD_SundayEndTime == null ? null : Ocd.OCD_SundayEndTime,
                       OCD_SundayStartTime = Ocd.OCD_SundayStartTime == null ? null : Ocd.OCD_SundayStartTime,
                       OCD_Charges = Ocd.OCD_Charges,
                       OCD_IsActive = true,
                    });
                }
                else
                {
                    var ID = data.Max(x => x.OCD_ID) + 1;
                    OCDList = data;
                    OCDList.Add(new ValidateDoctorOnlineConsultaionDetails()
                    {
                       OCD_ID = ID,
                       OCD_MondayEndTime = Ocd.OCD_MondayEndTime == null ? null : Ocd.OCD_MondayEndTime,
                       OCD_MondayStartTime = Ocd.OCD_MondayStartTime == null ? null : Ocd.OCD_MondayStartTime,
                       OCD_TuesdayEndTime = Ocd.OCD_TuesdayEndTime == null ? null : Ocd.OCD_TuesdayEndTime,
                       OCD_TuesdayStartTime = Ocd.OCD_TuesdayStartTime == null ? null : Ocd.OCD_TuesdayStartTime,
                       OCD_WednesdayEndTime = Ocd.OCD_WednesdayEndTime == null ? null : Ocd.OCD_WednesdayEndTime,
                       OCD_WednesdayStartTime = Ocd.OCD_WednesdayStartTime == null ? null : Ocd.OCD_WednesdayStartTime,
                       OCD_ThursdayEndTime = Ocd.OCD_ThursdayEndTime == null ? null : Ocd.OCD_ThursdayEndTime,
                       OCD_ThursdayStartTime = Ocd.OCD_ThursdayStartTime == null ? null : Ocd.OCD_ThursdayStartTime,
                       OCD_FridayEndTime = Ocd.OCD_FridayEndTime == null ? null : Ocd.OCD_FridayEndTime,
                       OCD_FridayStartTime = Ocd.OCD_FridayStartTime == null ? null : Ocd.OCD_FridayStartTime,
                       OCD_SaturdayEndTime = Ocd.OCD_SaturdayEndTime == null ? null : Ocd.OCD_SaturdayEndTime,
                       OCD_SaturdayStartTime = Ocd.OCD_SaturdayStartTime == null ? null : Ocd.OCD_SaturdayStartTime,
                       OCD_SundayEndTime = Ocd.OCD_SundayEndTime == null ? null : Ocd.OCD_SundayEndTime,
                       OCD_SundayStartTime = Ocd.OCD_SundayStartTime == null ? null : Ocd.OCD_SundayStartTime,
                       OCD_Charges = Ocd.OCD_Charges,
                       OCD_IsActive = true,
                    });
                }
            }
            Session["OCDList"] = OCDList;
            return PartialView("_ShowOnlineConsultation", OCDList);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [CustomAuthorize(Roles = "Admin, SuperAdmin, Doctor")]
        public ActionResult UpdateOcd(ValidateDoctorOnlineConsultaionDetails doctor)
        {
            var data = (List<ValidateDoctorOnlineConsultaionDetails>)Session["OCDList"];
            if (data != null && data.Count > 0 && doctor != null)
            {
                var dataUpdate = data.Where(x => x.OCD_ID == doctor.OCD_ID).FirstOrDefault();
                foreach (var item in data)
                {
                    if (item.OCD_ID == doctor.OCD_ID)
                    {
                        item.OCD_ID = doctor.OCD_ID;
                        item.OCD_MondayEndTime = doctor.OCD_MondayEndTime == null ? null : doctor.OCD_MondayEndTime;
                        item.OCD_MondayStartTime = doctor.OCD_MondayStartTime == null ? null : doctor.OCD_MondayStartTime;
                        item.OCD_TuesdayEndTime = doctor.OCD_TuesdayEndTime == null ? null : doctor.OCD_TuesdayEndTime;
                        item.OCD_TuesdayStartTime = doctor.OCD_TuesdayStartTime == null ? null : doctor.OCD_TuesdayStartTime;
                        item.OCD_WednesdayEndTime = doctor.OCD_WednesdayEndTime == null ? null : doctor.OCD_WednesdayEndTime;
                        item.OCD_WednesdayStartTime = doctor.OCD_WednesdayStartTime == null ? null : doctor.OCD_WednesdayStartTime;
                        item.OCD_ThursdayEndTime = doctor.OCD_ThursdayEndTime == null ? null : doctor.OCD_ThursdayEndTime;
                        item.OCD_ThursdayStartTime = doctor.OCD_ThursdayStartTime == null ? null : doctor.OCD_ThursdayStartTime;
                        item.OCD_FridayEndTime = doctor.OCD_FridayEndTime == null ? null : doctor.OCD_FridayEndTime;
                        item.OCD_FridayStartTime = doctor.OCD_FridayStartTime == null ? null : doctor.OCD_FridayStartTime;
                        item.OCD_SaturdayEndTime = doctor.OCD_SaturdayEndTime == null ? null : doctor.OCD_SaturdayEndTime;
                        item.OCD_SaturdayStartTime = doctor.OCD_SaturdayStartTime == null ? null : doctor.OCD_SaturdayStartTime;
                        item.OCD_SundayEndTime = doctor.OCD_SundayEndTime == null ? null : doctor.OCD_SundayEndTime;
                        item.OCD_SundayStartTime = doctor.OCD_SundayStartTime == null ? null : doctor.OCD_SundayStartTime;
                        item.OCD_Charges = doctor.OCD_Charges;
                        item.OCD_IsActive = true;
                    }
                }
            }
            Session["OCDList"] = data;
            return PartialView("_ShowOnlineConsultation", data);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [CustomAuthorize(Roles = "Admin, SuperAdmin, Doctor")]
        public ActionResult DeleteOnlineConsultation(int OcdID)
        {
            var reas = Session["OCDList"];
            var OCDList = new List<ValidateDoctorOnlineConsultaionDetails>();
            if (OcdID <= 0)
            {
                var data = (List<ValidateDoctorOnlineConsultaionDetails>)Session["OCDList"];
                OCDList = data;
                Session["OCDList"] = OCDList;
                return PartialView("_ShowOnlineConsultation", OCDList);
            }
            else
            {
                if (reas == null)
                {
                    return PartialView("_ShowOnlineConsultation", OCDList);
                }
                else
                {
                    var data = (List<ValidateDoctorOnlineConsultaionDetails>)Session["OCDList"];
                    var OcdItem = data.Where(x => x.OCD_ID == OcdID).FirstOrDefault();
                    OcdItem.OCD_IsActive = false;
                    OCDList = data;
                    Session["OCDList"] = OCDList;
                    return PartialView("_ShowOnlineConsultation", OCDList);
                }
            }
        }

        public ActionResult GetDoctorOnlineConsultationInfo(int OcdID)
        {
            if (OcdID > 0)
            {
                var dataOfcd = (List<ValidateDoctorOnlineConsultaionDetails>)Session["OCDList"];
                var OfcdInfo = dataOfcd.Where(x => x.OCD_ID == OcdID).FirstOrDefault();
                if (OfcdInfo != null)
                {
                    var data = new ValidateDoctorOnlineConsultaionDetails()
                    {
                        OCD_ID = OfcdInfo.OCD_ID,
                        OCD_MondayEndTime = OfcdInfo.OCD_MondayEndTime == null ? null : OfcdInfo.OCD_MondayEndTime,
                        OCD_MondayStartTime = OfcdInfo.OCD_MondayStartTime == null ? null : OfcdInfo.OCD_MondayStartTime,
                        OCD_TuesdayEndTime = OfcdInfo.OCD_TuesdayEndTime == null ? null : OfcdInfo.OCD_TuesdayEndTime,
                        OCD_TuesdayStartTime = OfcdInfo.OCD_TuesdayStartTime == null ? null : OfcdInfo.OCD_TuesdayStartTime,
                        OCD_WednesdayEndTime = OfcdInfo.OCD_WednesdayEndTime == null ? null : OfcdInfo.OCD_WednesdayEndTime,
                        OCD_WednesdayStartTime = OfcdInfo.OCD_WednesdayStartTime == null ? null : OfcdInfo.OCD_WednesdayStartTime,
                        OCD_ThursdayEndTime = OfcdInfo.OCD_ThursdayEndTime == null ? null : OfcdInfo.OCD_ThursdayEndTime,
                        OCD_ThursdayStartTime = OfcdInfo.OCD_ThursdayStartTime == null ? null : OfcdInfo.OCD_ThursdayStartTime,
                        OCD_FridayEndTime = OfcdInfo.OCD_FridayEndTime == null ? null : OfcdInfo.OCD_FridayEndTime,
                        OCD_FridayStartTime = OfcdInfo.OCD_FridayStartTime == null ? null : OfcdInfo.OCD_FridayStartTime,
                        OCD_SaturdayEndTime = OfcdInfo.OCD_SaturdayEndTime == null ? null : OfcdInfo.OCD_SaturdayEndTime,
                        OCD_SaturdayStartTime = OfcdInfo.OCD_SaturdayStartTime == null ? null : OfcdInfo.OCD_SaturdayStartTime,
                        OCD_SundayEndTime = OfcdInfo.OCD_SundayEndTime == null ? null : OfcdInfo.OCD_SundayEndTime,
                        OCD_SundayStartTime = OfcdInfo.OCD_SundayStartTime == null ? null : OfcdInfo.OCD_SundayStartTime,
                        OCD_Charges = OfcdInfo.OCD_Charges,
                        OCD_IsActive = true,
                    };
                    return PartialView("_UpdateOnlineConsultation", data);
                }
                else
                    return Json(false, JsonRequestBehavior.AllowGet);
            }
            else
                return Json(false, JsonRequestBehavior.AllowGet);
        }

        public void GetOnlineConsultation(int DoctorID)
        {
            Session["OCDList"] = null;
            var OCDInfo = OcdRepoObj.GetDoctorAllOnlineConsultaionDetailsByID(DoctorID);
            var OCDList = new List<ValidateDoctorOnlineConsultaionDetails>();

            if (OCDInfo != null && OCDInfo.Count() > 0)
            {
                foreach (var item in OCDInfo)
                {
                    OCDList.Add(new ValidateDoctorOnlineConsultaionDetails()
                    {
                       OCD_ID = item.OCD_ID,
                       OCD_MondayEndTime = item.OCD_MondayEndTime == null ? null : item.OCD_MondayEndTime,
                       OCD_MondayStartTime = item.OCD_MondayStartTime == null ? null : item.OCD_MondayStartTime,
                       OCD_TuesdayEndTime = item.OCD_TuesdayEndTime == null ? null : item.OCD_TuesdayEndTime,
                       OCD_TuesdayStartTime = item.OCD_TuesdayStartTime == null ? null : item.OCD_TuesdayStartTime,
                       OCD_WednesdayEndTime = item.OCD_WednesdayEndTime == null ? null : item.OCD_WednesdayEndTime,
                       OCD_WednesdayStartTime = item.OCD_WednesdayStartTime == null ? null : item.OCD_WednesdayStartTime,
                       OCD_ThursdayEndTime = item.OCD_ThursdayEndTime == null ? null : item.OCD_ThursdayEndTime,
                       OCD_ThursdayStartTime = item.OCD_ThursdayStartTime == null ? null : item.OCD_ThursdayStartTime,
                       OCD_FridayEndTime = item.OCD_FridayEndTime == null ? null : item.OCD_FridayEndTime,
                       OCD_FridayStartTime = item.OCD_FridayStartTime == null ? null : item.OCD_FridayStartTime,
                       OCD_SaturdayEndTime = item.OCD_SaturdayEndTime == null ? null : item.OCD_SaturdayEndTime,
                       OCD_SaturdayStartTime = item.OCD_SaturdayStartTime == null ? null : item.OCD_SaturdayStartTime,
                       OCD_SundayEndTime = item.OCD_SundayEndTime == null ? null : item.OCD_SundayEndTime,
                       OCD_SundayStartTime = item.OCD_SundayStartTime == null ? null : item.OCD_SundayStartTime,
                       OCD_Charges = item.OCD_Charges,
                       OCD_IsActive = item.OCD_IsActive,
                    });
                }
            }
            Session["OCDList"] = OCDList;
        }

        public List<ValidateDoctorOnlineConsultaionDetails> GetHospitalOnlineConsultation(int DoctorID)
        {
            var OCDInfo = OcdRepoObj.GetDoctorAllOnlineConsultaionDetailsByID(DoctorID);
            var OCDList = new List<ValidateDoctorOnlineConsultaionDetails>();

            if (OCDInfo != null && OCDInfo.Count() > 0)
            {
                foreach (var item in OCDInfo)
                {
                    OCDList.Add(new ValidateDoctorOnlineConsultaionDetails()
                    {
                        OCD_ID = item.OCD_ID,
                        OCD_MondayEndTime = item.OCD_MondayEndTime == null ? null : item.OCD_MondayEndTime,
                        OCD_MondayStartTime = item.OCD_MondayStartTime == null ? null : item.OCD_MondayStartTime,
                        OCD_TuesdayEndTime = item.OCD_TuesdayEndTime == null ? null : item.OCD_TuesdayEndTime,
                        OCD_TuesdayStartTime = item.OCD_TuesdayStartTime == null ? null : item.OCD_TuesdayStartTime,
                        OCD_WednesdayEndTime = item.OCD_WednesdayEndTime == null ? null : item.OCD_WednesdayEndTime,
                        OCD_WednesdayStartTime = item.OCD_WednesdayStartTime == null ? null : item.OCD_WednesdayStartTime,
                        OCD_ThursdayEndTime = item.OCD_ThursdayEndTime == null ? null : item.OCD_ThursdayEndTime,
                        OCD_ThursdayStartTime = item.OCD_ThursdayStartTime == null ? null : item.OCD_ThursdayStartTime,
                        OCD_FridayEndTime = item.OCD_FridayEndTime == null ? null : item.OCD_FridayEndTime,
                        OCD_FridayStartTime = item.OCD_FridayStartTime == null ? null : item.OCD_FridayStartTime,
                        OCD_SaturdayEndTime = item.OCD_SaturdayEndTime == null ? null : item.OCD_SaturdayEndTime,
                        OCD_SaturdayStartTime = item.OCD_SaturdayStartTime == null ? null : item.OCD_SaturdayStartTime,
                        OCD_SundayEndTime = item.OCD_SundayEndTime == null ? null : item.OCD_SundayEndTime,
                        OCD_SundayStartTime = item.OCD_SundayStartTime == null ? null : item.OCD_SundayStartTime,
                        OCD_Charges = item.OCD_Charges,
                        OCD_IsActive = item.OCD_IsActive,
                    });
                }
            }
            return OCDList;
        }

        public bool DeleteDoctorOCD(List<ValidateDoctorOnlineConsultaionDetails> OCDLst)
        {
            try
            {
                if (OCDLst != null && OCDLst.Count > 0)
                {
                    int NoOfReas = 0;
                    foreach (var item in OCDLst)
                    {
                        bool reas = OcdRepoObj.InActiveOnlineConsultaionDetail(item.OCD_ID);
                        if (reas)
                            NoOfReas++;
                        else
                            NoOfReas += 0;
                    }
                    if (NoOfReas == OCDLst.Count)
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

        #endregion

        #region **View Doctor Profile**

        [AcceptVerbs(HttpVerbs.Get)]
        [CustomAuthorize(Roles = "Admin, SuperAdmin, Doctor")]
        public ActionResult DoctorProfile(int DoctorID)
        {
            tblDoctor reas = DoctorsRepoObj.GetDoctorByID(DoctorID);
            IEnumerable<tblOfflineConsultaionDetail> ofcdDetails = OfcdRepoObj.GetDoctorAllOfflineConsultaionDetailsByID(DoctorID);
            IEnumerable<tblOnlineConsultaionDetail> ocdDetails = OcdRepoObj.GetDoctorAllOnlineConsultaionDetailsByID(DoctorID);
            IEnumerable<tblDoctorWorkExperience> WexDetails = WexRepoObj.GetDoctorAllWorkExperiencesByID(DoctorID);
            IEnumerable<string> serviceDetails = servicesRepoObj.GetAllDoctorServicesByID(DoctorID);
            DoctorProfileView doctorProfile = new DoctorProfileView()
            {
                Profile = reas,
                Experience = WexDetails,
                Services = serviceDetails,
                OnlineConsultation = ocdDetails,
                OfflineConsultation = ofcdDetails,
            };
            return View(doctorProfile);
        }

        #endregion

        #region **Doctor Profile Setting**

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetTempDataS()
        {
            var reas = TempData["MsgPS"];
            if (reas != null)
                return Json(reas, JsonRequestBehavior.AllowGet);
            else
                return Json(null, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        [CustomAuthorize(Roles = "Admin, SuperAdmin, Doctor")]
        public ActionResult Settings(int DoctorID)
        {
            try
            {
                Session["SettingsPage"] = true;
                tblDoctor reas = DoctorsRepoObj.GetDoctorByID(DoctorID);
                var reas1 = DoctorsRepoObj.GetUserDetailById(DoctorID);
                GetExperienceInfo(reas.D_ID);
                GetOfflineConsultation(reas.D_ID);
                GetOnlineConsultation(reas.D_ID);
                ValidateDoctor doctor = new ValidateDoctor()
                {
                    AreaID = reas.tblAddress.AddressZone.Value,
                    StateID = reas.tblAddress.AddressState.Value,
                    CityID = reas.tblAddress.AddressCity.Value,
                    CompleteAddress = reas.tblAddress.AddressComplete,
                    UserID = reas.D_ID,
                    ConfirmPassowrd = reas1.UserPassword,
                    UserPassword = reas1.UserPassword,
                    DoctorWorkPhoneNumber = reas.D_WorkPhoneNumber,
                    UserFirstName = reas.D_FirstName,
                    UserLastName = reas.D_LastName,
                    Gender = reas.D_Gender == "Male" ? "1" : "2",
                    UserProfileImage = reas.D_ProfileImage,
                    DoctorAboutMe = reas.D_AboutMe,
                    DoctorYearsOfExperience = reas.D_YearsOfExperience,
                    DoctorAwardsAndAchievements = reas.D_AwardsAndAchievements,
                    DoctorResponseTime = reas.D_ResponseTime,
                    DoctorSpecialization = reas.D_Specialization,
                    UserUpdateEmail = reas.D_Email,
                    UserUpdatePhoneNumber = reas.D_PhoneNumber,
                    D_IsProfileCompleted = reas.D_IsProfileCompleted,
                };
                ViewBag.State = AddressRepoObj.GetAllStateDropdown();
                ViewBag.City = AddressRepoObj.GetAllCityByStateDropdown(doctor.StateID);
                ViewBag.Zone = AddressRepoObj.GetAllZoneByCityDropdown(doctor.CityID);
                Session["DoctorID"] = doctor.UserID;
                return View(doctor);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [CustomAuthorize(Roles = "Admin, SuperAdmin, Doctor")]
        public ActionResult Settings(HttpPostedFileBase file, ValidateDoctor doctor)
        {
            try
            {
                if (file != null)
                {
                    string _filename = DateTime.Now.ToString("yymmssfff") + Path.GetFileName(file.FileName);
                    string path = Path.Combine(Server.MapPath("~/uploads/DoctorsProfileImage/"), _filename);
                    doctor.UserProfileImage = "~/uploads/DoctorsProfileImage/" + _filename;
                }
                else
                {
                    doctor.UserProfileImage = Session["UserImage"].ToString();
                }
                doctor.UserID = int.Parse(Session["DoctorID"].ToString());
                var Docreas = DoctorsRepoObj.GetUserDetailById(doctor.UserID);

                doctor.UserOTP = null;
                doctor.UserID = Docreas.UserID;
                doctor.D_IsProfileCompleted = true;
                doctor.UserUpdatedBy = Docreas.UserID;
                doctor.tblAddress = Docreas.tblAddress;
                doctor.UserEmail = doctor.UserUpdateEmail;
                doctor.UserProfileImage = doctor.UserProfileImage;
                doctor.UserPhoneNumber = doctor.UserUpdatePhoneNumber;
                doctor.Gender = doctor.Gender == "1" ? "Male" : "Female";
                var ExpList = Session["ExperienceList"];
                var OFCDList = Session["OFCDList"];
                var OCDList = Session["OCDList"];
                if (ExpList != null)
                {
                    var data = (List<ValidateDoctorHospitalInfo>)Session["ExperienceList"];
                    var originalData = GetHospitalExperienceInfo(doctor.UserID);
                    var dataForDelete = data.Where(x => x.IsActive == false).ToList();
                    var allData = data.Where(x => x.IsActive == true).ToList();
                    var dataForUpdate = new List<ValidateDoctorHospitalInfo>();
                    foreach (var item in originalData)
                    {
                        var Ischeck = allData.Where(x => x.HospitalID == item.HospitalID).FirstOrDefault();
                        if (Ischeck != null)
                        {
                            allData.Remove(Ischeck);
                            dataForUpdate.Add(Ischeck);
                        }
                    }
                    var dataForInsert = allData;
                    if (dataForDelete.Count > 0 && dataForDelete != null)
                    {
                        var itemToDelete = new List<ValidateDoctorHospitalInfo>();
                        foreach (var item in dataForDelete)
                        {
                            var CheckDataIsExist = WexRepoObj.GetDoctorWorkExperienceByID(item.HospitalID);
                            if (CheckDataIsExist != null)
                            {
                                itemToDelete.Add(item);
                            }
                        }
                        if (itemToDelete.Count > 0 && itemToDelete != null)
                        {
                            DeleteDoctorExp(itemToDelete);
                        }
                    }
                    if (dataForInsert.Count > 0 && dataForInsert != null)
                    {
                        ValidateDoctorWorkExperience experience = new ValidateDoctorWorkExperience()
                        {
                            WEX_ID = 0,
                            WEX_DoctorID = doctor.UserID,
                            HospitalInfos = dataForInsert,
                            WEX_CreatedBy = doctor.UserID,
                        };
                        WexRepoObj.InsertDoctorWorkExperience(experience);
                    }
                    if (dataForUpdate.Count > 0 && dataForUpdate != null)
                    {
                        ValidateDoctorWorkExperience experience = new ValidateDoctorWorkExperience()
                        {
                            WEX_DoctorID = doctor.UserID,
                            HospitalInfos = dataForUpdate,
                            WEX_UpdatedBy = doctor.UserID,
                        };
                        WexRepoObj.UpdateDoctorWorkExperience(experience);
                    }
                }
                if (OFCDList != null)
                {
                    var data = (List<ValidateDoctorOfflineConsultaionDetails>)Session["OFCDList"];
                    var originalData = GetHospitalOfflineConsultation(doctor.UserID);
                    var dataForDelete = data.Where(x => x.OFCD_IsActive == false).ToList();
                    var allData = data.Where(x => x.OFCD_IsActive == true).ToList();
                    var dataForUpdate = new List<ValidateDoctorOfflineConsultaionDetails>();
                    foreach (var item in originalData)
                    {
                        var Ischeck = allData.Where(x => x.OFCD_ID == item.OFCD_ID).FirstOrDefault();
                        if (Ischeck != null)
                        {
                            allData.Remove(Ischeck);
                            dataForUpdate.Add(Ischeck);
                        }
                    }
                    var dataForInsert = allData;
                    if (dataForDelete.Count > 0 && dataForDelete != null)
                    {
                        var itemToDelete = new List<ValidateDoctorOfflineConsultaionDetails>();
                        foreach (var item in dataForDelete)
                        {
                            var CheckDataIsExist = OfcdRepoObj.GetOfflineConsultaionDetailByID(item.OFCD_ID);
                            if (CheckDataIsExist != null)
                            {
                                itemToDelete.Add(item);
                            }
                        }
                        if (itemToDelete.Count > 0 && itemToDelete != null)
                        {
                            DeleteDoctorOFCD(itemToDelete);
                        }
                    }
                    if (dataForInsert.Count > 0 && dataForInsert != null)
                    {
                        foreach (var item in dataForInsert)
                        {
                            item.OFCD_CreatedBy = doctor.UserID;
                            item.OFCD_DoctorID = doctor.UserID;
                            OfcdRepoObj.InsertOfflineConsultaionDetail(item);    
                        }
                    }
                    if (dataForUpdate.Count > 0 && dataForUpdate != null)
                    {
                        foreach (var item in dataForUpdate)
                        {
                            item.OFCD_UpdatedBy = doctor.UserID;
                            item.OFCD_DoctorID = doctor.UserID;
                            OfcdRepoObj.UpdateOfflineConsultaionDetail(item);
                        }
                    }
                }
                if (OCDList != null)
                {
                    var data = (List<ValidateDoctorOnlineConsultaionDetails>)Session["OCDList"];
                    var originalData = GetHospitalOnlineConsultation(doctor.UserID);
                    var dataForDelete = data.Where(x => x.OCD_IsActive == false).ToList();
                    var allData = data.Where(x => x.OCD_IsActive == true).ToList();
                    var dataForUpdate = new List<ValidateDoctorOnlineConsultaionDetails>();
                    foreach (var item in originalData)
                    {
                        var Ischeck = allData.Where(x => x.OCD_ID == item.OCD_ID).FirstOrDefault();
                        if (Ischeck != null)
                        {
                            allData.Remove(Ischeck);
                            dataForUpdate.Add(Ischeck);
                        }
                    }
                    var dataForInsert = allData;
                    if (dataForDelete.Count > 0 && dataForDelete != null)
                    {
                        var itemToDelete = new List<ValidateDoctorOnlineConsultaionDetails>();
                        foreach (var item in dataForDelete)
                        {
                            var CheckDataIsExist = OcdRepoObj.GetOnlineConsultaionDetailByID(item.OCD_ID);
                            if (CheckDataIsExist != null)
                            {
                                itemToDelete.Add(item);
                            }
                        }
                        if (itemToDelete.Count > 0 && itemToDelete != null)
                        {
                            DeleteDoctorOCD(itemToDelete);
                        }
                    }
                    if (dataForInsert.Count > 0 && dataForInsert != null)
                    {
                        foreach (var item in dataForInsert)
                        {
                            item.OCD_CreatedBy = doctor.UserID;
                            item.OCD_DoctorID = doctor.UserID;
                            OcdRepoObj.InsertOnlineConsultaionDetail(item);
                        }
                    }
                    if (dataForUpdate.Count > 0 && dataForUpdate != null)
                    {
                        foreach (var item in dataForUpdate)
                        {
                            item.OCD_UpdatedBy = doctor.UserID;
                            item.OCD_DoctorID = doctor.UserID;
                            OcdRepoObj.UpdateOnlineConsultaionDetail(item);
                        }
                    }
                }
                if (doctor != null)
                {
                    var reas = DoctorsRepoObj.UpdateDoctor(doctor);
                    if (reas == 1)
                    {
                        Session["Email"] = doctor.UserUpdateEmail;
                        Session["UserImage"] = doctor.UserProfileImage;
                        Session["PhoneNumber"] = doctor.UserUpdatePhoneNumber;
                        TempData["SuccessMsg"] = "Your profile is completed successfully!";
                        TempData["MsgPS"] = "1";
                        Session["SettingsPage"] = false;
                        FormsAuthentication.SetAuthCookie(doctor.UserUpdateEmail, false);
                    }
                    else if (reas == -1)
                    {
                        TempData["ErrorMsg"] = "Email already exists. Please ensure to enter not used email account again!";
                        TempData["MsgPS"] = "5";
                    }
                    else if (reas == -2)
                    {
                        TempData["ErrorMsg"] = "Phone Number already exists. Please ensure to enter not used phone number again!";
                        TempData["MsgPS"] = "2";
                    }
                    else
                    {
                        TempData["ErrorMsg"] = "Error on updating profile. Please try again later!";
                        TempData["MsgPS"] = "3";
                    }
                }
                else
                {
                    TempData["ErrorMsg"] = "Error on updating profile!";
                    TempData["MsgPS"] = "4";
                    return RedirectToAction("Settings", new { DoctorID = int.Parse(Session["UserID"].ToString()) });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return RedirectToAction("Settings", new { DoctorID = int.Parse(Session["UserID"].ToString()) });
        }

        #endregion

    }
}