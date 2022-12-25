using System.Net;
using System.Net.Sockets;
using System.Text;

var successMessage = Encoding.UTF8.GetBytes("successful");
var errorMessage = Encoding.UTF8.GetBytes("error");
const int peerKnownPort = 50002;

Console.Write("enter listening port: ");
var listeningPort = Convert.ToInt32(Console.ReadLine());
using var udpClient = new UdpClient(listeningPort);
Console.WriteLine($"started listening on port {listeningPort}...");

var peersData = new Dictionary<string, IList<IPEndPoint>>();

while (true)
{
    var (endPoint, group) = ReadData();
    Console.WriteLine($"received {group} from {endPoint}");

    if (!peersData.ContainsKey(group))
    {
        peersData[group] = new List<IPEndPoint>();
    }
    peersData[group].Add(endPoint);
    udpClient.Send(successMessage, endPoint);

    if (peersData[group].Count == 2)
    {
        InformClient(peersData[group][0], peersData[group][1]);
        InformClient(peersData[group][1], peersData[group][0]);
        peersData.Remove(group);
        Console.WriteLine($"removed group {group}");
    }
}

void InformClient(IPEndPoint destinationEndPoint, IPEndPoint sourceEndPoint)
{
    var dataString = $"{sourceEndPoint.Address} {sourceEndPoint.Port} {peerKnownPort}";
    var dataBytes = Encoding.UTF8.GetBytes(dataString);
    Console.WriteLine($"sending \"{dataString}\"\n  to {destinationEndPoint}...");
    udpClient.Send(dataBytes, destinationEndPoint);
}

(IPEndPoint, string) ReadData()
{
    var endPoint = new IPEndPoint(IPAddress.Any, 0);
    var receivedData = udpClient.Receive(ref endPoint);
    var group = Encoding.UTF8.GetString(receivedData);

    return (endPoint, group);
}
