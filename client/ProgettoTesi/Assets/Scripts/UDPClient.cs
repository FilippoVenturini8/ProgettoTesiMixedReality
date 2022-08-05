using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net;
using System.Collections.Generic;
using UnityEngine.Events;
#if UNITY_EDITOR
using System.Net.Sockets;
using System.Threading;
#else
using Windows.Networking.Sockets;
using Windows.Networking.Connectivity;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using System.IO;
using Windows.Networking;
#endif


#if UNITY_EDITOR

public class UDPClient : MonoBehaviour, IDisposable
{
    //OnMessageReceived
    public delegate void AddOnMessageReceivedDelegate(string message, IPEndPoint remoteEndpoint);
    public event AddOnMessageReceivedDelegate MessageReceived;
    private void OnMessageReceivedEvent(string message, IPEndPoint remoteEndpoint)
    {
        if (MessageReceived != null)
            MessageReceived(message, remoteEndpoint);
    }

    private Thread _ReadThread;
    private UdpClient _Socket;

    private GameObject cube;
    private GameObject debugGameObj;
    private TextMeshProUGUI debugConsole;
    private float lastRotation;
    private float rotationReceived;

    private bool newLog = false;
    private string logMsg;


    void Start()
    {
        Receive(8051);
        cube = GameObject.Find("Cube");
        debugGameObj = GameObject.Find("DebugTxt");
        debugConsole = debugGameObj.GetComponent<TextMeshProUGUI>();
        debugConsole.text = "Your Text";
    }

    void Update()
    {
        if(lastRotation != rotationReceived)
        {
            if(newLog){
                debugConsole.text = logMsg;
                newLog = false;
            }
            
            cube.transform.Rotate(rotationReceived,0,0);
            lastRotation = rotationReceived;
        }
    }


    public void Receive(int port)
    {
        // create thread for reading UDP messages
        _ReadThread = new Thread(new ThreadStart(delegate
        {
            try
            {
                _Socket = new UdpClient(port);
                Debug.LogFormat("Receiving on port {0}", port);
            }
            catch (Exception err)
            {
                Debug.LogError(err.ToString());
                return;
            }
            while (true)
            {
                try
                {
                    // receive bytes
                    IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = _Socket.Receive(ref anyIP);

                    // encode UTF8-coded bytes to text format
                    string message = Encoding.UTF8.GetString(data);
                    
                    NewLog(message);

                    rotationReceived = float.Parse(message);
                    OnMessageReceivedEvent(message, anyIP);
                }
                catch (Exception err)
                {
                    Debug.LogError(err.ToString());
                }
            }
        }));
        _ReadThread.IsBackground = true;
        _ReadThread.Start();
    }

    public void Dispose()
    {
        if (_ReadThread.IsAlive)
        {
            _ReadThread.Abort();
        }
        if (_Socket != null)
        {
            _Socket.Close();
            _Socket = null;
        }
    }

    private void NewLog(string msg){
        newLog = true;
        logMsg = msg;
    }
}

#else

[System.Serializable]
public class UDPMessageEvent : UnityEvent<string, string, byte[]>
{

}

public class UDPClient : MonoBehaviour, Singleton<UDPCommunication>
{
   [Tooltip ("port to listen for incoming data")]
    public string internalPort = "8051";

    [Tooltip("IP-Address for sending")]
    public string externalIP = "192.168.17.110";

    [Tooltip("Port for sending")]
    public string externalPort = "12346";

    [Tooltip("Send a message at Startup")]
    public bool sendPingAtStart = true;

    [Tooltip("Conten of Ping")]
    public string PingMessage = "hello";

    [Tooltip("Function to invoke at incoming packet")]
    public UDPMessageEvent udpEvent = null;

    private readonly  Queue<Action> ExecuteOnMainThread = new Queue<Action>();

    //we've got a message (data[]) from (host) in case of not assigned an event
    void UDPMessageReceived(string host, string port, byte[] data)
    {
        Debug.Log("GOT MESSAGE FROM: " + host + " on port " + port + " " + data.Length.ToString() + " bytes ");
    }

    //Send an UDP-Packet
    public async void SendUDPMessage(string HostIP, string HostPort, byte[] data)
    {
        await _SendUDPMessage(HostIP, HostPort, data);
    }

    DatagramSocket socket;

    async void Start()
    {
        if (udpEvent == null)
        {
            udpEvent = new UDPMessageEvent();
            udpEvent.AddListener(UDPMessageReceived);
        }

        NewLog("Waiting for a connection...");
        Debug.Log("Waiting for a connection...");

        socket = new DatagramSocket();
        socket.MessageReceived += Socket_MessageReceived;

        HostName IP = null;
        try
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();

            IP = Windows.Networking.Connectivity.NetworkInformation.GetHostNames()
            .SingleOrDefault(
                hn =>
                    hn.IPInformation?.NetworkAdapter != null && hn.IPInformation.NetworkAdapter.NetworkAdapterId
                    == icp.NetworkAdapter.NetworkAdapterId);

            await socket.BindEndpointAsync(IP, internalPort);
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
            Debug.Log(SocketError.GetStatus(e.HResult).ToString());
            return;
        }

        if(sendPingAtStart)
            SendUDPMessage(externalIP, externalPort, Encoding.UTF8.GetBytes(PingMessage));

    }

    private async System.Threading.Tasks.Task _SendUDPMessage(string externalIP, string externalPort, byte[] data)
    {
        using (var stream = await socket.GetOutputStreamAsync(new Windows.Networking.HostName(externalIP), externalPort))
        {
            using (var writer = new Windows.Storage.Streams.DataWriter(stream))
            {
                writer.WriteBytes(data);
                await writer.StoreAsync();

            }
        }
    }

    static MemoryStream ToMemoryStream(Stream input)
    {
        try
        {                                         // Read and write in
            byte[] block = new byte[0x1000];       // blocks of 4K.
            MemoryStream ms = new MemoryStream();
            while (true)
            {
                int bytesRead = input.Read(block, 0, block.Length);
                if (bytesRead == 0) return ms;
                ms.Write(block, 0, bytesRead);
            }
        }
        finally { }
    }

    // Update is called once per frame
    void Update()
    {
        while (ExecuteOnMainThread.Count > 0)
        {
            ExecuteOnMainThread.Dequeue().Invoke();

        }
    }

    private void Socket_MessageReceived(Windows.Networking.Sockets.DatagramSocket sender,
        Windows.Networking.Sockets.DatagramSocketMessageReceivedEventArgs args)
    {
         Debug.Log("GOT MESSAGE FROM: " + args.RemoteAddress.DisplayName);
        //Read the message that was received from the UDP  client.
        Stream streamIn = args.GetDataStream().AsStreamForRead();
        MemoryStream ms = ToMemoryStream(streamIn);
        byte[] msgData = ms.ToArray();

        if (ExecuteOnMainThread.Count == 0)
        {
            ExecuteOnMainThread.Enqueue(() =>
            {
                Debug.Log("ENQEUED ");
                if (udpEvent != null)
                    udpEvent.Invoke(args.RemoteAddress.DisplayName, internalPort, msgData);
            });
        }
    }

    private void NewLog(string msg){
        newLog = true;
        logMsg = msg;
    }

}

#endif

