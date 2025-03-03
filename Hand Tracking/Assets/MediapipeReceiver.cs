using UnityEngine;
using NativeWebSocket;
using System;
using System.Text;
using PimDeWitte.UnityMainThreadDispatcher;

public class MediapipeReceiver : MonoBehaviour
{
    WebSocket websocket;
    public GameObject targetObject; // Assign this in the Unity Inspector
    public bool enableDebugging = true; // Toggle debugging messages in Inspector

    // Store the position data
    private Vector3 newPosition;
    private bool positionUpdated = false;

    async void Start()
    {
        try
        {
            DebugLog("Initializing WebSocket...");
            websocket = new WebSocket("ws://localhost:8765");

            websocket.OnMessage += (bytes) =>
            {
                string message = Encoding.UTF8.GetString(bytes);
                DebugLog($"Received message: {message}");

                try
                {
                    HandPosition data = JsonUtility.FromJson<HandPosition>(message);

                    // Convert normalized coordinates (0-1) to Unity world space
                    float newX = Mathf.Lerp(-5f, 5f, data.x);
                    float newY = Mathf.Lerp(-3f, 3f, 1 - data.y); // Flip Y-axis

                    DebugLog($"Converted Position -> X: {newX}, Y: {newY}");

                    // Set the position to be updated in Update()
                    newPosition = new Vector3(newX, newY, targetObject ? targetObject.transform.position.z : 0);
                    positionUpdated = true;
                }
                catch (Exception e)
                {
                    DebugLog($"JSON Parsing Error: {e.Message}");
                }
            };

            websocket.OnOpen += () => DebugLog("WebSocket Connection Open");
            websocket.OnError += (e) => DebugLog($"WebSocket Error: {e}");
            websocket.OnClose += (e) => DebugLog("WebSocket Closed");

            await websocket.Connect();
        }
        catch (Exception e)
        {
            DebugLog($"Error initializing WebSocket: {e.Message}");
        }
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        if (websocket != null)
        {
            websocket.DispatchMessageQueue();
        }

        if (positionUpdated && targetObject != null)
        {
            targetObject.transform.position = newPosition;
            DebugLog($"Updated Object Position: {targetObject.transform.position}");
            positionUpdated = false;
        }
#endif
    }

    private async void OnApplicationQuit()
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.Close();
        }
    }

    void DebugLog(string message)
    {
        if (enableDebugging)
        {
            Debug.Log($"[MediapipeReceiver] {message}");
        }
    }
}

[Serializable]
public class HandPosition
{
    public float x;
    public float y;
}