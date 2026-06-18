using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace VPet.Plugin.DoingDisplay
{
    public static class ForegroundAppHelper
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        public static IntPtr GetForegroundWindowHandle()
        {
            return GetForegroundWindow();
        }

        public static uint GetWindowProcessId(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) return 0;
            GetWindowThreadProcessId(hWnd, out uint processId);
            return processId;
        }

        public static string GetWindowTitle(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) return string.Empty;
            try
            {
                int length = GetWindowTextLength(hWnd);
                if (length == 0) return string.Empty;
                var sb = new StringBuilder(length + 1);
                GetWindowText(hWnd, sb, sb.Capacity);
                return sb.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string GetProcessName(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) return string.Empty;
            try
            {
                uint processId = GetWindowProcessId(hWnd);
                if (processId == 0) return string.Empty;
                return Process.GetProcessById((int)processId).ProcessName;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static (string ProcessName, string WindowTitle) GetForegroundAppInfo()
        {
            IntPtr hWnd = GetForegroundWindowHandle();
            if (hWnd == IntPtr.Zero) return (string.Empty, string.Empty);

            string processName = GetProcessName(hWnd);
            string windowTitle = GetWindowTitle(hWnd);
            return (processName, windowTitle);
        }
    }
}
