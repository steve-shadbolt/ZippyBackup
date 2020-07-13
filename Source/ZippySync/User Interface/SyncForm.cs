using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ZippySync
{
    public partial class SyncForm : Form
    {
        public SyncForm()
        {
            InitializeComponent();

            TrayMenu = new ContextMenu();
            TrayMenu.MenuItems.Add("&Close ZippySync", new EventHandler(OnCloseZippySync));

            TrayIcon = new NotifyIcon();
            TrayIcon.Text = "ZippySync";
            TrayIcon.Icon = Icon;
            TrayIcon.Visible = true;
            TrayIcon.ContextMenu = TrayMenu;
            TrayIcon.DoubleClick += new EventHandler(OnTrayDoubleClick);
            TrayIcon.BalloonTipClicked += new EventHandler(OnTrayBalloonClick);
        }

        NotifyIcon TrayIcon;
        ContextMenu TrayMenu;

        private void SyncForm_Load(object sender, EventArgs e)
        {

        }
    }
}
