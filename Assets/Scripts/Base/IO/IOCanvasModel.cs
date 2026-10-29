
using Newtonsoft.Json;
using SBoxApi;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum Language
{
    cht,
    en
}

public enum Month
{
    Jan,
    Feb,
    Mar,
    Apr,
    May,
    Jun,
    Jul,
    Aug,
    Sep,
    Oct,
    Nov,
    Dec,
}

public enum IOState
{
    CheckPermissions,
    SelectFunction,
    TestScreen,
    TestTouch,
    Params,
    Bill,
    Clients,
    Code,
    EditPassword,
    DateTime,
    JackpotSetting,
    JackpotBet,
    JackpotWins,
    ProSetting,
    BetSwitch,//zxd 增加
    PrintSetting,//打印设置
    BgmSetting,//背景音乐设置
    PrinterSetting,//打印机设置
    BillSetting,//纸钞机设置
    AddressSetting,//地址设置
}

public enum IOSectionState
{
    GameGear,//游戏档位
    NewGameMode,
    SkillMode,
    CountDown,
    MinBet,
    MaxBet,
    LimitBetsWins,
    CoinRatio,
    TicketRatio,
    ScoreUpRatio,
    Language,
    RefundMode,
    LineId,
    MacId,
    EditPassword,
    NewPassword,
    PrinterMode,
    RecordMode,
    PrintTime,
    PrintDensity,
    ClientWinLock,
    MachineWinLock,
    RoundWinLock,
    NetSwitch,
    OffsetRatio,
    //SoundVolumScale,
    BetSwitchCount,//zxd增加
    CutOdds,//kuarser增加
    DateTime,
    EditLineId,
    EditMacId,
    JackpotSwitch,
    JackpotPercent,
    BetSwitchConnect,//zxd 增加
    BetSwitch1,//zxd 增加
    BetSwitch2,//zxd 增加
    BetSwitch3,//zxd 增加
    BetSwitch4,//zxd 增加
    BetSwitch5,//zxd 增加
    PrintOffset,
    BgmSwitch,//背景音乐开关
    BgmSelect,//背景音乐选择
}

/// <summary>
/// 最外层菜单
/// </summary>
public enum IOFunction
{
    Params,
    Bill,
    Code,
    Language,
    EditPassword,
    ModifiedDate,
    JackpotSetting,
    JackpotBet,
    JackpotWins,
    ProSetting,
    PrintSetting,
    BgmSetting,//背景音乐设置
    BillSetting,//纸钞机设置
    PrinterSetting,//打印机设置
    Exit,
}

/// <summary>
/// 参数设置(与排序相关)
/// </summary>
public enum IOParams
{
    GameGear,//游戏档位
    NewGameMode,
    CountDown,
    MinBet,
    MaxBet,
    LimitBetsWins,
    CoinRatio,
    TicketRatio,
    RefundMode,
    PrintDensity,
#if ScoreUp
    ScoreUpRatio,
#endif
    PrinterMode,
    //RecordMode,
    PrintTime,
    //SkillMode,//切换倍率
    ClientWinLock,
    MachineWinLock,
    RoundWinLock,
    //NetSwitch,
    OffsetRatio,
    //SoundVolumScale,
    BetSwitchCount,//zxd增加
    BetSwitch,//zxd增加
    CutOdds,//kuarser增加
    PrintSetting,//打印设置
    Save,
    Return,
    OpenBox,
    ModifiedDate,
    ProSetting,
}

public enum IOBetSwitch
{
    BetSwitchConnect,
    Save,
    Return,
}


public enum IOBill
{
    Clients,
    Return
}

public enum IOClients
{
    Server,
    PrePage,
    NextPage,
    Return
}

public enum IOPlaceMode
{
    Normal,
    Skill,
    Pro,
}

public enum IOGameGear
{
    Gear1,
    Gear2,
    Gear3,
    Gear4,
    Gear5,
    Gear6,
    Random
}

public enum IOProSetting
{
    LineId,
    MacId,
    WinLock,
    EditPassword,
}

public enum IOMagnificationMode
{
    Low,
    Hight,
    Random
}

public enum IORefundMode
{
    NormalMode,
    Imeimmediately
}

