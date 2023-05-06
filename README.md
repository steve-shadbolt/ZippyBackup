# ZippyBackup
An open-source backup utility for Windows using encrypted ZIP files and XML metadata as the storage format.  Written in C#.

Features:
- Uses standard .zip files as the format.  So your important backups aren't locked behind anything proprietary but are also compressed and not wasting space.
- Performs differential backups- meaning that it only stores files that have changed since the previous backup.
- Allows you to review your backups over time- see the same file from 2020, 2018, or 2015 in all its different versions.
- Can use encrypted .zip files if you want your data protected.
- The table of information (metadata) about your files is in XML files, which are text and you can view and edit- though you shouldn't ever need to.
- Can automatically backup when your PC is idle.  Can launch at Windows start, if you want it to.
- Can run verifications where it checks that it can still access old archives and that the .zip files are intact.
- Although ZippyBackup is "complete", it is just one developer at present and might not work without some issues.
- Free, open-source.  Please contribute improvements!

![Example](Documentation/Example.png)
