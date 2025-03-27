using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork : IDisposable
    {
        ITaskTDRepository TaskTDRepository { get; }
        Task<int> CompleteAsync();
    }
}
