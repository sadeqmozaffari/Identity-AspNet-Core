﻿using App.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.ViewModels.DynamicAccess
{
    public class DynamicAccessIndexViewModel
    {
        public string ActionIds { set; get; }
        public int UserId { set; get; }
        public int RoleId { set; get; }
        public User UserIncludeUserClaims { set; get; }
        public Role RoleIncludeRoleClaims { set; get; }
        public ICollection<ControllerViewModel> SecuredControllerActions { set; get; }
    }
}
