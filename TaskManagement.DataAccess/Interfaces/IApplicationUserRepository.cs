﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Models;

namespace TaskManagement.DataAccess.Interfaces
{
    public interface IApplicationUserRepository : Interfaces<ApplicationUser>
    {
        void Update(ApplicationUser applicationUser);
    }
}
