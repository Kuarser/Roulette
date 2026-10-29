using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

//循环滚动视图 里面的一行(个)中奖信息 显示
public class LoopRecordItemView : MonoBehaviour
{
    [Tooltip("用图片显示中奖结果")]
    public bool isResultShowImage;//是否是用图片显示中奖结果//默认用文字
    public List<string> resultStringList;
    public List<Sprite> resultSpriteList;

    private Text round;//第几轮
    private Text game;//第几局
    private Text resultText;//中奖结果 是什么鱼种类(文本)
    private Image resultIcon;//中奖结果 是什么鱼种类(图片)
    private Text jackpot;//彩金
    // private Text showCard;//明牌
    private Text luckySeat;//幸运座位号
    private Text Banker;//庄家号
    private Text allBet;//总压分
    private Text win;//赢分
    private Text credit;//金币数
    private  GameObject luckySeatTitle;
    private  GameObject showCard;
    private GameObject jackpotTitle;
    private bool isInit = false;//是否已经初始化过

    // TotalDataStruct 对应的 Text 显示字段
    // RecordStruct 属性
    // private Text luckyPlayerIdText;//幸运座位玩家id显示
    // BankerDataStruct 属性-使用bankerViewHistory显示
    private UIBankerViewHistory bankerViewHistory;
    // playerData 属性
    private List<Text> selfBetTextListDisplay = new List<Text>();//玩家押分列表显示
    private Text selfWinScoreText;//玩家赢分显示
    
    // TotalDataStruct 自身属性
    private Text jackpotValueText;//当局彩金显示
    private const int POKER_COUNT = 6;//六张牌:前三庄家后三闲家
    private List<Image> pokerSpriteList = new List<Image>();//六张牌显示列表,对应预制件 Poker3_1~Poker3_6
    private List<SymbolViewHistory> symbolViewsList = new List<SymbolViewHistory>();//中奖

    private void InitView()
    {
        if(isInit)return;
        round = transform.Find("GameInfo/round/roundText").GetComponent<Text>();
        game = transform.Find("GameInfo/game/gameText").GetComponent<Text>();
        //收集六张牌位:预制件根节点下 Poker3_1~Poker3_6(替代原单个Poker3)
        for (int i = 1; i <= POKER_COUNT; i++)
        {
            var pokerImg = transform.Find($"Poker3_{i}")?.GetComponent<Image>();
            pokerSpriteList.Add(pokerImg);
        }
        var symbolList = transform.Find("SymbolList");
        for (int i = 0; i < symbolList.childCount; i++)
        {
            var symbol = symbolList.GetChild(i).GetComponent<SymbolViewHistory>();
            symbol.index = i;
            symbolViewsList.Add(symbol);//中奖视图-历史专用
        }
        // resultText = transform.Find("result/Text").GetComponent<Text>();
        // resultIcon = transform.Find("result/Image").GetComponent<Image>();
        jackpot = transform.Find("jackpot").GetComponent<Text>();
        jackpotTitle = transform.Find("jackpotTitle").gameObject;
        // showCard = transform.Find("showCard").GetComponent<Text>();
        luckySeat = transform.Find("luckySeat").GetComponent<Text>();
        luckySeatTitle = transform.Find("luckySeatTitle").gameObject;
        showCard = transform.Find("showCard").gameObject;
        // Banker = transform.Find("banker").GetComponent<Text>();
        // allBet = transform.Find("allBet").GetComponent<Text>();
        win = transform.Find("playerInfo/winScore").GetComponent<Text>();
        credit = transform.Find("playerInfo/credit").GetComponent<Text>();
        
        // 初始化 TotalDataStruct 对应的 Text 组件
        // RecordStruct
        // luckyPlayerIdText = transform.Find("luckyPlayerId")?.GetComponent<Text>();
        
        // BankerDataStruct - 使用新的bankerViewHistory显示
        
        bankerViewHistory = transform.Find("BankerView").GetComponent<UIBankerViewHistory>();
        // playerData
        for (int i = 0; i < Model.TYPE_COUNT; i++)
            selfBetTextListDisplay.Add(transform.Find($"selfBet{i}")?.GetComponent<Text>());
        selfWinScoreText = transform.Find("selfWinScore")?.GetComponent<Text>();
        
        // TotalDataStruct
        jackpotValueText = transform.Find("Jackpot/Text")?.GetComponent<Text>();
        
        isInit = true;

        // if (isResultShowImage)
        // {
        //     resultText.gameObject.SetActive(false);//用图片显示结果,隐藏文字
        // }
        // else
        // {
        //     resultIcon.gameObject.SetActive(false);//用文字显示结果,隐藏图片
        // }
    }

