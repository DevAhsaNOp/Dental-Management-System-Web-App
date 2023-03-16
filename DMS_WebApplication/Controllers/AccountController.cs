using System;
using DMS_BOL;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using Newtonsoft.Json;
using System.Web.Security;
using Newtonsoft.Json.Linq;
using DMS_BOL.Validation_Classes;
using System.Collections.Generic;
using System.Net;
using System.Web.Helpers;

namespace DMS_WebApplication.Controllers
{
    public class AccountController : Controller
    {
        private HttpClient _httpClient;
        string _BaseURL = "https://dmswepapi.azurewebsites.net/api/";

        public AccountController()
        {
            _httpClient = new HttpClient();
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
            var AllStates = GetAllState();
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
                    var reas = _httpClient.PostAsJsonAsync(_BaseURL + "Register/Doctor", users).Result;
                    if (reas.IsSuccessStatusCode)
                    {
                        var json = JObject.Parse(reas.Content.ReadAsStringAsync().Result);
                        var StatusCode = JsonConvert.DeserializeObject<int>(json["StatusCode"].ToString());
                        if (StatusCode == 201)
                            TempData["SuccessMsg"] = "Account Created Successfully!";
                        else if (StatusCode == 403)
                            TempData["ErrorMsg"] = "Invalid OTP provided. Please ensure to enter correct OTP again!";
                        else if (StatusCode == 404)
                            TempData["ErrorMsg"] = "Email already exists. Please ensure to enter not used email account again!";
                        else if (StatusCode == 405)
                            TempData["ErrorMsg"] = "Phone Number already exists. Please ensure to enter not used phone number again!";
                        else if(StatusCode == 500)
                            TempData["ErrorMsg"] = "Error occured on creating Account. Please try again later!";
                        else
                            TempData["ErrorMsg"] = "Error on creating account!";
                    }
                    else
                    {
                        TempData["ErrorMsg"] = "Error on creating account!";
                    }
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
                    var reas = _httpClient.PostAsJsonAsync(_BaseURL + "/login", usersLogin).Result;
                    if (reas.IsSuccessStatusCode)
                    {
                        var json = JObject.Parse(reas.Content.ReadAsStringAsync().Result);
                        var StatusCode = JsonConvert.DeserializeObject<int>(json["StatusCode"].ToString());
                        if (StatusCode == 200)
                        {
                            FormsAuthentication.SetAuthCookie(usersLogin.UserEmail, false);
                            Session["AccessToken"] = json["access_token"].ToString();
                            Session["Role"] = json["Role"].ToString().Normalize().Trim();
                            Session["Email"] = json["Email"].ToString().Normalize().Trim();
                            Session["UserID"] = json["UserID"].ToString().Normalize().Trim();
                            Session["Username"] = json["Username"].ToString().Normalize().Trim();
                            Session["UserImage"] = json["UserImage"].ToString().Normalize().Trim();
                            Session["PhoneNumber"] = json["PhoneNumber"].ToString().Normalize().Trim();
                            TempData["SuccessMsg"] = "Account Login Successfully!";
                        }
                        else
                        {
                            TempData["ErrorMsg"] = "Invalid Email Address or Password!";
                            return RedirectToAction("SignIn");
                        }
                    }
                    else
                    {
                        TempData["ErrorMsg"] = "Error on logon account, Please try again later!";
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

        public IEnumerable<tblState> GetAllState()
        {
            try
            {
                var response = _httpClient.GetAsync(_BaseURL + "Get/State");
                var reas = response.GetAwaiter().GetResult();
                response.Wait();
                using (HttpContent content = reas.Content)
                {
                    var json = JObject.Parse(content.ReadAsStringAsync().Result);
                    var StatusCode = JsonConvert.DeserializeObject<int>(json["StatusCode"].ToString());
                    if (StatusCode == 200)
                    {
                        IEnumerable<tblState> States = JsonConvert.DeserializeObject<IEnumerable<tblState>>(json["Datalist"].ToString());
                        return States;
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

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult SendOTP(string Email)
        {
            try
            {
                if (!string.IsNullOrEmpty(Email) && Email.Length > 5)
                {
                    _httpClient.BaseAddress = new Uri(_BaseURL);
                    HttpResponseMessage getOTP = _httpClient.GetAsync("Send/OTP?Email=" + Email).Result;
                    if (getOTP.IsSuccessStatusCode)
                    {
                        var json = JObject.Parse(getOTP.Content.ReadAsStringAsync().Result);
                        var StatusCode = JsonConvert.DeserializeObject<int>(json["StatusCode"].ToString());
                        if (StatusCode == 201)
                            return Json(true, JsonRequestBehavior.AllowGet);
                        else
                            return Json(false, JsonRequestBehavior.AllowGet);
                    }
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
                    _httpClient.BaseAddress = new Uri(_BaseURL);
                    HttpResponseMessage getOTP = _httpClient.GetAsync("Check/EmailExist?Email=" + Email).Result;
                    if (getOTP.IsSuccessStatusCode)
                    {
                        var json = JObject.Parse(getOTP.Content.ReadAsStringAsync().Result);
                        var StatusCode = JsonConvert.DeserializeObject<int>(json["StatusCode"].ToString());
                        if (StatusCode == 200)
                            return true;
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

        public bool CheckPhoneNumberExist(string PhoneNumber)
        {
            try
            {
                if (!string.IsNullOrEmpty(PhoneNumber) && PhoneNumber.Length > 5)
                {
                    _httpClient.BaseAddress = new Uri(_BaseURL);
                    HttpResponseMessage getOTP = _httpClient.GetAsync("Check/PhoneNumberExist?PhoneNumber=" + PhoneNumber).Result;
                    if (getOTP.IsSuccessStatusCode)
                    {
                        var json = JObject.Parse(getOTP.Content.ReadAsStringAsync().Result);
                        var StatusCode = JsonConvert.DeserializeObject<int>(json["StatusCode"].ToString());
                        if (StatusCode == 200)
                            return true;
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

        public IEnumerable<tblCity> GetCitiesByState(int StateID)
        {
            try
            {
                var response = _httpClient.GetAsync(_BaseURL + "Get/CityByState?StateID=" + StateID + "");
                var reas = response.GetAwaiter().GetResult();
                response.Wait();
                using (HttpContent content = reas.Content)
                {
                    var json = JObject.Parse(content.ReadAsStringAsync().Result);
                    var StatusCode = JsonConvert.DeserializeObject<int>(json["StatusCode"].ToString());
                    if (StatusCode == 200)
                    {
                        IEnumerable<tblCity> Cities = JsonConvert.DeserializeObject<IEnumerable<tblCity>>(json["Datalist"].ToString());
                        return Cities;
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

        public IEnumerable<tblZone> GetZoneByCity(int CityID)
        {
            try
            {
                var response = _httpClient.GetAsync(_BaseURL + "Get/ZoneByCity?CityID=" + CityID + "");
                var reas = response.GetAwaiter().GetResult();
                response.Wait();
                using (HttpContent content = reas.Content)
                {
                    var json = JObject.Parse(content.ReadAsStringAsync().Result);
                    var StatusCode = JsonConvert.DeserializeObject<int>(json["StatusCode"].ToString());
                    if (StatusCode == 200)
                    {
                        IEnumerable<tblZone> Zones = JsonConvert.DeserializeObject<IEnumerable<tblZone>>(json["Datalist"].ToString());
                        return Zones;
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

        public ActionResult GetCityList(int StateID)
        {
            var city = GetCitiesByState(StateID);
            ViewBag.City = new SelectList(city, "CityID", "CityName");
            return PartialView("DisplayCity");
        }

        public ActionResult GetZoneList(int CityID)
        {
            var zone = GetZoneByCity(CityID);
            ViewBag.Zone = new SelectList(zone, "ZoneID", "ZoneName");
            return PartialView("DisplayZone");
        }

    }
}