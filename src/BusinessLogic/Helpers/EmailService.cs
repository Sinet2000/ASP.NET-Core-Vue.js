using BusinessLogic.Config;
using BusinessLogic.EmailTemplatesModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using RazorLight;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace BusinessLogic.Helpers
{
    public class EmailService : IEmailService
    {
        public IHostingEnvironment HostingEnvironment { get; }
        public SmtpConfig SmtpConfig { get; }

        private string PathToEmailTemplates { get; set; }

        public EmailService(IHostingEnvironment hostingEnvironment, IOptions<SmtpConfig> smtpConfig)
        {
            HostingEnvironment = hostingEnvironment;
            SmtpConfig = smtpConfig.Value;

            PathToEmailTemplates = HostingEnvironment.ContentRootPath + "\\EmailTemplates";
        }

        private async Task SendEmail(string body, string subject, string toAddresses)
        {
            SmtpClient client = new SmtpClient(SmtpConfig.Host, SmtpConfig.Port);
            client.EnableSsl = SmtpConfig.EnableSsl;
            client.Credentials = new NetworkCredential(SmtpConfig.UserName, SmtpConfig.Password);

            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(SmtpConfig.From);
            mailMessage.To.Add(toAddresses);
            mailMessage.IsBodyHtml = true;
            mailMessage.Body = body;
            mailMessage.Subject = subject;

            await client.SendMailAsync(mailMessage);
        }

        public async Task SendForgotPasswordEmail(string forgotPasswordLink, string toAddresses)
        {
            var engine = new RazorLightEngineBuilder().UseFilesystemProject(PathToEmailTemplates).Build();

            ForgotPasswordModel model = new ForgotPasswordModel() { LinkToResetPassword = forgotPasswordLink };
            string result = await engine.CompileRenderAsync("ForgotPassword.cshtml", model);

            await SendEmail(result, "Reset Password", toAddresses);
        }

        public async Task SendConfirmAccountEmail(string confirmAccountLink, string toAddresses)
        {
            var engine = new RazorLightEngineBuilder().UseFilesystemProject(PathToEmailTemplates).Build();

            ConfirmAccountModel model = new ConfirmAccountModel() { LinkToConfirmAccount = confirmAccountLink };
            string result = await engine.CompileRenderAsync("ConfirmAccount.cshtml", model);

            await SendEmail(result, "Please confirm your account", toAddresses);
        }
    }

    public interface IEmailService
    {
        Task SendForgotPasswordEmail(string forgotPasswordLink, string toAddresses);
        Task SendConfirmAccountEmail(string confirmAccountLink, string toAddresses);
    }
}