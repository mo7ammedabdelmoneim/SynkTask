using SynkTask.DataAccess.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SynkTask.DataAccess.Repository.IRepository;
using SynkTask.Models.Models;

namespace SynkTask.DataAccess.Repository
{
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(
             ApplicationDbContext context,
             ILogger<GenericRepository<Notification>> logger) : base(context, logger)
        {
        }

    }
}
