using GameUtil;
using IOT;
using Newtonsoft.Json;
using SBoxApi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSockets;

public class Controller : BaseManager<Controller>
{
    public View view;// 游戏视图层引用
    private bool isChooseTreasure = false;//是否选择了宝箱
    private Coroutine showTreasureTimersCoroutine;//显示幸运座位 宝箱 的携程
    private int? pendingTreasureIndex = null;//轮询触发属性，标记待打开宝箱索引

    //#if UNITY_EDITOR
    public bool isTestLittleGame = true;//是否开启小游戏测试
    public int testLittleGame = 0;//小游戏代码
//#endif

#region 初始化  开启监听
    public void Init()
    {
        Utils.SetRandomSeed();
        Model.Instance.Init();
        AddEventListener();
        StartCheckBattleState();
        InputMgr.Instance.StartOrEndCheck(true);

    }

    /// <summary>
    /// 启动状态检测，定时轮询相关状态
    /// </summary>
    private void StartCheckBattleState()
    {
        Timer.LoopAction(0.25f, (loopTimes) =>
        {
            LoopCheck();
        });

        if (IOTModel.Instance.LinkIOT)
            Timer.LoopAction(30f, (loopTimes) => CheckIOTUnfinishTicketOut());

        MonoMgr.Instance.StartCoroutine(SetOnlinePlayerCoroutine());

        MonoMgr.Instance.StartCoroutine(GetState());
    }
    /// <summary>
    /// 定时轮询奖池、打印机、爆机状态
    /// </summary>
    private void LoopCheck()
    {
        OnSBoxPrinterStateCustom();
        EventCenter.Instance.EventTrigger(EventHandle.CHANGE_JACKPOT, SBoxIdea.Jackpot());
        EventCenter.Instance.EventTrigger(EventHandle.SBOX_WIN_LOCK_BALANCE, SBoxIdea.WinLockBalance());
        if (!Utils.HasLocalIP())
        {
            NetworkErrorTips.Instance.ShowSystemTips(Utils.GetLanguage("NetworkError"));
        }
        else
        {
            NetworkErrorTips.Instance.HideTips();
        }
        CheckPrintCountOver();
    }
    /// <summary>
    /// 打印次数超过阈值时,主屏持续弹提示提醒换纸(优先级最低:仅当主屏没有显示其他提示时才显示)
    /// 换纸(OpenBox)后计数归零,提示自动消失
    /// </summary>
    private void CheckPrintCountOver()
    {
        if (IOCanvasModel.Instance.PrintCountMax > 0
            && IOCanvasModel.Instance.PrintCount >= IOCanvasModel.Instance.PrintCountMax)
        {
            const string key = "PrinterPaperCountOver";
            //优先级最低:主屏正显示其他提示时不抢占;若正显示的是换纸提示(可能为旧语言),放行以刷新为当前语言
            if (PopTips.Instance.IsShowingOtherTips(key)) return;
            PopTips.Instance.ShowSystemTips(key, -1, sendToClient:false);
        }
    }
    /// <summary>
    /// 检查IOT未完成出票并重试
    /// </summary>
    private void CheckIOTUnfinishTicketOut()
    {
        if (IOTModel.Instance.unfinishTicketOutDatas.Count > 0)
        {
            for (int i = 0; i < IOTModel.Instance.unfinishTicketOutDatas.Count; i++)
            {
                TicketOutData ticketOutData = IOTModel.Instance.unfinishTicketOutDatas[i];
                if (Utils.GetTimeStamp() / 1000 - int.Parse(ticketOutData.seq) > 30)
                    IoTPayment.Instance.DeviceTicketOut(IOTModel.Instance.PortId, ticketOutData, null);
            }
        }
    }
    /// <summary>
    /// 定时同步在线玩家状态协程
    /// </summary>
    private IEnumerator SetOnlinePlayerCoroutine()//instate
    {
        while (true)
        {
            bool shortTime = false;
            if (IOCanvasModel.Instance.coinOuting || IOCanvasModel.Instance.coinIning)
                shortTime = true;
            List<SBoxIdeaPlayerInState> sBoxPlayerInState = new List<SBoxIdeaPlayerInState>();
            foreach (var player in PlayerMgr.Instance.playerDic.Values)
            {
                player.CheckCoinIn();
                player.CheckCoinOut();
                SBoxIdeaPlayerInState sBoxBattlePlayerInState = new SBoxIdeaPlayerInState
                {
                    PlayerId = player.macId,
                    state = player.inState,
                    //rfu = Model.Instance.betCoinList[player.betUnitIdx],
                    LNumberCoinIn = player.sBoxCoinInCount,
                    LNumberCoinOut = player.sBoxCoinOutCount,
                };
                sBoxPlayerInState.Add(sBoxBattlePlayerInState);
                if (player.coinOuting)
                    shortTime = true;
                //Debug.LogWarning($"一直刷新,2.押注硬币下标:{player.betUnitIdx},押注硬币面值数:{Model.Instance.betCoinList[player.betUnitIdx]}");
            }
            SBoxIdea.IdeaPlayerInState(sBoxPlayerInState);
            float waitTime = shortTime ? 0.1f : 0.5f;
            yield return new WaitForSeconds(waitTime);
        }
    }
    private IEnumerator GetState()
    {
        while (true)
        {
            float waitTime = FSMSystem.Instance.CurrentStateID == StateID.Bet ? 0.08f : 0.25f;
            yield return new WaitForSeconds(waitTime);
            SBoxIdea.BattleGetState();
        }
    }


