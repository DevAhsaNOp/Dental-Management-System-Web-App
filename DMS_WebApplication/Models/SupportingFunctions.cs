using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DMS_WebApplication.Models
{
    public class SupportingFunctions
    {
        private static Random random = new Random();

        public static string RandomString()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }

}