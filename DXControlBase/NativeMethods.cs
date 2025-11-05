using System;
using System.Runtime.InteropServices;

namespace DXControlBase
{
    /// <summary>
    /// Defines the native methods
    /// </summary>
    public static class NativeMethods
    {
        [DllImport("user32.dll", SetLastError = false)]
        public static extern IntPtr GetDesktopWindow();
    }
}
