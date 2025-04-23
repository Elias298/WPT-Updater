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

        public static void SetProfileNumber_helper(int profilenumber=1)
        { 
            Log.WriteLine($"setting profile number to {profilenumber}");
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["Profile"].Value = profilenumber.ToString();
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
            ProfileNumber = profilenumber;
            Log.WriteLine($"Profile number set to {profilenumber}");
            Console.WriteLine("Profile number set to " + profilenumber);
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


        public static int SetProfileNumber(string title = "Enter Chrome Profile number", string message = "Enter your Chrome Profile number")
        {
            int result;

            while (true)
            {
                Form form = new Form()
                {
                    Width = 300,
                    Height = 160,
                    Text = title,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    StartPosition = FormStartPosition.CenterScreen,
                    MinimizeBox = false,
                    MaximizeBox = false
                };

                Label label = new Label() { Left = 10, Top = 20, Text = message, Width = 260 };
                TextBox inputBox = new TextBox() { Left = 10, Top = 50, Width = 260 };
                Button okButton = new Button() { Text = "OK", Left = 100, Width = 80, Top = 80, DialogResult = DialogResult.OK };

                form.Controls.Add(label);
                form.Controls.Add(inputBox);
                form.Controls.Add(okButton);
                form.AcceptButton = okButton;

                if (form.ShowDialog() == DialogResult.OK)
                {
                    if (int.TryParse(inputBox.Text, out result) && result >= 0)
                    {
                        Auth.SetProfileNumber_helper(result);
                    }
                    else
                    {
                        MessageBox.Show("Invalid input. Profile number is a positive integer.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    Auth.SetProfileNumber_helper(Auth.GetProfileNumber());
                }
            }
        }



    }

}
