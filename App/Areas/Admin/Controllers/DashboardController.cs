using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using App.Common;
using App.Data.Contracts;
using App.ViewModels.DynamicAccess;

namespace App.Areas.Admin.Controllers
{
    [DisplayName("داشبورد")]
    [Authorize]
    public class DashboardController : BaseController
    {
        private readonly IUnitOfWork _uw;
        public DashboardController(IUnitOfWork uw)
        {
            _uw = uw;
        }

        //[HttpGet, DisplayName("مشاهده")]
        //[Authorize(Policy = ConstantPolicies.DynamicPermission)]
        public IActionResult Index()
        {
            return View();
        }
    }
}