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
/*
#if !UNITY_EDITOR
public class UDPClient : MonoBehaviour
{

    private string ClientPortNumber = "1336";
    private string ServerPortNumber = "10000";

    private GameObject debugGameObj;
    private TextMeshProUGUI debugConsole;

    private bool newLog = false;
    private string logMsg;

    // Start is called before the first frame update
    void Start()
    {
        debugGameObj = GameObject.Find("DebugTxt");
        debugConsole = debugGameObj.GetComponent<TextMeshProUGUI>();
        StartClient();
    }

    // Update is called once per frame
    void Update()
    {
        if(newLog){
            debugConsole.text += " " + logMsg;
            newLog = false;
        }
    }

    private async void StartClient()
    {
        try
        {
            // Create the DatagramSocket and establish a connection to the echo server.
            var clientDatagramSocket = new Windows.Networking.Sockets.DatagramSocket();

            clientDatagramSocket.MessageReceived += ClientDatagramSocket_MessageReceived;

            // The server hostname that we will be establishing a connection to. In this example, the server and client are in the same process.
            var hostName = new Windows.Networking.HostName("192.168.40.100");

            //NewLog("client is about to bind...");

            await clientDatagramSocket.BindServiceNameAsync(ClientPortNumber);

            //NewLog(string.Format("client is bound to port number {0}", ClientPortNumber));

            // Send a request to the echo server.
            string request = "Hello, World!";
            using (var serverDatagramSocket = new Windows.Networking.Sockets.DatagramSocket())
            {
                using (Stream outputStream = (await serverDatagramSocket.GetOutputStreamAsync(hostName, ServerPortNumber)).AsStreamForWrite())
                {
                    using (var streamWriter = new StreamWriter(outputStream))
                    {
                        await streamWriter.WriteLineAsync(request);
                        await streamWriter.FlushAsync();
                    }
                }
            }

            //NewLog(string.Format("client sent the request: \"{0}\"", request));
        }
        catch (Exception ex)
        {
            Windows.Networking.Sockets.SocketErrorStatus webErrorStatus = Windows.Networking.Sockets.SocketError.GetStatus(ex.GetBaseException().HResult);
            print(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message);
            NewLog("ERROR!");
        }
    }

    private async void ClientDatagramSocket_MessageReceived(Windows.Networking.Sockets.DatagramSocket sender, Windows.Networking.Sockets.DatagramSocketMessageReceivedEventArgs args)
    {
        NewLog("RISPOSTA");
        string response;
        using (DataReader dataReader = args.GetDataReader())
        {
            response = dataReader.ReadString(dataReader.UnconsumedBufferLength).Trim();
            NewLog(response);
        }

        //await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => NewLog(string.Format("client received the response: \"{0}\"", response)));

        sender.Dispose();

        //await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => NewLog("client closed its socket"));
    }

    private void NewLog(string msg){
        newLog = true;
        logMsg = msg;
    }
}
#endif

*/



/*

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

public class UDPClient : MonoBehaviour
{
    private DatagramSocket socket;
    private GameObject debugGameObj;
    private TextMeshProUGUI debugConsole;

    private bool newLog = false;
    private string logMsg;

     async void Start()
    {
        Debug.Log("Waiting for a connection...");

        debugGameObj = GameObject.Find("DebugTxt");
        debugConsole = debugGameObj.GetComponent<TextMeshProUGUI>();
        debugConsole.text = "Waiting for a connection...";

        socket = new DatagramSocket();
        socket.MessageReceived += Socket_MessageReceived;
        
        try
        {
            debugConsole.text = "Prova1";
            await socket.BindEndpointAsync(new HostName("192.168.40.102"), "8051");
            debugConsole.text = "Prova2";
        }
        catch (Exception e)
        {
            NewLog(e.ToString());
            Debug.Log(e.ToString());
            Debug.Log(SocketError.GetStatus(e.HResult).ToString());
            return;
        }

        debugConsole.text = "exit start";
        Debug.Log("exit start");
    }

    // Update is called once per frame
    void Update ()
    {
        if(newLog){
            debugConsole.text = logMsg;
            newLog = false;
        }
    }

    private async void Socket_MessageReceived(Windows.Networking.Sockets.DatagramSocket sender,
        Windows.Networking.Sockets.DatagramSocketMessageReceivedEventArgs args)
    {
        //Read the message that was received from the UDP echo client.
        Stream streamIn = args.GetDataStream().AsStreamForRead();
        StreamReader reader = new StreamReader(streamIn);
        string message = await reader.ReadLineAsync();

        Debug.Log("MESSAGE: " + message);
        NewLog("MESSAGE: " + message);
    }

    private void NewLog(string msg){
        newLog = true;
        logMsg = msg;
    }
}

#endif

*/

