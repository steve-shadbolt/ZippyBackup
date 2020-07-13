using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.ComponentModel;
using System.Net;

namespace ZippyBackup
{    
    public class CredentialsPrompt
    {
        #region "All Versions"

        [DllImport("ole32.dll")]
        private static extern void CoTaskMemFree(IntPtr ptr);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct CREDUI_INFO
        {
            public int cbSize;
            public IntPtr hwndParent;
            public string pszMessageText;
            public string pszCaptionText;
            public IntPtr hbmBanner;
        }

        #endregion

        #region "For Vista+"

        [DllImport("credui.dll", CharSet = CharSet.Auto)]
        private static extern bool CredUnPackAuthenticationBuffer(int dwFlags,
                                                                   IntPtr pAuthBuffer,
                                                                   uint cbAuthBuffer,
                                                                   StringBuilder pszUserName,
                                                                   ref int pcchMaxUserName,
                                                                   StringBuilder pszDomainName,
                                                                   ref int pcchMaxDomainame,
                                                                   StringBuilder pszPassword,
                                                                   ref int pcchMaxPassword);
        
        [DllImport("credui.dll", CharSet = CharSet.Auto)]
        private static extern int CredUIPromptForWindowsCredentials(ref CREDUI_INFO notUsedHere,
                                                                     int authError,
                                                                     ref uint authPackage,
                                                                     IntPtr InAuthBuffer,
                                                                     uint InAuthBufferSize,
                                                                     out IntPtr refOutAuthBuffer,
                                                                     out uint refOutAuthBufferSize,
                                                                     ref bool fSave,
                                                                     int flags);

        [Flags]
        enum CREDUIWIN_FLAGS
        {
            /// <summary>
            /// The caller is requesting that the credential provider return the user name and password in plain text.
            /// This value cannot be combined with SECURE_PROMPT.
            /// </summary>
            GENERIC = 0x1,

            /// <summary>
            /// The Save check box is displayed in the dialog box.
            /// </summary>
            CHECKBOX = 0x2,

            /// <summary>
            /// Only credential providers that support the authentication package specified by the pulAuthPackage parameter should be enumerated.
            /// This value cannot be combined with CREDUIWIN_IN_CRED_ONLY.
            /// </summary>
            AUTHPACKAGE_ONLY = 0x10,

            /// <summary>
            /// Only the credentials specified by the pvInAuthBuffer parameter for the authentication package specified by the pulAuthPackage parameter should be enumerated.
            /// If this flag is set, and the pvInAuthBuffer parameter is NULL, the function fails.
            /// This value cannot be combined with CREDUIWIN_AUTHPACKAGE_ONLY.
            /// </summary>
            IN_CRED_ONLY = 0x20,

            /// <summary>
            /// Credential providers should enumerate only administrators. This value is intended for User Account Control (UAC) purposes only. We recommend that external callers not set this flag.
            /// </summary>
            ENUMERATE_ADMINS = 0x100,

            /// <summary>
            /// Only the incoming credentials for the authentication package specified by the pulAuthPackage parameter should be enumerated.
            /// </summary>
            ENUMERATE_CURRENT_USER = 0x200,

            /// <summary>
            /// The credential dialog box should be displayed on the secure desktop. This value cannot be combined with CREDUIWIN_GENERIC.
            /// Windows Vista:  This value is supported beginning with Windows Vista with SP1.
            /// </summary>
            SECURE_PROMPT = 0x1000,

            /// <summary>
            /// The credential dialog box is invoked by the SspiPromptForCredentials function, and the client is prompted before a prior handshake. If SSPIPFC_NO_CHECKBOX is passed in the pvInAuthBuffer parameter, then the credential provider should not display the check box.
            /// Windows Vista:  This value is supported beginning with Windows Vista with SP1.
            /// </summary>
            PREPROMPTING = 0x2000,

