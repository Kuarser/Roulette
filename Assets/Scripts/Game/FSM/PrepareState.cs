

using Newtonsoft.Json;
using SBoxApi;
using UnityEngine;

public class PrepareState : FSMState
{
    //第一次开机器,
    //或者每次新一轮
    //或者每次新一局中,
    //进入这个状态,
    //获取游戏轮数和局数,根据轮数和局数判断是否需要打单,如果需要打单,则进入Print状态,如果不需要打单,则请求本局开奖结果和明牌信息,正式开始游戏,进入Bet状态
    public PrepareState()
    {
        stateID = StateID.Prepare;
        AddTransition(Transition.Bet, StateID.Bet);
        AddTransition(Transition.Error, StateID.Error);
        AddTransition(Transition.Setting, StateID.Setting);
        AddTransition(Transition.EndRound, StateID.EndRound);
    }

    private void AddEventListener()
    {
        //EventCenter.Instance.AddEventListener<int>(EventHandle.SBOX_NEED_ACTIVATED, OnNeedActivated); //0
        EventCenter.Instance.AddEventListener<int[]>(SBoxEventHandle.SBOX_BATTLE_GAME_NUMBER, OnGameNumber);//获取轮数,局数,prepare
        EventCenter.Instance.AddEventListener<double[]>(SBoxEventHandle.SBOX_BATTLE_RESET_GAME, OnResetGame);//1
        EventCenter.Instance.AddEventListener<int[]>(SBoxEventHandle.SBOX_BATTLE_REQUEST_RESULT, OnRequestResult);//2
        EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_BETS_START, OnBetsStart);//3
    }
    private void RemoveEventListener()
    {
        EventCenter.Instance.RemoveEventListener<int[]>(SBoxEventHandle.SBOX_BATTLE_GAME_NUMBER, OnGameNumber);
        EventCenter.Instance.RemoveEventListener<double[]>(SBoxEventHandle.SBOX_BATTLE_RESET_GAME, OnResetGame);
        EventCenter.Instance.RemoveEventListener<int[]>(SBoxEventHandle.SBOX_BATTLE_REQUEST_RESULT, OnRequestResult);
        EventCenter.Instance.RemoveEventListener<int>(SBoxEventHandle.SBOX_BETS_START, OnBetsStart);
    }

    public override void DoBeforeEntering()
    {
        AddEventListener();
        SBoxIdea.BattleGameNumber();
    }

    public override void DoBeforeLeaving()
    {
        RemoveEventListener();
    }

    private void OnResetGame(double[] odds)
    {
        //if (Model.Instance.curRound == 0)//Controller有个一样的if,Controller 的先执行并修改轮数,所以感觉这里的不会满足条件,所以永不执行
        //    Controller.Instance.StartNewRound();
        //else if (Model.Instance.curGame == Model.Instance.maxGame || Model.Instance.isInit)//本轮结束 或 第一次进入(打开)游戏软件时,结束本轮
        //    SBoxIdea.BattleEndRound();
        //else
        //{
        //    SBoxIdea.BattleRequestResult(0);//请求本局开奖结果和明牌信息,正式开始游戏
        //}

        //重置bankerBidState
        Model.Instance.bankerBidState = -1;
        Controller.Instance.view.uiBankerView.ResetBanker();//重置庄家信息
        SBoxIdea.BattleRequestResult(0);//请求本局开奖结果和明牌信息,正式开始游戏
    }

    private void OnRequestResult(int[] data)
    {
        Model.Instance.curGame++;
        if (Model.Instance.curGame != data[2])
        {
            Debug.LogError("BattleRequestResult 返回的游戏局数不正确,当前局数:" + Model.Instance.curGame + "返回局数:" + data[3]);
        }
        int[] temp = new int[6];
        for (int i = 0; i < 6; i++)
        {
            temp[i] = data[i+3];
        }
        Model.Instance.curRecord = new RecordStruct//prepare 状态获取一部分本局数据
        {
            round = data[1],
            game = data[2],
            value = temp,//3-8
            result = data[9],
            pair = new int[2]{data[10],data[11]},//10-11
            spresult = new int[4]{data[12],data[13],data[14],data[15]},//12-15
            cardnum = new int[2]{data[16],data[17]},//16-17
            luckyId = data[18] == 1 ? 6 : 0,
        };
        Debug.LogError($"庄闲对:{JsonConvert.SerializeObject(Model.Instance.curRecord.pair)}");
        SBoxIdea.BetsStart(IOCanvasModel.Instance.CfgData.CountDown);
    }

    private void OnBetsStart(int result)
    {
        FSMSystem.Instance.PerformTransition(Transition.Bet);
    }

    public void OnGameNumber(int[] data)
    {
        Model.Instance.curRound = data[1];
        Model.Instance.curGame = data[2];
        Controller.Instance.SetGameData();

        // 每局获取激活状态并检查打印机
        int activeCode = SBoxIdea.NeedActivated();
        bool isActive = OnNeedActivated(activeCode);
        if (!isActive)
        {
            FSMSystem.Instance.PerformTransition(Transition.Error);
            return;
        }

        if (Model.Instance.curGame == Model.MAX_GAME)
            FSMSystem.Instance.PerformTransition(Transition.EndRound);
        else
            SBoxIdea.BattleResetGame();
    }

    private bool OnNeedActivated(int activatedCode)
    {
        if (activatedCode == 0)
        {
            return true;
        }
        else if (activatedCode > 0)
        {
            switch (activatedCode)
            {
                case 1://运行时间到(时间打码相关)
                    Model.Instance.AddErrorCode(8, true);
                    break;
                case 2://盈余档机
                    Model.Instance.AddErrorCode(3, true);
                    Debug.LogError("盈余档机");
                    break;
                case 3://历史数据过大
                    Model.Instance.AddErrorCode(9, true);
                    break;
                case 4://全台爆机(总输分爆机)
                    Model.Instance.AddErrorCode(2, true);
                    Debug.LogError("全台爆机(总输分爆机)");
                    break;
                case 5://本轮爆机
                    Model.Instance.AddErrorCode(1, true);
                    Debug.LogError("本轮爆机");
                    break;
                default:
                    Debug.LogError($"报错,未标记的(不认识的)激活码:{activatedCode}");
                    break;
            }
        }
        return false;
    }
}
