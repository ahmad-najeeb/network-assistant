using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace NetworkAssistantNamespace
{
    public partial class SettingsForm : Form
    {
        static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        bool needToInitializeSettings = false;
        bool changesDone = false;

        public SettingsForm()
        {
            InitializeComponent();
        }

        public bool ShowSettings(bool needToInitializeSettings = false)
        {
            LogThis(LogLevel.Info, "Received request to show Settings dialog ...");

            this.needToInitializeSettings = needToInitializeSettings;

            LogThis(LogLevel.Debug, "Refreshing device choices ...");

            RefreshNetworkInterfaceDeviceChoices(!this.needToInitializeSettings);

            LogThis(LogLevel.Debug, "Showing dialog ...");

            ShowDialog();

            LogThis(LogLevel.Debug, $"Dialog closed ... returning changes done: {changesDone}");

            return changesDone;
        }

        void SaveButton_Click(object sender, EventArgs e)
        {
            LogThis(LogLevel.Debug, "Save button clicked ...");

            if (CheckIfNewSettingsAreValid())
            {
                LogThis(LogLevel.Trace, "Settings are valid so saving ...");

                SaveNewSettings();
                needToInitializeSettings = false;
                changesDone = true;

                LogThis(LogLevel.Debug, "Settings saved");

                Close();
            }

            LogThis(LogLevel.Trace, "Closing Settings dialog ...");
        }

        void ReloadOldSettings()
        {
            LogThis(LogLevel.Debug, "Loading old settings for Setting dialog population ...");

            if (Global.AppSettings.EthernetInterface != null)
            {
                EthernetComboBox.SelectedItem = Global.AppSettings.EthernetInterface;
                if (EthernetComboBox.SelectedIndex == -1 || EthernetComboBox.SelectedItem == null)
                {
                    throw new Exception("Previous Ethernet connection setting not found !");
                }
            }   
            
            if (Global.AppSettings.WifiInterface != null)
            {
                WifiComboBox.SelectedItem = Global.AppSettings.WifiInterface;
                if (WifiComboBox.SelectedIndex == -1 || WifiComboBox.SelectedItem == null)
                {
                    throw new Exception("Previous Wifi connection setting not found !");
                }
            }

            LogThis(LogLevel.Trace, "Device option loading complete - loading individual options");

            if (Global.AppSettings.NetworkInterfaceSwitchingEnabled.HasValue)
                NetworkInterfaceSwitchingEnabledCheckBox.Checked = Global.AppSettings.NetworkInterfaceSwitchingEnabled.Value;
            if (Global.AppSettings.AutoStartWithWindows.HasValue)
                StartWithWindowsCheckbox.Checked = Global.AppSettings.AutoStartWithWindows.Value;
            if (Global.AppSettings.AutoEnableSwitcherOnStartup.HasValue)
                AutoEnableNetworkInterfaceSwitchingOnStartupCheckBox.Checked = Global.AppSettings.AutoEnableSwitcherOnStartup.Value;
            if (Global.AppSettings.ShowCurrentConnectionTypeInSystemTray.HasValue)
                ShowCurrentConnectionTypeInSystemTrayCheckBox.Checked = Global.AppSettings.ShowCurrentConnectionTypeInSystemTray.Value;

            if (Global.AppSettings.EthernetInterface != null)
                EthernetDoNotAutoDiscardCheckBox.Checked = Global.AppSettings.EthernetInterface.DoNotAutoDiscard.Value;
            
            if (Global.AppSettings.WifiInterface != null)
                WifiDoNotAutoDiscardCheckBox.Checked = Global.AppSettings.WifiInterface.DoNotAutoDiscard.Value;

            LogThis(LogLevel.Debug, "Reloading old settings complete");

        }

        void SaveNewSettings()
        {
            LogThis(LogLevel.Debug, "Received call to save settings ...");


            Global.AppSettings.NetworkInterfaceSwitchingEnabled = NetworkInterfaceSwitchingEnabledCheckBox.Checked;
            Global.AppSettings.AutoStartWithWindows = StartWithWindowsCheckbox.Checked;
            Global.AppSettings.AutoEnableSwitcherOnStartup = AutoEnableNetworkInterfaceSwitchingOnStartupCheckBox.Checked;
            Global.AppSettings.ShowCurrentConnectionTypeInSystemTray = ShowCurrentConnectionTypeInSystemTrayCheckBox.Checked;

            ((NetworkInterfaceDevice)EthernetComboBox.SelectedItem).DoNotAutoDiscard = EthernetDoNotAutoDiscardCheckBox.Checked;
            ((NetworkInterfaceDevice)WifiComboBox.SelectedItem).DoNotAutoDiscard = WifiDoNotAutoDiscardCheckBox.Checked;

            Global.AppSettings.EthernetInterface = (NetworkInterfaceDevice)EthernetComboBox.SelectedItem;
            Global.AppSettings.WifiInterface = (NetworkInterfaceDevice)WifiComboBox.SelectedItem;

            LogThis(LogLevel.Debug, "Save settings complete");
        }

        bool CheckIfNewSettingsAreValid()
        {
            LogThis(LogLevel.Debug, "Received call to check if new selected settings are valid");

            bool settingsValid = true; //assumption

            if (EthernetComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("You have not selected an Ethernet connection", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                settingsValid = false;
            }

            if (WifiComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("You have not selected a Wifi connection", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                settingsValid = false;
            }

            LogThis(LogLevel.Debug, $"Check done - Settings valid: {settingsValid}");

            return settingsValid;
        }

        void SettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            LogThis(LogLevel.Debug, "Settings dialog close event fired");

            if (needToInitializeSettings)
            {
                LogThis(LogLevel.Debug, $"Need to initialize settings: ${needToInitializeSettings} ...");

                LogThis(LogLevel.Debug, $"Prompting user to either choose settings or force exit ...");

                DialogResult result = MessageBox.Show(this, "Please save settings before closing, or else the program will exit. Continue ?", "Settings", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    LogThis(LogLevel.Trace, $"User chose to exit. Exiting ...");


                    this.FormClosing -= this.SettingsForm_FormClosing;
                    
                    e.Cancel = false;
                    Global.Controller.ExitImmediately();

                }
                else
                {
                    LogThis(LogLevel.Trace, $"User canceled exit request");
                    e.Cancel = true;
                }
            }
        }

        void ReDetectNetworkInterfacesButton_Click(object sender, EventArgs e)
        {
            RefreshNetworkInterfaceDeviceChoices(true);
        }

        void RefreshNetworkInterfaceDeviceChoices(bool doNetworkDeviceReload)
        {
            LogThis(LogLevel.Debug, $"Network device re-detection request received ...");

            LogThis(LogLevel.Trace, $"Re-detecting network devices ...");


            if (doNetworkDeviceReload)
                NetworkInterfaceDevice.LoadAllNetworkInterfaces();

            LogThis(LogLevel.Trace, $"Network device re-detection complete");
            LogThis(LogLevel.Trace, $"Resetting UI controls ...");


            EthernetComboBox.Items.Clear();
            WifiComboBox.Items.Clear();

            EthernetComboBox.Items.AddRange(NetworkInterfaceDevice.AllEthernetNetworkInterfaces.ToArray());
            WifiComboBox.Items.AddRange(NetworkInterfaceDevice.AllWifiNetworkInterfaces.ToArray());

            LogThis(LogLevel.Trace, $"Populating control values from current settings ...");

            ReloadOldSettings();

            if (EthernetComboBox.SelectedIndex == -1)
            {
                EthernetDoNotAutoDiscardCheckBox.Checked = false;
                EthernetDoNotAutoDiscardCheckBox.Enabled = false;
            }
            else if (((NetworkInterfaceDevice)EthernetComboBox.SelectedItem).CurrentState == InterfaceState.DevicePhysicallyDisconnected)
            {
                EthernetDoNotAutoDiscardCheckBox.Enabled = false;
            }

            if (WifiComboBox.SelectedIndex == -1)
            {
                WifiDoNotAutoDiscardCheckBox.Checked = false;
                WifiDoNotAutoDiscardCheckBox.Enabled = false;
            }
            else if (((NetworkInterfaceDevice)WifiComboBox.SelectedItem).CurrentState == InterfaceState.DevicePhysicallyDisconnected)
            {
                WifiDoNotAutoDiscardCheckBox.Enabled = false;
            }

            if (EthernetComboBox.SelectedIndex == -1 || WifiComboBox.SelectedIndex == -1)
            {
                this.needToInitializeSettings = true;
            }

            LogThis(LogLevel.Debug, $"Network device re-detection request complete");

        }

        void EthernetComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            EthernetDoNotAutoDiscardCheckBox.Enabled = ((NetworkInterfaceDevice)EthernetComboBox.SelectedItem).CurrentState > InterfaceState.DevicePhysicallyDisconnected;
        }

        void WifiComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            WifiDoNotAutoDiscardCheckBox.Enabled = ((NetworkInterfaceDevice)WifiComboBox.SelectedItem).CurrentState > InterfaceState.DevicePhysicallyDisconnected;
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            LogThis(LogLevel.Debug, $"Cancel button clicked - Closing Settings dialog ...");

            Close();
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