using Newtonsoft.Json;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace NetworkAssistantNamespace
{
    [DataContract]
    public class Settings
    {

        private const string configFileName = "config.json";
        private static Settings settings = null;
        private readonly string configFilePath = AppDomain.CurrentDomain.BaseDirectory + configFileName;

        public bool? NetworkInterfaceSwitchingEnabled { get; set; } = null;

        [DataMember(Name = "autoStartWithWindows", EmitDefaultValue = false)]
        public bool? AutoStartWithWindows { get; set; } = null;

        [DataMember(Name = "autoEnableSwitcherOnStartup", EmitDefaultValue = false)]
        public bool? AutoEnableSwitcherOnStartup { get; set; } = null;

        [DataMember(Name = "ethernetInterfaceSelection", EmitDefaultValue = false)]
        public NetworkInterfaceDeviceSelection EthernetInterfaceSelection { get; set; } = null;

        [DataMember(Name = "wifiInterfaceSelection", EmitDefaultValue = false)]
        public NetworkInterfaceDeviceSelection WifiInterfaceSelection { get; set; } = null;

        private Settings()
        {   
        }

        public void LoadSettings()
        {
            if (File.Exists(configFilePath))
            {
                string configFileString = File.ReadAllText(configFilePath);
                JsonConvert.PopulateObject(configFileString, this);
                NetworkInterfaceSwitchingEnabled = AutoEnableSwitcherOnStartup;
            }

            NetworkInterfaceDeviceSelection.LoadAllNetworkInterfaceSelections(this);

            if (NetworkInterfaceDeviceSelection.AllEthernetNetworkInterfaceSelections.Count == 0
                || NetworkInterfaceDeviceSelection.AllWifiNetworkInterfaceSelections.Count == 0)
            {
                MessageBox.Show("Your system doesn't have Wifi and/or Ethernet adapters. Exiting.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MainAppContext.AppInstance.ExitImmediately();
            }

            if (!allSettingsValidAndPresent())
                ShowSettingsForm(true);
        }

        private bool allSettingsValidAndPresent()
        {
            if (AutoEnableSwitcherOnStartup == null)
                return false;

            if (AutoEnableSwitcherOnStartup == null)
                return false;

            if (EthernetInterfaceSelection == null)
                return false;

            if (WifiInterfaceSelection == null)
                return false;

            return true;
        }

        public bool ShowSettingsForm(bool needToInitializeSettings)
        {
            using (SettingsForm settingsForm = new SettingsForm())
            {
                bool changesDone = settingsForm.ShowSettings(this, needToInitializeSettings);
                if (changesDone)
                    WriteSettings();

                return changesDone;
            }
        }

        public static Settings GetSettingsInstance()
        {
            if (settings == null)
                settings = new Settings();

            return settings;
        }

        private void WriteSettings()
        {
            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);

            File.WriteAllText(configFilePath, json);
        }
    }
}
