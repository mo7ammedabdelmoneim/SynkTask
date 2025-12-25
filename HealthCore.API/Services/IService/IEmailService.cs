using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynkTask.API.Services.IService
{
    public interface IEmailService
    {
        Task SendNewTaskEmailEmailAsync(string email, string userName, string projectName, string taskTitle, string teamleadName, string dueDate);
        Task SendTaskUpdatedEmailEmailAsync(string email, string userName, string projectName, string taskTitle, string newStatus);
        Task SendTaskCompletedEmailEmailAsync(string email, string userName, string projectName, string taskTitle);
    }
}
