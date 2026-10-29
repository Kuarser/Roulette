using System.Net.Sockets;
using System.Collections.Generic;
using SBoxApi;
//服务器信息
public class ServerInfo
{
    public string IP { get; set; }
    public int port { get; set; }
}
public class ClientInfo
{
    public string IP { get; set; }
    public int port { get; set; }
    public string USN { get; set; }
    public int macId { get; set; }
}

//服务器收到的数据结构(TOOD 此处可能需要优化)
public class SrvMsgData
{
    public Socket mSocket { get; set; }
    public string mData { get; set; }
}

//服务器收到的websocket数据结构
public class WSSrvMsgData
{
    public WebSockets.ClientConnection Client { get; set; }
    public string Data { get; set; }
}

//消息体
public class MsgInfo
{
    public int cmd { get; set; }        //协议
    public int id { get; set; }         //这里一般都是机台ID
    public string jsonData; //
}

//S2C_Config
public class MsgConfig
{
    public int gameState;
    public int countDown;
    public int curLanguage;
    public int round;
    public int game;
    public int coinValue;
    public int ticketValue;
    public int scoreTicket;
    public int soundVolumnScale;
    public int bgmSwitch;//背景音乐开关
    public int bgmSelectIndex;//背景音乐选择
    public List<int> switchList;
    public bool clearCoinIn;
    //增加
    public int minBet;//最小押分
    public int limitBetsWins;//限红
    public int remainCards;                                     //剩余卡牌数
    public RecordStruct recordStruct;                             //当前记录结构体
    public int netJackpotState;     //网络彩金状态 0:关闭联网彩金 1:未连接联网彩金 2:连接联网彩金
    public List<int> betsMinOfNetJackpots;
    public int pulseValue;
}

//S2C_Start
public class GameInfo
{
    public int musicRan;
    public int curRound;
    public int curGame;
    public RecordStruct curRecord;
}
public class AllRoadStruct
{
    /// <summary>
    /// 珠盘路
    /// </summary>
    public List<int>beadPlateRoadList;
    /// <summary>
    /// 大路
    /// </summary>
    public List<List<int>>bigRoadList;
    /// <summary>
    /// 大眼路
    /// </summary>
    public List<List<int>> bigEyeRoadList;
    /// <summary>
    /// 小路值
    /// </summary>
    public List<List<int>> smallRoadList;
    /// <summary>
    /// 蟑螂路值
    /// </summary>
    public List<List<int>> dragonRoadList;
}
//快照，抢庄部分
public class BankerDataStruct
{
    public int BankerSeat;                                  //庄家座位号（机台号 1~6）
    public int[] Priority = new int[6];                     // 抢庄优先顺序，机台号 1~6
    public int[] PriorityLights = new int[6];               // 按 Priority 顺序的灯状态
    public int CanGrabMask;                                 // bit0~bit5 对应机台 1~6
    public int EstimateBankerProfit;                        // 按庄赢预估的抢庄实利
    public int EstimatePlayerProfit;                        // 按闲赢预估的抢庄实利
    public int WaterRateBasisPoints;                        // 水费率 basis points，200 = 2%
    public int TotalBets;                                   // 庄闲两门总押分合计
    public int TotalWins;                                   // 庄闲两门总得分
    public int BankerWins;                                  // 抢庄毛盈利
    public int BankerProfit;                                // 抢庄实利
    public int Cut;                                         // 本局水费
    public double CutOdds;                                  // 水费率（WaterRateBasisPoints / 10000.0）
    public int[] roleBetTotalTextList = new int[9];              //每种花色的总押分列表
    public int backerWinText;                               //抢庄盈利
    public int backerProfitText;                            //抢庄实力
    public int backerCutText;                               //抢庄抽水
    public double backerCutOddsText;                           //抢庄抽水赔率
    public int roleBetTotalText;                            //押注合计
    public int roleWinTotalText;                            //赢分合计
    // 旧字段保留仅为兼容已序列化的历史数据，不再写入新快照
    public SBoxBattleBankerPlayerInfo[] BankerRequstTable = new SBoxBattleBankerPlayerInfo[10];
}
//当局单玩家总快照 - S2C_UpdateHistoryData
public class TotalDataStruct
{
    public BankerDataStruct bankerDataStruct;               //抢庄部分快照
    public RecordStruct recordStruct;                       //当局记录结构体
    public playerData playerData;                           //玩家数据
    public int jackpotValue;                                //当局彩金
}
public class playerData
{
    public int[] selfBetTextList = new int[9];              //玩家押分列表
    public int selfWinScore;                                //玩家赢分
    public int selfCredit;                                  //玩家credit
}

