using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DMS_WebApplication.Models
{
    public class LoginResponseValidation
    {
        public int StatusCode { get; set; }
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        public bool Success { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; internal set; }
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; internal set; }
        [JsonProperty("token_type")]
        public string TokenType { get; internal set; }
        [JsonProperty(".issued")]
        public string IssuedTime { get; set; }
        [JsonProperty(".expires")]
        public string ExpiredTime { get; set; }
    }
}