    /// <summary>
    /// 注册所有游戏相关事件监听
    /// </summary>
    private void AddEventListener()
    {
        //游戏运行逻辑相关
        EventCenter.Instance.AddEventListener(EventHandle.BET_STOP_FINISH, OnBetStopFinish);
        EventCenter.Instance.AddEventListener(EventHandle.COUNTDOWN_FINISH, OnCountDownFinish);
        EventCenter.Instance.AddEventListener(EventHandle.INIT_VIEW_FINISH, OnInitViewFinish);
        EventCenter.Instance.AddEventListener<int[]>(SBoxEventHandle.SBOX_BATTLE_GET_COMPLETED_GAME, OnGetCompletedGame);
        EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_BATS_COUNT_DOWN, OnBetsCountDown);
        EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_BATTLE_LEAD_STOP, OnBattleLeadStop);

        //小游戏相关
        EventCenter.Instance.AddEventListener<int>(EventHandle.CHANGE_JACKPOT, OnChangeJackpot);
        EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_BATTLE_LUCK_SHOW, OnSBoxLuckShow);
        EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_BATTLE_LUCK_PRIZE, OnSBoxLuckPirze);

        //后台设置相关
        EventCenter.Instance.AddEventListener(EventHandle.INIT_IO_CANVAS, OnInitIOCanvas);
        EventCenter.Instance.AddEventListener(EventHandle.LEAVE_SETTING, OnLeaveSetting);
        EventCenter.Instance.AddEventListener<SBoxAccount>(SBoxEventHandle.SBOX_GET_ACCOUNT, OnGetAccount);
        EventCenter.Instance.AddEventListener(EventHandle.REFRESH_IOCANVAS, OnRefreshIOCanvas);

        //硬件按键相关
        EventCenter.Instance.AddEventListener<KeyCode>(EventHandle.KEY_DOWN, InputTest);
        EventCenter.Instance.AddEventListener<ulong>(EventHandle.HARDWARE_KEY_CLICK, OnHardwareKeyClick);
        EventCenter.Instance.AddEventListener<ulong>(EventHandle.HARDWARE_KEY_LONG_PRESS, OnHardwareKeyLongpress);

        //硬件错误码，报错相关
        EventCenter.Instance.AddEventListener(EventHandle.ACTIVE, OnActive);
        EventCenter.Instance.AddEventListener<int>(EventHandle.SBOX_WIN_LOCK_BALANCE, OnSBoxWinLockBalance);
        EventCenter.Instance.AddEventListener<bool>(EventHandle.CODE_SUCCEED, OnCodeSucceed);

        //网络连接，传输数据相关
        EventCenter.Instance.AddEventListener<SBoxBattleStateData>(SBoxEventHandle.SBOX_BATTLE_GET_STATE, OnGetState);
        EventCenter.Instance.AddEventListener<List<SBoxIdeaPlayerOutState>>(SBoxEventHandle.SBOX_BATTLE_PLAYER_OUT_STATE, OnBattlePlayerOutState);
        EventCenter.Instance.AddEventListener<WebSockets.ClientConnection>(EventHandle.PLAYER_DISCONNECT, OnPlayerDisconnect);
        EventCenter.Instance.AddEventListener(EventHandle.PLAYER_INSTATE_CHANGE, OnPlayerStateChange);

        //第三方支付相关
        EventCenter.Instance.AddEventListener<CoinData>(IOTEventHandle.COINT_IN, OnIOTCoinIn);
        EventCenter.Instance.AddEventListener<TicketOutData>(IOTEventHandle.TICKET_OUT, OnIOTTicketOut);
        EventCenter.Instance.AddEventListener(EventHandle.REFRESH_QRCORD, OnIOTRegisterDec);
        //IoTPayment.Instance.Init("10600001", 1, 1, 32, 1);
        //IoTPayment.Instance.Init(IOCanvasModel.Instance.CfgData.MachineId.ToString(), SBoxModel.Instance.macId, 1, 32, 1);

        //其他
        EventCenter.Instance.AddEventListener<string>(SBoxEventHandle.SBOX_IDEA_VERSION, OnSBoxVersion);
        EventCenter.Instance.AddEventListener(EventHandle.REFERESH_BET_COIN, OnRefreshBetCoin);
        EventCenter.Instance.AddEventListener(EventHandle.UPDATE_BET_SWITCH, OnUpdateBetSwitch);


        //联网彩金
        EventCenter.Instance.AddEventListener<WinJackpotInfo>(EventHandle.WIN_NET_JACKPOT, OnWinNetJackpot);
        EventCenter.Instance.AddEventListener(EventHandle.CHANGE_NETJP_ICON_ACTIVE, OnChangeNetJPIconActive);
        EventCenter.Instance.AddEventListener<bool>(EventHandle.HELP_DIALOG_ABOUT_NET_JACKPOT, OnShowHelpDialogAboutNetJackpot);

        //抽水返回
        EventCenter.Instance.AddEventListener<SBoxPrizePlayerData>(SBoxEventHandle.SBOX_GET_PRIZE_PLAYER, OnGetPlayerCut);
    }
    #endregion

//抽水返回
    public void OnGetPlayerCut(SBoxPrizePlayerData sBoxPrizePlayerData)
    {
        IOCanvasModel.Instance.sBoxPrizePlayerDatas.Add(sBoxPrizePlayerData);
        if (IOCanvasModel.Instance.sBoxPrizePlayerDatas.Count >= 10)
        {
            IOCanvasModel.Instance.sBoxPrizePlayerDatas.Sort((p1, p2) => p1.PlayerId.CompareTo(p2.PlayerId));
            EventCenter.Instance.EventTrigger(EventHandle.INIT_BILL_VIEW);
        }
    }

#region netJackpot


    private void OnShowHelpDialogAboutNetJackpot(bool flag)
    {
        view.ShowHlepContentAboutNetJackpot(flag);
    }

    private void OnChangeNetJPIconActive()
    {
        Debug.Log("OnChangeNetJPIconActive");
        view.SetNetJPIcon();//自己改变图标
    }

    public List<WinJackpotInfo> winJackpotInfoSave = new List<WinJackpotInfo>();
    public bool isHaveNetJackpot = false;
    /// <summary>
    /// 赢得网络奖池回调
    /// </summary>
    /// <param name="winJackpotInfo"></param>
    public void OnWinNetJackpot(WinJackpotInfo winJackpotInfo)
    {
        Debug.Log("OnWinNetJackpot");
        if (!PlayerMgr.Instance.playerDic.TryGetValue(winJackpotInfo.seat, out Player player))
        {
            Debug.LogError("未找到中奖分机!");
            return;
        }
        //同一周期内收到同一机台号(seat)的重复中奖消息,自动删除旧消息,只保留最新一条
        winJackpotInfoSave.RemoveAll(info => info.seat == winJackpotInfo.seat);
        winJackpotInfoSave.Add(winJackpotInfo);//彩金暂存
        isHaveNetJackpot = true;
    }
    #endregion

