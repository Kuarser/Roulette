using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIBetSettlementView : MonoBehaviour
{
    private GameObject betSettlementPanel;
    private Text[] fiveBetText;//5个压分金额文本
    private Text[] fiveBetWinText;//5个压分赢分文本
    private Text totalBetWinScoreText;//总押注赢分文本
    private GameObject showObj;

    private Text[] bankerFiveBetText;//5个压分金额文本
    private Text[] bankerFiveBetWinText;//5个压分赢分文本
    private Text bankerWinText;//抢庄盈利文本
    private Text bankerTotalBetWinScoreText;//总押注赢分文本
    private Text realTotalWinText;//真总押注赢分文本

    private GameObject showObj2;

    private void Awake()
    {
        InitView();
    }

    void Start()
    {

    }

    private void InitView()
    {
        var popCanvas = GameObject.Find("PopCanvas").transform;
        betSettlementPanel = popCanvas.Find("BetSettlement").gameObject;
        showObj = betSettlementPanel.transform.Find("BG").gameObject;
        totalBetWinScoreText = betSettlementPanel.transform.Find("BG/content/totalWinScoreText").GetComponent<Text>();
        totalBetWinScoreText.text = "0";
        int betCount = Model.TYPE_COUNT;
        fiveBetText = new Text[betCount];
        fiveBetWinText = new Text[betCount];
        for (int i = 0; i < betCount; i++)
        {
            fiveBetText[i] = betSettlementPanel.transform.Find($"BG/content/Bets/BetText{i + 1}").GetComponent<Text>();
            fiveBetText[i].text = "0";
        }
        for (int i = 0; i < betCount; i++)
        {
            fiveBetWinText[i] = betSettlementPanel.transform.Find($"BG/content/Win/WinText{i + 1}").GetComponent<Text>();
            fiveBetWinText[i].text = "0";
        }

        //庄家的结算界面
        showObj2 = betSettlementPanel.transform.Find("BG_Banker").gameObject;
        bankerWinText = betSettlementPanel.transform.Find("BG_Banker/content/BankerText").GetComponent<Text>();
        bankerWinText.text = "0";
        bankerTotalBetWinScoreText = betSettlementPanel.transform.Find("BG_Banker/content/totalWinScoreText").GetComponent<Text>();
        bankerTotalBetWinScoreText.text = "0";
        realTotalWinText = betSettlementPanel.transform.Find("BG_Banker/content/RealTotalWinText").GetComponent<Text>();
        realTotalWinText.text = "0";
        int betCount2 = Model.TYPE_COUNT;
        bankerFiveBetText = new Text[betCount2];
        bankerFiveBetWinText = new Text[betCount2];
        for (int i = 0; i < betCount2; i++)
        {
            bankerFiveBetText[i] = betSettlementPanel.transform.Find($"BG_Banker/content/Bets/BetText{i + 1}").GetComponent<Text>();
            bankerFiveBetText[i].text = "0";
        }
        for (int i = 0; i < betCount2; i++)
        {
            bankerFiveBetWinText[i] = betSettlementPanel.transform.Find($"BG_Banker/content/Win/WinText{i + 1}").GetComponent<Text>();
            bankerFiveBetWinText[i].text = "0";
        }
    }

    public void ChangeBetSettlementPanelActive(bool flag)
    {
        //betSettlementPanel.SetActive(!betSettlementPanel.activeSelf);
        betSettlementPanel.SetActive(flag);
        Model.Instance.myTriggerDetectionCanEnterCount = flag ? Model.Instance.myTriggerDetectionCanEnterCount + 1 : Model.Instance.myTriggerDetectionCanEnterCount - 1;//总结界面显示触摸区域不能响应,总结界面隐藏触摸区域能响应
        if (betSettlementPanel.activeSelf)
        {
            //还有彩金,彩金是否需要重新一行?还是说和抢庄实利一行?还是说直接把总赢分拆成彩金和游玩结算
            //Debug.Log("播放一局游戏总结界面");
            if (Model.Instance.banckerData != null && PlayerMgr.Instance.localPlayer.macId == Model.Instance.banckerData.BankerPlayerId)
            {
                showObj.SetActive(false);
                showObj2.SetActive(true);
                RefreshBetBetSettlementTextByBanker();
            }
            else
            {
                showObj.SetActive(true);
                showObj2.SetActive(false);
                RefreshBetBetSettlementText();
            }
        }
    }
    /// <summary>
    /// 计算当个位置的值
    /// </summary>
    /// <param name="index">下标</param>
    /// <param name="record">本局开奖记录</param>
    /// <param name="luckyId">中奖小游戏类型</param>
    /// <returns></returns>
    private int CalculateSingleWinScore(int index, RecordStruct record, int luckyId)
    {
        var localPlayer = PlayerMgr.Instance.localPlayer.outState;

        // ==========================================
        // 大满贯（luckyId ==7）→ 全部按倍率算
        // ==========================================
        if (luckyId == 7)
        {
            return localPlayer.bets[index] * localPlayer.odds[index] / 10;
        }

        // ==========================================
        // 普通中奖:按九门判定,中奖按倍率,不中得0(无返分)
        // ==========================================
        bool[] wins = Model.CalcWinDoors(record);
        return wins[index]
            ? localPlayer.bets[index] * localPlayer.odds[index] / 10
            : 0;
    }

    /// <summary>
    /// 从算法卡下发的 PayoutData 中取本机玩家的每门赢分
    /// </summary>
    /// <returns>null 表示无数据（回退自算）</returns>
    private int[] GetLocalPlayerRoleWins()
    {
        var payoutData = Model.Instance.PayoutData;
        if (payoutData == null) return null;
        int playerId = PlayerMgr.Instance.localPlayer.outState.PlayerId;
        for (int i = 0; i < payoutData.Length; i++)
        {
            if (payoutData[i] != null && payoutData[i].PlayerId == playerId)
                return payoutData[i].RoleWins;
        }
        return null;
    }

    private void RefreshBetBetSettlementTextByBanker()//庄家结算界面
    {
        var curRecord = Model.Instance.curRecord;
        var localPlayer = PlayerMgr.Instance.localPlayer.outState;
        //中奖结果由 CalcWinDoors 内部判定
        //Debug.Log("中奖下标:" + result2);

        //显示押注分
        for (int i = 0; i < bankerFiveBetText.Length; i++)
        {
            bankerFiveBetText[i].text = localPlayer.bets[i].ToString();
        }

        int totalWin = 0;

        // 2. 遍历显示赢分
        int[] roleWins = GetLocalPlayerRoleWins();//官方每门赢分,可能为null
        for (int i = 0; i < bankerFiveBetWinText.Length; i++)
        {
            int winScore = roleWins != null
                ? roleWins[i]
                : CalculateSingleWinScore(i, curRecord, curRecord.luckyId);
            bankerFiveBetWinText[i].text = winScore.ToString();
            totalWin += winScore;
        }

        // 3. 庄家盈利
        bankerWinText.text = Model.Instance.banckerData != null ? Model.Instance.banckerData.BankerProfit.ToString() : "0";
        //totalWin += Model.Instance.banckerData.BankerProfit;

        // // 4. 校验 + 显示总赢分
        // if (totalWin != /*localPlayer.wins*/ PlayerMgr.Instance.localPlayer.outState.wins)
        // {
        //     Debug.LogError($"报错,计算分数对不上 => 计算:{totalWin} 实际:{PlayerMgr.Instance.localPlayer.outState.wins}");
        //     Debug.LogError($"旧:{JsonConvert.SerializeObject(localPlayer)}");
        //     Debug.LogError($"实时:{JsonConvert.SerializeObject(PlayerMgr.Instance.localPlayer.outState)}");
        //     //if (localPlayer.wins == 0)//在中大小王时,localPlayer.wins 就会为 0,而有要求中大小王就返分,所以应该有分,
        //     //    localPlayer.wins = totalWin;//借用一下
        //     if (PlayerMgr.Instance.localPlayer.outState.wins == 0)
        //     {
        //         PlayerMgr.Instance.localPlayer.outState.wins = totalWin;
        //     }
        // }

        if (curRecord.luckyId == 8)//如果中彩金
        {
            if (curRecord.extraData != null && localPlayer.PlayerId - 1 >= 0 && localPlayer.PlayerId - 1 < curRecord.extraData.Length)
                bankerTotalBetWinScoreText.text = totalWin + " + " + curRecord.extraData[localPlayer.PlayerId-1];
            else
            {
                bankerTotalBetWinScoreText.text = PlayerMgr.Instance.localPlayer.outState.wins.ToString();
                Debug.LogError("报错,中彩金,却没有彩金值");
            }
        }
        else
            bankerTotalBetWinScoreText.text = PlayerMgr.Instance.localPlayer.outState.wins.ToString();
        int bankerProfit = Model.Instance.banckerData != null ? Model.Instance.banckerData.BankerProfit : 0;
        realTotalWinText.text = (bankerProfit + PlayerMgr.Instance.localPlayer.outState.wins).ToString();
        //if (result2 == 5)//如果中大小王,返分//美人鱼 5
        //{
        //    int totalWin = 0;
        //    for (int i = 0; i < bankerFiveBetWinText.Length; i++)
        //    {
        //        //如果大满贯
        //        if (Model.Instance.curRecord.luckyId == 7)
        //            bankerFiveBetWinText[i].text = (Model.Instance.totalBets[i] * PlayerMgr.Instance.localPlayer.outState.odds[i] / 10).ToString();//自己计算,有算小游戏
        //        else
        //        {
        //            if (i == result2 - 1)
        //            {
        //                //bankerFiveBetWinText[i].text = PlayerMgr.Instance.localPlayer.outState.wins.ToString();//中奖者赢分
        //                //totalWin += PlayerMgr.Instance.localPlayer.outState.wins;
        //                bankerFiveBetWinText[i].text = (Model.Instance.totalBets[i] * PlayerMgr.Instance.localPlayer.outState.odds[i] / 10).ToString();
        //                totalWin += (Model.Instance.totalBets[i] * PlayerMgr.Instance.localPlayer.outState.odds[i] / 10);
        //            }
        //            else
        //            {
        //                bankerFiveBetWinText[i].text = PlayerMgr.Instance.localPlayer.outState.bets[i].ToString();//其他返分
        //                totalWin += PlayerMgr.Instance.localPlayer.outState.bets[i];
        //            }
        //        }
        //    }
        //    bankerWinText.text = Model.Instance.banckerData.BankerProfit.ToString();
        //    totalWin += Model.Instance.banckerData.BankerProfit;
        //    //bankerTotalBetWinScoreText.text = totalWin.ToString();
        //    if (totalWin != PlayerMgr.Instance.localPlayer.outState.wins)
        //    {
        //        Debug.LogError("报错,计算分数对不上");
        //    }
        //    bankerTotalBetWinScoreText.text = PlayerMgr.Instance.localPlayer.outState.wins.ToString();
        //}
        //else
        //{
        //    //int result = Model.Instance.GetRewardResult(Model.Instance.curRecord.value);
        //    for (int i = 0; i < bankerFiveBetWinText.Length; i++)
        //    {
        //        //如果大满贯
        //        if (Model.Instance.curRecord.luckyId == 7)
        //            bankerFiveBetWinText[i].text = (Model.Instance.totalBets[i] * PlayerMgr.Instance.localPlayer.outState.odds[i] / 10).ToString();//自己计算,有算小游戏
        //        else
        //        {
        //            bankerFiveBetWinText[i].text = "0";//其他赢分
        //            if (i == result2 - 1)
        //            {
        //                //bankerFiveBetWinText[i].text = Model.Instance.banckerData.TotalWins.ToString();//中奖者赢分,不能用抢庄的,抢庄的不会累计小游戏
        //                bankerFiveBetWinText[i].text = (Model.Instance.totalBets[i] * PlayerMgr.Instance.localPlayer.outState.odds[i] / 10).ToString();//自己计算,有算小游戏
        //            }
        //        }
        //    }
        //    bankerWinText.text = Model.Instance.banckerData.BankerProfit.ToString();//需要负号艺术字
        //    bankerTotalBetWinScoreText.text = PlayerMgr.Instance.localPlayer.outState.wins.ToString();
        //}
    }
    private void RefreshBetBetSettlementText()//更新结算面板的押注数值显示//闲家结算界面
    {
        var curRecord = Model.Instance.curRecord;
        var localPlayer = PlayerMgr.Instance.localPlayer.outState;
        //中奖结果由 CalcWinDoors 内部判定
        //Debug.Log("中奖下标:" + result);

        // 显示押注分
        for (int i = 0; i < fiveBetText.Length; i++)
        {
            fiveBetText[i].text = localPlayer.bets[i].ToString();
        }

        int totalWin = 0;// 遍历显示每一项赢分
        int[] roleWins = GetLocalPlayerRoleWins();//官方每门赢分,可能为null
        for (int i = 0; i < fiveBetWinText.Length; i++)
        {
            int winScore = roleWins != null
                ? roleWins[i]
                : CalculateSingleWinScore(i, curRecord, curRecord.luckyId);
            fiveBetWinText[i].text = winScore.ToString();
            totalWin += winScore;
        }


        // // 数据校验
        // if (totalWin != /*localPlayer.wins*/PlayerMgr.Instance.localPlayer.outState.wins)//不能用固定的赢分,固定的话,那时候赢分还为0 
        // {
        //     Debug.LogError($"闲家结算报错：计算值={totalWin} 实际值={PlayerMgr.Instance.localPlayer.outState.wins}");

        //     if (PlayerMgr.Instance.localPlayer.outState.wins == 0)
        //     {
        //         PlayerMgr.Instance.localPlayer.outState.wins = totalWin;
        //     }
        // }

        // ======================
        // 正常显示 / 彩金显示（和庄家风格完全统一）
        // ======================
        if (curRecord.luckyId == 8) // 彩金
        {
            if (curRecord.extraData != null && curRecord.extraData.Length > 0)
            {
                totalBetWinScoreText.text = $"{totalWin} + {curRecord.extraData[0]}";
            }
            else
            {
                totalBetWinScoreText.text = totalWin.ToString();
            }
        }
        else
        {
            //totalBetWinScoreText.text = localPlayer.wins.ToString();
            totalBetWinScoreText.text = PlayerMgr.Instance.localPlayer.outState.wins.ToString();
        }

        //if (result == 5)//如果中大小王,返分//美人鱼 5
        //{
        //    int totalWin = 0;
        //    for (int i = 0; i < fiveBetWinText.Length; i++)
        //    {
        //        if (i == result - 1)
        //        {
        //            fiveBetWinText[i].text = (PlayerMgr.Instance.localPlayer.outState.bets[i] * 20).ToString();//20太绝对的了,应该随时可变,用变量
        //            totalWin += (PlayerMgr.Instance.localPlayer.outState.bets[i] * 20);
        //        }
        //        else
        //        {
        //            fiveBetWinText[i].text = PlayerMgr.Instance.localPlayer.outState.bets[i].ToString();
        //            totalWin += PlayerMgr.Instance.localPlayer.outState.bets[i];
        //        }
        //    }
        //    if (totalWin != PlayerMgr.Instance.localPlayer.outState.wins)
        //    {
        //        Debug.LogError("报错,计算分数对不上");
        //    }
        //    totalBetWinScoreText.text = PlayerMgr.Instance.localPlayer.outState.wins.ToString();
        //}
        //else
        //{
        //    //int result = Model.Instance.GetRewardResult(Model.Instance.curRecord.value);
        //    for (int i = 0; i < fiveBetWinText.Length; i++)
        //    {
        //        fiveBetWinText[i].text = "0";
        //        if (i == result - 1)
        //        {
        //            fiveBetWinText[i].text = PlayerMgr.Instance.localPlayer.outState.wins.ToString();
        //        }
        //    }
        //    totalBetWinScoreText.text = PlayerMgr.Instance.localPlayer.outState.wins.ToString();
        //}
    }
}
