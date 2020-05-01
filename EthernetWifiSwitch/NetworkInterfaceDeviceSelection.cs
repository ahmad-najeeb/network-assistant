using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.Serialization;

namespace EthernetWifiSwitch
{
    [DataContract]
    public class NetworkInterfaceDeviceSelection
    {
        [DataMember(Name = "doNotAutoDiscard")]
        public bool DoNotAutoDiscard { get; set; } = false;

        [DataMember]
        string physicalAddress;

        [DataMember]
        readonly string name;

        [DataMember]
        string description;

        public static List<NetworkInterfaceDeviceSelection> AllEthernetNetworkInterfaceSelections = null;
        public static List<NetworkInterfaceDeviceSelection> AllWifiNetworkInterfaceSelections = null;

        public bool IsActualNetworkInterface = false;
        

        NetworkInterface networkInterfaceInstance = null;

        //public OperationalStatus OperationalStatus { get; set; }

        public NetworkInterfaceDeviceSelection()
        {

        }

        

        public NetworkInterfaceDeviceSelection(NetworkInterface nic, bool doNotAutoDiscard = false)
        {
            this.DoNotAutoDiscard = doNotAutoDiscard;
            networkInterfaceInstance = nic;
            name = nic.Name;
            description = nic.Description;
            physicalAddress = networkInterfaceInstance.GetPhysicalAddress().ToString();
            IsActualNetworkInterface = true;
        }

        public override string ToString()
        {
            if (IsActualNetworkInterface)
                return $"Device: {name} | Description: {description} | MAC: {physicalAddress} | Status: {networkInterfaceInstance.OperationalStatus.ToString()}";
            else
                return $"Disconnected Device: {name} | Description: {description} | MAC: {physicalAddress}";
            //return $"Device not currently connected with MAC: {physicalAddress}";
        }

        public static void LoadAllNetworkInterfaceSelections(Settings settingsRef = null)
        {
            AllEthernetNetworkInterfaceSelections = GetInterfaceSelections(NetworkInterfaceType.Ethernet, settingsRef);
            AllWifiNetworkInterfaceSelections = GetInterfaceSelections(NetworkInterfaceType.Wireless80211, settingsRef);
        }

        /*

        public static List<NetworkInterfaceDeviceSelection> GetAttachedEthernetNetworkInterfaceSelections(
            NetworkInterfaceDeviceSelection previousSelection = null)
        {
            return GetInterfaceSelections(NetworkInterfaceType.Ethernet, previousSelection);
        }

        public static List<NetworkInterfaceDeviceSelection> GetAttachedWifiNetworkInterfaceSelections(
            NetworkInterfaceDeviceSelection previousSelection = null)
        {
            return GetInterfaceSelections(NetworkInterfaceType.Wireless80211, previousSelection);
        }

        */

        private static List<NetworkInterfaceDeviceSelection> GetInterfaceSelections(NetworkInterfaceType interfaceType, Settings settingsRef = null)
        {
            var all = NetworkInterface.GetAllNetworkInterfaces();
            List<NetworkInterfaceDeviceSelection> list = all.Where(x => (x.NetworkInterfaceType == interfaceType))
                .Select(x => new NetworkInterfaceDeviceSelection(x))
                .ToList();

            if (settingsRef != null)
            {
                NetworkInterfaceDeviceSelection previousSelection = interfaceType == NetworkInterfaceType.Ethernet ? settingsRef.EthernetInterfaceSelection : settingsRef.WifiInterfaceSelection;

                if (previousSelection != null)
                {
                    int index;
                    if ((index = list.IndexOf(previousSelection)) != -1)
                    {
                        list.ElementAt(index).DoNotAutoDiscard = previousSelection.DoNotAutoDiscard;
                        if (interfaceType == NetworkInterfaceType.Ethernet)
                            settingsRef.EthernetInterfaceSelection = list.ElementAt(index);
                        else
                            settingsRef.WifiInterfaceSelection = list.ElementAt(index);
                    }
                    else
                    {
                        if (previousSelection.DoNotAutoDiscard == true)
                        {
                            previousSelection.IsActualNetworkInterface = false;
                            list.Add(previousSelection);
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

            return list;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != typeof(NetworkInterfaceDeviceSelection))
                return false;

            NetworkInterfaceDeviceSelection otherSelection = (NetworkInterfaceDeviceSelection)obj;

            if (this.physicalAddress != otherSelection.physicalAddress)
                return false;

            return true;

            /*
            if (obj == null)
                return false;

            if (obj.GetType() != typeof(NetworkInterfaceDeviceSelection))
                return false;

            NetworkInterfaceDeviceSelection otherSelection = (NetworkInterfaceDeviceSelection)obj;

            if (this.DoNotAutoDiscard != otherSelection.DoNotAutoDiscard)
                return false;

            if (this.isActualNetworkInterface != otherSelection.isActualNetworkInterface)
                return false;

            if (this.isActualNetworkInterface && this.name != otherSelection.name)
                return false;

            if (this.physicalAddress != otherSelection.physicalAddress)
                return false;

            return true;

            */
        }

        public override int GetHashCode()
        {
            return this.physicalAddress.GetHashCode();
        }

        public void ChangeState(bool enable)
        {
            ProcessStartInfo psi = new ProcessStartInfo("netsh", "interface set interface \"" + name + "\" " + (enable ? "enable" : "disable"));
            Process p = new Process();
            p.StartInfo = psi;
            p.Start();

            /*

            if (enable && networkInterfaceInstance.OperationalStatus == OperationalStatus.Down)
            {
                throw new Exception("Unable to bring interface UP! Current status: " + networkInterfaceInstance.OperationalStatus.ToString());
            }
            else if (!enable && networkInterfaceInstance.OperationalStatus == OperationalStatus.Up)
            {
                throw new Exception("Unable to bring interface Down! Current status: " + networkInterfaceInstance.OperationalStatus.ToString());
            }

            */
        }

        /*

        public OperationalStatus? GetOperationalStatus()
        {
            if (IsActualNetworkInterface)
                return networkInterfaceInstance.OperationalStatus;
            else
                return null;
        }

        */

        public static bool IsOnline(NetworkInterfaceDeviceSelection networkInterfaceDeviceSelection)
        {
            if (networkInterfaceDeviceSelection.IsActualNetworkInterface
                && networkInterfaceDeviceSelection.networkInterfaceInstance.OperationalStatus == OperationalStatus.Up)
                return true;
            else
                return false;
        }
    }
}
