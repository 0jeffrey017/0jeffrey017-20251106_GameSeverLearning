using UnityEngine;
using System.Net.WebSockets;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;

public class WebSocketTest : MonoBehaviour
{
    private ClientWebSocket _websocket;
    [SerializeField] private TextMeshProUGUI _text;
    private async void Start()
    {
        _websocket = new ClientWebSocket();
        await ConnectSocketAsync();
    }
    private async void OnDestroy() //Unity 程式結束時關閉 Websocket
    {
        await DisconnectSocketAsync();
    }
    private async Task ConnectSocketAsync() //連線至伺服器的程式
    {
        if (_websocket.State == WebSocketState.Open) return;
        try
        {
            await _websocket.ConnectAsync(new Uri("ws://localhost:8080/ws"), CancellationToken.None);
            Debug.Log("Connected to " + "ws://localhost:8080/ws");
            await ReceiveAsync(); //接收到伺服器資料後的處理
        }
        catch (Exception ex)
        {
            Debug.Log("Error: " + ex.Message);
        }
    }
    private async Task DisconnectSocketAsync() //斷開連線的程式
    {
        if (_websocket.State != WebSocketState.Open) return;
        await _websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing connection", CancellationToken.None);
        Debug.Log("Disconnected to " + "ws://localhost:8080/ws");
    }
    private async Task ReceiveAsync() //處理接收資料
    {
        byte[] buffer = new byte[1024];
        while (_websocket.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result = await _websocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Text)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Debug.Log(message);
            }
        }
    }
    
    public async void SendStringAsync()
    {
        try
        {
            byte[] buffer = Encoding.UTF8.GetBytes(_text.text);
            await _websocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error sending message: " + ex.Message);
        }
    }
    
}

