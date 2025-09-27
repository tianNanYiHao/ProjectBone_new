using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class Client :SingletonManager<Client>,IGeneric
{
    private Socket _socket;
    private NetworkStream _networkStream;

    public async Task ConnectAsync(string ip, int port)
    {
        try
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await _socket.ConnectAsync(ip, port);
            //_networkStream = new NetworkStream(_socket, FileAccess.ReadWrite, true);
            Debug.Log("Connected to server.");
         
        }
        catch (Exception e)
        {
            Debug.LogError($"Error connecting to server: {e.Message}");
        }
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(float time)
    {
        base.Update(time);
    }

    public override void Dispose()
    {
        base.Dispose();
        Disconnect();
    }
  

    


   

    public void Disconnect()
    {
        _networkStream?.Close();
        _socket?.Close();
    }
    
    public  bool IsConnected()
    {
        return _socket.Connected;
    }
}
