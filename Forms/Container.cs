﻿using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using _4RTools.Model;
namespace _4RTools.Forms
{
    public partial class Container : Form
    {

        private Utils.Subject subject = new Utils.Subject();
        
        public Container()
        {
            if (!Directory.Exists(Utils.Config.ReadSetting("ProfileFolder")))
            {
                Directory.CreateDirectory(Utils.Config.ReadSetting("ProfileFolder")); //Create Profile Folder if don't exists.
            }
            InitializeComponent();
            Text = Utils.Config.ReadSetting("Name") + " - " + Utils.Config.ReadSetting("Version");
            Utils.KeyboardHook.Enable();
            Utils.KeyboardHook.Add(Keys.End, new Utils.KeyboardHook.KeyPressed(this.onPressEnd)); //Toggle System (ON-OFF)

            //Container Configuration
            this.IsMdiContainer = true;
            SetBackGroundColorOfMDIForm();

            //Paint Children Forms Below
            SetAutopotWindow();
            SetAHKWindow();
            SetProfileWindow();
        }


        public void SetAutopotWindow()
        {
            AutopotForm frm = new AutopotForm(subject);
            frm.FormBorderStyle = FormBorderStyle.None;
            frm.Location = new Point(0,65);
            frm.MdiParent = this;
            frm.Show();
        }

        public void SetAHKWindow()
        {
            AHKForm frm = new AHKForm(subject);
            frm.FormBorderStyle = FormBorderStyle.None;
            frm.Location = new Point(0, 65);
            frm.MdiParent = this;
            frm.Show();
            addform(this.tabPage2, frm) ;
        }

        public void SetProfileWindow()
        {
            ProfileForm frm = new ProfileForm(this);
            frm.FormBorderStyle = FormBorderStyle.None;
            frm.Location = new Point(0, 65);
            frm.MdiParent = this;
            frm.Show();
            addform(this.tabPage4, frm);
        }

        public void addform(TabPage tp, Form f)
        {

            if (!tp.Controls.Contains(f))
            {
                tp.Controls.Add(f);
                f.Dock = DockStyle.Fill;
                f.Show();
                Refresh();
            }
            Refresh();
        }

        private void SetBackGroundColorOfMDIForm()
        {
            foreach (Control ctl in this.Controls)
            {
                if ((ctl) is MdiClient)
                {
                    ctl.BackColor = Color.White;
                }
            }
        }

        private void processCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            Client client = new Client(this.processCB.SelectedItem.ToString());
            ClientSingleton.Instance(client);
            subject.Notify(new Utils.Message(Utils.MessageCode.PROCESS_CHANGED, null));
            
        }

        private void Container_Load(object sender, EventArgs e)
        {
            if (!Profile.ProfileExists("Default")) {
                new Profile("Default").Save();
            }

            this.refreshProcessList();
            this.refreshProfileList();
            this.profileCB.SelectedIndex = 0;
        }

        public void refreshProfileList()
        {
            this.Invoke((MethodInvoker)delegate ()
            {
                this.profileCB.Items.Clear();
            });
            foreach (string p in Profile.ListAll())
            {
                this.profileCB.Items.Add(p);
            }
        }

        private void refreshProcessList()
        {
            this.Invoke((MethodInvoker)delegate ()
            {
                this.processCB.Items.Clear();
            });
            foreach (Process p in Process.GetProcesses())
            {
                if (p.MainWindowTitle != "")
                {
                    this.processCB.Items.Add(string.Format("{0}.exe - {1}", p.ProcessName, p.Id));
                }
            }
        }

        private void chckToggle_CheckedChanged(object sender, EventArgs e)
        {
            subject.Notify(new Utils.Message(this.chckToggle.Checked ? Utils.MessageCode.TURN_ON : Utils.MessageCode.TURN_OFF, null));
        }

        private bool onPressEnd()
        {
            this.chckToggle.Checked = !this.chckToggle.Checked;
            return true;
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            this.refreshProcessList();
        }

        protected override void OnClosed(EventArgs e)
        {
            Utils.KeyboardHook.Disable();
            subject.Notify(new Utils.Message(Utils.MessageCode.TURN_OFF, null));
            base.OnClosed(e);
        }

        private void lblLinkGithub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(Utils.Config.ReadSetting("GithubLink"));
        }

        private void lblLinkDiscord_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(Utils.Config.ReadSetting("DiscordLink"));
        }

        private void profileCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            ProfileSingleton.Load(this.profileCB.Text); //LOAD PROFILE
            subject.Notify(new Utils.Message(Utils.MessageCode.PROFILE_CHANGED, null));
        }
    }
}