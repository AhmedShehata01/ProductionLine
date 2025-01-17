using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace StartUp.BLL.Models.AccountDTO
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "Full Name required.")]
        public string UserName { get; set; }


        [Remote(action: "IsEmailInUse", controller: "Account")]
        [Required(ErrorMessage = "Email required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [ValidEmailDomain(allowedDomain: "Dotsegypt.com", ErrorMessage = "Email Domain must be gmail.com")]
        public string Email { get; set; }


        [Required(ErrorMessage = "Password required.")]
        //[MinLength(8 , ErrorMessage = "min lin 8")]
        //[MaxLength(20 , ErrorMessage = "min lin 8")]
        public string Password { get; set; }



        [Required(ErrorMessage = "Confirm Password required.")]
        //[MinLength(8, ErrorMessage = "min lin 8")]
        //[MaxLength(20, ErrorMessage = "min lin 8")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "Password Not Match")]
        public string ConfirmPassword { get; set; }

        public bool IsAgree { get; set; }
    }
}
