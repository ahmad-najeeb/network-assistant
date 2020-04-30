using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows.Forms;

namespace EthernetWifiSwitch
{
    public partial class SettingsForm : Form
    {
        Settings settingsRef;
        bool needToInitializeSettings = false;
        bool changesDone = false;

        //List<NetworkInterfaceDeviceSelection> ethernetAdapterSelections;
        //List<NetworkInterfaceDeviceSelection> wifiAdapterSelections;

        public SettingsForm()
        {
            InitializeComponent();
        }

        public bool ShowSettings(Settings settings, bool needToInitializeSettings = false)
        {
            this.settingsRef = settings;
            this.needToInitializeSettings = needToInitializeSettings;

            //GetAllNetworkInterfaceSelections();

            ethernetComboBox.Items.Clear();
            wifiComboBox.Items.Clear();

            ethernetComboBox.Items.AddRange(NetworkInterfaceDeviceSelection.AllEthernetNetworkInterfaceSelections.ToArray());
            wifiComboBox.Items.AddRange(NetworkInterfaceDeviceSelection.AllWifiNetworkInterfaceSelections.ToArray());

            ReloadOldSettings();

            if (ethernetComboBox.SelectedIndex == -1)
            {
                ethernetDoNotAutoDiscardCheckBox.Checked = false;
                ethernetDoNotAutoDiscardCheckBox.Enabled = false;
            }
            else if (((NetworkInterfaceDeviceSelection)ethernetComboBox.SelectedItem).IsActualNetworkInterface == false) {
                ethernetDoNotAutoDiscardCheckBox.Enabled = false;
            }

            if (wifiComboBox.SelectedIndex == -1)
            {
                wifiDoNotAutoDiscardCheckBox.Checked = false;
                wifiDoNotAutoDiscardCheckBox.Enabled = false;
            }
            else if (((NetworkInterfaceDeviceSelection)wifiComboBox.SelectedItem).IsActualNetworkInterface == false)
            {
                wifiDoNotAutoDiscardCheckBox.Enabled = false;
            }

            if (ethernetComboBox.SelectedIndex == -1 || wifiComboBox.SelectedIndex == -1)
            {
                this.needToInitializeSettings = true;
            }
                

            ShowDialog();

            return changesDone;
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (CheckIfNewSettingsAreValid())
            {
                SaveNewSettings();
                needToInitializeSettings = false;
                changesDone = true;
                Close();
            }
        }

        /*

        private void GetAllNetworkInterfaceSelections()
        {
            ethernetComboBox.Items.Clear();
            wifiComboBox.Items.Clear();
            //NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();

            ethernetAdapterSelections = NetworkInterfaceDeviceSelection.GetAttachedEthernetNetworkInterfaceSelections(settingsRef.EthernetInterfaceSelection);
            wifiAdapterSelections = NetworkInterfaceDeviceSelection.GetAttachedWifiNetworkInterfaceSelections(settingsRef.WifiInterfaceSelection);


            //ethernetComboBox.Items.AddRange(ethernetAdapterSelections.Select(x => GetNICString(x)).ToArray());
            //wifiComboBox.Items.AddRange(wifiAdapterSelections.Select(x => GetNICString(x)).ToArray());

        }

        */

