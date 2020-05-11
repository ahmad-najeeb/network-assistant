using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.Serialization;

namespace NetworkAssistantNamespace
{
    [DataContract]
    public class NetworkInterfaceDevice
    {
        public static List<NetworkInterfaceDevice> AllEthernetNetworkInterfaces = null;
        public static List<NetworkInterfaceDevice> AllWifiNetworkInterfaces = null;

        static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        const int ethernetInterfaceTypeId = 6;
        const int wifiInterfaceTypeId = 71;
        const string managementObjectTagForMACAddress = "PermanentAddress";
        const string managementObjectTagForWindowsDeviceID = "DeviceID";
        const string managementObjectTagForNetworkInterfaceConnectionName = "Name";
        const string managementObjectTagForNetworkInterfaceDeviceName = "InterfaceDescription";
        const string managementObjectTagForCheckingIfNetworkInterfaceIsEnabled = "InterfaceAdminStatus"; // 1 = Up/Enabled, 2 = Down/Disabled, 3 = Testing
        const string managementObjectTagForCheckingIfThereIsNetworkConnectivity = "MediaConnectState"; //This can be true even if internet is inaccessible
        const string managementObjectTagForCheckingInterfaceType = "InterfaceType";

        public InterfaceState? CurrentState { get; set; } = null;

        [DataMember(Name = "doNotAutoDiscard")]
        public bool? DoNotAutoDiscard { get; set; } = null;

        [DataMember]
        readonly string windowsDeviceId;

        [DataMember]
        readonly string name;

        [DataMember]
        readonly string deviceName;

        [DataMember]
        readonly string physicalAddress;
        
        [DataMember]
        InterfaceType? interfaceType; //Can't be readonly because this value might get 'restored' later on during validation of a disconnected device

        public bool IsValid()
        {
            LogThis(LogLevel.Debug, "-> Check if device is valid (i.e. setup properly) ...");

            bool isValid = true;

            if (String.IsNullOrWhiteSpace(windowsDeviceId)
                || String.IsNullOrWhiteSpace(name)
                || String.IsNullOrWhiteSpace(deviceName)
                || String.IsNullOrWhiteSpace(physicalAddress)
                || CurrentState == null
                || interfaceType == null
                || DoNotAutoDiscard == null)
                isValid = false;
            else
                isValid = true;

            LogThis(LogLevel.Debug, $"Check done - Device is valid: {isValid}");
            return isValid;
        }

        public NetworkInterfaceDevice()
        {
        }

        public NetworkInterfaceDevice(ManagementObject managementObject, InterfaceType interfaceType, bool doNotAutoDiscard = false)
        {
            LogThis(LogLevel.Debug, "-> Create new network device ...");

            this.DoNotAutoDiscard = doNotAutoDiscard;
            windowsDeviceId = managementObject[managementObjectTagForWindowsDeviceID].ToString();
            name = managementObject[managementObjectTagForNetworkInterfaceConnectionName].ToString();
            deviceName = managementObject[managementObjectTagForNetworkInterfaceDeviceName].ToString();
            this.interfaceType = interfaceType;
            physicalAddress = managementObject[managementObjectTagForMACAddress].ToString();
            LogThis(LogLevel.Trace, "Created NetworkInterfaceDevice .. Loading current status values ...");

            LoadCurrentStatusValues(managementObject);

            LogThis(LogLevel.Debug, "Current status loaded - Network device creation fully complete");
        }

        public override string ToString()
        {
            LogThis(LogLevel.Trace, "-> Return ToString() value ...");
            if (CurrentState > InterfaceState.DevicePhysicallyDisconnected)
                return $"Device: {deviceName} | MAC: {physicalAddress}";
            else
                return $"Disconnected Device: {deviceName} | MAC: {physicalAddress}";
        }

        public static void LoadAllNetworkInterfaces()
        {
            LogThisStatic(LogLevel.Info, "-> Load all network devices ...");

            (AllEthernetNetworkInterfaces, AllWifiNetworkInterfaces) =
                GetEthernetAndWifiTypeNetworkInterfaces();
        }

