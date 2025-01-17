using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StartUp.BLL.Models.RoleDTO;
using StartUp.DAL.Database;
using StartUp.DAL.Extend;

namespace StartUp.WEP.Controllers
{
    public class RoleManagmentController : Controller
    {
        #region Prop
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<RoleManagmentController> logger;
        private readonly ApplicationContext db;

        #endregion

        #region Ctor
        public RoleManagmentController(RoleManager<ApplicationRole> _roleManager,
                                       UserManager<ApplicationUser> _userManager,
                                       IMapper _Mapper,
                                       ILogger<RoleManagmentController> logger,
                                       ApplicationContext db)
        {
            this._roleManager = _roleManager;
            this._userManager = _userManager;
            this._mapper = _Mapper;
            this.logger = logger;
            this.db = db;
        }

        #endregion

        #region Actions 
        [Authorize(Policy = "ViewRolePolicy")]
        public IActionResult Index()
        {
            return View();
        }



        [Authorize(Policy = "CreateRolePolicy")]
        public IActionResult CreateRole()
        {
            return View();
        }


        [Authorize(Policy = "CreateRolePolicy")]
        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleDTO model)
        {
            if (ModelState.IsValid)
            {
                var role = _mapper.Map<ApplicationRole>(model);
                var result = await _roleManager.CreateAsync(role);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }




        [Authorize(Policy = "EditRolePolicy")]
        [HttpGet]
        public async Task<IActionResult> EditRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role With Id = {id} Cannot be found";
                return View("PageNotFound");
            }

            //var model = _mapper.Map<ApplicationRole>(role);
            var model = new EditRoleDTO
            {
                Id = role.Id,
                RoleName = role.Name,
                IsExternal = role.IsExternal
            };


