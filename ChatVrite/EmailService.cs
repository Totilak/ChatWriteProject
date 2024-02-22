using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using System.Windows;

namespace ChatVrite
{
    public class EmailService
    {
        private const string EmailAddress = "write.chat@internet.ru";
        private const string EmailPassword = "QqaYxY5hdCzXAsyUtXtF";

        public void SendVerificationCode(string userEmail, string verificationCode)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.mail.ru");

                mail.From = new MailAddress(EmailAddress);
                mail.To.Add(userEmail);
                mail.Subject = "Подтверждение почты";
                mail.Body = "Ваш пятизначный код подтверждения: " + verificationCode;

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new NetworkCredential(EmailAddress, EmailPassword);
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);
                MessageBox.Show("Письмо отправленно на вашу почту!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка отправки письма. Error: " + ex.Message);
            }
            
        }
    }
}