        public override bool Equals(object obj)
        {
            LogThis(LogLevel.Debug, "-> Equals ...");

            bool isEquals = true; //assumptions

            if (obj == null)
                isEquals = false;
            else if (obj.GetType() != typeof(NetworkInterfaceDevice))
                isEquals = false;
            else
            {
                NetworkInterfaceDevice otherDevice = (NetworkInterfaceDevice)obj;

                if (this.windowsDeviceId != otherDevice.windowsDeviceId)
                    isEquals = false;
                else if (this.deviceName != otherDevice.deviceName)
                    isEquals = false;
                else if (this.physicalAddress != otherDevice.physicalAddress)
                    isEquals = false;
            }

            LogThis(LogLevel.Debug, $"Equals comparison done - Returning: {isEquals}");

            return isEquals;
        }

        public override int GetHashCode()
        {
            return windowsDeviceId.GetHashCode();
        }

        public void RefreshCurrentStatus()
        {
            LogThis(LogLevel.Debug, "-> Refresh current status ...");

            LoadCurrentStatusValues(GetNetAdapterManagementObject());

            LogThis(LogLevel.Debug, "Refresh current status done");
        }

        public bool ChangeStateIfNeeded(InterfaceChangeNeeded changeNeeded)
        {
            LogThis(LogLevel.Trace, "-> Determine if change is needed ...",
                Global.Pair(Global.LoggingVarNames.ChangeType, changeNeeded.GetDescriptionString()));

            bool doTheChange = false;

            if (changeNeeded != InterfaceChangeNeeded.Nothing)
            {
                LogThis(LogLevel.Trace, "Refreshing current device status ...",
                Global.Pair(Global.LoggingVarNames.ChangeType, changeNeeded.GetDescriptionString()));

                RefreshCurrentStatus();

                if (CurrentState > InterfaceState.DevicePhysicallyDisconnected)
                {
                    if ((changeNeeded == InterfaceChangeNeeded.Enable && CurrentState < InterfaceState.EnabledButNoNetworkConnectivity)
                        || (changeNeeded == InterfaceChangeNeeded.Disable && CurrentState > InterfaceState.Disabled))
                    {
                        LogThis(LogLevel.Trace, "Need to perform change ...",
                Global.Pair(Global.LoggingVarNames.ChangeType, changeNeeded.GetDescriptionString()));

                        doTheChange = true;
                    }
                }
                else
                    throw new Exception("ERROR hb2e86 :: Device not connected so cannot change state.");

                if (doTheChange)
                {
                    LogThis(LogLevel.Trace, "Doing change ...",
                Global.Pair(Global.LoggingVarNames.ChangeType, changeNeeded.GetDescriptionString()));

                    LogThis(LogLevel.Trace, "Stop network change detection ...",
                Global.Pair(Global.LoggingVarNames.ChangeType, changeNeeded.GetDescriptionString()));

                    Global.Controller.StopNetworkChangeMonitoring();

                    LogThis(LogLevel.Trace, "Executing change command ...",
                Global.Pair(Global.LoggingVarNames.ChangeType, changeNeeded.GetDescriptionString()));

                    
                    ProcessStartInfo psi = new ProcessStartInfo()
                    {
                        FileName = "netsh",
                        Arguments = "interface set interface \"" + name + "\" " + (changeNeeded == InterfaceChangeNeeded.Enable ? "enable" : "disable"),
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true
                    };

                    using (Process p = new Process())
                    {
                        p.StartInfo = psi;
                        p.Start();
                        p.WaitForExit();
                    }

                    LogThis(LogLevel.Trace, "Resume network change detection ...",
                Global.Pair(Global.LoggingVarNames.ChangeType, changeNeeded.GetDescriptionString()));

                    Global.Controller.StartNetworkChangeMonitoring();

                    LogThis(LogLevel.Trace, "Change done",
                Global.Pair(Global.LoggingVarNames.ChangeType, changeNeeded.GetDescriptionString()));

                    LogThis(LogLevel.Trace, "Refreshing status",
                Global.Pair(Global.LoggingVarNames.ChangeType, changeNeeded.GetDescriptionString()));
                    RefreshCurrentStatus();
                }
                else
                {
                    LogThis(LogLevel.Trace, "Proposed change is actually NOT needed",
                Global.Pair(Global.LoggingVarNames.ChangeType, changeNeeded.GetDescriptionString()));
                }
            }
            else
            {
                LogThis(LogLevel.Debug, $"Change reported is {changeNeeded.GetDescriptionString()}",
                Global.Pair(Global.LoggingVarNames.ChangeType, changeNeeded.GetDescriptionString()));
            }

            LogThis(LogLevel.Debug, $"Device-specific change request servicing done - Change done: ${doTheChange}",
                Global.Pair(Global.LoggingVarNames.ChangeType, changeNeeded.GetDescriptionString()));

            return doTheChange;
        }

