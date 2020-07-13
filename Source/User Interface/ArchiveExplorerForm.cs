using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;

namespace ZippyBackup.User_Interface
{
    public partial class ArchiveExplorerForm : Form
    {
        BackupProject Project;
        ArchiveFilename Archive;
        Manifest Manifest;

        public ArchiveExplorerForm(BackupProject Project, ArchiveFilename Archive)
        {
            this.Project = Project;
            this.Archive = Archive;
            InitializeComponent();
        }

        Dictionary<string, TreeNode> TreeByPath;

        static ImageList IconList;
        static Bitmap GreenStar;
        static Bitmap WhiteBox;

        private void ArchiveExplorerForm_Load(object sender, EventArgs e)
        {
            if (IconList == null)
            {
                IconList = new ImageList();
                IconList.ImageSize = new Size(16, 16);
                IconList.Images.Add(new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("ZippyBackup.Resources.FolderIcon.png")));
                IconList.Images.Add(new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("ZippyBackup.Resources.FolderIconWithStar.png")));
            }
            if (GreenStar == null)
                GreenStar = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("ZippyBackup.Resources.GreenStar.png"));
            if (WhiteBox == null)
                WhiteBox = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("ZippyBackup.Resources.WhiteBox.png"));

            ArchiveTree.ImageList = IconList;

            using (Impersonator newself = new Impersonator(Project.BackupCredentials))
                Manifest = Archive.LoadArchiveManifest(Project, true);

            TreeNode Root = new TreeNode(Archive.ToString());
            Root.Tag = Manifest.ArchiveRoot;
            ArchiveTree.Nodes.Add(Root);

            TreeByPath = new Dictionary<string, TreeNode>();
            PopulateNode(Root);
            Root.Expand();
        }

        /// <summary>
        /// PopulateNode() will generate the children to attach to a given
        /// tree node using the manifest.  PopulateNode() checks for updated
        /// files and sets the node's image index to 1 when there are updated
        /// files within the folder.
        /// </summary>
        /// <param name="Node"></param>
        /// <returns>True if any files were updated (incremental backup) within the folder.</returns>
        bool PopulateNode(TreeNode Node)
        {
            bool Changed = false;
            Manifest.Folder Folder = Node.Tag as Manifest.Folder;
            foreach (Manifest.Folder Subfolder in Folder.Folders)
            {
                TreeNode SubfolderNode = new TreeNode(Subfolder.Name);
                SubfolderNode.Tag = Subfolder;
                Node.Nodes.Add(SubfolderNode);
                if (PopulateNode(SubfolderNode) || IsUpdated(Subfolder.Files))
                {
                    SubfolderNode.ImageIndex = SubfolderNode.SelectedImageIndex = 1;
                    Node.ImageIndex = Node.SelectedImageIndex = 1;
                    Changed = true;
                }
                else
                    Node.ImageIndex = Node.SelectedImageIndex = 0;
                TreeByPath.Add(Subfolder.RelativePath, SubfolderNode);
            }            
            return Changed;
        }

        bool IsUpdated(List<Manifest.File> Files)
        {
            string ArchiveName = Archive.ToString();
            foreach (Manifest.File File in Files)
            {
                if (File.ArchiveFile.ToLower() == ArchiveName.ToLower()) return true;
            }
            return false;
        }

        private void ArchiveTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            CurrentView.Clear();
            ImageList Images = new ImageList();
            Images.ImageSize = new Size(32, 32);
            Images.ColorDepth = ColorDepth.Depth32Bit;

            Manifest.Folder Folder = e.Node.Tag as Manifest.Folder;
            
            Images.Images.Add(Properties.Resources.Folder);             // Image Index 0 is for folders
            foreach (Manifest.Folder Subfolder in Folder.Folders)
            {
                ListViewItem NewItem = new ListViewItem(Subfolder.Name, 0);
                NewItem.Tag = Subfolder;
                CurrentView.Items.Add(NewItem);
            }

            //Images.Images.Add(WhiteBox);            // Image index 1 is for missing icons.
            //const int iMissingIcon = 1;
            Images.Images.Add(GreenStar);           // Image index 2 is for missing icons with a green star.
            const int iMissingIconWithStar = 1;

            string ArchiveName = Archive.ToString().ToLower();
            Dictionary<string, int> ExtensionToImageIndex = new Dictionary<string, int>();
            Dictionary<string, int> ExtensionToImageIndexWithStar = new Dictionary<string, int>();            
            foreach (Manifest.File File in Folder.Files)
            {
                bool NeedStar = (File.ArchiveFile.ToLower() == ArchiveName);
                int iImage = -1;
                if (NeedStar)
                {
                    if (!ExtensionToImageIndexWithStar.TryGetValue(File.Extension.ToLower(), out iImage)) iImage = -1;
                }
                else
                {
                    if (!ExtensionToImageIndex.TryGetValue(File.Extension.ToLower(), out iImage)) iImage = -1;
                }
                
                if (iImage < 0)
                {
                    Icon FileIcon = Utility.GetFileIcon(File.Extension, true);
                    if (FileIcon == null)
                    {
                        if (NeedStar) iImage = iMissingIconWithStar;
                        else {
                            ListViewItem NewItemNI = new ListViewItem(File.Name);
                            NewItemNI.Tag = File;
                            CurrentView.Items.Add(NewItemNI); 
                            continue; 
                        }
                    }
                    else
                    {
                        Bitmap FileImage;
                        if (NeedStar) FileImage = OverlayOnIcon(FileIcon, GreenStar);
                        else FileImage = FileIcon.ToBitmap();

                        iImage = Images.Images.Count;
                        Images.Images.Add(FileImage);
                        if (NeedStar)
                            ExtensionToImageIndexWithStar.Add(File.Extension.ToLower(), iImage);
                        else
                            ExtensionToImageIndex.Add(File.Extension.ToLower(), iImage);
                    }
                }

                ListViewItem NewItem = new ListViewItem(File.Name, iImage);
                NewItem.Tag = File;
                CurrentView.Items.Add(NewItem);
            }

            CurrentView.View = View.LargeIcon;
            CurrentView.LargeImageList = Images;

            btnExtract.Enabled = (ArchiveTree.SelectedNode != null) || (CurrentView.SelectedItems.Count > 0);
        }

        Bitmap OverlayOnIcon(Icon Source, Bitmap Overlay)
        {
            Bitmap ret = new Bitmap(32, 32, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(ret))
            {
                g.DrawIconUnstretched(Source, new Rectangle(0, 0, ret.Width, ret.Height));
                g.DrawImage(Overlay, new Rectangle(0, 0, ret.Width, ret.Height));
            }
            return ret;
        }

        private void CurrentView_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnExtract.Enabled = (ArchiveTree.SelectedNode != null) || (CurrentView.SelectedItems.Count > 0);
        }

        private void CurrentView_DoubleClick(object sender, EventArgs e)
        {
            if (CurrentView.SelectedItems.Count == 1)
            {
                ListViewItem ClickedItem = CurrentView.SelectedItems[0];

                if (ClickedItem != null && ClickedItem.Tag is Manifest.Folder)
                {
                    Manifest.Folder Folder = (Manifest.Folder)ClickedItem.Tag;
                    TreeNode MatchingNode = TreeByPath[Folder.RelativePath];
                    ArchiveTree.SelectedNode = MatchingNode;
                }
                else if (ClickedItem != null && ClickedItem.Tag is Manifest.File)
                {
                    Manifest.File File = (Manifest.File)ClickedItem.Tag;                    
                    if (ExtractionInProgress) return;
                    ExtractionInProgress = true;
                    try
                    {
                        string TempFolder = Utility.StripTrailingSlash(Path.GetTempPath());
                        string TempPath = TempFolder + "\\" + File.Name;

                        List<Manifest.Entry> Entry = new List<Manifest.Entry>();
                        Entry.Add(File);
                        ExtractionRun Extraction = new ExtractionRun(Project);
                        Extraction.Run(Entry, TempFolder);
                        System.IO.File.SetAttributes(TempPath, System.IO.File.GetAttributes(TempPath) | FileAttributes.Temporary);

                        System.Diagnostics.Process.Start(TempPath);
                    }
                    catch (CancelException) { }
                    finally
                    {
                        ExtractionInProgress = false;
                    }
                }
            }
        }

        private void CurrentView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;

            ContextMenu Menu = new ContextMenu();
            MenuItem Cmd;
            Cmd = new MenuItem("&Open");
            Cmd.Click += new EventHandler(CurrentView_DoubleClick);
            Cmd = new MenuItem("&Extract To...");
            Cmd.Click += new EventHandler(btnExtract_Click);
            Menu.MenuItems.Add(Cmd);
            Menu.Show(CurrentView, e.Location);
        }

        bool ExtractionInProgress = false;
        private void btnExtract_Click(object sender, EventArgs e)
        {
            if (ExtractionInProgress) return;
            ExtractionInProgress = true;
            try
            {                
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.Description = "Select location to extract to...";
                if (fbd.ShowDialog() != DialogResult.OK) return;

                List<Manifest.Entry> Entries = new List<Manifest.Entry>();

                ExtractionRun Extraction = new ExtractionRun(Project);

                if (CurrentView.SelectedItems.Count > 0)
                {
                    foreach (ListViewItem Item in CurrentView.SelectedItems)
                        Entries.Add((Manifest.Entry)Item.Tag);
                    Extraction.Run(Entries, Utility.StripTrailingSlash(fbd.SelectedPath));
                }
                else if (ArchiveTree.SelectedNode != null)
                {
                    Manifest.Folder Top = ArchiveTree.SelectedNode.Tag as Manifest.Folder;
                    Entries.Add(Top);
                    Extraction.Run(Entries, Utility.StripTrailingSlash(fbd.SelectedPath));
                }
            }
            catch (CancelException) { }
            catch (Exception ex)
            {
#               if DEBUG
                MessageBox.Show(ex.ToString());
#               else
                MessageBox.Show(ex.Message);
#               endif
            }
            finally
            {
                ExtractionInProgress = false;
            }
        }        
    }
}
