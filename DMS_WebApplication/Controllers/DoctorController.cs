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
        private DoctorOfflineConsultaionDetailsRepo OfcdRepoObj;
        private DoctorWorkExperienceRepo WexRepoObj;

        public DoctorController()
        {
            _httpClient = new HttpClient();
            AddressRepoObj = new AddressRepo();
            DoctorsRepoObj = new DoctorsRepo();
            servicesRepoObj = new DoctorServicesRepo();
            WexRepoObj = new DoctorWorkExperienceRepo();
            OfcdRepoObj = new DoctorOfflineConsultaionDetailsRepo();
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

        public static IEnumerable<KeyValuePair<string, string>> ObjectToKeyValuePairs(object obj)
        {
            var properties = obj.GetType().GetProperties();

            foreach (var property in properties)
            {
                yield return new KeyValuePair<string, string>(property.Name, property.GetValue(obj)?.ToString());
            }
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
                    var reas = DoctorsRepoObj.UpdateDoctor(validateDoctor);
                    if (reas == 1)
                    {
                        TempData["SuccessMsg"] = "Your profile is completed successfully!";
                        TempData["SuccessP"] = "1";
                    }
                    else if (reas == -1)
                    {
                        TempData["ErrorMsg"] = "Email already exists. Please ensure to enter not used email account again!";
                        TempData["ErrorP"] = "0";
                    }
                    else if (reas == -2)
                    {
                        TempData["ErrorMsg"] = "Phone Number already exists. Please ensure to enter not used phone number again!";
                        TempData["ErrorP"] = "2";
                    }
                    else
                    {
                        TempData["ErrorMsg"] = "Error on completing profile. Please try again later!";
                        TempData["ErrorP"] = "3";
                    }
                }
                else
                {
                    TempData["ErrorMsg"] = "Error on completing profile!";
                    TempData["ErrorP"] = "4";
                    return RedirectToAction("ProfileComplete");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return RedirectToAction("Index", "Account");
        }

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
                        HospitalID = ID,
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
                    data.Remove(ExpItem);
                    ExperienceList = data;
                    Session["ExperienceList"] = ExperienceList;
                    return PartialView("_ShowExperience", ExperienceList);
                }
            }
        }

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
                    CompleteAddress = ofcd.CompleteAddress
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
                        CompleteAddress = ofcd.CompleteAddress
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
                        CompleteAddress = ofcd.CompleteAddress
                    });
                }
            }
            Session["OFCDList"] = OFCDList;
            return PartialView("_ShowOfflineConsultation", OFCDList);
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
                    var ExpItem = data.Where(x => x.OFCD_ID == OfcdID).FirstOrDefault();
                    data.Remove(ExpItem);
                    OFCDList = data;
                    Session["OFCDList"] = OFCDList;
                    return PartialView("_ShowOfflineConsultation", OFCDList);
                }
            }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        [CustomAuthorize(Roles = "Admin, SuperAdmin, Doctor")]
        public ActionResult DoctorProfile(int DoctorID)
        {
            tblDoctor reas = DoctorsRepoObj.GetDoctorByID(DoctorID);
            IEnumerable<tblOfflineConsultaionDetail> ofcdDetails = OfcdRepoObj.GetDoctorAllOfflineConsultaionDetailsByID(DoctorID);
            IEnumerable<tblDoctorWorkExperience> WexDetails = WexRepoObj.GetDoctorAllWorkExperiencesByID(DoctorID);
            IEnumerable<string> serviceDetails = servicesRepoObj.GetAllDoctorServicesByID(DoctorID);
            DoctorProfileView doctorProfile = new DoctorProfileView()
            {
                Experience = WexDetails,
                OfflineConsultation = ofcdDetails,
                Profile = reas,
                Services = serviceDetails
            };
            return View(doctorProfile);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        [CustomAuthorize(Roles = "Admin, SuperAdmin, Doctor")]
        public ActionResult Settings(int DoctorID)
        {
            try
            {
                tblDoctor reas = DoctorsRepoObj.GetDoctorByID(DoctorID);
                var reas1 = DoctorsRepoObj.GetUserDetailById(DoctorID);
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

                if (doctor != null)
                {
                    var reas = DoctorsRepoObj.UpdateDoctor(doctor);
                    if (reas == 1)
                    {
                        Session["Email"] = doctor.UserUpdateEmail;
                        Session["UserImage"] = doctor.UserProfileImage;
                        Session["PhoneNumber"] = doctor.UserUpdatePhoneNumber;
                        TempData["SuccessMsg"] = "Your profile is completed successfully!";
                        FormsAuthentication.SetAuthCookie(doctor.UserUpdateEmail, false);
                    }
                    else if (reas == -1)
                    {
                        TempData["ErrorMsg"] = "Email already exists. Please ensure to enter not used email account again!";
                    }
                    else if (reas == -2)
                    {
                        TempData["ErrorMsg"] = "Phone Number already exists. Please ensure to enter not used phone number again!";
                    }
                    else
                    {
                        TempData["ErrorMsg"] = "Error on completing profile. Please try again later!";
                    }
                }
                else
                {
                    TempData["ErrorMsg"] = "Error on completing profile!";
                    return RedirectToAction("Settings", new { DoctorID = int.Parse(Session["UserID"].ToString()) });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return RedirectToAction("Settings", new { DoctorID = int.Parse(Session["UserID"].ToString()) });
        }

    }
}