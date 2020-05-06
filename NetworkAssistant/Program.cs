using System;
using System.Threading;
using System.Windows.Forms;

namespace NetworkAssistantNamespace
{
    public static class Program
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Logger.Info("Application started");

            bool instanceCountOne = false;

            using (Mutex mtex = new Mutex(true, "MyRunningApp", out instanceCountOne))
            {
                if (instanceCountOne)
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new MainAppContext());
                    mtex.ReleaseMutex();
                }
                else
                {
                    MessageBox.Show("Application already running");
                }
            }
        }
    }
}
