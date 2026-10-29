using SBoxApi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIBankerViewHistory : MonoBehaviour
{
    //抢庄
    private float qiangZhuangInterval = 0;
    //private Button qiangZhuangBtn;//
    public List<Text> languageTextList;                           //多语言文字列表
    public List<Text> roleBetTextList;                           //每种花色的押分列表
    public List<Text> roleWinTextList;                           //每种花色的赢分列表
    public List<Text> seatTextList;                              //正在抢庄的座位号列表
    public Text backerSeatText;                                  //庄家座位号
    public Text backerWinText;                                   //抢庄盈利
    public Text backerProfitText;                                    //抢庄实力
    public Text backerCutText;                                       //抢庄抽水
    public Text backerCutOddsText;                                     //抢庄抽水赔率
    public Text roleBetTotalText;                                   //押注合计
    public Text roleWinTotalText;                                   //赢分合计
    private Color curSeatColor;//当前座位号颜色,满足条件的座位号显示默认颜色,不满足条件的座位号显示红色

    private bool isInit = false;

    void Awake()
    {
        InitView();
    }
    private void InitView()
    {
        if(isInit) return;
        var bankerView = this.transform;
        var seatList = bankerView.Find("Content/SeatList").transform;
        var title1 = bankerView.Find("Content/title1").transform;
        languageTextList = title1.GetComponentsInChildren<Text>().ToList();
        if (seatTextList == null || seatTextList.Count <= 0)
        {
            seatTextList = new List<Text>();
        }
        for (int i = 0; i < seatList.childCount; i++)
        {
            var seat = seatList.GetChild(i);
            seatTextList.Add(seat.GetComponent<Text>());
            seatTextList[i].text = "";
        }
        curSeatColor = seatTextList[0].color;//默认座位号颜色,满足条件的座位号显示默认颜色,不满足条件的座位号显示红色

        var roleWinList = bankerView.Find("Content/RoleWinList").transform;
        if (roleWinTextList == null || roleWinTextList.Count <= 0)
        {
            roleWinTextList = new List<Text>();
        }
        for (int i = 0; i < roleWinList.childCount; i++)
        {
            var roleWin = roleWinList.GetChild(i);
            roleWinTextList.Add(roleWin.GetComponent<Text>());
            roleWinTextList[i].text = "";
        }

        var roleBetList = bankerView.Find("Content/RoleBetList").transform;
        if (roleBetTextList == null || roleBetTextList.Count <= 0)
        {
            roleBetTextList = new List<Text>();
        }
        for (int i = 0; i < roleBetList.childCount; i++)
        {
            var roleBet = roleBetList.GetChild(i);
            roleBetTextList.Add(roleBet.GetComponent<Text>());
            roleBetTextList[i].text = "";
        }

        backerSeatText = bankerView.Find("Content/BackerSeat").GetComponent<Text>();
        backerSeatText.text = "";

        backerWinText = bankerView.Find("Content/BackerList/backerWin").GetComponent<Text>();
        backerWinText.text = "";

        backerProfitText = bankerView.Find("Content/BackerList/backerProfit").GetComponent<Text>();
        backerProfitText.text = "";

        backerCutText = bankerView.Find("Content/BackerList/backerCut").GetComponent<Text>();
        backerCutText.text = "";

        backerCutOddsText = bankerView.Find("Content/title1/BankerCutOdds").GetComponent<Text>();
        backerCutOddsText.text = Utils.GetLanguage("Taxes") + " " + "‰";//显示抽水赔率,比如:0.02 就显示 20‰,第一时间,Model.Instance.banckerData.CutOdds没有值所以先不显示用这个

        roleBetTotalText = bankerView.Find("Content/TotalList/totalRoleBet").GetComponent<Text>();
        roleBetTotalText.text = "";

        roleWinTotalText = bankerView.Find("Content/TotalList/totalRoleWin").GetComponent<Text>();
        roleWinTotalText.text = "";
        isInit = true;
    }
    public void RefreshBancker(BankerDataStruct bankerDataStruct,bool isShowCard,bool isJoker)
    {
        InitView();
        ResetBanker();
            if (bankerDataStruct == null)
            return;

        //抢庄：只有庄闲两门计入抢庄玩法
        for (int i = 0; i < roleBetTextList.Count; i++)
            roleBetTextList[i].text = i <= 1 ? bankerDataStruct.roleBetTotalTextList[i].ToString() : "";//只显示庄闲两门押分
        //座位：按 Priority 顺序显示机台号，PriorityLights 控制颜色
        for (int i = 0; i < seatTextList.Count; i++)
        {
            int machineId = i < bankerDataStruct.Priority.Length ? bankerDataStruct.Priority[i] : 0;
            seatTextList[i].text = machineId == 0 ? "" : machineId.ToString();
            bool lightOn = i < bankerDataStruct.PriorityLights.Length && bankerDataStruct.PriorityLights[i] != 0;
            seatTextList[i].color = lightOn ? curSeatColor : Color.red;
        }
        int textSize = IOCanvasModel.Instance.curlanguage == Language.en?13:17;
        foreach (var item in languageTextList)
        {
            item.fontSize = textSize;
        }
        //抽水费率直接用快照 WaterRateBasisPoints：200 = 2% = 20‰
        backerCutOddsText.text = Utils.GetLanguage("Taxes")+ " " + (bankerDataStruct.WaterRateBasisPoints / 10) + "‰";
        //backerCutText.text = Model.Instance.banckerData.Cut.ToString();//显示抽水金额,比如:1000

        backerSeatText.text = bankerDataStruct.BankerSeat == 0 ? "" : bankerDataStruct.BankerSeat.ToString();//停止押注后刷新庄家信息,
        //Debug.LogError("RefreshBancker");
        if (bankerDataStruct == null) return;
        if (bankerDataStruct.BankerSeat == 0 || isShowCard || isJoker) return;

        //预估直接使用算法卡下发、已写入快照的值
        for (int i = 0; i < roleWinTextList.Count; i++)
        {
            roleWinTextList[i].text = i == 0 ? bankerDataStruct.EstimateBankerProfit.ToString()
                : i == 1 ? bankerDataStruct.EstimatePlayerProfit.ToString() : "";
        }
        roleWinTotalText.text = bankerDataStruct.roleWinTotalText.ToString();//所有类型的总得分合计,没有计算小游戏倍率的情况

        backerCutText.text = bankerDataStruct.backerCutText.ToString();//税费直接获取,肯定会有值

        backerWinText.text = bankerDataStruct.backerWinText.ToString();//抢庄盈利
        backerProfitText.text = bankerDataStruct.backerProfitText.ToString();//抢庄实利
        roleBetTotalText.text = bankerDataStruct.roleBetTotalText.ToString();//押注合计
    }
        public void ResetBanker()
    {
        //Debug.LogError("ResetBanker");
        for (int i = 0; i < roleWinTextList.Count; i++)//每种卡片类型的总赢分
        {
            roleWinTextList[i].text = "";
            roleBetTextList[i].text = "";
        }
        backerCutText.text = "";
        backerSeatText.text = "";
        backerWinText.text = "";
        backerProfitText.text = "";
        roleBetTotalText.text = "";
        roleWinTotalText.text = "";

        //Controller.Instance.backerBidCount = 0;
    }
}
