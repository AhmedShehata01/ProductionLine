using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace StartUp.BLL.Models.AccountDTO
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "Email required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }


        [Required(ErrorMessage = "password required")]
        [MinLength(6, ErrorMessage = "min len 6")]
        public string Password { get; set; }

        public bool RememberMe { get; set; }


        #region Google Integration

        public string? ReturnURL { get; set; }
        public IList<AuthenticationScheme>? ExternalLogins { get; set; }

        #endregion
    }
}
