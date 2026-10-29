using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SBoxApi;
using Spine;
using UnityEngine;

public class Model : BaseManager<Model>
{


    private int _a;

    public int a
    {
        get { return _a; }
        set
        {
            _a = value;
            Debug.Log($"A的值被修改了,新值为:{_a}");
        }
    }


    private int b;

    public const int CONST_DATA = 10;

    private string myData;




#region 游戏数据
    //押注类型个数（5→9，2026-07-30）
    public const int TYPE_COUNT = 9;
    //设计:第几轮第几局
    public int curRound;//当前轮数
    public int curGame;//当前局数
    public const int MAX_GAME = 66;//最大局数,常量
    //每局开始前的倒计时
    public int curCountDown;//当前倒计时
    /*****************卡牌数*****************/
    //剩余卡牌数
    public int RemainCards;
    //最大卡牌数,常量
    public const int MAX_REMAIN_CARDS = 432;
    //硬币相关,押注倍率
    public List<int> betCoinList = new List<int>() { 5, 10, 50, 100, 500 };
    /***********************押分相关***********************/
    //所有在线机器的总押分情况
    public int[] totalBets = new int[TYPE_COUNT];

    /***********************牌路界面相关***********************/
    //珠盘路结果,用数字代表,Baccarat※ 1:庄,2:闲,3:和 
    private List<int> beadPlateList = new List<int>();
    //大路结果,用数字代表,0:红,1:蓝,2:绿
    private List<List<int>> bigRoadList = new List<List<int>>();
    //大眼路结果,用数字代表,0:红,1:蓝
    public List<List<int>> bigEyeRoadList = new List<List<int>>();
    //小路结果,用数字代表,0:红,1:蓝
    public List<List<int>> smallRoadList = new List<List<int>>();
    //小强路结果,用数字代表,0:红,1:蓝
    public List<List<int>> dragonRoadList = new List<List<int>>();
    //抢庄相关
    public SBoxBattleBankerData banckerData = new SBoxBattleBankerData();
    public bool canBidBanker = true;//是否可以抢庄了（明牌）

    /*******************游戏结果记录*******************/
    //本局游戏记录
    public RecordStruct curRecord;
    public SBoxBattlePlayerPayoutData[] PayoutData = new SBoxBattlePlayerPayoutData[10];
    //当前轮中已玩过的每局游戏结果记录
    public List<RecordStruct> records = new List<RecordStruct>();

    public List<RecordStruct> blizzTestRecords = new List<RecordStruct>();

    //当前轮中每种游戏结果累计出现次数
    public int[] recordTypeCounts = new int[TYPE_COUNT];
    /*******************押注历史记录的循环滚动视图(按页)*******************/
    //存储一页中的记录的容器
    List<TotalDataStruct> _cacheList = new List<TotalDataStruct>();//
    //当前页号
    public int recordListPage = 1;
    //总页号
    public int maxPage = 0;
    //原本13//一页最多记录个数7->1
    int maxCount = 1;

    /*******************联网彩金*******************/
    public int netJackpotState;
    public bool showingNetJackpot;
    
    public int bankerBidState = -1;//-1初始和开局状态,0未抢庄,1抢庄了
	public int testMode = -1;
    public Queue<WinJackpotInfo> winJackpotInfos = new Queue<WinJackpotInfo>();
    public Dictionary<int, List<JackBetInfo>> netJackpotBetBuffer = new Dictionary<int, List<JackBetInfo>>();

#endregion

#region 游戏辅助数据
    public bool isInit;
    public bool wait4Setting;
    public bool printerPrinting;
    public bool haveClickPrint;
    public bool checking;
    public bool wait4Print;
    public bool startingNewRound;
    public bool flashing;
    public List<int> errorCodeList = new List<int>();
    public List<int> winLockPlayers = new List<int>();
    public List<int> moveScoreIdList = new List<int>();
    //等待小游戏选卡牌
    public bool waitSelectCard;
    //触摸区域能押注的计数,控制在某些界面显示时不能响应触摸区域事件 -kuarser：已修改为计数器，正数时不可触摸
    public int myTriggerDetectionCanEnterCount = 0;
    public bool isJoker = false;   // 和局标志（原皇冠功能转移，开"和"时为 true）
    public bool isNoBanker = false;
    public bool isShowCard = false;
    //假积分标志,true时冻结分数刷新,用于开奖结算期间暂停刷新分数
    public bool fadeCredit = false;
#endregion

#region 初始化
    public void Init()
    {
        isInit = true;
        InitVariable();
    }