public enum IOPrinterMode
{
    NoPrint,
    NormallyPrint,
    //StretchPrint
}

public enum IORecordMode
{
    Word,
    Icon
}

public enum IOLanguage
{
    ch,
    en
}

public enum IODifficult
{
    Visitor,
    Primary,
    Middle,
    Professional,
    Master,
    Supreme
}

public enum IONewGameMode
{
    Auto,
    Manual,
}

public enum IONetSwitch
{
    NetSwitchClose,
    NetSwitchOpen
}

public enum IOJackpotSetting
{
    JackpotSwitch,
    JpPercent,
    Save,
    Return
}

public enum IOJackpotSwitch
{
    JackpotSwitchOff,
    JackpotSwitchOn,
}

public enum IOBgmSetting
{
    BgmSwitch,
    BgmSelect,
    Save,
    Return
}

public enum IOBgmSwitch
{
    BgmSwitchOff,
    BgmSwitchOn,
}

public enum IOBgmSelect
{
    Bgm1,
    Bgm2,
    Bgm3,
    Random,
}

public class BillData
{
    public int coinIn;
    public int ticketOut;
    public int scoreUp;
    public int scoreDown;
    public int bets;
    public int wins;
    public int credit;
    public List<SBoxPlayerAccount> playerAccountList = new List<SBoxPlayerAccount>();
}

public class JackpotConfig
{
    public int jackpotSwitch;
    public int betPercent;
    public int jpPercent;
}

public class BetData
{
    public int macId;
    public int seatId;
    public int bet;
    public int betPercent;
    public int scoreRate;
    public int jpPercent;
    public int win;
    public int grandWin;
    public int grandTimes;
    public int majorWin;
    public int majorTimes;
    public int minorWin;
    public int minorTimes;
    public int miniWin;
    public int miniTimes;
}

public class IOCanvasModel : BaseManager<IOCanvasModel>
{
    public const int GameType = 157;//游戏类型
    public bool mouseClick = true;

    public Language curlanguage = Language.cht;
    public Language CurLanguage
    {
        set
        {
            curlanguage = value;
            defaultFontSize = curlanguage == Language.en ? 38 : 50;
        }
    }
    public int defaultFontSize = 30;

    public int tempNetSwitch;
    public int netSwitch;
    public int winLockBalance;

    public string sBoxVersion;

    public bool isInCoinOutCoolDown;
    public int totalCoinOutCont;
    public bool coinOuting;
    public bool coinIning;
    public int curCoinOutCount;

    public IOState state;
    public int permissions;
    public bool horizontal;
    public bool isInitMenu;
    public int curBillPage = -1;
    public const int COLUMN_COUNT = 4;
    public Color selectedColor = Color.red;
    public Color disableColor = Color.gray;

    public Color imageNormalColor = Color.white;
    public Color textNormalColor = new Color(248 / 255f, 223 / 255f, 27 / 255f);
    public Color textSelectedColor = Color.green;

    public List<int> netJackpotMinBets = new List<int>();
    //新增加
    public bool isTouchClick = false;//这是触摸点击//双屏时使用
    public Vector3 switchTouchPointPos = Vector3.zero;//触摸点,特化,给设置界面 多个选择 用的
    public Vector3 dateTimeTouchPointPos = Vector3.zero;//触摸点,特化,给设置界面 时间多个选择 用的

    public SBoxConfData CfgData
    {
        set
        {
            cfgData = value;
            SetTempCfgData(value);
            switchList = new List<int> { cfgData.SwitchBetsUnitMin, cfgData.SwitchBetsUnitMid, cfgData.SwitchBetsUnitMax };
            tempSwitchList = new List<int> { cfgData.SwitchBetsUnitMin, cfgData.SwitchBetsUnitMid, cfgData.SwitchBetsUnitMax };
        }
        get { return cfgData; }
    }

    private SBoxConfData cfgData;
    public SBoxConfData tempCfgData;
    public int SoundVolumScale
    {
        set
        {
            soundVolumScale = value;
            PlayerPrefs.SetInt("soundVolumScale", soundVolumScale);
            PlayerPrefs.GetInt("soundVolumScale");//防止没有存入
            AudioManager.Instance.SoundVolumScale = float.Parse(soundVolumScale.ToString()) / 100f;
        }
        get { return soundVolumScale; }
    }