            foreach (var user in _userManager.Users)
            {
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    model.Users.Add(user.UserName);
                }
            }

            return View(model);
        }


        [Authorize(Policy = "EditRolePolicy")]
        [HttpPost]
        public async Task<IActionResult> EditRole(EditRoleDTO model)
        {
            var role = await _roleManager.FindByIdAsync(model.Id);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role With Id = {model.Id} Cannot be found";
                return View("PageNotFound");
            }
            else
            {
                role.Name = model.RoleName;
                role.IsExternal = model.IsExternal;
                var result = await _roleManager.UpdateAsync(role);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }
        }




        [Authorize(Policy = "EditRolePolicy")]
        [HttpGet]
        public async Task<IActionResult> EditUserInRole(string RoleId)
        {
            ViewBag.roleId = RoleId;

            var role = await _roleManager.FindByIdAsync(RoleId);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role With Id = {RoleId} Connot be found";
                return View("PageNotFound");
            }

            var model = new List<UserRoleDTO>();

            foreach (var user in _userManager.Users)
            {
                var UserRoleDTO = new UserRoleDTO
                {
                    UserId = user.Id,
                    UserName = user.UserName
                };

                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    UserRoleDTO.IsSelected = true;
                }
                else
                {
                    UserRoleDTO.IsSelected = false;
                }
                model.Add(UserRoleDTO);
            }
            return View(model);
        }

        [Authorize(Policy = "EditRolePolicy")]
        [HttpPost]
        public async Task<IActionResult> EditUserInRole(List<UserRoleDTO> model, string RoleId)
        {
            var role = await _roleManager.FindByIdAsync(RoleId);

            if (role == null)
            {

                ViewBag.ErrorMessage = $"Role With Id = {RoleId} Cannot be found";
                return View("PageNotFound");
            }

            for (int i = 0; i < model.Count; i++)
            {
                var user = await _userManager.FindByIdAsync(model[i].UserId);

                IdentityResult result = null;

                if (model[i].IsSelected && !(await _userManager.IsInRoleAsync(user, role.Name)))
                {
                    result = await _userManager.AddToRoleAsync(user, role.Name);
                }
                else if (!model[i].IsSelected && await _userManager.IsInRoleAsync(user, role.Name))
                {
                    result = await _userManager.RemoveFromRoleAsync(user, role.Name);
                }
                else
                {
                    continue;
                }

                if (result.Succeeded)
                {
                    if (i < (model.Count - 1))
                        continue;
                    else
                        return RedirectToAction("EditRole", new { Id = RoleId });
                }
            }

            return RedirectToAction("EditRole", new { Id = RoleId });
        }

        //[Authorize(Policy = "EditRolePolicy")]
        //public IActionResult AssignRolesToController()
        //{
        //    // Get controllers from the enum
        //    ViewBag.Controllers = Enum.GetValues(typeof(ControllerNames))
        //        .Cast<ControllerNames>()
        //        .Select(c => new SelectListItem
        //        {
        //            Value = c.ToString(),
        //            Text = c.ToString()
        //        }).ToList();

        //    // Get roles from the database
        //    ViewBag.Roles = _roleManager.Roles
        //        .Select(r => new SelectListItem
        //        {
        //            Value = r.Name,
        //            Text = r.Name
        //        }).ToList();

        //    return View();
        //}


        //[HttpPost]
        //[Authorize(Policy = "EditRolePolicy")]
        //public async Task<IActionResult> AssignRolesToController(string controllerName, List<string> roleNames)
        //{
        //    if (controllerName != null && roleNames != null && roleNames.Any())
        //    {
        //        foreach (var roleName in roleNames)
        //        {
        //            var roleControllerAccess = new RoleControllerAccess
        //            {
        //                ControllerName = controllerName,
        //                RoleName = roleName
        //            };

        //            // Add to the RoleControllerAccess table
        //            await db.RoleControllerAccesses.AddAsync(roleControllerAccess);
        //        }

        //        // Save changes to the database
        //        await db.SaveChangesAsync();

        //        return RedirectToAction("Index");  // Redirect to a page with all roles and controllers
        //    }

        //    ModelState.AddModelError("", "Please select a controller and at least one role.");
        //    return View();
        //}




















        [Authorize(Policy = "DeleteRolePolicy")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role With Id = {id} Cannot be found";
                return View("PageNotFound");
            }

            var model = new DeleteRoleDTO
            {
                Id = id,
                RoleName = role.Name,
            };

            return View(model);
        }

        [Authorize(Policy = "DeleteRolePolicy")]
        [HttpPost]
        public async Task<IActionResult> DeleteRole(DeleteRoleDTO model)
        {
            var role = await _roleManager.FindByIdAsync(model.Id);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role With Id = {model.Id} Cannot be found";
                return View("PageNotFound");
            }
            else
            {
                try
                {

                    // user.IsDeleted = true; 
                    var result = await _roleManager.DeleteAsync(role);

                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View("Index");
                }
                catch (DbUpdateException ex)
                {
                    // logger.LogError($"Error While Deleting Role {ex}");

                    ViewBag.ErrorTitle = $" {role.Name} Role is in use";
                    ViewBag.ErrorMessage = $" {role.Name} role cannot be deleted as there are users in this role" +
                        $". if you want to delete this role " +
                        $". please remove the users from the role and try again";
                    return View("Error");

                }
            }
        }

        #endregion


        #region Access Denied
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        #endregion

        #region Ajax Call DataTable
        [HttpPost]
        public async Task<IActionResult> GetData()
        {
            try
            {
                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var sortColumnIndex = Convert.ToInt32(Request.Form["order[0][column]"].FirstOrDefault());
                var sortColumnName = Request.Form["columns[" + sortColumnIndex + "][name]"].FirstOrDefault();
                var sortColumnDir = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                var pageSize = length != null ? Convert.ToInt32(length) : 0;
                var skip = start != null ? Convert.ToInt32(start) : 0;


                var data = (from RoleManager in db.Roles
                            where RoleManager.IsDeleted == false
                            select RoleManager);

                // Sorting 
                if (!string.IsNullOrEmpty(sortColumnName) && !string.IsNullOrEmpty(sortColumnDir))
                {
                    switch (sortColumnName)
                    {
                        case "Name":
                            data = sortColumnDir == "asc" ? data.OrderBy(e => e.Name) : data.OrderByDescending(e => e.Name);
                            break;
                        case "CreatedOn":
                            data = sortColumnDir == "asc" ? data.OrderBy(e => e.CreatedOn) : data.OrderByDescending(e => e.CreatedOn);
                            break;
                        case "IsActive":
                            data = sortColumnDir == "asc" ? data.OrderBy(e => e.IsActive) : data.OrderByDescending(e => e.IsActive);
                            break;
                        case "IsExternal":
                            data = sortColumnDir == "asc" ? data.OrderBy(e => e.IsExternal) : data.OrderByDescending(e => e.IsExternal);
                            break;
                        default:
                            break;
                    }
                }

                // Filtering 
                if (!string.IsNullOrEmpty(searchValue))
                {
                    data = data.Where(e =>
                        e.Name.Contains(searchValue) ||  // Filter by employee name
                        e.CreatedOn.ToString().Contains(searchValue));
                }

                var totalRecords = await data.CountAsync();
                var cData = await data.Skip(skip).Take(pageSize).ToListAsync();

                var jsonData = new
                {
                    draw = draw,
                    recordsFiltered = totalRecords,
                    recordsTotal = totalRecords,
                    data = cData
                };

                return new JsonResult(jsonData);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"An error occurred while processing the request: {ex}");
                return StatusCode(500, "An error occurred while processing your request.");
            }

        }




        #endregion
    }
}