        ManagementObject GetNetAdapterManagementObject()
        {
            LogThis(LogLevel.Debug, "-> Get Management object ...");

            var objectSearcher = new ManagementObjectSearcher("root\\StandardCimv2", $@"select * from MSFT_NetAdapter"); //Physical adapter

            string q = String.Format("select * from MSFT_NetAdapter where {0}=\"{1}\"",
                managementObjectTagForWindowsDeviceID,
                windowsDeviceId
                );

            using (var searcher = new ManagementObjectSearcher("root\\StandardCimv2", q))
            {
                ManagementObjectCollection collection = searcher.Get();

                if (collection.Count == 0) //Device isn't currently connected to host machine
                {
                    LogThis(LogLevel.Debug, "Management object count is zero - returning null ...");
                    return null;
                }
                else
                {
                    LogThis(LogLevel.Debug, "Return Management object ...");
                    var enumerator = collection.GetEnumerator();
                    enumerator.MoveNext();
                    return (ManagementObject)enumerator.Current;
                }
            }
        }

        void LoadCurrentStatusValues(ManagementObject managementObject)
        {
            LogThis(LogLevel.Debug, "-> Load current state values ...");
            
            if (managementObject != null)
            {
                //Check if device is enabled:
                if (Int32.Parse(managementObject[managementObjectTagForCheckingIfNetworkInterfaceIsEnabled].ToString()) == 1)
                {
                    //Check if device has network connectivity:
                    if (Int32.Parse(managementObject[managementObjectTagForCheckingIfThereIsNetworkConnectivity].ToString()) == 1)
                    {
                        LogThis(LogLevel.Debug, "Device is enabled and has network connectivity ...");
                        CurrentState = InterfaceState.HasNetworkConnectivity;
                    }
                    else
                    {
                        LogThis(LogLevel.Debug, "Device is enabled but has no network connectivity ...");
                        CurrentState = InterfaceState.EnabledButNoNetworkConnectivity;
                    }
                }
                else
                {
                    LogThis(LogLevel.Debug, "Device is disabled ...");
                    CurrentState = InterfaceState.Disabled;
                }
            }
            else
            {
                LogThis(LogLevel.Debug, "Device is physically disconnected ...");
                CurrentState = InterfaceState.DevicePhysicallyDisconnected;
            }
        }

        static (List<NetworkInterfaceDevice>, List<NetworkInterfaceDevice>) GetEthernetAndWifiTypeNetworkInterfaces()
        {
            LogThisStatic(LogLevel.Debug, $"-> Get all {InterfaceType.Ethernet.GetDescriptionString()} and {InterfaceType.WiFi.GetDescriptionString()} devices ...");

            List<NetworkInterfaceDevice> ethernetOptions = new List<NetworkInterfaceDevice>();
            List<NetworkInterfaceDevice> wifiOptions = new List<NetworkInterfaceDevice>();

            using (var searcher = new ManagementObjectSearcher("root\\StandardCimv2", $@"select * from MSFT_NetAdapter where ConnectorPresent=True"))
            {
                using (var managementObjectCollection = searcher.Get())
                {
                    LogThisStatic(LogLevel.Trace, $"Got Management Object collection containing {managementObjectCollection.Count} items ...");

                    foreach (var managementObject in managementObjectCollection)
                    {
                        if (Int32.Parse(managementObject[managementObjectTagForCheckingInterfaceType].ToString())
                            == ethernetInterfaceTypeId)
                            ethernetOptions.Add(new NetworkInterfaceDevice((ManagementObject)managementObject, InterfaceType.Ethernet));
                        else if (Int32.Parse(managementObject[managementObjectTagForCheckingInterfaceType].ToString())
                            == wifiInterfaceTypeId)
                            wifiOptions.Add(new NetworkInterfaceDevice((ManagementObject)managementObject, InterfaceType.WiFi));
                    }
                }
            }

            LogThisStatic(LogLevel.Trace, "Finding previous network device selections...");

            FindPreviousInterfaceIfAnyAndAddIfNeeded(ethernetOptions, InterfaceType.Ethernet);
            FindPreviousInterfaceIfAnyAndAddIfNeeded(wifiOptions, InterfaceType.WiFi);

            LogThisStatic(LogLevel.Debug, "Returning resultsets ...");

            return (ethernetOptions, wifiOptions);
        }

