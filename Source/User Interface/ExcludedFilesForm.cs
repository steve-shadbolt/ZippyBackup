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
    public partial class ExcludedFilesForm : Form
    {   
        public string BasePath;     

        public List<string> Files
        {
            get
            {
                List<string> ret = new List<string>();
                foreach (string Entry in listFiles.Items) ret.Add(Entry);
                return ret;
            }

            set
            {
                listFiles.Items.Clear();
                foreach (string Entry in value)
                {
                    listFiles.Items.Add(Entry);
                }
            }
        }

        string OriginalPrompt;
        public string Prompt
        {
            get { return labelPrompt.Text; }
            set { OriginalPrompt = value; labelPrompt.Text = value; SetSize(); }
        }

        public ExcludedFilesForm()
        {
            InitializeComponent();
            labelPrompt.Text = Text;            
        }        

        void SetSize()
        {
            listFiles.Top = labelPrompt.Bottom + 25;
            btnRemove.Top = listFiles.Top;
            textNewEntry.Top = listFiles.Bottom + 15;
            btnBrowse.Top = textNewEntry.Top;
            btnAdd.Top = textNewEntry.Top;
            btnOK.Top = btnAdd.Bottom + 25;
        }
        
        private void btnAdd_Click(object sender, EventArgs e)
        {
            string RelativePath;
            try
            {
                string NewFile = textNewEntry.Text.Trim();

                if (NewFile.Length < 1)
                {
                    MessageBox.Show("Please type a filename in the box to the left or click Browse before clicking Add.");
                    return;
                }

                if (!Alphaleonis.Win32.Filesystem.File.Exists(NewFile))
                {
                    if (MessageBox.Show("The file does not presently exist.  Do you want to add it to the exclusion list anyway?", "Confirm", MessageBoxButtons.YesNo)
                        != System.Windows.Forms.DialogResult.Yes) return;
                }

                RelativePath = Utility.GetRelativePath(BasePath, NewFile);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
                return;
            }

            textNewEntry.Text = "";
            textNewEntry.Focus();            

            foreach (string Existing in listFiles.Items)
            {                
                if (RelativePath.Equals(Existing, StringComparison.OrdinalIgnoreCase))
                {
                    labelPrompt.Text = Existing + " is already on the list.";
                    labelPrompt.ForeColor = System.Drawing.Color.Red;
                    return;
                }
            }            

            listFiles.Items.Add(RelativePath);
            labelPrompt.Text = OriginalPrompt;
            labelPrompt.ForeColor = System.Drawing.Color.Black;
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (listFiles.SelectedItem == null) return;
            listFiles.Items.Remove(listFiles.SelectedItem);
        }

        private void listExtensions_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnRemove.Enabled = (listFiles.SelectedItem != null);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = BasePath;
            ofd.ValidateNames = false;
            ofd.CheckFileExists = false;
            ofd.CheckPathExists = false;
            if (ofd.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            textNewEntry.Text = ofd.FileName;
        }
    }
}