public class RecordStruct
{
    /// <summary>
    /// 第几局
    /// </summary>
    public int game;
    /// <summary>
    /// 第几轮
    /// </summary>
    public int round;
    /// <summary>
    /// 牌值六位，前三庄家后三闲家
    /// </summary>
    public int[] value;
    /// <summary>
    /// 结果庄1，闲2，和3
    /// </summary>
    public int result;
    /// <summary>
    /// 对子结果有1无0 [0]庄对 [1]闲对
    /// </summary>
    public int[] pair;
    /// <summary>
    /// 特殊结果有1无0 [0]小老虎 [1]大老虎 [2]幸运7 [3]幸运8
    /// </summary>
    public int[] spresult;
    /// <summary>
    /// 发牌数 [0]庄牌数 [1]闲牌数
    /// </summary>
    public int[] cardnum;
    /// <summary>
    /// 庄家玩家id
    /// </summary>
    public int bankerID;
    /// <summary>
    /// 庄家玩家盈利
    /// </summary>
    public int bankerProfit;
    /// <summary>
    /// 幸运座位玩家id
    /// </summary>
    public int luckyPlayerId;
    /// <summary>
    /// 小游戏id 0:未中, 4:加倍, 5:翻倍, 6:明牌, 7:全中, 8:彩金
    /// </summary>
    public int luckyId;
    /// <summary>
    /// 额外信息: 加?倍, 翻?
    /// </summary>
    public int[] extraData;
}

//S2C_Action
public class ActionInfo
{
    public float sliderValue;
    public int openBoxId;
    public int openBoxScore;
}

//C2S_PlayerBet
public class PlayerBet
{
    public int[] bets = new int[15];
}

//C2S_CreditAction
public class CreditAction
{
    public int mode;        //mode 取值 0 : 投币,1 : 退币,2 : 上分,3 : 下分 
    public int value;
    public int isCredit;    //0:总分 1:赢分
}

//C2S_CoinGameCreditAction
public class CoinGameCreditAction
{
    public int value;
}
public class NetJPLoginInfo
{
    public int gameType;
    public int macId;
}

public class ReceiveJackpotInfo
{
    public int gameType;
    public long orderId;
}

//C2S_JackBet
public class JackBetInfo
{
    public int gameType;                        // 游戏类型 
    public int seat;                           // 分机号/座位号                   
    public int bet;                            // 当前的押分,为了避免丢失小数，需要乘以100，硬件读取这个值会除以100后使用
    public int betPercent;                     // 押分比例，目前拉霸默认值传1，同样需要乘以100          
    public int scoreRate;                      // 分值比，1分多少钱，需要乘以1000再往下传
    public int JPPercent;                      // 分机彩金百分比，每次押分贡献给彩金的比例。需要乘以1000再往下传
}

//S2C_WinJackpot
public class WinJackpotInfo
{
    public int macId;                           //主机号
    public int seat;                            //分机号-座位号
    public int win;
    public int jackpotId;
    public long orderId;
    public long time;
}

//C2S_SBoxBillInOrder 载荷
public class BillInOrderMsg
{
    public int credit;
    public string orderId;
}

//退分小票打印数据载荷
public class RefundPrintMsg
{
    public string orderId;
    public string address;
    public string time;
    public int coinCount;
}

//退分小票打印完成回执载荷
public class RefundPrintAckMsg
{
    public string orderId;
}

//S2C_Error
public class ErrorInfo
{
    public int errCode;
    public string errString;
}

//S2C_PlayLucky
public class PlayLucky
{
    public int lucky;
    public int maxplayer;
    public int jack_type;
}

//S2C_Result
public class GameResult
{
    public int winScore;
}

//S2C_ShowWinIndex
public class WinIndex
{
    public int winIndex;
}

//S2C_ShowScoreTimes
public class RoleTimes
{
    public int index;
    public List<ScoreTimes> scoreTimes = new List<ScoreTimes>();
}

public class ScoreTimes
{
    public int index;
    public int scoreTimes;
}

//S2C_ShowJackpotAni
public class Jackpot
{
    public int jackpot;
}

//S2C_Jack
public class RandJack
{
    public int bettime; //倒计时
    public int randjack; //彩金
}

//S2C_UpdateRoad
public class Record
{

}

//S2C_ShowIconEffect
public class ShowIconEffect
{
    public int index;
    public int times;
}

//S2C_State
public class CurrentStateID
{
    public int stateID;
}

//C2S_InState
public class InState
{
    public int inState;
    public bool isChange;
    public int betUnitIdx;
}

//S2C_ClientOverScore
public class OverScore
{
    //0:正常 1:超分
    public int state;
}

