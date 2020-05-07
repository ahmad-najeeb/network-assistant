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
    public class MainAppContext : ApplicationContext
    {
        static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        static readonly object changeHandlerLock = new object();
        
        const int ChangeIDLength = 8;
        const string NullChangeID = "--------";

        bool currentlyListeningForChanges = false;
        
        NotifyIcon trayIcon;
        MenuItem enabledMenuItem;
        MenuItem currentConnectionMenuItem;
        MenuItem settingsMenuItem;
        MenuItem exitMenuItem;
        
        public MainAppContext()
        {
            Global.Controller = this;
            Init();
        }

        void Init()
        {
            Logger.Info("Initialization started");

            bool needToExitImmediately = false;
            Global.ChangeIDBeingProcessed = NullChangeID;

            Thread.Sleep(500);

            if (RunningAsAdministrator())
            {
                LoadSettings();
                InitializeSystemTrayMenu();
                RefreshSystemTrayMenu();
                trayIcon.Visible = true;

                if (Global.AppSettings.NetworkInterfaceSwitchingEnabled == true)
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
            Logger.Trace("{changeID} :: Refreshing system tray menu ...", Global.ChangeIDBeingProcessed);

            enabledMenuItem.Checked = Global.AppSettings.NetworkInterfaceSwitchingEnabled.Value;

            if (Global.AppSettings.NetworkInterfaceSwitchingEnabled.Value)
            {
                currentConnectionMenuItem.Visible = true;
                var status = GetRefreshedConnectivityState().ToString();
                Logger.Trace("{changeID} :: Setting status text to: {statusText}", Global.ChangeIDBeingProcessed, status);
                currentConnectionMenuItem.Text = "      Current: " + status;
            } else
            {
                currentConnectionMenuItem.Visible = false;
            }

            UpdateSystemTrayIconAndTooltipOnly();
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

        void Exit(object sender, EventArgs e)
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
            Global.AppSettings.NetworkInterfaceSwitchingEnabled = !Global.AppSettings.NetworkInterfaceSwitchingEnabled;
            Logger.Info("Network switching has been" + (Global.AppSettings.NetworkInterfaceSwitchingEnabled == true ? "ENABLED" : "DISABLED"));

            if (Global.AppSettings.NetworkInterfaceSwitchingEnabled.Value == true)
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
            bool changesDone = Global.AppSettings.ShowSettingsForm(false);

            if (changesDone)
            {
                RefreshSystemTrayMenu();
            }   
        }

        CurrentConnectionType GetRefreshedConnectivityState()
        {
            Logger.Info("Calculating current connectivity state ...");
            Global.AppSettings.EthernetInterface.RefreshCurrentStatus();
            Global.AppSettings.WifiInterface.RefreshCurrentStatus();

            if (Global.AppSettings.EthernetInterface.CurrentState > InterfaceState.EnabledButNoNetworkConnectivity
                || Global.AppSettings.WifiInterface.CurrentState >= InterfaceState.EnabledButNoNetworkConnectivity)
                if (Global.AppSettings.EthernetInterface.CurrentState > InterfaceState.EnabledButNoNetworkConnectivity)
                    return CurrentConnectionType.Ethernet;
                else
                    return CurrentConnectionType.WiFi;
            else
                return CurrentConnectionType.None;
        }
        
        void LoadSettings()
        {
            Logger.Info("Loading settings ...");
            if (Global.AppSettings == null)
                Settings.SetSettingsInstance();

            Global.AppSettings.LoadSettings();

            Logger.Info("Settings loaded");
        }

        void ChangeEventHandler(object sender, EventArgs e)
        {
            Logger.Trace("Received change event fire -- spawning thread");

            Thread t = new Thread(new ParameterizedThreadStart(CreateChangeRequest_Common));
            t.Start(new List<object> { true, sender, e });
        }

        void CreateChangeRequest_Common(object changeData)
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

            if (!Global.CurrentlyProcessingAChangeRequest)
            {
                lock (changeHandlerLock)
                {
                    Global.CurrentlyProcessingAChangeRequest = true;
                    Logger.Trace("{localChangeID} :: Got lock access", $"{localChangeID}L");
                    Global.ChangeIDBeingProcessed = localChangeID;
                    Logger.Trace("{localChangeID} :: Global Change ID set", $"{localChangeID}L");
                    UpdateSystemTrayIconAndTooltipOnly();
                    Logger.Trace("{localChangeID} :: Starting change handling ...", $"{localChangeID}L");
                    CheckForChangeAndPerformUpdatesIfNeeded();
                    Logger.Trace("{localChangeID} :: Change handling ended. Releasing lock ...", $"{localChangeID}L");
                    Global.ChangeIDBeingProcessed = NullChangeID;
                    Global.CurrentlyProcessingAChangeRequest = false;
                    RefreshSystemTrayMenu();
                }
            }
            else
            {
                Logger.Warn("{localChangeID} :: Locked out: Another change being serviced: {changeID}", $"{localChangeID}L", Global.ChangeIDBeingProcessed);
            }
        }

        void TriggerManualChangeDetection()
        {
            Logger.Trace("Received manual change detection request -- spawning thread");

            Thread t = new Thread(new ParameterizedThreadStart(CreateChangeRequest_Common));
            t.Start(new List<object> { false });
        }

        void CheckForChangeAndPerformUpdatesIfNeeded()
        {
            Logger.Trace("{changeID} :: (Re)loading Network Interfaces ...", Global.ChangeIDBeingProcessed);
            NetworkInterfaceDevice.LoadAllNetworkInterfaces();

            Logger.Trace("{changeID} :: (Re)loading Network Interfaces done", Global.ChangeIDBeingProcessed);
            Logger.Trace("{changeID} :: Looking for changes ...", Global.ChangeIDBeingProcessed);

            if (Global.AppSettings.EthernetInterface.CurrentState == InterfaceState.Disabled)
            {
                Logger.Trace("{changeID} :: Ethernet is disabled so enabing it ...", Global.ChangeIDBeingProcessed);
                Global.AppSettings.EthernetInterface.ChangeStateIfNeeded(InterfaceChangeNeeded.Enable);
            }

            if (Global.AppSettings.EthernetInterface.CurrentState >= InterfaceState.HasNetworkConnectivity)
            {
                Logger.Trace("{changeID} :: Ethernet has connectivity so disabling Wi-Fi (if it's not already) ...", Global.ChangeIDBeingProcessed);
                Global.AppSettings.WifiInterface.ChangeStateIfNeeded(InterfaceChangeNeeded.Disable);
            }
            else
            {
                Logger.Trace("{changeID} :: Ethernet has no connectivity so enabling Wi-Fi (if it's not already) ...", Global.ChangeIDBeingProcessed);
                Global.AppSettings.WifiInterface.ChangeStateIfNeeded(InterfaceChangeNeeded.Enable);
            }
        }

        public static bool RunningAsAdministrator()
        {
            Logger.Trace("Checking if user running program has administrative privileges ...");
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                      .IsInRole(WindowsBuiltInRole.Administrator);
        }

        void InitializeSystemTrayMenu()
        {
            Logger.Info("Initializing system tray menu ...");

            trayIcon = new NotifyIcon();

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

        void UpdateSystemTrayIconAndTooltipOnly()
        {
            CurrentConnectionType currentState = GetRefreshedConnectivityState();
            
            if (Global.AppSettings.NetworkInterfaceSwitchingEnabled.Value == true)
                if (Global.CurrentlyProcessingAChangeRequest)
                {   
                    trayIcon.Text = "Currently processing a networking change ...";

                    if (Global.AppSettings.ShowCurrentConnectionTypeInSystemTray.Value == true)
                        trayIcon.Icon = Resources.ProcessingIcon;
                    else
                        trayIcon.Icon = Resources.GenericSystemTrayIcon;
                }
                else
                {
                    trayIcon.Text = $"Currently connected to {currentState}";

                    if (Global.AppSettings.ShowCurrentConnectionTypeInSystemTray.Value == true)
                    {
                        if (currentState == CurrentConnectionType.Ethernet)
                            trayIcon.Icon = Resources.EthernetSystemTrayIcon;
                        else if (currentState == CurrentConnectionType.WiFi)
                            trayIcon.Icon = Resources.WifiSystemTrayIcon;
                        else
                            trayIcon.Icon = Resources.DisconnectedIcon;
                    }
                    else
                        trayIcon.Icon = Resources.GenericSystemTrayIcon;
                }
            else
            {
                trayIcon.Text = "Network switching is currently disabled";

                trayIcon.Icon = Resources.GenericSystemTrayIcon;
            }
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

        string GenerateChangeID()
        {
            Random random = new Random();
            string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, ChangeIDLength)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }

    enum CurrentConnectionType
    {
        None,
        Ethernet,
        WiFi
    }
}