using NetworkAssistantNamespace.Properties;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace NetworkAssistantNamespace
{
    public class MainAppContext : ApplicationContext
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        extern static bool DestroyIcon(IntPtr handle);

        static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        static readonly object changeHandlerLock = new object();
        
        const int ChangeIDLength = 8;

        const int numberOfLoadingAnimationFramesNeeded = 20;

        bool initDone = false;

        bool currentlyListeningForChanges = false;
        
        NotifyIcon trayIcon;
        MenuItem enabledMenuItem;
        MenuItem currentConnectionMenuItem;
        MenuItem settingsMenuItem;
        MenuItem exitMenuItem;
        ContextMenu contextMenu;

        Icon[] loadingIcons;
        int currentDisplayedLoadingIconIndex = -1;
        
        System.Timers.Timer loadingIconAnimationTimer;
        
        public MainAppContext()
        {
            Global.Controller = this;
            Init();
        }

        void Init()
        {
            LogThis(LogLevel.Info, "Initialization started");
            LogThis(LogLevel.Info, "Initialization started");

            Thread.Sleep(500);

            if (RunningAsAdministrator())
            {
                try
                {
                    //Create app data directory if it does not exist
                    Directory.CreateDirectory(Global.AppDataDirectory);
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Fatal error: Unable to create AppData directory: {Global.AppDataDirectory}. Exiting.\n\nException details:\n\n{e.Message}", "Fatal error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ExitImmediately();
                }

                //TODO: Measure time taken for icon generation
                //TODO: Write icons to bin output if not present
                loadingIcons = GenerateLoadingIcons(Resources.LoadingIconTemplate, numberOfLoadingAnimationFramesNeeded);

                LoadSettings();
                InitializeSystemTrayMenu();
                //RefreshSystemTrayMenu();
                trayIcon.Visible = true;
                UpdateSystemTrayIconAndTooltipOnly();

                if (Global.AppSettings.NetworkInterfaceSwitchingEnabled == true)
                {
                    TriggerManualChangeDetection();
                    //RefreshSystemTrayMenu();
                    //UpdateSystemTrayIconAndTooltipOnly();

                    if (currentlyListeningForChanges == false)
                        StartNetworkChangeMonitoring();
                }
            }
            else
            {
                MessageBox.Show("Please run Network Assistant with administrative privileges. Exiting.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                ExitImmediately();
            }   

            initDone = true;

            //RefreshSystemTrayMenu();
            trayIcon.ContextMenu = contextMenu;

            LogThis(LogLevel.Info, "Initialization done");
        }

        public void RefreshSystemTrayMenu()
        {
            LogThis(LogLevel.Trace, "Refreshing system tray menu ...");

            enabledMenuItem.Checked = Global.AppSettings.NetworkInterfaceSwitchingEnabled.Value;

            if (Global.AppSettings.NetworkInterfaceSwitchingEnabled.Value)
            {
                currentConnectionMenuItem.Visible = true;
                var status = GetRefreshedConnectivityState().ToString();
                LogThis(LogLevel.Trace, $"Setting status text to: {status}");
                currentConnectionMenuItem.Text = "      Current: " + status;
            } else
            {
                currentConnectionMenuItem.Visible = false;
            }

            UpdateSystemTrayIconAndTooltipOnly();
        }

        void DoExitRoutine(bool doImmediateExit)
        {
            if (doImmediateExit)
                LogThis(LogLevel.Info, "Shutting down immediately ...");
            else
                LogThis(LogLevel.Info, "Shutting down ...");

            if (loadingIcons != null)
                foreach (Icon loadingIcon in loadingIcons)
                    DestroyIcon(loadingIcon.Handle);

            NLog.LogManager.Shutdown();
            if (trayIcon != null)
            {
                trayIcon.Visible = false;
            }

            LogThis(LogLevel.Info, "Bye bye :) ...");

            if (doImmediateExit)
                Environment.Exit(0); //TODO: See if this can be avoided
            else
                Application.Exit();
        }

        public void ExitImmediately()
        {   
            DoExitRoutine(true);
        }

        void Exit(object sender, EventArgs e)
        {   
            DoExitRoutine(false);
        }

        void ToggleNetworkInterfaceSwitching(object sender, EventArgs e)
        {
            Global.AppSettings.NetworkInterfaceSwitchingEnabled = !Global.AppSettings.NetworkInterfaceSwitchingEnabled;
            LogThis(LogLevel.Info, "Network switching has been" + (Global.AppSettings.NetworkInterfaceSwitchingEnabled == true ? "ENABLED" : "DISABLED"));

            if (Global.AppSettings.NetworkInterfaceSwitchingEnabled.Value == true)
            {
                TriggerManualChangeDetection();
                if (currentlyListeningForChanges == false)
                    StartNetworkChangeMonitoring();
            }
            else
            {
                StopNetworkChangeMonitoring();
                RefreshSystemTrayMenu();
            }

            //RefreshSystemTrayMenu(); //This is not needed because TriggerManualChangeDetection() already executes it at the end
        }

        void DisplaySettingsWindow(object sender, EventArgs e)
        {
            LogThis(LogLevel.Info, "Displaying Settings menu ...");
            bool changesDone = Global.AppSettings.ShowSettingsForm(false);

            if (changesDone)
            {
                RefreshSystemTrayMenu();
            }   
        }

        InterfaceType GetRefreshedConnectivityState()
        {
            LogThis(LogLevel.Info, "Calculating current connectivity state ...");
            Global.AppSettings.EthernetInterface.RefreshCurrentStatus();
            Global.AppSettings.WifiInterface.RefreshCurrentStatus();

            if (Global.AppSettings.EthernetInterface.CurrentState > InterfaceState.EnabledButNoNetworkConnectivity
                || Global.AppSettings.WifiInterface.CurrentState >= InterfaceState.EnabledButNoNetworkConnectivity)
                if (Global.AppSettings.EthernetInterface.CurrentState > InterfaceState.EnabledButNoNetworkConnectivity)
                    return InterfaceType.Ethernet;
                else
                    return InterfaceType.WiFi;
            else
                return InterfaceType.None;
        }

        void ValidateNetworkDeviceChoicesAndSettings(bool doingInitialSettingsLoad = false)
        {
            NetworkInterfaceDevice.LoadAllNetworkInterfaces();

            if (doingInitialSettingsLoad
                && (NetworkInterfaceDevice.AllEthernetNetworkInterfaces.Count == 0
                || NetworkInterfaceDevice.AllWifiNetworkInterfaces.Count == 0))
            {
                MessageBox.Show("Your system doesn't have Wifi and/or Ethernet adapters. Please connect them before launching this app. Exiting.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ExitImmediately();
            }

            if (!Global.AppSettings.AreAllSettingsValidAndPresent())
            {
                if (doingInitialSettingsLoad == false)
                    MessageBox.Show("You need to re-choose one or both network interfaces");
                Global.AppSettings.ShowSettingsForm(true);
            }
        }
        
        void LoadSettings()
        {
            LogThis(LogLevel.Info, "Loading settings ...");

            bool anyPersistedConfigRepaired = false;

            Settings.LoadSettingsFromFile(ref anyPersistedConfigRepaired);

            if (anyPersistedConfigRepaired)
                Global.AppSettings.WriteSettings();

            ValidateNetworkDeviceChoicesAndSettings(true);

            LogThis(LogLevel.Info, "Settings loaded");
        }

        void ChangeEventHandler(object sender, EventArgs e)
        {
            LogThis(LogLevel.Trace, "Received change event fire -- spawning thread");

            Thread t = new Thread(new ParameterizedThreadStart(CreateChangeRequest_Common));
            t.Start(new List<object> { true, sender, e });
        }

        void CreateChangeRequest_Common(object changeData)
        {
            string localChangeID = GenerateChangeID();
            
            LogThis(LogLevel.Trace, $"Initiating COMMON change handling: {localChangeID}L");

            List<object> changeDataList = (List<object>)changeData;

            bool isChangeEventFireHandling = (bool)(changeDataList).ElementAt(0);
            object sender = changeDataList.Count >= 2 ? (changeDataList).ElementAt(1) : null;
            EventArgs e = changeDataList.Count >= 3 ? (EventArgs)(changeDataList).ElementAt(2) : null;

            LogThis(LogLevel.Trace, $"Event source type: {(isChangeEventFireHandling == true ? "Event Fire" : "Monitor Enable")}");
            
            if (isChangeEventFireHandling)
            {
                LogThis(LogLevel.Trace, $"Event sender: {(sender != null ? sender.ToString() : "null")}");
                LogThis(LogLevel.Trace, $"Event details: {(e != null ? e.GetType().ToString() : "null")}");
            }

            if (!Global.CurrentlyProcessingAChangeRequest)
            {
                lock (changeHandlerLock)
                {

                    
                    Global.CurrentlyProcessingAChangeRequest = true;
                    LogThis(LogLevel.Trace, $"Got lock access: {localChangeID}L");
                    Global.ChangeIDBeingProcessed = localChangeID;
                    GlobalDiagnosticsContext.Set(Global.LoggingVarNames.ChangeId, localChangeID);
                    LogThis(LogLevel.Trace, "Global Change ID set");
                    UpdateSystemTrayIconAndTooltipOnly();
                    LogThis(LogLevel.Trace, "Starting change handling ...");
                    CheckForChangeAndPerformUpdatesIfNeeded();
                    LogThis(LogLevel.Trace, "Change handling ended. Releasing lock ...");
                    Global.ChangeIDBeingProcessed = null;

                    GlobalDiagnosticsContext.Remove(Global.LoggingVarNames.ChangeId);

                    Global.CurrentlyProcessingAChangeRequest = false;

                    RefreshSystemTrayMenu();
                }
            }
            else
            {
                Logger.Warn("Locked out: Another change being serviced: {changeID}");
            }
        }

        void TriggerManualChangeDetection()
        {
            LogThis(LogLevel.Trace, "Received manual change detection request -- spawning thread");

            Thread t = new Thread(new ParameterizedThreadStart(CreateChangeRequest_Common));
            t.Start(new List<object> { false });
        }

        void CheckForChangeAndPerformUpdatesIfNeeded()
        {
            LogThis(LogLevel.Trace, "(Re)loading Network Interfaces ...");
            ValidateNetworkDeviceChoicesAndSettings();

            LogThis(LogLevel.Trace, "(Re)loading Network Interfaces done");
            LogThis(LogLevel.Trace, "Looking for changes ...");

            if (Global.AppSettings.EthernetInterface.CurrentState == InterfaceState.Disabled)
            {
                LogThis(LogLevel.Trace, "Ethernet is disabled so enabing it ...");
                Global.AppSettings.EthernetInterface.ChangeStateIfNeeded(InterfaceChangeNeeded.Enable);
            }

            if (Global.AppSettings.EthernetInterface.CurrentState >= InterfaceState.HasNetworkConnectivity)
            {
                LogThis(LogLevel.Trace, "Ethernet has connectivity so disabling Wi-Fi (if it's not already) ...");
                Global.AppSettings.WifiInterface.ChangeStateIfNeeded(InterfaceChangeNeeded.Disable);
            }
            else
            {
                LogThis(LogLevel.Trace, "Ethernet has no connectivity so enabling Wi-Fi (if it's not already) ...");
                Global.AppSettings.WifiInterface.ChangeStateIfNeeded(InterfaceChangeNeeded.Enable);
            }
        }

        public static bool RunningAsAdministrator()
        {
            LogThisStatic(LogLevel.Trace, "Checking if user running program has administrative privileges ...");
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                      .IsInRole(WindowsBuiltInRole.Administrator);
        }

        void InitializeSystemTrayMenu()
        {
            LogThis(LogLevel.Info, "Initializing system tray menu ...");

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

            contextMenu = new ContextMenu(new MenuItem[] {
                    enabledMenuItem,
                    currentConnectionMenuItem,
                    settingsMenuItem,
                    exitMenuItem
                    });
        }

        void UpdateSystemTrayIconAndTooltipOnly()
        {
            InterfaceType currentState = GetRefreshedConnectivityState();
            
            if (Global.AppSettings.NetworkInterfaceSwitchingEnabled.Value == true)
                if (Global.CurrentlyProcessingAChangeRequest)
                {   
                    trayIcon.Text = "Currently processing a networking change ...";

                    if (Global.AppSettings.ShowCurrentConnectionTypeInSystemTray.Value == true)
                        StartLoadingIconAnimation();
                    else
                        trayIcon.Icon = Resources.GenericSystemTrayIcon;
                }
                else
                {
                    if (initDone && loadingIconAnimationTimer.Enabled)
                        StopLoadingIconAnimation();

                    trayIcon.Text = $"Currently connected to {currentState}";

                    if (Global.AppSettings.ShowCurrentConnectionTypeInSystemTray.Value == true)
                    {
                        if (currentState == InterfaceType.Ethernet)
                            trayIcon.Icon = Resources.EthernetSystemTrayIcon;
                        else if (currentState == InterfaceType.WiFi)
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

            LogThis(LogLevel.Info, "*** STARTED listening for network changes ***");

        }

        public void StopNetworkChangeMonitoring()
        {
            NetworkChange.NetworkAddressChanged -= new
                    NetworkAddressChangedEventHandler(ChangeEventHandler);

            currentlyListeningForChanges = false;

            LogThis(LogLevel.Info, "*** STOPPED listening for network changes ***");
        }

        string GenerateChangeID()
        {
            Random random = new Random();
            string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, ChangeIDLength)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        static Icon[] GetLoadingIcons()
        {


            return null;
        }

        static Icon[] GenerateLoadingIcons(Bitmap templateLoadingImage, int numberOfNeededFrames)
        {
            if (numberOfNeededFrames < 2)
                throw new Exception("Invalid number of needed frames provided for loading icons generation: " + numberOfNeededFrames);

            Icon[] icons = new Icon[numberOfNeededFrames];

            float angleInterval = (360f / numberOfNeededFrames);

            Bitmap tmpBitmap;
            float currentAngle = 0f;
            for (int i = 0; i < numberOfNeededFrames; i++)
            {
                if (i == 0)
                    tmpBitmap = templateLoadingImage;
                else
                {
                    currentAngle += angleInterval;
                    tmpBitmap = RotateImage(templateLoadingImage, currentAngle);
                }
                icons[i] = Icon.FromHandle(tmpBitmap.GetHicon());
            }

            return icons;
        }

        static Bitmap RotateImage(Bitmap bmp, float angle)
        {
            Bitmap rotatedImage = new Bitmap(bmp.Width, bmp.Height);
            rotatedImage.SetResolution(bmp.HorizontalResolution, bmp.VerticalResolution);

            using (Graphics g = Graphics.FromImage(rotatedImage))
            {
                // Set the rotation point to the center in the matrix
                g.TranslateTransform(bmp.Width / 2, bmp.Height / 2);
                // Rotate
                g.RotateTransform(angle);
                // Restore rotation point in the matrix
                g.TranslateTransform(-bmp.Width / 2, -bmp.Height / 2);
                // Draw the image on the bitmap
                g.DrawImage(bmp, new Point(0, 0));
            }
            return rotatedImage;
        }

        void StartLoadingIconAnimation()
        {
            if (loadingIconAnimationTimer == null)
            {
                loadingIconAnimationTimer = new System.Timers.Timer(1000 / loadingIcons.Length);
                loadingIconAnimationTimer.Elapsed += LoadingIconRotationEvent;
                loadingIconAnimationTimer.AutoReset = true;
            }

            if (loadingIconAnimationTimer.Enabled == false)
            {
                loadingIconAnimationTimer.Enabled = true;
                LogThis(LogLevel.Trace, "Started ANIMATION");
            }
            else
                throw new Exception("loadingIconAnimationTimer is already enabled to why the request to enable again ?");
            
        }

        void StopLoadingIconAnimation()
        {
            if (loadingIconAnimationTimer.Enabled == true)
            {
                loadingIconAnimationTimer.Enabled = false;
                LogThis(LogLevel.Trace, "Stopped ANIMATION");
            }
            else
                throw new Exception("loadingIconAnimationTimer is already disabled to why the request to disable again ?");
        }

        void LoadingIconRotationEvent(Object source, ElapsedEventArgs e)
        {
            if (currentDisplayedLoadingIconIndex == -1)
                currentDisplayedLoadingIconIndex = 0;
            else
                currentDisplayedLoadingIconIndex = (currentDisplayedLoadingIconIndex + 1) % loadingIcons.Length;

            trayIcon.Icon = loadingIcons[currentDisplayedLoadingIconIndex];
        }

        private void LogThis(LogLevel level, string message, params KeyValuePair<string, string>[] properties)
        {
            LogThisStatic(level: level, message: message, properties: properties, callerMethodName: new StackFrame(1).GetMethod().Name);
        }

        private static void LogThisStatic(LogLevel level, string message, string callerMethodName = null, int? callDepth = null, params KeyValuePair<string, string>[] properties)
        {
            int callDepthToUse = callDepth.HasValue ? callDepth.Value : (new StackTrace()).FrameCount;

            int additionalPropertiesSize = 1;
            
            KeyValuePair<string, string>[] additionalProperties = new KeyValuePair<string, string>[additionalPropertiesSize];

            additionalProperties[0] = new KeyValuePair<string, string>(Global.LoggingVarNames.CallerMethodName, callerMethodName ?? new StackFrame(1).GetMethod().Name);
            
            Global.Log(Logger, level, message, callDepthToUse, properties, additionalProperties);
        }
    }
}