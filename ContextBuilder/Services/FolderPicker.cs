using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace ContextBuilder.Services;

public class FolderPicker
{
    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern int SHBrowseForFolder(ref BROWSEINFO lpbi);

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern bool SHGetPathFromIDList(IntPtr pidl, IntPtr pszPath);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct BROWSEINFO
    {
        public IntPtr hwndOwner;
        public IntPtr pidlRoot;
        public IntPtr pszDisplayName;
        public string lpszTitle;
        public uint ulFlags;
        public IntPtr lpfn;
        public IntPtr lParam;
        public int iImage;
    }

    public string PickFolder()
    {
        var bi = new BROWSEINFO
        {
            lpszTitle = "Select folder"
        };

        IntPtr pidl = SHBrowseForFolder(ref bi);

        if (pidl == IntPtr.Zero)
            return null;

        IntPtr path = Marshal.AllocHGlobal(260);
        SHGetPathFromIDList(pidl, path);

        string result = Marshal.PtrToStringAuto(path);

        Marshal.FreeHGlobal(path);

        return result;
    }
}