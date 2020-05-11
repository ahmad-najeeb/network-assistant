using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;

namespace NetworkAssistantNamespace
{
    [DataContract]
    public class Settings
    {
        static readonly Logger Logger = LogManager.GetCurrentClassLogger();
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

        public static void LoadSettingsFromFile(ref bool anyPersistedConfigRepaired)
        {
            LogThisStatic(LogLevel.Debug, "-> Load settings from file ...");
            string configFilePath = Global.AppDataDirectory + "\\" + Global.ConfigFilename;
            Global.AppSettings = new Settings();

            string configFileString = File.ReadAllText(configFilePath);
            LogThisStatic(LogLevel.Trace, "Config file read - populating ...");
            JsonConvert.PopulateObject(configFileString, Global.AppSettings);
            anyPersistedConfigRepaired = Global.AppSettings.Repair();
            Global.AppSettings.NetworkInterfaceSwitchingEnabled = Global.AppSettings.AutoEnableSwitcherOnStartup;
            LogThisStatic(LogLevel.Debug, "Loading settings from file complete");
        }

        bool Repair()
        {
            LogThis(LogLevel.Debug, "-> Repair any broken setting which can be repaired ...");

            bool anyPersistedConfigRepaired = false;

            LogThis(LogLevel.Trace, $"Checking if {InterfaceType.Ethernet.GetDescriptionString()} needs any repairing ...");
            if (EthernetInterface != null && EthernetInterface.RepairConfig(InterfaceType.Ethernet) == true)
                anyPersistedConfigRepaired = true;

            LogThis(LogLevel.Trace, $"Checking if {InterfaceType.WiFi.GetDescriptionString()} needs any repairing ...");
            if (WifiInterface != null && WifiInterface.RepairConfig(InterfaceType.WiFi) == true)
                anyPersistedConfigRepaired = true;

            LogThis(LogLevel.Trace, $"Finished repair. Repairs done: {anyPersistedConfigRepaired}");
            return anyPersistedConfigRepaired;
        }

        public bool AreAllSettingsValidAndPresent()
        {
            LogThis(LogLevel.Debug, "-> Check if all settings are valid and present ...");

            bool isValid = true; //assumption

            if (AutoEnableSwitcherOnStartup == null)
                isValid = false;
            else if (AutoEnableSwitcherOnStartup == null)
                isValid = false;
            else if (ShowCurrentConnectionTypeInSystemTray == null)
                isValid = false;
            else if (EthernetInterface == null || EthernetInterface.IsValid() == false)
                isValid = false;
            else if (WifiInterface == null || WifiInterface.IsValid() == false)
                isValid = false;

            LogThis(LogLevel.Debug, $"Checking done - All settings are valid and present: {isValid}");

            return isValid;
        }

        public bool ShowSettingsForm(bool needToInitializeSettings)
        {
            LogThis(LogLevel.Debug, "-> Show Settings dialog ...");

            using (SettingsForm settingsForm = new SettingsForm())
            {
                LogThis(LogLevel.Debug, "Making the call to show Settings dialog ...");

                bool changesDone = settingsForm.ShowSettings(needToInitializeSettings);
                
                if (changesDone)
                {
                    LogThis(LogLevel.Debug, "Writing new settings to file ...");
                    WriteSettings();
                }

                return changesDone;
            }
        }

        public void WriteSettings()
        {
            LogThis(LogLevel.Debug, "-> Write Settings to file ...");

            string json = JsonConvert.SerializeObject(Global.AppSettings, Formatting.Indented);

            File.WriteAllText(configFilePath, json);

            LogThis(LogLevel.Debug, "Writing Settings to file done");
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
