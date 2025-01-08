﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace StartUp.DAL.Extend
{
    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole()
        {
            IsActive = true;
            IsExternal = false;
            CreatedOn = DateTime.Now.ToShortDateString();
            IsDeleted = false;
        }

        public bool IsActive { get; set; }
        public bool IsExternal { get; set; }
        public string CreatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}
