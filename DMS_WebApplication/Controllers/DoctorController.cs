using DMS_BOL.Validation_Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DMS_WebApplication.Controllers
{
    public class DoctorController : Controller
    {
        public DoctorController()
        {
            
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ProfileComplete()
        {
            ValidateDoctor doctor = new ValidateDoctor();
            return View(doctor);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ProfileComplete(ValidateDoctor doctor)
        {
            return View(doctor);
        }
    }
}