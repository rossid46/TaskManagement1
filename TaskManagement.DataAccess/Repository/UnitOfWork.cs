﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.DataAccess.Context;
using TaskManagement.DataAccess.Interfaces;

namespace TaskManagement.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _db;
        public ITaskItemRepository TaskItem { get; private set; }
        public IApplicationUserRepository ApplicationUser { get; private set; }
        public IHistoryRepository History { get; private set; }

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            TaskItem = new TaskItemRepository(_db);
            ApplicationUser = new ApplicationUserRepository(_db);
            History = new HistoryRepository(_db);
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
