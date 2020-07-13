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
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Win32;
using Ionic.Zip;
using Alphaleonis.Win32.Filesystem;         // Replaces System.IO entities such as FileInfo with more advanced versions.
using ZippyBackup.IO;

namespace ZippyBackup
{
    public class Manifest
    {
        [XmlAttribute]
        public string BackupProjectName;    // Informational use - identify which backup project this manifest corresponds to.

        [XmlAttribute]
        public string SourcePath;           // Informational use - store the location that this backup originally corresponded to.

        [XmlAttribute]
        public string BackupSoftware = "ZippyBackup";     // Information use - identify how the archive was created.

        [XmlAttribute]
        public DateTime BackupStartTime;    // Includes the time.  Date should match the value in the filename.

        [XmlAttribute]
        public int BackupSequence;          // Should match the value in the filename.

        [XmlAttribute]
        public bool IncrementalBackup;      // True if this is an incremental backup.  Full if complete.            

        public MachineId BackupMachineId;   // Identification of the computer generating the backup.

        public Folder ArchiveRoot;

        public class Entry
        {
            /// RelativePath gives the path relative to the source folder (root of the backup).
            /// The RelativePath refers to the original, relative path and not the path in the
            /// archive.
            [XmlAttribute]            
            public string RelativePath;

            [XmlIgnore]
            public string Name
            {
                get { return Path.GetFileName(RelativePath); }
            }

            [XmlIgnore]
            public string Extension
            {
                get { return Path.GetExtension(RelativePath); }
            }

            [XmlAttribute]
            public Flags Attributes = new Flags();

            [XmlIgnore]
            public System.IO.FileAttributes WindowsAttributes
            {
                get
                {
                    System.IO.FileAttributes ret = (System.IO.FileAttributes)0;
                    bool Normal = true;
                    foreach (System.IO.FileAttributes attr in Enum.GetValues(typeof(System.IO.FileAttributes)))
                    {
                        if (Attributes.Contains(attr.ToString())) { ret |= attr; Normal = false; }
                    }
                    if (Normal) return System.IO.FileAttributes.Normal;
                    return ret;
                }

                set
                {
                    Attributes.Clear();
                    foreach (System.IO.FileAttributes attr in Enum.GetValues(typeof(System.IO.FileAttributes)))
                    {
                        if (attr == System.IO.FileAttributes.Normal) continue;
                        if ((value & attr) != 0) Attributes.Add(attr.ToString());
                    }
                }
            }

            [XmlAttribute]
            public DateTime CreationTimeUtc;

            [XmlAttribute]
            public DateTime LastWriteTimeUtc;

            [XmlAttribute]
            public DateTime LastAccessTimeUtc;

            public Entry() { }

            /// <summary>
            /// Creates a new Entry.
            /// </summary>
            /// <param name="RelativePath">
            /// RelativePath gives the path relative to the source folder (root of the backup).
            /// The RelativePath refers to the original, relative path and not the path in the
            /// archive.
            /// </param>
            /// <param name="fsi">Information describing the file system entry.</param>
            public Entry(string RelativePath, FileSystemInfo fsi)
            {
                this.RelativePath = RelativePath;
                if (RelativePath != "")
                {
                    this.WindowsAttributes = fsi.Attributes;
                    this.CreationTimeUtc = fsi.CreationTimeUtc;
                    this.LastWriteTimeUtc = fsi.LastWriteTimeUtc;
                    this.LastAccessTimeUtc = fsi.LastAccessTimeUtc;
                }
                else
                {
                    // The root folder may not have accessible file attributes if it's a snapshot...
                    this.WindowsAttributes = System.IO.FileAttributes.Directory;
                    this.CreationTimeUtc = DateTime.UtcNow;
                    this.LastWriteTimeUtc = DateTime.UtcNow;
                    this.LastAccessTimeUtc = DateTime.UtcNow;
                }
            }
        }

        public class File : Entry
        {
            [XmlAttribute]
            public UInt64 Length;

            /** CRC32 is not calculated for all files.  As an expensive operation, it is only
             *  calculated on-demand.  It is used for comparison of two files that appear to
             *  be identical, as a confirmation.  Thus it is only calculated when such a
             *  comparison is made.  Once calculated, it is stored here to avoid recalculation.
             *  
             * The value is a 32-bit CRC using 0xEDB88320 for the polynomial, the same as the
             * CRC calculated by Zip.  This CRC-32 value is stored in the zip directory
             * of archives, and an improvement would be to retrieve the stored value whenever
             * performing a comparison to a previously archived file.  This current 
             * implementation only compares files on the file-system.
             * 
             * If the ValidCRC32 value is true then CRC32 is valid, otherwise it has not yet
             * been calculated for this file.
             */
            [XmlAttribute]
            public bool ValidCRC32 = false;

