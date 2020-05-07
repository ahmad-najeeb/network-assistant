using System;
using System.Windows.Forms;

namespace NetworkAssistantNamespace
{
    public partial class SettingsForm : Form
    {
        bool needToInitializeSettings = false;
        bool changesDone = false;

        public SettingsForm()
        {
            InitializeComponent();
        }

        public bool ShowSettings(bool needToInitializeSettings = false)
        {
            this.needToInitializeSettings = needToInitializeSettings;

            RefreshNetworkInterfaceDeviceChoices(!this.needToInitializeSettings);

            ShowDialog();
            return changesDone;
        }

        void SaveButton_Click(object sender, EventArgs e)
        {
            if (CheckIfNewSettingsAreValid())
            {
                SaveNewSettings();
                needToInitializeSettings = false;
                changesDone = true;
                Close();
            }
        }

        void ReloadOldSettings()
        {
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
        }

        void SaveNewSettings()
        {
            Global.AppSettings.NetworkInterfaceSwitchingEnabled = NetworkInterfaceSwitchingEnabledCheckBox.Checked;
            Global.AppSettings.AutoStartWithWindows = StartWithWindowsCheckbox.Checked;
            Global.AppSettings.AutoEnableSwitcherOnStartup = AutoEnableNetworkInterfaceSwitchingOnStartupCheckBox.Checked;
            Global.AppSettings.ShowCurrentConnectionTypeInSystemTray = ShowCurrentConnectionTypeInSystemTrayCheckBox.Checked;

            ((NetworkInterfaceDevice)EthernetComboBox.SelectedItem).DoNotAutoDiscard = EthernetDoNotAutoDiscardCheckBox.Checked;
            ((NetworkInterfaceDevice)WifiComboBox.SelectedItem).DoNotAutoDiscard = WifiDoNotAutoDiscardCheckBox.Checked;

            Global.AppSettings.EthernetInterface = (NetworkInterfaceDevice)EthernetComboBox.SelectedItem;
            Global.AppSettings.WifiInterface = (NetworkInterfaceDevice)WifiComboBox.SelectedItem;
        }

        bool CheckIfNewSettingsAreValid()
        {
            if (EthernetComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("You have not selected an Ethernet connection", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }

            if (WifiComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("You have not selected a Wifi connection", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }

            return true;
        }

        void SettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(needToInitializeSettings)
            {
                DialogResult result = MessageBox.Show(this, "Please save settings before closing, or else the program will exit. Continue ?", "Settings", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    this.FormClosing -= this.SettingsForm_FormClosing;
                    
                    e.Cancel = false;
                    Global.Controller.ExitImmediately();

                } else
                {
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
            if (doNetworkDeviceReload)
                NetworkInterfaceDevice.LoadAllNetworkInterfaces();

            EthernetComboBox.Items.Clear();
            WifiComboBox.Items.Clear();

            EthernetComboBox.Items.AddRange(NetworkInterfaceDevice.AllEthernetNetworkInterfaces.ToArray());
            WifiComboBox.Items.AddRange(NetworkInterfaceDevice.AllWifiNetworkInterfaces.ToArray());

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
            Close();
        }
    }
}