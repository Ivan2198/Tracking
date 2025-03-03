// WebSocketClient.cs
using UnityEngine;
using System;
using NativeWebSocket;

public class WebSocketClient : MonoBehaviour
{
    WebSocket websocket;

    async void Start()
    {
        websocket = new WebSocket("ws://localhost:8765");

        websocket.OnMessage += (bytes) =>
        {
            // Get the message as string
            var message = System.Text.Encoding.UTF8.GetString(bytes);

            // Parse the JSON data
            DataPacket data = JsonUtility.FromJson<DataPacket>(message);

            // Use the coordinates
            Debug.Log($"Hand position: x={data.x}, y={data.y}");

            // You can use these coordinates to update something in your Unity scene
            // For example: transform.position = new Vector3(data.x, data.y, 0);
        };

        websocket.OnError += (e) =>
        {
            Debug.LogError($"Error: {e}");
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed");
        };

        websocket.OnOpen += () =>
        {
            Debug.Log("Connection open");
        };

        // Connect to the server
        await websocket.Connect();
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket.DispatchMessageQueue();
#endif
    }

    private async void OnApplicationQuit()
    {
        await websocket.Close();
    }
}

[Serializable]
public class DataPacket
{
    public float x;
    public float y;
}
