using EthernetWifiSwitch.Properties;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EthernetWifiSwitch
{
    public class EthernetWifiSwitchApp : ApplicationContext
    {
        public static EthernetWifiSwitchApp AppInstance = null;

        public const string SilentExitExceptionString = "Exit";

        private bool? currentlyUsingEthernet = null;
        
        private NotifyIcon trayIcon;
        MenuItem enabledMenuItem;
        MenuItem currentConnectionMenuItem;
        MenuItem settingsMenuItem;
        MenuItem exitMenuItem;
        Settings settings = null;

        public EthernetWifiSwitchApp()
        {
            AppInstance = this;
            _ = Init();
        }

        private async Task Init()
        {
            Thread.Sleep(1000);

            LoadSettings();

            trayIcon = new NotifyIcon();
            trayIcon.Icon = Resources.SysTrayIcon;

            if (settings.NetworkInterfaceSwitchingEnabled.GetValueOrDefault(false))
                CheckForChange(this, EventArgs.Empty);

            RefreshSysTrayMenu();
        }

        private void RefreshSysTrayMenu()
        {
            if (enabledMenuItem == null)
                enabledMenuItem = new MenuItem("Enabled", ToggleEnabled);

            if (settingsMenuItem == null)
                settingsMenuItem = new MenuItem("Settings", DisplaySettings);

            if (currentConnectionMenuItem == null)
            {
                currentConnectionMenuItem = new MenuItem();
                currentConnectionMenuItem.Enabled = false;
            }

            if (exitMenuItem == null)
                exitMenuItem = new MenuItem("Exit", Exit);

            enabledMenuItem.Checked = settings.NetworkInterfaceSwitchingEnabled.GetValueOrDefault(false);

            if (enabledMenuItem.Checked)
            {
                currentConnectionMenuItem.Visible = true;
                currentConnectionMenuItem.Text = "      Current: " + "Wifi";
            } else
            {
                currentConnectionMenuItem.Visible = false;
            }

            trayIcon.Visible = true;

            trayIcon.ContextMenu = new ContextMenu(new MenuItem[] {
                    enabledMenuItem,
                    currentConnectionMenuItem,
                    settingsMenuItem,
                    exitMenuItem
                    });
        }

        

        public void Exit()
        {
            Exit(this, EventArgs.Empty);
        }

        private void Exit(object sender, EventArgs e)
        {
            // Hide tray icon, otherwise it will remain shown until user mouses over it
            if (trayIcon != null)
                trayIcon.Visible = false;
            Application.Exit();
        }

        void ToggleEnabled(object sender, EventArgs e)
        {
            settings.NetworkInterfaceSwitchingEnabled = !settings.NetworkInterfaceSwitchingEnabled;
            RefreshSysTrayMenu();
        }

        void DisplaySettings(object sender, EventArgs e)
        {
            bool changesDone = settings.ShowSettingsForm(false);

            if (changesDone)
                RefreshSysTrayMenu();
        }

        void LoadSettings()
        {
            if (settings == null)
                settings = Settings.GetSettingsInstance();

            settings.LoadSettings();
        }

        private void setupAutoStartWithWIndows(bool autoStartWithWindows)
        {

        }

        private void CheckForChange(object sender, EventArgs e)
        {

        }

        private void PerformNetworkInterfaceSwitch()
        {

        }
    }
}
