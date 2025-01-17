using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartUp.BLL.Models.UserDTO
{
    public class EditUserDTO
    {
        public EditUserDTO()
        {
            Claims = new List<string>();
            Roles = new List<string>();
        }

        public string Id { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public List<string> Claims { get; set; }
        public List<string> Roles { get; set; }
    }
}