    private void ShowResult(int winIndex)
    {
        if (isResultShowImage)
        {
            if (resultSpriteList == null || resultSpriteList.Count <= 0)
            {
                Debug.LogError("历史记录单行 预制体, 缺少 精灵 挂载");
            }
            resultIcon.sprite = resultSpriteList[winIndex];
            resultIcon.SetNativeSize();
        }
        else
        {
            if (resultStringList == null || resultStringList.Count <= 0)
            {
                Debug.LogError("历史记录单行 预制体, 缺少 文字 挂载");
            }
            resultText.text = resultStringList[winIndex];
            //switch (winIndex)
            //{
            //    case 1:
            //        resultText.text = "黑桃";
            //        break;
            //    case 2:
            //        resultText.text = "红心";
            //        break;
            //    case 3:
            //        resultText.text = "梅花";
            //        break;
            //    case 4:
            //        resultText.text = "方块";
            //        break;
            //    case 5:
            //        resultText.text = "大小王";
            //        break;
            //}
        }
    }

    // public void InitViewData(RecordData loopRecordData)
    // {
    //     if (!isInit) InitView();
    //     round.text = loopRecordData.round.ToString();
    //     game.text = loopRecordData.game.ToString();
    //     for (int i = 0; i < 5; i++)
    //         betList[i].text = loopRecordData.betData[i].ToString();

    //     ShowResult(loopRecordData.winIndex - 1);
    //     //switch (loopRecordData.winIndex)
    //     //{
    //     //    case 1:
    //     //        resultText.text = "黑桃";
    //     //        break;
    //     //    case 2:
    //     //        resultText.text = "红心";
    //     //        break;
    //     //    case 3:
    //     //        resultText.text = "梅花";
    //     //        break;
    //     //    case 4:
    //     //        resultText.text = "方块";
    //     //        break;
    //     //    case 5:
    //     //        resultText.text = "大小王";
    //     //        break;
    //     //}

    //     luckySeat.text = "0";//对不上,增加
    //     showCard.text = "0";
    //     jackpot.text = "0";
    //     switch (loopRecordData.luckyId)
    //     {
    //         case 4:
    //         case 5:
    //         case 7:
    //             luckySeat.text = loopRecordData.luckyWin.ToString();
    //             break;
    //         case 6:
    //             showCard.text = loopRecordData.luckyWin.ToString();
    //             break;
    //         case 8:
    //             jackpot.text = loopRecordData.luckyWin.ToString();
    //             break;
    //     }
    //     Banker.text = loopRecordData.banker.ToString();

