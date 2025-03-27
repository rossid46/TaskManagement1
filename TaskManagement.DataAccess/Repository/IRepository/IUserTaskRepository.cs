﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Models;

namespace TaskManagement.DataAccess.Repository.IRepository
{
    public interface ITaskTDRepository : IRepository<TaskItem>
    {
    }
}
