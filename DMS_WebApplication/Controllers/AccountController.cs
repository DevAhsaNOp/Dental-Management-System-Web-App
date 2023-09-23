using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using System.Web.Security;
using DMS_BLL.Repositories;
using DMS_BOL.Validation_Classes;
using System.Collections.Generic;
using DMS_WebApplication.Models;

namespace DMS_WebApplication.Controllers
{
    [SessionExpire]
    public class AccountController : Controller
    {
        private HttpClient _httpClient;
        private DoctorsRepo DoctorRepoObj;
        private UsersRepo UserRepoObj;
        private AddressRepo AddressRepoObj;
        private DoctorApprovalRepo DARepoObj;

        public AccountController()
        {
            _httpClient = new HttpClient();
            DoctorRepoObj = new DoctorsRepo();
            UserRepoObj = new UsersRepo();
            AddressRepoObj = new AddressRepo();
            DARepoObj = new DoctorApprovalRepo();
        }

        [CustomAuthorize]
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult Unauthorized()
        {
            return View();
        }

        [CustomAuthorize(Roles = "Admin, SuperAdmin")]
        public ActionResult About()
        {
            return View();
        }

        [CustomAuthorize(Roles = "Admin, SuperAdmin, Doctor")]
        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            Session.Clear();
            Session.RemoveAll();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            return RedirectToAction("SignIn");
        }

        public JsonResult IsEmailExist(string UserEmail)
        {
            return Json(CheckEmailExist(UserEmail), JsonRequestBehavior.AllowGet);
        }
        
        
        public JsonResult IsEmailExistForResetPass(string UserEmailForResetPass)
        {
            return Json(!CheckEmailExist(UserEmailForResetPass), JsonRequestBehavior.AllowGet);
        }

        public JsonResult IsPhoneNumberExist(string UserPhoneNumber)
        {
            return Json(CheckPhoneNumberExist(UserPhoneNumber), JsonRequestBehavior.AllowGet);
        }
        
        public JsonResult IsUpdateEmailExist(string UserUpdateEmail)
        {
            string UserCurrentEmail;
            if (Session["UserEditEmail"] != null)
                UserCurrentEmail = Session["UserEditEmail"].ToString();
            else
                UserCurrentEmail = Session["Email"].ToString();
            return Json(CheckUpdateEmailExist(UserUpdateEmail,UserCurrentEmail), JsonRequestBehavior.AllowGet);
        }

