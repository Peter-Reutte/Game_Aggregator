using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace GameAggregator.EGames
{
    /// <summary>
    /// Окно аутентификации
    /// </summary>
    public partial class AuthWindow : Window
    {
        public AuthWindow()
        {
            InitializeComponent();
            WebBrowserHelper.SetBrowserEmulationMode();
            webBrowser.Navigate("https://www.epicgames.com/id/login");
        }

        /// <summary>
        /// Разлогинивает из учетной записи (путем удаления cookies)
        /// </summary>
        public void Logout()
        {
            WinInetHelper.ClearIECache("epicgames");
        }

        private void WebBrowser_Navigated(object sender, NavigationEventArgs e)
        {
            //Сокрытие сообщений об ошибках сценария
            WebBrowserHelper.HideScriptErrors(webBrowser, true);
        }
    }

    #region Helpers

    /// <summary>
    /// Вспомогательный класс для работы с настройками компонента webBrowser
    /// </summary>
    static class WebBrowserHelper
    {
        private static void SetBrowserFeatureControlKey(string feature, string appName, uint value)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(
                String.Concat(@"Software\Microsoft\Internet Explorer\Main\FeatureControl\", feature),
                RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                key.SetValue(appName, (UInt32)value, RegistryValueKind.DWord);
            }
        }

        /// <summary>
        /// Изменение записи реестра FEATURE_BROWSER_EMULATION в режим Internet Explorer 10.
        /// </summary>
        public static void SetBrowserEmulationMode()
        {
            var fileName = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);

            if (String.Compare(fileName, "devenv.exe", true) == 0 || String.Compare(fileName, "XDesProc.exe", true) == 0)
                return;
            UInt32 mode = 10000;
            SetBrowserFeatureControlKey("FEATURE_BROWSER_EMULATION", fileName, mode);
        }

        /// <summary>
        /// Сокрытие сообщений об ошибках сценария
        /// </summary>
        /// <param name="wb">webBrowser control</param>
        /// <param name="Hide">если true - скрыть сообщения, иначе - показывать</param>
        public static void HideScriptErrors(WebBrowser wb, bool Hide)
        {
            FieldInfo fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fiComWebBrowser == null)
                return;
            object objComWebBrowser = fiComWebBrowser.GetValue(wb);
            if (objComWebBrowser == null)
                return;
            objComWebBrowser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, new object[] { Hide });
        }
    }

    /// <summary>
    /// Вспомогательный класс для работы с cookies
    /// </summary>
    static class WinInetHelper
    {
        /// <summary>
        /// Finds the first URL cache entry.
        /// </summary>
        /// <param name="lpszUrlSearchPattern">The LPSZ URL search pattern.</param>
        /// <param name="lpFirstCacheEntryInfo">The lp first cache entry info.</param>
        /// <param name="lpdwFirstCacheEntryInfoBufferSize">Size of the LPDW first cache entry info buffer.</param>
        /// <returns>IntPtr.</returns>
        [DllImport(@"wininet", SetLastError = true, CharSet = CharSet.Auto,
            EntryPoint = "FindFirstUrlCacheEntryA", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr FindFirstUrlCacheEntry(
            [MarshalAs(UnmanagedType.LPTStr)] string lpszUrlSearchPattern,
            IntPtr lpFirstCacheEntryInfo,
            ref int lpdwFirstCacheEntryInfoBufferSize);

        /// <summary>
        /// Finds the next URL cache entry.
        /// </summary>
        /// <param name="hFind">The h find.</param>
        /// <param name="lpNextCacheEntryInfo">The lp next cache entry info.</param>
        /// <param name="lpdwNextCacheEntryInfoBufferSize">Size of the LPDW next cache entry info buffer.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        [DllImport(@"wininet", SetLastError = true, CharSet = CharSet.Auto,
            EntryPoint = "FindNextUrlCacheEntryA", CallingConvention = CallingConvention.StdCall)]
        public static extern bool FindNextUrlCacheEntry(
            IntPtr hFind,
            IntPtr lpNextCacheEntryInfo,
            ref int lpdwNextCacheEntryInfoBufferSize);

        /// <summary>
        /// Deletes the URL cache entry.
        /// </summary>
        /// <param name="lpszUrlName">Name of the LPSZ URL.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        [DllImport(@"wininet", SetLastError = true, CharSet = CharSet.Auto,
            EntryPoint = "DeleteUrlCacheEntryA", CallingConvention = CallingConvention.StdCall)]
        public static extern bool DeleteUrlCacheEntry(IntPtr lpszUrlName);

        /// <summary>
        /// Clears the IE cache.
        /// </summary>
        /// <param name="url">The URL part (host).</param>
        public static void ClearIECache(string hostEntry)
        {
            try
            {
                // No more items have been found.
                const int ERROR_NO_MORE_ITEMS = 259;

                // Local variables
                int cacheEntryInfoBufferSizeInitial = 0;
                int cacheEntryInfoBufferSize = 0;
                IntPtr cacheEntryInfoBuffer = IntPtr.Zero;
                INTERNET_CACHE_ENTRY_INFOA internetCacheEntry;
                IntPtr enumHandle = IntPtr.Zero;
                bool returnValue = false;

                // Start to delete URLs that do not belong to any group.
                enumHandle = FindFirstUrlCacheEntry(null, IntPtr.Zero, ref cacheEntryInfoBufferSizeInitial);
                if (enumHandle == IntPtr.Zero && ERROR_NO_MORE_ITEMS == Marshal.GetLastWin32Error())
                    return;

                cacheEntryInfoBufferSize = cacheEntryInfoBufferSizeInitial;
                cacheEntryInfoBuffer = Marshal.AllocHGlobal(cacheEntryInfoBufferSize);
                enumHandle = FindFirstUrlCacheEntry(null, cacheEntryInfoBuffer, ref cacheEntryInfoBufferSizeInitial);

                while (true)
                {
                    internetCacheEntry = (INTERNET_CACHE_ENTRY_INFOA)Marshal.PtrToStructure(cacheEntryInfoBuffer, typeof(INTERNET_CACHE_ENTRY_INFOA));

                    string sourceUrlName = Marshal.PtrToStringAnsi(internetCacheEntry.lpszSourceUrlName);
                    cacheEntryInfoBufferSizeInitial = cacheEntryInfoBufferSize;

                    if (sourceUrlName.Contains(hostEntry) && sourceUrlName.ToLower().Contains("cookie"))
                    {
                        DeleteUrlCacheEntry(internetCacheEntry.lpszSourceUrlName);
                    }

                    returnValue = FindNextUrlCacheEntry(enumHandle, cacheEntryInfoBuffer, ref cacheEntryInfoBufferSizeInitial);

                    if (!returnValue && ERROR_NO_MORE_ITEMS == Marshal.GetLastWin32Error())
                    {
                        break;
                    }
                    if (!returnValue && cacheEntryInfoBufferSizeInitial > cacheEntryInfoBufferSize)
                    {
                        cacheEntryInfoBufferSize = cacheEntryInfoBufferSizeInitial;
                        cacheEntryInfoBuffer = Marshal.ReAllocHGlobal(cacheEntryInfoBuffer, (IntPtr)cacheEntryInfoBufferSize);
                        returnValue = FindNextUrlCacheEntry(enumHandle, cacheEntryInfoBuffer, ref cacheEntryInfoBufferSizeInitial);
                    }
                }

                Marshal.FreeHGlobal(cacheEntryInfoBuffer);
            }
            catch
            {
                //error
            }
        }

        /// <summary>
        /// Struct INTERNET_CACHE_ENTRY_INFOA
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 80)]
        public struct INTERNET_CACHE_ENTRY_INFOA
        {
            [FieldOffset(0)]
            public uint dwStructSize;
            [FieldOffset(4)]
            public IntPtr lpszSourceUrlName;
            [FieldOffset(8)]
            public IntPtr lpszLocalFileName;
            [FieldOffset(12)]
            public uint CacheEntryType;
            [FieldOffset(16)]
            public uint dwUseCount;
            [FieldOffset(20)]
            public uint dwHitRate;
            [FieldOffset(24)]
            public uint dwSizeLow;
            [FieldOffset(28)]
            public uint dwSizeHigh;
            [FieldOffset(32)]
            public System.Runtime.InteropServices.ComTypes.FILETIME LastModifiedTime;
            [FieldOffset(40)]
            public System.Runtime.InteropServices.ComTypes.FILETIME ExpireTime;
            [FieldOffset(48)]
            public System.Runtime.InteropServices.ComTypes.FILETIME LastAccessTime;
            [FieldOffset(56)]
            public System.Runtime.InteropServices.ComTypes.FILETIME LastSyncTime;
            [FieldOffset(64)]
            public IntPtr lpHeaderInfo;
            [FieldOffset(68)]
            public uint dwHeaderInfoSize;
            [FieldOffset(72)]
            public IntPtr lpszFileExtension;
            [FieldOffset(76)]
            public uint dwReserved;
            [FieldOffset(76)]
            public uint dwExemptDelta;
        }

    }

    #endregion

}
