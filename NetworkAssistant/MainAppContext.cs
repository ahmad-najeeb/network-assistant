using NetworkAssistantNamespace.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;

namespace NetworkAssistantNamespace
{
    public delegate int NetworkChangeDetectionHandler(object sender, EventArgs e);

    public class MainAppContext : ApplicationContext
    {
        public static string ChangeIDBeingProcessed = NullChangeID;


        public const int ChangeIDLength = 8;
        public const string NullChangeID = "--------";

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static MainAppContext AppInstance = null;

        public const string SilentExitExceptionString = "Exit";

        private NotifyIcon trayIcon;
        MenuItem enabledMenuItem;
        MenuItem currentConnectionMenuItem;
        MenuItem settingsMenuItem;
        MenuItem exitMenuItem;
        Settings settings = null;

        bool currentlyListeningForChanges = false;
        bool currentlyProcessingAChangeEvent = false;
        private static readonly object changeHandlerLock = new object();

        public MainAppContext()
        {
            AppInstance = this;
            Init();
        }

        private void Init()
        {
            Logger.Info("Initialization started");

            bool needToExitImmediately = false;

            Thread.Sleep(500);

            if (RunningAsAdministrator())
            {
                LoadSettings();
                InitializeSystemTrayMenu();
                RefreshSystemTrayMenu();
                trayIcon.Visible = true;

                if (settings.NetworkInterfaceSwitchingEnabled == true)
                {

                    TriggerManualChangeDetection();
                    RefreshSystemTrayMenu();

                    if (currentlyListeningForChanges == false)
                        StartNetworkChangeMonitoring();
                }
            }
            else
            {
                MessageBox.Show("Please run Network Assistant with administrative privileges. Exiting.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                needToExitImmediately = true;
            }

            if (needToExitImmediately)
                ExitImmediately();

            Logger.Info("Initialization done");
        }

        public void RefreshSystemTrayMenu()
        {
            Logger.Trace("{changeID} :: Refreshing system tray menu ...", ChangeIDBeingProcessed);


            enabledMenuItem.Checked = settings.NetworkInterfaceSwitchingEnabled.Value;

            if (enabledMenuItem.Checked)
            {
                currentConnectionMenuItem.Visible = true;
                var status = GetRefreshedConnectivityState().ToString();
                Logger.Trace("{changeID} :: Setting status text to: {statusText}", ChangeIDBeingProcessed, status);
                currentConnectionMenuItem.Text = "      Current: " + status;
            } else
            {
                currentConnectionMenuItem.Visible = false;
            }
        }

        public void ExitImmediately()
        {
            Logger.Info("Shutting down immediately ...");
            NLog.LogManager.Shutdown();
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
            Logger.Info("Shutting down ...");
            NLog.LogManager.Shutdown();
            if (trayIcon != null)
            {
                trayIcon.Visible = false;
            }
            Application.Exit();
        }

        void ToggleNetworkInterfaceSwitching(object sender, EventArgs e)
        {
            settings.NetworkInterfaceSwitchingEnabled = !settings.NetworkInterfaceSwitchingEnabled;
            Logger.Info("Network switching has been" + (settings.NetworkInterfaceSwitchingEnabled == true ? "ENABLED" : "DISABLED"));

            if (settings.NetworkInterfaceSwitchingEnabled.Value == true)
            {
                TriggerManualChangeDetection();
                if (currentlyListeningForChanges == false)
                    StartNetworkChangeMonitoring();
            }
            else
                StopNetworkChangeMonitoring();

            RefreshSystemTrayMenu();
        }

        void DisplaySettingsWindow(object sender, EventArgs e)
        {
            Logger.Info("Displaying Settings menu ...");
            bool changesDone = settings.ShowSettingsForm(false);

            if (changesDone && settings.NetworkInterfaceSwitchingEnabled.Value == true)
            {
                RefreshSystemTrayMenu();
            }
                
        }

        private CurrentEnabledInterface GetRefreshedConnectivityState()
        {
            Logger.Info("Calculating current connectivity state ...");
            settings.EthernetInterfaceSelection.RefreshCurrentStatus();
            settings.WifiInterfaceSelection.RefreshCurrentStatus();

            if (settings.EthernetInterfaceSelection.CurrentState > InterfaceState.EnabledButNoNetworkConnectivity
                || settings.WifiInterfaceSelection.CurrentState >= InterfaceState.EnabledButNoNetworkConnectivity)
                if (settings.EthernetInterfaceSelection.CurrentState > InterfaceState.EnabledButNoNetworkConnectivity)
                    return CurrentEnabledInterface.Ethernet;
                else
                    return CurrentEnabledInterface.WiFi;
            else
                return CurrentEnabledInterface.None;
        }

        void LoadSettings()
        {
            Logger.Info("Loading settings ...");
            if (settings == null)
                settings = Settings.GetSettingsInstance();

            settings.LoadSettings();

            Logger.Info("Settings loaded");
        }

        private void setupAutoStartWithWIndows(bool autoStartWithWindows)
        {

        }

        private void ChangeEventHandler(object sender, EventArgs e)
        {
            Logger.Trace("Received change event fire -- spawning thread");

            Thread t = new Thread(new ParameterizedThreadStart(CreateChangeRequest_Common));
            t.Start(new List<object> { true, sender, e });
        }

        /*

        private void CreateChangeRequest_EventFire(object listData)
        {   
            Logger.Trace("Received change event fire");

            
            CreateChangeRequest_Common(true, sender, e);
        }

        */

        private void CreateChangeRequest_Common(object changeData)
        {
            string localChangeID = GenerateChangeID();
            Logger.Trace("{localChangeID} :: Initiating COMMON change handling", $"{localChangeID}L");

            List<object> changeDataList = (List<object>)changeData;

            bool isChangeEventFireHandling = (bool)(changeDataList).ElementAt(0);
            object sender = changeDataList.Count >= 2 ? (changeDataList).ElementAt(1) : null;
            EventArgs e = changeDataList.Count >= 3 ? (EventArgs)(changeDataList).ElementAt(2) : null;

            Logger.Trace("Event source type: {eventSourceType}", (isChangeEventFireHandling == true ? "Event Fire" : "Monitor Enable"));
            
            if (isChangeEventFireHandling)
            {
                Logger.Trace("Event sender: {eventSender}", sender != null ? sender.ToString() : "null");
                Logger.Trace("Event details: {eventDetails}", e != null ? e.GetType().ToString() : "null");
            }

            if (!currentlyProcessingAChangeEvent)
            {
                lock (changeHandlerLock)
                {
                    currentlyProcessingAChangeEvent = true;
                    Logger.Trace("{localChangeID} :: Got lock access", $"{localChangeID}L");
                    MainAppContext.ChangeIDBeingProcessed = localChangeID;
                    Logger.Trace("{localChangeID} :: Global Change ID set", $"{localChangeID}L");
                    Logger.Trace("{localChangeID} :: Starting change handling ...", $"{localChangeID}L");
                    CheckForChangeAndPerformUpdatesIfNeeded();
                    Logger.Trace("{localChangeID} :: Change handling ended. Releasing lock ...", $"{localChangeID}L");
                    MainAppContext.ChangeIDBeingProcessed = NullChangeID;
                    currentlyProcessingAChangeEvent = false;
                }
            }
            else
            {
                Logger.Warn("{localChangeID} :: Locked out: Another change being serviced: {changeID}", $"{localChangeID}L", MainAppContext.ChangeIDBeingProcessed);
            }
        }

        private void TriggerManualChangeDetection()
        {
            Logger.Trace("Received manual change detection request -- spawning thread");

            Thread t = new Thread(new ParameterizedThreadStart(CreateChangeRequest_Common));
            t.Start(new List<object> { false });
        }

        private void CheckForChangeAndPerformUpdatesIfNeeded()
        {
            Logger.Trace("{changeID} :: (Re)loading Network Interfaces ...", ChangeIDBeingProcessed);
            NetworkInterfaceDeviceSelection.LoadAllNetworkInterfaceSelections(settings);

            Logger.Trace("{changeID} :: (Re)loading Network Interfaces done", ChangeIDBeingProcessed);
            Logger.Trace("{changeID} :: Looking for changes ...", ChangeIDBeingProcessed);

            if (settings.EthernetInterfaceSelection.CurrentState == InterfaceState.Disabled)
            {
                Logger.Trace("{changeID} :: Ethernet is disabled so enabing it ...", ChangeIDBeingProcessed);
                settings.EthernetInterfaceSelection.ChangeStateIfNeeded(InterfaceChangeNeeded.Enable);
            }

            if (settings.EthernetInterfaceSelection.CurrentState >= InterfaceState.HasNetworkConnectivity)
            {
                Logger.Trace("{changeID} :: Ethernet has connectivity so disabling Wi-Fi (if it's not already) ...", ChangeIDBeingProcessed);
                settings.WifiInterfaceSelection.ChangeStateIfNeeded(InterfaceChangeNeeded.Disable);
            }
            else
            {
                Logger.Trace("{changeID} :: Ethernet has no connectivity so enabling Wi-Fi (if it's not already) ...", ChangeIDBeingProcessed);
                settings.WifiInterfaceSelection.ChangeStateIfNeeded(InterfaceChangeNeeded.Enable);
            }

            MainAppContext.AppInstance.RefreshSystemTrayMenu(); //Do this in all cases to prevent latest Wifi data from not populating
        }

        public static bool RunningAsAdministrator()
        {
            Logger.Trace("Checking if user running program has administrative privileges ...");
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                      .IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void InitializeSystemTrayMenu()
        {
            Logger.Info("Initializing system tray menu ...");

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
        }

        public void StartNetworkChangeMonitoring()
        {
            NetworkChange.NetworkAddressChanged += new
                    NetworkAddressChangedEventHandler(ChangeEventHandler);

            currentlyListeningForChanges = true;

            Logger.Info("*** STARTED listening for network changes ***");

        }

        public void StopNetworkChangeMonitoring()
        {
            NetworkChange.NetworkAddressChanged -= new
                    NetworkAddressChangedEventHandler(ChangeEventHandler);

            currentlyListeningForChanges = false;

            Logger.Info("*** STOPPED listening for network changes ***");
        }

        private string GenerateChangeID()
        {
            Random random = new Random();
            string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, ChangeIDLength)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }

    enum CurrentEnabledInterface
    {
        None,
        Ethernet,
        WiFi
    }
}