    private int soundVolumScale;

    public int BgmSwitch
    {
        set
        {
            bgmSwitch = value;
            PlayerPrefs.SetInt("bgmSwitch", bgmSwitch);
        }
        get { return bgmSwitch; }
    }
    private int bgmSwitch = 1;//背景音乐开关,默认开

    public int BgmSelectIndex
    {
        set
        {
            bgmSelectIndex = value;
            PlayerPrefs.SetInt("bgmSelectIndex", bgmSelectIndex);
        }
        get { return bgmSelectIndex; }
    }
    private int bgmSelectIndex = 3;//背景音乐选择,默认随机

    public int tempBgmSwitch = 1;//背景音乐开关,临时值
    public int tempBgmSelectIndex = 3;//背景音乐选择,临时值

    public int BetSwitchCount
    {
        set
        {
            betSwitchCount = value;
        }
        get { return betSwitchCount; }
    }
    private int betSwitchCount = 6;//list  的 个数
    public int tempBetSwitchCount = 6;

    public int PrintCount
    {
        set
        {
            printCount = value;
            PlayerPrefs.SetInt("printCount", printCount);
            PlayerPrefs.GetInt("printCount");//防止没有存入
        }
        get { return printCount; }
    }
    private int printCount;

    public int PrintCountMax
    {
        set
        {
            printCountMax = value;
            PlayerPrefs.SetInt("printCountMax", printCountMax);
            PlayerPrefs.GetInt("printCountMax");//防止没有存入
        }
        get { return printCountMax; }
    }
    private int printCountMax = 5;//打印次数阈值,超过则持续弹提示提醒换纸,默认100

    public JackpotConfig JackpotCfg
    {
        set
        {
            jackpotCfg = value;
            tempJackpotCfg = new JackpotConfig
            {
                jackpotSwitch = jackpotCfg.jackpotSwitch,
                betPercent = jackpotCfg.betPercent,
                jpPercent = jackpotCfg.jpPercent
            };
        }
        get { return jackpotCfg; }
    }

    public JackpotConfig jackpotCfg;
    public JackpotConfig tempJackpotCfg;

    public Dictionary<int, List<BetData>> betDataDic;
    public List<BetData> betDataList;

    public SBoxCoderData coderData;

    public BillData billData = new BillData();

    public List<SBoxPrizePlayerData> sBoxPrizePlayerDatas = new List<SBoxPrizePlayerData>();

    public List<int> switchList = new List<int> { 10, 50, 100 };
    public List<int> tempSwitchList = new List<int> { 5, 10, 50, 100, 500, 1000 };

    public readonly List<int> SWITCH_LIMIT_MIN = new List<int> { 10, 50, 100 };
    public readonly List<int> SWITCH_LIMIT_MAX = new List<int> { 50, 200, 500 };

    //新bet 倍率切换
    public const int betSwitch_Limit_Min = 3;//最少压分倍率个数
    public const int betSwitch_Limit_Max = 6;//最多压分倍率个数

    //默认压分倍率值,用于直接修改切换个数(未打开倍率面板配置)时给未配置的位补默认值
    public static readonly int[] DEFAULT_BET_SWITCH = { 5, 10, 50, 100, 500, 1000 };

    public int[] BetSwitchList
    {
        get
        {
            return betSwitchList;
        }
        set
        {
            betSwitchList = value;
        }
    }
    private int[] betSwitchList = new int[betSwitch_Limit_Max];//默认创建最大个数的压分倍率
    public int[] tempBetSwitchList = new int[betSwitch_Limit_Max];//默认创建最大个数的压分倍率

    /// <summary>
    /// 确保前 count 个压分倍率位都有有效值,为0的位补默认倍率值(0表示该位未配置)
    /// </summary>
    public void EnsureBetSwitchDefaults(int count)
    {
        if (betSwitchList == null) betSwitchList = new int[betSwitch_Limit_Max];
        int len = count < betSwitchList.Length ? count : betSwitchList.Length;
        for (int i = 0; i < len; i++)
        {
            if (betSwitchList[i] == 0 && i < DEFAULT_BET_SWITCH.Length)
                betSwitchList[i] = DEFAULT_BET_SWITCH[i];
        }
    }

