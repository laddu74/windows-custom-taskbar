using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace CustomTaskbar.Interop
{
    public class AppBarManager : IDisposable
    {
        private Window _window;
        private IntPtr _hwnd;
        private int _callbackMessageId;
        private bool _isRegistered = false;

        public AppBarManager(Window window)
        {
            _window = window;
            _window.SourceInitialized += OnSourceInitialized;
            _window.Closed += OnClosed;
        }

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            var helper = new WindowInteropHelper(_window);
            _hwnd = helper.Handle;
            HwndSource.FromHwnd(_hwnd).AddHook(HwndProc);

            RegisterBar();
        }

        private void RegisterBar()
        {
            HideWindowsTaskbar();

            NativeMethods.APPBARDATA abd = new NativeMethods.APPBARDATA();
            abd.cbSize = Marshal.SizeOf(abd);
            abd.hWnd = _hwnd;

            _callbackMessageId = NativeMethods.RegisterWindowMessage("AppBarMessage");
            abd.uCallbackMessage = _callbackMessageId;

            uint ret = NativeMethods.SHAppBarMessage(NativeMethods.ABM_NEW, ref abd);
            _isRegistered = true;

            ABSetPos();
        }

        private void ABSetPos()
        {
            NativeMethods.APPBARDATA abd = new NativeMethods.APPBARDATA();
            abd.cbSize = Marshal.SizeOf(abd);
            abd.hWnd = _hwnd;
            abd.uEdge = NativeMethods.ABE_BOTTOM;

            double dpiScale = GetDpiScale();
            int barHeight = (int)(_window.Height * dpiScale); 

            abd.rc.left = 0;
            abd.rc.right = (int)(SystemParameters.PrimaryScreenWidth * dpiScale);
            
            int screenHeight = (int)(SystemParameters.PrimaryScreenHeight * dpiScale);
            abd.rc.top = screenHeight - barHeight;
            abd.rc.bottom = screenHeight;

            NativeMethods.SHAppBarMessage(NativeMethods.ABM_QUERYPOS, ref abd);
            NativeMethods.SHAppBarMessage(NativeMethods.ABM_SETPOS, ref abd);

            _window.Left = abd.rc.left / dpiScale;
            _window.Top = abd.rc.top / dpiScale;
            _window.Width = (abd.rc.right - abd.rc.left) / dpiScale;
            _window.Height = (abd.rc.bottom - abd.rc.top) / dpiScale;
            
            // Backup standard move just in case WPF ignores the properties
            NativeMethods.MoveWindow(_hwnd, abd.rc.left, abd.rc.top,
                abd.rc.right - abd.rc.left, abd.rc.bottom - abd.rc.top, true);
        }

        private double GetDpiScale()
        {
            try
            {
                PresentationSource source = PresentationSource.FromVisual(_window);
                if (source != null && source.CompositionTarget != null)
                {
                    return source.CompositionTarget.TransformToDevice.M11;
                }
            }
            catch { }
            return 1.0;
        }

        private IntPtr HwndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == _callbackMessageId)
            {
                if (wParam.ToInt32() == NativeMethods.ABM_WINDOWPOSCHANGED)
                {
                    ABSetPos();
                }
            }
            return IntPtr.Zero;
        }

        private void OnClosed(object sender, EventArgs e)
        {
            Dispose();
        }

        private void HideWindowsTaskbar()
        {
            IntPtr hwnd = NativeMethods.FindWindow("Shell_TrayWnd", null);
            if (hwnd != IntPtr.Zero)
            {
                NativeMethods.ShowWindow(hwnd, NativeMethods.SW_HIDE);
            }
        }

        private void ShowWindowsTaskbar()
        {
            IntPtr hwnd = NativeMethods.FindWindow("Shell_TrayWnd", null);
            if (hwnd != IntPtr.Zero)
            {
                NativeMethods.ShowWindow(hwnd, NativeMethods.SW_SHOW);
            }
        }

        public void Dispose()
        {
            ShowWindowsTaskbar();

            if (_isRegistered)
            {
                NativeMethods.APPBARDATA abd = new NativeMethods.APPBARDATA();
                abd.cbSize = Marshal.SizeOf(abd);
                abd.hWnd = _hwnd;
                NativeMethods.SHAppBarMessage(NativeMethods.ABM_REMOVE, ref abd);
                _isRegistered = false;
            }
        }
    }

    public static partial class NativeMethods
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
    }
}
