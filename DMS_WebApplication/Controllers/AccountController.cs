using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using System.Web.Security;
using DMS_BLL.Repositories;
using DMS_BOL.Validation_Classes;
using System.Collections.Generic;

namespace DMS_WebApplication.Controllers
{
    public class AccountController : Controller
    {
        private HttpClient _httpClient;
        private DoctorsRepo DoctorRepoObj;
        private UsersRepo UserRepoObj;
        private AddressRepo AddressRepoObj;

        public AccountController()
        {
            _httpClient = new HttpClient();
            DoctorRepoObj = new DoctorsRepo();
            UserRepoObj = new UsersRepo();
            AddressRepoObj = new AddressRepo();
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
            return View("SignIn");
        }
        public JsonResult IsEmailExist(string UserEmail)
        {
            return Json(CheckEmailExist(UserEmail), JsonRequestBehavior.AllowGet);
        }

        public JsonResult IsPhoneNumberExist(string UserPhoneNumber)
        {
            return Json(CheckPhoneNumberExist(UserPhoneNumber), JsonRequestBehavior.AllowGet);
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
            ValidateUsersLogin login = new ValidateUsersLogin();
            return View(login);
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
                        FormsAuthentication.SetAuthCookie(usersLogin.UserEmail, false);
                        Session["Role"] = reas.Role.ToString().Normalize().Trim();
                        Session["Email"] = reas.Email.ToString().Normalize().Trim();
                        Session["UserID"] = reas.ID.ToString().Normalize().Trim();
                        Session["Username"] = reas.Name.ToString().Normalize().Trim();
                        Session["UserImage"] = reas.Image.ToString().Normalize().Trim();
                        Session["PhoneNumber"] = reas.PhoneNumber.ToString().Normalize().Trim();
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
            return RedirectToAction("Index");
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