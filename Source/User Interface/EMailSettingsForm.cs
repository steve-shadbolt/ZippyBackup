using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ZippyBackup.User_Interface
{
    public partial class EMailSettingsForm : Form
    {
        public EMailSettingsForm()
        {
            InitializeComponent();
        }

        public EMailSettings Settings
        {
            get
            {
                EMailSettings ret = new EMailSettings();
                ret.EMailFrom = tbEMailFrom.Text;
                ret.EMailTo = tbEMailTo.Text;
                ret.SMTPServer = tbSMTPServer.Text;
                try
                {
                    ret.SMTPPort = int.Parse(tbSMTPPort.Text);
                }
                catch (Exception) { throw new Exception("Invalid SMTP Port number."); }
                ret.Username = tbUsername.Text;
                ret.Password.Password = tbPassword.Text;                
                return ret;
            }

            set
            {
                tbEMailFrom.Text = value.EMailFrom;
                tbEMailTo.Text = value.EMailTo;
                tbSMTPServer.Text = value.SMTPServer;
                tbSMTPPort.Text = value.SMTPPort.ToString();
                tbUsername.Text = value.Username;
                tbPassword.Text = value.Password.Password;
            }
        }

        private void EMailSettingsForm_Load(object sender, EventArgs e)
        {

        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            EMailSettings Test;
            try
            {
                Test = Settings;
            }
            catch (Exception exc) { MessageBox.Show(exc.Message); return; }

            switch (MessageBox.Show("Would you like to send a test e-mail?", "Verify", MessageBoxButtons.YesNoCancel))
            {
                case DialogResult.No: break;
                case DialogResult.Cancel: return;
                case DialogResult.Yes:
                    {
                        bool Done = false;
                        while (!Done)
                        {
                            try
                            {
                                System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
                                message.To.Add(Test.EMailTo);
                                message.Subject = "ZippyBackup Test";
                                message.From = new System.Net.Mail.MailAddress(Test.EMailFrom);
                                message.Body = "This is a test e-mail sent from the ZippyBackup application.";
                                System.Net.Mail.SmtpClient smtp
                                    = new System.Net.Mail.SmtpClient(Test.SMTPServer, Test.SMTPPort);
                                smtp.Credentials = new System.Net.NetworkCredential(Test.Username, Test.Password.Password);
                                smtp.Send(message);

                                MessageBox.Show("E-mail sent successfully!  Please verify that it was received.");
                                Done = true;
                            }
                            catch (Exception exc)
                            {
                                switch (MessageBox.Show("The following error occurred while sending test e-mail:  " + exc.Message, "Error", MessageBoxButtons.AbortRetryIgnore))
                                {
                                    case DialogResult.Abort: return;
                                    case DialogResult.Ignore: Done = true; break;
                                    case DialogResult.Retry: continue;
                                }
                            }
                        }
                        break;
                    }
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

    }
}