            /// <summary>
            /// The credential provider should align the credential BLOB pointed to by the ppvOutAuthBuffer parameter to a 32-bit boundary, even if the provider is running on a 64-bit system.
            /// </summary>
            PACK_32_WOW = 0x10000000
        }

        #endregion

        #region "Before Vista"

        [Flags]
        enum CREDUI_FLAGS
        {            
            INCORRECT_PASSWORD = 0x1,
            DO_NOT_PERSIST = 0x2,
            REQUEST_ADMINISTRATOR = 0x4,
            EXCLUDE_CERTIFICATES = 0x8,
            REQUIRE_CERTIFICATE = 0x10,
            SHOW_SAVE_CHECK_BOX = 0x40,
            ALWAYS_SHOW_UI = 0x80,
            REQUIRE_SMARTCARD = 0x100,
            PASSWORD_ONLY_OK = 0x200,
            VALIDATE_USERNAME = 0x400,
            COMPLETE_USERNAME = 0x800,
            PERSIST = 0x1000,
            SERVER_CREDENTIAL = 0x4000,
            EXPECT_CONFIRMATION = 0x20000,
            GENERIC_CREDENTIALS = 0x40000,
            USERNAME_TARGET_CREDENTIALS = 0x80000,
            KEEP_USERNAME = 0x100000,
        }

        enum CredUIReturnCodes
        {
            NO_ERROR = 0,
            ERROR_CANCELLED = 1223,
            ERROR_NO_SUCH_LOGON_SESSION = 1312,
            ERROR_NOT_FOUND = 1168,
            ERROR_INVALID_ACCOUNT_NAME = 1315,
            ERROR_INSUFFICIENT_BUFFER = 122,
            ERROR_INVALID_PARAMETER = 87,
            ERROR_INVALID_FLAGS = 1004,
        }

        [DllImport("credui.dll", CharSet = CharSet.Auto)]
        private static extern CredUIReturnCodes CredUIPromptForCredentials(ref CREDUI_INFO creditUR,
            string targetName,
            IntPtr reserved1,
            int iError,
            StringBuilder userName,
            int maxUserName,
            StringBuilder password,
            int maxPassword,
            [MarshalAs(UnmanagedType.Bool)] ref bool pfSave,
            CREDUI_FLAGS flags);

        [DllImport("credui.dll", EntryPoint = "CredUIParseUserNameW", CharSet = CharSet.Unicode)]
        private static extern CredUIReturnCodes CredUIParseUserName(
            string userName,
            StringBuilder user,
            int userMaxChars,
            StringBuilder domain,
            int domainMaxChars);

        private static CredUIReturnCodes ParseUserName(string userName, ref string userPart, ref string domainPart)
        {
            StringBuilder user = new StringBuilder(100);
            StringBuilder domain = new StringBuilder(100);
            CredUIReturnCodes result = CredUIParseUserName(userName,
                                                           user, 100,
                                                           domain, 100);
            userPart = user.ToString();
            domainPart = domain.ToString();
            return result;
        }

        #endregion

        internal static void GetCredentialsVistaAndUp(string ServerName, string DisplayMessage, out NetworkCredential networkCredential)
        {
            CREDUI_INFO credui = new CREDUI_INFO();
            credui.pszCaptionText = "Please enter the credentials for " + ServerName;
            credui.pszMessageText = DisplayMessage;
            credui.cbSize = Marshal.SizeOf(credui);
            uint authPackage = 0;
            IntPtr outCredBuffer = new IntPtr();
            uint outCredSize;
            bool save = false;
            CREDUIWIN_FLAGS flags = CREDUIWIN_FLAGS.GENERIC;
            int result = CredUIPromptForWindowsCredentials(ref credui,
                                                           0,
                                                           ref authPackage,
                                                           IntPtr.Zero,
                                                           0,
                                                           out outCredBuffer,
                                                           out outCredSize,
                                                           ref save,
                                                           (int)flags);

            var usernameBuf = new StringBuilder(100);
            var passwordBuf = new StringBuilder(100);
            var domainBuf = new StringBuilder(100);

            int maxUserName = 100;
            int maxDomain = 100;
            int maxPassword = 100;
            if (result == 0)
            {
                if (CredUnPackAuthenticationBuffer(0, outCredBuffer, outCredSize, usernameBuf, ref maxUserName,
                                                   domainBuf, ref maxDomain, passwordBuf, ref maxPassword))
                {
                    //TODO: ms documentation says we should call this but i can't get it to work
                    //SecureZeroMem(outCredBuffer, outCredSize);

                    //clear the memory allocated by CredUIPromptForWindowsCredentials 
                    CoTaskMemFree(outCredBuffer);
                    networkCredential = new NetworkCredential()
                                            {
                                                UserName = usernameBuf.ToString(),
                                                Password = passwordBuf.ToString(),
                                                Domain = domainBuf.ToString()
                                            };
                    return;
                }
            }

            networkCredential = null;
        }

