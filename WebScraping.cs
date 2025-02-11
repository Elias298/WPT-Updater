using System;

namespace WPT_Updater
{
    internal static class WebScraping
    {
        public static string GetAppWebsite(string appName)
        {
            if (string.IsNullOrWhiteSpace(appName))
            {
                throw new ArgumentException("App name cannot be empty.");
            }

            return $"https://www.{appName.ToLower()}.com";
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