#region 游戏运行逻辑监听回调

    private void OnBetStopFinish()
    {
        SBoxIdea.BetsStop();
    }

    private void OnCountDownFinish()
    {
        SBoxIdea.BetsCountDown(Model.Instance.curCountDown);

        NetMgr.Instance.SendToAllClient(JsonConvert.SerializeObject(new MsgInfo
        {
            cmd = (int)S2C_CMD.S2C_CcountDown,
            id = -1,
            jsonData = Model.Instance.curCountDown.ToString(),
        }));
    }

    /// <summary>
    /// 初始化完成回调,开始游戏
    /// </summary>
    private void OnInitViewFinish()
    {
        Timer.DelayAction(1, () =>
        {
            if (SBoxModel.Instance.macId != 1)
            {
                PopTips.Instance.ShowSystemTips("MacIdSettingError", -1, sendToClient: false);
                return;
            }
            SBoxIdea.Version();
            LoopCheck();
            PlayerMgr.Instance.AddLocalPlayer();
            //主机刚上线,通知算法卡主机当前选择的押注筹码下标(默认0),防止算法卡记录的筹码下标与主机实际不一致
            SBoxIdea.SetBetIndex(1, 0);
            //SBoxIdea.BattlePlayerOutStateListener();
            Debug.LogWarning("[OutStateDebug] Controller 初始化: 即将调用 IdeaPlayerOutStateListener 注册 20053 监听");
            SBoxIdea.IdeaPlayerOutStateListener();
            Debug.LogWarning("[OutStateDebug] Controller 初始化: IdeaPlayerOutStateListener 调用完成");
            SandboxController.Instance.Init();

            // 设置网络自动连接
            NetMgr.Instance.SetNetAutoConnect(true);//作为游戏主机

            //OnRefreshBetCoin();
            new BetManager();
            new RewardManager();
            new SettingManager();
            new ErrorManager();
            new PrepareManager();
            new EndRoundManager();
            new PrintManager();
            new FirstStartManager();//游戏开始入口
        });
    }

    /// <summary>
    /// 游戏开始事件
    /// </summary>
    public void OnGameStart()
    {
        Model.Instance.isJoker = false;   // 恢复：每局重置和局标志（原皇冠功能转移）
        Model.Instance.isNoBanker = false;
        Model.Instance.isShowCard = false;
        //FSMSystem.Instance.PerformTransition(Transition.Bet);//prepare 状态转bet 状态
		if (Model.Instance.testMode == 0)//有问题，明牌开始之后能再切换到幸运座位状态，正式版不允许切换
            Model.Instance.curRecord.luckyId = 6;
    	
        int ran = Utils.GetRandom(0, 3);
        if (IOCanvasModel.Instance.BgmSelectIndex != (int)IOBgmSelect.Random)//不是随机,用后台选中的背景音乐
            ran = IOCanvasModel.Instance.BgmSelectIndex;
        AudioManager.Instance.BgmVolumScale = 0.5f * AudioManager.Instance.SoundVolumScale;
        if (IOCanvasModel.Instance.BgmSwitch == (int)IOBgmSwitch.BgmSwitchOn)
            AudioManager.Instance.PlayMusic($"normalBgm{ran}");//播放背景音
        else
            AudioManager.Instance.StopMusic();//后台开关关闭,不播放背景音

        NetMgr.Instance.SendToAllClient(JsonConvert.SerializeObject(new MsgInfo
        {
            cmd = (int)S2C_CMD.S2C_Start,
            id = -1,
            jsonData = JsonConvert.SerializeObject(new GameInfo
            {
                curRound = Model.Instance.curRound,
                curGame = Model.Instance.curGame,
                musicRan = ran,
                curRecord = Model.Instance.curRecord,
            })
        }));//通知分机开始游戏,给分机发 轮,局,背景音,本局游戏记录

        view.UpdateRoundAndGameText();
        view.CountDownStart();//倒计时
        MingPaiWaitToStart();
    }

    private void MingPaiWaitToStart()
    {

        if (Model.Instance.curRecord.luckyId == 6)
        {
            Model.Instance.isShowCard = true;
            NetMgr.Instance.SendToAllClient(JsonConvert.SerializeObject(new MsgInfo
            {
                cmd = (int)S2C_CMD.S2C_MingPai,
                id = -1,
                jsonData = JsonConvert.SerializeObject(Model.Instance.curRecord),
            }));//明牌,
            bool[] wins = Model.CalcWinDoors(Model.Instance.curRecord);
            for (int i = 0; i < wins.Length && i < view.symbolViewsList.Count; i++)
            {
                if (wins[i])
                    view.symbolViewsList[i].ShowLightIcon();//显示结果中奖闪烁(可多门同亮)
            }
            view.uiRoadMapView.ShowResultWin(Model.Instance.curRecord.result - 1);
            
            //将抢庄按钮的子物体置灰
            view.setBidBankerBtngrey(true);
            Model.Instance.canBidBanker = false;
            SandboxController.Instance.TurnOffEnterLight();//不可抢庄,抢庄键灯灭
            PlayerMgr.Instance.localPlayer.ResetBankerKeyState();
            AudioManager.Instance.PlayVoiceEff("showCard");
            MonoMgr.Instance.StartCoroutine(view.ShowMingPaiLogo());
            MonoMgr.Instance.StartCoroutine(ShowResultImmediatelyCoroutine(Model.Instance.curRecord));
        }
        else
        {
            SandboxController.Instance.TurnOnBetLight();
        }
    }

    /// <summary>
    /// 押分倒计时事件
    /// </summary>
    private void OnBetsCountDown(int result)
    {
        if (result < 0)
        {
            Debug.LogError("BetsCountDown 参数错误");
            return;
        }
        else if (result > 0)
        {
            Debug.LogError("BetsCountDown result > 0");
            switch (result)
            {
                default:
                    break;
            }
            return;
        }
    }

    public void SendBetData2NetJackpot()
    {
        Debug.LogError($"call SendBetData2NetJackpot:{JsonConvert.SerializeObject(IOCanvasModel.Instance.CfgData)}");
        if (IOCanvasModel.Instance.JackpotCfg.jackpotSwitch != 1) return;
        List<JackBetInfo> betInfos = new List<JackBetInfo>();
        foreach (var player in PlayerMgr.Instance.playerDic.Values)
        {
            int totalBet = 0;
            if (player.outState == null)
                continue;
            for (int i = 0; i < player.outState.bets.Length; i++)
                totalBet += player.outState.bets[i];
            if (totalBet > 0)
            {
                JackBetInfo betInfo = new JackBetInfo
                {
                    gameType = IOCanvasModel.GameType,
                    seat = player.outState.PlayerId,
                    bet = totalBet * 100,
                    betPercent = IOCanvasModel.Instance.JackpotCfg.betPercent,
                    scoreRate = int.Parse(System.Math.Round(1000f / IOCanvasModel.Instance.CfgData.CoinValue).ToString()),
                    JPPercent = IOCanvasModel.Instance.jackpotCfg.jpPercent,
                };
                betInfos.Add(betInfo);
            }
        }
        if (NetMgr.Instance.CurNetStatus == NET_STATUS.NET_STATUS_CONNECTED)
        {
            MsgInfo msgInfo = new MsgInfo
            {
                cmd = (int)C2S_CMD.C2S_JackBet,
                id = IOCanvasModel.Instance.CfgData.MachineId,
                jsonData = JsonConvert.SerializeObject(betInfos)
            };
            if (msgInfo.jsonData != null && msgInfo.jsonData.Length > 0)
                Debug.LogError($"C2S_JackBet:{msgInfo.jsonData}");//联网彩金中奖
            NetMgr.Instance.SendToServer(JsonConvert.SerializeObject(msgInfo));

            for (int i = 1; i < 10; i++)
            {
                if (Model.Instance.netJackpotBetBuffer.ContainsKey(i)
                && Model.Instance.netJackpotBetBuffer[i] != null
                && Model.Instance.netJackpotBetBuffer[i].Count != 0)
                {
                    for (int j = 0; j < Model.Instance.netJackpotBetBuffer[i].Count; j++)
                    {
                        MsgInfo mes = new MsgInfo
                        {
                            cmd = (int)C2S_CMD.C2S_JackBet,
                            id = IOCanvasModel.Instance.CfgData.MachineId,
                            jsonData = JsonConvert.SerializeObject(Model.Instance.netJackpotBetBuffer[i])
                        };
                        NetMgr.Instance.SendToServer(JsonConvert.SerializeObject(mes));
                    }
                    Model.Instance.netJackpotBetBuffer[i].Clear();
                }
            }
        }
        else
        {
            for (int i = 0; i < betInfos.Count; i++)
            {
                if (!Model.Instance.netJackpotBetBuffer.ContainsKey(betInfos[i].seat) || Model.Instance.netJackpotBetBuffer[betInfos[i].seat] == null)
                    Model.Instance.netJackpotBetBuffer[betInfos[i].seat] = new List<JackBetInfo>();
                var netJackpotBetBuffer = Model.Instance.netJackpotBetBuffer[betInfos[i].seat];
                if (netJackpotBetBuffer.Count > 10)
                    netJackpotBetBuffer.RemoveAt(0);
                netJackpotBetBuffer.Add(betInfos[i]);
            }
        }
    }

    /// <summary>
    /// 小游戏开奖停止事件
    /// </summary>
    private void OnBattleLeadStop(int result)
    {
        //Debug.Log("OnBattleLeadStop");
        if (result < 0)
        {
            Debug.LogError("BattleLeadStop 参数错误");
            return;
        }
        else if (result > 0)
        {
            switch (result)
            {
                default:
                    break;
            }
            return;
        }

        //回调
        Debug.Log($"回调后本局游戏赢分:{PlayerMgr.Instance.localPlayer.outState.wins}");
    }
    
    public void RefreshOnNewRound()
    {
        view.RefreshOnNewRound();
    }
    
#endregion

