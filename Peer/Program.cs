using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

var successful = Encoding.UTF8.GetBytes("successful");
var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

Console.Write("enter STUN server endpoint: ");
var stunEndPoint = IPEndPoint.Parse(Console.ReadLine() ?? "");

Console.Write("enter port number: ");
var portNumber = Convert.ToInt32(Console.ReadLine());
using var stunUdpClient = new UdpClient(portNumber);

Console.Write("> add ");
var groupName = Console.ReadLine();
var addCommandData = $"add {groupName}";
var addCommandDataBytes = Encoding.UTF8.GetBytes(addCommandData);
stunUdpClient.Send(addCommandDataBytes, stunEndPoint);
var addCommandResponseBytes = stunUdpClient.Receive(ref remoteEndPoint);

if (!addCommandResponseBytes.SequenceEqual(successful))
{
    Console.WriteLine("an error occurred while connecting to STUN server");
    return 1;
}
Console.WriteLine("connected to STUN server");

var (peerIpAddress, sourcePort, destinationPort) = ReadPeerInfo();
Console.WriteLine("got peer data:");
Console.WriteLine($"  IP Address:       {peerIpAddress}");
Console.WriteLine($"  Source Port:      {sourcePort}");
Console.WriteLine($"  Destination Port: {destinationPort}");

using var listenerUdpClient = new UdpClient(sourcePort);
using var transmitterUdpClient = new UdpClient(destinationPort);
var peerSourceEndPoint = new IPEndPoint(peerIpAddress, sourcePort);
var peerDestinationEndPoint = new IPEndPoint(peerIpAddress, destinationPort);

Console.WriteLine("punching UDP hole...");
listenerUdpClient.Send(Array.Empty<byte>(), peerDestinationEndPoint);

var listenerThread = new Thread(Listen);
listenerThread.Start();

while (true)
{
    Console.Write("> ");
    var message = Console.ReadLine();
    if (!string.IsNullOrEmpty(message))
    {
        var messageDataBytes = Encoding.UTF8.GetBytes(message);
        transmitterUdpClient.Send(messageDataBytes, peerSourceEndPoint);
    }
}

void Listen()
{
    while (true)
    {
        var dataBytes = listenerUdpClient.Receive(ref remoteEndPoint);
        var data = Encoding.UTF8.GetString(dataBytes);
        Console.WriteLine($"peer: {data}");
    }
}

(IPAddress, int, int) ReadPeerInfo()
{
    var stunPeerInfoDataBytes = stunUdpClient.Receive(ref remoteEndPoint);
    var dataStringArray = Encoding.UTF8.GetString(stunPeerInfoDataBytes).Split(" ");
    var ipAddress = IPAddress.Parse(dataStringArray[0]);
    var sourcePort = Convert.ToInt32(dataStringArray[1]);
    var destinationPort = Convert.ToInt32(dataStringArray[2]);

    return (ipAddress, sourcePort, destinationPort);
}
