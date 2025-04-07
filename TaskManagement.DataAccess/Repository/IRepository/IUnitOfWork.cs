﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork 
    {
        ITaskItemRepository TaskItem { get; }
        IApplicationUserRepository ApplicationUser { get; }
        IHistoryRepository History { get; }
        void Save();
    }
}
