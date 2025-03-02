using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WPT_Updater
{
    class Auth
    {

        public static int ProfileNumber = GetProfileNumber();
        public static string UserName = Environment.UserName;


        public static List<string> GetProfiles()
        {
            List<string> profilelist = new();
            string localStatePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                             "Google", "Chrome", "User Data", "Local State");

            if (!File.Exists(localStatePath))
            {
                Log.WriteLine("Local State file not found.");
                return profilelist;
            }

            try
            {
                string json = File.ReadAllText(localStatePath);
                JObject jsonObj = JObject.Parse(json);

                // Ensure profile key exists
                if (jsonObj?["profile"]?["info_cache"] is not JObject profiles)
                {
                    Log.WriteLine("lol");
                    return profilelist;
                }

                Console.WriteLine("Chrome Profiles:");

                
                foreach (var profile in profiles.Properties())
                {
                    string? profileName = profile.Value?["name"]?.ToString();
                    if (!string.IsNullOrEmpty(profileName))
                    {
                        profilelist.Add(profileName);
                    }
                }
                return profilelist;

            }
            catch (Exception ex)
            {
                Log.WriteLine("Error reading Local State file: " + ex.Message);
                return profilelist;
            }
        }

        public static void SetProfileNumber()
        {

            //display profiles using GetProfiles()
            //user inputs profile number
            string profilenumber = "1"; //example
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["Profile"].Value = profilenumber;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        public static int GetProfileNumber()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            int profile;
            int.TryParse(ConfigurationManager.AppSettings["Profile"], out profile);
            return profile;
        }


    }

}
