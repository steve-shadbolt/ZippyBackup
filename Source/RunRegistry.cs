/** ZippyBackup
 *  Copyright (C) 2012-2013 by Wiley Black.  All rights reserved.
 *  See License.txt for licensing rules.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.IO;
using System.Reflection;

namespace ZippyBackup
{
    public class RunRegistry
    {
        public static void UpdateLegacyRegistry()
        {
            string SubKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";
            RegistryKey RunKey = Registry.CurrentUser.OpenSubKey(SubKey);
            string[] ValueNames = RunKey.GetValueNames();
            bool AddRunAtStartup = false;
            foreach (string ValueName in ValueNames)
            {
                if (RunKey.GetValueKind(ValueName) == RegistryValueKind.String)
                {
                    string Value = RunKey.GetValue(ValueName) as string;
                    if (Value == null) continue;

                    if (Value.Contains("ZipBackup.exe"))
                    {
                        RegistryKey RunKeyEx = Registry.CurrentUser.OpenSubKey(SubKey, true);
                        RunKeyEx.DeleteValue(ValueName, false);
                        AddRunAtStartup = true;
                    }
                }
            }
            if (AddRunAtStartup) MakeRunAtStartup("ZippyBackup", "/tray");            
        }

        public static string GetExecutablePath()
        {
            return Assembly.GetExecutingAssembly().Location;
        }

        private static bool IsExecutableReference(string Value)
        {
            string ExePath = GetExecutablePath().ToLowerInvariant();
            Value = Value.ToLowerInvariant();
            return Value.StartsWith(ExePath) || Value.StartsWith("\"" + ExePath);            
        }

        public static bool IsRunAtStartup()
        {
            string SubKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";
            RegistryKey RunKey = Registry.CurrentUser.OpenSubKey(SubKey);
            string[] ValueNames = RunKey.GetValueNames();
            foreach (string ValueName in ValueNames)
            {
                if (RunKey.GetValueKind(ValueName) == RegistryValueKind.String)
                {
                    string Value = RunKey.GetValue(ValueName) as string;
                    if (Value == null) continue;

                    if (IsExecutableReference(Value)) return true;
                }
            }
            return false;
        }

        public static void ClearRunAtStartup()
        {
            string SubKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";
            RegistryKey RunKey = Registry.CurrentUser.OpenSubKey(SubKey, true);
            string[] ValueNames = RunKey.GetValueNames();
            foreach (string ValueName in ValueNames)
            {
                if (RunKey.GetValueKind(ValueName) == RegistryValueKind.String)
                {
                    string Value = RunKey.GetValue(ValueName) as string;
                    if (Value == null) continue;

                    if (IsExecutableReference(Value))
                    {
                        RunKey.DeleteValue(ValueName, false);
                    }
                }
            }            
        }

        public static void MakeRunAtStartup(string ProgramLabel, string Arguments)
        {
            string FullLaunch = "\"" + GetExecutablePath() + "\" " + Arguments;

            // First, check for existing entries that match our executable.  If they match the executable
            // but don't match the label or arguments, remove them.  If they match exactly, only allow
            // one instance.
            string SubKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";
            RegistryKey RunKey = Registry.CurrentUser.OpenSubKey(SubKey, true);
            string[] ValueNames = RunKey.GetValueNames();
            bool Found = false;
            foreach (string ValueName in ValueNames)
            {
                if (RunKey.GetValueKind(ValueName) == RegistryValueKind.String)
                {
                    string Value = RunKey.GetValue(ValueName) as string;
                    if (Value == null) continue;

                    if (IsExecutableReference(Value))
                    {
                        if (!Found && ValueName == ProgramLabel && Value == FullLaunch) Found = true;
                        else RunKey.DeleteValue(ValueName, false);
                    }
                }
            }
            if (Found) return;
            
            // Either entries were not found or they didn't match exactly and were removed.  Add our
            // new, correct entry.
            RunKey.SetValue(ProgramLabel, FullLaunch);
        }
    }
}