    private void InitVariable()
    {
        curGame = 0;
        curRound = 0;
        curCountDown = 0;
        RemainCards = MAX_REMAIN_CARDS;


        if (IOCanvasModel.Instance.BetSwitchCount == 0)
        {
           if (IOCanvasModel.Instance.BetSwitchList != null)
           {
               if (IOCanvasModel.Instance.BetSwitchList[0] == 0 || IOCanvasModel.Instance.BetSwitchList[0] == 1)
               {
                   for (int i = 0; i < IOCanvasModel.Instance.BetSwitchList.Length && i < Model.Instance.betCoinList.Count; i++)
                       IOCanvasModel.Instance.BetSwitchList[i] = Model.Instance.betCoinList[i];
               }
           }
        }
        else
        {
           //压分倍率
           betCoinList.Clear();
           for (int i = 0; i < IOCanvasModel.Instance.BetSwitchList.Length && i < IOCanvasModel.Instance.BetSwitchCount; i++)
           {
               int item = IOCanvasModel.Instance.BetSwitchList[i];
               betCoinList.Add(item);
           }
        }
    }
#endregion

    /// <summary>
    /// 构建押注数组，将源数组复制到固定长度数组中（不足补0，超出截断）
    /// </summary>
    public static int[] BuildBetArray(int[] source, int count)
    {
        int[] arr = new int[count];
        if (source == null)
        {
            Debug.LogError($"BuildBetArray: 源数组为null, 返回全0的{count}元素数组");
            return arr;
        }
        int len = source.Length < count ? source.Length : count;
        for (int i = 0; i < len; i++) arr[i] = source[i];
        if (source.Length < count)
        {
            Debug.LogWarning($"BuildBetArray: 源数组长度({source.Length})小于目标长度({count}), 已补0 => [{string.Join(",", arr)}]");
        }
        else if (source.Length > count)
        {
            Debug.LogWarning($"BuildBetArray: 源数组长度({source.Length})大于目标长度({count}), 已截断 => [{string.Join(",", arr)}]");
        }
        return arr;
    }

#region 方法
    /*
         value ：结果，高4bits为花色：1：黑桃，2：红心，3：梅花，4：方块，5：王
                 低4bits为点数：1~13：A - 2~10 - J~K，14：小王，15：大王
                 算法塔主要是给卡牌用的,但是这个游戏也能使用高4bits,所以需要用方法GetRewardResult()计算重新计算一下
                 所以,单机游戏时这里只用高4bits,也就是UnityEngine.Random.Range(1, 6) >> 4,来适配GetRewardResult()方法
         round ：当前是第几轮游戏
         game  ：当前是第几局游戏
         luckyId ：是否=6,是否明牌
    */


    /// <summary>
    /// 解析扑克数据，将整数值拆分为类型和数值。
    ///  value ：结果，高4bits为花色：1：黑桃，2：红心，3：梅花，4：方块，5：王
    /// 低4bits为点数：1~13：A - 2~10 - J ~K，14：小王，15：大王
    /// </summary>
    /// <param name="value">扑克原始值</param>
    /// <returns>PokerData对象</returns>
    public PokerData GetPokerData(int value)//获取扑克牌,包含类型和值
    {
        int typeOffset = 0xF0;
        int valueOffset = 0x0F;

        PokerData poker = new PokerData
        {
            type = (value & typeOffset) >> 4,
            value = value & valueOffset
        };
        if(poker.type < 1 || poker.type > 5)Debug.LogError($"牌值类型非法：{poker.type}");
        return poker;
    }

    /// <summary>
    /// 解析奖励结果，从原始值中提取奖励类型。
    /// </summary>
    /// <param name="value">原始值</param>
    /// <returns>奖励类型</returns>
    public int GetRewardResult(int value)//获取值
    {
        int offset = 0xF0;
        int result = (value & offset) >> 4;
        return result;
    }

