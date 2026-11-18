using GameUtil;
using IOT;
using SBoxApi;
using System;
using System.Collections;
using UnityEngine;

//硬件控制器:监听并控制 投币,出币,硬件按钮
internal class SandboxController : BaseManager<SandboxController>
{
    private DelayTimer coinOutTimer;
    private DelayTimer nextStartTimer;
    private DelayTimer coinInTimer;
    private int curBillCredit;
    //凤凰打印机的 PrinterState() 编号(板卡打印机编号,从0开始);-1/-2 为未指定/未连接,不会命中
    private const int PhoenixPrinterState = 1;

    public void Init()
    {
        AddEventListener();
        AddHardwareBtnEvent();
        InitLightState();
    }

    private void AddEventListener()
    {
        EventCenter.Instance.AddEventListener<CoinInData>(SBoxSanboxEventHandle.COIN_IN, OnCoinIn);
        EventCenter.Instance.AddEventListener<int>(SBoxSanboxEventHandle.COIN_OUT, OnCoinOut);
        EventCenter.Instance.AddEventListener<ulong>(EventHandle.HARDWARE_KEY_DOWN, OnHardwareKeyDown);
        EventCenter.Instance.AddEventListener<ulong>(EventHandle.HARDWARE_KEY_UP, OnHardwareKeyUp);
        EventCenter.Instance.AddEventListener(EventHandle.PLAYER_COIN_IN, OnPlayerCoinIn);
        EventCenter.Instance.AddEventListener<int>(SBoxSanboxEventHandle.BILL_IN, OnBillIn);
    }

    private void OnBillIn(int credit)
    {
        if (Model.Instance.winLockPlayers.Contains(SBoxModel.Instance.macId))//玩家是否爆机，爆机就Reject
        {
            SBoxSandbox.BillReject();
            return;
        }
        curBillCredit = credit;
        EventCenter.Instance.AddEventListener(SBoxSanboxEventHandle.BILL_STACKED, OnBillStacked);
        SBoxSandbox.BillApprove();
    }

    private void OnBillStacked()
    {
        EventCenter.Instance.RemoveEventListener(SBoxSanboxEventHandle.BILL_STACKED, OnBillStacked);
        //todo 执行上分逻辑
        SBoxIdea.SetPlayerCoinInNumber(SBoxModel.Instance.macId,curBillCredit);
    }

    private void OnCoinIn(CoinInData coinData)
    {
        PlayerMgr.Instance.localPlayer.sBoxCoinInCount = Utils.SBoxCoinCount(PlayerMgr.Instance.localPlayer.sBoxCoinInCount, coinData.value);
        OnPlayerCoinIn();
    }

    private void OnCoinOut(int count)
    {
        IOCanvasModel.Instance.curCoinOutCount += count;
        IOCanvasModel.Instance.totalCoinOutCont -= count;
        PlayerMgr.Instance.localPlayer.sBoxCoinOutCount = Utils.SBoxCoinCount(PlayerMgr.Instance.localPlayer.sBoxCoinOutCount, count);
        if (IOCanvasModel.Instance.totalCoinOutCont > 0)
            CoinOutTimer();
    }

    public bool CheckCoinOutIsEnough()
    {
        float value = (float)IOCanvasModel.Instance.CfgData.TicketValue / IOCanvasModel.Instance.CfgData.scoreTicket * IOCanvasModel.Instance.CfgData.PulseValue;
        if (IOCanvasModel.Instance.CfgData.TicketValue == 0 || value == 0)
        {
            PopTips.Instance.ShowSystemTips( "PayOut setting error!");
            return false;
        }
        int coinOutCount = (int)(PlayerMgr.Instance.localPlayer.outState.credit / value);
        if (coinOutCount <= 0)
        {
            PopTips.Instance.ShowSystemTips( "Credit not enough!");
            return false;
        }
        return true;
    }

    //退分"能不能下分"预检:退分退的是币,按投币比例CoinValue计算可退币数(口径与OnRefundRequest一致)
    public bool CheckRefundIsEnough()
    {
        float value = (float)IOCanvasModel.Instance.CfgData.CoinValue;
        if (value <= 0)
        {
            PopTips.Instance.ShowSystemTips( "PayOut setting error!");
            return false;
        }
        int coinOutCount = (int)(PlayerMgr.Instance.localPlayer.outState.credit / value);
        if (coinOutCount <= 0)
        {
            PopTips.Instance.ShowSystemTips( "Credit not enough!");
            return false;
        }
        return true;
    }

