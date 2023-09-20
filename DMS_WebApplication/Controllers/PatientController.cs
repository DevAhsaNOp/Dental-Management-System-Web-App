using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DMS_BLL.Repositories;
using DMS_BOL;
using DMS_BOL.Validation_Classes;
using DMS_WebApplication.Models;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DMS_WebApplication.Controllers
{
    [SessionExpire]
    public class PatientController : Controller
    {
        private UsersRepo UserRepoObj;
        private AddressRepo AddressRepoObj;
        private PatientTestRepo patientTestRepoObj;

        public PatientController()
        {
            UserRepoObj = new UsersRepo();
            AddressRepoObj = new AddressRepo();
            patientTestRepoObj = new PatientTestRepo();
        }

        #region **Add Patients**

        [AcceptVerbs(HttpVerbs.Get)]
        [CustomAuthorize(Roles = "Admin, SuperAdmin, Doctor")]
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
            Session["ImageAvatar"] = "~/uploads/PatientsProfileImage/nophoto.png";
            return View(patient);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [CustomAuthorize(Roles = "Admin, SuperAdmin, Doctor")]
        public ActionResult AddPatient(HttpPostedFileBase file, ValidatePatient users)
        {
            try
            {
                string _filename = DateTime.Now.ToString("yymmssfff") + Path.GetFileName(file.FileName);
                string path = Path.Combine(Server.MapPath("~/uploads/PatientsProfileImage/"), _filename);
                users.UserProfileImage = "~/uploads/PatientsProfileImage/" + _filename;
                file.SaveAs(path);
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

        #region **View All Patients**

        [AcceptVerbs(HttpVerbs.Get)]
        [CustomAuthorize(Roles = "Admin, SuperAdmin, Doctor")]
        public ActionResult Patients()
        {
            IEnumerable<tblPatient> patient = UserRepoObj.GetAllPatient();
            return View(patient);
        }

        #endregion

        #region **View Patient**

        [AcceptVerbs(HttpVerbs.Get)]
        [CustomAuthorize(Roles = "Admin, SuperAdmin, Doctor")]
        public ActionResult PatientView(int PatientID)
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

        #region **Patient Test**

        [AcceptVerbs(HttpVerbs.Get)]
        [CustomAuthorize(Roles = "Admin, SuperAdmin, Doctor, Patient")]
        public ActionResult PatientTest()
        {
            var allPatients = UserRepoObj.GetAllPatient().Select(x => new { x.P_ID, x.P_FirstName, x.P_LastName }).ToList();
            var patients = new List<SelectListItem>();
            foreach (var patient in allPatients)
            {
                patients.Add(new SelectListItem() { Text = string.Concat(patient.P_FirstName, " ", patient.P_LastName, " ( Patient ID: " + patient.P_ID.ToString() + " )"), Value = patient.P_ID.ToString() });
            }
            ViewBag.patient = patients;
            ValidatePatientTest patientTest = new ValidatePatientTest();
            return View(patientTest);
        }

        public ActionResult GetPatientByID(int PatientId)
        {
            if (PatientId > 0)
            {
                var reas = UserRepoObj.GetPatientByID(PatientId);
                if (reas != null)
                {
                    ValidatePatient patient = new ValidatePatient();
                    patient.UserFirstName = reas.P_FirstName;
                    patient.UserLastName = reas.P_LastName;
                    patient.UserPhoneNumber = reas.P_PhoneNumber;
                    patient.UserEmail = reas.P_Email;
                    patient.Gender = reas.P_Gender;
                    patient.UserProfileImage = reas.P_ProfileImage.Replace("~", "");
                    patient.CompleteAddress = reas.tblAddress.AddressComplete == null ? "" : reas.tblAddress.AddressComplete + ", " + reas.tblAddress.tblCity.CityName + ", " + reas.tblAddress.tblState.StateName + ", " + reas.tblAddress.tblZone.ZoneName;
                    return Json(patient, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json(null, JsonRequestBehavior.AllowGet);
            }
            else
                return Json(null, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [CustomAuthorize(Roles = "Admin, SuperAdmin, Doctor, Patient")]
        public ActionResult PatientTest(ImageFile objImage, ValidatePatientTest patientTest)
        {
            try
            {
                if (objImage.files != null && objImage.files.Count > 0 && patientTest.PT_PatientID > 0)
                {
                    string _foldername = "~/uploads/PatientTeethDetection/" + DateTime.Now.ToString("mmss") + "Patient" + patientTest.PT_PatientID + "/PatientUploaded";
                    string createFolder = Server.MapPath(_foldername);
                    if (!Directory.Exists(createFolder))
                    {
                        Directory.CreateDirectory(createFolder);
                        foreach (var file in objImage.files)
                        {
                            if (file != null && file.ContentLength > 0)
                            {
                                file.SaveAs(Path.Combine(Server.MapPath(_foldername), Guid.NewGuid() + Path.GetExtension(file.FileName)));
                            }
                        }
                        patientTest.PT_Images = _foldername.Replace("~/uploads/", "");
                        var reas = patientTestRepoObj.InsertPatientTest(patientTest);
                        if (reas > 0)
                        {
                            TempData["SuccessMsg"] = "Patient teeth testing is in process. You will recieve a report on your email when it's done!";
                            var cloudinary = new Cloudinary(new Account("dg5esqkeb", "654286619656251", "7-JkBR5t_EU8lN3ArIdvsZ1txCw"));
                            string path = Server.MapPath("" + _foldername + "");
                            string[] imageFiles = Directory.GetFiles(path);
                            foreach (string filename in imageFiles)
                            {
                                var uploadParams = new ImageUploadParams()
                                {
                                    File = new FileDescription(filename),
                                    PublicId = RandomFileNameGenerator(),
                                    Folder = _foldername.Replace("~/uploads/", ""),
                                    Tags = "PatientImages"
                                };

                                var uploadResult = cloudinary.Upload(uploadParams);
                            }

                            //Patient Teeth Test Api Call Request

                            var options = new RestClientOptions("http://127.0.0.1:5000")
                            {
                                MaxTimeout = -1,
                            };
                            var client = new RestClient(options);
                            var request = new RestRequest("/api/patientTeethTest/" + patientTest.PT_PatientID, Method.Get);
                            RestResponse response = client.ExecuteGet(request);
                            var result = response.Content;
                            JObject jsonResponse = JObject.Parse(result);
                            if (result != null && response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                if (jsonResponse["StatusCode"].ToString() == "200")
                                    TempData["SuccessMsg"] = "Patient teeth testing is in process. You will recieve a report on your email when it's done!";
                                else if (jsonResponse["StatusCode"].ToString() == "204")
                                    TempData["SuccessMsg"] = "Patient Record exists. No images found!";
                                else if (jsonResponse["StatusCode"].ToString() == "404")
                                    TempData["SuccessMsg"] = "Patient Record does not exist!";
                                else if (jsonResponse["StatusCode"].ToString() == "500")
                                    TempData["SuccessMsg"] = "Error in retrieving Patient data!";
                                else if (jsonResponse["StatusCode"].ToString() == "504")
                                    TempData["SuccessMsg"] = "Please provide patient id!";
                                else
                                    TempData["ErrorMsg"] = "An error occured while testing please try again later!";
                            }
                            else if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                                TempData["ErrorMsg"] = "Patient found but no images found!";
                            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                                TempData["ErrorMsg"] = "Patient not found!";
                            else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                                TempData["ErrorMsg"] = "An error occured while testing please try again later!";
                            else
                                TempData["ErrorMsg"] = "An error occured while testing please try again later!";
                        }
                        else
                            TempData["ErrorMsg"] = "An error occured while testing please try again later!";
                    }
                }
                return RedirectToAction("PatientTest");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Usable Functions

        private static Random random = new Random();

        public static string RandomString()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string RandomFileNameGenerator()
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var reas = new string(Enumerable.Repeat(chars, 10)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return reas + DateTime.Now.Millisecond.ToString();
        }

        #endregion
    }
}