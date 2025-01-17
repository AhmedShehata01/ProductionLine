using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StartUp.BLL.Models.AccountDTO;
using StartUp.BLL.Services;
using StartUp.DAL.Extend;

namespace StartUp.WEP.Controllers
{
    public class AccountController : Controller
    {
        #region Prop
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailervice;
        private readonly ILogger<AccountController> _logger;

        #endregion

        #region Ctro 
        public AccountController(UserManager<ApplicationUser> _userManager,
                                SignInManager<ApplicationUser> _signInManager,
                                IMapper _mapper,
                                IEmailService _emailervice,
                                ILogger<AccountController> _logger)
        {
            this._userManager = _userManager;
            this._signInManager = _signInManager;
            this._mapper = _mapper;
            this._emailervice = _emailervice;
            this._logger = _logger;
        }
        #endregion

        #region Actions 



        #region Register
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [AcceptVerbs("Get", "Post")]
        [AllowAnonymous]
        public async Task<IActionResult> IsEmailInUse(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Json(true);
            }
            else
            {
                return Json($"Email {email} Is Already In Use");
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterDTO model)
        {
            if (ModelState.IsValid)
            {
                var user = _mapper.Map<ApplicationUser>(model);
                user.EmailConfirmed = true;
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {

                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var confirmation = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token = token }, Request.Scheme);
                    var emailTo = user.Email;
                    var emailsubject = "Confirm your account";
                    var emailbody = $"Please confirm your account by clicking this link: <a href='{confirmation}'>link</a>";


                    _logger.Log(LogLevel.Warning, confirmation);

                    if (_signInManager.IsSignedIn(User) && (User.IsInRole("Admin") || User.IsInRole("Super Admin")))
                    {
                        return RedirectToAction("Index", "UserManagment");
                    }
                    
                    return View("RegistrationSuccessful");
                }
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("", item.Description);
                }

            }
            return View(model);
        }

        [AllowAnonymous]
        public IActionResult RegistrationSuccessful()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return RedirectToAction("index", "home");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"The User With ID {userId} is Invalid";
                return View("PageNotFound");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                return View();
            }

            ViewBag.ErrorTitle = "Email Cannot be Confirmed";
            return View("Error");

        }


        #endregion


        #region Login
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl = "")
        {
            LoginDTO model = new LoginDTO
            {
                ReturnURL = returnUrl,
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginDTO model, string returnUrl = "")
        {
            model.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    ModelState.AddModelError("", "Email not Exist.");
                    return View(model);
                }

                if (user != null && !user.EmailConfirmed && (await _userManager.CheckPasswordAsync(user, model.Password)))
                {
                    ModelState.AddModelError(string.Empty, "Email Not Confirmed Yet.");
                    return View(model);
                }

                var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, true);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }

                if (result.IsLockedOut)
                {
                    return View("AccountLocked");
                }

                ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
            }
            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult ExternalLogin(string provider, string returnUrl)
        {
            var redirectUrl = Url.Action("ExternalLoginCallBack", "Account", new { ReturnUrl = returnUrl });

            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            return new ChallengeResult(provider, properties);
        }


        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallBack(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            LoginDTO loginDTO = new LoginDTO
            {
                ReturnURL = returnUrl,
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error From External Provider: {remoteError}");
                return View("Login", loginDTO);
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ModelState.AddModelError(string.Empty, $"Error From External Login Information: {remoteError}");
                return View("Login", loginDTO);
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            ApplicationUser user = null;

            //if (email != null)
            //{
            //    user = await _userManager.FindByEmailAsync(email);
            //    await _userManager.ConfirmEmailAsync(user, email);
            //    if (user != null && !user.EmailConfirmed)
            //    {
            //        ModelState.AddModelError(string.Empty, "Email Not Confirmed Yet.");
            //        return View("Login", loginDTO);
            //    }
            //}

            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider,
                                        info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (signInResult.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                if (email != null)
                {
                    if (user == null)
                    {
                        user = new ApplicationUser
                        {
                            UserName = info.Principal.FindFirstValue(ClaimTypes.Email),
                            Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                        };
                        await _userManager.CreateAsync(user);

                        //var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                        //var confirmation = Url.Action("ConfirmEmail", "Account",
                        //                                new { userId = user.Id, token = token }, Request.Scheme);
                        //_logger.Log(LogLevel.Warning, confirmation);

                        //ViewBag.ErrorTitle = "Registration Successful";
                        //ViewBag.ErrorMessage = "Please confirm Email to Procced..";
                        //return View("Error");
                    }

                    await _userManager.AddLoginAsync(user, info);
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    return LocalRedirect(returnUrl);
                }

                ViewBag.ErrorTitle = $"Email Claim not received from : {info.LoginProvider}";
                ViewBag.ErrorMessage = "Please Contact support on a.shehata.code@gmail.com";

                return View("Error");
            }
        }

        [AllowAnonymous]
        public IActionResult AccountLocked()
        {
            return View();
        }

        #endregion


        #region LogOff
        [HttpPost]
        public async Task<IActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
        #endregion


        #region Forget Password
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgetPassword()
        {
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordDTO model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    ModelState.AddModelError("", "Email not Exist.");
                    return View(model);
                }

                if (user != null && await _userManager.IsEmailConfirmedAsync(user))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var passwordResetLink = Url.Action("ResetPassword", "Account", new { Email = model.Email, Token = token }, Request.Scheme);
                    var emailTo = user.Email;
                    var emailsubject = "Password Reset";
                    var emailbody = $"Please Reset your Password by clicking this link: <a href='{passwordResetLink}'>link</a>";

                    _logger.Log(LogLevel.Warning, passwordResetLink);
                    await _emailervice.SendEmailAsync(emailTo, emailsubject, emailbody);
                    return RedirectToAction("ConfirmForgetPassword");
                }
                return RedirectToAction("ConfirmForgetPassword");
            }
            return View(model);
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult ConfirmForgetPassword()
        {
            return View();
        }


        #endregion


        #region Reset Password
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string email, string token)
        {
            if (token == null || email == null)
            {
                ModelState.AddModelError("", "Invalid Password Reset Token");
            }
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);

                    if (result.Succeeded)
                    {
                        if (await _userManager.IsLockedOutAsync(user))
                        {
                            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
                        }
                        return RedirectToAction("ConfirmResetPassword");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }
                return View("ConfirmResetPassword");
            }
            return View(model);
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult ConfirmResetPassword()
        {
            return View();
        }
        #endregion


        #region Admin reset Password
        [HttpPost]
        [Authorize(Roles = "Admin, Super Admin")]
        public async Task<IActionResult> AdminResetPassword([FromBody] AdminResetPasswordDTO model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    ModelState.AddModelError("", "User not found.");
                    return View(model);
                }

                // Generate reset token
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

                // Attempt to reset the password
                var result = await _userManager.ResetPasswordAsync(user, resetToken, model.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation($"Password successfully reset by admin for user: {user.Email}");
                    return Json(new { success = true, message = "Password reset successfully." });
                }

                // Add error descriptions to the model state
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                _logger.LogWarning($"Failed password reset attempt for user: {user.Email}");
                return Json(new { success = false, errors = result.Errors.Select(e => e.Description) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while resetting password.");
                return Json(new { success = false, message = "An error occurred. Please try again later." });
            }
        }


        #endregion

        #region Change Password

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login");
                }

                if (model.CurrentPassword == model.NewPassword)
                {
                    ModelState.AddModelError("", "Cannot Use Same Old Password.");
                    return View();
                }

                var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!result.Succeeded)
                {
                    foreach (var Error in result.Errors)
                    {
                        ModelState.AddModelError("", Error.Description);
                    }
                    return View();
                }

                await _signInManager.RefreshSignInAsync(user);
                return View("ChangePasswordConfirmation");
            }
            return View(model);
        }

        public async Task<IActionResult> ChangePasswordConfirmation()
        {
            return View();
        }

        #endregion


        #endregion

    }
}
