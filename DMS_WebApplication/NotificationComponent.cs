using DMS_BLL.Repositories;
using DMS_BOL.Validation_Classes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace DMS_WebApplication
{
    public class NotificationComponent
    {
        private DoctorApprovalRepo repoObj;

        public NotificationComponent()
        {
            repoObj = new DoctorApprovalRepo();
        }

        public void RegisterNotification(DateTime currentTime)
        {
            string constring = ConfigurationManager.ConnectionStrings["notificationConnString"].ConnectionString;
            SqlDependency.Start(constring);
            string SqlCmd = String.Empty;
            SqlCmd = @"SELECT [N_ID] ,[N_DoctorID] ,[N_IsRead] ,[N_CreateadOn] FROM [dbo].[tblDoctorApproved] WHERE (N_IsApproved <> 1)";

            using (SqlConnection con = new SqlConnection(constring))
            {
                SqlCommand cmd = new SqlCommand(SqlCmd, con);
                if (con.State != System.Data.ConnectionState.Open)
                {
                    con.Open();
                }
                cmd.Notification = null;
                SqlDependency sql = new SqlDependency(cmd);
                sql.OnChange += new OnChangeEventHandler(sqlDep_OnChange);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {

                }
            }
        }

        void sqlDep_OnChange(object sender, SqlNotificationEventArgs e)
        {
            if (e.Info == SqlNotificationInfo.Update)
            {
                NotificationHub.Show();
                RegisterNotification(DateTime.Now);
            }
            else if (e.Info == SqlNotificationInfo.Insert)
            {
                NotificationHub.Show();
                RegisterNotification(DateTime.Now);
            }
        }

        public IEnumerable<ValidateNotification> GetAllDoctorApprovalRequestForAD(int AdminID)
        {
            var reas = repoObj.GetAllDoctorApprovalRequestForAD(AdminID);
            return reas;
        }

        public int GetCountDoctorApprovalRequestForAD(int AdminID)
        {
            var reas = repoObj.GetCountDoctorApprovalRequestForAD(AdminID);
            return reas;
        }
        
        public bool ChangeDANotificationToReadForAD(int AdminID)
        {
            var reas = repoObj.ChangeDANotificationToReadForAD(AdminID);
            return reas;
        }

        public IEnumerable<ValidateNotification> GetAllDoctorApprovalRequestForSAD(int SuperAdminID)
        {
            var reas = repoObj.GetAllDoctorApprovalRequestForSAD(SuperAdminID);
            return reas;
        }

        public int GetCountDoctorApprovalRequestForSAD(int SuperAdminID)
        {
            var reas = repoObj.GetCountDoctorApprovalRequestForSAD(SuperAdminID);
            return reas;
        }

        public bool ChangeDANotificationToReadForSAD(int SuperAdminID)
        {
            var reas = repoObj.ChangeDANotificationToReadForSAD(SuperAdminID);
            return reas;
        }
    }
}