        static void FindPreviousInterfaceIfAnyAndAddIfNeeded(List<NetworkInterfaceDevice> existingDevices, InterfaceType interfaceType)
        {
            LogThisStatic(LogLevel.Debug, $"-> Find previous {interfaceType.GetDescriptionString()} device selection ...");

            if (Global.AppSettings != null
                && ((interfaceType == InterfaceType.Ethernet && Global.AppSettings.EthernetInterface != null)
                || (interfaceType == InterfaceType.WiFi && Global.AppSettings.WifiInterface != null)))
            {
                NetworkInterfaceDevice deviceToConsider;

                if (interfaceType == InterfaceType.Ethernet)
                    deviceToConsider = Global.AppSettings.EthernetInterface;
                else if (interfaceType == InterfaceType.WiFi)
                    deviceToConsider = Global.AppSettings.WifiInterface;
                else
                    throw new Exception("ERROR as2xj5 :: Invalid network interface type specfied.");

                if (deviceToConsider != null)
                {
                    int index;
                    if ((index = existingDevices.IndexOf(deviceToConsider)) != -1)
                    {
                        LogThisStatic(LogLevel.Trace, $"Previous {interfaceType.GetDescriptionString()} device is WAS NOT found ...");

                        existingDevices.ElementAt(index).DoNotAutoDiscard = deviceToConsider.DoNotAutoDiscard;
                        if (interfaceType == InterfaceType.Ethernet)
                            Global.AppSettings.EthernetInterface = existingDevices.ElementAt(index);
                        else
                            Global.AppSettings.WifiInterface = existingDevices.ElementAt(index);
                    }
                    else
                    {
                        LogThisStatic(LogLevel.Trace, $"Previous {interfaceType.GetDescriptionString()} device is WAS found ...");

                        if (deviceToConsider.DoNotAutoDiscard == true)
                        {
                            deviceToConsider.CurrentState = InterfaceState.DevicePhysicallyDisconnected;
                            existingDevices.Add(deviceToConsider);
                        }
                        else
                        {
                            if (interfaceType == InterfaceType.Ethernet)
                                Global.AppSettings.EthernetInterface = null;
                            else
                                Global.AppSettings.WifiInterface = null;
                        }
                    }
                }
                else
                {
                    LogThisStatic(LogLevel.Warn, $"Previous {interfaceType.GetDescriptionString()} device is null...");
                }
            }
        }

        public bool RepairConfig(InterfaceType interfaceType)
        {
            LogThis(LogLevel.Debug, "-> Attempt repair of device-specific config (if needed)");
            bool anyPersistedConfigRepaired = false;

            if (this.interfaceType != interfaceType)
            {
                this.interfaceType = interfaceType;
                anyPersistedConfigRepaired = true;
            }

            LogThis(LogLevel.Debug, $"Repair done: {anyPersistedConfigRepaired}");
            return anyPersistedConfigRepaired;
        }

        private void LogThis(LogLevel level, string message, params KeyValuePair<string, string>[] properties)
        {
            LogThisStatic(level: level, message: message, properties: properties, callerMethodName: new StackFrame(1).GetMethod().Name, device: this);
        }

        private static void LogThisStatic(LogLevel level, string message, string callerMethodName = null, int? callDepth = null, NetworkInterfaceDevice device = null, params KeyValuePair<string, string>[] properties)
        {
            int callDepthToUse = callDepth.HasValue ? callDepth.Value : (new StackTrace()).FrameCount;

            int additionalPropertiesSize = 1;
            if (device != null)
                additionalPropertiesSize += 1;

            KeyValuePair<string, string>[] additionalProperties = new KeyValuePair<string, string>[additionalPropertiesSize];

            additionalProperties[0] = new KeyValuePair<string, string>(Global.LoggingVarNames.CallerMethodName, callerMethodName ?? new StackFrame(1).GetMethod().Name);
            if (device != null)
                additionalProperties[1] = new KeyValuePair<string, string>(Global.LoggingVarNames.InterfaceType, device.interfaceType != null ? device.interfaceType.GetDescriptionString() : "NULL");

            Global.Log(Logger, level, message, callDepthToUse, properties, additionalProperties);
        }
    }

    public enum InterfaceChangeNeeded
    {
        Nothing,
        Enable,
        Disable
    }

    public enum InterfaceState
    {
        DevicePhysicallyDisconnected=0,
        Disabled=1,
        EnabledButNoNetworkConnectivity=2,
        HasNetworkConnectivity=3
    }

    public enum InterfaceType
    {
        None,
        Ethernet,
        WiFi
    }
}
