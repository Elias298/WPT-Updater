using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

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

        public static void SetProfileNumber_helper(int profilenumber=1)
        { 
            Log.WriteLine($"setting profile number to {profilenumber}");
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["Profile"].Value = profilenumber.ToString();
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
            Log.WriteLine($"Profile number set to {profilenumber}");
            Console.WriteLine("Profile number set to " + profilenumber);
        }

        public static void SetProfileNumber()
        {
            List<string> profiles = GetProfiles();

            string selected = ShowSelectionDialog(profiles);

            int profileNum = 0;

            for (int i = 0; i < profiles.Count; i++)
            {
                if (profiles[i].Equals(selected))
                {
                    profileNum = i;
                    break;
                }
            }

            Console.WriteLine("Selected profile: " + selected + "\nProfile number: " + profileNum);

            SetProfileNumber_helper(profileNum);
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

        //this method takes as an input a list of strings (from getProfiles()), produces the box with the buttons, and returns the 
        //string chosen by the user

        static string ShowSelectionDialog(List<string> items)
        {
            using (var form = new ButtonSelectionForm(items))
            {
                var result = form.ShowDialog();
                return result == DialogResult.OK ? form.SelectedItem : null;
            }
        }

        //this class defines the box containing the different buttons

        public class ButtonSelectionForm : Form
        {
            public string SelectedItem { get; private set; }

            public ButtonSelectionForm(List<string> items)
            {
                this.Text = "Select an Option";
                this.Size = new Size(300, 400);
                this.StartPosition = FormStartPosition.CenterScreen;
                this.AutoScroll = true;

                int yOffset = 10;
                foreach (var item in items)
                {
                    var button = new Button
                    {
                        Text = item,
                        Size = new Size(250, 40),
                        Location = new Point(10, yOffset)
                    };

                    button.Click += (sender, e) =>
                    {
                        SelectedItem = ((Button)sender).Text;
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    };

                    this.Controls.Add(button);
                    yOffset += 50;
                }
            }
        }


    }

}
