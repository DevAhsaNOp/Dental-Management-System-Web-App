using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DMS_BOL.Validation_Classes
{
    public class ValidateUsersLogin
    {
        [Required(ErrorMessage = "*")]
        [Display(Name = "Email Address")]
        [RegularExpression("^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}$", ErrorMessage = "Invalid Email Address")]
        public string UserEmail { get; set; }

        [Required(ErrorMessage = "*")]
        [Display(Name = "Password")]
        public string UserPassword { get; set; }
        
        [Required(ErrorMessage = "*")]
        [Display(Name = "Password")]
        [RegularExpression("^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$", ErrorMessage = "Password should contain minimum 8 characters in length and At least one uppercase, lowercase English letter and one digit and special character")]
        public string UserPasswordForReset { get; set; }

        [Required(ErrorMessage = "*")]
        [Display(Name = "Confirm Password")]
        [System.ComponentModel.DataAnnotations.Compare("UserPassword", ErrorMessage = "Password and Confirm Password must match")]
        public string UserConfirmPassword { get; set; }

        [Required(ErrorMessage = "*")]
        [Display(Name = "Email Address")]
        [RegularExpression("^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}$", ErrorMessage = "Invalid Email Address")]
        [Remote("IsEmailExistForResetPass", "Account", ErrorMessage = "<br><span>No account associated with the provided Email<span/>")]
        public string UserEmailForResetPass { get; set; }

        [Required(ErrorMessage = "*")]
        [Display(Name = "OTP")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Invalid OTP")]
        public string OTP { get; set; }

        public int? UserUpdatedBy { get; set; }

        public int UserID { get; set; }
    }
}
