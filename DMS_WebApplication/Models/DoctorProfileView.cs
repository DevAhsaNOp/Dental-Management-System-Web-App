using DMS_BOL;
using DMS_BOL.Validation_Classes;
using System.Collections.Generic;
using System.Web;

namespace DMS_WebApplication.Models
{
    public class DoctorProfileView
    {
        public tblDoctor Profile { get; set; }
        public IEnumerable<string> Services { get; set; }
        public IEnumerable<tblDoctorWorkExperience> Experience { get; set; }
        public IEnumerable<tblOfflineConsultaionDetail> OfflineConsultation { get; set; }
        public IEnumerable<tblOnlineConsultaionDetail> OnlineConsultation { get; set; }
    }

    public class ImageFile
    {
        public List<HttpPostedFileBase> files { get; set; }
    }
}