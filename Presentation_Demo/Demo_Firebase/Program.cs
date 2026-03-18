using System;
using System.Windows.Forms;

namespace Demo_Firebase
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            // Thiết lập khởi chạy lại FormLogin
            Application.Run(new FormLogin());
        }
    }
}