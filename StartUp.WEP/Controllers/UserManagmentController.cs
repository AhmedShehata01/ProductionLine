using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using StartUp.DAL.Database;
using StartUp.DAL.Extend;
using StartUp.BLL.Models.UserDTO;
using StartUp.BLL.Models.StaticData;
using Microsoft.EntityFrameworkCore;

namespace StartUp.WEP.Controllers
{
    [Authorize(Roles = "Admin,Super Admin")]
    public class UserManagmentController : Controller
    {
        #region Prop
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ApplicationContext db;

        #endregion


        #region Ctor
        public UserManagmentController(UserManager<ApplicationUser> _userManager,
                                        RoleManager<ApplicationRole> _roleManager,
                                        ApplicationContext db)
        {
            this._userManager = _userManager;
            this._roleManager = _roleManager;
            this.db = db;
        }

        #endregion


        #region Action


        [Authorize(Policy = "ViewRolePolicy")]
        public async Task<IActionResult> Index()
        {
            return View();
        }




        [Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User With Id = {id} Cannot be found";
                return View("PageNotFound");
            }

            var userClaims = await _userManager.GetClaimsAsync(user);
            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new EditUserDTO
            {
                Id = id,
                Email = user.Email,
                UserName = user.UserName,
                Claims = userClaims.Select(e => e.Type + " : " + e.Value).ToList(),
                Roles = userRoles.ToList()
            };

            return View(model);
        }

