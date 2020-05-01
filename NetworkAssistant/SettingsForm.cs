﻿using System;
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

            EthernetComboBox.Items.Clear();
            WifiComboBox.Items.Clear();

            EthernetComboBox.Items.AddRange(NetworkInterfaceDeviceSelection.AllEthernetNetworkInterfaceSelections.ToArray());
            WifiComboBox.Items.AddRange(NetworkInterfaceDeviceSelection.AllWifiNetworkInterfaceSelections.ToArray());

            ReloadOldSettings();

            if (EthernetComboBox.SelectedIndex == -1)
            {
                EthernetDoNotAutoDiscardCheckBox.Checked = false;
                EthernetDoNotAutoDiscardCheckBox.Enabled = false;
            }
            else if (((NetworkInterfaceDeviceSelection)EthernetComboBox.SelectedItem).IsActualNetworkInterface == false) {
                EthernetDoNotAutoDiscardCheckBox.Enabled = false;
            }

            if (WifiComboBox.SelectedIndex == -1)
            {
                WifiDoNotAutoDiscardCheckBox.Checked = false;
                WifiDoNotAutoDiscardCheckBox.Enabled = false;
            }
            else if (((NetworkInterfaceDeviceSelection)WifiComboBox.SelectedItem).IsActualNetworkInterface == false)
            {
                WifiDoNotAutoDiscardCheckBox.Enabled = false;
            }

            if (EthernetComboBox.SelectedIndex == -1 || WifiComboBox.SelectedIndex == -1)
            {
                this.needToInitializeSettings = true;
            }   

            ShowDialog();
            return changesDone;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (CheckIfNewSettingsAreValid())
            {
                SaveNewSettings();
                needToInitializeSettings = false;
                changesDone = true;
                Close();
            }
        }

        private void ReloadOldSettings()
        {
            if (settingsRef.EthernetInterfaceSelection != null)
            {
                EthernetComboBox.SelectedItem = settingsRef.EthernetInterfaceSelection;
                if (EthernetComboBox.SelectedIndex == -1 || EthernetComboBox.SelectedItem == null)
                {
                    throw new Exception("Previous Ethernet connection setting not found !");
                }
            }   
            
            if (settingsRef.WifiInterfaceSelection != null)
            {
                WifiComboBox.SelectedItem = settingsRef.WifiInterfaceSelection;
                if (WifiComboBox.SelectedIndex == -1 || WifiComboBox.SelectedItem == null)
                {
                    throw new Exception("Previous Wifi connection setting not found !");
                }
            }
                
            NetworkInterfaceSwitchingEnabledCheckBox.Checked = settingsRef.NetworkInterfaceSwitchingEnabled.GetValueOrDefault(false);
            StartWithWindowsCheckbox.Checked = settingsRef.AutoStartWithWindows.GetValueOrDefault(false);
            AutoEnableNetworkInterfaceSwitchingOnStartupCheckBox.Checked = settingsRef.AutoEnableSwitcherOnStartup.GetValueOrDefault(false);
            
            if (settingsRef.EthernetInterfaceSelection != null)
                EthernetDoNotAutoDiscardCheckBox.Checked = settingsRef.EthernetInterfaceSelection.DoNotAutoDiscard;
            
            if (settingsRef.WifiInterfaceSelection != null)
                WifiDoNotAutoDiscardCheckBox.Checked = settingsRef.WifiInterfaceSelection.DoNotAutoDiscard;
        }

        private void SaveNewSettings()
        {
            settingsRef.NetworkInterfaceSwitchingEnabled = NetworkInterfaceSwitchingEnabledCheckBox.Checked;
            settingsRef.AutoStartWithWindows = StartWithWindowsCheckbox.Checked;
            settingsRef.AutoEnableSwitcherOnStartup = AutoEnableNetworkInterfaceSwitchingOnStartupCheckBox.Checked;

            ((NetworkInterfaceDeviceSelection)EthernetComboBox.SelectedItem).DoNotAutoDiscard = EthernetDoNotAutoDiscardCheckBox.Checked;
            ((NetworkInterfaceDeviceSelection)WifiComboBox.SelectedItem).DoNotAutoDiscard = WifiDoNotAutoDiscardCheckBox.Checked;

            settingsRef.EthernetInterfaceSelection = (NetworkInterfaceDeviceSelection)EthernetComboBox.SelectedItem;
            settingsRef.WifiInterfaceSelection = (NetworkInterfaceDeviceSelection)WifiComboBox.SelectedItem;
        }

        private bool CheckIfNewSettingsAreValid()
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

        private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e)
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

        private void ReDetectNetworkInterfacesButton_Click(object sender, EventArgs e)
        {
            //TODO
        }

        private void EthernetComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            EthernetDoNotAutoDiscardCheckBox.Enabled = ((NetworkInterfaceDeviceSelection)EthernetComboBox.SelectedItem).IsActualNetworkInterface;
        }

        private void WifiComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            WifiDoNotAutoDiscardCheckBox.Enabled = ((NetworkInterfaceDeviceSelection)WifiComboBox.SelectedItem).IsActualNetworkInterface;
        }
    }
}
