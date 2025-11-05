using System;
using System.Runtime.InteropServices;

namespace WinAPIWrapper
{
    public static class Win32APIWrapper
    {
        [DllImport("user32.dll")] public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        public static bool CheckWindowExists(string windowName)
        {
            bool bFindWindow = false;
            var hWnd = FindWindow(null, windowName);
            if (!hWnd.Equals(IntPtr.Zero))
            {
                bFindWindow = true;
            }

            return bFindWindow;
        }
    }
}
