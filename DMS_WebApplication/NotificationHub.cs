using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DMS_WebApplication
{
    public class NotificationHub : Hub
    {
        public static void Show()
        {
            var notificationhub = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            notificationhub.Clients.All.notify("added");
        }
    }
}