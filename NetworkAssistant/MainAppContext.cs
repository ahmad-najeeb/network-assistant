using NetworkAssistantNamespace.Properties;
using System;
using System.Net.NetworkInformation;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;

namespace NetworkAssistantNamespace
{   public class MainAppContext : ApplicationContext
    {
        public static MainAppContext AppInstance = null;

        public const string SilentExitExceptionString = "Exit";

        private bool? currentlyOnEthernet = null;
        
        private NotifyIcon trayIcon;
        MenuItem enabledMenuItem;
        MenuItem currentConnectionMenuItem;
        MenuItem settingsMenuItem;
        MenuItem exitMenuItem;
        Settings settings = null;

        public MainAppContext()
        {
            AppInstance = this;
            Init();
        }

        private void Init()
        {
            bool needToExitImmediately = false;

            Thread.Sleep(500);

            if (RunningAsAdministrator())
            {
                LoadSettings();
                InitializeSystemTrayMenu();

                if (settings.NetworkInterfaceSwitchingEnabled == true)
                {
                    CheckForChangeAndPerformUpdatesIfNeeded(this, EventArgs.Empty);
                }

                StartNetworkChangeMonitoring();
            }
            else
            {
                MessageBox.Show("Please run Network Assistant with administrative privileges. Exiting.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                needToExitImmediately = true;
            }

            if (needToExitImmediately)
                ExitImmediately();
        }

        private void RefreshSystemTrayMenu()
        {
            enabledMenuItem.Checked = settings.NetworkInterfaceSwitchingEnabled.Value;

            if (enabledMenuItem.Checked)
            {
                currentConnectionMenuItem.Visible = true;
                currentConnectionMenuItem.Text = "      Current: " + (currentlyOnEthernet.Value ? "Ethernet" : "Wifi");
            } else
            {
                currentConnectionMenuItem.Visible = false;
            }
        }

        public void ExitImmediately()
        {
            if (trayIcon != null)
            {
                trayIcon.Visible = false;
            }
            Environment.Exit(0);
        }

        public void Exit()
        {
            Exit(this, EventArgs.Empty);
        }

        private void Exit(object sender, EventArgs e)
        {
            if (trayIcon != null)
            {
                trayIcon.Visible = false;
            }
            Application.Exit();
        }

        void ToggleNetworkInterfaceSwitching(object sender, EventArgs e)
        {
            settings.NetworkInterfaceSwitchingEnabled = !settings.NetworkInterfaceSwitchingEnabled;
            CheckForChangeAndPerformUpdatesIfNeeded(this, EventArgs.Empty);
            RefreshSystemTrayMenu();
        }

        void DisplaySettingsWindow(object sender, EventArgs e)
        {
            bool changesDone = settings.ShowSettingsForm(false);

            if (changesDone)
                RefreshSystemTrayMenu();
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

        private void CheckForChangeAndPerformUpdatesIfNeeded(object sender, EventArgs e)
        {
            NetworkInterfaceDeviceSelection.LoadAllNetworkInterfaceSelections(settings);

            if (NetworkInterfaceDeviceSelection.IsOnline(settings.EthernetInterfaceSelection))
            {
                MakeInterfaceChange(settings.WifiInterfaceSelection, InterfaceChangeNeeded.Disable);
                currentlyOnEthernet = true;
            }
            else
            {
                currentlyOnEthernet = false;

                if (!NetworkInterfaceDeviceSelection.IsOnline(settings.WifiInterfaceSelection))
                {
                    MakeInterfaceChange(settings.WifiInterfaceSelection, InterfaceChangeNeeded.Enable);
                }
            }

            if (currentlyOnEthernet.Value == true && !NetworkInterfaceDeviceSelection.IsOnline(settings.EthernetInterfaceSelection)) 
            {
                MakeInterfaceChange(settings.WifiInterfaceSelection, InterfaceChangeNeeded.Enable);
                currentlyOnEthernet = false;
            }
        }

        private void MakeInterfaceChange(NetworkInterfaceDeviceSelection deviceSelection, InterfaceChangeNeeded changeNeeded)
        {
            StopNetworkChangeMonitoring();

            deviceSelection.ChangeState(changeNeeded);

            StartNetworkChangeMonitoring();
        }

        public static bool RunningAsAdministrator()
        {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                      .IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void InitializeSystemTrayMenu()
        {
            trayIcon = new NotifyIcon();
            trayIcon.Icon = Resources.SysTrayIcon;

            if (enabledMenuItem == null)
                enabledMenuItem = new MenuItem("Enabled", ToggleNetworkInterfaceSwitching);

            if (settingsMenuItem == null)
                settingsMenuItem = new MenuItem("Settings", DisplaySettingsWindow);

            if (currentConnectionMenuItem == null)
            {
                currentConnectionMenuItem = new MenuItem();
                currentConnectionMenuItem.Enabled = false;
            }

            if (exitMenuItem == null)
                exitMenuItem = new MenuItem("Exit", Exit);

            trayIcon.ContextMenu = new ContextMenu(new MenuItem[] {
                    enabledMenuItem,
                    currentConnectionMenuItem,
                    settingsMenuItem,
                    exitMenuItem
                    });

            RefreshSystemTrayMenu();

            trayIcon.Visible = true;
        }

        private void StartNetworkChangeMonitoring()
        {
            NetworkChange.NetworkAddressChanged += new
                    NetworkAddressChangedEventHandler(CheckForChangeAndPerformUpdatesIfNeeded);
        }

        private void StopNetworkChangeMonitoring()
        {
            NetworkChange.NetworkAddressChanged -= new
                    NetworkAddressChangedEventHandler(CheckForChangeAndPerformUpdatesIfNeeded);
        }
    }
}
