using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NativeWebSocket;

/// <summary> This class calls the NativeWebSocket library. It connects, pings OBS, and then "crashes" by quitting it's connection with OBS. </summary>
public class OBSWebSocketWatcher : MonoBehaviour
{
    int socketLimit = 2; // I have no idea why would you need 16 streams but here it is. 
    List<WebSocket> obsSockets = new List<WebSocket>();
    bool attemptingConnection = false;
    /// <summary> Calls Init() which initalizes obsSockets with null values up to socketLimit. </summary>
    public OBSWebSocketWatcher ()
    {
        Init();
    }

    /// <summary>
    /// This is a websocket service that could be expanded in the future to include different OBS services. Currently it creates a list of address pointers that are empty for now.
    /// </summary>
    private void Init()
    {
        //Empty padding. 
        for (int i = 0; i < socketLimit; i++)
        {
            obsSockets.Add(null);
        }
    }

    public WebSocketStateThread webSocketStatus = WebSocketStateThread.Disconnected;
    private void Awake()
    {
        Init();
    }

    /// <summary> Defines various states of WebSocket connection.  </summary>
    public enum WebSocketStateThread
    {
        Connecting,
        Disconnected,
        Connected,
        PriorConnection
    }


    /// <summary> Initalizes WebSocket connection. </summary>
    public void TestOBSConnection()
    {
        StartConnection();
    }

    /// <summary> Attempts to establish a WebSocket connetion, and loops unitl a connection is successful or a prior one is detected. </summary>

    public IEnumerator TryConnecting()
    {
        webSocketStatus = WebSocketStateThread.Disconnected;
        StartConnection();
        while (webSocketStatus != WebSocketStateThread.PriorConnection)
        {
            if (attemptingConnection == false)
                StartConnection();
            yield return null;
        }
    }

    /// <summary> Initalies an asynchronous WebSocket connection to OBS. </summary>
    public async void StartConnection(int indexReference = 0, string address = "ws://localhost:4455")
    {
        webSocketStatus = WebSocketStateThread.Connecting;
        obsSockets[indexReference] = new WebSocket(address);
        var currentSocket = obsSockets[indexReference];

        currentSocket.OnOpen += () =>
        {
            webSocketStatus = WebSocketStateThread.Connected;
            SendWebsocketMessage(0, "", 1); // Just close the websocket after connection, we don't need it as we cannot
            // fix the connections issues with this package yet. 
            //SendWebsocketMessage(0, "", 2);
            //SendWebsocketMessage(0, "", 3);
            //Debug.Log("Connection open!");
            //obsSockets[0].Close(); 
            currentSocket.Close();
            attemptingConnection = false; 
        };

        currentSocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
            attemptingConnection = false;
        };

        currentSocket.OnClose += (e) =>
        {
            webSocketStatus = WebSocketStateThread.PriorConnection;
            Debug.Log("Connection closed!");
            attemptingConnection = false;
        };

        currentSocket.OnMessage += (bytes) =>
        {
            Debug.Log("OnMessage!");
            Debug.Log(bytes);

            // getting the message as a string
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log("OnMessage! " + message);
            attemptingConnection = false;
        };
        //// Keep sending messages at every 0.3s
        //InvokeRepeating("SendWebsocketMessage", 0.0f, 0.3f);

        // waiting for messages
        attemptingConnection = true;
        await currentSocket.Connect();
    }

    /// <summary> Update is called once per frame. Checks and processes and message queue if the WebSocket is open. </summary>
    void Update()
    {
    #if !UNITY_WEBGL || UNITY_EDITOR
        if (obsSockets[0] != null)
            if(obsSockets[0].State == WebSocketState.Open)
                obsSockets[0].DispatchMessageQueue();
    #endif
    }

    /// <summary> Sends a test message to OBS. </summary>
    public void SendTestMessage()
    {

        //var outMsg = buildOBSMessage("GetSourcesList");
        SendWebsocketMessage(0, "GetSourcesList", 6);
    }

    /// <summary> Sends a message over WebSocket based on operation type.  </summary>
    private async void SendWebsocketMessage(int index, string message,int opType)
    {
        if (obsSockets[index].State == WebSocketState.Open)
        {
            //await obsSockets[index].Send(System.Text.Encoding.UTF8.GetBytes(message));
            //await obsSockets[index].Send(new byte[1]);
            // Sending plain text
            //"{\"op\":7,\"d\":{\"requestType\": \"GetSourcesList\",\"requestId\":\"2\"}}"

            if(opType == 1)
            {
                await obsSockets[index].SendText("{\"op\":1,\"d\":{\"rpcVersion\": \"1\"}}");
            }
            else if (opType == 2)
            {
                await obsSockets[index].SendText("{\"op\":2,\"d\":{\"negotiatedRpcVersion\": \"1\"}}");
            }
            else if (opType == 3)
            {
                await obsSockets[index].SendText("{\"op\":3,\"d\":{\"eventSubscriptions\": \"1\"}}"); 
            }
            else if (opType == 6)
            {
                await obsSockets[index].SendText("{\"op\":6,\"d\":{\"requestType\": \""+ message+" \",\"requestId\":\"2\"}}");
                
            }
            
        }
    }
    int messageNumber = 0;
    /// <summary> Builds a message string for OBS. </summary>
    private string buildOBSMessage(string requestType, (string, string)[] parameters = null)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append(@"{""request-type"":""");
        sb.Append(requestType);
        sb.Append(@""",");
        if (parameters != null)
        {
            foreach (var p in parameters)
            {
                /*
                if (int.TryParse(p.Item2, out int iVal))
                {
                    sb.Append($"\"{p.Item1}\":{iVal},");
                }

                else if (bool.TryParse(p.Item2, out bool bVal))
                {
                    sb.Append($"\"{p.Item1}\":{bVal},");
                }
                else
                {
                    sb.Append($"\"{p.Item1}\":\"{p.Item2}\",");
                }
                */
                sb.Append($"\"{p.Item1}\":\"{p.Item2}\",");
            }
        }
        sb.Append(@"""message-id"":""XRT-OBSRemote-");
        sb.Append(++messageNumber);
        sb.Append(@"""}");
        return sb.ToString();
    }

    /// <summary> Ensures WebSocket is closed when application quits. </summary>
    private async void OnApplicationQuit()
    {
        if (obsSockets[0] != null)
            await obsSockets[0].Close();
    }
}

