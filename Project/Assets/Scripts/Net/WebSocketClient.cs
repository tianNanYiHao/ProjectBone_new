using System.Net.WebSockets;
using System.Threading;
using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

public class WebSocketClient : SingletonManager<WebSocketClient>,IGeneric
{
    private ClientWebSocket _webSocket = new ClientWebSocket();
    
    public async Task ConnectAsync(string uri)
    {
        await _webSocket.ConnectAsync(new Uri(uri), CancellationToken.None);
        Debug.Log("Connected to WebSocket server.");
    }


}
