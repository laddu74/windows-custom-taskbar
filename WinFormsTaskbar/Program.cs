using System;
using System.Windows.Forms;

namespace WinFormsTaskbar
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new TaskbarForm());
        }
    }
}