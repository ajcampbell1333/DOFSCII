using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class ReceiveKeystrokes : MonoBehaviour {

    [SerializeField]
    int port;

    public void Listen()
    {
        UdpClient listener = new UdpClient();
        IPEndPoint receiverEndPoint = new IPEndPoint(IPAddress.Parse(Network.player.ipAddress), port);

        while (true)
        {
            byte[] data = listener.Receive(ref receiverEndPoint);
            //RaiseDataReceived(new ReceivedDataArgs(receiverEndPoint.Address, receiverEndPoint.Port, data));
//            Debug.Log(string.Format("Received message from [{0}:{1}]:{2}",args.IpAddress.ToString(),args.Port.ToString(),Encoding.ASCII.GetString(DownloadDataCompletedEventArgs.ReceivedBytes));
        }
    }

    //public delegate void DataReceived(object sender, ReceivedDataArgs args);
    //public event DataReceived DataReceivedEvent;

    //private void RaiseDataReceived(ReceivedDataArgs args)
    //{
    //    if (DataReceivedEvent != null)
    //        DataReceivedEvent(this, args);
    //}



}

public class ReceivedDataArgs
{
    public IPAddress IpAddress { get; set; }
    public int Port { get; set; }
    public byte[] ReceivedBytes;

    public ReceivedDataArgs(IPAddress ip, int port, byte[] data)
    {
        this.IpAddress = ip;
        this.Port = port;
        this.ReceivedBytes = data;
    }
}