            [XmlAttribute]
            public UInt32 CRC32;

            /// <summary>
            /// DuplicateFile is a flag that indicates that the contents of this file were
            /// determined to exactly match another file by the same name - either in a previous
            /// archive or present archive.  Instead of storing the file twice, this file
            /// references the alternate archive entry.  This flag is not set for a file which
            /// is simply updated from a previous version, but occurs only when a file matches
            /// a file from a different path.
            /// </summary>
            [XmlAttribute]
            public bool DuplicateFile = false;

            /** Information on where to locate the file contents.. **/

            /// ArchiveFile indicates which archive contains the content of the file.  If the
            /// file is contained within this archive, ArchiveFile has the same name as the 
            /// archive.
            [XmlAttribute]
            public string ArchiveFile;

            /// PathInArchive gives the path within the archive to the content file.  In order to
            /// encrypt filenames, files within the ZIP file are given new, numeric names.  This
            /// string provides the mapping to the new, numeric name for a file entry which 
            /// describes the original file.
            [XmlAttribute]
            public string PathInArchive;

            public File() { }

            /// <summary>
            /// Creates a new Manifest File Entry.
            /// </summary>
            /// <param name="RelativePath">
            /// RelativePath gives the path relative to the source folder (root of the backup).
            /// The RelativePath refers to the original, relative path and not the path in the
            /// archive.
            /// </param>
            /// <param name="fi">Information describing the file system entry.</param>
            public File(string RelativePath, FileInfo fi)
                : base(RelativePath, fi)
            {
                this.Length = (ulong)fi.Length;
            }

            public override string ToString()
            {
                return "Manifest File Entry {" + RelativePath + "}";
            }
        }

        public class Folder : Entry
        {
            public List<File> Files = new List<File>();

            public List<Folder> Folders = new List<Folder>();

            public Folder() { }

            /// <summary>
            /// Creates a new Manifest Folder Entry.
            /// </summary>
            /// <param name="RelativePath">
            /// RelativePath gives the path relative to the source folder (root of the backup).
            /// The RelativePath refers to the original, relative path and not the path in the
            /// archive.
            /// </param>
            /// <param name="di">Information describing the file system entry.</param>
            public Folder(string RelativePath, DirectoryInfo di)
                : base(RelativePath, di)
            {
            }

            public override string ToString()
            {
                return "Manifest Folder {" + RelativePath + "}";
            }
        }        

        public Manifest() { }

        public Manifest(BackupProject Project)
        {
            BackupProjectName = Project.Name;
            SourcePath = Project.SourceFolder;
        }

        static XmlSerializer Serializer = new XmlSerializer(typeof(Manifest));

        public static Manifest FromXml(System.IO.Stream XmlStream)
        {
            return (Manifest)Serializer.Deserialize(XmlStream);
        }

        public void ToXml(System.IO.Stream to)
        {
            Serializer.Serialize(to, this);
        }
    }

    [Serializable]
    public class Flags : List<string>, ICloneable
    {
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int ii = 0; ii < base.Count; ii++)
            {
                if (ii > 0) sb.Append("," + base[ii]); else sb.Append(base[ii]);
            }
            return sb.ToString();
        }

        public new bool Contains(string str)
        {
            str = str.ToLowerInvariant();
            for (int ii = 0; ii < base.Count; ii++)
                if (base[ii].ToLowerInvariant() == str) return true;
            return false;
        }

        public override bool Equals(object obj) { return Equals(obj as Flags); }
        public override int GetHashCode()
        {
            int ret = 0;
            for (int ii = 0; ii < base.Count; ii++) ret += base[ii].GetHashCode();
            return ret;
        }

        public bool Equals(Flags b)
        {
            if (Object.ReferenceEquals(b, null)) return false;
            if (Object.ReferenceEquals(this, b)) return true;
            if (this.GetType() != b.GetType()) return false;
            if (base.Count != b.Count) return false;
            for (int ii = 0; ii < base.Count; ii++)
                if (base[ii] != b[ii]) return false;
            return true;
        }

        public static bool operator ==(Flags lhs, Flags rhs)
        {
            if (Object.ReferenceEquals(lhs, null))
            {
                if (Object.ReferenceEquals(rhs, null)) return true;
                return false;
            }
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Flags lhs, Flags rhs)
        {
            return !(lhs == rhs);
        }

        object ICloneable.Clone() { return Clone(); }
        public Flags Clone()
        {
            Flags ff = new Flags();
            for (int ii = 0; ii < base.Count; ii++) ff.Add(base[ii]);
            return ff;
        }
    }
}
