using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.Serialization;

namespace NetworkAssistantNamespace
{
    [DataContract]
    public class NetworkInterfaceDevice
    {
        public static List<NetworkInterfaceDevice> AllEthernetNetworkInterfaces = null;
        public static List<NetworkInterfaceDevice> AllWifiNetworkInterfaces = null;

        static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

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
        readonly NetworkInterfaceType? networkInterfaceType;

        public bool IsValid()
        {
            if (String.IsNullOrWhiteSpace(windowsDeviceId)
                || String.IsNullOrWhiteSpace(name)
                || String.IsNullOrWhiteSpace(deviceName)
                || String.IsNullOrWhiteSpace(physicalAddress)
                || CurrentState == null
                || networkInterfaceType == null
                || DoNotAutoDiscard == null)
                return false;
            else
                return true;
        }

        public NetworkInterfaceDevice()
        {
        }

        public NetworkInterfaceDevice(ManagementObject managementObject, NetworkInterfaceType interfaceType, bool doNotAutoDiscard = false)
        {
            this.DoNotAutoDiscard = doNotAutoDiscard;
            windowsDeviceId = managementObject[managementObjectTagForWindowsDeviceID].ToString();
            name = managementObject[managementObjectTagForNetworkInterfaceConnectionName].ToString();
            deviceName = managementObject[managementObjectTagForNetworkInterfaceDeviceName].ToString();
            networkInterfaceType = interfaceType;
            physicalAddress = managementObject[managementObjectTagForMACAddress].ToString();
            LoadCurrentStatusValues(managementObject);
        }

        public override string ToString()
        {
            if (CurrentState > InterfaceState.DevicePhysicallyDisconnected)
                return $"Device: {deviceName} | MAC: {physicalAddress}";
            else
                return $"Disconnected Device: {deviceName} | MAC: {physicalAddress}";
        }

        public static void LoadAllNetworkInterfaces()
        {
            Logger.Info("Loading all network interfaces ...");

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
            Logger.Trace("{changeID}-{adapterType} :: Refreshing current status ...",
                Global.ChangeIDBeingProcessed, (networkInterfaceType == NetworkInterfaceType.Ethernet ? "E" : "W"));
            LoadCurrentStatusValues(GetNetAdapterManagementObject());
        }

        public bool ChangeStateIfNeeded(InterfaceChangeNeeded changeNeeded)
        {
            Logger.Trace("{changeID}-{adapterType}-{changeType} :: Determing if change is needed ...",
                Global.ChangeIDBeingProcessed, (networkInterfaceType == NetworkInterfaceType.Ethernet ? "E" : "W"),
                changeNeeded.ToString());

            bool doTheChange = false;

            if (changeNeeded != InterfaceChangeNeeded.Nothing)
            {
                RefreshCurrentStatus();

                if (CurrentState > InterfaceState.DevicePhysicallyDisconnected)
                {
                    if (changeNeeded == InterfaceChangeNeeded.Enable && CurrentState < InterfaceState.EnabledButNoNetworkConnectivity)
                    {
                        Logger.Trace("{changeID}-{adapterType}-{changeType} :: Need to perform change ...",
                Global.ChangeIDBeingProcessed, (networkInterfaceType == NetworkInterfaceType.Ethernet ? "E" : "W"),
                changeNeeded.ToString());
                        doTheChange = true;
                    }
                    else if (changeNeeded == InterfaceChangeNeeded.Disable && CurrentState > InterfaceState.Disabled)
                    {
                        Logger.Trace("{changeID}-{adapterType}-{changeType} :: Need to perform change ...",
                Global.ChangeIDBeingProcessed, (networkInterfaceType == NetworkInterfaceType.Ethernet ? "E" : "W"),
                changeNeeded.ToString());
                        doTheChange = true;
                    }
                }
                else
                    throw new Exception("ERROR hb2e86 :: Device not connected so cannot change state.");

                if (doTheChange)
                {
                    Logger.Trace("{changeID}-{adapterType}-{changeType} :: Doing change ...",
                        Global.ChangeIDBeingProcessed, (networkInterfaceType == NetworkInterfaceType.Ethernet ? "E" : "W"),
                changeNeeded.ToString());

                    Global.Controller.StopNetworkChangeMonitoring();

                    Logger.Trace("{changeID}-{adapterType}-{changeType} :: Executing change command ...",
                        Global.ChangeIDBeingProcessed, (networkInterfaceType == NetworkInterfaceType.Ethernet ? "E" : "W"),
                changeNeeded.ToString());

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

                    Logger.Trace("{changeID}-{adapterType}-{changeType} :: Change done",
                        Global.ChangeIDBeingProcessed, (networkInterfaceType == NetworkInterfaceType.Ethernet ? "E" : "W"),
                changeNeeded.ToString());

                    RefreshCurrentStatus();
                }
                else
                {
                    Logger.Trace("{changeID}-{adapterType}-{changeType} :: Proposed change is actually NOT needed ...",
                        Global.ChangeIDBeingProcessed, (networkInterfaceType == NetworkInterfaceType.Ethernet ? "E" : "W"),
                changeNeeded.ToString());
                }
            }

            return doTheChange;
        }

