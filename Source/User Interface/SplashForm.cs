/** ZippyBackup
 *  Copyright (C) 2012-2013 by Wiley Black.  All rights reserved.
 *  See License.txt for licensing rules.
 */

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
    public partial class SplashForm : Form
    {
        public int FirstShown;
        
        public SplashForm()
        {
            InitializeComponent();
        }

        private void SplashForm_Load(object sender, EventArgs e)
        {
            Visible = false;
            BackColor = Color.FromArgb(0, 1, 0);
            TransparencyKey = Color.FromArgb(0, 1, 0);
            Width = BackgroundImage.Width;
            Height = BackgroundImage.Height;

            labelVersion.Text = "Version: " + BuildVersion.Version.ToShortDateString();
            
            Screen CurrentScreen = Screen.FromControl(this);
            Location = new Point(CurrentScreen.Bounds.Width / 2 - Width / 2, CurrentScreen.Bounds.Height / 2 - Height / 2);

            Status = "Loading...";
            Visible = true;

            FirstShown = Environment.TickCount;
        }

        public string Status
        {
            get { return labelStatus.Text; }
            set
            {
                Graphics g = CreateGraphics();
                SizeF sz = g.MeasureString(value, labelStatus.Font);
                labelStatus.Left = (int)(Width / 2.0f - sz.Width / 2.0f);
                labelStatus.Text = value;
                Application.DoEvents();
            }
        }
    }
}
