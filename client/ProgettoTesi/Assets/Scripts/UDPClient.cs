using System.Text;
using TMPro;
using UnityEngine;
using System;
using System.Net;
#if UNITY_EDITOR
using System.Net.Sockets;
using System.Threading;
#else
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using System.IO;
using Windows.Networking;
#endif
 
 
 
public class UDPClient : MonoBehaviour, IDisposable
{
    public static UDPClient instance;
 
    //OnMessageReceived
    public delegate void AddOnMessageReceivedDelegate(string message, IPEndPoint remoteEndpoint);
    public event AddOnMessageReceivedDelegate MessageReceived;
    private void OnMessageReceivedEvent(string message, IPEndPoint remoteEndpoint)
    {
 
        if (MessageReceived != null)
        {
            MessageReceived(message, remoteEndpoint);
 
        }
    }
 
#if UNITY_EDITOR
 
    private Thread _ReadThread;
    private UdpClient _Socket;
 
#else
 
    DatagramSocket _Socket = null;
 
#endif
 
    #region COMMON
    string messageReceived = "";
    public TMP_Text txtLog;
    private bool isReceiving = false;

    private GameObject cube;
    private float lastRotation;
    private float rotationReceived;
 
    private void Awake()
    {
        instance = this;
    }
 
    void Start()
    {
        Receive(8051);
        cube = GameObject.Find("Cube");
        InvokeRepeating("OnBtn_FreeMemory", 2, 5);
    }
    
    void Update()
    {
        rotationReceived = float.Parse(messageReceived);
        //print(messageReceived);
        if(lastRotation != rotationReceived)
        {
            cube.transform.Rotate(rotationReceived,0,0);
            lastRotation = rotationReceived;
        }
        //txtLog.text = messageReceived;
    }
    #endregion
 
#if UNITY_EDITOR
 
    public void Receive(int port)
    {
        if (isReceiving) return;
        isReceiving = true;
 
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
                    OnMessageReceivedEvent(message, anyIP);
                    messageReceived = message;
 
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
        isReceiving = false;
    }
 
 
#else
 
    public async void Receive(int port)
    {
        if (isReceiving) return;
 
        isReceiving = true;
   
        string portStr = port.ToString();
        // start the client
        try
        {
            _Socket = new DatagramSocket();
            _Socket.MessageReceived += _Socket_MessageReceived;
 
            await _Socket.BindServiceNameAsync(portStr);
 
            //await _Socket.BindEndpointAsync(null, portStr);
 
            //await _Socket.ConnectAsync(new HostName("255.255.255.255"), portStr.ToString());
 
 
            //HostName hostname = Windows.Networking.Connectivity.NetworkInformation.GetHostNames().FirstOrDefault();
            //var ep = new EndpointPair(hostname, portStr, new HostName("255.255.255.255"), portStr);
            //await _Client.ConnectAsync(ep);
 
            Debug.Log(string.Format("Receiving on {0}", portStr));
 
            await Task.Delay(3000);
            // send out a message, otherwise receiving does not work ?!
            var outputStream = await _Socket.GetOutputStreamAsync(new HostName("255.255.255.255"), portStr);
            DataWriter writer = new DataWriter(outputStream);
            writer.WriteString("Hello World!");
            await writer.StoreAsync();
        }
        catch (Exception ex)
        {
            _Socket.Dispose();
            _Socket = null;
            Debug.LogError(ex.ToString());
            Debug.LogError(Windows.Networking.Sockets.SocketError.GetStatus(ex.HResult).ToString());
        }
    }
 
    private async void _Socket_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
    {
        try
        {
            Stream streamIn = args.GetDataStream().AsStreamForRead();
            StreamReader reader = new StreamReader(streamIn, Encoding.UTF8);
 
            string message = await reader.ReadLineAsync();
            IPEndPoint remoteEndpoint = new IPEndPoint(IPAddress.Parse(args.RemoteAddress.RawName), Convert.ToInt32(args.RemotePort));
            OnMessageReceivedEvent(message, remoteEndpoint);
            messageReceived = message;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }
 
    public void Dispose()
    {
        if (_Socket != null)
        {
            _Socket.Dispose();
            _Socket = null;
        }
        isReceiving = false;
    }
#endif
 
    public void OnBtn_FreeMemory()
    {
        GC.Collect();
        Resources.UnloadUnusedAssets();
    }
    public void OnBtn_StartSocket()
    {
        Receive(9999);
    }
    public void OnBtn_CloseSocket()
    {
        Dispose();
    }
 
    private void OnDisable()
    {
        Dispose();
    }
 
}