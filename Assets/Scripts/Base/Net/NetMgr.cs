using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;


public class NetMgr : MonoSingleton<NetMgr>
{
    private readonly int port = 33653;
    public int serverBroadcastPort = 10157;
    public int clientBroadcastPort = 10999;

    //WebSocket
    ServerWS serverWS;
    ClientWS clientWS;

    // 轮盘模式没有分机：主机服务端不启动，所有 S2C 广播直接丢弃。
    // 默认为 true，保持原九门流程的双屏/分机行为。
    public bool HostServerEnabled = true;

    //每条连接最后一次收到消息的时间，使用 realtimeSinceStartup 避免 timeScale/暂停影响存活判断
    private readonly Dictionary<WebSockets.ClientConnection, float> lastSeen
        = new Dictionary<WebSockets.ClientConnection, float>();

    public NET_STATUS CurNetStatus
    {
        get
        {
            if (clientWS == null)
                return NET_STATUS.NET_STATUS_DISCONNECTED;
            return clientWS.CurNetStatus;
        }
    }


    private void Awake()
    {
        serverWS = this.transform.GetComponent<ServerWS>();
        clientWS = this.transform.GetComponent<ClientWS>();

        Messenger.AddListener<WSSrvMsgData>(MessageName.Event_NetworkWSServerData, OnWSServerData);
        Messenger.AddListener<byte[]>(MessageName.Event_NetworkClientData, OnClientData);
    }


    public void SetLastHeartHeat()
    {
        if (clientWS != null)
            clientWS.LastHeartHeatTime = Time.time;
    }

    void OnClientData(byte[] data)
    {
        Messenger.Broadcast<byte[]>(MessageName.Event_ClientNetworkRecv, data);
    }

    public void SetNetAutoConnect(bool Host)
    {
        if (Host)
        {
            // 无分机时不启动主机服务端，避免监听端口与向分机广播。
            if (!HostServerEnabled) return;
            if (serverWS == null)
                serverWS = gameObject.AddComponent<ServerWS>();
            serverWS.StartServer(port, serverBroadcastPort);
        }
        else
        {
            if (clientWS == null)
                clientWS = gameObject.AddComponent<ClientWS>();
            clientWS.StartUdp(clientBroadcastPort);
        }
    }

    //客户端发送数据给主机
    public void SendToServer(string strMsg)
    {
        clientWS?.SendToServer(strMsg);
    }

    //主机发送数据给客户端
    public void SendToClient(WebSockets.ClientConnection client,string strMsg)
    {
        if (!HostServerEnabled) return;
        serverWS?.SendToClient(client, strMsg);
    }

    //主机向所有客户端发送消息
    public void SendToAllClient(string strMsg)
    {
        if (!HostServerEnabled) return;
        serverWS?.SendToAllClient(strMsg);
    }

    private int lastSyncedNetJPState = -1;
    /// <summary>主机联网彩金状态变化时,直接同步给所有分机(不依赖UI刷新)</summary>
    public void SyncNetJackpotState()
    {
        int state = IOCanvasModel.Instance.JackpotCfg.jackpotSwitch == (int)IOJackpotSwitch.JackpotSwitchOn
            ? (CurNetStatus == NET_STATUS.NET_STATUS_CONNECTED ? 2 : 1)
            : 0;
        if (state == lastSyncedNetJPState) return;//只有状态真的变了才下发
        lastSyncedNetJPState = state;
        SendToAllClient(JsonConvert.SerializeObject(new MsgInfo
        {
            cmd = (int)S2C_CMD.S2C_ChangeNetJPState,
            id = -1,
            jsonData = state.ToString(),
        }));
    }

    //����WS�������յ�����Ϣ
    void OnWSServerData(WSSrvMsgData data)
    {
        if (data.Data.Length == 0)
            return;
        string singlePacket = data.Data;
        MsgInfo info = null;
        try
        {
            info = JsonConvert.DeserializeObject<MsgInfo>(singlePacket);
        }
        catch (System.Exception ex)
        {
            Debug.Log("JSON : " + ex.Message);
        }

        if (info != null)
        {
            //任何成功解析的消息都刷新活跃时间，而不仅是心跳
            lastSeen[data.Client] = Time.realtimeSinceStartup;

            switch ((C2S_CMD)info.cmd)
            {
                case C2S_CMD.C2S_HeartHeat:
                    info.cmd = (int)S2C_CMD.S2C_HeartHeat;
                    info.id = info.id;
                    SendToClient(data.Client, JsonConvert.SerializeObject(info));
                    break;
                default:
                    Messenger.Broadcast(MessageName.Event_ClientNetworkLogic, info, data.Client);
                    break;
            }
        }
    }

    /// <summary>判断分机连接在指定时间内是否仍有消息（用于识别半开连接）</summary>
    public bool IsClientAlive(WebSockets.ClientConnection client, float timeout = 15f)
    {
        if (client == null || !client.IsConnected)
            return false;

        float last;
        if (!lastSeen.TryGetValue(client, out last))
            return false;
        return Time.realtimeSinceStartup - last <= timeout;
    }

    /// <summary>移除分机连接的最后活跃记录，断线时由 PlayerMgr 调用</summary>
    public void RemoveClient(WebSockets.ClientConnection client)
    {
        if (client != null)
            lastSeen.Remove(client);
    }

    public void CloseSocket()
    {
        clientWS.CloseSocket();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Messenger.RemoveListener<WSSrvMsgData>(MessageName.Event_NetworkWSServerData, OnWSServerData);
        Messenger.RemoveListener<byte[]>(MessageName.Event_NetworkClientData, OnClientData);
    }
}