        public JsonResult IsUpdatePhoneNumberExist(string UserUpdatePhoneNumber)
        {
            string UserCurrentPhone;
            if (Session["UserEditPhoneNumber"] != null)
                UserCurrentPhone = Session["UserEditPhoneNumber"].ToString();
            else
                UserCurrentPhone = Session["PhoneNumber"].ToString();
            return Json(CheckUpdatePhoneNumberExist(UserUpdatePhoneNumber, UserCurrentPhone), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult SignUp()
        {
            var AllStates = AddressRepoObj.GetAllState();
            var states = new List<SelectListItem>();
            foreach (var item in AllStates)
            {
                states.Add(new SelectListItem() { Text = item.StateName, Value = item.StateID.ToString() });
            }
            ViewBag.State = states;
            ValidateDoctor doctor = new ValidateDoctor();
            return View(doctor);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult SignUp(HttpPostedFileBase file, ValidateDoctor users)
        {
            try
            {
                string _filename = DateTime.Now.ToString("yymmssfff") + Path.GetFileName(file.FileName);
                string path = Path.Combine(Server.MapPath("~/uploads/DoctorsProfileImage/"), _filename);
                users.UserProfileImage = "~/uploads/DoctorsProfileImage/" + _filename;
                if (users != null)
                {
                    users.StateID = int.Parse(users.State);
                    users.CityID = int.Parse(users.City);
                    users.AreaID = int.Parse(users.Area);
                    users.UserCreatedBy = 1;
                    users.UserUpdatedBy = 0;
                    users.Gender = users.Gender == "1" ? "Male" : "Female";
                    file.SaveAs(path);
                    ModelState.Clear();
                    var StatusCode = DoctorRepoObj.InsertDoctor(users);
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
                }
                else
                {
                    TempData["ErrorMsg"] = "Error on uploading Image!";
                    return RedirectToAction("SignUp");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return RedirectToAction("SignUp");
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult SignIn()
        {
            if (User.Identity.IsAuthenticated) 
            {
                return RedirectToAction("Index","Account");
            }
            else
            {
                ValidateUsersLogin login = new ValidateUsersLogin();
                return View(login);
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult SignIn(ValidateUsersLogin usersLogin)
        {
            try
            {
                if (usersLogin != null)
                {
                    ModelState.Clear();
                    var reas = UserRepoObj.CheckLoginDetails(usersLogin.UserEmail, usersLogin.UserPassword);
                    if (reas != null)
                    {
                        Session["Role"] = reas.Role.ToString().Normalize().Trim();
                        Session["Email"] = reas.Email.ToString().Normalize().Trim();
                        Session["UserID"] = reas.ID.ToString().Normalize().Trim();
                        Session["Username"] = reas.Name.ToString().Normalize().Trim();
                        Session["UserImage"] = reas.Image.ToString().Normalize().Trim();
                        Session["PhoneNumber"] = reas.PhoneNumber.ToString().Normalize().Trim();
                        if (reas.Role == "Doctor")
                        {
                            if (reas.IsProfileCompleted)
                            {
                                if (DARepoObj.IsDoctorProfileApproved(reas.ID))
                                {
                                    FormsAuthentication.SetAuthCookie(usersLogin.UserEmail, false);
                                    return RedirectToAction("Index");
                                }
                                else
                                {
                                    TempData["ErrorMsg"] = "Your profile is still in approval contact admin to make process faster!";
                                    return RedirectToAction("SignIn");
                                }
                            }
                            else
                            {
                                FormsAuthentication.SetAuthCookie(usersLogin.UserEmail, false);
                                return RedirectToAction("ProfileComplete", "Doctor");
                            }
                        }
                        else if (reas.Role == "Admin" || reas.Role == "SuperAdmin")
                        {
                            if (reas.IsProfileCompleted == true)
                            {
                                FormsAuthentication.SetAuthCookie(usersLogin.UserEmail, false);
                                return RedirectToAction("Index");
                            }
                            else
                            {
                                TempData["ErrorMsg"] = "Invalid Email Address or Password!";
                                return RedirectToAction("SignIn");
                            }
                        }
                        else
                        {
                            TempData["ErrorMsg"] = "Invalid Email Address or Password!";
                            return RedirectToAction("SignIn");
                        }
                    }
                    else
                    {
                        TempData["ErrorMsg"] = "Invalid Email Address or Password!";
                        return RedirectToAction("SignIn");
                    }
                }
                else
                {
                    TempData["ErrorMsg"] = "Please provide data!";
                    return RedirectToAction("SignUp");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ForgotPassword()
        {
            ValidateUsersLogin usersLogin = new ValidateUsersLogin();
            return View(usersLogin);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ForgotPassword(ValidateUsersLogin user)
        {
            try
            {
                if (!CheckEmailExist(user.UserEmailForResetPass))
                {
                    var IsSuccess = UserRepoObj.GenerateUserOTP(user.UserEmailForResetPass);
                    if (IsSuccess)
                    {
                        TempData["SuccessMsg"] = "Please check your <b>email</b> for a message with your code. Your code is 6 numbers long!";
                        Session["Email"] = user.UserEmailForResetPass;
                        ValidateUsersLogin usersLogin = new ValidateUsersLogin();
                        return View("_PasswordRecover", usersLogin);
                    }
                    else
                    {
                        TempData["ErrorMsg"] = "An error occured please try again later!";
                        return View();
                    }
                }
                else
                {
                    TempData["ErrorMsg"] = "No Account associated with this Email Address!";
                    return View();
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMsg"] = "Error occured on Account!" + ex.Message;
                return RedirectToAction("ForgotPassword");
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CheckOTP(ValidateUsersLogin user)
        {
            try
            {
                var Email = @Session["Email"].ToString();
                TempData["Email"] = Email;
                var IsSuccess = UserRepoObj.CheckOTP(Email, user.OTP);
                if (IsSuccess)
                {
                    TempData["SuccessMsg"] = "OTP Confirmed!";
                    Session["Email"] = user.UserEmailForResetPass;
                    ValidateUsersLogin usersLogin = new ValidateUsersLogin();
                    return View("_ResetPassword", usersLogin);
                }
                else
                {
                    TempData["ErrorMsg"] = "Incorrect OTP please recheck your email!";
                    return View("_PasswordRecover");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMsg"] = "Error occured on Account!" + ex.Message;
                return RedirectToAction("SignIn");
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult ResetPass(ValidateUsersLogin user)
        {
            try
            {
                var Email = @TempData["Email"].ToString();
                var userDetailObj = UserRepoObj.GetUserDetailByEmail(Email);
                user.UserEmail = Email;
                user.UserID = userDetailObj.ID;
                user.UserUpdatedBy = userDetailObj.ID;
                var Role = userDetailObj.Role;
                var IsSuccess = UserRepoObj.UpdateUserPasswword(user, Role);
                if (IsSuccess)
                {
                    TempData["SuccessMsg"] = "<b>Hurry!</b> your Password is Reset...";
                    return Json(new { message = "yes" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    TempData["ErrorMsg"] = "No Account found with this Email Address!";
                    return Json(new { message = "no" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMsg"] = "Error occured on Account!" + ex.Message;
                return Json(new { message = "error" }, JsonRequestBehavior.AllowGet);
            }
        }

        public string[] GetRoleByUsername(string username)
        {
            string UserRole = null;
            if (System.Web.HttpContext.Current.Session["Email"].ToString().Equals(username))
                UserRole = System.Web.HttpContext.Current.Session["Role"].ToString();
            return new string[] { UserRole };
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult SendOTP(string Email)
        {
            try
            {
                if (!string.IsNullOrEmpty(Email) && Email.Length > 5)
                {
                    var reas = UserRepoObj.GenerateUserOTP(Email);
                    if (reas)
                        return Json(true, JsonRequestBehavior.AllowGet);
                    else
                        return Json(false, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json(false, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public bool CheckEmailExist(string Email)
        {
            try
            {
                if (!string.IsNullOrEmpty(Email) && Email.Length > 5)
                {
                    var reas = UserRepoObj.IsEmailExist(Email);
                    if (reas)
                        return false;
                    else
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

        [AcceptVerbs(HttpVerbs.Get)]
        public bool CheckPhoneNumberExist(string PhoneNumber)
        {
            try
            {
                if (!string.IsNullOrEmpty(PhoneNumber) && PhoneNumber.Length > 5)
                {
                    var reas = UserRepoObj.IsPhoneNumberExist(PhoneNumber);
                    if (reas)
                        return false;
                    else
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
        
        public bool CheckUpdateEmailExist(string Email, string CurrentEmail)
        {
            try
            {
                if (!string.IsNullOrEmpty(Email) && Email.Length > 5 && !string.IsNullOrEmpty(CurrentEmail) && CurrentEmail.Length > 5)
                {
                    var reas = UserRepoObj.IsUpdateEmailExist(Email, CurrentEmail);
                    if (reas)
                        return false;
                    else
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

        [AcceptVerbs(HttpVerbs.Get)]
        public bool CheckUpdatePhoneNumberExist(string PhoneNumber, string CurrentPhoneNumber)
        {
            try
            {
                if (!string.IsNullOrEmpty(PhoneNumber) && PhoneNumber.Length > 5 && !string.IsNullOrEmpty(CurrentPhoneNumber) && CurrentPhoneNumber.Length > 5)
                {
                    var reas = UserRepoObj.IsUpdatePhoneNumberExist(PhoneNumber, CurrentPhoneNumber);
                    if (reas)
                        return false;
                    else
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

        public ActionResult GetCityList(int StateID)
        {
            var city = AddressRepoObj.GetCitiesByState(StateID);
            ViewBag.City = new SelectList(city, "CityID", "CityName");
            return PartialView("DisplayCity");
        }

        public ActionResult GetZoneList(int CityID)
        {
            var zone = AddressRepoObj.GetZoneByCity(CityID);
            ViewBag.Zone = new SelectList(zone, "ZoneID", "ZoneName");
            return PartialView("DisplayZone");
        }

    }
}