using Newtonsoft.Json;
using SBoxApi;
using System.Collections;
using UnityEngine;

public class FirstStartState : FSMState
{
    //一开机进入状态
    public FirstStartState()
    {
        stateID = StateID.FirstStart;
        AddTransition(Transition.Prepare, StateID.Prepare);
        AddTransition(Transition.Print, StateID.Print);
        AddTransition(Transition.Error, StateID.Error);
        AddTransition(Transition.EndRound, StateID.EndRound);
    }

    void AddEventListener()
    {   
        EventCenter.Instance.AddEventListener<int[]>(SBoxEventHandle.SBOX_BATTLE_GAME_NUMBER, OnGameNumber);//读取当前轮数和局数,firstStartState
        EventCenter.Instance.AddEventListener<double[]>(SBoxEventHandle.SBOX_BATTLE_RESET_GAME, OnResetGame);//读取当前轮数和局数,firstStartState
        EventCenter.Instance.AddEventListener<int[]>(SBoxEventHandle.SBOX_BATTLE_GET_COMPLETED_GAME, OnGetCompletedGame);//读取已经开奖结果,firstStartState
    }

    void RemoveEventListener()
    {
        EventCenter.Instance.RemoveEventListener<int[]>(SBoxEventHandle.SBOX_BATTLE_GAME_NUMBER, OnGameNumber);
        EventCenter.Instance.RemoveEventListener<double[]>(SBoxEventHandle.SBOX_BATTLE_RESET_GAME, OnResetGame);//读取当前轮数和局数,firstStartState
        EventCenter.Instance.RemoveEventListener<int[]>(SBoxEventHandle.SBOX_BATTLE_GET_COMPLETED_GAME, OnGetCompletedGame);
    }

    public override void DoBeforeEntering()
    {
        if (Model.Instance.rouletteFlow)
        {
            MonoMgr.Instance.StartCoroutine(DelayRoulettePrepare());
            return;
        }

        AddEventListener();

        SBoxIdea.BattleGameNumber();
    }

    public override void DoBeforeLeaving()
    {
        if (!Model.Instance.rouletteFlow) RemoveEventListener();
    }

    // 轮盘模式：不查询算法卡轮局，延迟一帧直接进入 Prepare 打开押分窗口。
    private IEnumerator DelayRoulettePrepare()
    {
        yield return null;
        if (FSMSystem.Instance.CurrentStateID != StateID.FirstStart) yield break;
        FSMSystem.Instance.PerformTransition(Transition.Prepare);
    }

    private void ShowErrorStr(int activatedCode)
    {
        if (activatedCode > 0)
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
    }

    private void OnGameNumber(int[] data)
    {
        //本地保存轮局
        Model.Instance.curRound = data[1];
        Model.Instance.curGame = data[2];
        if (Model.Instance.curRound == 0)
        {               
            Model.Instance.isInit = false;
            StartNewRound();
            return;
        }
        //ui显示轮局
        Controller.Instance.SetGameData();//更新轮数,局数
        //增加的,可能不用
        Model.Instance.isInit = false;
        Model.Instance.RemainCards = Model.MAX_REMAIN_CARDS;//重置剩余牌数
        SBoxIdea.BattleGetCompletedGame();
    }

#region 第0轮特殊情况才会调用,正常情况不会调用
    public void StartNewRound()
    {
        //判断机器状态
        int activeCode = SBoxIdea.NeedActivated();
        if (activeCode == 0 && Model.Instance.errorCodeList.Count == 0)//正常,机器是激活的,获取轮,局
            SBoxIdea.BattleResetGame();
        else //不正常,检查原因,如果报错,进入错误状态
        {
            ShowErrorStr(activeCode);
            FSMSystem.Instance.PerformTransition(Transition.Error);
            SBoxIdea.BattleEndRound();
        }
    }

    private void OnResetGame(double[] odds)
    {
        FSMSystem.Instance.PerformTransition(Transition.Print);
    }
#endregion



    private void OnGetCompletedGame(int[] data)
    {
        MonoMgr.Instance.StartCoroutine(DelayTransition());//延迟显示
    }

    private IEnumerator DelayTransition()
    {
        yield return new WaitForSeconds(1f);
        if (Model.Instance.errorCodeList.Count > 0)
        {
            FSMSystem.Instance.PerformTransition(Transition.Error);
            SBoxIdea.BattleEndRound();
            yield break;
        }
        FSMSystem.Instance.PerformTransition(Transition.EndRound);
    }
}