        ManagementObject GetNetAdapterManagementObject()
        {
            Logger.Trace("{changeID}-{adapterType} :: Getting Management object ...",
                Global.ChangeIDBeingProcessed, (networkInterfaceType == NetworkInterfaceType.Ethernet ? "E" : "W"));

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
                    Logger.Trace("{changeID}-{adapterType} :: Management object count is zero - returning null ...",
                Global.ChangeIDBeingProcessed, (networkInterfaceType == NetworkInterfaceType.Ethernet ? "E" : "W"));
                    return null;
                }
                else
                {
                    var enumerator = collection.GetEnumerator();
                    enumerator.MoveNext();
                    return (ManagementObject)enumerator.Current;
                }
            }
        }

        void LoadCurrentStatusValues(ManagementObject managementObject)
        {
            Logger.Trace("{changeID}-{adapterType} :: Loading current state values ...",
                Global.ChangeIDBeingProcessed, (networkInterfaceType == NetworkInterfaceType.Ethernet ? "E" : "W"));

            if (managementObject != null)
            {
                //Check if device is enabled:
                if (Int32.Parse(managementObject[managementObjectTagForCheckingIfNetworkInterfaceIsEnabled].ToString()) == 1)
                {
                    Logger.Trace("{changeID}-{adapterType} :: Device is enabled ...",
                Global.ChangeIDBeingProcessed, (networkInterfaceType == NetworkInterfaceType.Ethernet ? "E" : "W"));
                    //Check if device has network connectivity:
                    if (Int32.Parse(managementObject[managementObjectTagForCheckingIfThereIsNetworkConnectivity].ToString()) == 1)
                    {
                        Logger.Trace("{changeID}-{adapterType} :: Device has network connectivity ...",
                Global.ChangeIDBeingProcessed, (networkInterfaceType == NetworkInterfaceType.Ethernet ? "E" : "W"));
                        CurrentState = InterfaceState.HasNetworkConnectivity;
                    }
                    else
                    {
                        Logger.Trace("{changeID}-{adapterType} :: Device has no network connectivity ...",
                Global.ChangeIDBeingProcessed, (networkInterfaceType == NetworkInterfaceType.Ethernet ? "E" : "W"));
                        CurrentState = InterfaceState.EnabledButNoNetworkConnectivity;
                    }
                }
                else
                {
                    Logger.Trace("{changeID}-{adapterType} :: Device is disabled ...",
                Global.ChangeIDBeingProcessed, (networkInterfaceType == NetworkInterfaceType.Ethernet ? "E" : "W"));
                    CurrentState = InterfaceState.Disabled;
                }
            }
            else
            {
                Logger.Trace("{changeID}-{adapterType} :: Device is physically disconnected ...",
                Global.ChangeIDBeingProcessed, (networkInterfaceType == NetworkInterfaceType.Ethernet ? "E" : "W"));
                CurrentState = InterfaceState.DevicePhysicallyDisconnected;
            }
        }

        static (List<NetworkInterfaceDevice>, List<NetworkInterfaceDevice>) GetEthernetAndWifiTypeNetworkInterfaces()
        {
            List<NetworkInterfaceDevice> ethernetOptions = new List<NetworkInterfaceDevice>();
            List<NetworkInterfaceDevice> wifiOptions = new List<NetworkInterfaceDevice>();

            using (var searcher = new ManagementObjectSearcher("root\\StandardCimv2", $@"select * from MSFT_NetAdapter where ConnectorPresent=True"))
            {
                using (var managementObjectCollection = searcher.Get())
                {
                    foreach (var managementObject in managementObjectCollection)
                    {
                        if (Int32.Parse(managementObject[managementObjectTagForCheckingInterfaceType].ToString())
                            == ethernetInterfaceTypeId)
                            ethernetOptions.Add(new NetworkInterfaceDevice((ManagementObject)managementObject, NetworkInterfaceType.Ethernet));
                        else if (Int32.Parse(managementObject[managementObjectTagForCheckingInterfaceType].ToString())
                            == wifiInterfaceTypeId)
                            wifiOptions.Add(new NetworkInterfaceDevice((ManagementObject)managementObject, NetworkInterfaceType.Wireless80211));
                    }
                }
            }

            FindPreviousInterfaceIfAnyAndAddIfNeeded(ethernetOptions, NetworkInterfaceType.Ethernet);
            FindPreviousInterfaceIfAnyAndAddIfNeeded(wifiOptions, NetworkInterfaceType.Wireless80211);

            return (ethernetOptions, wifiOptions);
        }

        static void FindPreviousInterfaceIfAnyAndAddIfNeeded(List<NetworkInterfaceDevice> existingDevices, NetworkInterfaceType interfaceType)
        {
            if (Global.AppSettings != null
                && ((interfaceType == NetworkInterfaceType.Ethernet && Global.AppSettings.EthernetInterface != null)
                || (interfaceType == NetworkInterfaceType.Wireless80211 && Global.AppSettings.WifiInterface != null)))
            {
                NetworkInterfaceDevice deviceToConsider;

                if (interfaceType == NetworkInterfaceType.Ethernet)
                    deviceToConsider = Global.AppSettings.EthernetInterface;
                else if (interfaceType == NetworkInterfaceType.Wireless80211)
                    deviceToConsider = Global.AppSettings.WifiInterface;
                else
                    throw new Exception("ERROR as2xj5 :: Invalid network interface type specfied.");

                if (deviceToConsider != null)
                {
                    int index;
                    if ((index = existingDevices.IndexOf(deviceToConsider)) != -1)
                    {
                        existingDevices.ElementAt(index).DoNotAutoDiscard = deviceToConsider.DoNotAutoDiscard;
                        if (interfaceType == NetworkInterfaceType.Ethernet)
                            Global.AppSettings.EthernetInterface = existingDevices.ElementAt(index);
                        else
                            Global.AppSettings.WifiInterface = existingDevices.ElementAt(index);
                    }
                    else
                    {
                        if (deviceToConsider.DoNotAutoDiscard == true)
                        {
                            deviceToConsider.CurrentState = InterfaceState.DevicePhysicallyDisconnected;
                            existingDevices.Add(deviceToConsider);
                        }
                        else
                        {
                            if (interfaceType == NetworkInterfaceType.Ethernet)
                                Global.AppSettings.EthernetInterface = null;
                            else
                                Global.AppSettings.WifiInterface = null;
                        }
                    }
                }
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
}
