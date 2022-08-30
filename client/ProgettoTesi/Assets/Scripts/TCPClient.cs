using System;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions; 

public class TCPClient : MonoBehaviour
{
    #region private members 	
	private TcpClient socketConnection; 	
	private Thread clientReceiveThread; 

    private GameObject debugGameObj;
    private TextMeshProUGUI debugConsole;

	private List<GameObject> cubes = new List<GameObject>();
	private UpdateMessage[] setupMessages;
	private UpdateMessage[] updateMessages;
	private Queue<UpdateMessage[]> queue = new Queue<UpdateMessage[]>();

	private bool update = false;
	private bool create = false;

    private bool newLog = false;
    private string logMsg;	

	private bool firstMsg = true;
	#endregion 

    void Start()
    {
        debugGameObj = GameObject.Find("DebugTxt");
        debugConsole = debugGameObj.GetComponent<TextMeshProUGUI>();
        ConnectToTcpServer();
    }

    void Update()
    {
        if(newLog){
            debugConsole.text = logMsg;
            newLog = false;
        }

		if(create)
		{
			for(int i = 0; i < setupMessages.Length; i++){
				GameObject newCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
				string [] positionXYZ = setupMessages[i].position.Split('|');
				string [] scaleXYZ = setupMessages[i].scale.Split('|');
				newCube.transform.position = new Vector3(float.Parse(positionXYZ[0]), float.Parse(positionXYZ[1]), float.Parse(positionXYZ[2]));
				newCube.transform.localScale = new Vector3(float.Parse(scaleXYZ[0]), float.Parse(scaleXYZ[1]), float.Parse(scaleXYZ[2]));
				cubes.Add(newCube);
			}
			create = false;
		}

		if(update)
        {   
			updateMessages = queue.Dequeue();			
			for(int i = 0; i < updateMessages.Length; i++){
				int id = updateMessages[i].cubeID;
				string [] rotationXYZ = updateMessages[i].rotation.Split('|');
				cubes[id].transform.eulerAngles = new Vector3(
					float.Parse(rotationXYZ[0]),
					float.Parse(rotationXYZ[1]),
					float.Parse(rotationXYZ[2])
				);
			}
			SendAcknowledge();
            update = false;
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
			NewLog("On client connect exception " + e); 		
		} 	
	} 

    private void ListenForData() 
    { 		
		try { 			
			socketConnection = new TcpClient("192.168.40.100", 10000);  			
			Byte[] bytes = new Byte[1024];             
			while (true) { 								
				using (NetworkStream stream = socketConnection.GetStream()) { 					
					int length; 					

					while ((length = stream.Read(bytes, 0, bytes.Length)) != 0) { 						
						var incommingData = new byte[length]; 						
						Array.Copy(bytes, 0, incommingData, 0, length); 

						string msgString = Encoding.ASCII.GetString(incommingData); 
						int count = Regex.Matches(msgString, "Items").Count;
						NewLog(count + " " + msgString);
						if(count != 1){
							continue;
						}

						if(firstMsg){
							setupMessages = JsonHelper.FromJson<UpdateMessage>(msgString);
							create = true;
							firstMsg = false;
						}else{	
							update = true;
							queue.Enqueue(JsonHelper.FromJson<UpdateMessage>(msgString));
						}				
					} 				
				} 			
			}         
		}         
		catch (SocketException socketException) {             
			NewLog("Socket exception: " + socketException);         
		}     
	}

    private void SendAcknowledge() 
    {         
		if (socketConnection == null) {             
			return;         
		}  		
		try { 					
			NetworkStream stream = socketConnection.GetStream(); 			
			if (stream.CanWrite) {                 
				string clientMessage = "ACK"; 
				byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage); 
				stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);        
			}         
		} 		
		catch (SocketException socketException) {             
			NewLog("Socket exception: " + socketException);         
		}     
	}

    private void NewLog(string msg){
        newLog = true;
        logMsg = msg;
    }
}

[System.Serializable]
public class UpdateMessage
{
	public int cubeID;
	public string position;
    public string rotation;
	public string scale;

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
