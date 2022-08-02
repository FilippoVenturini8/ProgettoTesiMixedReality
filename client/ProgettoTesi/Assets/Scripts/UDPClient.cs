 
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
    public int sendingPort;
    public int receivingPort;
   
    IPEndPoint remoteEndPoint;
    UdpClient client;
    UdpClient receiveClient;

    Thread receiveThread;

    public string lastReceivedUDPPacket="";
    public string allReceivedUDPPackets="";

    private GameObject cube;

    private float lastRotation;
    private float rotationReceived;

    void Start()
    {      
        cube = GameObject.Find("Cube");

        IP="127.0.0.1";
        sendingPort = 10000;
        receivingPort=8051;
       
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), sendingPort);
        client = new UdpClient();

        receiveThread = new Thread(
            new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    void Update()
    {
        if(lastRotation != rotationReceived)
        {
            cube.transform.Rotate(rotationReceived,0,0);
            lastRotation = rotationReceived;
        }
        //Vector3 cubePosition = GameObject.Find("Cube").transform.position;
        //sendString(cubePosition.ToString());
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

    private  void ReceiveData()
    {
        receiveClient = new UdpClient(receivingPort);
        while (true)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = receiveClient.Receive(ref anyIP);
                
                string text = Encoding.UTF8.GetString(data);
 
                print(">> " + text);

                rotationReceived = float.Parse(text);
               
                lastReceivedUDPPacket=text;
               
                allReceivedUDPPackets=allReceivedUDPPackets+text;
            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        }
    }
   
    public string getLatestUDPPacket()
    {
        allReceivedUDPPackets="";
        return lastReceivedUDPPacket;
    }
}
 
 