using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;

namespace MatthiWare.PortChecker
{
    public partial class CheckPortDialog : Form
    {
        public CheckPortDialog()
        {
            InitializeComponent();
        }

        public void SetIp(String ip)
        {
            txtIp.Text = ip;
        }

        private void btnCheck_Click(object sender, EventArgs e)
        {
            UInt16 port;
            IPAddress ip;

            bool suc1 = UInt16.TryParse(txtPort.Text, out port);
            bool suc2 = IPAddress.TryParse(txtIp.Text, out ip);

            if (!suc1 && !suc2)
            {
                MessageBox.Show("Invalid ip and port", "Invalid cast error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else if (!suc1)
            {
                MessageBox.Show("Invalid port", "Invalid cast error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else if (!suc2)
            {
                MessageBox.Show("Invalid ip", "Invalid cast error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            CheckPortTask task = new CheckPortTask(new EventHandler<PortEventArgs>((Object o , PortEventArgs ee) => 
            {
                if (ee.Open)
                    MessageBox.Show(String.Format("The port {0} from IP {1} is open.", ee.Port.ToString(), txtIp.Text), "Port open!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                else
                    MessageBox.Show(String.Format("The port {0} from IP {1} is closed.\nMessage: {2}", ee.Port.ToString(), txtIp.Text, ee.Message), "Port closed!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }), ip);

            task.BeginCheckPort(port);

        }
    }
}
