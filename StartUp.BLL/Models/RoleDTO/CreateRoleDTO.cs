using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartUp.BLL.Models.RoleDTO
{
    public class CreateRoleDTO
    {
        [Required]
        public string Name { get; set; }
        public bool IsExternal { get; set; }
    }
}
