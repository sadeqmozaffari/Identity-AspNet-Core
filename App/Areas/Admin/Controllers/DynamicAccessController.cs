using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using App.Services.Contracts;
using App.ViewModels.DynamicAccess;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using App.Entities.Identity;

namespace App.Areas.Admin.Controllers
{
    [DisplayName("سطح دسترسی پویا")]
    public class DynamicAccessController : BaseController
    {
        public readonly IApplicationUserManager _userManager;
        public readonly SignInManager<User> _signInManager;
       
     
        public readonly IApplicationRoleManager _roleManager;
        public readonly IMvcActionsDiscoveryService _mvcActionsDiscovery;
        public DynamicAccessController(IApplicationUserManager userManager, IMvcActionsDiscoveryService mvcActionsDiscovery, IApplicationRoleManager roleManager, SignInManager<User> signInManager)
        {
            _signInManager = signInManager;
            _roleManager = roleManager;
            _userManager = userManager;
            _mvcActionsDiscovery = mvcActionsDiscovery;
        }


   



        [HttpGet, DisplayName("تنظیمات سطح دسترسی پویا")]
        [Authorize(Policy = ConstantPolicies.DynamicPermission)]
        public async Task<IActionResult> Index(int userId)
        {
            if (userId == 0)
                return NotFound();


            var user = await _userManager.FindClaimsInUser(userId);
            if (user == null)
                return NotFound();

            var securedControllerActions = _mvcActionsDiscovery.GetAllSecuredControllerActionsWithPolicy(ConstantPolicies.DynamicPermission);
            return View(new DynamicAccessIndexViewModel
            {
                UserIncludeUserClaims = user,
                SecuredControllerActions = securedControllerActions,
            });
        }




        [HttpPost, ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Index(DynamicAccessIndexViewModel ViewModel)
        {
            var Result = await _userManager.AddOrUpdateClaimsAsync(ViewModel.UserId, ConstantPolicies.DynamicPermissionClaimType, ViewModel.ActionIds.Split(","));
            if (!Result.Succeeded)
                ModelState.AddModelError(string.Empty, "در حین انجام عملیات خطایی رخ داده است.");

            return RedirectToAction("Index", new { userId = ViewModel.UserId });
        }






        [HttpGet, DisplayName("تنظیمات سطح دسترسی نقش")]
        [Authorize(Policy = ConstantPolicies.DynamicPermission)]
        public async Task<IActionResult> Role(int RoleId)
        {
            if (RoleId == 0)
                return NotFound();


            var role = await _roleManager.FindClaimsInRole(RoleId);
            if (role == null)
                return NotFound();

            var securedControllerActions = _mvcActionsDiscovery.GetAllSecuredControllerActionsWithPolicy(ConstantPolicies.DynamicPermission);
            return View(new DynamicAccessIndexViewModel
            {
                RoleIncludeRoleClaims = role,
                SecuredControllerActions = securedControllerActions,
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Role(DynamicAccessIndexViewModel ViewModel)
        {
            var Result = await _roleManager.AddOrUpdateClaimsAsync(ViewModel.RoleId, ConstantPolicies.DynamicPermissionClaimType, ViewModel.ActionIds.Split(","));
            if (!Result.Succeeded)
                ModelState.AddModelError(string.Empty, "در حین انجام عملیات خطایی رخ داده است.");



            return RedirectToAction("Role", new { RoleId = ViewModel.RoleId });
        }
    }
}