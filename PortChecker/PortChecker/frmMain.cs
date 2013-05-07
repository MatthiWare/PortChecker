using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.IO;

namespace MatthiWare.PortChecker
{
    public partial class frmMain : Form
    {
        SettingsDialog sd = new SettingsDialog();
        CheckPortDialog cpd = new CheckPortDialog();
        PortChecker portChecker;
        delegate void PortAdderDelegate(Object o);

        public frmMain()
        {
            InitializeComponent();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            UInt16 minPortRange, maxPortRange = 1024;

            bool parse1 = !UInt16.TryParse(txtPortRange1.Text, out minPortRange);
            bool parse2 = !UInt16.TryParse(txtPortRange2.Text, out maxPortRange);

            if (parse1 || parse2)
            {
                MessageBox.Show(String.Format("Unable to parse port numbers\nThe value should be between {0}-{1}", UInt16.MinValue, UInt16.MaxValue), "Parsing error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            else if (minPortRange > maxPortRange)
            {
                MessageBox.Show("The minimum port range can't be higher then the maximum port range", "Max port less then min port error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            lbPortsClosed.Items.Clear();
            lbPortsOpen.Items.Clear();

            portChecker = new PortChecker(minPortRange, maxPortRange, sd.GetNewIp());

            portChecker.PortCheckCompleted += new EventHandler<PortEventArgs>(portChecker_PortCheckCompleted);
            portChecker.StartChecking();
        }

        void portChecker_PortCheckCompleted(object sender, PortEventArgs e)
        {
            if (e.Open)
                AddOpenPort(String.Format("Port: {0}: {1}", e.Port, e.Message));
            else
                AddClosedPort(String.Format("Port: {0}: {1}", e.Port, e.Message));
        }

        void AddOpenPort(Object obj)
        {
            if (lbPortsOpen.InvokeRequired)
                lbPortsOpen.Invoke(new PortAdderDelegate(AddOpenPort), obj);
            else
            {
                lbPortsOpen.Items.Add(obj);

                lbPortsOpen.SelectedIndex = lbPortsOpen.Items.Count - 1;
            }
        }

        void AddClosedPort(Object obj)
        {
            if (lbPortsClosed.InvokeRequired)
                lbPortsClosed.Invoke(new PortAdderDelegate(AddClosedPort), obj);
            else
            {
                lbPortsClosed.Items.Add(obj);

                lbPortsClosed.SelectedIndex = lbPortsClosed.Items.Count - 1;
            }
        }

        private void checkPortToolStripMenuItem_Click(object sender, EventArgs e)
        {
            cpd.ShowDialog();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Save report!";
            sfd.SupportMultiDottedExtensions = true;
            sfd.ValidateNames = true;
            sfd.OverwritePrompt = true;
            sfd.InitialDirectory = Application.StartupPath;
            sfd.Filter = "Xml Files (*.xml)|*.xml";

            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SaveAsXml(sfd.FileName);
            }

            if (sfd != null)
                sfd.Dispose(); sfd = null;
        }

        private void SaveAsXml(String path)
        {
            SAVE_REPORT sr = new SAVE_REPORT(lbPortsOpen.Items, lbPortsClosed.Items);
            FileStream fs = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write);
            XmlSerializer xml = new XmlSerializer(typeof(SAVE_REPORT));
            xml.Serialize(fs, sr);
            fs.Close();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseHandler();
        }

        private void CloseHandler()
        {
            if (portChecker != null)
            {
                bool wasRunning = portChecker._running;

                portChecker.End();

                if (wasRunning)
                {
                    MessageBox.Show("All pending port checks have been canceled\nThe process might still be running till all of the tasks are done", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 ab = new AboutBox1();
            ab.ShowDialog();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sd.ShowDialog();
            cpd.SetIp(sd.GetStrIp());
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            cpd.SetIp(sd.GetStrIp());
        }
    }

    [Serializable]
    public struct SAVE_REPORT
    {
        public ListBox.ObjectCollection Open;
        public ListBox.ObjectCollection Closed;

        public SAVE_REPORT(ListBox.ObjectCollection open, ListBox.ObjectCollection closed)
        {
            Open = open;
            Closed = closed;
        }
    }
}