    public void StartCoinOut()
    {
        NextStartTimer();
        //float value = (float)IOCanvasModel.Instance.CfgData.TicketValue / IOCanvasModel.Instance.CfgData.scoreTicket;
        float value = (float)IOCanvasModel.Instance.CfgData.TicketValue / IOCanvasModel.Instance.CfgData.scoreTicket * IOCanvasModel.Instance.CfgData.PulseValue;//退票计算脉冲比例
        if (IOCanvasModel.Instance.CfgData.TicketValue <= 0 || value <= 0)
        {
            PopTips.Instance.ShowSystemTips( "PayOut setting error!");
            return;
        }
        int coinOutCount = (int)(PlayerMgr.Instance.localPlayer.outState.credit / value);
        IOCanvasModel.Instance.curCoinOutCount = 0;
        if (coinOutCount == 0)
        {
            PopTips.Instance.ShowSystemTips( "Credit not enough!");
            return;
        }
        IOCanvasModel.Instance.totalCoinOutCont = coinOutCount;
        IOCanvasModel.Instance.coinOuting = true;
        SBoxSandbox.CoinOutStart(0, coinOutCount, 0);
        CoinOutTimer();
    }

    private void NextStartTimer()
    {
        IOCanvasModel.Instance.isInCoinOutCoolDown = true;
        if (nextStartTimer == null)
        {
            nextStartTimer = Timer.DelayAction(5, () =>
            {
                IOCanvasModel.Instance.isInCoinOutCoolDown = false;
            });
        }
        else
            nextStartTimer.Restart();
    }

    private void CoinOutTimer()
    {
        if (coinOutTimer == null)
            coinOutTimer = Timer.DelayAction(5, () => {
                if (IOCanvasModel.Instance.totalCoinOutCont > 0)
                    PopTips.Instance.ShowTips(Utils.GetLanguage("Payment timeout!"));
                StopCoinOut();
                IOCanvasModel.Instance.coinOuting = false;
            });
        else
            coinOutTimer.Restart();
    }

    private void OnPlayerCoinIn()
    {
        IOCanvasModel.Instance.coinIning = true;
        if (coinInTimer == null)
            coinInTimer = Timer.DelayAction(2, () => {
                IOCanvasModel.Instance.coinIning = false;
            });
        else
            coinInTimer.Restart();
    }

    private void StopCoinOut()
    {
        SBoxSandbox.CoinOutStop(0);
    }

