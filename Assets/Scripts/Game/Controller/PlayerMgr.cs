using GameUtil;
using Newtonsoft.Json;
using SBoxApi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//分机管理
 class PlayerMgr : BaseManager<PlayerMgr>
{
    public Dictionary<int, Player> playerDic = new Dictionary<int, Player>();

    public List<SBoxIdeaPlayerOutState> SBoxIdeaPlayerOutState = new List<SBoxIdeaPlayerOutState>();

    public Dictionary<int, SBoxPlayerCoinInfo> playerCoinInfoDic = new Dictionary<int, SBoxPlayerCoinInfo>();

    private Coroutine playerCoinInfoTimer;

    //分机号冲突状态机(Option A: 第一个连接保持权威, 主连接离开后提升最早的冲突连接)
    private readonly Dictionary<int, List<WebSockets.ClientConnection>> conflictClients = new Dictionary<int, List<WebSockets.ClientConnection>>();
    private readonly Dictionary<WebSockets.ClientConnection, int> boundMacId = new Dictionary<WebSockets.ClientConnection, int>();
    private bool reconcileStarted;

    private const int CompletedOrderMaxCount = 10; //每台分机保留的已完成订单号数量
    private const string BillDoneOrderPrefsKeyPrefix = "billDoneOrder_";
    private Dictionary<int, Queue<string>> completedOrderDic = new Dictionary<int, Queue<string>>();

    public Player localPlayer
    {
        get
        {
            if (playerDic.TryGetValue(1, out Player player))
                return player;
            Debug.LogWarning($"[OutStateDebug] localPlayer: playerDic 无 macId=1, 从 SBoxIdeaPlayerOutState 列表查找, 列表 Count={SBoxIdeaPlayerOutState.Count}");
            SBoxIdeaPlayerOutState.ForEach((info) =>
            {
                if (info.PlayerId == 1)
                {
                    Debug.LogWarning($"[OutStateDebug] localPlayer fallback 命中 PlayerId=1, 新建 Player 并赋值 outState");
                    Player player = new Player
                    {
                        macId = info.PlayerId,
                        outState = info,
                        IsOnline = true,
                    };
                    playerDic[1] = player;
                }
            });
            if (!playerDic.ContainsKey(1))
            {
                Debug.LogWarning($"[OutStateDebug] localPlayer fallback 未找到 PlayerId=1 的出状态记录, outState 将保持 null");
            }
            return playerDic[1];
        }
    }

    public Player AddLocalPlayer()
    {
        if (playerDic.TryGetValue(1, out Player player))
            return player;
        else
        {
            playerDic[1] = new Player
            {
                macId = 1,
                IsOnline = true,
                //sBoxCoinInCount = SBoxSandbox.NumberOfCoinIn(0),
                //sBoxCoinOutCount = SBoxSandbox.NumberOfCoinOut(0),
            };
            var localPlayerRecords = SQLite.Instance.ReadRecordData2(1);           //本地玩家（主机）的数据库记录，玩家来了再读取DB的信息
            playerDic[1].totalDataStruct = JsonConvert.DeserializeObject<List<TotalDataStruct>>(localPlayerRecords);
            return playerDic[1];
        }
    }

    public Player GetPlayer(int macId)
    {
        if (playerDic.TryGetValue(macId, out Player player))
            return player;
        return null;
    }

    /// <summary>订单是否已完成(已加分)</summary>
    public bool IsOrderCompleted(int macId, string orderId)
    {
        return !string.IsNullOrEmpty(orderId)
            && GetOrLoadQueue(macId).Contains(orderId);
    }

    /// <summary>标记订单已完成</summary>
    public void MarkOrderCompleted(int macId, string orderId)
    {
        if (string.IsNullOrEmpty(orderId))
            return;
        Queue<string> orders = GetOrLoadQueue(macId);
        if (orders.Contains(orderId))
            return;
        orders.Enqueue(orderId);
        while (orders.Count > CompletedOrderMaxCount)
            orders.Dequeue();
        PlayerPrefs.SetString(BillDoneOrderPrefsKeyPrefix + macId, JsonConvert.SerializeObject(orders.ToArray()));
        PlayerPrefs.Save();
    }

