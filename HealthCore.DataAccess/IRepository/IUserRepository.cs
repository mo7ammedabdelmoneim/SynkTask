using SynkTask.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynkTask.DataAccess.IRepository
{
    public interface IUserRepository: IGenericRepository<ApplicationUser>
    {
    }
}
