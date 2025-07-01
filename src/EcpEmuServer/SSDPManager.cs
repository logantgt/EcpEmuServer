using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EcpEmuServer
{
    public class SSDPManager
    {
        public static void StartSSDP()
        {
            try
            {
                IPEndPoint multicastEndpoint = new IPEndPoint(IPAddress.Parse("239.255.255.250"), 1900);
                IPEndPoint localEndpoint = new IPEndPoint(IPAddress.Any, 1900);

                UdpClient udpServer = new UdpClient();
                UdpClient udpClient = new UdpClient();

                udpServer.Client.Bind(localEndpoint);

                udpServer.JoinMulticastGroup(multicastEndpoint.Address);

                udpClient.Connect(multicastEndpoint);

                Logger.Log(Logger.LogSeverity.warn, $"SSDP Multicasting with address {GetLocalIPAddress()} (if this is not your preferred interface, provide an IPv4 address as first arg)");

                while (true)
                {
                    byte[] data = udpServer.Receive(ref multicastEndpoint);
                    string res = Encoding.ASCII.GetString(data);
                    if (res.Contains("M-SEARCH"))
                    {
                        if (res.Contains("ssdp:all") || res.Contains("upnp:rootdevice"))
                        {
                            Thread.Sleep(100);
                            udpClient.Send(SSDPMessages.ecpDeviceNotifyMessage, SSDPMessages.ecpDeviceNotifyMessage.Length);
                            udpServer.Send(SSDPMessages.ecpDeviceOKMessage, SSDPMessages.ecpDeviceOKMessage.Length, multicastEndpoint);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(Logger.LogSeverity.error, ex.Message);
            }
        }
        public static string GetLocalIPAddress()
        {
            if (Program.UserProvidedEndpoint != "") return Program.UserProvidedEndpoint;
            
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    // Don't run on local loopback
                    if (!ip.ToString().Contains("127.0."))
                    {
                        return ip.ToString();
                    }
                }
            }
            throw new Exception("No network adapters with an IPv4 address available!");
        }
    }

    public static class SSDPMessages
    {
        public static string GetSSDPName()
        {
            if (!File.Exists("./devicename"))
            {
                Logger.Log(Logger.LogSeverity.warn, "devicename was not found, generating default \"EcpEmuServer\"...");

                using (StreamWriter writer = new StreamWriter(File.Create("./devicename")))
                {
                    // Default configuration
                    writer.Write("EcpEmuServer");
                }

                Logger.Log(Logger.LogSeverity.info, "Generated new devicename file");
            }

            return File.ReadAllText("./devicename");
        }

        public static readonly byte[] ecpDeviceNotifyMessage = Encoding.ASCII.GetBytes(
            $"NOTIFY * HTTP/1.1\r\n" +
            $"Host: 239.255.255.250:1900\r\n" +
            $"Cache-Control: max-age=3600\r\n" +
            $"NT: roku:ecp\r\n" +
            $"NTS: ssdp:alive\r\n" +
            $"Location: http://{SSDPManager.GetLocalIPAddress()}:8060/\r\n" +
            $"USN: uuid:roku:ecp:{Dns.GetHostName()}\r\n\r\n");

        public static readonly byte[] ecpDeviceOKMessage = Encoding.ASCII.GetBytes(
            $"HTTP/1.1 200 OK\r\n" +
            $"ST: roku:ecp\r\n" +
            $"USN: uuid:roku:ecp:{Dns.GetHostName()}::roku:ecp\r\n" +
            $"Cache-Control: max-age=3600\r\n" +
            $"SERVER: Server: Roku/9.3.0 UPnp/1.0 Roku/9.3.0\r\n" +
            $"Location: http://{SSDPManager.GetLocalIPAddress()}:8060/\r\n\r\n");

        public static readonly string ecpDeviceRootResponse =
            $"<root>" +
            $"<device>" +
            $"<friendlyName>{GetSSDPName()}</friendlyName>" +
            $"<manufacturer>TCL</manufacturer>" +
            $"<manufacturerURL>http://www.github.com/logantgt/EcpEmuServer</manufacturerURL>" +
            $"<modelName>7108X</modelName>" +
            $"<serialNumber>{Dns.GetHostName()}</serialNumber>" +
            $"<UDN>uuid:{Dns.GetHostName()}</UDN>" +
            $"</device>" +
            $"</root>";
    }
}