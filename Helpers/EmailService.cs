using System;
using System.IO;

namespace RealEstate.Helpers
{
    public static class EmailService
    {
        private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "verification_emails.log");

        public static void SendVerificationCode(string email, string code)
        {
            var message = $"==================================================\n" +
                          $"📧 TO: {email}\n" +
                          $"🔑 YOUR VERIFICATION CODE IS: {code}\n" +
                          $"⏰ SENT AT: {DateTime.Now}\n" +
                          $"==================================================";

            // Log to console
            Console.WriteLine(message);

            // Log to local file in case the user misses console output
            try
            {
                File.AppendAllText(LogFilePath, message + "\n\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmailService] Failed to write to log file: {ex.Message}");
            }
        }
    }
}
