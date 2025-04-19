using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

public class EmailSender : IEmailSender
{
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
		MailMessage mailMessage = new MailMessage();
		mailMessage.From = new MailAddress("kluczsukces@gmail.com");
		mailMessage.To.Add(new MailAddress(email));
		mailMessage.Subject = subject;
		mailMessage.Body = htmlMessage;
		mailMessage.IsBodyHtml = true;

		SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
		smtpClient.EnableSsl = true;
		smtpClient.Credentials = new NetworkCredential(
			"kluczsukces@gmail.com", "vlva suhh dvuh fnpo");
		smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
		await smtpClient.SendMailAsync(mailMessage);
    }
}
