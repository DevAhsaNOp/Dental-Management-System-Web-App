﻿using DMS_BLL.Repositories;
using DMS_BOL;
using DMS_BOL.Validation_Classes;
using DMS_WebApplication.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace DMS_WebApplication.Controllers
{
    public class AdminController : Controller
    {
        private UsersRepo UserRepoObj;
        private AddressRepo AddressRepoObj;
        private DoctorsRepo DoctorsRepoObj;
        private PatientTestRepo patientTestRepoObj;
        private DoctorServicesRepo servicesRepoObj;
        private DoctorWorkExperienceRepo WexRepoObj;
        private DoctorOfflineConsultaionDetailsRepo OfcdRepoObj;
        private DoctorOnlineConsultaionDetailsRepo OcdRepoObj;
        private AdminsRepo adminRepoObj;

        public AdminController()
        {
            UserRepoObj = new UsersRepo();
            adminRepoObj = new AdminsRepo();
            AddressRepoObj = new AddressRepo();
            DoctorsRepoObj = new DoctorsRepo();
            patientTestRepoObj = new PatientTestRepo();
            servicesRepoObj = new DoctorServicesRepo();
            WexRepoObj = new DoctorWorkExperienceRepo();
            OfcdRepoObj = new DoctorOfflineConsultaionDetailsRepo();
            OcdRepoObj = new DoctorOnlineConsultaionDetailsRepo();
        }


        #region **Manage Doctor**

        #region **View All Doctors**

        [CustomAuthorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Doctors()
        {
            var reas = DoctorsRepoObj.GetAllDoctors();
            return View(reas);
        }

        #endregion

        #region **Add Doctor**

        [AcceptVerbs(HttpVerbs.Get)]
        [CustomAuthorize(Roles = "Admin, SuperAdmin")]
        public ActionResult AddDoctor()
        {
            var AllStates = AddressRepoObj.GetAllState();
            var states = new List<SelectListItem>();
            foreach (var item in AllStates)
            {
                states.Add(new SelectListItem() { Text = item.StateName, Value = item.StateID.ToString() });
            }
            ViewBag.State = states;
            ValidateDoctor doctor = new ValidateDoctor();
            Session["ImageAvatar"] = "~/uploads/DoctorsProfileImage/nophoto.png";
            return View(doctor);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [CustomAuthorize(Roles = "Admin, SuperAdmin")]
        public ActionResult AddDoctor(HttpPostedFileBase file, ValidateDoctor users)
        {
            try
            {
                if (file != null)
                {
                    string _filename = DateTime.Now.ToString("yymmssfff") + Path.GetFileName(file.FileName);
                    string path = Path.Combine(Server.MapPath("~/uploads/DoctorsProfileImage/"), _filename);
                    users.UserProfileImage = "~/uploads/DoctorsProfileImage/" + _filename;
                    file.SaveAs(path);
                }
                else
                {
                    users.UserProfileImage = "~/uploads/DoctorsProfileImage/nophoto.png";
                }
                if (users != null)
                {
                    users.StateID = int.Parse(users.State);
                    users.CityID = int.Parse(users.City);
                    users.AreaID = int.Parse(users.Area);
                    users.UserCreatedBy = 1;
                    users.UserUpdatedBy = 0;
                    users.Gender = users.Gender == "1" ? "Male" : "Female";
                    ModelState.Clear();
                    var StatusCode = DoctorsRepoObj.InsertDoctor(users);
                    if (StatusCode == 1)
                        TempData["SuccessMsg"] = "Account Created Successfully!";
                    else if (StatusCode == -1)
                        TempData["ErrorMsg"] = "Invalid OTP provided. Please ensure to enter correct OTP again!";
                    else if (StatusCode == -2)
                        TempData["ErrorMsg"] = "Email already exists. Please ensure to enter not used email account again!";
                    else if (StatusCode == -3)
                        TempData["ErrorMsg"] = "Phone Number already exists. Please ensure to enter not used phone number again!";
                    else
                        TempData["ErrorMsg"] = "Error occured on creating Account. Please try again later!";
                    return RedirectToAction("AddDoctor");
                }
                else
                {
                    TempData["ErrorMsg"] = "Error on uploading Image!";
                    return RedirectToAction("AddDoctor");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region **Edit Doctor**

        [AcceptVerbs(HttpVerbs.Get)]
        [CustomAuthorize(Roles = "Admin, SuperAdmin")]
        public ActionResult EditDoctor(int DoctorID)
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
                Session["AdminDoctorID"] = doctor.UserID;
                Session["DoctorImage"] = reas.D_ProfileImage;
                return View(doctor);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [CustomAuthorize(Roles = "Admin, SuperAdmin")]
        public ActionResult EditDoctor(HttpPostedFileBase file, ValidateDoctor doctor)
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
                    doctor.UserProfileImage = Session["DoctorImage"].ToString();
                }
                doctor.UserID = int.Parse(Session["AdminDoctorID"].ToString());
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
                        TempData["SuccessMsg"] = "Your profile is completed successfully!";
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
                        TempData["ErrorMsg"] = "Error on updating profile. Please try again later!";
                    }
                }
                else
                {
                    TempData["ErrorMsg"] = "Error on updating profile!";
                    return RedirectToAction("EditDoctor", new { DoctorID = doctor.UserID });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return RedirectToAction("EditDoctor", new { DoctorID = doctor.UserID });
        }

        #endregion

        #region **Doctor Details**

        [CustomAuthorize(Roles = "Admin, SuperAdmin")]
        public ActionResult DoctorDetails(int DoctorID)
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

        #endregion

        #region **Manage Patients**

        #region **Add Patients**

        [AcceptVerbs(HttpVerbs.Get)]
        [CustomAuthorize(Roles = "Admin, SuperAdmin")]
        public ActionResult AddPatient()
        {
            var AllStates = AddressRepoObj.GetAllState();
            var states = new List<SelectListItem>();
            foreach (var item in AllStates)
            {
                states.Add(new SelectListItem() { Text = item.StateName, Value = item.StateID.ToString() });
            }
            ViewBag.State = states;
            ValidatePatient patient = new ValidatePatient();
            Session["PatientImageAvatar"] = "~/uploads/PatientsProfileImage/nophoto.png";
            return View(patient);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [CustomAuthorize(Roles = "Admin, SuperAdmin")]
        public ActionResult AddPatient(HttpPostedFileBase file, ValidatePatient users)
        {
            try
            {
                if (file != null)
                {
                    string _filename = DateTime.Now.ToString("yymmssfff") + Path.GetFileName(file.FileName);
                    string path = Path.Combine(Server.MapPath("~/uploads/PatientsProfileImage/"), _filename);
                    users.UserProfileImage = "~/uploads/PatientsProfileImage/" + _filename;
                }
                else
                {
                    users.UserProfileImage = "~/uploads/PatientsProfileImage/nophoto.png";
                }
                if (users != null)
                {
                    users.StateID = int.Parse(users.State);
                    users.CityID = int.Parse(users.City);
                    users.AreaID = int.Parse(users.Area);
                    users.UserCreatedBy = 1;
                    users.UserUpdatedBy = 0;
                    users.Gender = users.Gender == "1" ? "Male" : "Female";
                    ModelState.Clear();
                    var StatusCode = UserRepoObj.InsertPatient(users);
                    if (StatusCode == 1)
                        TempData["SuccessMsg"] = "Patient account Created Successfully!";
                    else if (StatusCode == -1)
                        TempData["ErrorMsg"] = "Invalid OTP provided. Please ensure to enter correct OTP again!";
                    else if (StatusCode == -2)
                        TempData["ErrorMsg"] = "Email already exists. Please ensure to enter not used email account again!";
                    else if (StatusCode == -3)
                        TempData["ErrorMsg"] = "Phone Number already exists. Please ensure to enter not used phone number again!";
                    else
                        TempData["ErrorMsg"] = "Error occured on creating Account. Please try again later!";
                }
                else
                {
                    TempData["ErrorMsg"] = "Error on uploading Image!";
                    return RedirectToAction("AddPatient");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return RedirectToAction("AddPatient");
        }

        #endregion

        #region **Edit Patient**

        [AcceptVerbs(HttpVerbs.Get)]
        [CustomAuthorize(Roles = "Admin, SuperAdmin")]
        public ActionResult EditPatient(int PatientID)
        {
            try
            {
                tblPatient reas = UserRepoObj.GetPatientByID(PatientID);
                var reas1 = UserRepoObj.GetUserDetailById(PatientID);
                ValidatePatient patient = new ValidatePatient()
                {
                    AreaID = reas.tblAddress.AddressZone.Value,
                    StateID = reas.tblAddress.AddressState.Value,
                    CityID = reas.tblAddress.AddressCity.Value,
                    CompleteAddress = reas.tblAddress.AddressComplete,
                    UserID = reas.P_ID,
                    ConfirmPassowrd = reas1.UserPassword,
                    UserPassword = reas1.UserPassword,
                    UserFirstName = reas.P_FirstName,
                    UserLastName = reas.P_LastName,
                    Gender = reas.P_Gender == "Male" ? "1" : "2",
                    UserProfileImage = reas.P_ProfileImage,
                    UserUpdateEmail = reas.P_Email,
                    UserUpdatePhoneNumber = reas.P_PhoneNumber,
                    IsProfileCompleted = true,
                };
                ViewBag.State = AddressRepoObj.GetAllStateDropdown();
                ViewBag.City = AddressRepoObj.GetAllCityByStateDropdown(patient.StateID);
                ViewBag.Zone = AddressRepoObj.GetAllZoneByCityDropdown(patient.CityID);
                Session["AdminPatientID"] = patient.UserID;
                Session["PatientImage"] = reas.P_ProfileImage;
                return View(patient);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [CustomAuthorize(Roles = "Admin, SuperAdmin")]
        public ActionResult EditPatient(HttpPostedFileBase file, ValidatePatient patient)
        {
            try
            {
                if (file != null)
                {
                    string _filename = DateTime.Now.ToString("yymmssfff") + Path.GetFileName(file.FileName);
                    string path = Path.Combine(Server.MapPath("~/uploads/PatientsProfileImage/"), _filename);
                    patient.UserProfileImage = "~/uploads/PatientsProfileImage/" + _filename;
                }
                else
                {
                    patient.UserProfileImage = Session["PatientImage"].ToString();
                }
                patient.UserID = int.Parse(Session["AdminPatientID"].ToString());
                var Docreas = UserRepoObj.GetUserDetailById(patient.UserID);
                patient.UserOTP = null;
                patient.UserID = Docreas.UserID;
                patient.IsProfileCompleted = true;
                patient.UserUpdatedBy = Docreas.UserID;
                patient.tblAddress = Docreas.tblAddress;
                patient.UserEmail = patient.UserUpdateEmail;
                patient.UserProfileImage = patient.UserProfileImage;
                patient.UserPhoneNumber = patient.UserUpdatePhoneNumber;
                patient.Gender = patient.Gender == "1" ? "Male" : "Female";
                if (patient != null)
                {
                    var reas = UserRepoObj.UpdatePatient(patient);
                    if (reas == 1)
                    {
                        TempData["SuccessMsg"] = "Your profile is completed successfully!";
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
                        TempData["ErrorMsg"] = "Error on updating profile. Please try again later!";
                    }
                }
                else
                {
                    TempData["ErrorMsg"] = "Error on updating profile!";
                    return RedirectToAction("EditPatient", new { PatientID = patient.UserID });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return RedirectToAction("EditPatient", new { PatientID = patient.UserID });
        }

        #endregion

        #region **View All Patients**

        [AcceptVerbs(HttpVerbs.Get)]
        [CustomAuthorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Patients()
        {
            IEnumerable<tblPatient> patient = UserRepoObj.GetAllPatient();
            return View(patient);
        }

        #endregion

        #region **View Patient**

        [AcceptVerbs(HttpVerbs.Get)]
        [CustomAuthorize(Roles = "Admin, SuperAdmin")]
        public ActionResult PatientDetails(int PatientID)
        {
            var reas = UserRepoObj.GetPatientByID(PatientID);
            if (reas != null)
            {
                var addressInfo = AddressRepoObj.GetAddressById(reas.P_AddressID.Value);
                var patientInfo = new ValidatePatient()
                {
                    UserID = reas.P_ID,
                    UserFirstName = reas.P_FirstName,
                    UserLastName = reas.P_LastName,
                    UserPhoneNumber = reas.P_PhoneNumber,
                    UserEmail = reas.P_Email,
                    Gender = reas.P_Gender,
                    UserAddressID = reas.P_AddressID.Value,
                    UserRoleID = reas.P_RoleID.Value,
                    UserProfileImage = reas.P_ProfileImage,
                    State = addressInfo.State,
                    City = addressInfo.City,
                    Area = addressInfo.Area,
                    CompleteAddress = addressInfo.CompleteAddress,
                };
                return View(patientInfo);
            }
            else
                return RedirectToAction("Index", "Account");
        }

        #endregion

        #endregion

        #region **Manage Settings**

        [AcceptVerbs(HttpVerbs.Get)]
        [CustomAuthorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Settings(int UserID)
        {
            try
            {
                var reas = adminRepoObj.GetAdminByID(UserID);
                var reas1 = adminRepoObj.GetUserDetailById(UserID);
                ValidateAdmin admin = new ValidateAdmin()
                {
                    AreaID = reas.tblAddress.AddressZone.Value,
                    StateID = reas.tblAddress.AddressState.Value,
                    CityID = reas.tblAddress.AddressCity.Value,
                    CompleteAddress = reas.tblAddress.AddressComplete,
                    UserID = reas.A_ID,
                    ConfirmPassowrd = reas1.UserPassword,
                    UserPassword = reas1.UserPassword,
                    UserFirstName = reas.A_FirstName,
                    UserLastName = reas.A_LastName,
                    Gender = reas.A_Gender == "Male" ? "1" : "2",
                    UserProfileImage = reas.A_ProfileImage,
                    UserUpdateEmail = reas.A_Email,
                    UserUpdatePhoneNumber = reas.A_PhoneNumber,
                    IsProfileCompleted = true,
                };
                ViewBag.State = AddressRepoObj.GetAllStateDropdown();
                ViewBag.City = AddressRepoObj.GetAllCityByStateDropdown(admin.StateID);
                ViewBag.Zone = AddressRepoObj.GetAllZoneByCityDropdown(admin.CityID);
                Session["AdminUserID"] = admin.UserID;
                Session["AdminImage"] = reas.A_ProfileImage;
                return View(admin);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [CustomAuthorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Settings(HttpPostedFileBase file, ValidateAdmin admin)
        {
            try
            {
                if (file != null)
                {
                    string _filename = DateTime.Now.ToString("yymmssfff") + Path.GetFileName(file.FileName);
                    string path = Path.Combine(Server.MapPath("~/uploads/AdminsProfileImage/"), _filename);
                    admin.UserProfileImage = "~/uploads/AdminsProfileImage/" + _filename;
                }
                else
                {
                    admin.UserProfileImage = Session["AdminImage"].ToString();
                }
                admin.UserID = int.Parse(Session["AdminUserID"].ToString());
                var Docreas = DoctorsRepoObj.GetUserDetailById(admin.UserID);
                admin.UserOTP = null;
                admin.UserID = Docreas.UserID;
                admin.IsProfileCompleted = true;
                admin.UserUpdatedBy = Docreas.UserID;
                admin.tblAddress = Docreas.tblAddress;
                admin.UserEmail = admin.UserUpdateEmail;
                admin.UserProfileImage = admin.UserProfileImage;
                admin.UserPhoneNumber = admin.UserUpdatePhoneNumber;
                admin.Gender = admin.Gender == "1" ? "Male" : "Female";
                if (admin != null)
                {
                    var reas = adminRepoObj.UpdateAdmin(admin);
                    if (reas == 1)
                    {
                        TempData["SuccessMsg"] = "Your profile is updated successfully!";
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
                        TempData["ErrorMsg"] = "Error on updating profile. Please try again later!";
                    }
                }
                else
                {
                    TempData["ErrorMsg"] = "Error on updating profile!";
                    return RedirectToAction("Settings", new { UserID = admin.UserID });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return RedirectToAction("Settings", new { UserID = admin.UserID });
        }

        #endregion
    }
}