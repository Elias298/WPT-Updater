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
            Log.WriteLine($"Trying to find chrome profiles");
            List<string> profilelist = new();
            string localStatePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                             "Google", "Chrome", "User Data", "Local State");
            Log.WriteLine($"From {localStatePath}");

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
                    Log.WriteLine("Profile key doesn't exist");
                    return profilelist;
                }

                Log.WriteLine("Chrome Profiles:");
                
                foreach (var profile in profiles.Properties())
                {
                    string? profileName = profile.Value?["name"]?.ToString();
                    if (!string.IsNullOrEmpty(profileName))
                    {
                        profilelist.Add(profileName);
                        Log.Write($"{profileName} , ");
                    }
                }
                Log.WriteLine("");
                return profilelist;

            }
            catch (Exception ex)
            {
                Log.WriteLine("Error reading Local State file: " + ex.Message);
                return profilelist;
            }
        }

        public static void SetProfileNumber(int profilenumber=1)
        {
            Log.WriteLine($"setting profile number to {profilenumber}");
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["Profile"].Value = profilenumber.ToString();
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
            Log.WriteLine($"Profile number set to {profilenumber}");
        }

        public static int GetProfileNumber()
        {
            Log.WriteLine("finding profile number in config");
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            int profile;
            int.TryParse(ConfigurationManager.AppSettings["Profile"], out profile);
            Log.WriteLine($"profile number {profile} will be used for selenium");
            return profile;
        }


    }

}
