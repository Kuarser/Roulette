using SBoxApi;
using System.Collections;
using UnityEngine;


public class PrintState : FSMState
{
    //从prepareState进入,
    //准备打单,打开保单箱,等待对单完成,如果是自动打印或者手动打印但已经点击了打印按钮,则等待打印机状态正常后开始新一轮,如果是手动打印但还没有点击打印按钮,则等待点击打印按钮事件,点击后等待打印机状态正常后开始新一轮
    //如果没有问题,开始新一轮,进入betState
    //如果有问题,根据不同的错误类型,显示不同的提示,等待人工干预,进入errorState
    //或者进入settingState调整设置,调整完成后根据不同的情况,进入prepareState或errorState

    public PrintState()
    {
        stateID = StateID.Print;
        AddTransition(Transition.Prepare, StateID.Prepare);
        AddTransition(Transition.Setting, StateID.Setting);
        AddTransition(Transition.Error, StateID.Error);
    }

    private void AddEventListener()
    {
        EventCenter.Instance.AddEventListener(EventHandle.PRINT_BTN_CLICK, OnPrintBtnClick);
        EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_BATTLE_NEW_ROUND, OnBattleNewRound);
    }
    private void RemoveEventListener()
    {
        EventCenter.Instance.RemoveEventListener(EventHandle.PRINT_BTN_CLICK, OnPrintBtnClick);
        EventCenter.Instance.RemoveEventListener<int>(SBoxEventHandle.SBOX_BATTLE_NEW_ROUND, OnBattleNewRound);
    }

    public override void DoBeforeEntering()
    {
        Model.Instance.startingNewRound = true;
        AddEventListener();
        OpenPrinterBox();
    }

    public override void DoBeforeLeaving()
    {
        RemoveEventListener();
        Model.Instance.startingNewRound = false;
        Model.Instance.wait4Print = false;
        Model.Instance.haveClickPrint = false;
        Model.Instance.printerPrinting = false;
        Model.Instance.checking = false;
    }


    private void OpenPrinterBox()
    {
        if (IOCanvasModel.Instance.CfgData.PrintMode != 0)
            SBoxIdea.BattPrinterOpenBox(false, true);
        MonoMgr.Instance.StartCoroutine(DelayStartNewRoundCorountine());
    }
    private IEnumerator DelayStartNewRoundCorountine()
    {      
        // 打单模式，0：不打印，1：正常打印，2：伸缩打印
        if (IOCanvasModel.Instance.CfgData.PrintMode == 0)
            StartNewRound();//不打印,直接开始新一轮
        else
        {
            Model.Instance.checking = true;
            float delayTimes = IOCanvasModel.Instance.CfgData.CheckTime;
            //等待服务员对单(提示:正在验证...)
            while (delayTimes > -1)
            {
                if (!Model.Instance.checking)
                {
                    yield break;
                }
                if (Model.Instance.errorCodeList.Count == 0)
                    PopTips.Instance.ShowSystemTips($"{Utils.GetLanguage("Checking")} {delayTimes} {Utils.GetLanguage("Seconds")}", -1, isChecking:true, seconds:(int)delayTimes);
                else
                {
                    FSMSystem.Instance.PerformTransition(Transition.Error);
                    yield break;
                }
                yield return new WaitForSeconds(1);
                delayTimes--;
            }

            Controller.Instance.HideTips();
            Model.Instance.checking = false;
            if (IOCanvasModel.Instance.CfgData.NewGameMode == 0)
                //自动打印直接走开局流程
                StartNewRound();
            else
            {
                //手动打印,等待点击打印按钮
                Model.Instance.wait4Print = true;//正在等待手动打印
                if (Model.Instance.errorCodeList.Count == 0)
                    PopTips.Instance.ShowSystemTips("WaitingPrint", -1);
            }
        }
    }

    private void OnPrintBtnClick()
    {
        if (IOCanvasModel.Instance.CfgData.NewGameMode != 1
            || !Model.Instance.startingNewRound
            || Model.Instance.haveClickPrint
            || !Model.Instance.wait4Print)
            return;
        Model.Instance.haveClickPrint = true;
        if (Model.Instance.wait4Print)
        {
            Controller.Instance.HideTips();
            StartNewRound();
        }
    }

    private void StartNewRound()
    {
        Debug.LogError("Call StartNewRound");
        Model.Instance.wait4Print = false;
        Model.Instance.printerPrinting = true;
        //调用SBoxIdea接口,通知保单箱开始新一轮(保单箱开始打印)
        SBoxIdea.BattleNewRound();
    }

    private void OnBattleNewRound(int result)
    {
        //一轮开始,若为打印模式则打印次数+1
        if (IOCanvasModel.Instance.CfgData.PrintMode != 0)
        {
            IOCanvasModel.Instance.PrintCount += 1;
        }
        MonoMgr.Instance.StartCoroutine(DelayNewRoundEnumerator());
    }

    private IEnumerator DelayNewRoundEnumerator()
    {
        bool isPrinterError = false;
        if (IOCanvasModel.Instance.CfgData.PrintMode != 0)
        {
            //显示"画面正在自检(正在打印)"的提示
            if (Model.Instance.errorCodeList.Count == 0)
                PopTips.Instance.ShowSystemTips("Printing", -1);
            yield return new WaitForSeconds(1);
            int ran = Utils.GetRandom(0, 8);
            //播放保单箱声音(干扰玩家听力音效)
            AudioManager.Instance.PlaySoundEff($"print{ran}");

            while (SBoxIdea.PrinterStateCustom() != 0)
            {
                if (SBoxIdea.PrinterStateCustom() != 1)//打印机未知错误 跳出等待
                {
                    isPrinterError = true;  
                    break;
                }
                yield return new WaitForSeconds(0.5f);
            }
        }
        Model.Instance.bigEyeRoadList.Clear();
        Model.Instance.smallRoadList.Clear();
        Model.Instance.dragonRoadList.Clear();//一轮开始先清掉
        Controller.Instance.RefreshOnNewRound();
        Controller.Instance.HideTips();//隐藏"正在打印"的提示
        Model.Instance.printerPrinting = false;//打印完成
        Model.Instance.haveClickPrint = false;//重置点击打单状态


        Debug.LogError("打单完成, 打单过程中是否出现异常:" + isPrinterError);
        //打印过程中无错直接走流程
        if (!isPrinterError)
            FSMSystem.Instance.PerformTransition(Transition.Prepare);
        else
        {
            //打印过程中有错,如果有错误码,走错误流程
            if (Model.Instance.errorCodeList.Count > 0)
                FSMSystem.Instance.PerformTransition(Transition.Error);
            else
                //如果此时没有错误码
                FSMSystem.Instance.PerformTransition(Transition.EndRound);
        }
    }
}
