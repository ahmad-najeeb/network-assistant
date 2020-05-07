using System;
using System.Windows.Forms;

namespace NetworkAssistantNamespace
{
    public partial class SettingsForm : Form
    {
        Settings settingsRef;
        bool needToInitializeSettings = false;
        bool changesDone = false;

        public SettingsForm()
        {
            InitializeComponent();
        }

        public bool ShowSettings(Settings settings, bool needToInitializeSettings = false)
        {
            this.settingsRef = settings;
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
            if (settingsRef.EthernetInterface != null)
            {
                EthernetComboBox.SelectedItem = settingsRef.EthernetInterface;
                if (EthernetComboBox.SelectedIndex == -1 || EthernetComboBox.SelectedItem == null)
                {
                    throw new Exception("Previous Ethernet connection setting not found !");
                }
            }   
            
            if (settingsRef.WifiInterface != null)
            {
                WifiComboBox.SelectedItem = settingsRef.WifiInterface;
                if (WifiComboBox.SelectedIndex == -1 || WifiComboBox.SelectedItem == null)
                {
                    throw new Exception("Previous Wifi connection setting not found !");
                }
            }
            
            if (settingsRef.NetworkInterfaceSwitchingEnabled.HasValue)
                NetworkInterfaceSwitchingEnabledCheckBox.Checked = settingsRef.NetworkInterfaceSwitchingEnabled.Value;
            if (settingsRef.AutoStartWithWindows.HasValue)
                StartWithWindowsCheckbox.Checked = settingsRef.AutoStartWithWindows.Value;
            if (settingsRef.AutoEnableSwitcherOnStartup.HasValue)
            AutoEnableNetworkInterfaceSwitchingOnStartupCheckBox.Checked = settingsRef.AutoEnableSwitcherOnStartup.Value;
            
            if (settingsRef.EthernetInterface != null)
                EthernetDoNotAutoDiscardCheckBox.Checked = settingsRef.EthernetInterface.DoNotAutoDiscard.Value;
            
            if (settingsRef.WifiInterface != null)
                WifiDoNotAutoDiscardCheckBox.Checked = settingsRef.WifiInterface.DoNotAutoDiscard.Value;
        }

        void SaveNewSettings()
        {
            settingsRef.NetworkInterfaceSwitchingEnabled = NetworkInterfaceSwitchingEnabledCheckBox.Checked;
            settingsRef.AutoStartWithWindows = StartWithWindowsCheckbox.Checked;
            settingsRef.AutoEnableSwitcherOnStartup = AutoEnableNetworkInterfaceSwitchingOnStartupCheckBox.Checked;

            ((NetworkInterfaceDevice)EthernetComboBox.SelectedItem).DoNotAutoDiscard = EthernetDoNotAutoDiscardCheckBox.Checked;
            ((NetworkInterfaceDevice)WifiComboBox.SelectedItem).DoNotAutoDiscard = WifiDoNotAutoDiscardCheckBox.Checked;

            settingsRef.EthernetInterface = (NetworkInterfaceDevice)EthernetComboBox.SelectedItem;
            settingsRef.WifiInterface = (NetworkInterfaceDevice)WifiComboBox.SelectedItem;
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
                    MainAppContext.AppInstance.ExitImmediately();

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
                NetworkInterfaceDevice.LoadAllNetworkInterfaces(settingsRef);

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
    }
}