        private void ReloadOldSettings()
        {
            if (settingsRef.EthernetInterfaceSelection != null)
            {
                ethernetComboBox.SelectedItem = settingsRef.EthernetInterfaceSelection;
                if (ethernetComboBox.SelectedIndex == -1 || ethernetComboBox.SelectedItem == null)
                {
                    throw new Exception("Previous Ethernet connection setting not found !");
                }
            }   
            
            if (settingsRef.WifiInterfaceSelection != null)
            {
                wifiComboBox.SelectedItem = settingsRef.WifiInterfaceSelection;
                if (wifiComboBox.SelectedIndex == -1 || wifiComboBox.SelectedItem == null)
                {
                    throw new Exception("Previous Wifi connection setting not found !");
                }
            }
                


            /*

            for (int i = 0; i < ethernetAdapterSelections.Length; i++)
            {
                if (ethernetAdapterSelections[i] == settingsRef.EthernetInterfaceSelection)
                {
                    ethernetComboBox.SelectedIndex = i;
                    break;
                }
            }

            for (int i = 0; i < wifiAdapterSelections.Length; i++)
            {
                if (wifiAdapterSelections[i] == settingsRef.WifiInterfaceSelection)
                {
                    wifiComboBox.SelectedIndex = i;
                    break;
                }
            }

            if (ethernetComboBox.SelectedIndex == -1)
            {
                ethernetComboBox.Items.Add(settingsRef.EthernetInterfaceSelection);
                ethernetComboBox.SelectedItem(settingsRef())
            }

            */

            nicSwitchEnabledCheckBox.Checked = settingsRef.NetworkInterfaceSwitchingEnabled.GetValueOrDefault(false);
            startWithWindowsCheckbox.Checked = settingsRef.AutoStartWithWindows.GetValueOrDefault(false);
            autoEnableSwitcherOnStartupCheckBox.Checked = settingsRef.AutoEnableSwitcherOnStartup.GetValueOrDefault(false);
            
            if (settingsRef.EthernetInterfaceSelection != null)
                ethernetDoNotAutoDiscardCheckBox.Checked = settingsRef.EthernetInterfaceSelection.DoNotAutoDiscard;
            
            if (settingsRef.WifiInterfaceSelection != null)
                wifiDoNotAutoDiscardCheckBox.Checked = settingsRef.WifiInterfaceSelection.DoNotAutoDiscard;
        }

        /*

        private string GetNICString(NetworkInterface nic)
        {
            return $"Device: {nic.Description} | MAC: {nic.GetPhysicalAddress().ToString()} | Status: {nic.OperationalStatus.ToString()}";
        }
        */

        private void SaveNewSettings()
        {
            settingsRef.NetworkInterfaceSwitchingEnabled = nicSwitchEnabledCheckBox.Checked;
            settingsRef.AutoStartWithWindows = startWithWindowsCheckbox.Checked;
            settingsRef.AutoEnableSwitcherOnStartup = autoEnableSwitcherOnStartupCheckBox.Checked;

            ((NetworkInterfaceDeviceSelection)ethernetComboBox.SelectedItem).DoNotAutoDiscard = ethernetDoNotAutoDiscardCheckBox.Checked;
            ((NetworkInterfaceDeviceSelection)wifiComboBox.SelectedItem).DoNotAutoDiscard = wifiDoNotAutoDiscardCheckBox.Checked;

            settingsRef.EthernetInterfaceSelection = (NetworkInterfaceDeviceSelection)ethernetComboBox.SelectedItem;
            settingsRef.WifiInterfaceSelection = (NetworkInterfaceDeviceSelection)wifiComboBox.SelectedItem;
        }

        private bool CheckIfNewSettingsAreValid()
        {
            if (ethernetComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("You have not selected an Ethernet connection", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }

            if (wifiComboBox.SelectedIndex == -1)
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
                    EthernetWifiSwitchApp.AppInstance.Exit();

                } else
                {
                    e.Cancel = true;
                }
            }
        }

        private void redetectNICsButton_Click(object sender, EventArgs e)
        {
            //GetAttachedNetworkInterfaceSelections();
        }

        private void ethernetComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ethernetDoNotAutoDiscardCheckBox.Enabled = ((NetworkInterfaceDeviceSelection)ethernetComboBox.SelectedItem).IsActualNetworkInterface;
        }

        private void wifiComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            wifiDoNotAutoDiscardCheckBox.Enabled = ((NetworkInterfaceDeviceSelection)wifiComboBox.SelectedItem).IsActualNetworkInterface;
        }

        private void ethernetDoNotAutoDiscardCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            /*
            if (ethernetDoNotAutoDiscardCheckBox.Checked == false
                && ethernetComboBox.SelectedItem != null
                && ((NetworkInterfaceDeviceSelection)ethernetComboBox.SelectedItem).IsActualNetworkInterface == false)
            {
                DialogResult result = MessageBox.Show(this, $"This network interface (MAC: {null} is not currently connected to this machine, so disabling this checkbox will remove this selection upon saving. Continue ?", "Settings", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                if (result != DialogResult.Yes)
                {
                    ethernetDoNotAutoDiscardCheckBox.CheckedChanged -= ethernetDoNotAutoDiscardCheckBox_CheckedChanged;
                    ethernetDoNotAutoDiscardCheckBox.Checked = false;
                    ethernetDoNotAutoDiscardCheckBox.CheckedChanged += ethernetDoNotAutoDiscardCheckBox_CheckedChanged;
                } else
                {

                }
            }

            */
        }
    }
}
