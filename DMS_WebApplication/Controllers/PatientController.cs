using DMS_BLL.Repositories;
using DMS_BOL.Validation_Classes;
using DMS_DAL.DBLayer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DMS_WebApplication.Controllers
{
    public class PatientController : Controller
    {
        private UsersRepo UserRepoObj;
        private AddressRepo AddressRepoObj;

        public PatientController() 
        { 
            UserRepoObj = new UsersRepo();
            AddressRepoObj = new AddressRepo();
        }
        
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
            return View(patient);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [CustomAuthorize(Roles = "Admin, SuperAdmin, Doctor")]
        public ActionResult AddPatient(HttpPostedFileBase file, ValidatePatient users)
        {
            try
            {
                users.UserProfileImage = "~/uploads/PatientsProfileImage/patient.jpg";
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

        [AcceptVerbs(HttpVerbs.Get)]
        [CustomAuthorize(Roles = "Admin, SuperAdmin, Doctor")]
        public ActionResult Patients()
        {
            var AllStates = AddressRepoObj.GetAllState();
            var states = new List<SelectListItem>();
            foreach (var item in AllStates)
            {
                states.Add(new SelectListItem() { Text = item.StateName, Value = item.StateID.ToString() });
            }
            ViewBag.State = states;
            ValidatePatient patient = new ValidatePatient();
            return View(patient);
        }
    }
}