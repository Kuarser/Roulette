using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;


public class ServerWS : MonoBehaviour
{
    private WebSockets.WebSocketServer mServer;

    //主机相关
    private UdpClient mUdpClient = null; //主机和分机的udpclient
    private IPEndPoint endPoint;
    ServerInfo serverinfo;

    private bool isStop = false;
    private Thread rcvThread = null;
    private string curLocalIP = "";
    private int mBroadcastPort;
    private readonly object udpLock = new object();
    private bool renewingIp;

    private bool needRenewIp = false;

    public void StartServer(int port, int broadcastPort)
    {
        serverinfo = new ServerInfo();
        serverinfo.IP = Utils.LocalIP();
        serverinfo.port = port;
        StartUdp(broadcastPort);
        InitSocket(port);
    }

    public void StopServer()
    {
        if (mServer != null)
        {
            mServer.Stop();
            mServer = null;
        }
    }

    protected void StartUdp(int broadcastPort)
    {
        mBroadcastPort = broadcastPort;
        curLocalIP = Utils.LocalIP();
        mUdpClient = new UdpClient(new IPEndPoint(IPAddress.Any, broadcastPort));
        endPoint = new IPEndPoint(IPAddress.Any, 0);
        isStop = false;
        rcvThread = new Thread(new ThreadStart(ReciveUdpMsg))
        {
            IsBackground = true
        };
        rcvThread.Start();
    }

    public void InitSocket(int port)
    {
        StopServer();
        mServer = new WebSockets.WebSocketServer(IPAddress.Any, port);
        mServer.OnClientConnected += OnClientConnected;
        mServer.Start();
    }

    private void ReciveUdpMsg()
    {
        while (!isStop && mUdpClient != null)
        {
            byte[] buf = mUdpClient.Receive(ref endPoint);
            if (buf != null)
            {
                string msg = Encoding.UTF8.GetString(buf);
                Debug.LogWarning($"ReciveUdpMsg:{msg}");
                if (!string.IsNullOrEmpty(msg))
                {
                    ServerInfo srvInfo = new ServerInfo
                    {
                        IP = Utils.LocalIP(),
                        port = serverinfo.port
                    };
                    SendUdpMsg(JsonConvert.SerializeObject(srvInfo), endPoint);
                }
            }
            Thread.Sleep(500);
        }
    }

    //使用udp发送消息
    public void SendUdpMsg(string strMsg, IPEndPoint endPoint)
    {
        if (renewingIp || needRenewIp || !Utils.HasLocalIP()) return;
            try
            {
                string localIp = Utils.LocalIP();
                if (mUdpClient != null && curLocalIP == localIp)
                {
                    byte[] bf = Encoding.UTF8.GetBytes(strMsg);

                    mUdpClient.Send(bf, bf.Length, endPoint);
                }
                else if (curLocalIP != localIp)
                {
                    lock (udpLock)
                    {
                        // 停止旧线程
                        isStop = true;
                        if (rcvThread != null && rcvThread.IsAlive)
                        {
                            rcvThread.Abort();
                            rcvThread = null;
                        }

                        // 关闭旧的 UdpClient
                        if (mUdpClient != null)
                        {
                            mUdpClient.Close();
                            mUdpClient = null;
                        }

                        // 获取新 IP 并重建 UdpClient
                        curLocalIP = Utils.LocalIP();
                        mUdpClient = new UdpClient(new IPEndPoint(IPAddress.Any, mBroadcastPort));
                        endPoint = new IPEndPoint(IPAddress.Any, 0);
                        isStop = false;

                        // 启动新线程
                        rcvThread = new Thread(ReciveUdpMsg)
                        {
                            IsBackground = true
                        };
                        rcvThread.Start();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"SendUpdMsg Error:{e.Message}");
            }
    }

    private void OnClientConnected(WebSockets.ClientConnection client)
    {
        client.ReceivedTextualData += OnReceivedTextualData;
        client.Disconnected += OnClientDisconnected;
        client.StartReceiving();
        Debug.Log(string.Format("Client {0} Connected...", client.Id));
    }

    private void OnClientDisconnected(WebSockets.ClientConnection client)
    {
        client.ReceivedTextualData -= OnReceivedTextualData;
        client.Disconnected -= OnClientDisconnected;
        Debug.Log(string.Format("Client {0} Disconnected...", client.Id));
        EventCenter.Instance.EventTrigger(EventHandle.PLAYER_DISCONNECT, client);
    }

    private void OnReceivedTextualData(WebSockets.ClientConnection client, string data)
    {
        WSSrvMsgData wmd = new WSSrvMsgData
        {
            Client = client,
            Data = data
        };
        Loom.QueueOnMainThread((wmd) =>
        {
            Messenger.Broadcast<WSSrvMsgData>(MessageName.Event_NetworkWSServerData, (WSSrvMsgData)wmd);
            wmd = null;
        }, wmd);
    }

    public void SendToClient(WebSockets.ClientConnection client, string msg)
    {
        client?.Send(msg);
    }

    public void SendToAllClient(string msg)
    {
        if (mServer != null)
        {
            mServer.SendToAllClient(msg);
        }
    }

    private void OnDestroy()
    {
        isStop = true;
        if (rcvThread != null)
        {
            rcvThread.Abort();
            rcvThread = null;
        }
        if (mUdpClient != null)
        {
            mUdpClient.Close();
            mUdpClient = null;
        }
        StopServer();
    }
}
