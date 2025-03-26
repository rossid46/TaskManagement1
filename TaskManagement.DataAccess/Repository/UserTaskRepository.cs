using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.DataAccess.Context;
using TaskManagement.DataAccess.Repository.IRepository;
using TaskManagement.Models;

namespace TaskManagement.DataAccess.Repository
{
    public class UserTaskRepository : Repository<UserTask>, IUserTaskRepository
    {
        public UserTaskRepository(ApplicationDbContext context) : base(context) { }
    }
}
