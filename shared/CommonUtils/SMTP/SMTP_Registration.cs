using System.Net;
using System.Net.Mail;
namespace CommonUtils.SMTP;
public class SMTP_Registration
{
    public static string GenerateVerificationCode()
    {
        Random random = new Random();
        return random.Next(1000, 9999).ToString();
    }

    public static void EmailSender(string ToAddress, string firstName, string verificationCode)
    {
        string senderEmail = "nikalobjanidze014@gmail.com";
        string appPassword = ""; // APP PASSWORD

        /// HTML CONTENT OF EMAIL  /// HTML CONTENT OF EMAIL  /// HTML CONTENT OF EMAIL
        /// HTML CONTENT OF EMAIL  /// HTML CONTENT OF EMAIL  /// HTML CONTENT OF EMAIL
        string htmlContent = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>ArtPlatform Verification Code</title>
</head>
<body style='font-family: Segoe UI, Roboto, Helvetica, sans-serif; background-color: #f5f5f5; margin: 0; padding: 0;'>
    <table style='width: 100%; max-width: 650px; margin: 40px auto; background-color: #ffffff; border-radius: 12px; box-shadow: 0 6px 18px rgba(0,0,0,0.1); overflow: hidden;'>
        <tr>
            <td style='background: linear-gradient(90deg, #6A1B9A, #AB47BC, #F06292); color: #ffffff; text-align: center; padding: 35px 20px;'>
                <h1 style='font-size: 30px; margin: 0;'>Welcome to <span style='color: #FFF59D;'>Art</span><span style='color: #80DEEA;'>Platform</span></h1>
                <p style='margin-top: 10px; font-size: 17px;'>A Creative Space for Artists and Critics</p>
            </td>
        </tr>
        <tr>
            <td style='padding: 35px 45px;'>
                <p style='font-size: 18px; color: #333;'>Dear {firstName}</p>
                <p style='font-size: 16px; color: #444; line-height: 1.6;'>
                    Thank you for joining <strong>ArtPlatform</strong>! You're just one step away from unlocking a world of creative opportunities. Please use the verification code below to complete your registration:
                </p>
                <div style='text-align: center; margin: 30px 0;'>
                    <div style='display: inline-block; padding: 22px 35px; background-color: #E91E63; color: white; font-size: 34px; font-weight: bold; border-radius: 10px; letter-spacing: 4px;'>
                        {verificationCode}
                    </div>
                </div>
                <p style='font-size: 16px; color: #444;'>Enter this code in the verification field to confirm your email and start exploring the community.</p>
                <p style='font-size: 15px; color: #888; margin-top: 25px;'>If you didn’t create an account with us, please disregard this email.</p>
            </td>
        </tr>
        <tr>
            <td style='background-color: #fafafa; padding: 20px; text-align: center; font-size: 14px; color: #999;'>
                &copy; 2025 ArtPlatform. All rights reserved. | Created with ❤️ for art lovers
            </td>
        </tr>
    </table>
</body>
</html>";

        /// HTML CONTENT OF EMAIL  /// HTML CONTENT OF EMAIL  /// HTML CONTENT OF EMAIL
        /// HTML CONTENT OF EMAIL  /// HTML CONTENT OF EMAIL  /// HTML CONTENT OF EMAIL
        MailMessage mail = new MailMessage();
        mail.From = new MailAddress(senderEmail);
        mail.To.Add(ToAddress);
        mail.Subject = "ArtPlatform - Your Verification Code";
        mail.Body = htmlContent;
        mail.IsBodyHtml = true;

        var smtpClient = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            EnableSsl = true,
            Credentials = new NetworkCredential(senderEmail, appPassword),
        };
        smtpClient.Send(mail);
    }
}