        /// <summary>
        /// Prompts for password.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <returns>True if no errors.</returns>
        internal static bool GetCredentialsBeforeVista(string ShareOrServerName, out string user, out string domain, out string password)
        {
            // Setup the flags and variables
            StringBuilder userPassword = new StringBuilder(100), userID = new StringBuilder(100);
            CREDUI_INFO credUI = new CREDUI_INFO();
            credUI.cbSize = Marshal.SizeOf(credUI);
            bool save = false;
            CREDUI_FLAGS flags = CREDUI_FLAGS.ALWAYS_SHOW_UI | CREDUI_FLAGS.GENERIC_CREDENTIALS | CREDUI_FLAGS.DO_NOT_PERSIST;

            // Prompt the user
            CredUIReturnCodes returnCode = CredUIPromptForCredentials(ref credUI, ShareOrServerName, IntPtr.Zero, 0, userID, 100, userPassword, 100, ref save, flags);

            if (returnCode != CredUIReturnCodes.NO_ERROR) { user = null; domain = null; password = null; return false; }
            
            password = userPassword.ToString();
            user = ""; domain = "";
            returnCode = ParseUserName(userID.ToString(), ref user, ref domain);

            return (returnCode == CredUIReturnCodes.NO_ERROR);
        }

        public static string ServerNameFromPath(string Path)
        {
            int iAfterPrefix = 0;
            if (Path.StartsWith(@"\\?\UNC\")) iAfterPrefix = @"\\?\UNC\".Length;
            else if (Path.StartsWith(@"\\")) iAfterPrefix = @"\\".Length;

            int iSlash = Path.IndexOf('\\', iAfterPrefix);
            if (iSlash < 0) iSlash = Path.IndexOf('/', iAfterPrefix);
            if (iSlash < 0) return Path;
            return Path.Substring(0, iSlash);
        }

        /// <summary>
        /// Prompts the user for network credentials using a system dialog.  The interface appears
        /// differently for Windows versions Vista and above than for previous versions.  
        /// </summary>
        /// <param name="ServerName">Name of server to acquire credentials for.  i.e. "\\Server"</param>
        /// <param name="DisplayPrompt">Text to be displayed to user (may not appear in all OS versions).</param>
        /// <returns>The NetworkCredential provided by the user, or null on cancel.</returns>
        public static NetworkCredential PromptForCredentials(string ServerName, string DisplayPrompt)
        {
            if (System.Environment.OSVersion.Version.Major >= 6)
            {
                // Running Windows Vista or newer.
                NetworkCredential ret;
                GetCredentialsVistaAndUp(ServerName, DisplayPrompt, out ret);
                return ret;
            }
            else
            {
                // Running Windows before Vista.
                string user, domain, password;
                if (!GetCredentialsBeforeVista(ServerName, out user, out domain, out password)) return null;
                NetworkCredential ret = new NetworkCredential();
                ret.UserName = user;
                ret.Domain = domain;
                ret.Password = password;
                return ret;
            }
        }
    }
}
