using System;
using System.Threading;
using System.Windows.Forms;

namespace EthernetWifiSwitch
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
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