    /// <summary>
    /// 根据开奖结果计算九门中奖情况，返回 bool[9]（每门是否中奖）
    /// 下标：0庄 1闲 2和 3庄对 4闲对 5小老虎 6大老虎 7幸运7 8幸运8
    /// </summary>
    public static bool[] CalcWinDoors(RecordStruct r)
    {
        bool[] wins = new bool[TYPE_COUNT];
        if (r == null) return wins;
        if (r.result == 1) wins[0] = true;      // 庄
        else if (r.result == 2) wins[1] = true; // 闲
        else if (r.result == 3) wins[2] = true; // 和
        int[] pair = r.pair;
        if (pair != null && pair.Length > 1)
        {
            if (pair[0] == 1) wins[3] = true;   // 庄对
            if (pair[1] == 1) wins[4] = true;   // 闲对
        }
        int[] sp = r.spresult;
        if (sp != null && sp.Length > 3)
        {
            if (sp[0] == 1) wins[5] = true;     // 小老虎
            if (sp[1] == 1) wins[6] = true;     // 大老虎
            if (sp[2] == 1) wins[7] = true;     // 幸运7
            if (sp[3] == 1) wins[8] = true;     // 幸运8
        }
        return wins;
    }

    //错误码
    //0:按键档机
    //1:本轮爆机
    //2:全台爆机(总输分爆机)
    //3:盈余档机
    //4:保单箱未连接
    //5:打印机缺纸
    //6:非法开箱1
    //7:非法开箱2
    //8:运行时间到(时间打码相关)
    //9:历史数据过大
    //10:正在打印
    //11:打印机切刀故障
    //12:打印机门未关闭


    /// <summary>
    ///  添加错误码到列表，并可立即显示提示
    /// </summary>
    /// <param name="errCode">错误码</param>
    /// <param name="showImmediately">是否立即显示错误提示</param>
    public void AddErrorCode(int errCode, bool showImmediately = false)
    {
        if (errorCodeList.Contains(errCode)) return;
        errorCodeList.Add(errCode);
        if (showImmediately)
        {
            PopTips.Instance.ShowSystemTips(GetErrorStr(errCode), -1);
        }
        Debug.LogError($"AddErrorCode: {errCode}");
    }

    public bool PrinterError()
    {
        return errorCodeList.Contains(4) 
        || errorCodeList.Contains(5) 
        || errorCodeList.Contains(6) 
        || errorCodeList.Contains(7) 
        || errorCodeList.Contains(11) 
        || errorCodeList.Contains(12);
    }

    /// <summary>
    /// 移除错误码，并根据情况触发状态切换事件
    /// </summary>
    /// <param name="errCode">错误码</param>
    /// <param name="checkContinue">是否要切换状态</param>
    public void RemoveErrorCode(int errCode, bool checkContinue = true)
    {
        if (!errorCodeList.Contains(errCode)) return;
        errorCodeList.Remove(errCode);
        Debug.LogError($"RemoveErrorCode: {errCode}");
        if (checkContinue && !printerPrinting)
            EventCenter.Instance.EventTrigger(EventHandle.ACTIVE);

        // 防止快速拔插保单箱,不会重新开始新一轮,
        if (errCode != 0
            && errCode != 10
            && !errorCodeList.Contains(0)
            && FSMSystem.Instance.CurrentStateID != StateID.Setting
            && FSMSystem.Instance.CurrentStateID != StateID.Error
            && FSMSystem.Instance.CurrentStateID != StateID.Prepare
            && FSMSystem.Instance.CurrentStateID != StateID.Print
            && FSMSystem.Instance.CurrentStateID != StateID.EndRound
            )
            AddErrorCode(0);//0:直接结束本轮
    }

    public string GetErrorStr(int errCode)
    {
        switch (errCode)
        {
            case 0:
                return $"{"EndRound"}";
            case 1:
                return $"{"RoundWinLock"}";
            case 2:
                return $"{ "MachineWinLock"}";
            case 3:
                return $"{ "WinLock"}";
            case 4:
                return $"{ "PolicyBoxDisconnect"}"; ;
            case 5:
                return $"{ "PrinterOutOfPaper"}";
            case 6:
                return $"{ "IllegalOpeningBox"}1";
            case 7:
                return $"{ "IllegalOpeningBox"}2"; ;
            case 8:
                return $"{ "RunningTimeIsUp"}"; ;
            case 9:
                return $"{ "DataOutOfLimit"}";
            case 10:
                return $"{ "Printing"}";
            case 11:
                return $"{ "PrinterCutterBroken"}";
            case 12:
                return $"{ "PrinterDoorNotClosed"}";
            default:
                return "";
        }
    }

    //历史记录相关
    public int GetRecordListCount()
    {
        return _cacheList.Count;
    }

