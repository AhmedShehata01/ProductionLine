﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace StartUp.BLL.Models.StaticData
{
    public class ClaimsStore
    {
        public static List<Claim> AllClaims = new List<Claim>()
        {
            new Claim("View Role" , "View Role"),
            new Claim("Create Role" , "Create Role"),
            new Claim("Edit Role" , "Edit Role"),
            new Claim("Delete Role" , "Delete Role"),
        };
    }
}
