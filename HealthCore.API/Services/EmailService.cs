using SynkTask.API.Services.IService;
using SynkTask.DataAccess.Repository.IRepository;

namespace SynkTask.API.Services
{
    public class EmailService:IEmailService
    {
        private readonly IEmailSender emailSender;

        public EmailService(IEmailSender emailSender)
        {
            this.emailSender = emailSender;
        }
        public async Task SendNewTaskEmailEmailAsync(string email, string userName, string projectName, string taskTitle, string teamleadName, string dueDate)
        {
            var subject = "New Task Assigned to You";

            var message = $""""
                Hello {userName},

                You have been assigned a new task in the project "{projectName}".

                Task Details:
                - Task: {taskTitle}
                - Assigned By: {teamleadName}
                - Due Date: {dueDate}

                Please log in to the system to review the task and start working on it.

                Best regards,
                Task Management System
                
                """";

            await emailSender.SendEmailAsync(email, subject, message);
        }
        public async Task SendTaskUpdatedEmailEmailAsync(string email, string userName, string projectName, string taskTitle, string newStatus)
        {
            var subject = "Task Status Update Notification";

            var message = $""""
                Hello {userName},

                The status of the following task has been updated:

                Task: {taskTitle}
                Project: {projectName}
                New Status: {newStatus}

                Please log in to the system for more details.

                Best regards,
                Task Management System
                
                """";

            await emailSender.SendEmailAsync(email, subject, message);
        }
        public async Task SendTaskCompletedEmailEmailAsync(string email, string userName, string projectName, string taskTitle)
        {
            var subject = "Task Completed – Project Update";

            var message = $""""
                Hello {userName},

                We’re happy to inform you that a task has been successfully completed.

                Task Details:

                Task Title: {taskTitle}

                Project: {projectName}

                This update indicates progress in the project workflow.
                You can log in to the system to review the task details or take any further action if needed.

                Best regards,
                Task Management System

                """";

            await emailSender.SendEmailAsync(email, subject, message);
        }

    }
}
