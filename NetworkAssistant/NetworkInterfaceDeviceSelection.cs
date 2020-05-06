using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetworkAssistantNamespace
{
    [DataContract]
    public class NetworkInterfaceDeviceSelection
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private const int ethernetInterfaceTypeId = 6;
        private const int wifiInterfaceTypeId = 71;

        private const string managementObjectTagForMACAddress = "PermanentAddress";
        private const string managementObjectTagForWindowsDeviceID = "DeviceID";
        private const string managementObjectTagForNetworkInterfaceConnectionName = "Name";
        private const string managementObjectTagForNetworkInterfaceDeviceName = "InterfaceDescription";
        private const string managementObjectTagForCheckingIfNetworkInterfaceIsEnabled = "InterfaceAdminStatus"; // 1 = Up/Enabled, 2 = Down/Disabled, 3 = Testing
        private const string managementObjectTagForCheckingIfThereIsNetworkConnectivity = "MediaConnectState"; //This can be true even if internet is inaccessible
        private const string managementObjectTagForCheckingInterfaceType = "InterfaceType";

        [DataMember]
        string windowsDeviceId;

        [DataMember]
        string name;

        [DataMember]
        string deviceName;

        [DataMember]
        string physicalAddress;

        public InterfaceState CurrentState { get; set; }

        [DataMember]
        NetworkInterfaceType networkInterfaceType;

        [DataMember(Name = "doNotAutoDiscard")]
        public bool DoNotAutoDiscard { get; set; } = false;

        public static List<NetworkInterfaceDeviceSelection> AllEthernetNetworkInterfaceSelections = null;
        public static List<NetworkInterfaceDeviceSelection> AllWifiNetworkInterfaceSelections = null;

        //private static readonly object changeStateLock = new object();

        NetworkInterface networkInterfaceInstance = null;

        public NetworkInterfaceDeviceSelection()
        {

        }

        public NetworkInterfaceDeviceSelection(ManagementObject managementObject, NetworkInterfaceType interfaceType, bool doNotAutoDiscard = false)
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

        public static void LoadAllNetworkInterfaceSelections(Settings settingsRef = null)
        {
            Logger.Info("Loading all Adapter selections ...");

            (AllEthernetNetworkInterfaceSelections, AllWifiNetworkInterfaceSelections) =
                GetEthernetAndWifiTypeNetworkInterfaces(settingsRef);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != typeof(NetworkInterfaceDeviceSelection))
                return false;

            NetworkInterfaceDeviceSelection otherSelection = (NetworkInterfaceDeviceSelection)obj;

            if (this.windowsDeviceId != otherSelection.windowsDeviceId)
                return false;

            if (this.deviceName != otherSelection.deviceName)
                return false;

            if (this.physicalAddress != otherSelection.physicalAddress)
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
                MainAppContext.ChangeIDBeingProcessed, (networkInterfaceType == NetworkInterfaceType.Ethernet ? "E" : "W"));
            LoadCurrentStatusValues(GetNetAdapterManagementObject());
        }

        public bool ChangeStateIfNeeded(InterfaceChangeNeeded changeNeeded)
        {
            Logger.Trace("{changeID}-{adapterType}-{changeType} :: Determing if change is needed ...",
                MainAppContext.ChangeIDBeingProcessed, (networkInterfaceType == NetworkInterfaceType.Ethernet ? "E" : "W"),
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
                MainAppContext.ChangeIDBeingProcessed, (networkInterfaceType == NetworkInterfaceType.Ethernet ? "E" : "W"),
                changeNeeded.ToString());
                        doTheChange = true;
                    }
                    else if (changeNeeded == InterfaceChangeNeeded.Disable && CurrentState > InterfaceState.Disabled)
                    {
                        Logger.Trace("{changeID}-{adapterType}-{changeType} :: Need to perform change ...",
                MainAppContext.ChangeIDBeingProcessed, (networkInterfaceType == NetworkInterfaceType.Ethernet ? "E" : "W"),
                changeNeeded.ToString());
                        doTheChange = true;
                    }
                }
                else
                    throw new Exception("ERROR hb2e86 :: Device not connected so cannot change state.");

                if (doTheChange)
                {
                    Logger.Trace("{changeID}-{adapterType}-{changeType} :: Doing change ...",
                        MainAppContext.ChangeIDBeingProcessed, (networkInterfaceType == NetworkInterfaceType.Ethernet ? "E" : "W"),
                changeNeeded.ToString());
                    
                    //InterfaceState previousState = CurrentState;

                    MainAppContext.AppInstance.StopNetworkChangeMonitoring();

                    Logger.Trace("{changeID}-{adapterType}-{changeType} :: Executing change command ...",
                        MainAppContext.ChangeIDBeingProcessed, (networkInterfaceType == NetworkInterfaceType.Ethernet ? "E" : "W"),
                changeNeeded.ToString());

                    ProcessStartInfo psi = new ProcessStartInfo("netsh", "interface set interface \"" + name + "\" " + (changeNeeded == InterfaceChangeNeeded.Enable ? "enable" : "disable"));

                    using (Process p = new Process())
                    {
                        p.StartInfo = psi;
                        p.Start();
                        p.WaitForExit();
                    }

                    

                    MainAppContext.AppInstance.StartNetworkChangeMonitoring();

                    

                    /*

                    if (changeNeeded == InterfaceChangeNeeded.Enable)
                        DoDelayedStatusUpdate(previousState);

                    */

                    Logger.Trace("{changeID}-{adapterType}-{changeType} :: Change done",
                        MainAppContext.ChangeIDBeingProcessed, (networkInterfaceType == NetworkInterfaceType.Ethernet ? "E" : "W"),
                changeNeeded.ToString());

                    RefreshCurrentStatus();
                }
                else
                {
                    Logger.Trace("{changeID}-{adapterType}-{changeType} :: Proposed change is actually NOT needed ...",
                        MainAppContext.ChangeIDBeingProcessed, (networkInterfaceType == NetworkInterfaceType.Ethernet ? "E" : "W"),
                changeNeeded.ToString());
                }
            }

            return doTheChange;
        }

        private ManagementObject GetNetAdapterManagementObject()
        {
            Logger.Trace("{changeID}-{adapterType} :: Getting Management object ...",
                MainAppContext.ChangeIDBeingProcessed, (networkInterfaceType == NetworkInterfaceType.Ethernet ? "E" : "W"));

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
                MainAppContext.ChangeIDBeingProcessed, (networkInterfaceType == NetworkInterfaceType.Ethernet ? "E" : "W"));
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

        private void LoadCurrentStatusValues(ManagementObject managementObject)
        {
            //TODO
            Logger.Trace("{changeID}-{adapterType} :: Loading current state values ...",
                MainAppContext.ChangeIDBeingProcessed, (networkInterfaceType == NetworkInterfaceType.Ethernet ? "E" : "W"));

            if (managementObject != null)
            {
                //Check if device is enabled:
                if (Int32.Parse(managementObject[managementObjectTagForCheckingIfNetworkInterfaceIsEnabled].ToString()) == 1)
                {
                    Logger.Trace("{changeID}-{adapterType} :: Device is enabled ...",
                MainAppContext.ChangeIDBeingProcessed, (networkInterfaceType == NetworkInterfaceType.Ethernet ? "E" : "W"));
                    //Check if device has network connectivity:
                    if (Int32.Parse(managementObject[managementObjectTagForCheckingIfThereIsNetworkConnectivity].ToString()) == 1)
                    {
                        Logger.Trace("{changeID}-{adapterType} :: Device has network connectivity ...",
                MainAppContext.ChangeIDBeingProcessed, (networkInterfaceType == NetworkInterfaceType.Ethernet ? "E" : "W"));
                        CurrentState = InterfaceState.HasNetworkConnectivity;
                    }
                    else
                    {
                        Logger.Trace("{changeID}-{adapterType} :: Device has no network connectivity ...",
                MainAppContext.ChangeIDBeingProcessed, (networkInterfaceType == NetworkInterfaceType.Ethernet ? "E" : "W"));
                        CurrentState = InterfaceState.EnabledButNoNetworkConnectivity;
                    }
                }
                else
                {
                    Logger.Trace("{changeID}-{adapterType} :: Device is disabled ...",
                MainAppContext.ChangeIDBeingProcessed, (networkInterfaceType == NetworkInterfaceType.Ethernet ? "E" : "W"));
                    CurrentState = InterfaceState.Disabled;
                }
            }
            else
            {
                Logger.Trace("{changeID}-{adapterType} :: Device is physically disconnected ...",
                MainAppContext.ChangeIDBeingProcessed, (networkInterfaceType == NetworkInterfaceType.Ethernet ? "E" : "W"));
                CurrentState = InterfaceState.DevicePhysicallyDisconnected;
            }
        }

        static (List<NetworkInterfaceDeviceSelection>, List<NetworkInterfaceDeviceSelection>) GetEthernetAndWifiTypeNetworkInterfaces(Settings settingsRef)
        {
            List<NetworkInterfaceDeviceSelection> ethernetOptions = new List<NetworkInterfaceDeviceSelection>();
            List<NetworkInterfaceDeviceSelection> wifiOptions = new List<NetworkInterfaceDeviceSelection>();

            using (var searcher = new ManagementObjectSearcher("root\\StandardCimv2", $@"select * from MSFT_NetAdapter where ConnectorPresent=True"))
            {
                using (var managementObjectCollection = searcher.Get())
                {
                    foreach (var managementObject in managementObjectCollection)
                    {
                        if (Int32.Parse(managementObject[managementObjectTagForCheckingInterfaceType].ToString())
                            == ethernetInterfaceTypeId)
                            ethernetOptions.Add(new NetworkInterfaceDeviceSelection((ManagementObject)managementObject, NetworkInterfaceType.Ethernet));
                        else if (Int32.Parse(managementObject[managementObjectTagForCheckingInterfaceType].ToString())
                            == wifiInterfaceTypeId)
                            wifiOptions.Add(new NetworkInterfaceDeviceSelection((ManagementObject)managementObject, NetworkInterfaceType.Wireless80211));
                    }
                }
            }

            FindPreviousInterfaceIfAnyAndAddIfNeeded(ethernetOptions, settingsRef, NetworkInterfaceType.Ethernet);
            FindPreviousInterfaceIfAnyAndAddIfNeeded(wifiOptions, settingsRef, NetworkInterfaceType.Wireless80211);

            return (ethernetOptions, wifiOptions);
        }

        private static void FindPreviousInterfaceIfAnyAndAddIfNeeded(List<NetworkInterfaceDeviceSelection> existingDevices, Settings settingsRef, NetworkInterfaceType interfaceType)
        {
            if (settingsRef != null
                && ((interfaceType == NetworkInterfaceType.Ethernet && settingsRef.EthernetInterfaceSelection != null)
                || (interfaceType == NetworkInterfaceType.Wireless80211 && settingsRef.WifiInterfaceSelection != null)))
            {
                NetworkInterfaceDeviceSelection deviceToConsider;

                if (interfaceType == NetworkInterfaceType.Ethernet)
                    deviceToConsider = settingsRef.EthernetInterfaceSelection;
                else if (interfaceType == NetworkInterfaceType.Wireless80211)
                    deviceToConsider = settingsRef.WifiInterfaceSelection;
                else
                    throw new Exception("ERROR as2xj5 :: Invalid network interface type specfied.");

                if (deviceToConsider != null)
                {
                    int index;
                    if ((index = existingDevices.IndexOf(deviceToConsider)) != -1)
                    {
                        existingDevices.ElementAt(index).DoNotAutoDiscard = deviceToConsider.DoNotAutoDiscard;
                        if (interfaceType == NetworkInterfaceType.Ethernet)
                            settingsRef.EthernetInterfaceSelection = existingDevices.ElementAt(index);
                        else
                            settingsRef.WifiInterfaceSelection = existingDevices.ElementAt(index);
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
                                settingsRef.EthernetInterfaceSelection = null;
                            else
                                settingsRef.WifiInterfaceSelection = null;
                        }
                    }
                }
            }
        }

        private void DoDelayedStatusUpdate(InterfaceState previousState)
        {
            Logger.Trace("{changeID}-{adapterType} :: Initiating delayed status update ...",
                MainAppContext.ChangeIDBeingProcessed, (networkInterfaceType == NetworkInterfaceType.Ethernet ? "E" : "W"));
            int numberOfRetriesToGetUpdatedStatus = 3;
            int timeToWaitBetweenRetriesInMilliseconds = 4000;

            for(int i = 0; i < numberOfRetriesToGetUpdatedStatus; i++)
            {
                RefreshCurrentStatus();
                if (CurrentState != previousState)
                    break;
                Thread.Sleep(timeToWaitBetweenRetriesInMilliseconds);
            }

            if (CurrentState != previousState)
            {
                Logger.Trace("{changeID}-{adapterType} :: Change detected - Previous: {previousState}, Current: {newState} ...",
                MainAppContext.ChangeIDBeingProcessed, (networkInterfaceType == NetworkInterfaceType.Ethernet ? "E" : "W"),
                previousState.ToString(), CurrentState.ToString());
                //MessageBox.Show("Change Detected.");
            }
            else
            {
                Logger.Trace("{changeID}-{adapterType} :: Change NOT detected ...",
                MainAppContext.ChangeIDBeingProcessed, (networkInterfaceType == NetworkInterfaceType.Ethernet ? "E" : "W"));
                //MessageBox.Show("Change NOT detected!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            //MainAppContext.AppInstance.StartNetworkChangeMonitoring();
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
