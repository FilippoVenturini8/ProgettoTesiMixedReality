 
using UnityEngine;
using System.Collections;
 
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
 
public class UDPClient : MonoBehaviour
{
    private static int localPort;
   
    private string IP;
    public int port;
   
    IPEndPoint remoteEndPoint;
    UdpClient client;
   
    /*// call it from shell (as program)
    private static void Main()
    {
        UDPSend sendObj=new UDPSend();

        sendObj.init();
        sendObj.sendEndless(" endless infos \n");
    }*/

    void Start()
    {
        print("UDPSend.init()");
       
        IP="127.0.0.1";
        port=10000;
       
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);
        client = new UdpClient();
       
        print("Sending to "+IP+" : "+port);
        print("Testing: nc -lu "+IP+" : "+port);
    }

    void Update()
    {
        sendString("Prova UDP");
    }
 
    private void sendString(string message)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
 
            client.Send(data, data.Length, remoteEndPoint);
        }
        catch (Exception err)
        {
            print(err.ToString());
        }
    }
   
    private void sendEndless(string testStr)
    {
        do
        {
            sendString(testStr);           
        }
        while(true);
       
    }
   
}
 
 