#region 游戏运行逻辑补充 辅助函数
    public void Reset()
    {
        Model.Instance.curRecord = null;
        SBoxIdea.BattleResetGame();
    }

    /// <summary>
    /// 向奖池设备发送押分信息
    /// </summary>
    //private void SendBetsToNetJackpotDivice()
    //{
    //    if (IOCanvasModel.Instance.JackpotCfg.jackpotSwitch != 1) return;
    //    foreach (var player in PlayerMgr.Instance.playerDic.Values)
    //    {
    //        if (!player.IsOnline || player.outState == null) continue;
    //        int totalBet = 0;
    //        foreach (var bet in player.outState.bets)
    //            totalBet += bet;
    //        if (totalBet > 0)
    //        {
    //            JackBetInfo betInfo = new JackBetInfo
    //            {
    //                seat = SBoxModel.Instance.macId,
    //                bet = totalBet,
    //                betPercent = IOCanvasModel.Instance.jackpotCfg.betPercent,
    //                scoreRate = int.Parse(((1f / IOCanvasModel.Instance.CfgData.CoinValue) * 1000).ToString()),
    //                JPPercent = IOCanvasModel.Instance.jackpotCfg.jpPercent,
    //            };

    //            MsgInfo msgInfo1 = new MsgInfo
    //            {
    //                cmd = (int)C2S_CMD.C2S_JackBet,
    //                id = IOCanvasModel.Instance.CfgData.MachineId,
    //                jsonData = JsonConvert.SerializeObject(betInfo)
    //            };

    //            NetMgr.Instance.SendToServer(JsonConvert.SerializeObject(msgInfo1));
    //        }
    //    }
    //}
    #endregion

