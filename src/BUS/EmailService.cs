using System.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace BUS
{
    // Chỉ làm 1 việc: gửi mail. Cấu hình đọc từ App.config (mục appSettings)
    public static class EmailService
    {
        public static void Gui(string denEmail, string tieuDe, string noiDungHtml)
        {
            string host = ConfigurationManager.AppSettings["SmtpHost"];
            int port = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
            string tuEmail = ConfigurationManager.AppSettings["SmtpUser"];
            string matKhau = ConfigurationManager.AppSettings["SmtpPassword"];

            var mail = new MimeMessage();
            mail.From.Add(MailboxAddress.Parse(tuEmail));
            mail.To.Add(MailboxAddress.Parse(denEmail));
            mail.Subject = tieuDe;
            mail.Body = new TextPart("html") { Text = noiDungHtml };

            using (var client = new SmtpClient())
            {
                client.Connect(host, port, SecureSocketOptions.StartTls);
                client.Authenticate(tuEmail, matKhau);
                client.Send(mail);
                client.Disconnect(true);
            }
        }
    }
}
