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
    public partial class OverwritePromptForm : Form
    {
        public OverwritePromptForm()
        {
            InitializeComponent();
        }

        public static Result Show(string Prompt)
        {
            OverwritePromptForm opf = new OverwritePromptForm();
            opf.labelMsg.Text = Prompt;
            if (opf.ShowDialog() != DialogResult.OK) return Result.No;
            return opf.Selected;
        }

        public enum Result
        {
            No,
            Yes,
            NoToAll,
            YesToAll,
            Cancel
        }

        public Result Selected = Result.No;

        private void btnYes_Click(object sender, EventArgs e)
        {
            Selected = Result.Yes;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnNo_Click(object sender, EventArgs e)
        {
            Selected = Result.No;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnYesAll_Click(object sender, EventArgs e)
        {
            Selected = Result.YesToAll;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnNoAll_Click(object sender, EventArgs e)
        {
            Selected = Result.NoToAll;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Selected = Result.Cancel;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