    private Queue<string> GetOrLoadQueue(int macId)
    {
        if (completedOrderDic.TryGetValue(macId, out Queue<string> cached))
            return cached;
        Queue<string> orders = new Queue<string>();
        string json = PlayerPrefs.GetString(BillDoneOrderPrefsKeyPrefix + macId, null);
        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                List<string> list = JsonConvert.DeserializeObject<List<string>>(json);
                if (list != null)
                    foreach (string id in list)
                        orders.Enqueue(id);
            }
            catch (Exception e)
            {
                Debug.LogError($"[BillIn] 读取已完成订单缓存失败,按空处理 macId={macId}, err={e.Message}");
                orders = new Queue<string>();
            }
        }
        completedOrderDic[macId] = orders;
        return orders;
    }

    private Player CreatePlayer(MsgInfo info, WebSockets.ClientConnection client)
    {
        int macId = info.id;
        playerDic[macId] = new Player
        {
            macId = macId,
            IsOnline = true,
            client = client,
            coinInCount = PlayerPrefs.GetInt($"player{macId}coinInCount", -1),
            coinOutCount = PlayerPrefs.GetInt($"player{macId}coinOutCount", -1),
        };
        var localPlayerRecords = SQLite.Instance.ReadRecordData2(macId);           //玩家的数据库记录，玩家来了再读取DB的信息
        playerDic[macId].totalDataStruct = JsonConvert.DeserializeObject<List<TotalDataStruct>>(localPlayerRecords);
        //登录时统一在 PlayerInsert 里补发历史快照，这里不再单独发送
        //MsgInfo MsgHis = new MsgInfo//发送到分机
        //{
        //    cmd = (int)S2C_CMD.S2C_UpdateHistoryData,
        //    id = -1,
        //    jsonData = JsonConvert.SerializeObject(playerDic[macId].totalDataStruct),
        //};
        //NetMgr.Instance.SendToClient(playerDic[macId].client,JsonConvert.SerializeObject(MsgHis));
        return playerDic[macId];
    }

    /// <summary>补发一次历史快照(首次建号 + 重连都覆盖)</summary>
    private void SendHistorySnapshot(Player player, WebSockets.ClientConnection client)
    {
        MsgInfo MsgHis = new MsgInfo//发送到分机
        {
            cmd = (int)S2C_CMD.S2C_UpdateHistoryData,
            id = -1,
            jsonData = JsonConvert.SerializeObject(player.totalDataStruct),
        };
        NetMgr.Instance.SendToClient(client, JsonConvert.SerializeObject(MsgHis));
    }

    /// <summary>判断连接是否仍存活(已连接且15秒内有过消息)</summary>
    private bool IsAlive(WebSockets.ClientConnection c)
    {
        return c != null && c.IsConnected && NetMgr.Instance.IsClientAlive(c);
    }

    /// <summary>向指定分机发送冲突角色: 0=正常/解除, 1=冲突分机, 2=主连接存在竞争</summary>
    private void NotifyMacConflict(int macId, WebSockets.ClientConnection client, int role)
    {
        if (client == null)
            return;
        MsgInfo msg = new MsgInfo
        {
            cmd = (int)S2C_CMD.S2C_MacConflict,
            id = macId,
            jsonData = role.ToString(),
        };
        NetMgr.Instance.SendToClient(client, JsonConvert.SerializeObject(msg));
    }

    /// <summary>仅在首次需要时启动5秒冲突巡检</summary>
    private void StartReconcileIfNeeded()
    {
        // 轮盘模式没有分机，无需巡检分机连接冲突。
        if (Model.Instance.rouletteFlow)
            return;
        if (reconcileStarted)
            return;
        reconcileStarted = true;
        Timer.LoopAction(5f, _ => ReconcileConflicts());
    }

    /// <summary>安全写入在线状态, 避免非法分机号越界</summary>
    private void SetOnlineState(int macId, int state)
    {
        if (macId >= 1 && macId - 1 < NetModel.Instance.playerOnlineState.Count)
            NetModel.Instance.playerOnlineState[macId - 1] = state;
    }

    /// <summary>把连接加入冲突队列, 返回是否为新加入(保持加入顺序, 队首即最早冲突)</summary>
    private bool AddConflict(int macId, WebSockets.ClientConnection client)
    {
        if (!conflictClients.TryGetValue(macId, out List<WebSockets.ClientConnection> list))
        {
            list = new List<WebSockets.ClientConnection>();
            conflictClients[macId] = list;
        }
        if (list.Contains(client))
            return false;
        list.Add(client);
        return true;
    }

    /// <summary>从冲突队列移除指定连接</summary>
    private void RemoveConflict(int macId, WebSockets.ClientConnection client)
    {
        if (!conflictClients.TryGetValue(macId, out List<WebSockets.ClientConnection> list))
            return;
        list.Remove(client);
        if (list.Count == 0)
            conflictClients.Remove(macId);
    }

    /// <summary>清理死亡/半开冲突连接, 同时解除绑定与最后活跃记录, 返回是否有移除</summary>
    private bool CleanDeadConflicts(int macId)
    {
        if (!conflictClients.TryGetValue(macId, out List<WebSockets.ClientConnection> list))
            return false;
        bool removed = false;
        for (int i = list.Count - 1; i >= 0; i--)
        {
            WebSockets.ClientConnection c = list[i];
            if (IsAlive(c))
                continue;
            list.RemoveAt(i);
            removed = true;
            if (c != null)
            {
                boundMacId.Remove(c);
                NetMgr.Instance.RemoveClient(c);
            }
        }
        if (list.Count == 0)
            conflictClients.Remove(macId);
        return removed;
    }

    /// <summary>主连接不在线时把最早的存活冲突连接提升为 primary, 不断线不重连</summary>
    private void PromoteEarliestConflict(int macId, Player player)
    {
        if (!conflictClients.TryGetValue(macId, out List<WebSockets.ClientConnection> list) || list.Count == 0)
            return;
        WebSockets.ClientConnection q = list[0];
        player.client = q;
        //保持在线, 避免 IsOnline 由 true→false→true 触发多余的 SBox 状态变化
        if (!player.IsOnline)
            player.IsOnline = true;
        SetOnlineState(macId, 1);
        list.RemoveAt(0);
        if (list.Count == 0)
            conflictClients.Remove(macId);
        NotifyMacConflict(macId, q, 0);
        if (list.Count > 0)
            NotifyMacConflict(macId, q, 2);
    }

    /// <summary>分机注册; 返回 -1=拒绝, 0=主连接, 1=冲突分机, 2=主连接但存在竞争</summary>
    public int PlayerInsert(MsgInfo info, WebSockets.ClientConnection client)
    {
        //主机号(1)与超出上限的分机号不允许连接; 分机应提示后重启
        if (info == null || client == null || info.id < 2 || info.id > SBoxModel.MaxMacId)
        {
            Debug.LogError($"[MacConflict] 拒绝非法分机号 id={(info == null ? -1 : info.id)}");
            return -1;
        }

        int macId = info.id;

        //一个 ClientConnection 只能绑定一个分机号
        if (boundMacId.TryGetValue(client, out int boundId) && boundId != macId)
        {
            Debug.LogError($"[MacConflict] 连接已绑定分机号 {boundId}, 拒绝改绑 {macId}");
            return -1;
        }

        if (!playerDic.TryGetValue(macId, out Player player))
        {
            //新分机号: 绑定连接并清理可能残留的冲突记录
            boundMacId[client] = macId;
            if (conflictClients.TryGetValue(macId, out List<WebSockets.ClientConnection> staleList))
            {
                foreach (WebSockets.ClientConnection stale in staleList.ToList())
                {
                    if (stale != null && stale != client)
                    {
                        boundMacId.Remove(stale);
                        NetMgr.Instance.RemoveClient(stale);
                    }
                }
                conflictClients.Remove(macId);
            }
            player = CreatePlayer(info, client);
            SetOnlineState(macId, 1);
            StartReconcileIfNeeded();
            SendHistorySnapshot(player, client);
            EventCenter.Instance.EventTrigger(EventHandle.REFRESH_CLIENT_DATA_PANEL);
            return 0;
        }

        //已有分机: 先清理死亡冲突, 避免把失效连接当作竞争
        CleanDeadConflicts(macId);

        WebSockets.ClientConnection primary = player.client;
        if (primary == client)
        {
            //重复握手: 不改变在线状态, 只回报当前角色
            SendHistorySnapshot(player, client);
            if (conflictClients.TryGetValue(macId, out List<WebSockets.ClientConnection> list) && list.Count > 0)
            {
                NotifyMacConflict(macId, client, 2);
                return 2;
            }
            return 0;
        }

        boundMacId[client] = macId;

        if (primary == null || !IsAlive(primary))
        {
            if (primary != null && primary != client)
            {
                if (primary.IsConnected)
                {
                    //半开主连接降为冲突连接并通知, 不调用 Disconnect
                    AddConflict(macId, primary);
                    NotifyMacConflict(macId, primary, 1);
                }
                else
                {
                    boundMacId.Remove(primary);
                    NetMgr.Instance.RemoveClient(primary);
                }
            }

            //提升新连接为 primary, 保持 IsOnline 为 true(不触发 false→true)
            player.client = client;
            if (!player.IsOnline)
                player.IsOnline = true;
            SetOnlineState(macId, 1);
            RemoveConflict(macId, client);
            NotifyMacConflict(macId, client, 0);
            bool hasConflict = conflictClients.TryGetValue(macId, out List<WebSockets.ClientConnection> remain) && remain.Count > 0;
            if (hasConflict)
                NotifyMacConflict(macId, client, 2);
            StartReconcileIfNeeded();
            SendHistorySnapshot(player, client);
            EventCenter.Instance.EventTrigger(EventHandle.REFRESH_CLIENT_DATA_PANEL);
            return hasConflict ? 2 : 0;
        }

        //主连接存在, 新连接进入冲突队列(只读), 不修改 IsOnline
        bool added = AddConflict(macId, client);
        NotifyMacConflict(macId, client, 1);
        if (added && conflictClients[macId].Count == 1)
            NotifyMacConflict(macId, primary, 2);
        EventCenter.Instance.EventTrigger(EventHandle.REFRESH_CLIENT_DATA_PANEL);
        return 1;
    }

    public void PlayerDisconnect(WebSockets.ClientConnection client)
    {
        //先移除存活记录, 避免后续存活判断仍认为该连接有效
        NetMgr.Instance.RemoveClient(client);

        if (!boundMacId.TryGetValue(client, out int macId))
            return;
        boundMacId.Remove(client);

        if (!playerDic.TryGetValue(macId, out Player player))
            return;

        //冲突连接离开: 只处理冲突队列, 不修改 onlineState
        if (conflictClients.TryGetValue(macId, out List<WebSockets.ClientConnection> list) && list.Contains(client))
        {
            list.Remove(client);
            CleanDeadConflicts(macId);
            bool empty = !conflictClients.TryGetValue(macId, out List<WebSockets.ClientConnection> remain) || remain.Count == 0;
            if (empty && IsAlive(player.client))
                NotifyMacConflict(macId, player.client, 0);
            EventCenter.Instance.EventTrigger(EventHandle.REFRESH_CLIENT_DATA_PANEL);
            return;
        }

        //已被替换的旧主连接, 不再影响当前状态
        if (player.client != client)
            return;

        //主连接离开: 先清理死亡冲突, 再把最早的存活冲突提升为 primary(不重连)
        CleanDeadConflicts(macId);
        if (conflictClients.TryGetValue(macId, out List<WebSockets.ClientConnection> conflicts) && conflicts.Count > 0)
        {
            PromoteEarliestConflict(macId, player);
        }
        else
        {
            player.client = null;
            player.IsOnline = false;
            SetOnlineState(macId, 0);
        }
        EventCenter.Instance.EventTrigger(EventHandle.REFRESH_CLIENT_DATA_PANEL);
    }

    /// <summary>5秒巡检: 处理半开主连接, 提升最早的存活冲突连接</summary>
    private void ReconcileConflicts()
    {
        bool changed = false;
        foreach (KeyValuePair<int, Player> pair in playerDic.ToList())
        {
            int macId = pair.Key;
            if (macId < 2 || macId > SBoxModel.MaxMacId)
                continue;
            Player player = pair.Value;
            if (player == null)
                continue;

            bool hadConflict = conflictClients.TryGetValue(macId, out List<WebSockets.ClientConnection> before) && before.Count > 0;
            CleanDeadConflicts(macId);
            bool hasConflict = conflictClients.TryGetValue(macId, out List<WebSockets.ClientConnection> conflicts) && conflicts.Count > 0;

            if (IsAlive(player.client))
            {
                //半开冲突连接被清理后, 若冲突数变为 0, 通知主连接解除警告
                if (hadConflict && !hasConflict)
                {
                    NotifyMacConflict(macId, player.client, 0);
                    changed = true;
                }
                continue;
            }

            WebSockets.ClientConnection oldPrimary = player.client;
            if (hasConflict)
            {
                //旧主连接可能只是半开, 保留其绑定, 不调用 Disconnect; 后续消息会被权威校验拒绝
                PromoteEarliestConflict(macId, player);
                changed = true;
            }
            else if (oldPrimary != null || player.IsOnline || GetOnlineState(macId) != 0)
            {
                player.client = null;
                if (player.IsOnline)
                    player.IsOnline = false;
                SetOnlineState(macId, 0);
                if (oldPrimary != null)
                {
                    boundMacId.Remove(oldPrimary);
                    NetMgr.Instance.RemoveClient(oldPrimary);
                }
                changed = true;
            }
        }
        if (changed)
            EventCenter.Instance.EventTrigger(EventHandle.REFRESH_CLIENT_DATA_PANEL);
    }

    /// <summary>读取在线状态, 越界时返回0</summary>
    private int GetOnlineState(int macId)
    {
        if (macId < 1 || macId - 1 >= NetModel.Instance.playerOnlineState.Count)
            return 0;
        return NetModel.Instance.playerOnlineState[macId - 1];
    }

    public void GetPlayerInfosFromSBox()
    {
        SBoxIdeaPlayerOutState.Clear();
        SBoxIdea.PlayerScoreInfo();
    }

    public void UpdatePlayerCoinInfo(int macId, int flag, int value)
    {
        SBoxPlayerCoinInfo playerCoinInfo;
        if (playerCoinInfoDic.TryGetValue(macId, out playerCoinInfo))
        {
            switch (flag)
            {
                case 0:
                    playerCoinInfo.CoinIn += value;
                    break;
                case 1:
                    playerCoinInfo.CoinOut += value;
                    break;
                case 2:
                    playerCoinInfo.ScoreUp += value;
                    break;
                case 3:
                    playerCoinInfo.ScoreDown += value;
                    break;
                case 4:
                    playerCoinInfo.CoinOutWin += value;
                    break;
                default:
                    break;
            }
        }
        else
        {
            playerCoinInfo = new SBoxPlayerCoinInfo
            {
                PlayerId = macId,
            };
            switch (flag)
            {
                case 0:
                    playerCoinInfo.CoinIn += value;
                    break;
                case 1:
                    playerCoinInfo.CoinOut += value;
                    break;
                case 2:
                    playerCoinInfo.ScoreUp += value;
                    break;
                case 3:
                    playerCoinInfo.ScoreDown += value;
                    break;
                case 4:
                    playerCoinInfo.CoinOutWin += value;
                    break;
                default:
                    break;
            }
            playerCoinInfoDic[macId] = playerCoinInfo;
        }
        PlayerCoinInfoTimer();
    }

    public void PlayerCoinInfoTimer()
    {
        if (playerCoinInfoTimer == null)
            playerCoinInfoTimer = MonoMgr.Instance.StartCoroutine(PlayerCoinInfoTimerCoroutine());
    }

    private IEnumerator PlayerCoinInfoTimerCoroutine()
    {
        while (true)
        {
            List<SBoxPlayerCoinInfo> playerCoinInfos = new List<SBoxPlayerCoinInfo>();
            foreach (var item in playerCoinInfoDic.Values)
                playerCoinInfos.Add(item);
            if (playerCoinInfos.Count > 0)
                SBoxIdea.SetPlayerCoinInfo(playerCoinInfos);
            playerCoinInfoDic.Clear();
            yield return new WaitForSeconds(0.2f);
        }
    }
}