    /// <summary>
    /// 获取指定页的历史记录列表
    /// </summary>
    /// <param name="page">页码</param>
    /// <returns>历史记录列表</returns>
    public object GetRecordListByPage(int page)
    {
        if (recordListPage != page)
        {
            recordListPage = page;
            RefreshCacheRecordList();
        }
        return _cacheList;
    }

    public int GetRecordListCountByPage(int page)
    {
        if (recordListPage != page)
        {
            recordListPage = page;
        }
        RefreshCacheRecordList();
        return _cacheList.Count;
    }

    public void RefreshCacheRecordList()
    {
        _cacheList.Clear();
        var list = PlayerMgr.Instance.playerDic[SBoxModel.Instance.macId].totalDataStruct;
        //list.Reverse();

        maxPage = PlayerMgr.Instance.playerDic[SBoxModel.Instance.macId].totalDataStruct.Count;//(list.Count + maxCount - 1) / maxCount;※
        if (recordListPage <= 0)
            recordListPage = maxPage;
        if (recordListPage > maxPage)
            recordListPage = 1;
        _cacheList = list.Skip((recordListPage - 1) * maxCount).Take(maxCount).ToList();
    }

#region 计算各种路的数据:珠盘路,大路,小路

    //就珠盘路需要从外面赋值,其他路都是用珠盘路计算的
    public void AddBeadPlateRoadData(int value)//添加 珠盘路
    {
        beadPlateList.Add(value);
    }

    public void SetAllRoadData()//设置路
    {
        if (records == null) return;
        //计算数字模型的珠盘路,大路,大眼路,小路,小强路
        //通过算法卡数据设置珠盘路
        beadPlateList.Clear();
        for (int i = 0; i < records.Count; i++)
        {
            RecordStruct record = records[i];
            AddBeadPlateRoadData(record.result);//珠盘路只存庄闲和(1庄/2闲/3和),六门不进路
        }
        bigEyeRoadList.Clear();
        smallRoadList.Clear();
        dragonRoadList.Clear();//先清掉
        //通过珠盘路设置大路
        CalculateBigRoadData();
        //通过大路设置大眼路,小路,小强路
        CalculateOtherRoadData();
        AllRoadStruct xiarecord = new AllRoadStruct
        {
            beadPlateRoadList = beadPlateList,
            bigRoadList = bigRoadList,
            bigEyeRoadList = bigEyeRoadList,
            smallRoadList = smallRoadList,
            dragonRoadList = dragonRoadList,
        };
        MsgInfo msgInfo = new MsgInfo
        {
            cmd = (int)S2C_CMD.S2C_XiaSanRoadData,
            id = -1,
            jsonData = JsonConvert.SerializeObject(xiarecord),
        };
        NetMgr.Instance.SendToAllClient(JsonConvert.SerializeObject(msgInfo));
    }

    //计算各种路的数据,根据珠盘路数据计算大路数据,再根据大路数据计算大眼路,小路,小强路数据

