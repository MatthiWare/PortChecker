using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Xml.Serialization;

namespace MatthiWare.PortChecker
{
    [Serializable]
    public struct IP_SETTINGS_FILE
    {
        public String IP;

        public IP_SETTINGS_FILE(String ip)
        {
            IP = ip;
        }
    }

    public partial class SettingsDialog : Form
    {
        public SettingsDialog()
        {
            InitializeComponent();

            if (File.Exists(@".\settings.xml"))
            {
                FileStream fs = File.Open(@".\settings.xml", FileMode.OpenOrCreate, FileAccess.Read);
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(IP_SETTINGS_FILE));
                IP_SETTINGS_FILE settings = (IP_SETTINGS_FILE)xmlSerializer.Deserialize(fs);
                fs.Close();

                txtIp.Text = settings.IP;
            }

        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            IPAddress ip;
            String strIp = txtIp.Text;

            if (IPAddress.TryParse(strIp, out ip))
            {
                IP_SETTINGS_FILE setting = new IP_SETTINGS_FILE(strIp);
                FileStream fs = File.Open(@".\settings.xml", FileMode.OpenOrCreate, FileAccess.Write);
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(IP_SETTINGS_FILE));
                xmlSerializer.Serialize(fs, setting);
                fs.Close();

                MessageBox.Show("Settings saved!");
            }
        }

        private string GetPublicIpFromIfConfig()
        {
            var request = (HttpWebRequest)WebRequest.Create("http://ifconfig.me");

            request.UserAgent = "curl";

            string publicIPAddress;

            request.Method = "GET";
            using (WebResponse response = request.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    publicIPAddress = reader.ReadToEnd();
                }
            }

            return publicIPAddress.Replace("\n", "");
        }

        private string GetPublicIPFromDynDns()
        {
            string url = "http://checkip.dyndns.org";
            WebRequest req = WebRequest.Create(url);
            string ip = "127.0.0.1";

            using (WebResponse resp = req.GetResponse())
            {
                using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                {
                    string response = sr.ReadToEnd().Trim();
                    string[] a = response.Split(':');
                    string a2 = a[1].Substring(1);
                    string[] a3 = a2.Split('<');
                    string a4 = a3[0];
                    ip = a4;
                }
            }

            return ip;
        }

        private void btnExternal_Click(object sender, EventArgs e)
        {
            String dynDns = GetPublicIPFromDynDns();
            String ifConfig = GetPublicIpFromIfConfig();

            IPAddress ip;

            bool sameIp = (dynDns == ifConfig);
            bool suc1 = IPAddress.TryParse(dynDns, out ip);
            bool suc2 = IPAddress.TryParse(ifConfig, out ip);

            if (sameIp && suc1 && suc2)
                txtIp.Text = dynDns;

            if (!sameIp)
            {
                if (suc1)
                    txtIp.Text = dynDns;

                if (suc2)
                    txtIp.Text = ifConfig;
            }
        }

        private void btnDefault_Click(object sender, EventArgs e)
        {
            txtIp.Text = "127.0.0.1";
        }

        public IPAddress GetNewIp()
        {
            IPAddress ip = IPAddress.Loopback;
            String strIp = txtIp.Text;
            bool succes = IPAddress.TryParse(strIp, out ip);

            return ip;
        }

        public String GetStrIp()
        {
            return txtIp.Text;
        }
    }
}
