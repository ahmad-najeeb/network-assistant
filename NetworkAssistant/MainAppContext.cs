using NetworkAssistantNamespace.Properties;
using System;
using System.Net.NetworkInformation;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;

namespace NetworkAssistantNamespace
{
    public delegate int NetworkChangeDetectionHandler(object sender, EventArgs e);

    public class MainAppContext : ApplicationContext
    {
        public static MainAppContext AppInstance = null;

        public const string SilentExitExceptionString = "Exit";

        private NotifyIcon trayIcon;
        MenuItem enabledMenuItem;
        MenuItem currentConnectionMenuItem;
        MenuItem settingsMenuItem;
        MenuItem exitMenuItem;
        Settings settings = null;

        bool currentlyListeningForChanges = false;

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
                RefreshSystemTrayMenu();

                if (settings.NetworkInterfaceSwitchingEnabled == true)
                {
                    CheckForChangeAndPerformUpdatesIfNeeded(this, EventArgs.Empty);
                    RefreshSystemTrayMenu();
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

        public void RefreshSystemTrayMenu()
        {
            enabledMenuItem.Checked = settings.NetworkInterfaceSwitchingEnabled.Value;

            if (enabledMenuItem.Checked)
            {
                currentConnectionMenuItem.Visible = true;
                currentConnectionMenuItem.Text = "      Current: " + GetRefreshedConnectivityState().ToString();
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
            Environment.Exit(0); //TODO: See if this can be avoided
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
            if (settings.NetworkInterfaceSwitchingEnabled.Value == true)
            {
                CheckForChangeAndPerformUpdatesIfNeeded(this, EventArgs.Empty);
                if (currentlyListeningForChanges == false)
                    StartNetworkChangeMonitoring();
            }
            else
                StopNetworkChangeMonitoring();

            RefreshSystemTrayMenu();
        }

        void DisplaySettingsWindow(object sender, EventArgs e)
        {
            bool changesDone = settings.ShowSettingsForm(false);

            if (changesDone && settings.NetworkInterfaceSwitchingEnabled.Value == true)
            {
                RefreshSystemTrayMenu();
            }
                
        }

        private CurrentEnabledInterface GetRefreshedConnectivityState()
        {
            settings.EthernetInterfaceSelection.RefreshCurrentStatus();
            settings.WifiInterfaceSelection.RefreshCurrentStatus();

            if (settings.EthernetInterfaceSelection.CurrentState > InterfaceState.EnabledButNoNetworkConnectivity
                || settings.WifiInterfaceSelection.CurrentState > InterfaceState.EnabledButNoNetworkConnectivity)
                if (settings.EthernetInterfaceSelection.CurrentState > InterfaceState.EnabledButNoNetworkConnectivity)
                    return CurrentEnabledInterface.Ethernet;
                else
                    return CurrentEnabledInterface.WIfi;
            else
                return CurrentEnabledInterface.None;
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

            if (settings.EthernetInterfaceSelection.CurrentState == InterfaceState.Disabled)
                settings.EthernetInterfaceSelection.ChangeStateIfNeeded(InterfaceChangeNeeded.Enable);


            if (settings.EthernetInterfaceSelection.CurrentState >= InterfaceState.HasNetworkConnectivity)
            {
                settings.WifiInterfaceSelection.ChangeStateIfNeeded(InterfaceChangeNeeded.Disable);
            }
            else
            {
                settings.WifiInterfaceSelection.ChangeStateIfNeeded(InterfaceChangeNeeded.Enable);
            }
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

        public void StartNetworkChangeMonitoring()
        {
            NetworkChange.NetworkAddressChanged += new
                    NetworkAddressChangedEventHandler(CheckForChangeAndPerformUpdatesIfNeeded);

            currentlyListeningForChanges = true;
        }

        public void StopNetworkChangeMonitoring()
        {
            NetworkChange.NetworkAddressChanged -= new
                    NetworkAddressChangedEventHandler(CheckForChangeAndPerformUpdatesIfNeeded);

            currentlyListeningForChanges = false;
        }
    }

    enum CurrentEnabledInterface
    {
        None,
        Ethernet,
        WIfi
    }
}