    public void CalculateBigRoadData()//计算大路数据
    {
        if (beadPlateList == null || beadPlateList.Count <= 0) return;
        bigRoadList.Clear();

        int lastValue = 2;//上一个结果,初始值为2,假设为特殊值
        for (int i = 0; i < beadPlateList.Count; i++)//在已有的珠盘路数据基础上计算大路数据,所以从0开始循环,而不是从最后一条数据开始循环
        {
            int result = beadPlateList[i] - 1;//需要减一,BeadPlateList里面是1:黑桃2红心3梅花4方块5王
            switch (result)
            {
                case 0://庄
                    AddRoadData(bigRoadList, 0, lastValue);//0:红
                    lastValue = 0;
                    break;
                case 1://闲
                    AddRoadData(bigRoadList, 1, lastValue);//1:蓝
                    lastValue = 1;
                    break;
            }
        }
        if(bigRoadList.Count == 0)Controller.Instance.view.uiRoadMapView.RefreshOnNewRound();
    }
    public List<List<int>> CalculateBigRoadDataTemp(List<List<int>> bigRoadList, List<int> beadPlateList)//计算大路数据用于预测
    {
        if (beadPlateList == null || beadPlateList.Count <= 0) return new List<List<int>>();
        bigRoadList.Clear();

        int lastValue = 2;//上一个结果,初始值为2,假设为特殊值
        for (int i = 0; i < beadPlateList.Count; i++)//在已有的珠盘路数据基础上计算大路数据,所以从0开始循环,而不是从最后一条数据开始循环
        {
            int result = beadPlateList[i] - 1;//需要减一,BeadPlateList里面是1:黑桃2红心3梅花4方块5王
            switch (result)
            {
                case 0://庄
                    AddRoadData(bigRoadList, 0, lastValue);//0:红
                    lastValue = 0;
                    break;
                case 1://闲
                    AddRoadData(bigRoadList, 1, lastValue);//1:蓝
                    lastValue = 1;
                    break;
            }
        }
        return bigRoadList;
    }
    public int[] lastValueYuce = {-1,-1,-1};
    //计算大眼路,小路,小强路数据
    public void CalculateOtherRoadData()
    {
        bigEyeRoadList.Clear();
        smallRoadList.Clear();
        dragonRoadList.Clear();
        if (bigRoadList == null || bigRoadList.Count == 0) return;
        
        List<int> beadPlateListTemp = new List<int>();
        string jsdata = JsonConvert.SerializeObject(beadPlateList);
        beadPlateListTemp = JsonConvert.DeserializeObject<List<int>>(jsdata);
        beadPlateListTemp.Add(2);//加一个闲算
        List<List<int>> bigRoadListTemp = new List<List<int>>();
        CalculateBigRoadDataTemp(bigRoadListTemp,beadPlateListTemp);
        List<List<int>> bigEyeRoadListTemp = new List<List<int>>();
        List<List<int>> smallRoadListTemp = new List<List<int>>();
        List<List<int>> dragonRoadListTemp = new List<List<int>>();
        AddRoadData(bigEyeRoadListTemp, 0, bigRoadListTemp, out lastValueYuce[0]);
        AddRoadData(smallRoadListTemp, 1, bigRoadListTemp, out lastValueYuce[1]);
        AddRoadData(dragonRoadListTemp, 2, bigRoadListTemp, out lastValueYuce[2]);
        int TempValue = 0;
        AddRoadData(bigEyeRoadList, 0, bigRoadList,out TempValue);
        AddRoadData(smallRoadList, 1, bigRoadList,out TempValue);
        AddRoadData(dragonRoadList, 2, bigRoadList,out TempValue);
    }
    
    //添加大眼路数据,小路和小强路数据的计算方法一样,所以共用这个方法,通过传入不同的参数来控制计算结果的不同
    private void AddRoadData(List<List<int>> tempRoadList, int intervalCol, List<List<int>> bigRoadList, out int TempValue)
    {
        int lastValue = -1;//上一个结果,初始值为-1,表示没有结果
        int startCol = 1;//从第二列开始计算,第一列没有路可言
        int startRow = 1;
        for (int i = startCol + intervalCol; i < bigRoadList.Count; i++)//从第二列开始计算,第一列没有路可言
        {
            int tempSpaceRow = 0;//上一列与本列的同行为空的个数,用来判断大眼路和小路的结果
            for (int j = 0; j < bigRoadList[i].Count; j++)
            {
                if (i == startCol + intervalCol && j < startRow) continue;

                if (j == 0)//如果是第一行,比较上一列和上两列的列数,如果相同,结果为红;如果不同,结果为蓝
                {
                    if (bigRoadList[i - 1].Count == bigRoadList[i - 2 - intervalCol].Count)
                    {
                        AddRoadData(tempRoadList, 0, lastValue);
                        lastValue = 0;
                    }
                    else
                    {
                        AddRoadData(tempRoadList, 1, lastValue);
                        lastValue = 1;
                    }
                }
                else
                {
                    if (tempSpaceRow > 0 || j < bigRoadList[i - 1 - intervalCol].Count)//前一列同一行有数据,或者上一列与本列的同行为空的个数大于0,结果为红;否则为蓝,并且上一列与本列的同行为空的个数加1
                    {
                        AddRoadData(tempRoadList, 0, lastValue);
                        lastValue = 0;
                    }
                    else
                    {
                        AddRoadData(tempRoadList, 1, lastValue);
                        lastValue = 1;
                        tempSpaceRow++;
                    }
                }
            }
        }
        TempValue = lastValue;
    }

