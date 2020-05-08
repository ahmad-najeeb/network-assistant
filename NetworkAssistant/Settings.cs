using Newtonsoft.Json;
using System;
using System.IO;
using System.Runtime.Serialization;

namespace NetworkAssistantNamespace
{
    [DataContract]
    public class Settings
    {
        static readonly string configFilePath = Global.AppDataDirectory + "\\" + Global.ConfigFilename;

        

        public bool? NetworkInterfaceSwitchingEnabled { get; set; } = null;

        [DataMember(Name = "autoStartWithWindows", EmitDefaultValue = false)]
        public bool? AutoStartWithWindows { get; set; } = null;

        [DataMember(Name = "autoEnableSwitcherOnStartup", EmitDefaultValue = false)]
        public bool? AutoEnableSwitcherOnStartup { get; set; } = null;

        [DataMember(Name = "showCurrentConnectionTypeInSystemTray", EmitDefaultValue = false)]
        public bool? ShowCurrentConnectionTypeInSystemTray { get; set; } = null;

        [DataMember(Name = "ethernetInterface", EmitDefaultValue = false)]
        public NetworkInterfaceDevice EthernetInterface { get; set; } = null;

        [DataMember(Name = "wifiInterface", EmitDefaultValue = false)]
        public NetworkInterfaceDevice WifiInterface { get; set; } = null;

        Settings()
        {   
        }

        public static void LoadSettingsFromFile()
        {
            string configFilePath = Global.AppDataDirectory + "\\" + Global.ConfigFilename;
            Global.AppSettings = new Settings();

            string configFileString = File.ReadAllText(configFilePath);
            JsonConvert.PopulateObject(configFileString, Global.AppSettings);
            Global.AppSettings.NetworkInterfaceSwitchingEnabled = Global.AppSettings.AutoEnableSwitcherOnStartup;
        }

        public bool AreAllSettingsValidAndPresent()
        {
            if (AutoEnableSwitcherOnStartup == null)
                return false;

            if (AutoEnableSwitcherOnStartup == null)
                return false;

            if (ShowCurrentConnectionTypeInSystemTray == null)
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
        /*

        public static void SetSettingsInstance()
        {
            if (Global.AppSettings == null)
                Global.AppSettings = new Settings();
        }
        */

        void WriteSettings()
        {
            string json = JsonConvert.SerializeObject(Global.AppSettings, Formatting.Indented);

            File.WriteAllText(configFilePath, json);
        }
    }
}
