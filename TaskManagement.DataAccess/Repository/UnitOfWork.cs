using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.DataAccess.Context;
using TaskManagement.DataAccess.Repository.IRepository;

namespace TaskManagement.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;
        public ITaskTDRepository TaskTDRepository { get; private set; }
        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            TaskTDRepository = new TaskTDRepository(db);
        }

        public async Task<int> CompleteAsync()
        { return await _db.SaveChangesAsync(); }

        public void Dispose()
            { _db.Dispose();  }
    }
}
