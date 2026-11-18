using Newtonsoft.Json;
using SBoxApi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIBankerView : MonoBehaviour
{
    //抢庄
    private float qiangZhuangInterval = 0;
    //private Button qiangZhuangBtn;//
    private GameObject qiangZhuangGreyImage;
    private GameObject AnimationEffect;
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

    //辅助
    private int curBankerId = 0;//当前庄家号
    private bool isClickBanker = true;
    private Color curSeatColor;//当前座位号颜色,满足条件的座位号显示默认颜色,不满足条件的座位号显示红色
    private bool isFirstFlag = false;
    private bool isCheck = false;//检查按钮状态,并及时改变
    private float bidSoundCoolDownTimer = 0;//抢庄音效冷却计时器
    private void Awake()
    {
        InitView();
    }
    private void OnEnable()
    {
        EventCenter.Instance.AddEventListener(EventHandle.CHANGE_LANGUAGE, SetTextSize);
    }
    private void OnDisable()
    {
        EventCenter.Instance.RemoveEventListener(EventHandle.CHANGE_LANGUAGE, SetTextSize);
    }

    void Start()
    {
        if (!View.OriginalBaccaratUiPresent) return;
        InitViewData();
        curSeatColor = seatTextList[0].color;//默认座位号颜色,满足条件的座位号显示默认颜色,不满足条件的座位号显示红色
    }

    private void InitView()
    {
        if (!View.OriginalBaccaratUiPresent) return;
        var UICanvas = GameObject.Find("UICanvas").transform;
        //抢庄
        var bankerView = UICanvas.Find("Center/BankerView");
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

        //抢庄按钮
        //qiangZhuangBtn = UICanvas.Find("Bottom/qiangzhuangBtn").GetComponent<Button>();
        qiangZhuangGreyImage = UICanvas.Find("Bottom/qiangzhuangBtn/grey").gameObject;
        qiangZhuangGreyImage.SetActive(true);
        AnimationEffect = UICanvas.Find("Bottom/qiangzhuangBtn/Animator").gameObject;
        AnimationEffect.SetActive(false);

    }

    private void InitViewData()
    {
        if (!View.OriginalBaccaratUiPresent) return;
        EventCenter.Instance.AddEventListener(EventHandle.CHANGE_LANGUAGE, () =>
        {
            backerCutOddsText.text = Utils.GetLanguage("Taxes")+ " " + (Model.Instance.banckerData.WaterRateBasisPoints / 10) + "‰";//抽水费率直接用协议 WaterRateBasisPoints：200 = 2% = 20‰
        });

        //qiangZhuangBtn.onClick.RemoveAllListeners();
        //qiangZhuangBtn.onClick.AddListener(() =>
        //{
        //    if (FSMSystem.Instance.CurrentStateID != StateID.Bet)
        //    {
        //        Debug.Log("当前状态不可抢庄,抢庄按钮点击无效");
        //        return;
        //    }

        //    if (Time.time - qiangZhuangInterval < 0.8f)
        //    {
        //        Debug.Log("多次点击,点击过快,抢庄按钮点击无效");
        //        return;
        //    }
        //    qiangZhuangInterval = Time.time;

        //    if (qiangZhuangGreyImage.activeSelf)
        //        AudioManager.Instance.PlayVoiceEff("bankerBid");//抢庄声音
        //    else
        //        AudioManager.Instance.PlayVoiceEff("noBid");//不抢声音

        //    qiangZhuangGreyImage.SetActive(!qiangZhuangGreyImage.activeSelf);
        //    AnimationEffect.SetActive(!AnimationEffect.activeSelf);

        //    Debug.Log("点击抢庄按钮");
        //    if (isClickBanker)
        //    { PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_SET, true); isClickBanker = false; }
        //    else
        //    { PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_SET, false); isClickBanker = true; }
        //    //应该用 trigger 的
        //});
    }
    public void UpdateBankerText()
    {
        if (!View.OriginalBaccaratUiPresent) return;
        var bankerData = Model.Instance.banckerData;
        if (bankerData == null)
            return;
        //抢庄：只有庄闲两门计入抢庄玩法
        for (int i = 0; i < roleBetTextList.Count; i++)
        {
            roleBetTextList[i].text = i <= 1 ? Model.Instance.totalBets[i].ToString() : "";//只显示庄闲两门押分
        }
        // 押注合计直接使用协议 TotalBets（庄闲两门合计），替代本地累加
        int Cutodds = bankerData.Cut;//使用算法卡的水费数据
        if (Model.Instance.isJoker || Model.Instance.isShowCard || Model.Instance.isNoBanker)
        {
            backerCutText.text = ""; 
            roleBetTotalText.text = "";//总押分
            for(int i = 0; i < roleWinTextList.Count; i++)
            {
                //清空抢庄预估
                roleWinTextList[i].text = "";
            }
        }
        else
        {
            backerCutText.text = Cutodds.ToString();
            roleBetTotalText.text = bankerData.TotalBets.ToString();//总押分(庄闲合计)，协议直接值
            //预估直接使用算法卡下发值，替代本地计算
            for(int i = 0; i < roleWinTextList.Count; i++)
            {
                roleWinTextList[i].text = i == 0 ? bankerData.EstimateBankerProfit.ToString()
                    : i == 1 ? bankerData.EstimatePlayerProfit.ToString() : "";
            }
        }


        //座位：按 Priority 顺序显示机台号，PriorityLights 控制颜色（亮=满足条件）
        for (int i = 0; i < seatTextList.Count; i++)
        {
            int machineId = i < bankerData.Priority.Length ? bankerData.Priority[i] : 0;
            seatTextList[i].text = machineId == 0 ? "" : machineId.ToString();
            bool lightOn = i < bankerData.PriorityLights.Length && bankerData.PriorityLights[i] != 0;
            seatTextList[i].color = lightOn ? curSeatColor : Color.red;
        }

        //抽水费率直接用协议 WaterRateBasisPoints：200 = 2% = 20‰
        backerCutOddsText.text = Utils.GetLanguage("Taxes") + " " + (bankerData.WaterRateBasisPoints / 10) + "‰";//显示抽水赔率,比如:0.02 就显示 20‰
        //backerCutText.text = Model.Instance.banckerData.Cut.ToString();//显示抽水金额,比如:1000

        if (curBankerId == 0)
            curBankerId = Model.Instance.banckerData.BankerPlayerId;
        else
        {
            //说明上把有人抢庄,有庄家,这把继续让他庄家
            //连庄已由算法卡实现，本地很难分辨局与局的更替且算法卡需要实时更新
        }

        backerSeatText.text = Model.Instance.banckerData.BankerPlayerId == 0 ? "" : Model.Instance.banckerData.BankerPlayerId.ToString();//停止押注后刷新庄家信息,

        //判断自己是否已抢庄：ConfirmedMask 位图（IsConfirmed）表示已按抢庄键确认（协议机台号 1~6 与 macId 对齐）
        bool isBidBanker = bankerData.IsConfirmed(SBoxModel.Instance.macId);//判断该玩家是否已按抢庄键
        if (isBidBanker)
        {
            if (Model.Instance.bankerBidState == -1)
            {
                Model.Instance.bankerBidState = 1;
                bidSoundCoolDownTimer = Time.time;
            }
            else if (Model.Instance.bankerBidState == 0)
            {
                Model.Instance.bankerBidState = 1;
                if(Time.time-bidSoundCoolDownTimer > 0.8f)
                    AudioManager.Instance.PlayVoiceEff("bankerBid");
            }
        }

        if(!isBidBanker)
        {
            if (Model.Instance.bankerBidState == -1)
            {
                Model.Instance.bankerBidState = 0;
                bidSoundCoolDownTimer = Time.time;           
            }
            else if (Model.Instance.bankerBidState == 1)
            {
                Model.Instance.bankerBidState = 0;
                if(Time.time-bidSoundCoolDownTimer > 0.8f)
                    AudioManager.Instance.PlayVoiceEff("noBid");
            }
            //Debug.LogError("Model.Instance.bankerBidState = 0;");
            //StartCoroutine(BankerBid());//不是庄家就抢庄 ※测试用 打包恢复 dabao
        }
        //不管抢庄状态怎么变化,都要根据状态刷新抢庄按钮状态，只不过每局第一次进入都不播抢庄声音
        qiangZhuangGreyImage.SetActive(Model.Instance.bankerBidState == 0);
        AnimationEffect.SetActive(Model.Instance.bankerBidState == 1);
    }
    public const float pressDelay = 0.5f;
    public IEnumerator BankerBid()
    {
        if(!Model.Instance.canBidBanker)yield break; 
        PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_SET, true);
        yield return new WaitForSeconds(pressDelay);
        PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_SET, false);
    }
    public void RefreshBancker()
    {
        if (!View.OriginalBaccaratUiPresent) return;
        var bankerData = Model.Instance.banckerData;
        if (bankerData == null)
        {
            return;
        }
        if (bankerData.BankerPlayerId == 0) 
        {
            Model.Instance.isNoBanker = true;
            return;
        }


        //播放赢分动画后,刷新显示庄家信息
        int result = Model.Instance.curRecord.result;
        if (Model.Instance.isJoker || Model.Instance.isShowCard)
        {
            roleWinTotalText.text = "";
            backerCutText.text = "";
            backerWinText.text = "";
            backerProfitText.text = "";
            roleBetTotalText.text = "";
            return;
        }
        roleWinTotalText.text = Model.Instance.banckerData.TotalWins.ToString();//所有类型的总得分合计,没有计算小游戏倍率的情况
        backerCutText.text = Model.Instance.banckerData.Cut.ToString();//税费直接获取,肯定会有值

        backerWinText.text = Model.Instance.banckerData.BankerWins.ToString();//抢庄盈利
        backerProfitText.text = Model.Instance.banckerData.BankerProfit.ToString();//抢庄实利
        roleBetTotalText.text = Model.Instance.banckerData.TotalBets.ToString();//押注合计
    }

    public void ResetBanker()
    {
        if (!View.OriginalBaccaratUiPresent) return;
        //Debug.LogError("ResetBanker");
        for (int i = 0; i < roleWinTextList.Count; i++)//每种卡片类型的总赢分
        {
            roleWinTextList[i].text = "";
        }
        backerCutText.text = "";
        backerSeatText.text = "";
        backerWinText.text = "";
        backerProfitText.text = "";
        roleBetTotalText.text = "";
        roleWinTotalText.text = "";

        //Controller.Instance.backerBidCount = 0;
    }

    public void ResetBanker2()
    {
        if (!View.OriginalBaccaratUiPresent) return;
        qiangZhuangGreyImage.SetActive(true);//恢复抢庄按钮灰态,可以继续抢庄了
        AnimationEffect.SetActive(false);
    }
    public void setBidBankerBtngrey(bool isGrey)//将抢庄按钮的子物体置灰
    {
        if (!View.OriginalBaccaratUiPresent) return;
        var child = qiangZhuangGreyImage.GetComponentsInChildren<Image>(true);
        if(isGrey)
            foreach (var item in child)
            {
                item.color = Color.grey;
            }
        else
        {
            foreach (var item in child)
            {
                item.color = Color.white;
            }
        }
    }
    public void SetTextSize()
    {
        if (!View.OriginalBaccaratUiPresent) return;
        int textSize = IOCanvasModel.Instance.curlanguage == Language.en?13:17;
        foreach (var item in languageTextList)
        {
            item.fontSize = textSize;
        }
    }
}
