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
            if (String.IsNullOrWhiteSpace(windowsDeviceId)
                || String.IsNullOrWhiteSpace(name)
                || String.IsNullOrWhiteSpace(deviceName)
                || String.IsNullOrWhiteSpace(physicalAddress)
                || CurrentState == null
                || interfaceType == null
                || DoNotAutoDiscard == null)
                return false;
            else
                return true;
        }

        public NetworkInterfaceDevice()
        {
        }

        public NetworkInterfaceDevice(ManagementObject managementObject, InterfaceType interfaceType, bool doNotAutoDiscard = false)
        {
            this.DoNotAutoDiscard = doNotAutoDiscard;
            windowsDeviceId = managementObject[managementObjectTagForWindowsDeviceID].ToString();
            name = managementObject[managementObjectTagForNetworkInterfaceConnectionName].ToString();
            deviceName = managementObject[managementObjectTagForNetworkInterfaceDeviceName].ToString();
            this.interfaceType = interfaceType;
            physicalAddress = managementObject[managementObjectTagForMACAddress].ToString();
            LogThis(LogLevel.Debug, "Created NetworkInterfaceDevice .. Loading current status values ...");

            LoadCurrentStatusValues(managementObject);

        }

        public override string ToString()
        {
            LogThis(LogLevel.Trace, "Returning ToString() value ...");
            if (CurrentState > InterfaceState.DevicePhysicallyDisconnected)
                return $"Device: {deviceName} | MAC: {physicalAddress}";
            else
                return $"Disconnected Device: {deviceName} | MAC: {physicalAddress}";
        }

        public static void LoadAllNetworkInterfaces()
        {
            LogThisStatic(LogLevel.Info, "Loading all network interfaces ...");

            (AllEthernetNetworkInterfaces, AllWifiNetworkInterfaces) =
                GetEthernetAndWifiTypeNetworkInterfaces();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != typeof(NetworkInterfaceDevice))
                return false;

            NetworkInterfaceDevice otherDevice = (NetworkInterfaceDevice)obj;

            if (this.windowsDeviceId != otherDevice.windowsDeviceId)
                return false;

            if (this.deviceName != otherDevice.deviceName)
                return false;

            if (this.physicalAddress != otherDevice.physicalAddress)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return windowsDeviceId.GetHashCode();
        }

        public void RefreshCurrentStatus()
        {
            LogThis(LogLevel.Trace, "Refreshing current status ...");

            LoadCurrentStatusValues(GetNetAdapterManagementObject());
        }

        public bool ChangeStateIfNeeded(InterfaceChangeNeeded changeNeeded)
        {
            LogThis(LogLevel.Trace, "Determing if change is needed ...",
                Global.Pair(Global.LoggingVarNames.ChangeType, changeNeeded.GetDescriptionString()));

            bool doTheChange = false;

            if (changeNeeded != InterfaceChangeNeeded.Nothing)
            {
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

            return doTheChange;
        }

        ManagementObject GetNetAdapterManagementObject()
        {
            LogThis(LogLevel.Trace, "Getting Management object ...");

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
                    LogThis(LogLevel.Trace, "Management object count is zero - returning null ...");
                    return null;
                }
                else
                {
                    LogThis(LogLevel.Trace, "Return Management object ...");
                    var enumerator = collection.GetEnumerator();
                    enumerator.MoveNext();
                    return (ManagementObject)enumerator.Current;
                }
            }
        }

        void LoadCurrentStatusValues(ManagementObject managementObject)
        {
            LogThis(LogLevel.Trace, "Loading current state values ...");
            
            if (managementObject != null)
            {
                //Check if device is enabled:
                if (Int32.Parse(managementObject[managementObjectTagForCheckingIfNetworkInterfaceIsEnabled].ToString()) == 1)
                {
                    LogThis(LogLevel.Trace, "Device is enabled ...");

                    //Check if device has network connectivity:
                    if (Int32.Parse(managementObject[managementObjectTagForCheckingIfThereIsNetworkConnectivity].ToString()) == 1)
                    {
                        LogThis(LogLevel.Trace, "Device has network connectivity ...");
                        CurrentState = InterfaceState.HasNetworkConnectivity;
                    }
                    else
                    {
                        LogThis(LogLevel.Trace, "Device has no network connectivity ...");
                        CurrentState = InterfaceState.EnabledButNoNetworkConnectivity;
                    }
                }
                else
                {
                    LogThis(LogLevel.Trace, "Device is disabled ...");
                    CurrentState = InterfaceState.Disabled;
                }
            }
            else
            {
                LogThis(LogLevel.Trace, "Device is physically disconnected ...");
                CurrentState = InterfaceState.DevicePhysicallyDisconnected;
            }
        }

        static (List<NetworkInterfaceDevice>, List<NetworkInterfaceDevice>) GetEthernetAndWifiTypeNetworkInterfaces()
        {
            LogThisStatic(LogLevel.Trace, $"Getting {InterfaceType.Ethernet.GetDescriptionString()} and {InterfaceType.WiFi.GetDescriptionString()} interfaces ...");

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

            LogThisStatic(LogLevel.Trace, "Finding previous interface selections...");

            FindPreviousInterfaceIfAnyAndAddIfNeeded(ethernetOptions, InterfaceType.Ethernet);
            FindPreviousInterfaceIfAnyAndAddIfNeeded(wifiOptions, InterfaceType.WiFi);

            return (ethernetOptions, wifiOptions);
        }

        static void FindPreviousInterfaceIfAnyAndAddIfNeeded(List<NetworkInterfaceDevice> existingDevices, InterfaceType interfaceType)
        {
            LogThisStatic(LogLevel.Trace, $"Finding previous {interfaceType.GetDescriptionString()} selection ...");


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
            LogThis(LogLevel.Trace, "Attempting device-specific config repair");
            bool anyPersistedConfigRepaired = false;

            if (this.interfaceType != interfaceType)
            {
                this.interfaceType = interfaceType;
                anyPersistedConfigRepaired = true;
            }

            LogThis(LogLevel.Trace, $"Repair done: {anyPersistedConfigRepaired}");
            return anyPersistedConfigRepaired;
        }

        private void LogThis(LogLevel level, string message, params KeyValuePair<string, string>[] properties)
        {
            LogThisStatic(level: level, message: message, properties: properties, callerMethodName: new StackFrame(1).GetMethod().Name, device: this);
        }

        private static void LogThisStatic(LogLevel level, string message, string callerMethodName = null, int? callDepth = null, NetworkInterfaceDevice device = null, params KeyValuePair<string, string>[] properties)
        {
            int callDepthToUse = callDepth.HasValue ? callDepth.Value : (new StackTrace()).FrameCount;
            if (device != null)
            {
                Global.Log(Logger, level, message, callDepthToUse, properties,
                Global.Pair(Global.LoggingVarNames.InterfaceType, device.interfaceType.GetDescriptionString()),
                Global.Pair(Global.LoggingVarNames.CallerMethodName, callerMethodName ?? new StackFrame(1).GetMethod().Name));
            }
            else
            {
                Global.Log(Logger, level, message, callDepthToUse, properties);
            }
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
