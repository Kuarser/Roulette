using GameUtil;
using Newtonsoft.Json;
using SBoxApi;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//分机:分机投币,心跳包连接主机
public class Player
{

    private int _macId;//分机号
    public int macId
    {
        set { _macId = value; }
        get { return _macId; }
    }
    private WebSockets.ClientConnection _client;

    public WebSockets.ClientConnection client
    {
        set { _client = value; }
        get { return _client; }
    }

    private bool isOnline;
    public bool IsOnline
    {
        set
        {
            isOnline = value;
            LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_STATE_EXIST, isOnline);
        }
        get { return isOnline; }
    }

    private bool isChangeState;
    public bool IsChangeState
    {
        get
        {
            bool value = isChangeState;
            isChangeState = false;
            return value;
        }
    }

    public int inState;
    public int betUnitIdx;
    public int coinInCount;
    public int coinInChange;
    public int coinOutCount;
    public int coinOutChange;
    public int sBoxCoinInCount;
    public int sBoxCoinOutCount;
    public bool coinOuting;
    public SBoxIdeaPlayerOutState outState;

    private Timer heartBeatTimer;
    private float lastHeartBeatTime;
    private const float HeartBeatDelta = 10;
    public List<TotalDataStruct> totalDataStruct;//记录快照数据列表 大小暂定10
    //kuarser：仅维护一个数据结构体用于记录当前数据，玩家每次状态改变时更新这个数据结构体，
    // 并保存到数据库中，数据库只保存最新的一组记录，玩家连接时读取这组记录来恢复状态
    private Timer changeTimer;

    /// <summary>抢庄按键来源</summary>
    public enum BankerKeySource { Hardware = 0, Virtual = 1 }

    private bool bankerHardwareDown;//实体ENTER键按下标志
    private bool bankerVirtualDown;//虚拟抢庄键按下标志

    public void DealWithCoinIn(int coinInFromClient)
    {
        if (coinInFromClient == coinInCount) return;
        if (coinInFromClient > coinInCount)
            coinInChange += coinInFromClient - coinInCount;
        else
            coinInChange += coinInFromClient + (8192 - coinInCount);
        coinInCount = coinInFromClient;
        PlayerPrefs.SetInt($"player{macId}coinInCount", coinInCount);
        PlayerPrefs.GetInt($"player{macId}coinInCount", -1);
    }

    public void DealWithCoinOut(int coinOutFromClient)
    {
        if (coinOutFromClient == coinOutCount) return;
        if (coinOutFromClient > coinOutCount)
            coinOutChange += coinOutFromClient - coinOutCount;
        else
            coinOutChange += coinOutFromClient + (8192 - coinOutCount);
        coinOutCount = coinOutFromClient;
        PlayerPrefs.SetInt($"player{macId}coinOutCount", coinOutCount);
        PlayerPrefs.GetInt($"player{macId}coinOutCount", -1);
    }

    public bool CheckCoinIn()
    {
        if (coinInChange <= 0) return false;
        if (coinInChange > 250)
        {
            sBoxCoinInCount = Utils.SBoxCoinCount(sBoxCoinInCount, 250);
            coinInChange -= 250;
        }
        else
        {
            sBoxCoinInCount = Utils.SBoxCoinCount(sBoxCoinInCount, coinInChange);
            coinInChange = 0;
        }
        return true;
    }

    public bool CheckCoinOut()
    {
        if (coinOutChange <= 0) return false;
        if (coinOutChange > 250)
        {
            sBoxCoinOutCount = Utils.SBoxCoinCount(sBoxCoinOutCount, 250);
            coinOutChange -= 250;
        }
        else
        {
            sBoxCoinOutCount = Utils.SBoxCoinCount(sBoxCoinOutCount, coinOutChange);
            coinOutChange = 0;
        }
        return true;
    }
    private InState lastInStateData = null;//上一个周期最后的inStateData，将作为下一个周期的初始状态
    //如果这个周期除了初始状态没有更新instate，下个周期的初始状态为空，
    //检测到初始状态为空同时这一个周期没有新的instate，就不往算法卡发信息了，
    //-------------------------------
    //单周期流程：
    //周期一开始如果lastInStateData不为空，就用lastInStateData作为融合基底（似乎直接用lastInStateData也可以，空也是空）
    //周期中lastInStateData一直更新为最后融合的instate（可能出现对空融合和深拷贝）
    //期间一直融合（或逻辑）新来的instate状态，维护一个bool属性标记有没有更新过instate
    //最后发送处理后的instate，如果bool属性标记发现没更新过instate，将退化lastInStateData，
    //lastInStateData不空的情况退化为空，
    //lastInStateData在空状态退化就意味着该玩家不触发消息发送
    //总的来说只要有一个玩家触发消息发送就要全发
    //※kuarser：lastInStateData暂未实现
    public void PlayerChangeInState(InState inStateData)//instate
    {
        CheckHeartBeat();
        //Debug.LogError($"PlayerChangeInState-id:{macId}");
        
        // if((inStateData.inState & (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_SET) != 0)//※
        // {
        //     Debug.LogError("抢庄按钮是true");
        // }
        // else
        // {
        //     Debug.LogError("抢庄按钮是false");
        // }

        if (Model.Instance.winLockPlayers.Contains(macId) || !IsOnline)
        {
            int tempState = 0;
            tempState += ((inStateData.inState & (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_PAYOUT) != 0) ? (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_PAYOUT : 0;
            tempState += ((inStateData.inState & (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_SCORE_DOWN) != 0) ? (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_SCORE_DOWN : 0;
            inState = (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_STATE_EXIST + tempState;
            return;
        }
        inState = inStateData.inState;
        isChangeState = inStateData.isChange;
        betUnitIdx = inStateData.betUnitIdx;//新增加
        if (inStateData.isChange)
        {
            if (changeTimer == null)
            {
                changeTimer = Timer.DelayAction(0.02f, () =>
                {
                    EventCenter.Instance.EventTrigger(EventHandle.PLAYER_INSTATE_CHANGE);
                });
            }
            else
            {
                if (changeTimer.isDone)
                    changeTimer.Restart();
            }
        }
    }

    private void CheckHeartBeat()
    {
        if (heartBeatTimer == null)
        {
            heartBeatTimer = Timer.LoopAction(3.0f, (_) =>
            {
                float delta = Time.time - lastHeartBeatTime;
                if (delta > HeartBeatDelta)
                {
                    IsOnline = false;
                    heartBeatTimer.Cancel();
                }
            });
        }
        else if (heartBeatTimer.isCancelled)
            heartBeatTimer.Restart();

        lastHeartBeatTime = Time.time;
    }

    public void LocalPlayerChangeInState(int state, bool down)//测试instate
    {
       if (Model.Instance.winLockPlayers.Contains(macId) || !IsOnline)
        {
            int tempState = 0;
            //((PAYOUT || SCORE_DOWN || KEY_ROOT) && down)
            if((state == (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_PAYOUT ||
            state == (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_SCORE_DOWN ||
            state == (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_ROOT) && down)
                tempState += state;
            
            inState = (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_STATE_EXIST + tempState;
            return;
        }
        bool isStateChange = false;
        if (down && (inState & state) == 0)
        {
            inState += state;  
            isStateChange = true;
        }
        else if (!down && (inState & state) == state)
        {
            inState -= state;
            isStateChange = true;
        }
        isChangeState = true;
        if(isStateChange)
            EventCenter.Instance.EventTrigger(EventHandle.PLAYER_INSTATE_CHANGE);

        //仅在实际按下按键时判断积分,松键/抢庄键复位(down=false)不触发提示
        if (down && outState != null && IOCanvasModel.Instance != null && IOCanvasModel.Instance.CfgData != null)
        {
            bool isBetKey = FSMSystem.Instance.CurrentStateID == StateID.Bet &&
                            ((state & (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_A) != 0 ||
                             (state & (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_B) != 0 ||
                             (state & (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_C) != 0 ||
                             (state & (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_D) != 0 ||
                             (state & (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_E) != 0 ||
                             (state & (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_F) != 0 ||
                             (state & (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_G) != 0 ||
                             (state & (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_H) != 0 ||
                             (state & (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_I) != 0);
            bool isPayoutOrScoreDown = (state & (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_PAYOUT) != 0 ||
                                       (state & (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_SCORE_DOWN) != 0||
                                       (state & (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_SET) != 0;

            if (isBetKey)
            {
                if (outState.credit <= IOCanvasModel.Instance.CfgData.MinBet)
                {
                    PopTips.Instance.ShowSystemTips("TotalScoreNotEnough", sendToClient: false, isShowInLittle:false);  //硬件按钮压分时积分不足,提示
                }
            }
            else if (isPayoutOrScoreDown)
            {
                if (outState.credit <= 0)
                {
                    PopTips.Instance.ShowSystemTips("TotalScoreNotEnough", sendToClient: false, isShowInLittle:false);  //硬件按钮压分时积分不足,提示
                }
            }
        }
    }

    /// <summary>抢庄键按下/抬起:只记录来源标志,再由两个标志或运算得到抢庄按键真正的状态</summary>
    public void SetBankerKeyState(BankerKeySource source, bool down)
    {
        if (down && !Model.Instance.canBidBanker) return;//明牌时阻塞,与旧行为一致
        if (source == BankerKeySource.Hardware) bankerHardwareDown = down;
        else bankerVirtualDown = down;
        ApplyBankerKeyState();
    }

    /// <summary>两个按钮状态做或运算,转为抢庄按键真正的状态</summary>
    private void ApplyBankerKeyState()
    {
        LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_SET,
            bankerHardwareDown || bankerVirtualDown);
    }

    /// <summary>清空两个来源标志并同步状态位</summary>
    public void ResetBankerKeyState()
    {
        bankerHardwareDown = false;
        bankerVirtualDown = false;
        ApplyBankerKeyState();
    }

    public void SendToClient(string data)
    {
        NetMgr.Instance.SendToClient(client, data);
    }

    public void OnUpdateSQLRecord2()
    {
        string data = JsonConvert.SerializeObject(totalDataStruct);
        SQLite.Instance.UpdateRecordData2(_macId,data);//更新历史记录
    }
}