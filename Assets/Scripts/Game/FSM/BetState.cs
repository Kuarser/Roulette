

using Newtonsoft.Json;
using SBoxApi;
using System.Collections;
using UnityEngine;

public class BetState : FSMState
{
    //是新一局从prepareState进入,是新一轮从printState进入
    //进入后,玩家可以进行投注,如果倒计时结束或者玩家点击停止投注按钮,则进入rewardState,
    ////如果在投注过程中发生错误,则进入errorState,比如压分不足,总积分不足,投退币异常

    public BetState()
    {
        stateID = StateID.Bet;
        AddTransition(Transition.Reward, StateID.Reward);
        //AddTransition(Transition.Error, StateID.Error);
    }

    void AddEventListenr()
    {
        EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_BETS_STOP, OnBetsStop);
        EventCenter.Instance.AddEventListener<SBoxBattleLeadData>(SBoxEventHandle.SBOX_BATTLE_LEAD_START, OnBattleLeadStart);
    }

    void RemoveEventListenr()
    {
        EventCenter.Instance.RemoveEventListener<int>(SBoxEventHandle.SBOX_BETS_STOP, OnBetsStop);
        EventCenter.Instance.RemoveEventListener<SBoxBattleLeadData>(SBoxEventHandle.SBOX_BATTLE_LEAD_START, OnBattleLeadStart);
    }

    public override void DoBeforeEntering()
    {
        // 轮盘模式：押分窗口由轮盘自己管理，不启动宿主倒计时/算法卡流程。
        if (Model.Instance.rouletteFlow) return;

        AddEventListenr();

        Controller.Instance.OnGameStart();
        //MonoMgr.Instance.StartCoroutine(CountDownEnumerator());//开始倒计时
    }

    public override void DoBeforeLeaving()
    {
        if (!Model.Instance.rouletteFlow) RemoveEventListenr();
    }


    private void OnBetsStop(int result)
    {
        int ran = Utils.GetRandom(0, 2);
        AudioManager.Instance.BgmVolumScale = 0.3f * AudioManager.Instance.SoundVolumScale;
        AudioManager.Instance.PlayMusic($"rewardBgm{ran}");//随机播放开奖声音

        MsgInfo msgInfo = new MsgInfo
        {
            cmd = (int)S2C_CMD.S2C_StopBet,
            id = -1,
            jsonData = ran.ToString()
        };
        NetMgr.Instance.SendToAllClient(JsonConvert.SerializeObject(msgInfo));//通知分机停止压分

        //SendBetsToNetJackpotDivice();
        Debug.LogError("before SendBetData2NetJackpot");
        Controller.Instance.SendBetData2NetJackpot();

        SBoxIdea.BattleLeadStart();

        SandboxController.Instance.TurnOffBetLight();//停止压分,硬件压分按钮灯关闭
    }


    //private void OnBattleLeadStart(SBoxBattleLeadData arg0)
    //{
    //    FSMSystem.Instance.PerformTransition(Transition.Reward);
    //}

    /// <summary>
    /// 小游戏开奖开始事件//从算法卡获取本局游戏数据
    /// </summary>
    private void OnBattleLeadStart(SBoxBattleLeadData sBoxBattleLeadData)
    {
        //Debug.Log("OnBattleLeadStart");
        if (sBoxBattleLeadData.result < 0)
        {
            Debug.LogError("BattleLeadStart 参数错误");
            return;
        }
        else if (sBoxBattleLeadData.result > 0)
        {
            switch (sBoxBattleLeadData.result)
            {
                default:
                    break;
            }
            return;
        }

        //给本地本局游戏数据 补充数据
        Model.Instance.curRecord.luckyPlayerId = sBoxBattleLeadData.LuckPlayerId;
        Model.Instance.curRecord.bankerID = sBoxBattleLeadData.BankerID;
        Model.Instance.curRecord.bankerProfit = sBoxBattleLeadData.BankerProfit;
        if (Model.Instance.curRecord.bankerProfit != 0)
        {
            //FakeCredit 这里暂停刷新分数
            Model.Instance.fadeCredit = true;//暂停本机刷新分数
            Debug.LogError("Model.Instance.fadeCredit = true");
            MsgInfo pauseMsg = new MsgInfo//通知分机暂停刷新分数
            {
                cmd = (int)S2C_CMD.S2C_PauseFadeCredit,
                id = -1,
            };
            NetMgr.Instance.SendToAllClient(JsonConvert.SerializeObject(pauseMsg));
        }
        Model.Instance.curRecord.luckyId = Model.Instance.curRecord.luckyId == 6 ? 6 : sBoxBattleLeadData.LuckId;
        Model.Instance.curRecord.extraData = sBoxBattleLeadData.Data;
        if(sBoxBattleLeadData.HasPlayerPayouts)//每个玩家每一门的赢分
            Model.Instance.PayoutData = sBoxBattleLeadData.PlayerPayouts;

        DealWithLeadData();

        NetMgr.Instance.SendToAllClient(JsonConvert.SerializeObject(new MsgInfo
        {
            cmd = (int)S2C_CMD.S2C_Reward,
            id = -1,
            jsonData = JsonConvert.SerializeObject(Model.Instance.curRecord)
        }));//通知分机开奖了,并发给分机本局完整游戏数据

        FSMSystem.Instance.PerformTransition(Transition.Reward);//切换状态,实际上就是执行下面的 OnReward()
    }

    private void DealWithLeadData()
    {
#if UNITY_EDITOR
        //编辑器测试小游戏用
        if (Controller.Instance.isTestLittleGame && Application.isEditor)
        {
            //Blizz
            //幸运座位测试
            //指定幸运座位中将玩家Id
            Model.Instance.curRecord.luckyPlayerId = 1;//1:主机测试,5:分机测试,3：分机测试
            switch (Controller.Instance.testLittleGame)
            {
                case 1:
                    //加倍测试
                    //int result = Model.Instance.GetRewardResult(Model.Instance.curRecord.value) - 1;
                    Model.Instance.curRecord.luckyId = 4;
                    //加 ? 倍
                    //Model.Instance.curRecord.extraData[result] = 2;
                    Model.Instance.curRecord.extraData[0] = Utils.GetRandom(1, 9);
                    break;

                case 2:
                    //翻倍测试
                    Model.Instance.curRecord.luckyId = 5;
                    break;

                case 3:
                    //押分全中(大满贯)测试
                    Model.Instance.curRecord.luckyId = 7;
                    break;

                case 4:
                    //彩金测试
                    Model.Instance.curRecord.luckyId = 8;
                    break;
                case 5:
                    //明牌测试
                    Model.Instance.curRecord.luckyId = 6;
                    break;

                default:
                    break;
            }
        }
#endif
    }

    //private IEnumerator CountDownEnumerator()
    //{
    //    do
    //    {
    //        SBoxIdea.BetsCountDown(Model.Instance.curCountDown);

    //        yield return new WaitForSeconds(1f);
    //        Model.Instance.curCountDown--;
    //        if (Model.Instance.curCountDown == 5)
    //            AudioManager.Instance.PlaySoundEff("countDown");
    //    }
    //    while (Model.Instance.curCountDown >= 0);

    //    SBoxIdea.BetsStop();
    //}

}