        [Authorize(Policy = "EditRolePolicy")]
        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserDTO model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User With Id = {model.Id} Cannot be found";
                return View("PageNotFound");
            }
            else
            {
                user.Email = model.Email;
                user.UserName = model.UserName;

                var result = await _userManager.UpdateAsync(user);

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





        [Authorize(Policy = "DeleteRolePolicy")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User With Id = {id} Cannot be found";
                return View("PageNotFound");
            }
            var model = new DeleteUserDTO
            {
                Id = id,
                UserName = user.UserName,
                Email = user.Email
            };

            return View(model);
        }


        [Authorize(Policy = "DeleteRolePolicy")]
        [HttpPost]
        public async Task<IActionResult> DeleteUser(EditUserDTO model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User With Id = {model.Id} Cannot be found";
                return View("PageNotFound");
            }
            else
            {

                // Check if the user is associated with any roles
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Any())
                {
                    // User is associated with roles, cannot delete
                    ViewBag.ErrorTitle = "Error Deleting User";
                    ViewBag.ErrorMessage = "Not able to delete user. Please remove user from role(s) first and try again.";
                    return View("Error");
                }

                // Remove all claims associated with the user
                var claims = await _userManager.GetClaimsAsync(user);
                foreach (var claim in claims)
                {
                    var claimRemovalResult = await _userManager.RemoveClaimAsync(user, claim);
                    if (!claimRemovalResult.Succeeded)
                    {
                        // Handle claim removal failure if necessary
                        ViewBag.ErrorTitle = "Error Deleting User Claims";
                        ViewBag.ErrorMessage = "An error occurred while deleting the user claims.";
                        return View("Error");
                    }
                }

                // Remove any logins associated with the user
                var logins = await _userManager.GetLoginsAsync(user);
                foreach (var login in logins)
                {
                    var loginRemovalResult = await _userManager.RemoveLoginAsync(user, login.LoginProvider, login.ProviderKey);
                    if (!loginRemovalResult.Succeeded)
                    {
                        ViewBag.ErrorTitle = "Error Deleting User";
                        ViewBag.ErrorMessage = "An error occurred while deleting the user logins.";
                        return View("Error");
                    }
                }


                // Proceed with user deletion
                var deletionResult = await _userManager.DeleteAsync(user);

                if (deletionResult.Succeeded)
                {
                    return RedirectToAction("Index");
                }

                foreach (var error in deletionResult.Errors)
                {
                    ViewBag.ErrorTitle = "Error Deleting User";
                    ViewBag.ErrorMessage = "An error occurred while deleting the user.";
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }
        }








        public async Task<IActionResult> ManageUserRoles(string userId)
        {
            ViewBag.userId = userId;

            var user = await _userManager.FindByIdAsync(userId);
            ViewBag.userName = user.UserName;

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User With Id = {userId} Cannot be found";
                return View("PageNotFound");
            }

            var model = new List<UserRolesDTO>();

            foreach (var role in _roleManager.Roles)
            {
                var userRolesDTO = new UserRolesDTO
                {
                    RoleId = role.Id,
                    RoleName = role.Name
                };

                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    userRolesDTO.IsSelected = true;
                }
                else
                {
                    userRolesDTO.IsSelected = false;
                }

                model.Add(userRolesDTO);
            }

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> ManageUserRoles(List<UserRolesDTO> model, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found.";
                return View("PageNotFound");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, roles);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot Remove User Existing Roles.");
                return View(model);
            }

            result = await _userManager.AddToRolesAsync(user, model.Where(x => x.IsSelected).Select(y => y.RoleName));
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot Add The Selected Roles To User.");
                return View(model);
            }


            return RedirectToAction("EditUser", new { Id = userId });
        }




        public async Task<IActionResult> ManageUserClaims(string userId)
        {
            ViewBag.userId = userId;

            var user = await _userManager.FindByIdAsync(userId);

            ViewBag.userName = user.UserName;

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found";
                return View("PageNotFound");
            }

            var existingUserClaims = await _userManager.GetClaimsAsync(user);
            var model = new UserClaimsDTO
            {
                UserId = userId
            };

            foreach (Claim claim in ClaimsStore.AllClaims)
            {
                UserClaim userClaim = new UserClaim
                {
                    ClaimType = claim.Type
                };

                if (existingUserClaims.Any(c => c.Type == claim.Type && c.Value == "true"))
                {
                    userClaim.IsSelected = true;
                }
                model.Claims.Add(userClaim);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ManageUserClaims(UserClaimsDTO model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {model.UserId} cannot be found.";
                return View("PageNotFound");
            }

            var claims = await _userManager.GetClaimsAsync(user);
            var result = await _userManager.RemoveClaimsAsync(user, claims);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "cannot remove user existing claims");
                return View(model);
            }

            result = await _userManager.AddClaimsAsync(user, model.Claims
                                       .Select(c => new Claim(c.ClaimType, c.IsSelected ? "true" : "false")));

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "cannot add selected claims to user");
                return View(model);
            }

            return RedirectToAction("EditUser", new { Id = model.UserId });
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


                var data = (from UserManager in db.Users
                            where UserManager.IsDeleted == false
                            select UserManager);

                // Sorting 
                if (!string.IsNullOrEmpty(sortColumnName) && !string.IsNullOrEmpty(sortColumnDir))
                {
                    switch (sortColumnName)
                    {
                        case "UserName":
                            data = sortColumnDir == "asc" ? data.OrderBy(e => e.UserName) : data.OrderByDescending(e => e.UserName);
                            break;
                        case "Email":
                            data = sortColumnDir == "asc" ? data.OrderBy(e => e.Email) : data.OrderByDescending(e => e.Email);
                            break;
                        case "EmailConfirmed":
                            data = sortColumnDir == "asc" ? data.OrderBy(e => e.EmailConfirmed) : data.OrderByDescending(e => e.EmailConfirmed);
                            break;
                        case "PhoneNumber":
                            data = sortColumnDir == "asc" ? data.OrderBy(e => e.PhoneNumber) : data.OrderByDescending(e => e.PhoneNumber);
                            break;
                        case "CreatedOn":
                            data = sortColumnDir == "asc" ? data.OrderBy(e => e.CreatedOn) : data.OrderByDescending(e => e.CreatedOn);
                            break;
                        default:
                            break;
                    }
                }

                // Filtering 
                if (!string.IsNullOrEmpty(searchValue))
                {
                    data = data.Where(e =>
                        e.UserName.Contains(searchValue) ||                     // Filter by employee name
                        e.Email.Contains(searchValue) ||                        // Filter by employee name
                        e.EmailConfirmed.ToString().Contains(searchValue) ||    // Filter by employee name
                        e.PhoneNumber.Contains(searchValue) ||                  // Filter by employee name
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