    /// <summary>
    /// 未显示的位清零(0表示该位不显示),发送给算法卡的押分数组只保留前 count 个有效值
    /// </summary>
    public void ClearBetSwitchTail(int count)
    {
        if (betSwitchList == null) return;
        for (int i = count; i < betSwitchList.Length; i++)
            betSwitchList[i] = 0;
    }

    /// <summary>
    /// 保存时按切换个数整理押分数组:前 count 位为0的补默认倍率值,count 之后的位清零
    /// </summary>
    public void ApplyBetSwitchCount(int count)
    {
        EnsureBetSwitchDefaults(count);
        ClearBetSwitchTail(count);
    }

    public const int switch_limit_min_zxd = 5;//压分倍率最小值
    public const int switch_limit_max_zxd = 10000;//压分倍率最大值

    public int curBetSwitchIndex = 0;//当前压分倍率下标


    public bool CanEditMachineId
    {
        get
        {
            return CfgData.MachineIdLock == 0 || permissions == 3;
        }
    }

    public const int MIN_COUNT_DOWN = 10;
    public const int MAX_COUNT_DOWN = 30;

    public const int MIN_MIN_BET = 10;
    public const int MAX_MIN_BET = 1000;

    public const int MIN_MAX_BET = 100;
    public const int MAX_MAX_BET = 9999;

    public const int MIN_LIMIT_BETS_WINS = 1000;
    public const int MAX_LIMIT_BETS_WINS = 100000;

    public const int MIN_COIN_RATIO = 1;
    public const int MAX_COIN_RATIO = 200;

    public const int MAX_TICKET_VALUE = 200;
    public const int MAX_VALUE_TICKET = 50;

    public const int MIN_SCORE_UP_RATIO = 100;
    public const int MAX_SCORE_UP_RATIO = 10000;

    public const int MIN_PRINT_TIME = 10;
    public const int MAX_PRINT_TIME = 120;

    public const int MIN_PRINT_DENSITY = 0;
    public const int MAX_PRINT_DENSITY = 5;

    public const int MIN_CLIENT_WIN_LOCK = 10000;
    public const int MAX_CLIENT_WIN_LOCK = 300000;//TODO:300000

    public const int MIN_ROUND_WIN_LOCK = 100000;
    public const int MAX_ROUND_WIN_LOCK = 500000;
    
    public const int MIN_MACHINE_WIN_LOCK = 500000;
    public const int MAX_MACHINE_WIN_LOCK = 2000000;
    public const int MIN_DOOR_OPEN_TIME = 20;
    public const int MAX_DOOR_OPEN_TIME = 120;

    public const int MIN_OFFSET_RATIO = 1;
    public const int MAX_OFFSET_RATIO = 100;

    public const int MIN_JACKPOT_PERCENT = 1;
    public const int MAX_JACKPOT_PERCENT = 100;

    public const int MIN_CUTODDS = 5;
    public const int MAX_CUTODDS = 50;

    public const int MIN_SOUND_VOLUM_SCALE = 0;
    public const int MAX_SOUND_VOLUM_SCALE = 100;

    public const int MIN_OFFSET_PRINT = 0;
    public const int MAX_OFFSET_PRINT = 16;


    public ulong password;
    public ulong tempPassword;
    public DateTime IODateTime;

    public int[] IOPrintSettingDate;
    public int[] tempIOPrintSettingDate;
    public int curPrintOffsetIndex;
    public List<string> IOBillList = new List<string>();//纸钞机型号列表
    public int IOBillSelectIndex = -1;//当前选中的纸钞机编号(BillState 返回值)
    public const int MAX_BILL_BTN = 5;//纸钞机按钮最多显示个数
    public List<string> IOPrinterList = new List<string>();//打印机型号列表
    public int IOPrinterSelectIndex = -1;//当前选中的打印机编号(PrinterState 返回值)
    public const int MAX_PRINTER_BTN = 5;//打印机按钮最多显示个数

    public bool isExitIOCanvas;//增加特殊判断
    public List<int> errorCodeListAboutPrint = new List<int>();

    public bool isBillPanelNeedCoderData;//zxd add
    public bool isShowInstantiateBillDetal;

