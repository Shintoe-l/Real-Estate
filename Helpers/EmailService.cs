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
        public static void SendMaintenanceReminder(string email, string landlordName, string propertyAddress, string requestDescription)
        {
            var message = $"==================================================\n" +
                          $"📧 TO: {email}\n" +
                          $"⚠️ URGENT: UNRESOLVED MAINTENANCE REQUEST\n" +
                          $"Dear {landlordName},\n\n" +
                          $"This is an automated reminder that a maintenance request for your property at {propertyAddress} remains unresolved.\n\n" +
                          $"Issue Description:\n" +
                          $"{requestDescription}\n\n" +
                          $"Please review and resolve this request as soon as possible.\n" +
                          $"⏰ SENT AT: {DateTime.Now}\n" +
                          $"==================================================";

            Console.WriteLine(message);

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
