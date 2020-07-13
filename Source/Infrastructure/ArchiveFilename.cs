/** ArchiveFilename.cs
 *  Copyright (C) 2012-2015 by Wiley Black.  All rights reserved.
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
using ZippyBackup.User_Interface;
using Alphaleonis.Win32.Filesystem;         // Replaces System.IO entities such as FileInfo with more advanced versions.
using ZippyBackup.IO;
using ZippyBackup.Diagnostics;

namespace ZippyBackup
{
    public class ArchiveFilename : ICloneable, ISortable
    {
        /// <summary>
        /// Date (without time) when the backup was made.  This is written into the filename
        /// of a ZippyBackup file.
        /// </summary>
        public DateTime BackupDate;

        /// <summary>
        /// Sequence number (time) when the backup was made.  Zero for the first backup of the
        /// day, 1 for the second, and so on.  This is written into the filename of a ZippyBackup
        /// file as a letter (A, B, so on.)
        /// </summary>
        public int BackupTime;

        /// <summary>
        /// Indicates whether the file includes a comprehensive backup or an incremental backup.
        /// </summary>
        public BackupTypes BackupType;

        public static ArchiveFilename MinValue = new ArchiveFilename(DateTime.MinValue, 0, BackupTypes.Any);
        public static ArchiveFilename MaxValue = new ArchiveFilename(DateTime.MaxValue, int.MaxValue, BackupTypes.Any);
        public static ArchiveFilename MinCompleteValue = new ArchiveFilename(DateTime.MinValue, 0, BackupTypes.Complete);
        public static ArchiveFilename MaxCompleteValue = new ArchiveFilename(DateTime.MaxValue, int.MaxValue, BackupTypes.Complete);
        public static ArchiveFilename MinIncrementalValue = new ArchiveFilename(DateTime.MinValue, 0, BackupTypes.Incremental);
        public static ArchiveFilename MaxIncrementalValue = new ArchiveFilename(DateTime.MaxValue, int.MaxValue, BackupTypes.Incremental);

        public ArchiveFilename() { }

        public ArchiveFilename(DateTime BackupDate, int BackupTime)
        {
            this.BackupDate = new DateTime(BackupDate.Year, BackupDate.Month, BackupDate.Day);
            this.BackupTime = BackupTime;
        }

        public ArchiveFilename(DateTime BackupDate, int BackupTime, BackupTypes BackupType)
        {
            this.BackupDate = new DateTime(BackupDate.Year, BackupDate.Month, BackupDate.Day);
            this.BackupTime = BackupTime;
            this.BackupType = BackupType;
        }

        public object Clone()
        {
            return new ArchiveFilename(BackupDate, BackupTime, BackupType);
        }

        // Largest values rise to the top of the SortedListBox.  Values are relative (do not
        // need to be consecutive).
        public long GetSortOrder()
        {
            return BackupDate.Ticks + (long)BackupTime;
        }

        /// <summary>
        /// Parse() converts a filename in the ZippyBackup format to a date and time indicator.
        /// The time indicator is a sequence counter (i.e. A, B, C, etc.) as opposed to an exact time.  The
        /// exact time is available from the XML Manifest inside the zip file.  
        /// </summary>
        /// <param name="Filename">ZippyBackup format filename to parse.</param>        
        /// <returns>Parsed archive filename.  An exception is thrown if the filename cannot be parsed.</returns>
        public static ArchiveFilename Parse(string Filename)
        {
            ArchiveFilename ret;
            if (!TryParse(Filename, out ret))
                throw new FormatException("Unable to interpret archive filename.");
            return ret;
        }

        /// <summary>
        /// TryParse() converts a filename in the ZippyBackup format to a date and time indicator.
        /// The time indicator is a sequence counter (i.e. A, B, C, etc.) as opposed to an exact time.  The
        /// exact time is available from the XML Manifest inside the zip file.  
        /// </summary>
        /// <param name="Filename">ZippyBackup format filename to parse.</param>
        /// <returns>True if the filename was parsed successfully.  False otherwise.  An exception is thrown
        /// if a file is identified as a backup zip file but we are otherwise unable to parse the filename.</returns>
        public static bool TryParse(string Filename, out ArchiveFilename Result)
        {
            Result = new ArchiveFilename();
            if (!Filename.ToLowerInvariant().StartsWith("backup_")) return false;
            if (!Filename.ToLowerInvariant().EndsWith(".zip")) return false;

            // Backup_Jan21A_1980.zip
            // Backup_Mar18B_1981_Complete.zip
            // Backup_Jan21BB_1980.zip
            // 012345678901234567890123456

            if (Filename.Length < 22) return false;            

            // Locate the index of the first underscore.
            int iUnderscore = Filename.IndexOf('_');
            if (iUnderscore < 0 || iUnderscore + 1 >= Filename.Length) throw new FormatException("Unable to locate second underscore in filename within project backup directory: " + Filename);
            // Locate the index of the second underscore, place it into iUnderscore for later use.
            int ii = Filename.Substring(iUnderscore + 1).IndexOf('_');
            iUnderscore += ii + 1;
            if (ii < 0 || iUnderscore + 4 >= Filename.Length) throw new FormatException("Invalid backup filename format in project backup directory: " + Filename);

            // Parse the month abbreviation as a string
            string MonthAbbr = new string(new char[] { Filename[7], Filename[8], Filename[9] });
            int Day, Year;
            // Parse the day
            if (!int.TryParse(new string(new char[] { Filename[10], Filename[11] }), out Day)) throw new FormatException("Invalid day specification in backup filename in project backup directory: " + Filename);
            // Parse the time, which is just the sequence letter (i.e. A, B, C, ... AA, AB, AC, etc.)
            try
            {
                Result.BackupTime = Utility.LettersToInt(Filename.Substring(12));
            }
            catch (Exception) { throw new FormatException("Unable to parse backup time (letter) from filename in backup directory file: " + Filename); }

            // Parse the year
            if (!int.TryParse(new string(new char[] { Filename[iUnderscore + 1], Filename[iUnderscore + 2], Filename[iUnderscore + 3], Filename[iUnderscore + 4] }),
                out Year)) throw new FormatException("Unable to parse year from filename in project backup directory filename: " + Filename);

            // Parse the month from abbreviation string
            int Month = DateTime.ParseExact(MonthAbbr, "MMM", CultureInfo.CurrentCulture).Month;
            // Combine the parsed date information into a DateTime.
            Result.BackupDate = new DateTime(Year, Month, Day);
            // Look for "_Complete" indicator on filename.
            if (Filename.Contains("_Complete")) Result.BackupType = BackupTypes.Complete; else Result.BackupType = BackupTypes.Incremental;

            return true;
        }

        public override string ToString()
        {
            return "Backup_" + BackupDate.ToString("MMMdd")
                + Utility.IntToLetters(BackupTime)
                + "_" + BackupDate.ToString("yyyy")
                + (BackupType == BackupTypes.Complete ? "_Complete" : "") + ".zip";
        }

        public static bool operator <(ArchiveFilename a, ArchiveFilename b)
        {
            return a.BackupDate < b.BackupDate || (a.BackupDate == b.BackupDate && a.BackupTime < b.BackupTime);
        }

        public static bool operator >(ArchiveFilename a, ArchiveFilename b)
        {
            return a.BackupDate > b.BackupDate || (a.BackupDate == b.BackupDate && a.BackupTime > b.BackupTime);
        }

        public static bool operator ==(ArchiveFilename a, ArchiveFilename b)
        {
            if ((object)a == null || (object)b == null)
            {
                if ((object)a != null || (object)b != null) return false;
                return true;
            }

            return (a.BackupDate == b.BackupDate && a.BackupTime == b.BackupTime);
        }

        public static bool operator !=(ArchiveFilename a, ArchiveFilename b)
        {
            return !(a == b);
        }

        public static bool operator >=(ArchiveFilename a, ArchiveFilename b)
        {
            return a.BackupDate > b.BackupDate || (a.BackupDate == b.BackupDate && a.BackupTime >= b.BackupTime);
        }

        public static bool operator <=(ArchiveFilename a, ArchiveFilename b)
        {
            return a.BackupDate < b.BackupDate || (a.BackupDate == b.BackupDate && a.BackupTime <= b.BackupTime);
        }

        public override int GetHashCode()
        {
            return (BackupDate.GetHashCode() << 8) + (BackupTime << 2) + (int)BackupType;
        }

        public override bool Equals(object obj)
        {
            ArchiveFilename b = obj as ArchiveFilename;
            if ((object)b == null) return false;
            return (BackupDate == b.BackupDate && BackupTime == b.BackupTime && BackupType == b.BackupType);
        }

        /// <summary>
        /// LoadArchiveManifest decompresses the manifest information from the backup archive into RAM
        /// and converts it from XML into a Manifest object.  An exception is thrown if the manifest
        /// cannot be retrieved.
        /// 
        /// Precondition: Access to the backup folder must be available - thus any impersonation should
        /// be done before calling.
        /// </summary>        
        /// <returns>The manifest from this backup archive.</returns>
        public Manifest LoadArchiveManifest(BackupProject Project, bool PromptForPassword)
        {
            try
            {
                using (ZipFile zip = ZipFile.Read(Project.CompleteBackupFolder + "\\" + this.ToString()))
                {
                    foreach (ZipEntry ze in zip)
                    {
                        if (ze.FileName.ToLowerInvariant() == "manifest.xml")
                        {
                            using (MemoryStream ms = new MemoryStream())
                            {
                                try
                                {
                                    if (String.IsNullOrEmpty(Project.SafePassword.Password))
                                        ze.Extract(ms);
                                    else
                                        ze.ExtractWithPassword(ms, Project.SafePassword.Password);
                                }
                                catch (Ionic.Zip.BadPasswordException bpe1)
                                {
                                    try
                                    {
                                        if (!String.IsNullOrEmpty(Project.AlternativePassword))
                                            ze.ExtractWithPassword(ms, Project.AlternativePassword);
                                        else throw bpe1;
                                    }
                                    catch (Ionic.Zip.BadPasswordException)
                                    {
                                        bool FirstPrompt = true;
                                        if (!PromptForPassword) throw bpe1;
                                        while (PromptForPassword)
                                        {
                                            PasswordForm pf = new PasswordForm();
                                            if (FirstPrompt)
                                                pf.Prompt = "The archive '" + ToString() + "' was created with a different password.  (242)";
                                            else
                                                pf.Prompt = "That was not a valid password for the archive '" + ToString() + "'.";
                                            FirstPrompt = false;
                                            if (pf.ShowDialog() != DialogResult.OK) throw new CancelException();
                                            Project.AlternativePassword = pf.Password;
                                            try { ze.ExtractWithPassword(ms, Project.AlternativePassword); break; }
                                            catch (Ionic.Zip.BadPasswordException) { }
                                        }
                                    }
                                }
                                ms.Seek(0, System.IO.SeekOrigin.Begin);
                                try
                                {
                                    Manifest ret = Manifest.FromXml(ms);
                                    if (ret == null) throw new FormatException();
                                    return ret;
                                }
                                catch (Exception ex)
                                {
                                    throw new FormatException("Unable to parse archive's manifest.  Error: " + ex.Message + "\n\n" +
                                        "Project: " + Project.ToString() + "\nArchive: " + ToString() + "\nManifest File: " + ze.FileName);
                                }
                            }
                        }
                    }
                    throw new FileNotFoundException("Manifest was not found within the archive.");
                }
            }
            catch (CancelException ce) { throw ce; }
            catch (Ionic.Zip.BadPasswordException ex)
            {
                throw new Ionic.Zip.BadPasswordException(ex.Message + "\nUnable to retrieve archive manifest.\nArchive name: " + this.ToString(), ex);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\nUnable to retrieve archive manifest.\nArchive name: " + this.ToString(), ex);
            }
        }
    }

    public class ArchiveFileList
    {
        public List<ArchiveFilename> Archives;

        public ArchiveFileList() { }

        public void LoadAll(BackupProject Project)
        {
            Archives = new List<ArchiveFilename>();

            try
            {
                FileInfo[] FileList = new DirectoryInfo(Project.CompleteBackupFolder).GetFiles("Backup_*.zip");
                foreach (FileInfo fi in FileList)
                {
                    ArchiveFilename af;
                    if (!ArchiveFilename.TryParse(fi.Name, out af)) continue;
                    Archives.Add(af);
                }
            }
            catch (DirectoryNotFoundException)
            {
                return;
            }
        }

        /// <summary>
        /// Locates the most recent backup (complete or incremental) in the file list.  
        /// </summary>
        /// <returns>The filename of the most recent backup, or ArchiveFilename.MaxValue if no match was found.</returns>
        public ArchiveFilename FindMostRecent()
        {
            return Find(ArchiveFilename.MinValue, ArchiveFilename.MaxValue, ArchiveFilename.MaxValue);
        }

#       if false    // Filename based search
        /// <summary>
        /// Find() locates the backup file which meets all of the following criteria:
        /// <list type="bullet">
        ///     <item><description>Has a date and time sequence which is greater than or equal to Before.</description></item>
        ///     <item><description>Has a date and time sequence which is less than or equal to After.</description></item>
        ///     <item><description>Matches the BackupType value in Target, unless Any is specified.</description></item>
        ///     <item><description>Is the file meeting the above criteria which is closest matching to Target.</description></item>
        /// </list>
        /// </summary>
        /// <param name="After">Date and time sequence after or equal to which the match must be marked.</param>
        /// <param name="Target">Ideal date and time to locate, and BackupType value.</param>
        /// <param name="Before">Date and time sequence before or equal to which the match must be marked.</param>
        /// <returns>The filename of the best match to the request, or ArchiveFilename.MaxValue if no match was found.</returns>
        public ArchiveFilename Find(ArchiveFilename After, ArchiveFilename Target, ArchiveFilename Before)
        {
            ArchiveFilename BestMatch = ArchiveFilename.MaxValue;
            long BestDateError = long.MaxValue;
            int BestTimeError = int.MaxValue;

            foreach (FileInfo File in Archives)
            {
                if (Target.BackupType == BackupTypes.Any
                 || ((Target.BackupType == BackupTypes.Complete && File.Name.Contains("_Complete"))
                  || (Target.BackupType == BackupTypes.Incremental && !File.Name.Contains("_Complete"))))
                {
                    ArchiveFilename Entry;
                    if (!ArchiveFilename.TryParse(File.Name, out Entry)) continue;
                    if (Entry >= After && Entry <= Before)
                    {
                        long DateError = Math.Abs((Target.BackupDate - Entry.BackupDate).Ticks);
                        if (DateError > BestDateError) continue;
                        if (DateError == BestDateError && Math.Abs(Target.BackupTime - Entry.BackupTime) > BestTimeError) continue;
                        BestMatch = Entry;
                    }
                }
            }
            return BestMatch;
        }
#       else        // ArchiveFilename based search
        /// <summary>
        /// Find() locates the backup file which meets all of the following criteria:
        /// <list type="bullet">
        ///     <item><description>Has a date and time sequence which is greater than or equal to Before.</description></item>
        ///     <item><description>Has a date and time sequence which is less than or equal to After.</description></item>
        ///     <item><description>Matches the BackupType value in Target, unless Any is specified.</description></item>
        ///     <item><description>Is the file meeting the above criteria which is closest matching to Target.</description></item>
        /// </list>
        /// </summary>
        /// <param name="After">Date and time sequence after or equal to which the match must be marked.</param>
        /// <param name="Target">Ideal date and time to locate, and BackupType value.</param>
        /// <param name="Before">Date and time sequence before or equal to which the match must be marked.</param>
        /// <returns>The filename of the best match to the request, or ArchiveFilename.MaxValue if no match was found.</returns>
        public ArchiveFilename Find(ArchiveFilename After, ArchiveFilename Target, ArchiveFilename Before)
        {
            ArchiveFilename BestMatch = ArchiveFilename.MaxValue;
            long BestDateError = long.MaxValue;
            int BestTimeError = int.MaxValue;

            foreach (ArchiveFilename Archive in Archives)
            {
                if (Target.BackupType == BackupTypes.Any || Target.BackupType == Archive.BackupType)
                {
                    if (Archive >= After && Archive <= Before)
                    {
                        long DateError = Math.Abs((Target.BackupDate - Archive.BackupDate).Ticks);
                        if (DateError > BestDateError) continue;
                        int TimeError = Math.Abs(Target.BackupTime - Archive.BackupTime);
                        if (DateError == BestDateError && TimeError > BestTimeError) continue;
                        BestMatch = Archive;
                        BestDateError = DateError;
                        BestTimeError = TimeError;
                    }
                }
            }
            return BestMatch;
        }
#       endif

        //object ICloneable.Clone() { return Clone(); }
        public ArchiveFileList Clone()
        {
            ArchiveFileList cp = new ArchiveFileList();
            cp.Archives = Utility.DeepCopy(cp.Archives);
            return cp;
        }
    }    
}
