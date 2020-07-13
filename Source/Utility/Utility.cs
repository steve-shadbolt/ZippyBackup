/** ZippyBackup
 *  Copyright (C) 2012-2013 by Wiley Black.  All rights reserved.
 *  See License.txt for licensing rules.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
//using System.IO;
using Alphaleonis.Win32.Filesystem;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System.Net;
using System.Net.NetworkInformation;
using System.Management;

namespace ZippyBackup
{
    public static class Utility
    {
        public static int ToInt(char ch1, char ch2) { char[] ca = new char[] { ch1, ch2 }; return int.Parse(new string(ca)); }
        public static int ToInt(char ch1, char ch2, char ch3, char ch4) { char[] ca = new char[] { ch1, ch2, ch3, ch4 }; return int.Parse(new string(ca)); }

        /// <summary>
        /// IsContainedIn() checks whether Path is a file or subfolder within BaseFolder.  IsContainedIn() also returns true if
        /// the Path is the same folder as BaseFolder.  For example, a Path of "C:\Example\Folder\File.txt" and a BaseFolder 
        /// of "C:\Example" will return true.  IsContainedIn() can also support relative paths.  For example, a Path of 
        /// "Folder\File.txt" and a BaseFolder of "Folder" would return true.  The form must be the same however - that
        /// is, "\Folder\File.txt" and "Folder" would return false.
        /// </summary>
        /// <seealso>GetRelativePath()</seealso>
        /// <param name="BaseFolder">The base drive or directory.</param>
        /// <param name="Path">The path which is/isn't contained inside of BaseFolder.</param>
        /// <returns>True if Path is the same or contained within BaseFolder.  False otherwise.</returns>
        public static bool IsContainedIn(string BaseFolder, string Path)
        {
            BaseFolder = Utility.StripTrailingSlash(BaseFolder);
            if (BaseFolder.Length == 0)
            {
                if (Path.Length < 1) return true;
                if (Path[0] == '\\') return false;
                return true;
            }
            if (!Path.ToLowerInvariant().StartsWith(BaseFolder.ToLowerInvariant())) return false;
            if (Path.Length == BaseFolder.Length) return true;
            string RelPath = Path.Substring(BaseFolder.Length);
            if (!RelPath.StartsWith("\\")) return false;
            return true;            
        }

        /// <summary>
        /// GetFileName() works just like System.IO.Path.GetFileName(), but handles special format paths.
        /// </summary>        
        /// <param name="Path">The full or relative path to the file.</param>
        /// <returns>The filename part of the path, any trailing slash is stripped.</returns>
        public static string GetFileName(string Path)
        {
            Path = StripTrailingSlash(Path);
            int Index = Path.LastIndexOf('\\');
            if (Index < 0) Index = Path.LastIndexOf('/');
            if (Index < 0) return Path;
            return Path.Substring(Index + 1);
        }

        /// <summary>
        /// GetRelativePath() returns the path relative to a base path.  For example, a Path of "C:\Example\Folder\File.txt"
        /// and a BaseFolder of "C:\Example" will return "Folder\File.txt".
        /// </summary>
        /// <param name="BaseFolder">The base drive or directory to retrieve the path relative to.</param>
        /// <param name="Path">The path to retrieve.</param>
        /// <returns>A path string relative to BaseFolder.  If the path references a directory, any trailing slash is stripped.
        /// An exception is thrown is Path is not contained in BaseFolder.</returns>
        public static string GetRelativePath(string BaseFolder, string Path)
        {
            BaseFolder = Utility.StripTrailingSlash(BaseFolder);
            if (!Path.ToLowerInvariant().StartsWith(BaseFolder.ToLowerInvariant())) throw new FormatException("Cannot retrieve relative path - '" + Path + "' is not a part of base folder '" + BaseFolder + "'.");
            //MessageBox.Show("GetRelativePath:\nBaseFolder = " + BaseFolder + "\nPath = " + Path + "\nBaseFolder.Length = " + BaseFolder.Length + "\nPath.Length = " + Path.Length);
            if (Path.Length == BaseFolder.Length) return "";
            string RelPath = Path.Substring(BaseFolder.Length);
            if (!RelPath.StartsWith("\\")) throw new FormatException("Cannot retrieve relative path - '" + Path + "' is not a part of base folder '" + BaseFolder + "'.");
            if (RelPath.Length < 2) return "";
            return StripTrailingSlash(RelPath.Substring(1));
        }

        public static string StripTrailingSlash(string Path)
        {
            if (Path.Length < 1) return Path;
            Path = Path.Trim();
            if (Path.EndsWith("\\")) return Path.Substring(0, Path.Length - 1);
            return Path;
        }

        public static string EnsureTrailingSlash(string Path)
        {
            if (Path.Length < 1) return "\\";
            Path = Path.Trim();
            if (Path.EndsWith("\\")) return Path;
            return Path + "\\";
        }

        public static string StripLeadingSlash(string Path)
        {
            if (Path.Length < 1) return Path;
            Path = Path.Trim();
            if (Path.StartsWith("\\")) return Path.Substring(1);
            return Path;
        }

        /// <summary>
        /// GetUserHomeDirectory() retrieves the user's home directory.  For example, "C:\Users\[User]".
        /// </summary>
        /// <returns>Path to the user's home directory.</returns>
        public static string GetUserHomeDirectory()
        {
            /** In .NET 4 there is a SpecialFolder enumeration available for this purpose which is better.  I'm just
             *  not using .NET 4 at this time. **/

            if (Environment.OSVersion.Version.Major < 6)
                return Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName;
            else
                return Directory.GetParent(Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName).FullName;
        }

        public enum ListToEnglishStyle
        {
            SingleLine,
            Multiline
        }        

        /// <summary>
        /// ListToEnglish() converts a listing of various items into a proper english list.  For example, if
        /// the list consists of items Apples, Oranges, and Grapes, then the return value is "Apples, Oranges, and Grapes",
        /// but if only two items were present the return value would be "Apples and Oranges".
        /// </summary>
        /// <param name="List">The list to be converted.</param>
        /// <param name="Style">An enumeration member specifying the list style.</param>
        /// <param name="MaxLength">For single-line conversion, the maximum number of characters to permit in the list.  If the list
        /// would extend beyond this limit, then ellipsis (...) are used to terminate early.  For multi-line lists, the maximum number 
        /// of characters to permit on each line.  </param>
        /// <returns>English representation of list.</returns>
        public static string ListToEnglish(List<string> List, ListToEnglishStyle Style, int MaxLength)
        {
            if (List.Count == 0) return "None.";

            StringBuilder sb = new StringBuilder();
            StringBuilder sbLine = new StringBuilder();
            for (int ii = 0; ii < List.Count; ii++)
            {
                if (ii > 0)
                {
                    if (ii == List.Count - 1)
                    {
                        if (List.Count == 2)
                            sbLine.Append(" and ");
                        else
                            sbLine.Append(", and ");
                    }
                    else sbLine.Append(", ");
                }
                string sItem = List[ii];
                if (sbLine.Length + sItem.Length > MaxLength) {
                    if (Style == ListToEnglishStyle.SingleLine) { sbLine.Append("..."); break; }
                    else { sb.AppendLine(sbLine.ToString()); sbLine = new StringBuilder(); }
                }
                sbLine.Append(sItem);
            }
            if (sbLine.Length > 0) sb.Append(sbLine);
            return sb.ToString();
        }

        /// <summary>
        /// Converts a single index (0-based) into an alphabetic sequence (i.e. 2 -> "C". 26 -> "AA".)
        /// </summary>
        /// <param name="Value">The value to convert.</param>
        /// <returns>The letter-sequence representation of the value.</returns>
        public static string IntToLetters(int Value)
        {
            int MaxPerChar = (int)'Z' - (int)'A' + 1;
            string ret = "";

            // If MaxPerChar were 2 (A and B):
            // 0 -> "A".  1 -> "B".  2 -> "AA".  3 -> "AB".  4 -> "BA".
            // 5 -> "BB".  6 -> "AAA".  7 -> "AAB".  8 -> "ABA".
            // We'll construct the string from the right-to-left.

            while (Value >= MaxPerChar)
            {
                ret = (char)((int)'A' + (Value % MaxPerChar)) + ret;
                Value /= MaxPerChar;
                Value--;
            }
            return (char)((int)'A' + Value) + ret;

            // i.e. Value = 7.  ret -> "B".  Value -> 2.
            // Then Value = 2.  ret -> "AB".  Value -> 0.
            // Then Value = 0.  ret -> "AAB".
        }

        /// <summary>
        /// Attempts to parse an alphabetic sequence into a single 0-based value.  For example,
        /// the strings "Z", "AA", and "AB" return 25, 26, and 27 respectively.  An exception is 
        /// thrown if at least one letter is not found.  Parsing terminates on the first 
        /// non-alphabetic character or at end of string.
        /// </summary>
        /// <param name="Seq"></param>
        /// <returns></returns>
        public static int LettersToInt(string Seq)
        {
            const int MaxPerChar = (int)'Z' - (int)'A' + 1;            

            Seq = Seq.TrimStart().ToUpper();
            if (Seq.Length < 1) throw new FormatException("Expected letter sequence.");            
            char NextChar = Seq[0];
            if (NextChar < 'A' || NextChar > 'Z') throw new FormatException("Expected letter sequence from A to Z.");
            int ret = 0; int ii = 1;
            while (NextChar >= 'A' && NextChar <= 'Z')
            {
                ret *= MaxPerChar;
                ret += (int)NextChar - (int)'A' + 1;
                if (ii >= Seq.Length) break;
                NextChar = Seq[ii++];
            }
            return ret - 1;

            // If MaxPerChar were 2 (A and B)...
            // i.e. Seq = "AAB" (7).  NextChar -> "A".  ret -> 1.
            //      NextChar -> "A".  ret -> 2 + 1 -> 3.
            //      NextChar -> "B".  ret -> 6 + 2 -> 8.  Return 7.

            // i.e. Seq = "ABA" (8).  NextChar -> "A".  ret -> 1.
            //      NextChar -> "B".  ret -> 2 + 2 -> 4.
            //      NextChar -> "A".  ret -> 8 + 1 -> 9.  Return 8.
        }

        [DllImport("shell32.dll", EntryPoint = "ExtractIconA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern IntPtr ExtractIcon(int hInst, string lpszExeFileName, int nIconIndex);

        /// <summary>Retrieves the icon for a registered file type.
        /// </summary>
        /// <param name="LargeIcon">If true, the large icon is retrieved.  If false, the small icon.</param>
        /// <returns>Icon for the file type if found.  Null otherwise.</returns>
        public static Icon GetFileIcon(string Extension, bool LargeIcon)
        {
            string DefaultValue = Registry.GetValue("HKEY_CLASSES_ROOT\\" + Extension, "", null) as string;
            if (DefaultValue == null) return null;
            
            // Next it gets a little confusing.  The registry contains a default value
            // for each file extension.  For example, the HKEY_CLASSES_ROOT\.txt key
            // has a default value of txtfile.  This tells us to look for another key,
            // HKEY_CLASSES_ROOT\txtfile.
            
            string DefaultIcon = Registry.GetValue("HKEY_CLASSES_ROOT\\" + DefaultValue + "\\DefaultIcon", "", null) as string;
            if (DefaultIcon == null) return null;
            DefaultIcon = DefaultIcon.Replace('\"',' ').Trim();

            // The DefaultIcon value contains the file and icon index.  For example,
            // "C:\Program Files\Example\Software.exe,3".

            string File;
            int Index;

            int iComma = DefaultIcon.IndexOf(',');
            if (iComma < 0) { File = DefaultIcon; Index = 0; }
            else
            {
                File = DefaultIcon.Substring(0, iComma);
                Index = int.Parse(DefaultIcon.Substring(iComma + 1));
            }

            IntPtr[] hLargeIcons = new IntPtr[1] { IntPtr.Zero };
            IntPtr[] hSmallIcons = new IntPtr[1] { IntPtr.Zero };

            uint Count = ExtractIconEx(File, Index, hLargeIcons, hSmallIcons, 1);
            try
            {
                if (Count < 1) return null;

                if (LargeIcon)
                {
                    if (hLargeIcons[0] != IntPtr.Zero) return (Icon)Icon.FromHandle(hLargeIcons[0]).Clone();
                }
                else
                {
                    if (hSmallIcons[0] != IntPtr.Zero) return (Icon)Icon.FromHandle(hSmallIcons[0]).Clone();
                }
                return null;
            }
            finally
            {
                foreach (IntPtr ptr in hLargeIcons)
                    if (ptr != IntPtr.Zero) DestroyIcon(ptr);

                foreach (IntPtr ptr in hSmallIcons)
                    if (ptr != IntPtr.Zero) DestroyIcon(ptr);
            }

            //IntPtr Handle = ExtractIcon(0, File, Index);
            //return Icon.FromHandle(Handle);            
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern uint ExtractIconEx(string szFileName, int nIconIndex,
            IntPtr[] phiconLarge, IntPtr[] phiconSmall, uint nIcons);

        [DllImport("user32.dll", EntryPoint = "DestroyIcon", SetLastError = true)]
        private static extern int DestroyIcon(IntPtr hIcon);

        public static void Debug(string str)
        {
#           if DEBUG
            int count = 0;
            for (int ii = 0; ii < str.Length; ii++)
            {
                System.Diagnostics.Debug.Write(str[ii]);
                if (count++ > 100) { System.Threading.Thread.Sleep(10); count = 0; }
            }
#           endif
        }

        public static List<T> DeepCopy<T>(List<T> list)
            where T : ICloneable
        {
            List<T> ret = new List<T>(list.Count);
            for (int ii = 0; ii < list.Count; ii++)
            {
                ret.Add((T)((ICloneable)list[ii]).Clone());
            }
            return ret;
        }

        /// <summary>
        /// Represents a wildcard running on the
        /// <see cref="System.Text.RegularExpressions"/> engine.
        /// Credit: http://www.codeproject.com/Articles/11556/Converting-Wildcards-to-Regexes
        ///         Rei Miyasaka
        /// </summary>
        public class Wildcard : System.Text.RegularExpressions.Regex
        {
            /// <summary>
            /// Initializes a wildcard with the given search pattern.
            /// </summary>
            /// <param name="pattern">The wildcard pattern to match.</param>
            public Wildcard(string pattern)
                : base(WildcardToRegex(pattern))
            {
            }

            /// <summary>
            /// Initializes a wildcard with the given search pattern and options.
            /// </summary>
            /// <param name="pattern">The wildcard pattern to match.</param>
            /// <param name="options">A combination of one or more
            /// <see cref="System.Text.RegexOptions"/>.</param>
            public Wildcard(string pattern, System.Text.RegularExpressions.RegexOptions options)
                : base(WildcardToRegex(pattern), options)
            {
            }

            /// <summary>
            /// Converts a wildcard to a regex.
            /// </summary>
            /// <param name="pattern">The wildcard pattern to convert.</param>
            /// <returns>A regex equivalent of the given wildcard.</returns>
            public static string WildcardToRegex(string pattern)
            {
                return "^" + System.Text.RegularExpressions.Regex.Escape(pattern).
                 Replace("\\*", ".*").
                 Replace("\\?", ".") + "$";
            }
        }
    }    

    public static class SymbolicLink
    {
        /// <remarks>
        /// Refer to http://msdn.microsoft.com/en-us/library/windows/hardware/ff552012%28v=vs.85%29.aspx
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        public struct SymbolicLinkReparseData
        {            
            public uint ReparseTag;
            public ushort ReparseDataLength;
            public ushort Reserved;
            public ushort SubstituteNameOffset;
            public ushort SubstituteNameLength;
            public ushort PrintNameOffset;
            public ushort PrintNameLength;
            public uint Flags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
            public string PathBuffer;
        }

        internal const int ReparseHeaderSize = sizeof(uint) + sizeof(ushort) + sizeof(ushort);

        public enum ReparseTagType : uint
        {
            IO_REPARSE_TAG_MOUNT_POINT = (0xA0000003),
            IO_REPARSE_TAG_HSM = (0xC0000004),
            IO_REPARSE_TAG_SIS = (0x80000007),
            IO_REPARSE_TAG_DFS = (0x8000000A),
            IO_REPARSE_TAG_SYMLINK = (0xA000000C),
            IO_REPARSE_TAG_DFSR = (0x80000012)
        }

        private const uint genericReadAccess = 0x80000000;

        private const uint fileFlagsForOpenReparsePointAndBackupSemantics = 0x02200000;        

        private const uint openExisting = 0x3;

        //private const uint pathNotAReparsePointError = 0x80071126;

        private const uint shareModeAll = 0x7; // Read, Write, Delete

        private const int INVALID_HANDLE_VALUE = -1;

        //private const uint symLinkTag = 0xA000000C;

        //private const int targetIsAFile = 0;

        //private const int targetIsADirectory = 1;

        internal const uint FSCTL_GET_REPARSE_POINT = 0x000900a8;

        private const uint Error_PathNotAReparsePoint = 0x80071126;

#       if false
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern SafeFileHandle CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);
#       endif

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern IntPtr CreateFileW(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr SecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, int dwFlags);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool DeviceIoControl(
            IntPtr hDevice,
            uint dwIoControlCode,
            IntPtr lpInBuffer,
            uint nInBufferSize,
            IntPtr lpOutBuffer,
            uint nOutBufferSize,
            out uint lpBytesReturned,
            IntPtr lpOverlapped);

#       if false
        public static void CreateDirectoryLink(string linkPath, string targetPath)
        {
            if (!CreateSymbolicLink(linkPath, targetPath, targetIsADirectory) || Marshal.GetLastWin32Error() != 0)
            {
                try
                {
                    Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                }
                catch (COMException exception)
                {
                    throw new IOException(exception.Message, exception);
                }
            }
        }

        public static void CreateFileLink(string linkPath, string targetPath)
        {
            if (!CreateSymbolicLink(linkPath, targetPath, targetIsAFile))
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
        }

        public static bool Exists(string path)
        {
            if (!Directory.Exists(path) && !File.Exists(path))
            {
                return false;
            }
            string target = GetTarget(path);
            return target != null;
        }

        private static SafeFileHandle getFileHandle(string path)
        {
            return CreateFile(path, genericReadAccess, shareModeAll, IntPtr.Zero, openExisting,
                fileFlagsForOpenReparsePointAndBackupSemantics, IntPtr.Zero);
        }

        public static string GetTarget(string path)
        {
            SymbolicLinkReparseData reparseDataBuffer;

            using (SafeFileHandle fileHandle = getFileHandle(path))
            {
                if (fileHandle.IsInvalid)
                {
                    Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                }

                int outBufferSize = Marshal.SizeOf(typeof(SymbolicLinkReparseData));
                IntPtr outBuffer = IntPtr.Zero;
                try
                {
                    outBuffer = Marshal.AllocHGlobal(outBufferSize);
                    int bytesReturned;
                    bool success = DeviceIoControl(
                        fileHandle.DangerousGetHandle(), ioctlCommandGetReparsePoint, IntPtr.Zero, 0,
                        outBuffer, outBufferSize, out bytesReturned, IntPtr.Zero);

                    fileHandle.Close();

                    if (!success)
                    {
                        if (((uint)Marshal.GetHRForLastWin32Error()) == pathNotAReparsePointError)
                        {
                            return null;
                        }
                        Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                    }

                    reparseDataBuffer = (SymbolicLinkReparseData)Marshal.PtrToStructure(
                        outBuffer, typeof(SymbolicLinkReparseData));
                }
                finally
                {
                    Marshal.FreeHGlobal(outBuffer);
                }
            }
            if (reparseDataBuffer.ReparseTag != symLinkTag)
            {
                return null;
            }

            string target = Encoding.Unicode.GetString(reparseDataBuffer.PathBuffer,
                reparseDataBuffer.PrintNameOffset, reparseDataBuffer.PrintNameLength);

            return target;
        }
#       endif

        public static bool IsLink(string Path)
        {
            System.IO.FileAttributes fa = File.GetAttributes(Path);
            return ((fa & System.IO.FileAttributes.ReparsePoint) != 0);
        }        

        public static bool IsSymbolicLink(string Path)
        {
            try
            {
                // Step 1: If there is no reparse point information, then it definitely isn't a link.                
                System.IO.FileAttributes fa = File.GetAttributes(Path);
                if ((fa & System.IO.FileAttributes.ReparsePoint) == 0) return false;

                // Step 2: Need to pull up the reparse point information and find out if it is a symbolic link.
                IntPtr Handle = CreateFileW(Path, genericReadAccess, shareModeAll, IntPtr.Zero, openExisting,
                    fileFlagsForOpenReparsePointAndBackupSemantics, IntPtr.Zero);
                if (Handle.ToInt32() == INVALID_HANDLE_VALUE)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                try
                {
                    SymbolicLinkReparseData ReparseData = new SymbolicLinkReparseData();
                    uint BufferSize = (uint)(Marshal.SizeOf(ReparseData) + ReparseHeaderSize);
                    uint BufferSizeReceived = 0;
                    IntPtr pBuffer = Marshal.AllocHGlobal((int)BufferSize);
                    try
                    {
                        if (!DeviceIoControl(Handle, FSCTL_GET_REPARSE_POINT, IntPtr.Zero, 0, pBuffer, BufferSize, out BufferSizeReceived, IntPtr.Zero))
                        {
                            int LastError = Marshal.GetLastWin32Error();
                            if ((uint)LastError == Error_PathNotAReparsePoint) return false;
                            throw new Win32Exception(LastError);
                        }
                        ReparseData = (SymbolicLinkReparseData)Marshal.PtrToStructure(pBuffer, typeof(SymbolicLinkReparseData));
                        return ((ReparseTagType)ReparseData.ReparseTag == ReparseTagType.IO_REPARSE_TAG_SYMLINK);
                    }
                    finally { Marshal.FreeHGlobal(pBuffer); }
                }
                finally { CloseHandle(Handle); }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to determine symbolic link status for file: " + ex.Message, ex);
            }
        }        
    }

    /// <summary>
    /// A class for identifying a machine.  Used for synchronization operations by identifying which of a user's multiple
    /// computers generated a particular archive.  Use GetHashCode() to retrieve an ID specific to the machine.
    /// </summary>
    public class MachineId
    {
        public string FQDN;
        public string ComputerName;
        public List<string> HDDSerialNumbers = new List<string>();
        public List<string> MACAddresses = new List<string>();

        private class NICInfo
        {
            public long Speed;
            public PhysicalAddress MAC;
            public NetworkInterfaceType Type;
        }

        private static List<NICInfo> GetNICs()
        {
            List<NICInfo> ret = new List<NICInfo>();

            NetworkInterface[] All = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface ni in All)
            {
                NICInfo NewInfo = new NICInfo();
                NewInfo.Speed = ni.Speed;
                NewInfo.MAC = ni.GetPhysicalAddress();
                NewInfo.Type = ni.NetworkInterfaceType;
                ret.Add(NewInfo);
            }
            return ret;
        }

        private static List<string> GetMACAddresses()
        {
            List<NICInfo> NICs = GetNICs();
            List<string> MACs = new List<string>();
            foreach (NICInfo nic in NICs)
            {
                if (nic.MAC.ToString().Length < 4) continue;
                MACs.Add(nic.MAC.ToString());
            }
            return MACs;
        }

        private static List<string> GetHDDSerialNumbers()
        {
            List<string> ret = new List<string>();
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMedia");
            
            foreach (ManagementObject wmi_HD in searcher.Get())
            {
                if (wmi_HD["SerialNumber"] == null) ret.Add("");
                else ret.Add(wmi_HD["SerialNumber"].ToString());
            }

            // Trim and remove short HDD serial numbers from list...
            for (int ii = 0; ii < ret.Count; )
            {
                ret[ii] = ret[ii].Trim();
                if (ret[ii].Length < 4) ret.RemoveAt(ii);
                else ii++;
            }

            return ret;
        }

        private static string GetFQDN()
        {
            string domainName = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
            string hostName = Dns.GetHostName();
            if (!hostName.Contains(domainName)) 
                return hostName + "." + domainName;
            else 
                return hostName;
        }

        public static MachineId Host
        {
            get
            {
                MachineId mid = new MachineId();
                mid.MACAddresses = MachineId.GetMACAddresses();
                mid.HDDSerialNumbers = MachineId.GetHDDSerialNumbers();
                mid.FQDN = MachineId.GetFQDN();
                mid.ComputerName = SystemInformation.ComputerName;
                return mid;
            }
        }

        public override int GetHashCode()
        {
            int ret = 0;
            foreach (string MAC in MACAddresses)
            {
                ret += MAC.GetHashCode();
            }
            return ret;
        }
    }

    /// <summary>
    /// ExceptionWithDetail represents an exception to which all the necessary message information
    /// has already been attached - and recursive levels should not repeat the information.
    /// </summary>
    public class ExceptionWithDetail : Exception
    {
        public ExceptionWithDetail(string Message, Exception innerException)
            : base(Message, innerException)
        {
        }

        public ExceptionWithDetail(string Message)
            : base(Message)
        {
        }
    }
}
