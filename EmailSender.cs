namespace Inventory_Management_System
{

    using Microsoft.AspNetCore.Identity.UI.Services;

    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // For now do nothing
            return Task.CompletedTask;
        }
    }
}
