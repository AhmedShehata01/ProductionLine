using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartUp.BLL.Models.AccountDTO
{
    public class ResetPasswordDTO
    {
        [Required]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Password And Confirm Password Not Match")]
        public string ConfirmPassword { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string Token { get; set; }
    }
}