#region 网络连接，传输数据相关
    /// <summary>
    /// 获取算法塔状态事件
    /// </summary>
    private void OnGetState(SBoxBattleStateData state)//outState
    {
        //Debug.LogError($"算法塔OnGetState:{JsonConvert.SerializeObject(state)}");
        Model.Instance.totalBets = state.TotalBets;
        view.UpdateAllBets();
        if (state.LuckPlayerId > 0 && state.LuckSelectIndex > -1)
            StopShowTreasure(state.LuckSelectIndex);

        Model.Instance.banckerData = state.BankerData;//玩家获取庄家状态

        NetMgr.Instance.SendToAllClient(JsonConvert.SerializeObject(new MsgInfo
        {
            cmd = (int)S2C_CMD.S2C_SBoxSetState,
            id = -1,
            jsonData = JsonConvert.SerializeObject(state)
        }));
    }
    private bool CheckClientId(int value, int index)
    {
        return (value & (1 << index)) >> index == 1;
    }




    /// <summary>
    /// 构造下发给分机的 outState 副本:将 odds 数组替换为算法卡下发的官方每门赢分,
    /// 分机结算页直接读取该数组显示(属性已改名为 roleWins, 保持 JSON 字段名 odds 兼容)
    /// </summary>
    private SBoxIdeaPlayerOutState BuildOutStateWithRoleWins(SBoxIdeaPlayerOutState outState)
    {
        var payoutData = Model.Instance.PayoutData;
        if (payoutData == null || outState.PlayerId < 1 || outState.PlayerId > payoutData.Length)
            return outState;
        var payout = payoutData[outState.PlayerId - 1];
        if (payout == null) return outState;

        SBoxIdeaPlayerOutState copy = new SBoxIdeaPlayerOutState
        {
            PlayerId = outState.PlayerId,
            LIndex = outState.LIndex,
            rfu = outState.rfu,
            credit = outState.credit,
            wins = outState.wins,
            state = outState.state,
            seconds = outState.seconds,
            bets = (int[])outState.bets.Clone(),
            odds = (int[])outState.odds.Clone(),
        };
        int count = copy.odds.Length < payout.RoleWins.Length ? copy.odds.Length : payout.RoleWins.Length;
        for (int i = 0; i < count; i++)
        {
            copy.odds[i] = payout.RoleWins[i];// 官方每门赢分
        }
        return copy;
    }

    /// <summary>
    /// 玩家出局状态事件，更新玩家状态并同步到客户端
    /// </summary>
    private void OnBattlePlayerOutState(List<SBoxIdeaPlayerOutState> sBoxBattlePlayerOutStates)
    {
        //Debug.LogWarning($"[OutStateDebug] OnBattlePlayerOutState 被调用, 收到 {sBoxBattlePlayerOutStates.Count} 个玩家出状态");
        PlayerMgr.Instance.SBoxIdeaPlayerOutState = sBoxBattlePlayerOutStates;

        foreach (var outState in sBoxBattlePlayerOutStates)
        {
            if (PlayerMgr.Instance.playerDic.TryGetValue(outState.PlayerId, out Player player))
            {
                player.outState = outState;
                //Debug.LogWarning($"[OutStateDebug] 赋值成功: PlayerId={outState.PlayerId}, wins={outState.wins}, credit={outState.credit}");
                if (outState.PlayerId != 1)
                {
                    MsgInfo msgInfo = new MsgInfo
                    {
                        cmd = (int)S2C_CMD.S2C_SBoxPlayerOutState,
                        id = player.macId,
                        jsonData = JsonConvert.SerializeObject(BuildOutStateWithRoleWins(outState))
                    };
                    player.SendToClient(JsonConvert.SerializeObject(msgInfo));
                }
            }
            else
            {
                //Debug.LogWarning($"[OutStateDebug] 赋值失败: PlayerId={outState.PlayerId} 不在 playerDic 中, playerDic.Count={PlayerMgr.Instance.playerDic.Count}");
            }
        }

        foreach (Player player in PlayerMgr.Instance.playerDic.Values)
        {
            if (player.outState == null) continue;
            CheckClientWinlockWithOutStateCredit(player.outState);
        }

        string clientIdStr = "";
        if (Model.Instance.winLockPlayers.Count > 0)
        {
            for (int i = 0; i < Model.Instance.winLockPlayers.Count; i++)
            {
                clientIdStr += Model.Instance.winLockPlayers[i] + ",";
            }
            if (!string.IsNullOrEmpty(clientIdStr))
                clientIdStr = clientIdStr.Substring(0, clientIdStr.Length - 1);
            
            if (IOCanvasModel.Instance.curlanguage == Language.en)
            {
                if (Model.Instance.winLockPlayers.Contains(SBoxModel.Instance.macId))
                    ClientWinLockTips.Instance.ShowSystemTips($"Player {SBoxModel.Instance.macId} win lock", -1);
                ClientWinLockTipsSetting.Instance.ShowSystemTips($"Player {clientIdStr} win lock", -1);
            }
            else
            {
                if (Model.Instance.winLockPlayers.Contains(SBoxModel.Instance.macId))
                    ClientWinLockTips.Instance.ShowSystemTips($"玩家{SBoxModel.Instance.macId}爆机", -1);
                ClientWinLockTipsSetting.Instance.ShowSystemTips($"玩家{clientIdStr}爆机", -1);
            }
            if(!Model.Instance.winLockPlayers.Contains(SBoxModel.Instance.macId))
                ClientWinLockTips.Instance.HideTips();
        }
        else
        {
            ClientWinLockTips.Instance.HideTips();
            ClientWinLockTipsSetting.Instance.HideTips();
        }

        if (!IOCanvasModel.Instance.coinOuting
            && (1 << 5 & PlayerMgr.Instance.localPlayer.outState.state) == 1 << 5
            && !IOTModel.Instance.LinkIOT
            && !IOCanvasModel.Instance.isInCoinOutCoolDown)
        {
            SandboxController.Instance.StartCoinOut();
        }

        view.RefreshPlayerInfo();
    }
    /// <summary>
    /// 检查玩家是否爆机
    /// </summary>
    private bool CheckClientWinlockWithOutStateCredit(SBoxIdeaPlayerOutState outState)
    {
        bool result = false;

        if (IOCanvasModel.Instance.CfgData.PlayerWinLock == 0)
            return result;

        if (outState.credit > IOCanvasModel.Instance.CfgData.PlayerWinLock)
        {
            if (!Model.Instance.winLockPlayers.Contains(outState.PlayerId))
            {
                Model.Instance.winLockPlayers.Add(outState.PlayerId);
                string tips = "";
                TipsStruct tipsStruct = new TipsStruct
                {
                    isshow = true,
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
                NetMgr.Instance.SendToAllClient(JsonConvert.SerializeObject(msgWinlock));
                //Debug.LogError($"winLockPlayersA:{JsonConvert.SerializeObject(Model.Instance.winLockPlayers)}");
            }
            result = true;
        }
        else
        {
            Model.Instance.winLockPlayers.Remove(outState.PlayerId);
            string tips = "";
            TipsStruct tipsStruct = new TipsStruct
            {
                isshow = false,
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
            NetMgr.Instance.SendToAllClient(JsonConvert.SerializeObject(msgWinlock));
            //Debug.LogError($"winLockPlayersE:{JsonConvert.SerializeObject(Model.Instance.winLockPlayers)}");
        }
        return result;
    }


    /// <summary>
    /// 玩家断开连接事件
    /// </summary>
    private void OnPlayerDisconnect(ClientConnection client)
    {
        //断线回调来自 socket 线程，转到主线程执行，避免与主线程玩家列表并发修改
        Loom.QueueOnMainThread(_ => PlayerMgr.Instance.PlayerDisconnect(client), null);
    }

    /// <summary>
    /// 玩家状态变更事件，向SBox同步玩家状态
    /// </summary>
    private void OnPlayerStateChange()//instate
    {
        List<SBoxIdeaPlayerInState> sBoxPlayerInState = new List<SBoxIdeaPlayerInState>();
        foreach (var player in PlayerMgr.Instance.playerDic.Values)
        {
            SBoxIdeaPlayerInState sBoxBattlePlayerInState = new SBoxIdeaPlayerInState
            {
                PlayerId = player.macId,
                state = player.inState,
                //rfu = Model.Instance.betCoinList[player.betUnitIdx],
                LNumberCoinIn = player.sBoxCoinInCount,
                LNumberCoinOut = player.sBoxCoinOutCount,
            };
            sBoxPlayerInState.Add(sBoxBattlePlayerInState);
        }
        SBoxIdea.IdeaPlayerInState(sBoxPlayerInState);
    }
#endregion

#region 小游戏相关
    /// <summary>
    /// 奖池变更事件
    /// </summary>
    private void OnChangeJackpot(int jackpot)
    {
        //Debug.Log("OnChangeJackpot");
        view.SetJackpot(jackpot);
        MsgInfo msgInfo = new MsgInfo
        {
            cmd = (int)S2C_CMD.S2C_SetJackpot,
            id = -1,
            jsonData = jackpot.ToString()
        };
        NetMgr.Instance.SendToAllClient(JsonConvert.SerializeObject(msgInfo));
    }

    /// <summary>
    /// 彩金开奖
    /// </summary>
    private void OnSBoxLuckPirze(int result)//传值有什么用?没有用到啊
    {
        Debug.LogError("OnSBoxLuckPirze");
        if (result < 0)
        {
            Debug.LogError("BattleLeadStart 参数错误");
            return;
        }
        else if (result > 0)
        {
            switch (result)
            {
                default:
                    break;
            }
            return;
        }
        //Debug.Log("OnSBoxLuckPirze");
        if (Model.Instance.curRecord.luckyId == 8)//彩金开奖
            MonoMgr.Instance.StartCoroutine(DelayShowResult());
        //else if (Model.Instance.curRecord.luckyId == 7)//全中,大满贯
        //{

        //}
    }
    private IEnumerator DelayShowResult()
    {
        Debug.LogError("DelayShowResult");
        yield return new WaitForSeconds(5);//播放彩金动画延迟5秒
        ShowResult(Model.Instance.curRecord);
    }


    /// <summary>
    /// 幸运宝箱展示事件，显示3个宝箱
    /// </summary>
    private void OnSBoxLuckShow(int result)//传值是为了判断是否接受到数据的,可以不用
    {
        Debug.LogError("OnSBoxLuckShow");
        if (result < 0)
        {
            Debug.LogError("BattleLeadStart 参数错误");
            return;
        }
        else if (result > 0)
        {
            switch (result)
            {
                default:
                    break;
            }
            return;
        }
        StartShowLuckyTreasureTimers();
    }

    private void StopShowTreasure(int selectIndex)//手动选择宝箱,提前打开
    {
        //Debug.LogError($"StopShowTreasure:{selectIndex}");
        isChooseTreasure = true;//先标记，确保协程即使未被停止也不会再触发
        pendingTreasureIndex = selectIndex;//设置触发属性，由DelayShowTreasureCoroutine轮询检测后统一调用TreasureAnimation
    }

    private void StartShowLuckyTreasureTimers()//开始10秒等待倒计时后显示打开幸运宝箱动画
    {
        showTreasureTimersCoroutine = MonoMgr.Instance.StartCoroutine(DelayShowTreasureCoroutine());
    }

    private IEnumerator DelayShowTreasureCoroutine()//等待10秒,10秒内,没有手动选择宝箱,随机选择一个宝箱打开,但是值已经确定的了
    {
        float elapsed = 0f;
        Debug.LogError("开始计时");
        while (true)
        {
            yield return new WaitForSeconds(0.2f);
            elapsed += 0.2f;
            if (pendingTreasureIndex.HasValue)
            {
                int index = pendingTreasureIndex.Value;
                pendingTreasureIndex = null;
                MonoMgr.Instance.StartCoroutine(TreasureAnimation(index));
                showTreasureTimersCoroutine = null;
                yield break;
            }
            if (!isChooseTreasure && elapsed >= 10f)
            {
                pendingTreasureIndex = Utils.GetRandom(0, 3);
            }
        }
    }
    private IEnumerator TreasureAnimation(int index)
    {
        Debug.LogError($"TreasureAnimation:{index}");
        isChooseTreasure = true;
        MsgInfo msgInfo = new MsgInfo
        {
            cmd = (int)S2C_CMD.S2C_BattleLuckSelect,//选择宝箱(卡牌)打开
            id = -1,
            jsonData = index.ToString(),
        };
        NetMgr.Instance.SendToAllClient(JsonConvert.SerializeObject(msgInfo));
        SandboxController.Instance.TurnOffTreasureLight();

        view.ShowLuckySeatCardResult(index);//显示幸运座位卡牌选中后翻牌动画结果

        yield return new WaitForSeconds(1.5f);

        SBoxIdea.BattLuckPrize();//请求算法卡,判断幸运座位是什么奖
        yield return new WaitForSeconds(5f);//修改了,改短了,从10变5

        view.ResetCardList();//隐藏幸运座位3个卡牌

        ShowResult(Model.Instance.curRecord);//小游戏结束,开始显示结果流程,

        isChooseTreasure = false;
    }

    public void ShowResult(RecordStruct recordStruct)
    {
        MsgInfo msgInfo = new MsgInfo
        {
            cmd = (int)S2C_CMD.S2C_ShowResult,
            id = -1,
            jsonData = JsonConvert.SerializeObject(recordStruct),
        };
        NetMgr.Instance.SendToAllClient(JsonConvert.SerializeObject(msgInfo));

        view.ShowResult(recordStruct);
    }
    
    public void ShowResultSpecialFinish(RecordStruct recordStruct)
    {
        view.ShowResultSpecialFinish(recordStruct);
    }

    private IEnumerator ShowResultImmediatelyCoroutine(RecordStruct recordStruct)//中明牌小游戏
    {

        //播放翻动卡牌
        yield return view.ShowPokerCard(recordStruct);
        yield return new WaitForSeconds(view.GetPokerTurnAnimationTime());
        int result = recordStruct.result;
        yield return new WaitForSeconds(0.7f);

        view.SetPokerValue(recordStruct.value[0]); //增加
        //yield return view.PlayPokerSFX(recordStruct.value);

        SandboxController.Instance.LightFlashing(result, true);
        view.ShowResultWin(result);//显示闪光
        Model.Instance.records.Add(recordStruct);

        Model.Instance.AddBeadPlateRoadData(recordStruct.result);
        Model.Instance.CalculateBigRoadData();

        view.UpdateRecord(recordStruct, Model.Instance.curGame);
        //yield return new WaitForSeconds(IOCanvasModel.Instance.CfgData.CountDown);//等待倒计时时间
        //Debug.LogError("明牌等待倒计时结束完成,停止押注");
    }


#endregion

#region 后台设置相关
    /// <summary>
    /// 初始化后台界面
    /// </summary>
    private void OnInitIOCanvas()
    {
        view.InitIOCanvas();
    }

    /// <summary>
    /// 离开设置界面事件
    /// </summary>
    private void OnLeaveSetting()
    {
        Model.Instance.RemoveErrorCode(0);
        if (Model.Instance.errorCodeList.Count > 0)
        {
            if (FSMSystem.Instance.CurrentStateID != StateID.Error)
                FSMSystem.Instance.PerformTransition(Transition.Error);
            string errStr = Model.Instance.GetErrorStr(Model.Instance.errorCodeList[0]);
            PopTips.Instance.ShowSystemTips(errStr, -1);
            return;
        }
        HideTips();
        if (FSMSystem.Instance.CurrentStateID == StateID.Setting)
            FSMSystem.Instance.PerformTransition(Transition.Print);
    }

    /// <summary>
    /// 获取账户信息事件，统计账单数据并触发账单视图初始化
    /// </summary>
    private void OnGetAccount(SBoxAccount sBoxAccount)
    {
        if (sBoxAccount.result < 0)
        {
            IOPopTips.Instance.ShowTips(Utils.GetLanguage("ParamsWrong"));
            return;
        }
        else if (sBoxAccount.result > 0)
        {
            IOPopTips.Instance.ShowTips($"{Utils.GetLanguage("Wrong")}:{sBoxAccount.result}");
            return;
        }

        BillData billData = new BillData
        {
            playerAccountList = sBoxAccount.PlayerAccountList
        };
        for (int i = 0; i < sBoxAccount.PlayerAccountList.Count; i++)
        {
            var playerAccount = sBoxAccount.PlayerAccountList[i];
            billData.coinIn += playerAccount.CoinIn;
            billData.ticketOut += playerAccount.CoinOut;
            billData.scoreUp += playerAccount.ScoreIn;
            billData.scoreDown += playerAccount.ScoreOut;
            billData.bets += playerAccount.Bets;
            billData.wins += playerAccount.Wins;
            billData.credit += playerAccount.Credit;
        }
        IOCanvasModel.Instance.billData = billData;

        //拿十次
        // for (int i = 0; i < 10; i++)
        // {
        //     SBoxIdea.GetPrizePlayer(i+1);
        // }
        EventCenter.Instance.EventTrigger(EventHandle.INIT_BILL_VIEW);
    }
    void OnRefreshIOCanvas()
    {
        view.RefreshIOCanvas();
    }
#endregion

#region 硬件按键相关
    /// <summary>
    /// 按键测试事件（调试用）
    /// </summary>
    private void InputTest(KeyCode keyCode)//没啥用
    {
        switch (keyCode)
        {
            case KeyCode.W:
                break;
        }
    }

    /// <summary>
    /// 硬件长按按键事件
    /// </summary>
    private void OnHardwareKeyLongpress(ulong SBoxSwitch)
    {
        if (FSMSystem.Instance.CurrentStateID == StateID.Setting)
            return;
        switch (SBoxSwitch)
        {
            case SBOX_SWITCH.SWITCH_KEYBOARD_LITTLE_GAME:
            case SBOX_SWITCH.SWITCH_SET://长按进入设置
                view.OnSettingBtnClick();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 硬件短按按键事件
    /// </summary>
    private void OnHardwareKeyClick(ulong SBoxSwicth)
    {
        if (FSMSystem.Instance.CurrentStateID == StateID.Setting) return;
        switch (SBoxSwicth)
        {
            case SBOX_SWITCH.SWITCH_RULE:
                EventCenter.Instance.EventTrigger(EventHandle.PRINT_BTN_CLICK);
                break;
            case SBOX_SWITCH.SWITCH_KEYBOARD_LITTLE_GAME:
            case SBOX_SWITCH.SWITCH_SET:
                view.OnBillBtnClick();
                break;
            case SBOX_SWITCH.SWITCH_ENTER:
                view.OnEnterClick();//额，三方支付平台，没有资源显示
                break;
            case SBOX_SWITCH.SWITCH_PAYOUT:
                if (!IOTModel.Instance.LinkIOT)
                {
                    if (SBoxSandbox.PrinterState() < 0)                                  // 主机打印机不在线:忽略+本地提示
                    { PopTips.Instance.ShowSystemTips("PrinterNotConnected", 2, sendToClient: false); return; }
                    if (!SandboxController.Instance.CheckRefundIsEnough()) return;
                    NetMessageController.Instance.OnRefundRequest(1, null);
                    break;
                }
                float value = (float)IOCanvasModel.Instance.CfgData.TicketValue / IOCanvasModel.Instance.CfgData.scoreTicket;
                if (IOCanvasModel.Instance.CfgData.TicketValue == 0 || value == 0)
                {
                    PopTips.Instance.ShowSystemTips( "Pay setting error!");
                    return;
                }
                int coinOutCount = (int)(PlayerMgr.Instance.localPlayer.outState.credit / value);
                if (coinOutCount == 0)
                {
                    PopTips.Instance.ShowSystemTips( "Coin not enough!");
                    return;
                }
                TicketOutData ticketOutData = new TicketOutData
                {
                    type = (int)IOTModel.Instance.ticketOutType,
                    num = coinOutCount,
                    orderNum = IOTModel.Instance.LinkId,
                    seq = (Utils.GetTimeStamp() / 1000).ToString(),
                };
                IOTModel.Instance.unfinishTicketOutDatas.Add(ticketOutData);
                IoTPayment.Instance.DeviceTicketOut(IOTModel.Instance.PortId, ticketOutData, null);
                break;
            case SBOX_SWITCH.SWITCH_LEFT:
                view.ChangeHelpPanelActive();
                break;
        }
    }
#endregion

#region 硬件错误码，报错相关

    /// <summary>
    /// 激活事件，处理错误码和状态切换
    /// </summary>
    private void OnActive()
    {
        if (Model.Instance.errorCodeList.Count > 0)
        {
            if (FSMSystem.Instance.CurrentStateID == StateID.Setting)
                FSMSystem.Instance.PerformTransition(Transition.Error);
            string errStr = Model.Instance.GetErrorStr(Model.Instance.errorCodeList[0]);
            PopTips.Instance.ShowSystemTips(errStr, -1);
            return;
        }
        HideTips();
        if (FSMSystem.Instance.CurrentStateID == StateID.Error)
        {
            if (Model.Instance.wait4Setting)
                FSMSystem.Instance.PerformTransition(Transition.Setting);
            else
                FSMSystem.Instance.PerformTransition(Transition.EndRound);
        }
    }

    /// <summary>
    /// 爆机余额变更事件
    /// </summary>
    private void OnSBoxWinLockBalance(int balance)
    {
        IOCanvasModel.Instance.winLockBalance = balance;
    }

    /// <summary>
    /// 打印机状态变更事件，处理相关错误码
    /// </summary>
    private void OnSBoxPrinterStateCustom()
    {
        switch (SBoxIdea.PrinterStateCustom())
        {
            case 0:
                Model.Instance.RemoveErrorCode(4);
                IOCanvasModel.Instance.errorCodeListAboutPrint.Remove(4);
                Model.Instance.RemoveErrorCode(5);
                IOCanvasModel.Instance.errorCodeListAboutPrint.Remove(5);
                Model.Instance.RemoveErrorCode(10);
                IOCanvasModel.Instance.errorCodeListAboutPrint.Remove(10);
                Model.Instance.RemoveErrorCode(11);
                IOCanvasModel.Instance.errorCodeListAboutPrint.Remove(11);
                Model.Instance.RemoveErrorCode(12);
                IOCanvasModel.Instance.errorCodeListAboutPrint.Remove(12);
                break;
            case 1:
                Model.Instance.AddErrorCode(10, true);
                IOCanvasModel.Instance.errorCodeListAboutPrint.Add(10);
                Model.Instance.RemoveErrorCode(4, false);
                IOCanvasModel.Instance.errorCodeListAboutPrint.Remove(4);
                Model.Instance.RemoveErrorCode(5, false);
                IOCanvasModel.Instance.errorCodeListAboutPrint.Remove(5);
                Model.Instance.RemoveErrorCode(11, false);
                IOCanvasModel.Instance.errorCodeListAboutPrint.Remove(11);
                Model.Instance.RemoveErrorCode(12, false);
                IOCanvasModel.Instance.errorCodeListAboutPrint.Remove(12);
                break;
            case 2:
                Model.Instance.AddErrorCode(5, true);
                IOCanvasModel.Instance.errorCodeListAboutPrint.Add(5);
                Model.Instance.RemoveErrorCode(4, false);
                IOCanvasModel.Instance.errorCodeListAboutPrint.Remove(4);
                Model.Instance.RemoveErrorCode(10, false);
                IOCanvasModel.Instance.errorCodeListAboutPrint.Remove(10);
                Model.Instance.RemoveErrorCode(11, false);
                IOCanvasModel.Instance.errorCodeListAboutPrint.Remove(11);
                Model.Instance.RemoveErrorCode(12, false);
                IOCanvasModel.Instance.errorCodeListAboutPrint.Remove(12);
                break;
            case 3:
                Model.Instance.AddErrorCode(4, true);
                IOCanvasModel.Instance.errorCodeListAboutPrint.Add(4);
                Model.Instance.RemoveErrorCode(5, false);
                IOCanvasModel.Instance.errorCodeListAboutPrint.Remove(5);
                Model.Instance.RemoveErrorCode(10, false);
                IOCanvasModel.Instance.errorCodeListAboutPrint.Remove(10);
                Model.Instance.RemoveErrorCode(11, false);
                IOCanvasModel.Instance.errorCodeListAboutPrint.Remove(11);
                Model.Instance.RemoveErrorCode(12, false);
                IOCanvasModel.Instance.errorCodeListAboutPrint.Remove(12);
                break;
            case 4:
                Model.Instance.AddErrorCode(11, true);
                IOCanvasModel.Instance.errorCodeListAboutPrint.Add(11);
                Model.Instance.RemoveErrorCode(4, false);
                IOCanvasModel.Instance.errorCodeListAboutPrint.Remove(4);
                Model.Instance.RemoveErrorCode(5, false);
                IOCanvasModel.Instance.errorCodeListAboutPrint.Remove(5);
                Model.Instance.RemoveErrorCode(10, false);
                IOCanvasModel.Instance.errorCodeListAboutPrint.Remove(10);
                Model.Instance.RemoveErrorCode(12, false);
                IOCanvasModel.Instance.errorCodeListAboutPrint.Remove(12);
                break;
            case 5:
                Model.Instance.AddErrorCode(12, true);
                IOCanvasModel.Instance.errorCodeListAboutPrint.Add(12);
                Model.Instance.RemoveErrorCode(4, false);
                IOCanvasModel.Instance.errorCodeListAboutPrint.Remove(4);
                Model.Instance.RemoveErrorCode(5, false);
                IOCanvasModel.Instance.errorCodeListAboutPrint.Remove(5);
                Model.Instance.RemoveErrorCode(10, false);
                IOCanvasModel.Instance.errorCodeListAboutPrint.Remove(10);
                Model.Instance.RemoveErrorCode(11, false);
                IOCanvasModel.Instance.errorCodeListAboutPrint.Remove(11);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 移除相关错误码,错误码初始化?
    /// </summary>
    private void OnCodeSucceed(bool isclear)
    {
        Model.Instance.RemoveErrorCode(1, false);
        Model.Instance.RemoveErrorCode(2, false);
        Model.Instance.RemoveErrorCode(3, false);
        Model.Instance.RemoveErrorCode(8, false);
        Model.Instance.RemoveErrorCode(9, false);
        //打码清零
        if(isclear)
        {
            SQLite.Instance.ClearReacord();

            // foreach (var outState in PlayerMgr.Instance.SBoxIdeaPlayerOutState)
            // {
            //     if (PlayerMgr.Instance.playerDic.TryGetValue(outState.PlayerId, out Player player))
            //     {
            //         Debug.LogError($"clear-macid:{outState.PlayerId}");
            //         player.totalDataStruct.Clear();
            //         MsgInfo MsgHis = new MsgInfo//发送到分机
            //         {
            //             cmd = (int)S2C_CMD.S2C_UpdateHistoryData,
            //             id = -1,
            //             jsonData = JsonConvert.SerializeObject(player.totalDataStruct),
            //         };
            //         NetMgr.Instance.SendToClient(player.client,JsonConvert.SerializeObject(MsgHis));
            //     }
            // }
            //PlayerMgr.Instance.localPlayer.totalDataStruct.Clear();//补全清空本地玩家历史记录，本地玩家的似乎不包含在SBoxIdeaPlayerOutState里面
            //Kuarser：曾发现清账打码不清主机历史记录，重启之后历史记录消失，再试就不出现了，不知道是不是眼花26-6-22
            //切换为一种比较激进的写法，直接使用playerDic
            foreach (var player in PlayerMgr.Instance.playerDic)
            {
                Debug.LogError($"clear-macid:{player.Key}");
                player.Value.totalDataStruct.Clear();
                MsgInfo MsgHis = new MsgInfo//发送到分机
                {
                    cmd = (int)S2C_CMD.S2C_UpdateHistoryData,
                    id = -1,
                    jsonData = JsonConvert.SerializeObject(player.Value.totalDataStruct),
                };
                NetMgr.Instance.SendToClient(player.Value.client,JsonConvert.SerializeObject(MsgHis));
            }
            view.ResetHistoryRecord();
        }

    }
    #endregion

#region 第三方支付相关
    private void OnIOTRegisterDec()
    {
        view.InitQRCode();//原来的有资源,现在的没有这个资源要显示,所以不监听了
    }

    private void OnIOTCoinIn(CoinData coinData)
    {
        //IOTModel.Instance.LinkId = coinData.orderNum;
        //PlayerMgr.Instance.localPlayer.sBoxCoinInCount += Utils.CoinCount(PlayerMgr.Instance.localPlayer.sBoxCoinInCount, coinData.Num);
        //IoTPayment.Instance.ReplyCoinIn(IOTModel.Instance.PortId, coinData.Num, coinData.orderNum, true, "操作成功");
    }

    /// <summary>
    /// IOT出票事件处理
    /// </summary>
    private void OnIOTTicketOut(TicketOutData ticketOutData)
    {
        bool contains = false;
        foreach (var data in IOTModel.Instance.unfinishTicketOutDatas)
        {
            if (data.seq == ticketOutData.seq)
            {
                ticketOutData = data;
                contains = true;
                break;
            }
        }
        if (!contains) return;
        IOTModel.Instance.unfinishTicketOutDatas.Remove(ticketOutData);
        IOTModel.Instance.ticketOutFrame += ticketOutData.num;
        if (IOTModel.Instance.ticketOutFrame > 0)
            MonoMgr.Instance.StartCoroutine(IOTCoinOut());//额,不知道有啥用处,携程里面没有逻辑
    }
    private IEnumerator IOTCoinOut()
    {
        yield return new WaitForSeconds(0.01f);
        //if (IOTModel.Instance.ticketOutFrame > 0)
        //{
        //    PlayerMgr.Instance.localPlayer.sBoxCoinOutCount = Utils.CoinCount(PlayerMgr.Instance.localPlayer.sBoxCoinOutCount, 1);
        //    IOTModel.Instance.ticketOutFrame--;
        //    if (IOTModel.Instance.ticketOutFrame > 0)
        //        MonoMgr.Instance.StartCoroutine(IOTCoinOut());
        //}
    }
#endregion

#region 其他

    /// <summary>
    /// SBox版本号事件
    /// </summary>
    private void OnSBoxVersion(string version)
    {
        IOCanvasModel.Instance.sBoxVersion = version;
    }

    void OnRefreshBetCoin()
    {
        view.ShowBetCoinViewActive();//显示修改
    }
    void OnUpdateBetSwitch()
    {
        Debug.LogError("OnUpdateBetSwitch");
        Model.Instance.betCoinList.Clear();
        for (int i = 0; i < IOCanvasModel.Instance.BetSwitchList.Length && i < IOCanvasModel.Instance.BetSwitchCount; i++)
            Model.Instance.betCoinList.Add(IOCanvasModel.Instance.BetSwitchList[i]);
        //只修改了数值,没有更改视图显示,需要修改视图
        Debug.LogError($"Model.Instance.betCoinList{JsonConvert.SerializeObject(Model.Instance.betCoinList)}");
        MsgInfo msg = new MsgInfo
        {
            cmd = (int)S2C_CMD.S2C_SetbetCoinList,
            id = -1,
            jsonData = JsonConvert.SerializeObject(new BetCoinList() { betCoinList = Model.Instance.betCoinList})
        };
        NetMgr.Instance.SendToAllClient(JsonConvert.SerializeObject(msg));
        view.UpdateBetSwitch();
    }

    public void HideTips()
    {
        if (Model.Instance.wait4Setting)
            PopTips.Instance.ShowSystemTips( "About to enter settings", -1);
        else if (Model.Instance.errorCodeList.Count > 0)
        {
            string errStr = Model.Instance.GetErrorStr(Model.Instance.errorCodeList[0]);
            PopTips.Instance.ShowSystemTips(errStr, -1);
        }
        else if (Model.Instance.wait4Print)
            PopTips.Instance.ShowSystemTips("WaitingPrint", -1);
        else
            PopTips.Instance.HideTips();
    }

    internal void PlaySpineAnimation()
    {
        view.PlaySpineAnim2(2);
    }
#endregion

#region Add New 
    public void ShowLuckySeatGame()
    {
        //幸运座位小游戏
        view.PlayLuckySeatSound(Model.Instance.curRecord.luckyPlayerId);

        string str = Utils.GetLanguage("Congratulations") + "! ";
        str += IOCanvasModel.Instance.curlanguage == Language.en ? $"Player {Model.Instance.curRecord.luckyPlayerId} " : $"{Model.Instance.curRecord.luckyPlayerId}";
        str += $"{Utils.GetLanguage("WinLuckySeat")}!";
        view.SetLuckyTips(str, 10f);//显示小游戏幸运座位提示
                                    //PopTips.Instance.ShowSystemTips(str, 10);//旧版本的提示

        view.ShowLuckySeatCard();//显示幸运座位小游戏动画
        SandboxController.Instance.TurnOnTreasureLight();//显示幸运座位3个选择按钮灯

        SBoxIdea.BattLuckShow();//通知算法卡显示幸运座位小游戏动画//回调 OnSBoxLuckShow()

        //先给分机发消息:显示小游戏幸运座位卡牌
        // MsgInfo msgInfo = new MsgInfo
        // {
        //     cmd = (int)S2C_CMD.S2C_ShowLuckySeatCard,
        //     id = -1,
        //     jsonData = JsonConvert.SerializeObject(Model.Instance.curRecord),
        // };
        // NetMgr.Instance.SendToAllClient(JsonConvert.SerializeObject(msgInfo));
        // Debug.LogError("通知所有分机,显示小游戏了,分机自己那里判断是显示卡牌,还是显示恭喜文本");
    }

    public void ShowJackpot()
    {
        AudioManager.Instance.PlayVoiceEff("jackpot");
        MonoMgr.Instance.StartCoroutine(view.ShowCoinEff());
    }

    public void SetGameData()
    {
        view.UpdateRoundAndGameText();
    }


    /// <summary>
    /// 获取已完成游戏记录事件
    /// </summary>
    private void OnGetCompletedGame(int[] data)
    {
        Model.Instance.records.Clear();
        for (int i = 1; i < Model.MAX_GAME + 1; i++)
        {
            if (data[i] == 0)
                break;
            RecordStruct record = new RecordStruct
            {
                game = i,
                result = data[i] & 0x03,
                pair = new int[2]{data[i] >> 2 & 0x01, data[i] >> 3 & 0x01},
                spresult = new int[4]{data[i] >> 4 & 0x01, data[i] >> 5 & 0x01, data[i] >> 6 & 0x01, data[i] >> 7 & 0x01},
                luckyId = 0,
            };
            Model.Instance.records.Add(record);
            //int pokerTpye = Model.Instance.GetPokerData(record.value[0]).type;
            Model.Instance.AddBeadPlateRoadData(record.result);
            Model.Instance.CalculateBigRoadData();
        }

        //给分机发送100局结果
        MsgInfo msgInfo = new MsgInfo
        {
            cmd = (int)S2C_CMD.S2C_SetRecod,
            id = -1,
            jsonData = Utils.CompressString(JsonConvert.SerializeObject(Model.Instance.records)),
        };
        NetMgr.Instance.SendToAllClient(JsonConvert.SerializeObject(msgInfo));
    }

    public void ViewShowAllRoad()
    {
        view.RefreshAllRecords();
    }

    public void ViewShowTypeCount()
    {
        view.ShowTypeCount();
    }


    #endregion

}

