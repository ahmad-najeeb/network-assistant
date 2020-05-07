﻿using Newtonsoft.Json;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace NetworkAssistantNamespace
{
    [DataContract]
    public class Settings
    {
        static readonly string configFilePath = AppDomain.CurrentDomain.BaseDirectory + configFileName;

        const string configFileName = "config.json";

        public bool? NetworkInterfaceSwitchingEnabled { get; set; } = null;

        [DataMember(Name = "autoStartWithWindows", EmitDefaultValue = false)]
        public bool? AutoStartWithWindows { get; set; } = null;

        [DataMember(Name = "autoEnableSwitcherOnStartup", EmitDefaultValue = false)]
        public bool? AutoEnableSwitcherOnStartup { get; set; } = null;

        [DataMember(Name = "ethernetInterface", EmitDefaultValue = false)]
        public NetworkInterfaceDevice EthernetInterface { get; set; } = null;

        [DataMember(Name = "wifiInterface", EmitDefaultValue = false)]
        public NetworkInterfaceDevice WifiInterface { get; set; } = null;

        Settings()
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

            NetworkInterfaceDevice.LoadAllNetworkInterfaces();

            if (NetworkInterfaceDevice.AllEthernetNetworkInterfaces.Count == 0
                || NetworkInterfaceDevice.AllWifiNetworkInterfaces.Count == 0)
            {
                MessageBox.Show("Your system doesn't have Wifi and/or Ethernet adapters. Exiting.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Global.Controller.ExitImmediately();
            }

            if (!AllSettingsValidAndPresent())
                ShowSettingsForm(true);
        }

        bool AllSettingsValidAndPresent()
        {
            if (AutoEnableSwitcherOnStartup == null)
                return false;

            if (AutoEnableSwitcherOnStartup == null)
                return false;

            if (EthernetInterface == null || EthernetInterface.IsValid() == false)
                return false;

            if (WifiInterface == null || WifiInterface.IsValid() == false)
                return false;

            return true;
        }

        public bool ShowSettingsForm(bool needToInitializeSettings)
        {
            using (SettingsForm settingsForm = new SettingsForm())
            {
                bool changesDone = settingsForm.ShowSettings(needToInitializeSettings);
                if (changesDone)
                    WriteSettings();

                return changesDone;
            }
        }

        public static void SetSettingsInstance()
        {
            if (Global.AppSettings == null)
                Global.AppSettings = new Settings();
        }

        void WriteSettings()
        {
            string json = JsonConvert.SerializeObject(Global.AppSettings, Formatting.Indented);

            File.WriteAllText(configFilePath, json);
        }
    }
}
