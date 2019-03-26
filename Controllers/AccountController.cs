using BotDetect.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Vacation24.Models;
using Vacation24.Core;
using System.Configuration;
using Vacation24.Core.Payment;
using Vacation24.Services;
using Vacation24.Core.Mailer.Concrete;
using Vacation24.Core.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Vacation24.Controllers
{
    [Authorize]
    public class AccountController : CustomController
    {
        private string rootUrl => configuration.SiteConfiguration.RootPath;

        private readonly IActivationMail activationMail;
        private readonly IPasswordResetMail passwordResetMail;
        private readonly DefaultContext dbContext;
        private readonly IPaymentServices paymentServices;
        private readonly AppConfiguration configuration;
        private readonly UserManager<Profile> userManager;
        private readonly SignInManager<Profile> signInManager;
        private readonly IHttpContextAccessor httpContextAccessor;

        public AccountController(
            IActivationMail activationMail, 
            IPasswordResetMail passwordResetMail,
            IPaymentServices paymentServices,
            DefaultContext dbContext,
            AppConfiguration configuration,
            UserManager<Profile> userManager,
            SignInManager<Profile> signInManager,
            IHttpContextAccessor httpContextAccessor
        ){
            this.activationMail = activationMail;
            this.passwordResetMail = passwordResetMail;
            this.paymentServices = paymentServices;
            this.dbContext = dbContext;
            this.configuration = configuration;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.httpContextAccessor = httpContextAccessor;
        }
        
        [AllowAnonymous]
        public ActionResult Login(string reg)
        {
            ViewBag.isRegister = reg != null;
            ViewBag.requestForm = reg == null ? "seller" : reg;

            return View();
        }

        [AllowAnonymous]
        public ActionResult UserLogin()
        {
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserLogin(LoginModel model)
        {
            var userProfile = dbContext.Profiles
                .Where(p => p.UserName == model.Email)
                .FirstOrDefault();

            ViewBag.UserId = userProfile.Id;

            if (userProfile.Locked)
            {
                ModelState.AddModelError("", "This account has been locked. Pleas contact the Site administrator.");

                return View(model);
            }

            if (ModelState.IsValid && !await userManager.IsEmailConfirmedAsync(userProfile))
            {
                ViewBag.NotConfirmed = true;
            }

            if (ModelState.IsValid)
            {
                var profile = new Profile()
                {
                    Email = model.Email,
                    UserName = model.Email
                };
                var authResult = await signInManager.PasswordSignInAsync(profile, model.Password, true, false);
                if (authResult.Succeeded) {
                    // Store last user login
                    dbContext.UserIps.Add(new LoggedUserIp()
                    {
                        Ip = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
                        Logged = DateTime.Now,
                        UserId = userProfile.Id
                    });
                    dbContext.SaveChanges();
                    // Mark as successfull
                    ViewBag.Success = true;

                    return View(model);
                }
            }
            // Return error
            ModelState.AddModelError("", "E-mail or password are invalid.");

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ResendActivation(string userId)
        {
            var profile = dbContext.Profiles
                .Where(p => p.Id == userId)
                .FirstOrDefault();
            
            try {
                if (profile != null) {
                    await sendActivationEmail(profile);
                } else {
                    throw new Exception("User has not been found for activation resend.");
                }
            } catch (Exception exc) {
                return Json(
                    new ResultViewModel()
                    {
                        Status = (int)ResultStatus.Error,
                        Message = exc.Message
                    }
                );
            }

            return Json(
                new ResultViewModel()
                {
                    Status = (int)ResultStatus.Success,
                    Message = "Your activation email has been resent. Check your email."
                }
            );
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public ActionResult ShowUserIps(string userId)
        {
            var ipList = dbContext.UserIps
                .Where(uip => uip.Id == userId)
                .OrderByDescending(uip => uip.Logged)
                .ToList();

            return View(ipList);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();

            return View();
        }


        //Used in the main layout
        [AllowAnonymous]
        public ActionResult ShowLoggedStatus()
        {
            if (User.Identity.IsAuthenticated)
            {
                ViewBag.UserName = User.Identity.Name;
            }
            ViewBag.IsAuthenticated = User.Identity.IsAuthenticated;

            return View();
        }

        [AllowAnonymous]
        public ActionResult RegisterSeller()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [CaptchaValidation("RegisterSellerInput", "RegisterSellerCaptcha", "Invalid Captcha code")]
        public async Task<IActionResult> RegisterSeller(RegisterSellerModel model, bool captchaValid)
        {
            if (!captchaValid)
            {
                ModelState.AddModelError("RegisterSellerInput", "Invalid Captcha code");

                return View(model);
            }

            if (ModelState.IsValid && validateTos(model))
            {
                var newUser = new Profile()
                    {
                        UserName = model.UserName,
                        Name = model.CompanyName,
                        Address = model.Address,
                        PostalCode = model.PostalCode,
                        NIP = model.Nip,
                        City = model.City,

                        Contact = new PrevilegedContact()
                        {
                            FirstName = model.ContactFirstName,
                            LastName = model.ContactLastName,
                            Phone = model.ContactPhone
                        }
                    };

                var result = await userManager.CreateAsync(newUser);
                if (!result.Succeeded) {
                    foreach(var error in result.Errors) {
                        ModelState.AddModelError("", error.Description);
                    }

                    ViewBag.Success = false;
                } else {
                    await sendActivationEmail(newUser);

                    ViewBag.Success = true;
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult RegisterUser()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [CaptchaValidation("RegisterUserInput", "RegisterUserCaptcha", "Invalid Captcha code")]
        public async Task<IActionResult> RegisterUser(RegisterUserModel model, bool captchaValid)
        {
            if (!captchaValid)
            {
                ModelState.AddModelError("RegisterUserInput", "Invalid Captcha code");

                return View(model);
            }

            if (ModelState.IsValid && validateTos(model))
            {
                var newUser = new Profile()
                    {
                        UserName = model.UserName,
                        Name = model.Name,
                        Address = model.Address,
                        PostalCode = model.PostalCode,
                        NIP = String.Empty,
                        City = model.City,
                    };

                var result = await userManager.CreateAsync(newUser);
                if (!result.Succeeded) {
                    foreach(var error in result.Errors) {
                        ModelState.AddModelError("", error.Description);
                    }

                    ViewBag.Success = false;
                } else {
                    await sendActivationEmail(newUser);

                    ViewBag.Success = true;
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ActivateAccount(string userId, string token)
        {
            var targetUser = dbContext.Profiles
                .Where(p => p.Id == userId)
                .FirstOrDefault();

            var confirmResult = await userManager.ConfirmEmailAsync(targetUser, token);
            
            ViewBag.Result = confirmResult.Succeeded;
            
            return View();
        }

        #region Password Reset
        [HttpGet]
        public ActionResult ChangePassword()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            ViewBag.Success = false;

            if (ModelState.IsValid)
            {
                var profile = dbContext.Profiles
                    .FirstOrDefault(p => p.Id == User.Identity.Name);
                var result = await userManager.ChangePasswordAsync(profile, model.CurrentPassword, model.NewPassword);

                if (!result.Succeeded)
                {
                    ModelState.AddModelError("CurrentPassword", "Niewłaściwe aktualne hasło.");

                    return View(model);
                }

                ViewBag.Success = true;
            }

            return View(model);
        }
        
        
        
        [HttpGet]
        [AllowAnonymous]
        public ActionResult PasswordReset()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PasswordReset(PasswordResetRequestModel model)
        {
            var profileToChange = dbContext.Profiles.FirstOrDefault(p => p.Email == model.Email);
            if (profileToChange != null)
            {
                string resetToken =
                    await userManager.GeneratePasswordResetTokenAsync(profileToChange);

                passwordResetMail.Name = profileToChange.Name;
                passwordResetMail.Url = Url.Action(
                    "PasswordResetFinal",
                    "Account",
                    new { token = resetToken, userId = profileToChange.Id }
                );
                passwordResetMail.Send(model.Email);

                ViewBag.Email = model.Email;
                ViewBag.Result = true;
            }else
                ViewBag.Result = false;

            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult PasswordResetFinal(string token, string userId)
        {
            ViewBag.ResetToken = token;
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PasswordResetFinal(PasswordResetModel model)
        {
            var userToChange = dbContext.Profiles.FirstOrDefault(p => p.Id == model.UserId);
            if (userToChange != null) {
                var resetResult = await userManager.ResetPasswordAsync(
                    userToChange,
                    model.PasswordResetToken,
                    model.NewPassword
                );
                ViewBag.ResultStatus = resetResult.Succeeded;
            } else {
                ViewBag.Result = false;
            }

            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> SetUserCookie()
        {
            Action<string, string> setCookie = (string key, string value) => {
                Response.Cookies.Append(key, value, new CookieOptions() { Expires = DateTime.Now.AddYears(1) });
            };

            var isAuthenticated = User.Identity.IsAuthenticated;
            var userId = User.Identity.Name;
            var user = dbContext.Profiles.FirstOrDefault(p => p.Id == userId);

            setCookie("IsLoggedIn", isAuthenticated.ToString());
            setCookie("UserId", userId);
            
            if (isAuthenticated && user != null) {
                var userRoles = await userManager.GetRolesAsync(user);
                setCookie("Role", userRoles.FirstOrDefault());

                var subscription = paymentServices
                    .GetUserServices(userId)
                    .FirstOrDefault();
                
                if (subscription != null && subscription.Expiriation > DateTime.Now) {
                    setCookie("Subscribed", "true");
                }

                return new EmptyResult();
            }

            setCookie("Subscribed", String.Empty);

            return new EmptyResult();
        }
        #endregion


        private bool validateTos(IRegisterModel model)
        {
            if (!model.Tos)
            {
                ModelState.AddModelError("Tos", "Należy zaakceptować warunki korzystania z serwisu.");
                return false;
            }
            return true;
        }

        private async Task sendActivationEmail(Profile user) {
            var activationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
            activationMail.Name = user.Name;
            activationMail.Url = Flurl.Url.Combine(
                rootUrl,
                Url.Action("ActivateAccount", "Account", new { token = activationToken, userId = user.Id })
            );
            if (user.IsOwner) {
                activationMail.SendSeller(user.Email);
            } else {
                activationMail.SendUser(user.Email);
            }
        }
    }
}
