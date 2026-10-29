
using UnityEngine;
using SBoxApi;

public class RewardState : FSMState
{
    //从betState进入,
    //进入后根据幸运ID处理不同奖励流程,
    //如果中小游戏根据小游戏类型执行不同的小游戏流程和开奖流程,
    //如果没有中小游戏,则直接显示开奖结果,
    //显示完成后,如果是新一局进入prepareState,是新一轮进入endRoundState,等待新一轮开始
    ////如果在奖励过程中发生错误,则进入errorState,比如开奖异常

    public RewardState()
    {
        stateID = StateID.Reward;
        AddTransition(Transition.Prepare, StateID.Prepare);
        AddTransition(Transition.Setting, StateID.Setting);
        AddTransition(Transition.Error, StateID.Error);
        AddTransition(Transition.EndRound, StateID.EndRound);
    }

    public override void DoBeforeEntering()
    {
        OnReward();

    }

    public override void DoBeforeLeaving()
    {

    }


    /// <summary>
    /// 奖励事件，根据幸运ID处理不同奖励流程,根据小游戏类型执行不同的小游戏流程和开奖流程
    /// </summary>
    private void OnReward()
    {
    	switch (Model.Instance.testMode)
        {
            case 1:
                Model.Instance.curRecord.luckyId = 4;
                Model.Instance.curRecord.luckyPlayerId = 1;
                int[] ints = new int[Model.TYPE_COUNT];
                for (int j = 0; j < Model.TYPE_COUNT; j++) ints[j] = 3;
                Model.Instance.curRecord.extraData = ints;
                break;
            case 2:
                Model.Instance.curRecord.luckyId = 5;
                Model.Instance.curRecord.luckyPlayerId = 1;
                break;
            case 3:
                Model.Instance.curRecord.luckyId = 7;
                Model.Instance.curRecord.luckyPlayerId = 1;
                break;
            case 4:
                Model.Instance.curRecord.luckyId = 8;
                break;
            default:
                break;
        }
        switch (Model.Instance.curRecord.luckyId)
        {
            case 4:
            case 5:
            case 7:
                Controller.Instance.ShowLuckySeatGame();
                break;
            case 6:
                //明牌小游戏
                Controller.Instance.ShowResultSpecialFinish(Model.Instance.curRecord);
                break;
            case 8:
                //彩金小游戏
                Controller.Instance.ShowJackpot();
                Debug.LogError("彩金小游戏发送");
                SBoxIdea.BattLuckPrize();
                break;
            default:
                //没有中小游戏
                Controller.Instance.ShowResult(Model.Instance.curRecord);
                break;
        }
    }
}