    private void OnHardwareKeyDown(ulong key)
    {
        // 轮盘模式下 A~E 等投注键不再写入算法卡九门押分。
        if (Model.Instance.rouletteFlow &&
            (key == SBOX_SWITCH.SWITCH_A || key == SBOX_SWITCH.SWITCH_B ||
             key == SBOX_SWITCH.SWITCH_C || key == SBOX_SWITCH.SWITCH_D ||
             key == SBOX_SWITCH.SWITCH_E)) return;
        switch (key)
        {
            case SBOX_SWITCH.SWITCH_A:
                PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_A, true);
                break;
            case SBOX_SWITCH.SWITCH_B:
                PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_B, true);
                break;
            case SBOX_SWITCH.SWITCH_C:
                PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_C, true);
                break;
            case SBOX_SWITCH.SWITCH_D:
                PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_D, true);
                break;
            case SBOX_SWITCH.SWITCH_E:
                PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_E, true);
                break;
            case SBOX_SWITCH.SWITCH_DOWN:
                PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_ESC, true);
                break;
            case SBOX_SWITCH.SWITCH_SCORE_UP:
                PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_SCORE_UP, true);
                break;
            case SBOX_SWITCH.SWITCH_SCORE_DOWN:
            if(Model.Instance.banckerData != null && Model.Instance.banckerData.BankerPlayerId != SBoxModel.Instance.macId)//庄家不是自己
                PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_SCORE_DOWN, true);
                break;
            case SBOX_SWITCH.SWITCH_ROOT_SET:
                PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_ROOT, true);
                break;
            case SBOX_SWITCH.SWITCH_PAYOUT: break;
            case SBOX_SWITCH.SWITCH_UP:
                PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_SWITCH, true);
                break;
        }
    }

    private void OnHardwareKeyUp(ulong key)
    {
        switch (key)
        {
            case SBOX_SWITCH.SWITCH_A:
                PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_A, false);
                break;
            case SBOX_SWITCH.SWITCH_B:
                PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_B, false);
                break;
            case SBOX_SWITCH.SWITCH_C:
                PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_C, false);
                break;
            case SBOX_SWITCH.SWITCH_D:
                PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_D, false);
                break;
            case SBOX_SWITCH.SWITCH_E:
                PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_E, false);
                break;
            case SBOX_SWITCH.SWITCH_DOWN:
                PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_ESC, false);
                break;
            case SBOX_SWITCH.SWITCH_SCORE_UP:
                PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_SCORE_UP, false);
                break;
            case SBOX_SWITCH.SWITCH_SCORE_DOWN:
                PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_SCORE_DOWN, false);
                break;
            case SBOX_SWITCH.SWITCH_ROOT_SET:
                PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_ROOT, false);
                break;
            case SBOX_SWITCH.SWITCH_PAYOUT: break;
            case SBOX_SWITCH.SWITCH_UP:
                PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_SWITCH, false);
                break;
        }
    }

    private void AddHardwareBtnEvent()
    {
        SBoxSandboxListener.Instance.AddButtonDown(SBOX_SWITCH.SWITCH_A, () =>
            PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_A, true));
        SBoxSandboxListener.Instance.AddButtonUp(SBOX_SWITCH.SWITCH_A, () =>
            PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_A, false));

        SBoxSandboxListener.Instance.AddButtonDown(SBOX_SWITCH.SWITCH_B, () =>
            PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_B, true));
        SBoxSandboxListener.Instance.AddButtonUp(SBOX_SWITCH.SWITCH_B, () =>
            PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_B, false));

        SBoxSandboxListener.Instance.AddButtonDown(SBOX_SWITCH.SWITCH_C, () =>
            PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_C, true));
        SBoxSandboxListener.Instance.AddButtonUp(SBOX_SWITCH.SWITCH_C, () =>
            PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_C, false));

        SBoxSandboxListener.Instance.AddButtonDown(SBOX_SWITCH.SWITCH_D, () =>
            PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_D, true));
        SBoxSandboxListener.Instance.AddButtonUp(SBOX_SWITCH.SWITCH_D, () =>
            PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_D, false));

        SBoxSandboxListener.Instance.AddButtonDown(SBOX_SWITCH.SWITCH_E, () =>
            PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_E, true));
        SBoxSandboxListener.Instance.AddButtonUp(SBOX_SWITCH.SWITCH_E, () =>
            PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_E, false));

        SBoxSandboxListener.Instance.AddButtonDown(SBOX_SWITCH.SWITCH_DOWN, () =>
            PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_ESC, true));
        SBoxSandboxListener.Instance.AddButtonUp(SBOX_SWITCH.SWITCH_DOWN, () =>
            PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_ESC, false));

        SBoxSandboxListener.Instance.AddButtonDown(SBOX_SWITCH.SWITCH_SCORE_UP, () =>
            PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_SCORE_UP, true));
        SBoxSandboxListener.Instance.AddButtonUp(SBOX_SWITCH.SWITCH_SCORE_UP, () =>
            PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_SCORE_UP, false));

        SBoxSandboxListener.Instance.AddButtonDown(SBOX_SWITCH.SWITCH_SCORE_DOWN, () =>
        {
            if(Model.Instance.banckerData != null && Model.Instance.banckerData.BankerPlayerId != SBoxModel.Instance.macId)//庄家不是自己
                PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_SCORE_DOWN, true);
        });
        SBoxSandboxListener.Instance.AddButtonUp(SBOX_SWITCH.SWITCH_SCORE_DOWN, () =>
            PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_SCORE_DOWN, false));

        SBoxSandboxListener.Instance.AddButtonDown(SBOX_SWITCH.SWITCH_ROOT_SET, () =>
            PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_ROOT, true));
        SBoxSandboxListener.Instance.AddButtonUp(SBOX_SWITCH.SWITCH_ROOT_SET, () =>
            PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_ROOT, false));

        SBoxSandboxListener.Instance.AddButtonDown(SBOX_SWITCH.SWITCH_PAYOUT, () =>
        {
            if (IOTModel.Instance.LinkIOT) return;
        });
        SBoxSandboxListener.Instance.AddButtonUp(SBOX_SWITCH.SWITCH_PAYOUT, () =>
        {
            if (IOTModel.Instance.LinkIOT) return;
        });

        SBoxSandboxListener.Instance.AddButtonDown(SBOX_SWITCH.SWITCH_UP, () =>
            PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_SWITCH, true));
        SBoxSandboxListener.Instance.AddButtonUp(SBOX_SWITCH.SWITCH_UP, () =>
            PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_SWITCH, false));

        //抢庄:实体ENTER键,与虚拟抢庄键共用同一个状态位(两按钮状态或运算合成)
        SBoxSandboxListener.Instance.AddButtonDown(SBOX_SWITCH.SWITCH_ENTER, () =>
            PlayerMgr.Instance.localPlayer.SetBankerKeyState(Player.BankerKeySource.Hardware, true));
        SBoxSandboxListener.Instance.AddButtonUp(SBOX_SWITCH.SWITCH_ENTER, () =>
            PlayerMgr.Instance.localPlayer.SetBankerKeyState(Player.BankerKeySource.Hardware, false));

        // SBoxSandboxListener.Instance.AddButtonClick(SBOX_SWITCH.SWITCH_ENTER, () =>
        //     EventCenter.Instance.EventTrigger(EventHandle.HARDWARE_KEY_CLICK, SBOX_SWITCH.SWITCH_ENTER));

        SBoxSandboxListener.Instance.AddButtonClick(SBOX_SWITCH.SWITCH_PAYOUT, () =>
            EventCenter.Instance.EventTrigger(EventHandle.HARDWARE_KEY_CLICK, SBOX_SWITCH.SWITCH_PAYOUT));

        SBoxSandboxListener.Instance.AddButtonClick(SBOX_SWITCH.SWITCH_LEFT, () =>
            EventCenter.Instance.EventTrigger(EventHandle.HARDWARE_KEY_CLICK, SBOX_SWITCH.SWITCH_LEFT));

        SBoxSandboxListener.Instance.AddButtonDown(SBOX_SWITCH.SWITCH_KEYBOARD_UP, () =>
        {
            if (FSMSystem.Instance.CurrentStateID == StateID.Setting)
                EventCenter.Instance.EventTrigger(EventHandle.HARDWARE_KEY_DOWN, SBOX_SWITCH.SWITCH_KEYBOARD_UP);
        });
        SBoxSandboxListener.Instance.AddButtonUp(SBOX_SWITCH.SWITCH_KEYBOARD_UP, () =>
        {
            if (FSMSystem.Instance.CurrentStateID == StateID.Setting)
                EventCenter.Instance.EventTrigger(EventHandle.HARDWARE_KEY_UP, SBOX_SWITCH.SWITCH_KEYBOARD_UP);
        });

        SBoxSandboxListener.Instance.AddButtonDown(SBOX_SWITCH.SWITCH_KEYBOARD_DOWN, () =>
        {
            if (FSMSystem.Instance.CurrentStateID == StateID.Setting)
                EventCenter.Instance.EventTrigger(EventHandle.HARDWARE_KEY_DOWN, SBOX_SWITCH.SWITCH_KEYBOARD_DOWN);
        });
        SBoxSandboxListener.Instance.AddButtonUp(SBOX_SWITCH.SWITCH_KEYBOARD_DOWN, () =>
        {
            if (FSMSystem.Instance.CurrentStateID == StateID.Setting)
                EventCenter.Instance.EventTrigger(EventHandle.HARDWARE_KEY_UP, SBOX_SWITCH.SWITCH_KEYBOARD_DOWN);
        });

        SBoxSandboxListener.Instance.AddButtonDown(SBOX_SWITCH.SWITCH_KEYBOARD_LEFT, () =>
        {
            if (FSMSystem.Instance.CurrentStateID == StateID.Setting)
                EventCenter.Instance.EventTrigger(EventHandle.HARDWARE_KEY_DOWN, SBOX_SWITCH.SWITCH_KEYBOARD_LEFT);
        });
        SBoxSandboxListener.Instance.AddButtonUp(SBOX_SWITCH.SWITCH_KEYBOARD_LEFT, () =>
        {
            if (FSMSystem.Instance.CurrentStateID == StateID.Setting)
                EventCenter.Instance.EventTrigger(EventHandle.HARDWARE_KEY_UP, SBOX_SWITCH.SWITCH_KEYBOARD_LEFT);
        });

        SBoxSandboxListener.Instance.AddButtonDown(SBOX_SWITCH.SWITCH_KEYBOARD_RIGHT, () =>
        {
            if (FSMSystem.Instance.CurrentStateID == StateID.Setting)
                EventCenter.Instance.EventTrigger(EventHandle.HARDWARE_KEY_DOWN, SBOX_SWITCH.SWITCH_KEYBOARD_RIGHT);
        });
        SBoxSandboxListener.Instance.AddButtonUp(SBOX_SWITCH.SWITCH_KEYBOARD_RIGHT, () =>
        {
            if (FSMSystem.Instance.CurrentStateID == StateID.Setting)
                EventCenter.Instance.EventTrigger(EventHandle.HARDWARE_KEY_UP, SBOX_SWITCH.SWITCH_KEYBOARD_RIGHT);
        });

        SBoxSandboxListener.Instance.AddButtonClick(SBOX_SWITCH.SWITCH_KEYBOARD_CONFIRM, () =>
        {
            if (FSMSystem.Instance.CurrentStateID == StateID.Setting)
                EventCenter.Instance.EventTrigger(EventHandle.HARDWARE_KEY_CLICK, SBOX_SWITCH.SWITCH_KEYBOARD_CONFIRM);
            else if (Model.Instance.checking && IOCanvasModel.Instance.CfgData.PrintMode != 0)
                SBoxIdea.BattPrinterOpenBox(false, true);
        });
        //手动打印
        SBoxSandboxListener.Instance.AddButtonClick(SBOX_SWITCH.SWITCH_RULE, () =>
        EventCenter.Instance.EventTrigger(EventHandle.HARDWARE_KEY_CLICK, SBOX_SWITCH.SWITCH_RULE));
        //结束本轮
        SBoxSandboxListener.Instance.AddButtonClick(SBOX_SWITCH.SWITCH_ROOT_SET, () =>
        {
            // 轮盘模式不参与算法卡本轮协议：结算键写入的错误码 0（结束本轮）
            // 会经 RemoveErrorCode -> ACTIVE 推进 FSM，必须屏蔽。
            if (Model.Instance.rouletteFlow) return;
            if (Model.Instance.checking && IOCanvasModel.Instance.CfgData.PrintMode != 0)
            {
                SBoxIdea.BattPrinterOpenBox(false, true);
                return;
            }
            if (FSMSystem.Instance.CurrentStateID != StateID.Setting
            && FSMSystem.Instance.CurrentStateID != StateID.Error
            && FSMSystem.Instance.CurrentStateID != StateID.Print
            && FSMSystem.Instance.CurrentStateID != StateID.EndRound
            && !Model.Instance.wait4Setting)
                Model.Instance.AddErrorCode(0, true);
        });

        SBoxSandboxListener.Instance.AddButtonClick(SBOX_SWITCH.SWITCH_KEYBOARD_LITTLE_GAME, () =>
        EventCenter.Instance.EventTrigger(EventHandle.HARDWARE_KEY_CLICK, SBOX_SWITCH.SWITCH_KEYBOARD_LITTLE_GAME));
        SBoxSandboxListener.Instance.AddButtonLongPress(SBOX_SWITCH.SWITCH_KEYBOARD_LITTLE_GAME, () =>
        EventCenter.Instance.EventTrigger(EventHandle.HARDWARE_KEY_LONG_PRESS, SBOX_SWITCH.SWITCH_KEYBOARD_LITTLE_GAME));

        SBoxSandboxListener.Instance.AddButtonClick(SBOX_SWITCH.SWITCH_SET, () =>
        EventCenter.Instance.EventTrigger(EventHandle.HARDWARE_KEY_CLICK, SBOX_SWITCH.SWITCH_SET));
        SBoxSandboxListener.Instance.AddButtonLongPress(SBOX_SWITCH.SWITCH_SET, () =>
        EventCenter.Instance.EventTrigger(EventHandle.HARDWARE_KEY_LONG_PRESS, SBOX_SWITCH.SWITCH_SET));
    }

    private void InitLightState()
    {
        TurnOnAllLight();
        TurnOnEnterLight();//开机抢庄键(实体ENTER)灯默认亮
        TurnOffBetLight();
    }

    public void TurnOnAllLight()
    {
        Type t = typeof(SBOX_SWITCH);
        var fields = t.GetFields();
        foreach (var fieldInfo in fields)
            SBoxSandbox.SwitchOutStateOn((ulong)fieldInfo.GetRawConstantValue());
    }

    public void TurnOffAllLight()
    {
        Type t = typeof(SBOX_SWITCH);
        var fields = t.GetFields();
        foreach (var fieldInfo in fields)
            SBoxSandbox.SwitchOutStateOff((ulong)fieldInfo.GetRawConstantValue());
    }

    public void TurnOnBetLight()
    {
        TurnOnTheLight(SBOX_SWITCH.SWITCH_A);
        TurnOnTheLight(SBOX_SWITCH.SWITCH_B);
        TurnOnTheLight(SBOX_SWITCH.SWITCH_C);
        TurnOnTheLight(SBOX_SWITCH.SWITCH_D);
        TurnOnTheLight(SBOX_SWITCH.SWITCH_E);
    }

    public void TurnOffBetLight()
    {
        TurnOffTheLight(SBOX_SWITCH.SWITCH_A);
        TurnOffTheLight(SBOX_SWITCH.SWITCH_B);
        TurnOffTheLight(SBOX_SWITCH.SWITCH_C);
        TurnOffTheLight(SBOX_SWITCH.SWITCH_D);
        TurnOffTheLight(SBOX_SWITCH.SWITCH_E);
    }

    //抢庄键(实体ENTER)灯
    public void TurnOnEnterLight()
    {
        TurnOnTheLight(SBOX_SWITCH.SWITCH_ENTER);
    }

    public void TurnOffEnterLight()
    {
        TurnOffTheLight(SBOX_SWITCH.SWITCH_ENTER);
    }

    public void TurnOnTreasureLight()
    {
        TurnOnTheLight(SBOX_SWITCH.SWITCH_A);
        TurnOnTheLight(SBOX_SWITCH.SWITCH_B);
        TurnOnTheLight(SBOX_SWITCH.SWITCH_C);
    }

    public void TurnOffTreasureLight()
    {
        TurnOffTheLight(SBOX_SWITCH.SWITCH_A);
        TurnOffTheLight(SBOX_SWITCH.SWITCH_B);
        TurnOffTheLight(SBOX_SWITCH.SWITCH_C);
    }

    public void LightFlashing(int result, bool isWin)
    {
        Model.Instance.flashing = true;
        var sboxSwitch = result switch
        {
            1 => SBOX_SWITCH.SWITCH_A,
            2 => SBOX_SWITCH.SWITCH_B,
            3 => SBOX_SWITCH.SWITCH_C,
            4 => SBOX_SWITCH.SWITCH_D,
            5 => SBOX_SWITCH.SWITCH_E,
            _ => SBOX_SWITCH.SWITCH_A,
        };
        MonoMgr.Instance.StartCoroutine(LightFlashingCoroutine(sboxSwitch));
        if (isWin)
            MonoMgr.Instance.StartCoroutine(LightFlashingCoroutine(SBOX_SWITCH.SWITCH_ESC));
    }

    private IEnumerator LightFlashingCoroutine(ulong sboxSwitch)
    {
        int offset = 1;
        while (Model.Instance.flashing)
        {
            if (offset == 1)
                TurnOffTheLight(sboxSwitch);
            else
                TurnOnTheLight(sboxSwitch);
            offset *= -1;
            yield return new WaitForSeconds(0.25f);
        }
    }

    public void TurnOnTheLight(ulong sboxSwitch)
    {
        SBoxSandbox.SwitchOutStateOn(sboxSwitch);
    }

    private void TurnOffTheLight(ulong sboxSwitch)
    {
        SBoxSandbox.SwitchOutStateOff(sboxSwitch);
    }

    //退分小票:凤凰打印机(PrinterState编号==PhoenixPrinterState)逐段控制字号(标题/金额最大且居中,时间小,订单号/地址中);其余打印机仅\n换行文本,无法居中
    public void PrintRefundReceipt(string orderId, string address, string time, int coinCount)
    {
        int idx = SBoxSandbox.PrinterState();
        if (idx == PhoenixPrinterState)
        {
            PhoenixPrinterManager.Instance.PrintRefundTicket(orderId, address, time, coinCount);
        }
        else
        {
            string text = "PAYOUT TICKET" + "\r\n" +
                "$" + coinCount + "\r\n" +
                NumberToEnglish.Convert(coinCount) + "\r\n" +
                time + "\r\n" +
                "Order No.:" + orderId + "\r\n" +
                "Address:" + address + "\r\n";
            SBoxSandbox.PrinterMessage(text);
        }
    }
}