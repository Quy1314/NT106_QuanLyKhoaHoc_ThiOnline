using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace CourseGuard.Backend.Services
{
    public class SmtpEmailService
    {
        private const string DefaultSmtpHost = "smtp.gmail.com";
        private const int DefaultSmtpPort = 587;
        private const string DefaultSmtpUser = "24521494@gm.uit.edu.vn";
        private const string DefaultSmtpPassword = "mtlm rmvr dvrn ytsv";
        private const string DefaultFromName = "CourseGuard Admin";
        private readonly string _host;
        private readonly int _port;
        private readonly string _username;
        private readonly string _password;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public SmtpEmailService()
        {
            _host = Environment.GetEnvironmentVariable("SMTP_HOST") ?? DefaultSmtpHost;
            _port = int.TryParse(Environment.GetEnvironmentVariable("SMTP_PORT"), out int parsedPort) ? parsedPort : DefaultSmtpPort;
            _username = Environment.GetEnvironmentVariable("SMTP_USER") ?? DefaultSmtpUser;
            _password = Environment.GetEnvironmentVariable("SMTP_PASS") ?? DefaultSmtpPassword;
            _fromEmail = Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL") ?? _username;
            _fromName = Environment.GetEnvironmentVariable("SMTP_FROM_NAME") ?? DefaultFromName;
        }

        public bool SendEmail(string toEmail, string subject, string body, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(toEmail))
            {
                errorMessage = "Email người nhận không hợp lệ.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(_username) || string.IsNullOrWhiteSpace(_password))
            {
                errorMessage = "Thiếu cấu hình SMTP_USER/SMTP_PASS.";
                return false;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_fromName, _fromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("plain")
            {
                Text = body
            };

            try
            {
                using var client = new SmtpClient();
                client.Connect(_host, _port, SecureSocketOptions.StartTls);
                client.Authenticate(_username, _password);
                client.Send(message);
                client.Disconnect(true);
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Gửi SMTP thất bại: {ex.Message}";
                return false;
            }
        }
    }
}