    private void AddRoadData(List<List<int>> bigRoadList, int value, int lastValue)//添加路
    {
        if (bigRoadList == null) return;

        if (value == 2 && value == lastValue)//第一行第一列第一个值是2,和,
        {
            if (bigRoadList.Count == 0)//如果还没有列,先添加一列
            {
                List<int> tempColList = new List<int>
                {
                    value
                };
                bigRoadList.Add(tempColList);
            }
            bigRoadList[bigRoadList.Count - 1].Add(value);
        }
        else if (value != lastValue && value != 2)// 值不同,换行了,所以新建一列;
        {
            List<int> tempColList = new List<int>
            {
                value
            };
            bigRoadList.Add(tempColList);
        }
        else// 值相同,继续在当前列添加数据
        {
            if (bigRoadList.Count == 0)//如果还没有列,先添加一列
            {
                List<int> tempColList = new List<int>
                {
                    value
                };
                bigRoadList.Add(tempColList);
            }
            bigRoadList[bigRoadList.Count - 1].Add(value);
        }
    }

    //预测,在大路的基础上
    public int DaluYuCe(int intervalCol)
    {
        if (bigRoadList == null || bigRoadList.Count < 1 + intervalCol) return -1;

        int lastCol = bigRoadList.Count - 1;
        int i = lastCol;
        int tempLast = bigRoadList[lastCol][0];
        int lastValue;

        if (tempLast == 0)//预测下一个数据为红,是同一列
        {
            if (i - 1 - intervalCol < 0 || i - 1 - intervalCol >= bigRoadList.Count)
            {
                Debug.Log($"i-1-intervalCol:{i - 1 - intervalCol}");
                return -1;
            }
            if (bigRoadList[i].Count + 1 < bigRoadList[i - 1 - intervalCol].Count
                || bigRoadList[i].Count + 1 > bigRoadList[i - 1 - intervalCol].Count + 1)//当前列个数小于前一列个数,即前一列同一行有数据;或者当前列个数大于等于前一列个数2行及以上,即上一列与本列的同行为空的个数大于0,结果为红;否则为蓝,并且上一列与本列的同行为空的个数加1
            {
                lastValue = 0;
            }
            else
            {
                lastValue = 1;
            }
        }
        else//新一列
        {
            if (i - 2 - intervalCol >= bigRoadList.Count || i - 2 - intervalCol < 0)
            {
                Debug.Log($"i-2-intervalCol:{i - 2 - intervalCol}");
                return -1;
            }
            if (bigRoadList[i - 1].Count == bigRoadList[i - 2 - intervalCol].Count)//前两行的个数是否相同
            {
                lastValue = 0;
            }
            else
            {
                lastValue = 1;
            }
        }
        return lastValue;
    }

    //i,j 下标, intervalCol 判断是什么路?大眼路?小路?小强路?
    public int DaluYuCeByIndex(int intervalCol, int i, int j)
    {
        if (i < 1 + intervalCol) return -1;
        if (i > bigRoadList.Count - 1) return -2;
        if (j > bigRoadList[i].Count - 1) return -3;

        int tempLast = bigRoadList[i][0];
        int lastValue;

        if (tempLast == 0)//预测下一个数据为红,是同一列
        {
            //j+1 从下标变为个数,再 +1 预测下一个数
            if (j + 2 <= bigRoadList[i - 1 - intervalCol].Count
                || j + 2 > bigRoadList[i - 1 - intervalCol].Count + 1)//当前列个数小于等于前一列个数,即前一列同一行有数据;或者当前列个数大于等于前一列个数2行及以上,即上一列与本列的同行为空的个数大于0,结果为红;否则为蓝,并且上一列与本列的同行为空的个数加1
            {
                lastValue = 0;
            }
            else
            {
                lastValue = 1;
            }
        }
        else//新一列
        {
            if (j + 1 == bigRoadList[i - 1 - intervalCol].Count)//前两行的个数是否相同
            {
                lastValue = 0;
            }
            else
            {
                lastValue = 1;
            }
        }
        return lastValue;
    }

    internal void SetRecordTypeCount()
    {
        for (int i = 0; i < recordTypeCounts.Length; i++)//重置
        {
            recordTypeCounts[i] = 0;
        }
        for (int i = 0; i < records.Count; i++)
        {
            var record = records[i];
            bool[] wins = CalcWinDoors(record);
            for (int j = 0; j < wins.Length; j++)
            {
                if (wins[j]) recordTypeCounts[j]++;//赋值(每门独立)
            }
        }
    }
#endregion

#endregion
}


/// <summary>
/// 扑克数据结构，包含类型和数值。
/// </summary>
public class PokerData
{
    /// <summary>扑克类型</summary>
    public int type;
    /// <summary>扑克数值</summary>
    public int value;
}
