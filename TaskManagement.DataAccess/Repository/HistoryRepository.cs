using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskManagement.DataAccess.Context;
using TaskManagement.Models;

namespace TaskManagement.DataAccess.Repository
{
    public class HistoryRepository : Repository<History>, DataAccess.Interfaces.IHistoryRepository
    {
        public HistoryRepository(ApplicationDbContext context) : base(context) { }
    }
}
