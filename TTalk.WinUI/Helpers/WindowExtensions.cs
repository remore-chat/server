using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WinRT;
using Microsoft.UI.Xaml;

namespace TTalk.WinUI.Helpers
{
    public static class WindowExtensions
    {
        public static void SetIcon(this Window window, string iconName)
        {
            unsafe
            {
                fixed (char* icon = iconName)
                {
                    LoadIcon(icon, window);
                }
            }
        }

        #region helpers

        private const int IMAGE_ICON = 1;
        private const int LR_LOADFROMFILE = 0x0010;

        private static unsafe void LoadIcon(char* iconName, Window window)
        {
            //Get the Window's HWND
            var hwnd = window.As<IWindowNative>().WindowHandle;
            IntPtr hIcon = PInvoke.User32.LoadImage(IntPtr.Zero, iconName,
                      PInvoke.User32.ImageType.IMAGE_ICON, 32, 32, PInvoke.User32.LoadImageFlags.LR_LOADFROMFILE);

            PInvoke.User32.SendMessage(hwnd, PInvoke.User32.WindowMessage.WM_SETICON, (IntPtr)0, hIcon);
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("EECDBF0E-BAE9-4CB6-A68E-9598E1CB57BB")]
        internal interface IWindowNative
        {
            IntPtr WindowHandle { get; }
        }

        #endregion //helpers
    }
}