//C2S_GetGameConfig
public class SBoxCoinNum
{
    public int coinInNum;
    public int coinOutNum;
}
//S2C_SetbetCoinList
public class BetCoinList
{
    public List<int> betCoinList;
}
//S2C_SetNetJackpotIcon
public class NetJackpotIconState
{
    public int JPState;
    public int IconState;
}

#region 发送给网页的数据结构
/// <summary>
/// 通用结构
/// </summary>

public class S2P_SBoxRecord
{
    public List<S2P_universal> waveUp = new List<S2P_universal>();
    public List<S2P_universal> waveDown = new List<S2P_universal>();
    public List<S2P_universal> role = new List<S2P_universal>();
    public List<S2P_universal> dragon = new List<S2P_universal>();
    public List<S2P_universal> totalWave = new List<S2P_universal>();
    public List<S2P_universal> littleGame = new List<S2P_universal>();
    public List<S2P_universal> superTimes = new List<S2P_universal>();
    public List<S2P_universal> JP = new List<S2P_universal>();
    public List<S2P_universal> freeRole = new List<S2P_universal>();
    public S2P_universal allPick = new S2P_universal();
    public S2P_extra jackpot = new S2P_extra();
    public S2P_extra proposeScore = new S2P_extra();
    public List<S2P_universal> box = new List<S2P_universal>();
    public List<S2P_universal> reward = new List<S2P_universal>();
}

public class S2P_universal
{
    public int index;
    public float count;
    public float bet;
    public float win;
    public float profitRate;
}

public class S2P_extra
{
    public List<S2P_universal> list = new List<S2P_universal>();
    public float extraData;
}

public class WaveSeries
{
    List<int> pullUpArray = new List<int>();
    List<int> pullDownArray = new List<int>();
    List<int> pullUpShuffle = new List<int>();
    List<int> pullDownShuffle = new List<int>();
    List<int> arraySeries = new List<int>();
    List<int> curArray = new List<int>();
    List<int> leve = new List<int>();
}
#endregion

#region 发送给网页的数据结构
/// <summary>
/// 通用结构
/// </summary>
public class S2P_Record
{
    //游戏id
    public int gameId;
    //局数
    public int roundId;
    //设置倍率
    public float difficult;
    //主机机台号
    public int machineId;
    //总开分
    public int recharge;
    //总洗分
    public int takeOut;
    //总投币
    public int coinIn;
    //总退币
    public int coinOut;
    //总押分
    public int bet;
    //总得分
    public int win;
    //总盈利
    public int profit;
    //当前机率: 保留百分比后三位 例如: 91.215% 传入91.215
    public float profitRate;
    //开奖结果
    public int winIndex;
    //开奖类型 0:正常开奖 1:炮灯开奖
    public int winMode;
    //小游戏结果
    public int extraWinIndex;
    //数据对照列表
    public List<string> dataComparisonList = new List<string>() { "紫色", "蓝色", "绿色", "红色", "黄色" };
    //每个玩家的投注情况
    public Dictionary<int, S2P_Player> playerBetDic = new Dictionary<int, S2P_Player>();
    //额外数据
    public S2P_ExtraRecord extraRecord;
    //小游戏积分库
    public S2P_ExtraScore extraScore;
}

public class S2P_ExtraScore
{
    //当前局数 (1 ~ 20)局
    public int curRound;
    //800局平均值
    public float partValue;
    //20局押分
    public int[] bets = new int[20];
    //20局应中分数
    public int[] deserveScore = new int[20];
    //累计积分库
    public int accruedScored;
    //一段返还积分库1
    public int repayScore1;
    //二段返还积分库2
    public int repayScore2;
    //小游戏自备款 保留百分比后1位 例如: 1.5% 传入1.5
    public float payPercent;
    //小游戏出分详情
    public List<S2P_ExtraScoreDetail> extraScoreDetailList = new List<S2P_ExtraScoreDetail>();
}

public class S2P_ExtraScoreDetail
{
    //八百局计数器 从等级2开始
    public int count;
    //积分库等级
    public int level;
    //累计提取值
    public int extractScore;
    //当前库存
    public int curExtractScore;
    //已出分数
    public int scoreHavePay;
}

public class S2P_ExtraRecord
{

}

/// <summary>
/// 喜从天降结构
/// </summary>
public class S2P_BallRecord : S2P_ExtraRecord
{
    //幸运座位开奖结果
    //0: 加倍, 1:翻倍, 2:押分全中
    public int extraWinDetail;
    //滚球洞号
    public int hollIndex;
    //调用例子:
    //S2P_Record record = new S2P_Record();
    //给record赋值 喜从天降:gameId = 1001
    //S2P_BallRecord extraRecord = new S2P_BallRecord();
    //record.extraRecord = extraRecord;

