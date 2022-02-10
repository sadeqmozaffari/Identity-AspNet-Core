using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using App.Data.Contracts;
using App.Entities.Identity;
using App.Services.Contracts;
using App.ViewModels.Manage;
using App.Common;
using App.Common.Attributes;
using App.Entities;
using App.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using App.ViewModels.UserManager;

namespace App.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUnitOfWork _uw;
        private readonly IHttpContextAccessor _accessor;
        private readonly IApplicationUserManager _userManager;
        private readonly IApplicationRoleManager _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly IViewRenderService _viewRender;
        private const string BookmarkNotFound = "خبر بوکمارک شده یافت نشد.";
        public AccountController(IUnitOfWork uw, IHttpContextAccessor accessor, IApplicationUserManager userManager, IApplicationRoleManager roleManager, IEmailSender emailSender, SignInManager<User> signInManager, ILogger<AccountController> logger, IViewRenderService viewRender)
        {
            _uw = uw;
            _accessor = accessor;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _signInManager = signInManager;
            _logger = logger;
            _viewRender = viewRender; 
        }

        [HttpGet]
        public IActionResult SignIn()
        {
            return PartialView("_SignIn");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(SignInViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var User = await _userManager.FindByNameAsync(viewModel.UserName);
                if (User != null)
                {
                    if (User.IsActive)
                    {
                        var result = await _signInManager.PasswordSignInAsync(viewModel.UserName, viewModel.Password, viewModel.RememberMe, true);
                        if (result.Succeeded)
                            return Json("success");

                        else if (result.IsLockedOut)
                            ModelState.AddModelError(string.Empty, "حساب کاربری شما به مدت 20 دقیقه به دلیل تلاش های ناموفق قفل شد.");

                        else if (result.RequiresTwoFactor)
                            return Json("requiresTwoFactor");

                        else
                        {
                            _logger.LogWarning($"The user attempts to login with the IP address({_accessor.HttpContext?.Connection?.RemoteIpAddress.ToString()}) and username ({viewModel.UserName}) and password ({viewModel.Password}).");
                            ModelState.AddModelError(string.Empty, "نام کاربری یا کلمه عبور شما صحیح نمی باشد.");
                        }
                    }
                    else
                        ModelState.AddModelError(string.Empty, "حساب کاربری شما غیرفعال است.");
                }

                else
                {
                    _logger.LogWarning($"The user attempts to login with the IP address({_accessor.HttpContext?.Connection?.RemoteIpAddress.ToString()}) and username ({viewModel.UserName}) and password ({viewModel.Password}).");
                    ModelState.AddModelError(string.Empty, "نام کاربری یا کلمه عبور شما صحیح نمی باشد.");
                }
            }

            return PartialView("_SignIn");
        }



        [HttpGet]
        public IActionResult Register()
        {
            return PartialView("_Register");
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var user = new User { UserName = viewModel.UserName, Email = viewModel.Email, RegisterDateTime = DateTime.Now, IsActive = true ,FirstName="",LastName=""};
                IdentityResult result = await _userManager.CreateAsync(user, viewModel.Password);

                if (result.Succeeded)
                {
                    var role = await _roleManager.FindByNameAsync("کاربر");
                    if (role == null)
                        await _roleManager.CreateAsync(new Role("کاربر"));

                    result = await _userManager.AddToRoleAsync(user, "کاربر");

                    if (result.Succeeded)
                    {
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var callbackUrl = Url.Action("ConfirmEmail", "Account", values: new { userId = user.Id, code = code }, protocol: Request.Scheme);
                        await _emailSender.SendEmailAsync(viewModel.Email, "تایید حساب کاربری - سایت میزفا", $"<div dir='rtl' style='font-family:tahoma;font-size:14px'>لطفا با کلیک روی لینک رویه رو حساب کاربری خود را فعال کنید.  <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>کلیک کنید</a></div>");

                        TempData["notification"] = $" ایمیل فعال سازی حساب کاربری به {viewModel.Email} ارسال شد. ";
                    }
                }

                ModelState.AddErrorsFromResult(result);
            }

            return PartialView("_Register");
        }


        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
                return RedirectToAction("Index", "Home");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound($"Unable to load user with ID '{userId}'");

            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (!result.Succeeded)
                throw new InvalidOperationException($"Error Confirming email for user with ID '{userId}'");

            return View();
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }


        #region ForgotPassword

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgetPasswordViewModel ViewModel)
        {
            if (ModelState.IsValid)
            {
                var User = await _userManager.FindByEmailAsync(ViewModel.Email);
                if (User == null)
                    ModelState.AddModelError(string.Empty, "ایمیل شما صحیح نمی باشد.");
                else
                {
                    if (!await _userManager.IsEmailConfirmedAsync(User))
                        ModelState.AddModelError(string.Empty, "لطفا با تایید ایمیل حساب کاربری خود را فعال کنید.");
                    else
                    {
                        var Code = await _userManager.GeneratePasswordResetTokenAsync(User);
                        var CallbackUrl = Url.Action("ResetPassword", "Account", values: new { Code }, protocol: Request.Scheme);
                       // await _emailSender.SendEmailAsync(ViewModel.Email, "بازیابی کلمه عبور", $"<p style='font-family:tahoma;font-size:14px'> برای بازنشانی کلمه عبور خود <a href='{HtmlEncoder.Default.Encode(CallbackUrl)}'>اینجا کلیک کنید</a> </p>");
                        SendEmail.Send(ViewModel.Email, "بازیابی کلمه عبور", $"<p style='font-family:tahoma;font-size:14px'> برای بازنشانی کلمه عبور خود <a href='{HtmlEncoder.Default.Encode(CallbackUrl)}'>اینجا کلیک کنید</a> </p>");
                        return RedirectToAction("ForgotPasswordConfirmation");
                    }
                }
            }

            return View(ViewModel);
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string Code = null)
        {
            if (Code == null)
                return NotFound();
            else
            {
                var ViewModel = new ViewModels.Manage.ResetPasswordViewModel { Code = Code };
                return View(ViewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ViewModels.Manage.ResetPasswordViewModel ViewModel)
        {
            if (ModelState.IsValid)
            {
                var User = await _userManager.FindByEmailAsync(ViewModel.Email);
                if (User == null)
                    ModelState.AddModelError(string.Empty, "ایمیل شما صحیح نمی باشد.");

                else
                {
                    var Result = await _userManager.ResetPasswordAsync(User, ViewModel.Code, ViewModel.Password);
                    if (Result.Succeeded)
                        return RedirectToAction("ResetPasswordConfirmation");
                    else
                    {
                        foreach (var error in Result.Errors)
                            ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            return View(ViewModel);
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
        #endregion


    }
}
