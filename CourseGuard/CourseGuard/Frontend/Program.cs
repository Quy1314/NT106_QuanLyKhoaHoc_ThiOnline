using System;
using System.Windows.Forms;
using CourseGuard.Backend.Data;
using CourseGuard.Frontend.Forms.Admin;

namespace CourseGuard
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            DotNetEnv.Env.Load(".env");
            
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // Ensure default seed accounts exist (e.g. student/admin123)
            new CourseGuardDbContext("").EnsureSeedAccounts();

            System.Windows.Forms.Application.Run(new RedirectForm());
        }
    }
}
