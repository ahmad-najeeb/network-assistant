using EthernetWifiSwitch.Properties;
using System;
using System.Net.NetworkInformation;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EthernetWifiSwitch
{
    public class EthernetWifiSwitchApp : ApplicationContext
    {
        public static EthernetWifiSwitchApp AppInstance = null;

        public const string SilentExitExceptionString = "Exit";

        private bool? currentlyOnEthernet = null;
        
        private NotifyIcon trayIcon;
        MenuItem enabledMenuItem;
        MenuItem currentConnectionMenuItem;
        MenuItem settingsMenuItem;
        MenuItem exitMenuItem;
        Settings settings = null;

        public EthernetWifiSwitchApp()
        {
            AppInstance = this;
            Init();
        }

        private void Init()
        {
            Thread.Sleep(500);

            if (RunningAsAdministrator())
            {
                LoadSettings();

                trayIcon = new NotifyIcon();
                trayIcon.Icon = Resources.SysTrayIcon;

                if (settings.NetworkInterfaceSwitchingEnabled == true)
                {
                    CheckForChange(this, EventArgs.Empty);
                    
                    /*
                    
                    //Set current status:
                    currentlyOnEthernet = NetworkInterfaceDeviceSelection.IsOnline(settings.EthernetInterfaceSelection);

                    //Disable Wifi if it's enabled
                    if (currentlyOnEthernet.Value == true && NetworkInterfaceDeviceSelection.IsOnline(settings.WifiInterfaceSelection))
                        settings.WifiInterfaceSelection.ChangeState(false);

                    NetworkChange.NetworkAddressChanged += new
                        NetworkAddressChangedEventHandler(CheckForChange);

                    */
                }


                NetworkChange.NetworkAddressChanged += new
                    NetworkAddressChangedEventHandler(CheckForChange);
            }
            else
            {
                MessageBox.Show("Please run Network Assistant with administrative privileges. Exiting.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                Exit();
            }
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
            {
                trayIcon.Visible = false;
            }
            Application.Exit();
        }

        void ToggleEnabled(object sender, EventArgs e)
        {
            settings.NetworkInterfaceSwitchingEnabled = !settings.NetworkInterfaceSwitchingEnabled;
            CheckForChange(this, EventArgs.Empty);
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
            NetworkInterfaceDeviceSelection.LoadAllNetworkInterfaceSelections(settings);

            if (NetworkInterfaceDeviceSelection.IsOnline(settings.EthernetInterfaceSelection))
            {
                NetworkChange.NetworkAddressChanged -= new
                    NetworkAddressChangedEventHandler(CheckForChange);

                settings.WifiInterfaceSelection.ChangeState(false);

                NetworkChange.NetworkAddressChanged += new
                    NetworkAddressChangedEventHandler(CheckForChange);

                currentlyOnEthernet = true;

                RefreshSysTrayMenu();
            }
            else
            {
                currentlyOnEthernet = false;

                if (!NetworkInterfaceDeviceSelection.IsOnline(settings.WifiInterfaceSelection))
                {
                    NetworkChange.NetworkAddressChanged -= new
                    NetworkAddressChangedEventHandler(CheckForChange);

                    settings.WifiInterfaceSelection.ChangeState(true);

                    NetworkChange.NetworkAddressChanged += new
                    NetworkAddressChangedEventHandler(CheckForChange);
                }
            }

            if (currentlyOnEthernet.Value == true && !NetworkInterfaceDeviceSelection.IsOnline(settings.EthernetInterfaceSelection)) 
            {
                NetworkChange.NetworkAddressChanged -= new
                    NetworkAddressChangedEventHandler(CheckForChange);

                settings.WifiInterfaceSelection.ChangeState(true);

                NetworkChange.NetworkAddressChanged += new
                    NetworkAddressChangedEventHandler(CheckForChange);

                currentlyOnEthernet = false;

                RefreshSysTrayMenu();
            }
            
        }

        public static bool RunningAsAdministrator()
        {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                      .IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