    public Dictionary<Language, Dictionary<string, string>> languageDic;
    public Dictionary<Language, Dictionary<string, Sprite>> spriteDic;//TODO:

    public string GetDate(int remainMinute)
    {
        int temp = remainMinute;
        int day = temp / 1440;
        temp %= 1440;
        int hour = temp / 60;
        temp %= 60;
        return day + Utils.GetLanguage("Day") + hour + Utils.GetLanguage("Hour") + temp + Utils.GetLanguage("Minute");
    }

    public void SetTempCfgData(SBoxConfData data)
    {
        tempCfgData = new SBoxConfData
        {
            result = data.result,
            PwdType = data.PwdType,
            GameGear = data.GameGear,
            difficulty = data.difficulty,
            odds = data.odds,
            WinLock = data.WinLock,
            MachineId = data.MachineId,
            LineId = data.LineId,
            TicketMode = data.TicketMode,
            TicketValue = data.TicketValue,
            scoreTicket = data.scoreTicket,
            CoinValue = data.CoinValue,
            MaxBet = data.MaxBet,
            MinBet = data.MinBet,
            CountDown = data.CountDown,
            MachineIdLock = data.MachineIdLock,
            BetsMinOfJackpot = data.BetsMinOfJackpot,
            JackpotStartValue = data.JackpotStartValue,
            LimitBetsWins = data.LimitBetsWins,
            ReturnScore = data.ReturnScore,
            SwitchBetsUnitMin = data.SwitchBetsUnitMin,
            SwitchBetsUnitMid = data.SwitchBetsUnitMid,
            SwitchBetsUnitMax = data.SwitchBetsUnitMax,
            ScoreUpUnit = data.ScoreUpUnit,
            PrintMode = data.PrintMode,
            ShowMode = data.ShowMode,
            CheckTime = data.CheckTime,
            OpenBoxTime = data.OpenBoxTime,
            PrintLevel = data.PrintLevel,
            PlayerWinLock = data.PlayerWinLock,
            LostLock = data.LostLock,
            LostLockCustom = data.LostLockCustom,
            PulseValue = data.PulseValue,
            NewGameMode = data.NewGameMode,
            CutOdds = data.CutOdds
        };
        tempBetSwitchCount = BetSwitchCount;
    }

    public void SetTempJackpotCfg(JackpotConfig data)
    {
        tempJackpotCfg = new JackpotConfig
        {
            jackpotSwitch = data.jackpotSwitch,
            betPercent = data.betPercent,
            jpPercent = data.jpPercent
        };
    }

    public void DealWithBetDataDic()
    {
        betDataList = new List<BetData>();
        foreach (var clientBetDataList in betDataDic.Values)
            foreach (var item in clientBetDataList)
                if (item.jpPercent == jackpotCfg.jpPercent && int.Parse(System.Math.Round(1000f / IOCanvasModel.Instance.CfgData.CoinValue).ToString()) == item.scoreRate)
                    betDataList.Add(item);
        betDataList.Sort((a, b) => b.macId.CompareTo(a.macId));
    }

    /// <summary>
    /// 从算法卡 20002 配置更新压分倍率数据
    /// 显示个数 = 非 0 值个数（0 表示该位不显示/未配置）
    /// 算法卡未配置(全 0/空)时用默认倍率兜底,避免押注筹码为空
    /// </summary>
    public void UpdateBetSwitchFromConf(int[] betSwitch)
    {
        betSwitchList = new int[betSwitch_Limit_Max];
        betSwitchCount = 0;
        if (betSwitch != null && betSwitch.Length > 0)
        {
            for (int i = 0; i < betSwitch.Length && i < betSwitch_Limit_Max; i++)
            {
                betSwitchList[i] = betSwitch[i];
                if (betSwitch[i] > 0)
                    betSwitchCount++;
            }
        }
        //算法卡未配置时用默认倍率兜底,与旧 LoadIntArray 默认值行为一致
        if (betSwitchCount == 0)
        {
            for (int i = 0; i < DEFAULT_BET_SWITCH.Length && i < betSwitch_Limit_Max; i++)
                betSwitchList[i] = DEFAULT_BET_SWITCH[i];
            betSwitchCount = DEFAULT_BET_SWITCH.Length;
        }
    }
}