using System;
using System.Windows;
using System.Windows.Threading;
using System.Diagnostics;
using CustomTaskbar.Interop;

namespace CustomTaskbar
{
    public partial class MainWindow : Window
    {
        private AppBarManager _appBarManager;
        private DispatcherTimer _clockTimer;

        public MainWindow()
        {
            InitializeComponent();
            // Temporarily disable AppBar logic to test raw window visibility
            // _appBarManager = new AppBarManager(this);

            // Initialize Clock
            _clockTimer = new DispatcherTimer();
            _clockTimer.Interval = TimeSpan.FromSeconds(1);
            _clockTimer.Tick += ClockTimer_Tick;
            _clockTimer.Start();
            UpdateClock();
        }

        private void ClockTimer_Tick(object sender, EventArgs e)
        {
            UpdateClock();
        }

        private void UpdateClock()
        {
            ClockText.Text = DateTime.Now.ToString("h:mm tt\nM/d/yyyy");
        }

        protected override void OnClosed(EventArgs e)
        {
            _appBarManager.Dispose();
            base.OnClosed(e);
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            // Simulate Windows Key to open Start Menu
            NativeMethods.keybd_event(NativeMethods.VK_LWIN, 0, 0, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_LWIN, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        private void AppButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            if (button != null)
            {
                string app = button.Content?.ToString() ?? "";
                try
                {
                    switch (app)
                    {
                        case "Explorer":
                            Process.Start("explorer.exe");
                            break;
                        case "Browser":
                            Process.Start(new ProcessStartInfo("https://google.com") { UseShellExecute = true });
                            break;
                        case "Terminal":
                            Process.Start(new ProcessStartInfo("cmd.exe") { UseShellExecute = true });
                            break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error launching {app}: {ex.Message}");
                }
            }
        }
    }
}