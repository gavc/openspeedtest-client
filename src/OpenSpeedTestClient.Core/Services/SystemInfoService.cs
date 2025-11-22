using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace OpenSpeedTestClient.Core.Services;

public class SystemInfoService
{
    public string GetComputerName()
    {
        return Environment.MachineName;
    }

    public string GetLocalIPAddress()
    {
        try
        {
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var ni in networkInterfaces)
            {
                if (ni.OperationalStatus == OperationalStatus.Up &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    var properties = ni.GetIPProperties();
                    foreach (var ip in properties.UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            return ip.Address.ToString();
                        }
                    }
                }
            }
        }
        catch
        {
            // Fallback if network detection fails
        }

        return "Unknown";
    }

    public string GetConnectionType()
    {
        try
        {
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var ni in networkInterfaces)
            {
                if (ni.OperationalStatus == OperationalStatus.Up &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    return ni.NetworkInterfaceType switch
                    {
                        NetworkInterfaceType.Ethernet => "Ethernet",
                        NetworkInterfaceType.Ethernet3Megabit => "Ethernet",
                        NetworkInterfaceType.FastEthernetT => "Ethernet",
                        NetworkInterfaceType.FastEthernetFx => "Ethernet",
                        NetworkInterfaceType.GigabitEthernet => "Ethernet",
                        NetworkInterfaceType.Wireless80211 => "WiFi",
                        NetworkInterfaceType.Wwanpp => "4G",
                        NetworkInterfaceType.Wwanpp2 => "4G",
                        _ => "LAN"
                    };
                }
            }
        }
        catch
        {
            // Fallback if detection fails
        }

        return "Unknown";
    }
}
