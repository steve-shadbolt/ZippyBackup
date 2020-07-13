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
    public partial class ProgressForm : Form
    {
        #region "Thread-Safe Pending Changes Mechanism"

        object PendingLock = new object();

        private string m_Pending_Label1_Text = null;
        public string Pending_Label1_Text
        {
            set { lock (PendingLock) m_Pending_Label1_Text = value; }
        }

        private string m_Pending_Label2_Text = null;
        public string Pending_Label2_Text
        {
            set { lock (PendingLock) m_Pending_Label2_Text = value; }
        }

        private string m_Pending_Label3_Text = null;
        public string Pending_Label3_Text
        {
            set { lock (PendingLock) m_Pending_Label3_Text = value; }
        }

        private int m_Pending_OverallProgressBar_Minimum;
        private bool m_bPending_OverallProgressBar_Minimum;
        public int Pending_OverallProgressBar_Minimum { set { lock (PendingLock) { m_Pending_OverallProgressBar_Minimum = value; m_bPending_OverallProgressBar_Minimum = true; } } }

        private int m_Pending_OverallProgressBar_Maximum;
        private bool m_bPending_OverallProgressBar_Maximum;
        public int Pending_OverallProgressBar_Maximum { set { lock (PendingLock) { m_Pending_OverallProgressBar_Maximum = value; m_bPending_OverallProgressBar_Minimum = true; } } }

        private int m_Pending_OverallProgressBar_Value;
        private bool m_bPending_OverallProgressBar_Value;
        public int Pending_OverallProgressBar_Value { set { lock (PendingLock) { m_Pending_OverallProgressBar_Value = value; m_bPending_OverallProgressBar_Minimum = true; } } }

        private int m_Pending_CurrentProgressBar_Minimum;
        private bool m_bPending_CurrentProgressBar_Minimum;
        public int Pending_CurrentProgressBar_Minimum { set { lock (PendingLock) { m_Pending_CurrentProgressBar_Minimum = value; m_bPending_OverallProgressBar_Minimum = true; } } }

        private int m_Pending_CurrentProgressBar_Maximum;
        private bool m_bPending_CurrentProgressBar_Maximum;
        public int Pending_CurrentProgressBar_Maximum { set { lock (PendingLock) { m_Pending_CurrentProgressBar_Maximum = value; m_bPending_OverallProgressBar_Minimum = true; } } }

        private int m_Pending_CurrentProgressBar_Value;
        private bool m_bPending_CurrentProgressBar_Value;
        public int Pending_CurrentProgressBar_Value { set { lock (PendingLock) { m_Pending_CurrentProgressBar_Value = value; m_bPending_OverallProgressBar_Minimum = true; } } }

        public void ApplyPending()
        {
            lock (PendingLock)
            {
                if (m_Pending_Label1_Text != null) label1.Text = m_Pending_Label1_Text; m_Pending_Label1_Text = null;
                if (m_Pending_Label2_Text != null) label2.Text = m_Pending_Label2_Text; m_Pending_Label2_Text = null;
                if (m_Pending_Label3_Text != null) label3.Text = m_Pending_Label3_Text; m_Pending_Label3_Text = null;
                if (m_bPending_OverallProgressBar_Minimum) OverallProgressBar.Minimum = m_Pending_OverallProgressBar_Minimum; m_bPending_OverallProgressBar_Minimum = false;
                if (m_bPending_OverallProgressBar_Maximum) OverallProgressBar.Maximum = m_Pending_OverallProgressBar_Maximum; m_bPending_OverallProgressBar_Maximum = false;
                if (m_bPending_OverallProgressBar_Value) OverallProgressBar.Value = m_Pending_OverallProgressBar_Value; m_bPending_OverallProgressBar_Value = false;
                if (m_bPending_CurrentProgressBar_Minimum) CurrentProgressBar.Minimum = m_Pending_CurrentProgressBar_Minimum; m_bPending_CurrentProgressBar_Minimum = false;
                if (m_bPending_CurrentProgressBar_Maximum) CurrentProgressBar.Maximum = m_Pending_CurrentProgressBar_Maximum; m_bPending_CurrentProgressBar_Maximum = false;
                if (m_bPending_CurrentProgressBar_Value) CurrentProgressBar.Value = m_Pending_CurrentProgressBar_Value; m_bPending_CurrentProgressBar_Value = false;
            }
        }

        #endregion

        public ProgressForm()
        {
            InitializeComponent();
            label1.Text = "";
            label2.Text = "";
            label3.Text = "";            
        }

        public bool Cancel = false;

        public string CancelPrompt = "Are you sure you wish to cancel this backup run?";

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(CancelPrompt, "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;
            Cancel = true;
        }

        private void ProgressForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Call Progress.Dispose() instead of Progress.Close() to avoid triggering this confirmation programmatically.

            if (e.CloseReason == CloseReason.UserClosing)
            {
                if (MessageBox.Show(CancelPrompt, "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
                Cancel = true;
            }
        }

        public int ClientHeight
        {
            get { return ClientRectangle.Height; }
            set { Height = value + (Height - ClientRectangle.Height); }
        }

        public int ClientWidth
        {
            get { return ClientRectangle.Width; }
            set { Width = value + (Width - ClientRectangle.Width); }
        }

        private void ProgressForm_Resize(object sender, EventArgs e)
        {
            btnCancel.Left = ClientWidth - OverallProgressBar.Left - btnCancel.Width;
            OverallProgressBar.Width = btnCancel.Left - OverallProgressBar.Left - 20;
            CurrentProgressBar.Width = ClientWidth - 2 * CurrentProgressBar.Left;
        }

        private void ProgressForm_Load(object sender, EventArgs e)
        {
            ProgressForm_Resize(null, null);
        }
    }
}
