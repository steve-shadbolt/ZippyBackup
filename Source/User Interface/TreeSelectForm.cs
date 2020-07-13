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
    public partial class TreeSelectForm : Form
    {        
        public TreeSelectForm()
        {
            InitializeComponent();
        }

        private void TreeSelectForm_Load(object sender, EventArgs e)
        {

        }
        
        private void Tree_AfterCheck(object sender, TreeViewEventArgs e)
        {            
            foreach (TreeNode sub in e.Node.Nodes)
            {
                sub.Checked = e.Node.Checked;                
            }            
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
