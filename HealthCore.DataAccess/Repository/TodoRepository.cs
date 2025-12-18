using SynkTask.DataAccess.Data;
using SynkTask.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SynkTask.DataAccess.Repository.IRepository;

namespace SynkTask.DataAccess.Repository
{
    public class TodoRepository : GenericRepository<Todo>, ITodoRepository
    {
        public TodoRepository(
             ApplicationDbContext context,
             ILogger<GenericRepository<Todo>> logger) : base(context, logger)
        {
        }

    }
}
