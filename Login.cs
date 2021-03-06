﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Runtime.InteropServices;

namespace MarsClientLauncher
{
    public partial class Login : UserControl
    {
        public static readonly HttpClient client = new HttpClient();
        public readonly string yggdrasil = "https://authserver.mojang.com";

        public Login()
        {
            InitializeComponent();
            Form1.SetBorderCurve(25, confirmButton);
            DoubleBuffered = true;

            offlineCheckBox.title.Text = "Play Offline";
            offlineCheckBox.subtitle.Text = "(Only username is required)";
        }
        public static int Lerp(int a, int b, double t)
        {
            return (int)Math.Round((1-t) * a + t * b);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(!string.IsNullOrEmpty(Data.accessToken))
            {
                SendToBack();
            }
        }

        private void confirmButton_Click(object sender, EventArgs e)
        {
            if(!offlineCheckBox.isChecked)
                AttemptLoginWithoutToken();
            else
            {
                string uname = usernameBox.Text;
                if (uname.Equals("Username") || string.IsNullOrEmpty(uname))
                {
                    MessageBox.Show("Please enter a username! You don't have to enter a password for this option.", "Mars", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                Data.username = uname;
                Data.offline = true;
                SendToBack();
            }
        }
        public void AttemptLoginWithoutToken()
        {
            string username = usernameBox.Text;
            string password = passwordBox.Text;
            if (string.IsNullOrEmpty(username) || username.Equals("Username"))
            {
                MessageBox.Show("Please enter a username.", "Mars", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrEmpty(password) || password.Equals("Password"))
            {
                MessageBox.Show("Please enter a password.", "Mars", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string firstpost;
            if(File.Exists("mars_client\\clientToken.tok"))
            {
                string token = File.ReadAllText("mars_client\\clientToken.tok");
                firstpost =
                "{\n" +
                "   \"agent\": {\n" +
                "       \"name\": \"Minecraft\",\n" +
                "       \"version\": 1\n" +
                "   },\n" +
                "   \"username\": \"" + username + "\",\n" +
                "   \"password\": \"" + password + "\",\n" +
                "   \"clientToken\": \"" + token + "\",\n" +
                "   \"requestUser\": true\n" +
                "}";
            } else {
                firstpost =
                "{\n" +
                "   \"agent\": {\n" +
                "       \"name\": \"Minecraft\",\n" +
                "       \"version\": 1\n" +
                "   },\n" +
                "   \"username\": \"" + username + "\",\n" +
                "   \"password\": \"" + password + "\",\n" +
                "   \"requestUser\": true\n" +
                "}";
            }

            var response = POST(firstpost, yggdrasil + "/authenticate");
            JObject j = JObject.Parse(response);
            if (j["errorMessage"] != null)
            {
                string error = j["errorMessage"].ToString();
                MessageBox.Show(error, "Mars", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // First login
            string atoken = j["accessToken"].ToString();
            string ctoken = j["clientToken"].ToString();
            string mjuuid = j["user"]["id"].ToString();
            string name = j["availableProfiles"][0]["name"].ToString();
            string profileuuid = j["selectedProfile"]["id"].ToString();

            File.WriteAllText("mars_client\\accessToken.tok", atoken);
            if(!File.Exists("mars_client\\clientToken.tok"))
            {
                File.WriteAllText("mars_client\\clientToken.tok", ctoken);
            }
            Data.accessToken = atoken;
            Data.mojangUUID = mjuuid;
            Data.clientToken = ctoken;
            Data.username = name;
            Data.mcUUID = profileuuid;
            Data.offline = false;
            SendToBack();
        }
        public void AttemptLogin()
        {
            string atoken = File.ReadAllText("mars_client\\accessToken.tok");
            string ctoken = File.ReadAllText("mars_client\\clientToken.tok");

            string validate = "{\n" +
            "   \"accessToken\": \"" + atoken + "\",\n" +
            "   \"clientToken\": \"" + ctoken + "\",\n" +
            "   \"requestUser\": true\n" +
            "}";
            bool isValid = ValidateAccessToken(validate, yggdrasil + "/validate");
            string uname = "";
            string uuid = "";
            string profileuuid = "";
            if (!isValid)
            {
                return;
            } else {
                try
                {
                    string rsp = POST(validate, yggdrasil + "/refresh");
                    JObject jo = JObject.Parse(rsp);
                    atoken = jo["accessToken"].ToString();
                    ctoken = jo["clientToken"].ToString();
                    uname = jo["selectedProfile"]["name"].ToString();
                    uuid = jo["user"]["id"].ToString();
                    profileuuid = jo["selectedProfile"]["id"].ToString();
                    File.WriteAllText("mars_client\\accessToken.tok", atoken);
                    File.WriteAllText("mars_client\\clientToken.tok", ctoken);
                } catch(Exception exc) {
                    MessageBox.Show(exc.Message + "\n\n" + exc.StackTrace, "Mars", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            Data.accessToken = atoken;
            Data.mojangUUID = uuid;
            Data.clientToken = ctoken;
            Data.username = uname;
            Data.mcUUID = profileuuid;
            Data.offline = false;
            SendToBack();
        }

        public string POST(Dictionary<string, string> _data, string url)
        {
            var data = new FormUrlEncodedContent(_data);
            data.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var _response = client.PostAsync(url, data);
            return _response.Result.Content.ReadAsStringAsync().Result;
        }
        public string POST(string content, string url)
        {
            var data = new StringContent(content, Encoding.UTF8, "application/json");
            var _response = client.PostAsync(url, data);
            return _response.Result.Content.ReadAsStringAsync().Result;
        }
        public bool ValidateAccessToken(string content, string url)
        {
            var data = new StringContent(content, Encoding.UTF8, "application/json");
            var _response = client.PostAsync(url, data);
            if(_response.Result.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return false;
            }
            return true;
        }

        private void passwordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                confirmButton_Click(this, new EventArgs());
            }
        }
        private void usernameBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                confirmButton_Click(this, new EventArgs());
            }
        }

        private void Login_Load(object sender, EventArgs e)
        {
            timer1.Start();

            if (File.Exists("mars_client\\accessToken.tok")
             && File.Exists("mars_client\\clientToken.tok"))
            {
                AttemptLogin();
            }
        }

        private void Login_MouseDown(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                Data.Capture(Parent.Handle);
            }
        }
    }
}
