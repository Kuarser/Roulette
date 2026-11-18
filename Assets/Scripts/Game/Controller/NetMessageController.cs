using GameUtil;
using Newtonsoft.Json;
using SBoxApi;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using WebSockets;

public class NetMessageController : BaseManager<NetMessageController>
{
    private DelayTimer moveScoreTimer;

    private const string refundPrintPendingPrefsKeyPrefix = "refundPrintPending_";
    private const int refundPrintPendingMax = 10; //每台分机保留的未回执打印订单数量
    private readonly Dictionary<int, List<RefundPrintMsg>> refundPrintPending = new Dictionary<int, List<RefundPrintMsg>>();
    private readonly HashSet<int> refundInFlight = new HashSet<int>();
    private LoopTimer refundResendTimer;

    public void Init()
    {
        EventCenter.Instance.AddEventListener(EventHandle.NETWORK_STATUS_CHANGE, OnNetworkStatus);//欢乐水果的
        //EventCenter.Instance.AddEventListener<bool>(EventHandle.NETWORK_STATUS, OnNetworkStatus);
        Messenger.AddListener<MsgInfo, ClientConnection>(MessageName.Event_ClientNetworkLogic, OnClientNetworkLogic);
        Messenger.AddListener<byte[]>(MessageName.Event_ClientNetworkRecv, OnClientNetworkRecv);

        // 轮盘模式没有分机：不加载、也不重发分机打印回执订单。
        if (Model.Instance.rouletteFlow) return;

        //开局加载未回执打印订单(PlayerPrefs无键枚举,按macId 2..MaxMacId逐个读;跳过#1本机,本机打印不纳入回执/重发)
        for (int macId = 2; macId <= SBoxModel.MaxMacId; macId++)
        {
            string json = PlayerPrefs.GetString($"{refundPrintPendingPrefsKeyPrefix}{macId}", null);
            if (string.IsNullOrEmpty(json))
                continue;
            try
            {
                List<RefundPrintMsg> list = JsonConvert.DeserializeObject<List<RefundPrintMsg>>(json);
                if (list != null && list.Count > 0)
                    refundPrintPending[macId] = list;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Refund] 读取未回执打印缓存失败,按空处理 macId={macId}, err={e.Message}");
            }
        }
        //每2s重发未回执打印命令(同一orderId);重复Init()先停旧循环,防两个循环并存
        if (refundResendTimer != null)
            refundResendTimer.Cancel();
        refundResendTimer = Timer.LoopAction(2f, _ => ResendPendingPrints(), null, true);
    }

    private void OnNetworkStatus()
    {
        //if (NetMgr.Instance.CurNetStatus == NET_STATUS.NET_STATUS_CONNECTED)
        //    Login();

        Debug.LogError($"OnNetworkStatus:{NetMgr.Instance.CurNetStatus}");
        if (NetMgr.Instance.CurNetStatus == NET_STATUS.NET_STATUS_CONNECTED)
            Login();//登录
        //主机联网彩金状态变化,直接同步给分机(与UI刷新解耦)
        NetMgr.Instance.SyncNetJackpotState();
        EventCenter.Instance.EventTrigger(EventHandle.CHANGE_NETJP_ICON_ACTIVE);//根据联网状态改变彩金图标状态
    }

    private void OnNetworkStatus(bool isConnected)
    {
        if (isConnected)
            Login();
    }

    public void Login()
    {
        //MsgInfo msgInfo = new MsgInfo()
        //{
        //    cmd = (int)C2S_CMD.C2S_Login,
        //    id = IOCanvasModel.Instance.CfgData.MachineId,
        //};
        //string msg = JsonConvert.SerializeObject(msgInfo);
        //NetMgr.Instance.SendToServer(msg);

        NetJPLoginInfo netJPLoginInfo = new NetJPLoginInfo
        {
            gameType = IOCanvasModel.GameType,
            macId = IOCanvasModel.Instance.CfgData.MachineId,
        };

        MsgInfo msgInfo = new MsgInfo()
        {
            cmd = (int)C2S_CMD.C2S_Login,
            id = IOCanvasModel.Instance.CfgData.MachineId,
            jsonData = JsonConvert.SerializeObject(netJPLoginInfo)
        };
        string msg = JsonConvert.SerializeObject(msgInfo);
        NetMgr.Instance.SendToServer(msg);
    }

    private void OnClientNetworkLogic(MsgInfo info, ClientConnection client)
    {
        switch ((C2S_CMD)info.cmd)
        {
            case C2S_CMD.C2S_GetGameConfig:
                OnGetGameConfig(info, client);
                break;
            case C2S_CMD.C2S_GetGameState:
                OnGetGameState(info, client);
                break;
            case C2S_CMD.C2S_HARDWARD_KEYDOWN:
                OnHardwareKeydown(info);
                break;
            case C2S_CMD.C2S_HARDWARD_KEYUP:
                OnHardwareKeyup(info);
                break;
            case C2S_CMD.C2S_SBoxPlayerInState:
                //冲突分机只读, 仅权威连接可修改玩家状态
                if (!IsAuthoritative(info, client)) break;
                OnSBoxPlayerInState(info);
                break;
            case C2S_CMD.C2S_SBoxCoinIn:
                if (!IsAuthoritative(info, client)) break;
                OnClientCoinIn(info);
                break;
            case C2S_CMD.C2S_SBoxCoinOut:
                if (!IsAuthoritative(info, client)) break;
                OnClientCoinOut(info);
                break;
            case C2S_CMD.C2S_CoinOutStateChange:
                if (!IsAuthoritative(info, client)) break;
                OnCoinOutStateChange(info);
                break;
            case C2S_CMD.C2S_GetClientCoinIn:
                if (!IsAuthoritative(info, client)) break;
                OnGetClientCoinIn(info);
                break;
            case C2S_CMD.C2S_GetClientCoinOut:
                if (!IsAuthoritative(info, client)) break;
                OnGetClientCoinOut(info);
                break;
            case C2S_CMD.C2S_SetBetIndex:
                if (!IsAuthoritative(info, client)) break;
                OnSetBetIndex(info);
                break;
            case C2S_CMD.C2S_SBoxBillIn:
                if (!IsAuthoritative(info, client)) break;
                OnClientBillIn(info);
                break;
            case C2S_CMD.C2S_SBoxBillInOrder:
                if (!IsAuthoritative(info, client)) break;
                OnClientBillInOrder(info, client);
                break;
            case C2S_CMD.C2S_SBoxRefund:
                if (!IsAuthoritative(info, client)) break;
                OnRefundRequest(info.id, client);
                break;
            case C2S_CMD.C2S_SBoxRefundPrintAck:
                if (!IsAuthoritative(info, client)) break;
                OnRefundPrintAck(info, client);
                break;
            default:
                Debug.LogError($"未设置 {info.cmd} 响应");
                break;
        }
    }

    /// <summary>校验连接是否为该分机号的权威(primary)连接; 冲突分机只读</summary>
    private bool IsAuthoritative(MsgInfo info, ClientConnection client)
    {
        return PlayerMgr.Instance.GetPlayer(info.id)?.client == client;
    }

    private void OnGetGameConfig(MsgInfo info, ClientConnection client)
    {
        //分机号非法或连接绑定冲突时直接拒绝握手
        if (PlayerMgr.Instance.PlayerInsert(info, client) < 0)
            return;
        //分机刚上线,模仿分机通知算法卡该分机当前选择的押注筹码下标(默认0),防止算法卡记录的分机筹码下标与分机实际不一致
        SBoxIdea.SetBetIndex(info.id, 0);
        MsgInfo msg = new MsgInfo
        {
            cmd = (int)S2C_CMD.S2C_Config,
            id = info.id,
            jsonData = ConfigJson(info.id)
        };
        NetMgr.Instance.SendToClient(client, JsonConvert.SerializeObject(msg));

        MsgInfo msgInfo = new MsgInfo()
        {
            cmd = (int)S2C_CMD.S2C_SetRecod,
            id = info.id,
            jsonData = JsonConvert.SerializeObject(Model.Instance.records),
        };
        NetMgr.Instance.SendToClient(client, JsonConvert.SerializeObject(msgInfo));
        MsgInfo msgBetSwitch = new MsgInfo
        {
            cmd = (int)S2C_CMD.S2C_SetbetCoinList,
            id = -1,
            jsonData = JsonConvert.SerializeObject(new BetCoinList() { betCoinList = Model.Instance.betCoinList})
        };
        NetMgr.Instance.SendToClient(client, JsonConvert.SerializeObject(msgBetSwitch));
        Controller.Instance.view.SetNetJPIcon();//同步联网彩金状态
        bool isshow = false;
        if (Model.Instance.winLockPlayers.Contains(info.id))
            isshow = true;
        string tips = "";
        TipsStruct tipsStruct = new TipsStruct
        {
            isshow = isshow,
            str = tips,
            time = -1,
            winLockPlayers = Model.Instance.winLockPlayers.ToArray()
        };
            MsgInfo msgWinlock = new MsgInfo
        {
            cmd = (int)S2C_CMD.S2C_ShowClientWinLock,
            id = -1,
            jsonData = JsonConvert.SerializeObject(tipsStruct)
        };
        NetMgr.Instance.SendToClient(client, JsonConvert.SerializeObject(msgWinlock));
        //Debug.LogError($"winLockPlayersII:{JsonConvert.SerializeObject(Model.Instance.winLockPlayers)}");
            
        AllRoadStruct xiarecord = new AllRoadStruct
        {
            bigEyeRoadList = Model.Instance.bigEyeRoadList,
            smallRoadList = Model.Instance.smallRoadList,
            dragonRoadList = Model.Instance.dragonRoadList,
        };
        MsgInfo msgInfo2 = new MsgInfo
        {
            cmd = (int)S2C_CMD.S2C_XiaSanRoadData,
            id = -1,
            jsonData = JsonConvert.SerializeObject(xiarecord),
        };
        NetMgr.Instance.SendToAllClient(JsonConvert.SerializeObject(msgInfo2));
    }


    public class GameDataStruct
    {
        public SBoxIdeaPlayerOutState outState;
        public RecordStruct records;
    }
    private void OnGetGameState(MsgInfo info, ClientConnection client)
    {
        //暂时只用于领取奖励 状态时用
        //StateID clientStateId = JsonConvert.DeserializeObject<StateID>(info.jsonData);

        GameDataStruct gameDataStruct = new GameDataStruct()
        {
            outState = PlayerMgr.Instance.GetPlayer(info.id).outState,
            records = Model.Instance.curRecord,
        };
        //根据当前状态,主机找到对应数据,发送给分机
        MsgInfo msg = new MsgInfo
        {
            cmd = (int)S2C_CMD.S2C_SetGameData,
            id = info.id,
            jsonData = JsonConvert.SerializeObject(gameDataStruct)
        };
        NetMgr.Instance.SendToClient(client, JsonConvert.SerializeObject(msg));

    }

    private void OnHardwareKeydown(MsgInfo info)
    {
        var sboxSwitch = (ulong.Parse(info.jsonData));
    }

    private void OnHardwareKeyup(MsgInfo info)
    {
        var sboxSwitch = (ulong.Parse(info.jsonData));
    }

    private void OnSBoxPlayerInState(MsgInfo info)
    {
        //Debug.LogError("OnSBoxPlayerInState");
        if (PlayerMgr.Instance.playerDic.TryGetValue(info.id, out Player player))
        {
            InState inStateData = JsonConvert.DeserializeObject<InState>(info.jsonData);
            player.PlayerChangeInState(inStateData);
        }
        else
            Debug.Log($"玩家 {info.id} 未登录");
    }

    /// <summary>
    /// 分机通知主机当前选择的押注筹码下标,转发算法卡(20054)
    /// </summary>
    private void OnSetBetIndex(MsgInfo info)
    {
        if (PlayerMgr.Instance.playerDic.TryGetValue(info.id, out Player player))
        {
            int betIndex = int.Parse(info.jsonData);
            SBoxIdea.SetBetIndex(player.macId, betIndex);
        }
        else
            Debug.Log($"玩家 {info.id} 未登录");
    }

    //分机纸币入钞,主机代该座位上分
    private void OnClientBillIn(MsgInfo info)
    {
        Debug.LogError($"[BillIn] OnClientBillIn: 收到分机上分请求 macId={info.id}, coin={info.jsonData}");
        SBoxIdea.SetPlayerCoinInNumber(info.id, int.Parse(info.jsonData));
    }

    //分机纸币入钞(带订单号),主机代该座位上分;同一订单号只加一次分,重复请求直接回执
    private void OnClientBillInOrder(MsgInfo info, ClientConnection client)
    {
        BillInOrderMsg msg = JsonConvert.DeserializeObject<BillInOrderMsg>(info.jsonData);
        if (msg == null || string.IsNullOrEmpty(msg.orderId) || msg.credit <= 0)
        {
            Debug.LogError($"[BillIn] OnClientBillInOrder 请求无效: macId={info.id}, jsonData={info.jsonData}");
            return;
        }
        bool completed = PlayerMgr.Instance.IsOrderCompleted(info.id, msg.orderId);
        Debug.LogError($"[BillIn] OnClientBillInOrder: macId={info.id}, credit={msg.credit}, orderId={msg.orderId}, 已完成={completed}");
        if (!completed)
        {
            Debug.LogError($"[BillIn] 主机处理加分: seat={info.id}, credit={msg.credit}, orderId={msg.orderId}");
            PlayerMgr.Instance.MarkOrderCompleted(info.id, msg.orderId);
            SBoxIdea.SetPlayerCoinInNumber(info.id, msg.credit);
        }
        MsgInfo reply = new MsgInfo { cmd = (int)S2C_CMD.S2C_SBoxBillInOrderResult, id = info.id, jsonData = msg.orderId };
        Debug.LogError($"[BillIn] 主机发送回执: seat={info.id}, orderId={msg.orderId}, completed={completed}");
        NetMgr.Instance.SendToClient(client, JsonConvert.SerializeObject(reply));
    }

    //分机退分请求:按该机台可退币数向算法卡发退币申请,再下发(或本机打印)小票数据
    //public:主机本机路径由 Controller.OnHardwareKeyClick 跨类调用
    public void OnRefundRequest(int macId, ClientConnection client)
    {
        if (IOT.IOTModel.Instance.LinkIOT) { Debug.LogError($"[Refund] macId={macId} 已链接IOT,跳过本地退分"); return; } //镜像既有IOT门控
        if (macId == 1 && client != null) { Debug.LogError("[Refund] 远端连接不得声称 #1,拒绝"); return; } //防冒充:仅主机本机路径(client==null)可用 macId==1
        if (client != null && PlayerMgr.Instance.GetPlayer(macId)?.client != client) { Debug.LogError($"[Refund] macId={macId} 连接校验失败,拒绝"); return; } //防冒充:连接必须与机台绑定
        if (macId == 1 && SBoxSandbox.PrinterState() < 0) { Debug.LogError("[Refund] macId=1 打印机未连接,忽略退分"); return; } //主机#1打印机不在线:防御性忽略
        //远端:该机台仍有未回执打印单时阻断新的退分(避免同机多单)
        if (macId != 1 && refundPrintPending.TryGetValue(macId, out var pendingRefundPrints) && pendingRefundPrints.Count > 0) { Debug.LogError($"[Refund] macId={macId} 存在未回执打印单,拒绝新的退分"); return; }
        if (refundInFlight.Contains(macId)) { Debug.LogError($"[Refund] macId={macId} 在途,忽略重复请求"); return; } //请求时闩,防双击重复退币
        Player p = PlayerMgr.Instance.GetPlayer(macId);
        if (p == null || p.outState == null) { Debug.LogError($"[Refund] macId={macId} 未登录/无状态,忽略"); return; }
        float value = (float)IOCanvasModel.Instance.CfgData.CoinValue; //退分退的是币:按投币比例(每币Credit)计算退币数,不用票值比例TicketValue
        if (value <= 0) { Debug.LogError($"[Refund] macId={macId} PayOut setting error!"); return; }
        //仅本机#1受本机出币/冷却/投币状态约束
        if (macId == 1 && (IOCanvasModel.Instance.coinOuting || IOCanvasModel.Instance.isInCoinOutCoolDown || IOCanvasModel.Instance.coinIning)) { Debug.LogError($"[Refund] macId=1 出币/冷却/投币中,忽略"); return; }
        int count = (int)(p.outState.credit / value);
        if (count <= 0) { Debug.LogError($"[Refund] macId={macId} Credit not enough!"); return; }
        refundInFlight.Add(macId);
        Timer.DelayAction(5, () => refundInFlight.Remove(macId)); // 本地闩5s后清除(非协议重发)
        SBoxIdea.SetPlayerCoinOutNumber(macId, count); // 算法卡退币申请;落币由既有 out-state bit5 自动路径完成
        string orderId = $"{IOCanvasModel.Instance.CfgData.MachineId}-{macId}-{System.DateTime.UtcNow.Ticks}"; // 打印订单号(机台号-分机号-时间戳)
        string address = PlayerPrefs.GetString("PrinterAddress", "");
        if (string.IsNullOrEmpty(address)) address = "Address not set";
        RefundPrintMsg msg = new RefundPrintMsg { orderId = orderId, address = address, time = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), coinCount = count };
        Debug.LogError($"[Refund] macId={macId} count={count} orderId={orderId} 申请退币");
        if (macId == 1)
        {
            SandboxController.Instance.PrintRefundReceipt(msg.orderId, msg.address, msg.time, msg.coinCount); // 本机直打,不纳入回执/重发
        }
        else
        {
            AddPendingPrint(macId, msg); // 加入未回执持久队列并落盘
            NetMgr.Instance.SendToClient(client, JsonConvert.SerializeObject(new MsgInfo { cmd = (int)S2C_CMD.S2C_SBoxRefundPrint, id = macId, jsonData = JsonConvert.SerializeObject(msg) }));
        }
    }

    //未回执打印订单入队并落盘(保留最新refundPrintPendingMax条,超出丢最旧);仿纸钞订单持久化
    internal void AddPendingPrint(int macId, RefundPrintMsg msg)
    {
        if (!refundPrintPending.TryGetValue(macId, out List<RefundPrintMsg> list) || list == null)
        {
            list = new List<RefundPrintMsg>();
            refundPrintPending[macId] = list;
        }
        list.Add(msg);
        while (list.Count > refundPrintPendingMax)
            list.RemoveAt(0);
        PlayerPrefs.SetString($"{refundPrintPendingPrefsKeyPrefix}{macId}", JsonConvert.SerializeObject(list));
        PlayerPrefs.Save();
    }

    //打印完成回执:先校验回执连接与id一致(防冒充),再从该分机未回执队列删除orderId并落盘
    internal void OnRefundPrintAck(MsgInfo info, ClientConnection client)
    {
        Player player = PlayerMgr.Instance.GetPlayer(info.id);
        if (player == null || player.client != client)
        {
            Debug.LogError($"[Refund] OnRefundPrintAck 回执连接校验失败,拒绝 macId={info.id}");
            return;
        }
        RefundPrintAckMsg ack = null;
        try
        {
            ack = JsonConvert.DeserializeObject<RefundPrintAckMsg>(info.jsonData);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Refund] OnRefundPrintAck 解析失败: macId={info.id}, err={e.Message}");
            return;
        }
        if (ack == null || string.IsNullOrEmpty(ack.orderId) ||
            !refundPrintPending.TryGetValue(info.id, out List<RefundPrintMsg> list) || list == null)
        {
            Debug.LogError($"[Refund] OnRefundPrintAck 无匹配队列,忽略 macId={info.id}, jsonData={info.jsonData}");
            return;
        }
        int removed = list.RemoveAll(m => m != null && m.orderId == ack.orderId);
        if (removed <= 0)
        {
            Debug.LogError($"[Refund] OnRefundPrintAck 未找到订单,忽略 macId={info.id}, orderId={ack.orderId}");
            return;
        }
        PlayerPrefs.SetString($"{refundPrintPendingPrefsKeyPrefix}{info.id}", JsonConvert.SerializeObject(list));
        PlayerPrefs.Save();
        Debug.LogError($"[Refund] 打印回执已清除 macId={info.id}, orderId={ack.orderId}");
    }

    //每2s重发未回执打印命令:仅对"队列非空且分机在线"的macId逐条重发,orderId保持不变
    internal void ResendPendingPrints()
    {
        foreach (KeyValuePair<int, List<RefundPrintMsg>> pair in refundPrintPending)
        {
            List<RefundPrintMsg> pending = pair.Value;
            if (pending == null || pending.Count == 0)
                continue;
            int macId = pair.Key;
            Player player = PlayerMgr.Instance.GetPlayer(macId);
            if (player == null || !player.IsOnline)
                continue;
            foreach (RefundPrintMsg msg in pending)
            {
                if (msg == null)
                    continue;
                Debug.LogError($"[Refund] 重发打印命令 macId={macId}, orderId={msg.orderId}");
                NetMgr.Instance.SendToClient(player.client, JsonConvert.SerializeObject(new MsgInfo
                {
                    cmd = (int)S2C_CMD.S2C_SBoxRefundPrint,
                    id = macId,
                    jsonData = JsonConvert.SerializeObject(msg)
                }));
            }
        }
    }

    private void OnClientCoinIn(MsgInfo info)
    {
        if (PlayerMgr.Instance.playerDic.TryGetValue(info.id, out Player player))
        {
            player.DealWithCoinIn(int.Parse(info.jsonData));
            EventCenter.Instance.EventTrigger(EventHandle.PLAYER_COIN_IN);
        }
        else
            Debug.Log($"玩家 {info.id} 未登录");
    }

    private void OnClientCoinOut(MsgInfo info)
    {
        if (PlayerMgr.Instance.playerDic.TryGetValue(info.id, out Player player))
            player.DealWithCoinOut(int.Parse(info.jsonData));
        else
            Debug.Log($"玩家 {info.id} 未登录");
    }

    private void OnCoinOutStateChange(MsgInfo info)
    {
        if (PlayerMgr.Instance.playerDic.TryGetValue(info.id, out Player player))
            player.coinOuting = info.jsonData == "1" ? true : false;
        else
            Debug.Log($"玩家 {info.id} 未登录");
    }

    private void OnGetClientCoinIn(MsgInfo info)
    {
        if (PlayerMgr.Instance.playerDic.TryGetValue(info.id, out Player player))
        {
            MsgInfo msgInfo = new MsgInfo()
            {
                cmd = (int)S2C_CMD.S2C_ClientCoinCoin,
                id = info.id,
                jsonData = player.coinInCount.ToString()
            };
            player.SendToClient(JsonConvert.SerializeObject(msgInfo));
        }
        else
            Debug.Log($"玩家 {info.id} 未登录");
    }

    private void OnGetClientCoinOut(MsgInfo info)
    {
        if (PlayerMgr.Instance.playerDic.TryGetValue(info.id, out Player player))
        {
            MsgInfo msgInfo = new MsgInfo()
            {
                cmd = (int)S2C_CMD.S2C_ClientCoinOut,
                id = info.id,
                jsonData = player.coinOutCount.ToString()
            };
            player.SendToClient(JsonConvert.SerializeObject(msgInfo));
        }
        else
            Debug.Log($"玩家 {info.id} 未登录");
    }

    /// <summary>
    /// 玩家赢分移动至Credit
    /// </summary>
    private void MoveScoreTimer()
    {
        if (moveScoreTimer == null)
            moveScoreTimer = Timer.DelayAction(0.5f, () =>
            {
                SBoxIdea.MovePlayerScore(Model.Instance.moveScoreIdList);
                Model.Instance.moveScoreIdList.Clear();
            });
        else
            moveScoreTimer.Restart();
    }

    private string ConfigJson(int macId)
    {
        //初始值-1, 视作主机从未连上过该分机, 清除该分机缓存
        bool clearCoinIn = PlayerMgr.Instance.GetPlayer(macId).coinInCount == -1;
        if (clearCoinIn)
        {
            PlayerMgr.Instance.GetPlayer(macId).coinInCount = 0;
            PlayerPrefs.SetInt($"player{macId}coinInCount", 0);
            PlayerPrefs.GetInt($"player{macId}coinInCount", -1);

            PlayerMgr.Instance.GetPlayer(macId).coinOutCount = 0;
            PlayerPrefs.SetInt($"player{macId}coinOutCount", 0);
            PlayerPrefs.GetInt($"player{macId}coinOutCount", -1);
        }
        MsgConfig msgConfig = new MsgConfig()//发送所有配置信息
        {
            gameState = (int)FSMSystem.Instance.CurrentStateID,
            countDown = IOCanvasModel.Instance.CfgData.CountDown,
            curLanguage = (int)IOCanvasModel.Instance.curlanguage,
            round = Model.Instance.curRound,
            game = Model.Instance.curGame,
            coinValue = IOCanvasModel.Instance.CfgData.CoinValue,
            ticketValue = IOCanvasModel.Instance.CfgData.TicketValue,
            scoreTicket = IOCanvasModel.Instance.CfgData.scoreTicket,
            pulseValue = IOCanvasModel.Instance.CfgData.PulseValue,
            soundVolumnScale = IOCanvasModel.Instance.SoundVolumScale,
            bgmSwitch = IOCanvasModel.Instance.BgmSwitch,//背景音乐开关
            bgmSelectIndex = IOCanvasModel.Instance.BgmSelectIndex,//背景音乐选择
            switchList = IOCanvasModel.Instance.switchList,
            clearCoinIn = clearCoinIn,
            //增加
            minBet = IOCanvasModel.Instance.CfgData.MinBet,//最小押分
            limitBetsWins = IOCanvasModel.Instance.CfgData.LimitBetsWins,//限红
            remainCards = Model.Instance.RemainCards,
        };
        msgConfig.netJackpotState = IOCanvasModel.Instance.jackpotCfg.jackpotSwitch == (int)IOJackpotSwitch.JackpotSwitchOn ? (NetMgr.Instance.CurNetStatus == NET_STATUS.NET_STATUS_CONNECTED ? 2 : 1) : 0;
        //msgConfig.betsMinOfNetJackpots = Model.Instance.netJackpotMinBets;
        msgConfig.betsMinOfNetJackpots = IOCanvasModel.Instance.netJackpotMinBets;

        return JsonConvert.SerializeObject(msgConfig);
    }

    private void OnClientNetworkRecv(byte[] data)
    {
        if (data.Length == 0)
            return;

        string singlePacket = Encoding.UTF8.GetString(data);
        MsgInfo info = JsonConvert.DeserializeObject<MsgInfo>(singlePacket);

        if (info == null)
            return;
        int macId = info.id;
        S2C_CMD netCmd = (S2C_CMD)info.cmd;
        if (macId != -1 && macId != IOCanvasModel.Instance.CfgData.MachineId) return;

        switch (netCmd)
        {
            case S2C_CMD.S2C_JackpotHearHeat:
                NetMgr.Instance.SetLastHeartHeat();
                break;
            case S2C_CMD.S2C_WinJackpot:
                WinJackpot(info.jsonData);
                break;
            case S2C_CMD.S2C_GetJackpotData:
                GetJackpotData(info.jsonData);
                break;
            case S2C_CMD.S2C_Error:
                MessageError(info.jsonData);
                break;

            case S2C_CMD.S2C_JackpotMinBet://登录成功后大厅彩金主机会主动下发各彩金最小押分//用IOCanvasModel的netJackpotMinBets来接收, 可以关机保存数据,而Model保存的,关机就没了
                //Model.Instance.netJackpotMinBets  改成  IOCanvasMocel.Instance.netJackpotMinBets;
                IOCanvasModel.Instance.netJackpotMinBets = JsonConvert.DeserializeObject<List<int>>(info.jsonData);
                IOCanvasModel.Instance.netJackpotMinBets.Reverse();
                for (int i = 0; i < IOCanvasModel.Instance.netJackpotMinBets.Count; i++)
                    IOCanvasModel.Instance.netJackpotMinBets[i] = IOCanvasModel.Instance.netJackpotMinBets[i] / 100;
                MsgInfo msgInfo = new MsgInfo
                {
                    cmd = (int)S2C_CMD.S2C_NetJackpotMinBet,
                    id = -1,
                    jsonData = JsonConvert.SerializeObject(IOCanvasModel.Instance.netJackpotMinBets)
                };
                NetMgr.Instance.SendToAllClient(JsonConvert.SerializeObject(msgInfo));
                break;
            default:
                Debug.LogError($"未设置 {info.cmd} 响应");
                break;
        }
    }

    private void WinJackpot(string jsonData)
    {
        var winJackpotInfo = JsonConvert.DeserializeObject<WinJackpotInfo>(jsonData);
        Debug.LogError($"WinJackpot↑1{JsonConvert.SerializeObject(winJackpotInfo)}");

        EventCenter.Instance.EventTrigger(EventHandle.WIN_NET_JACKPOT, winJackpotInfo);

        ////数据库留档
        //SQLite.Instance.InsertJackpot(winJackpotInfo.orderId);

        ReceiveJackpotInfo receiveJackpotInfo = new ReceiveJackpotInfo
        {
            gameType = IOCanvasModel.GameType,
            orderId = winJackpotInfo.orderId
        };//回复主机已收到本条彩金数据

        NetMgr.Instance.SendToServer(JsonConvert.SerializeObject(new MsgInfo()
        {
            cmd = (int)C2S_CMD.C2S_ReceiveJackpot,
            id = IOCanvasModel.Instance.CfgData.MachineId,
            //jsonData = winJackpotInfo.orderId.ToString(),
            jsonData = JsonConvert.SerializeObject(receiveJackpotInfo),
        }));
    }

    private void GetJackpotData(string jsonData)
    {
        var jackpotData = JsonConvert.DeserializeObject<Dictionary<int, List<BetData>>>(jsonData);
        EventCenter.Instance.EventTrigger(EventHandle.GET_NET_JACKPOT_DATA, jackpotData);
    }

    private void MessageError(string jsonData)
    {
        var errorInfo = JsonConvert.DeserializeObject<ErrorInfo>(jsonData);
    }
}
