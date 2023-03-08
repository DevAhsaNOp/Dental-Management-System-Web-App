﻿using DMS_BOL;
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

        public DoctorController()
        {
            _httpClient = new HttpClient();            
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ProfileComplete()
        {
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
        public ActionResult Complete(ValidateDoctorHospitalInfo doctor)
        {
            return View(doctor);
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