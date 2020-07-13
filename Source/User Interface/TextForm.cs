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
    public partial class TextForm : Form
    {
        public new string Text
        {
            get { return tb.Text; }
            set { tb.Text = value; }
        }

        public string Caption
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        public TextForm()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            return;
        }
    }
}
