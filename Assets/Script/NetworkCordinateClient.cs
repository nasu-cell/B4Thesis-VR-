using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class NetworkCoordinateClient : MonoBehaviour
{
    [Header("サーバー設定")]
    public string serverIP = "127.0.0.1";
    public int port = 5000;
    
    [Header("受信座標データ")]
    public float cordinate_L = 0f;
    public float cordinate_R = 0f;

    [Header("ステータス")]
    public bool isConnected = false;
    [Tooltip("マーカーが正常に認識されているか")]
    public bool isMarkerVisible = false; 

    private TcpClient client;
    private NetworkStream stream;
    private CancellationTokenSource cancellationTokenSource;
    private Task receiveTask;
    private readonly object lockObject = new object();
    private string incompleteMessage = "";

    void Start()
    {
        cancellationTokenSource = new CancellationTokenSource();
        receiveTask = Task.Run(() => StartClient(cancellationTokenSource.Token));
    }

    private async Task StartClient(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                if (token.IsCancellationRequested) break;
                client = new TcpClient();
                await client.ConnectAsync(serverIP, port);
                
                stream = client.GetStream();
                isConnected = true; 
                
                byte[] buffer = new byte[1024];
                while (client.Connected && !token.IsCancellationRequested)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token);
                    if (bytesRead > 0)
                    {
                        string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        ProcessReceivedData(data);
                    }
                    else if (bytesRead == 0) throw new SocketException();
                }
            }
            catch (Exception)
            {
                isConnected = false;
            }
            finally
            {
                if (stream != null) stream.Dispose();
                if (client != null) client.Close();
                if (!token.IsCancellationRequested) await Task.Delay(2000, token); 
            }
        }
    }

    private void ProcessReceivedData(string data)
    {
        string message = incompleteMessage + data;
        string[] packets = message.Split('\n');
        incompleteMessage = packets[packets.Length - 1];

        for (int i = 0; i < packets.Length - 1; i++)
        {
            ParseCoordinates(packets[i]);
        }
    }

    private void ParseCoordinates(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) 
        {
            isMarkerVisible = false;
            return;
        }

        string[] parts = line.Split(',');
        if (parts.Length == 2)
        {
            if (float.TryParse(parts[0], out float yL) && float.TryParse(parts[1], out float yR))
            {
                lock (lockObject) 
                {
                    cordinate_L = yL;
                    cordinate_R = yR;

                    // マーカーが(0,0)を送ってきている場合はロストとみなす
                    // お使いのPython側の仕様に合わせてここを -1.0 等に変更してください
                    if (yL == 0 && yR == 0) isMarkerVisible = false;
                    else isMarkerVisible = true;
                }
            }
        }
    }

    void OnDisable()
    {
        if (cancellationTokenSource != null) cancellationTokenSource.Cancel();
        if (stream != null) stream.Dispose();
        if (client != null) client.Close();
    }
}