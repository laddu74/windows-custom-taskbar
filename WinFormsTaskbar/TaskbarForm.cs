using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace WinFormsTaskbar
{
    public partial class TaskbarForm : Form
    {
        private int _callbackMessageId;
        private bool _isRegistered;
        private int _originalTaskbarState;
        private int _originalTaskbarLockState;
        private Label clockLabel;
        private Button startBtn;
        private Button expBtn;
        private Button webBtn;

        public TaskbarForm()
        {
            // Boilerplate styling for a modern edge-to-edge look without borders
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.BackColor = Color.FromArgb(31, 31, 31); // Dark aesthetic
            this.Height = 56;
            this.StartPosition = FormStartPosition.Manual;

            InitializeUI();
        }

        private void InitializeUI()
        {
            // Start Button
            startBtn = new Button
            {
                Text = "START",
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 120, 215), // Windows Blue
                FlatStyle = FlatStyle.Flat,
                Size = new Size(80, 40),
                Location = new Point(10, 8)
            };
            startBtn.FlatAppearance.BorderSize = 0;
            startBtn.Click += (s, e) => 
            {
                NativeMethods.keybd_event(NativeMethods.VK_LWIN, 0, 0, UIntPtr.Zero);
                NativeMethods.keybd_event(NativeMethods.VK_LWIN, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            };

            // Apps
            expBtn = new Button
            {
                Text = "Explorer",
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(100, 40),
                Location = new Point(110, 8)
            };
            expBtn.FlatAppearance.BorderSize = 0;
            expBtn.Click += (s, e) => Process.Start("explorer.exe");

            webBtn = new Button
            {
                Text = "Browser",
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(100, 40),
                Location = new Point(220, 8)
            };
            webBtn.FlatAppearance.BorderSize = 0;
            webBtn.Click += (s, e) => Process.Start(new ProcessStartInfo("https://google.com") { UseShellExecute = true });

            // Clock
            clockLabel = new Label
            {
                Text = DateTime.Now.ToString("h:mm tt"),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(this.Width - 100, 16),
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };

            var timer = new System.Windows.Forms.Timer { Interval = 1000 };
            timer.Tick += (s, e) => clockLabel.Text = DateTime.Now.ToString("h:mm tt");
            timer.Start();

            var hideTimer = new System.Windows.Forms.Timer { Interval = 100 };
            hideTimer.Tick += (s, e) => HideWindowsTaskbar();
            hideTimer.Start();

            this.Controls.Add(startBtn);
            this.Controls.Add(expBtn);
            this.Controls.Add(webBtn);
            this.Controls.Add(clockLabel);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            // Layout to screen width
            this.Width = Screen.PrimaryScreen.Bounds.Width;
            clockLabel.Location = new Point(this.Width - 100, 16);

            RegisterBar();
        }

        private void RegisterBar()
        {
            ApplyDefaultSettings();
            HideWindowsTaskbar();

            NativeMethods.APPBARDATA abd = new NativeMethods.APPBARDATA();
            abd.cbSize = Marshal.SizeOf(abd);
            abd.hWnd = this.Handle;

            _callbackMessageId = NativeMethods.RegisterWindowMessage("AppBarMessage");
            abd.uCallbackMessage = _callbackMessageId;

            NativeMethods.SHAppBarMessage(NativeMethods.ABM_NEW, ref abd);
            _isRegistered = true;

            ABSetPos();
        }

        private void ABSetPos()
        {
            NativeMethods.APPBARDATA abd = new NativeMethods.APPBARDATA();
            abd.cbSize = Marshal.SizeOf(abd);
            abd.hWnd = this.Handle;
            abd.uEdge = NativeMethods.ABE_BOTTOM;

            abd.rc.left = 0;
            abd.rc.right = Screen.PrimaryScreen.Bounds.Width;
            abd.rc.top = Screen.PrimaryScreen.Bounds.Height - this.Height;
            abd.rc.bottom = Screen.PrimaryScreen.Bounds.Height;

            NativeMethods.SHAppBarMessage(NativeMethods.ABM_QUERYPOS, ref abd);
            NativeMethods.SHAppBarMessage(NativeMethods.ABM_SETPOS, ref abd);

            this.Location = new Point(abd.rc.left, abd.rc.top);
            this.Size = new Size(abd.rc.right - abd.rc.left, abd.rc.bottom - abd.rc.top);

            NativeMethods.SetWindowPos(this.Handle, NativeMethods.HWND_TOPMOST, 
                0, 0, 0, 0, 
                NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOACTIVATE);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == _callbackMessageId)
            {
                // Appbar callback (re-calculate on resolution change or taskbar toggle)
                if (m.WParam.ToInt32() == 0x0000009 /* ABM_WINDOWPOSCHANGED */)
                {
                    ABSetPos();
                }
            }
            base.WndProc(ref m);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            ShowWindowsTaskbar();
            RestoreDefaultSettings();

            if (_isRegistered)
            {
                NativeMethods.APPBARDATA abd = new NativeMethods.APPBARDATA();
                abd.cbSize = Marshal.SizeOf(abd);
                abd.hWnd = this.Handle;
                NativeMethods.SHAppBarMessage(NativeMethods.ABM_REMOVE, ref abd);
            }
            
            base.OnFormClosed(e);
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

        private void ApplyDefaultSettings()
        {
            IntPtr hwnd = NativeMethods.FindWindow("Shell_TrayWnd", null);
            if (hwnd != IntPtr.Zero)
            {
                NativeMethods.APPBARDATA abd = new NativeMethods.APPBARDATA();
                abd.cbSize = Marshal.SizeOf(abd);
                abd.hWnd = hwnd;

                // 1. Get original auto-hide state and force our new state.
                _originalTaskbarState = (int)NativeMethods.SHAppBarMessage(NativeMethods.ABM_GETSTATE, ref abd);
                abd.lParam = (IntPtr)(NativeMethods.ABS_AUTOHIDE | NativeMethods.ABS_ALWAYSONTOP);
                NativeMethods.SHAppBarMessage(NativeMethods.ABM_SETSTATE, ref abd);
            }

            // 2. Disable locking the taskbar in registry
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced\TaskbarSizeMove", true))
                {
                    if (key != null)
                    {
                        object val = key.GetValue("");
                        _originalTaskbarLockState = val != null ? (int)val : 0;
                        key.SetValue("", 0, RegistryValueKind.DWord);
                    }
                }
            }
            catch { /* Ignore registry permission errors if any */ }
        }

        private void RestoreDefaultSettings()
        {
            IntPtr hwnd = NativeMethods.FindWindow("Shell_TrayWnd", null);
            if (hwnd != IntPtr.Zero)
            {
                NativeMethods.APPBARDATA abd = new NativeMethods.APPBARDATA();
                abd.cbSize = Marshal.SizeOf(abd);
                abd.hWnd = hwnd;
                abd.lParam = (IntPtr)_originalTaskbarState;
                NativeMethods.SHAppBarMessage(NativeMethods.ABM_SETSTATE, ref abd);
            }

            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced\TaskbarSizeMove", true))
                {
                    if (key != null)
                    {
                        key.SetValue("", _originalTaskbarLockState, RegistryValueKind.DWord);
                    }
                }
            }
            catch { /* Ignore */ }
        }
    }
}
