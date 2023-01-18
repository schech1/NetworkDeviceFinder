using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Linq;


class Program
{

    public static List<IPAddress> devices = new List<IPAddress>();


    static void Main()
    {
        string myIP = GetLocalIPAddress();
        string[] ipPart = myIP.Split(".");

        //  var host = Dns.GetHostEntry(Dns.GetHostName());
        //  var localIP = host.AddressList[0];


        for (int i = 1; i < 255; i++)
        {
            // var address = IPAddress.Parse(localIP.ToString().Substring(0, localIP.ToString().LastIndexOf(".") + 1) + i.ToString());

            string fullIp = ipPart[0] + "." + ipPart[1] + "." + ipPart[2] + "." + i;

            try
            {
                if (ForcePingTimeoutWithThreads(IPAddress.Parse(fullIp), 25) != null)  // Adjust timeout if not all devices are found
                {
                    Console.WriteLine("found: " + fullIp + ": "); //+ GetHostName(fullIp)                    
                    devices.Add(IPAddress.Parse(fullIp));
                }
            }
            catch (SocketException ex)

            {
                Console.WriteLine(ex);
            }
        }

        foreach (var device in devices)
        {
            try
            {
                IPHostEntry hostInfo = System.Net.Dns.GetHostEntry(device.ToString());
                string hostName = hostInfo.HostName;

                // var hostname = GetHostName(device.ToString());   // Super slow
                Console.WriteLine($"{device} ({hostName})");
            }
            catch (SocketException)
            {
                Console.WriteLine($"{device} (Unable to resolve hostname)");
            }
        }
        // Console.ReadLine();
    }


    private static PingReply ForcePingTimeoutWithThreads(IPAddress ipadress, int timeout)
    {
        PingReply reply = null;
        var a = new Thread(() => reply = normalPing(ipadress, timeout));
        a.Start();
        a.Join(timeout);
        return reply;
    }

    private static PingReply normalPing(IPAddress ipadress, int timeout)
    {
        try
        {
            return new Ping().Send(ipadress, timeout);
        }
        catch
        {
            return null;
        }
    }

    public static string GetHostName(string ipAddress)
    {
        try
        {
            IPHostEntry entry = Dns.GetHostEntry(ipAddress);
            if (entry != null)
            {
                return entry.HostName;
            }
        }
        catch (SocketException ex)
        {
            Console.WriteLine(ex);
            return "Hostname not found";
        }
        return null;
    }


    public static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) { return value; }

        return value.Substring(0, Math.Min(value.Length, maxLength));
    }

    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new Exception("No network adapters with an IPv4 address found");
    }

    // Test only
    public static void GetSubnetMask()
    {
        foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
        {
            foreach (UnicastIPAddressInformation unicastIPAddressInformation in adapter.GetIPProperties().UnicastAddresses)
            {
                if (unicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    Console.WriteLine(unicastIPAddressInformation.IPv4Mask);
                }
            }
        }
    }

}
