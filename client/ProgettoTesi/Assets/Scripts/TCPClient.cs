using System;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class TCPClient : MonoBehaviour
{
    #region private members 	
	private TcpClient socketConnection; 	
	private Thread clientReceiveThread; 

    private GameObject debugGameObj;
    private TextMeshProUGUI debugConsole;

	private List<GameObject> cubes = new List<GameObject>();
	private SetupMessage setupMessage;
	private UpdateMessage serverMessage;

	private UpdateMessage[] updateMessages;

	private bool rotate = false;
	private bool create = false;

    private bool newLog = false;
    private string logMsg;	

	private bool firstMsg = true;
	#endregion 

    // Start is called before the first frame update
    void Start()
    {
        debugGameObj = GameObject.Find("DebugTxt");
        debugConsole = debugGameObj.GetComponent<TextMeshProUGUI>();
        ConnectToTcpServer();
    }

    // Update is called once per frame
    void Update()
    {
        if(newLog){
            debugConsole.text = logMsg;
            newLog = false;
        }

		if(create)
		{
			for(int i=0; i < setupMessage.numberOfCube; i++)
			{
				GameObject newCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
				newCube.transform.position = new Vector3(0, 0, -(float)i);
				newCube.transform.localScale = new Vector3(setupMessage.scale, setupMessage.scale, setupMessage.scale);
				cubes.Add(newCube);
			}
			create = false;
		}

		if(rotate)
        {       
			//Ciclo sugli updateMessages e in base agli ID si modificano solo i cubi corrispondenti
			for(int i = 0; i < cubes.Count; i++){
				cubes[i].transform.Rotate(float.Parse(serverMessage.rotation),0,0);
			}
            rotate = false;
        }		
    }

    private void ConnectToTcpServer () 
    { 		
		try {  			
			clientReceiveThread = new Thread (new ThreadStart(ListenForData)); 			
			clientReceiveThread.IsBackground = true; 			
			clientReceiveThread.Start();  		
		} 		
		catch (Exception e) { 			
			Debug.Log("On client connect exception " + e); 		
		} 	
	} 

    private void ListenForData() 
    { 		
		try { 			
			socketConnection = new TcpClient("192.168.40.100", 10000);  			
			Byte[] bytes = new Byte[1024];             
			while (true) { 				
				// Get a stream object for reading 				
				using (NetworkStream stream = socketConnection.GetStream()) { 					
					int length; 					
					// Read incomming stream into byte arrary. 					
					while ((length = stream.Read(bytes, 0, bytes.Length)) != 0) { 						
						var incommingData = new byte[length]; 						
						Array.Copy(bytes, 0, incommingData, 0, length); 						
						// Convert byte array to string message. 						
						string msgString = Encoding.ASCII.GetString(incommingData); 
						NewLog(msgString);
						if(firstMsg){
							setupMessage = SetupMessage.CreateFromJSON(msgString);
							create = true;
							firstMsg = false;
						}else{
							//serverMessage = UpdateMessage.CreateFromJSON(msgString);	
							updateMessages = JsonHelper.FromJson<UpdateMessage>(msgString);
							rotate = true;
						}
						
						Debug.Log("server message received as: " + serverMessage); 					
					} 				
				} 			
			}         
		}         
		catch (SocketException socketException) {             
			Debug.Log("Socket exception: " + socketException);         
		}     
	}

    private void SendMessage() 
    {         
		if (socketConnection == null) {             
			return;         
		}  		
		try { 			
			// Get a stream object for writing. 			
			NetworkStream stream = socketConnection.GetStream(); 			
			if (stream.CanWrite) {                 
				string clientMessage = "This is a message from one of your clients."; 				
				// Convert string message to byte array.                 
				byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage); 				
				// Write byte array to socketConnection stream.                 
				stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);                 
				Debug.Log("Client sent his message - should be received by server");             
			}         
		} 		
		catch (SocketException socketException) {             
			Debug.Log("Socket exception: " + socketException);         
		}     
	}

    private void NewLog(string msg){
        newLog = true;
        logMsg = msg;
    }
}

[System.Serializable]
public class SetupMessage
{
    public float scale;
	public int numberOfCube;

    public static SetupMessage CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<SetupMessage>(jsonString);
    }
}

[System.Serializable]
public class UpdateMessage
{
	public int cubeID;
	public string position;
    public string rotation;
	public string scale;
	public string timestamp;

    public static UpdateMessage CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<UpdateMessage>(jsonString);
    }
}

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }

    public static string ToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(T[] array, bool prettyPrint)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}
