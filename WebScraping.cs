using System;
using System.Collections.Generic;

namespace WPT_Updater
{
    internal static class WebScraping
    {
        private static readonly Dictionary<string, string> appWebsites = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Facebook", "https://www.facebook.com" },
            { "Twitter", "https://www.twitter.com" },
            { "Instagram", "https://www.instagram.com" },
            { "LinkedIn", "https://www.linkedin.com" },
            { "Snapchat", "https://www.snapchat.com" },
            { "WhatsApp", "https://www.whatsapp.com" },
            { "TikTok", "https://www.tiktok.com" }
        };

        public static string GetAppWebsite(string appName)
        {
            if (string.IsNullOrWhiteSpace(appName))
            {
                throw new ArgumentException("App name cannot be empty.");
            }

            if (!appWebsites.TryGetValue(appName, out string website))
            {
                throw new KeyNotFoundException($"Website for '{appName}' not found.");
            }

            if (!website.StartsWith("https://"))
            {
                throw new InvalidOperationException("Invalid website: does not use HTTPS.");
            }

            return website;
        }
    }

    class Program
    {
        static void Main()
        {
            Console.Write("Enter app name: ");
            string appName = Console.ReadLine()?.Trim();

            try
            {
                string website = WebScraping.GetAppWebsite(appName);
                Console.WriteLine($"Website for {appName}: {website}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