    //data = JsonConvert.SerializeObject(record)
    //Dictionary<string, string> paramDic = new Dictionary<string, string>();
    //paramDic["record"] = data;
    //Utils.Post("http://8.134.167.255:8082/api/bet_log", paramDic)
}

public class S2P_Player : S2P_ExtraRecord
{
    //分机号
    public int macId;
    //押注列表
    public List<int> betList = new List<int>();
    //小游戏得分
    public int extraWin;
    //总押注
    public int bet;
    //总赢分(由于统计结果拆分!不能包含黄色球返回的分)
    public int win;
    //总积分
    public int score;
}

public class S2P_TotalRecord
{
    //游戏id
    public int gameId;
    //局数
    public int roundCount;
    //设置倍率
    public float difficult;
    //主机机台号
    public int machineId;
    //总开分
    public int recharge;
    //总洗分
    public int takeOut;
    //总投币
    public int coinIn;
    //总退币
    public int coinOut;
    //总押分
    public int bet;
    //总得分
    public int win;
    //总盈利
    public int profit;
    //当前机率: 保留百分比后三位 例如: 91.215% 传入91.215
    public float profitRate;
    //最近一次上传时间(当前时间时间戳)
    public long lastTime;

    public S2P_TotalExtraRecord totalExtraRecord = new S2P_TotalExtraRecord();
}

public class S2P_TotalExtraRecord
{

}

public class S2P_BallTotalRecord : S2P_TotalExtraRecord
{
    public S2P_HollPage hollPage = new S2P_HollPage();
    public S2P_RewardPage rewardPage = new S2P_RewardPage();
    public S2P_ExtraGamePage extraGamePage = new S2P_ExtraGamePage();
    public S2P_ExtraScore extraScore = new S2P_ExtraScore();

    //调用例子:
    //S2P_TotalRecord record = new S2P_TotalRecord();
    //给record赋值 喜从天降:gameId = 1001
    //S2P_BallTotalRecord extraRecord = new S2P_BallTotalRecord();
    //record.totalExtraRecord = extraRecord;

    //data = JsonConvert.SerializeObject(record)
    //Dictionary<string, string> paramDic = new Dictionary<string, string>();
    //paramDic["total"] = data;
    //Utils.Post("http://8.134.167.255:8082/api/bet_total", paramDic)
}

public class S2P_HollPage
{
    //滚球进洞结构
    public struct HollData
    {
        //滚球洞号
        public int index;
        //次数
        public int times;
        //几率
        public float profitRate;
    }

    //滚球进洞数据
    public HollData[] hollData = new HollData[18];
}

public class S2P_RewardPage
{
    //开奖统计结构
    public struct ResultData
    {
        //颜色 0:紫色 1:蓝色 2:绿色 3:红色 4:黄色
        public int index;
        //次数
        public int times;
        //押分
        public int bet;
        //得分
        public int win;
        //返分
        public int reward;
        //得分+返分
        public int totalWin;
        //几率
        public float profitRate;
    }

    //滚球押分
    public int rollBet;
    //滚球得分
    public int rollWin;
    //滚球几率
    public float rollProfitRate;
    //炮灯押分
    public int lightBet;
    //炮灯得分
    public int lightWin;
    //炮灯几率
    public float lightProfitRate;
    //杀分押分
    public int profigBet;
    //杀分得分
    public int profitWin;
    //杀分几率
    public float profitProfitRate;
    //总押分
    public int totalBet;
    //总得分
    public int totalWin;
    //总几率
    public float totalProfitRate;
    //滚球统计
    public ResultData[] rollData = new ResultData[5];

    public ResultData rollTotalData = new ResultData();
    //炮灯统计
    public ResultData[] lightData = new ResultData[5];

    public ResultData lightTotalData = new ResultData();
    //杀分统计
    public ResultData[] profitData = new ResultData[5];

    public ResultData profitTotalData = new ResultData();
}

public class S2P_ExtraGamePage
{
    public struct ExtraGameData
    {
        public int extraType;
        public int times;
        public int win;
        public float profitRate;
        public int gaveScore;
    }

    public ExtraGameData[] extraGameData = new ExtraGameData[5];
    public int extraGameTotalTimes;
    public int extraGameTotalWin;
    public int extraGameTotalGaveScore;
    public float extraGameTotalProfitRate;
}

#endregion


public enum NET_STATUS
{
    NET_STATUS_DISCONNECTED = 0,
    NET_STATUS_CONNECTED,
}