    //     allBet.text = loopRecordData.allBet.ToString();
    //     win.text = loopRecordData.allWin.ToString();
    //     //credit.text = loopRecordData.credit.ToString();//对不上,修改
    //     credit.text = (loopRecordData.credit + loopRecordData.allWin).ToString();
    //     gameObject.SetActive(true);
    // }
    public void InitViewData2(TotalDataStruct totalDataStruct)
    {
        // 从 TotalDataStruct 创建对应的映射属性
        // RecordStruct 属性
        if(!isInit)InitView();
        this.jackpotTitle.SetActive(false);
        this.luckySeatTitle.SetActive(false);
        this.showCard.SetActive(false);
        Debug.LogError(JsonConvert.SerializeObject(totalDataStruct));
        gameObject.SetActive(true);
        int round = totalDataStruct.recordStruct.round;
        int game = totalDataStruct.recordStruct.game;
        int[] cardValues = totalDataStruct.recordStruct.value;//牌值六位:前三庄家后三闲家
        int luckyPlayerId = totalDataStruct.recordStruct.luckyPlayerId;
        int luckyId = totalDataStruct.recordStruct.luckyId;
        int[] extraData = totalDataStruct.recordStruct.extraData;
        
        // BankerDataStruct 属性
        int[] roleBetTotalTextList = totalDataStruct.bankerDataStruct.roleBetTotalTextList;
        int roleBetTotalText = totalDataStruct.bankerDataStruct.roleBetTotalText;
        
        // playerData 属性
        int[] selfBetTextList = totalDataStruct.playerData.selfBetTextList;
        int selfWinScore = totalDataStruct.playerData.selfWinScore;
        int credit = totalDataStruct.playerData.selfCredit;
        
        // TotalDataStruct 自身属性
        int jackpotValue = totalDataStruct.jackpotValue;
        
        int luckyWinEx = extraData != null && extraData.Length > 0 ? extraData[0] : 0;
        
        if (!isInit) InitView();
        
        // 原有的显示逻辑 - 使用与 RecordData 对应的字段
        this.round.text = round.ToString();
        this.game.text = game.ToString();
        luckySeat.text = "";//对不上,增加
        this.jackpot.text = "";
        bool isShowCard = false;
        if(luckyPlayerId == SBoxModel.Instance.macId || luckyId == 8 || luckyId == 6)
        switch (luckyId)
        {
            case 4:
                luckySeat.text = $"+{luckyWinEx}";
                this.luckySeatTitle.SetActive(true);
                break; 
            case 5:
                luckySeat.text = $"X2";
                this.luckySeatTitle.SetActive(true);
                break;
            case 7:
                luckySeat.text = Utils.GetLanguage("Allwins");
                this.luckySeatTitle.SetActive(true);
                break;
            case 6:
                showCard.SetActive(true);
                isShowCard = true;
                break;
            case 8:
                //extraData长度可能小于座位数,越界时按0显示
                int macIndex = SBoxModel.Instance.macId - 1;
                this.jackpot.text = (extraData != null && macIndex >= 0 && macIndex < extraData.Length ? extraData[macIndex] : 0).ToString();
                this.jackpotTitle.SetActive(true);
                break;
        }
        this.win.text = selfWinScore.ToString();
        this.credit.text = (credit + selfWinScore).ToString();
        
        // 显示 TotalDataStruct 的所有额外属性
        // RecordStruct
        // if (luckyPlayerIdText != null)
        //     luckyPlayerIdText.text = luckyPlayerId.ToString();
        
        // BankerDataStruct
        bankerViewHistory.RefreshBancker(totalDataStruct.bankerDataStruct, isShowCard,
            totalDataStruct.recordStruct != null && totalDataStruct.recordStruct.result == 3);
        bool[] wins = Model.CalcWinDoors(totalDataStruct.recordStruct);
        for (int i = 0; i < Model.TYPE_COUNT; i++)
        {
            if (i < symbolViewsList.Count && symbolViewsList[i] != null)
            {
                symbolViewsList[i].SetBet(selfBetTextList[i]);
                symbolViewsList[i].SetAllBet(roleBetTotalTextList[i]);
                if(luckyId == 7 && luckyPlayerId == SBoxModel.Instance.macId)
                    symbolViewsList[i].ShowLightIcon();//大满贯全亮
                else if (wins[i])
                    symbolViewsList[i].ShowLightIcon();//九门中奖亮灯(可多门同亮)
                else
                    symbolViewsList[i].HideLightIcon();
            }
        }
        
        // playerData
        if (selfWinScoreText != null)
            selfWinScoreText.text = selfWinScore.ToString();
        
        // TotalDataStruct
        if (jackpotValueText != null)
            jackpotValueText.text = jackpotValue.ToString();

        //六张牌:按 value.Length 显示,缺位隐藏
        for (int i = 0; i < POKER_COUNT; i++)
        {
            if (pokerSpriteList[i] == null) continue;
            if (cardValues != null && i < cardValues.Length && cardValues[i] != 0)//牌值为0视为未发牌,缺位隐藏
            {
                PokerData pd = Model.Instance.GetPokerData(cardValues[i]);
                pokerSpriteList[i].sprite = ResMgr.Instance.Load<Sprite>($"poker/{pd.type}_{pd.value}");
                pokerSpriteList[i].gameObject.SetActive(true);
            }
            else
            {
                pokerSpriteList[i].gameObject.SetActive(false);
            }
        }

    }
}

//数据库声明了,这里可以不用声明了
//public class RecordData
//{
//    public int id;
//    public int round;
//    public int game;
//    public int[] betData;//鱼种类 押注情况
//    public int winIndex;//中奖鱼下标
//    /// <summary>
//    /// 小游戏id 0:未中, 4:加倍, 5:翻倍, 6:明牌, 7:全中, 8:彩金
//    /// </summary>
//    public int luckyId;
//    public int luckyWin;
//    public int allBet;
//    public int allWin;
//    public int credit;
//}