using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SBoxApi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class IOCanvasView : MonoSingleton<IOCanvasView>
{
    private Coroutine pressCorutine;
    private bool pressFlag;
    private bool pressing;

    private void OnKeyDown(KeyCode keyCode)
    {
        IOArrowRect.gameObject.SetActive(false);
        switch (keyCode)
        {
            case KeyCode.W:
                SelectUp();
                break;
            case KeyCode.S:
                SelectDown();
                break;
            case KeyCode.A:
                SelectLeft();
                break;
            case KeyCode.D:
                SelectRight();
                break;
            case KeyCode.F:
                OnConfirm();
                break;
            case KeyCode.G:
                OnCancle();
                break;
        }
    }

    private void OnKeyUp(KeyCode keyCode)
    {
        IOArrowRect.gameObject.SetActive(false);
        switch (keyCode)
        {
            case KeyCode.W:
                KeyUp(true);
                break;
            case KeyCode.S:
                KeyUp(false);
                break;
            case KeyCode.A:
                KeyUp(false);
                break;
            case KeyCode.D:
                KeyUp(true);
                break;

        }
    }


    private void OnHardwareKeyDown(ulong keyCode)
    {
        IOArrowRect.gameObject.SetActive(false);
        switch (keyCode)
        {
            case SBOX_SWITCH.SWITCH_KEYBOARD_UP:
                SelectUp();
                break;
            case SBOX_SWITCH.SWITCH_KEYBOARD_DOWN:
                SelectDown();
                break;
            case SBOX_SWITCH.SWITCH_KEYBOARD_LEFT:
                SelectLeft();
                break;
            case SBOX_SWITCH.SWITCH_KEYBOARD_RIGHT:
                SelectRight();
                break;
        }
    }

    private void OnHardwareKeyUp(ulong keyCode)
    {
        IOArrowRect.gameObject.SetActive(false);
        switch (keyCode)
        {
            case SBOX_SWITCH.SWITCH_KEYBOARD_UP:
                KeyUp(true);
                break;
            case SBOX_SWITCH.SWITCH_KEYBOARD_DOWN:
                KeyUp(false);
                break;
            case SBOX_SWITCH.SWITCH_KEYBOARD_LEFT:
                KeyUp(false);
                break;
            case SBOX_SWITCH.SWITCH_KEYBOARD_RIGHT:
                KeyUp(true);
                break;
        }
    }

    private void OnHardwareClick(ulong keyCode)
    {
        IOArrowRect.gameObject.SetActive(false);
        switch (keyCode)
        {
            case SBOX_SWITCH.SWITCH_KEYBOARD_CONFIRM:
                OnConfirm();
                break;
            case SBOX_SWITCH.SWITCH_KEYBOARD_CANCLE:
                OnCancle();
                break;
        }
    }

    private void OnPasswordCurSelect(int curSelect)
    {
        if (selectionList.Count < 1)
            return;
        this.curSelect = curSelect;
        SetCurSelect(this.curSelect);
    }

    private void OnPasswordInput(int num)
    {
        string str = selectSection.baseSection.Content;
        str += num.ToString();
        selectSection.baseSection.Content = str;
    }

    private void OnPasswordDelete()
    {
        string str = selectSection.baseSection.Content;
        if (!string.IsNullOrEmpty(str))
        {
            str = str.Substring(0, str.Length - 1);
            selectSection.baseSection.Content = str;
        }
    }

    private void OnPasswordConfirm()
    {
        string str = selectSection.baseSection.Content;
        if (string.IsNullOrEmpty(str))
            selectSection.baseSection.Content = "0";

        var section = selectionList[curSelect] as IOBaseSection;
        if (section != null)
        {
            section.Selected = !section.Selected;
            selectSection.selected = section.Selected;
            selectSection.baseSection = section;
            if (!selectSection.selected)
                SetCurSelect(curSelect);
            passwordPanel.Focuse(false);
            switch (selectSection.state)
            {
                case IOSectionState.LineId:
                    break;
                case IOSectionState.MacId:
                    break;
                case IOSectionState.ClientWinLock:
                    break;
                case IOSectionState.EditPassword:
                    break;
                default:
                    break;
            }
        }

    }

    private void OnRefreshIOCanvas()
    {
        switch (State)
        {
            case IOState.BetSwitch:
            case IOState.Params:
                RefreshParamsPanel();
                break;
            case IOState.Bill:
            case IOState.Code:
                RefreshDiffcultyShow();
                break;
            case IOState.ProSetting:
                RefreshProSetting();
                break;
            case IOState.JackpotSetting:
                RefreshJackpotSetting();
                break;
            // case IOState.BetSwitch://新增加//刷新界面
            //     RefreshBetSwitch();
            //     break;
            default:
                break;
        }
    }

    private void RefreshParamsPanel()
    {
        for (int i = 0; i < selectionList.Count; i++)
        {
            var selection = selectionList[i];
            switch (i)
            {
                case (int)IOParams.GameGear:
                    (selection as IOBaseSection).CurIndex = IOCanvasModel.Instance.tempCfgData.GameGear;
                    break;
                case (int)IOParams.NewGameMode:
                    (selection as IOBaseSection).CurIndex = IOCanvasModel.Instance.tempCfgData.NewGameMode;
                    break;
                case (int)IOParams.CountDown:
                    (selection as IOBaseSection).CurIndex = IOCanvasModel.Instance.tempCfgData.CountDown;
                    break;
                case (int)IOParams.MinBet:
                    (selection as IOBaseSection).CurIndex = IOCanvasModel.Instance.tempCfgData.MinBet;
                    break;
                case (int)IOParams.MaxBet:
                    (selection as IOBaseSection).CurIndex = IOCanvasModel.Instance.tempCfgData.MaxBet;
                    break;
                case (int)IOParams.LimitBetsWins:
                    (selection as IOBaseSection).CurIndex = IOCanvasModel.Instance.tempCfgData.LimitBetsWins;
                    break;
                case (int)IOParams.CoinRatio:
                    (selection as IOBaseSection).CurIndex = IOCanvasModel.Instance.tempCfgData.CoinValue;
                    break;
                case (int)IOParams.TicketRatio:
                    (selection as IOTicketRatioSection).SetCurIndex();
                    break;
                case (int)IOParams.RefundMode:
                    (selection as IOBaseSection).CurIndex = (int)IOCanvasModel.Instance.tempCfgData.TicketMode;
                    break;
                //case (int)IOParams.SkillMode:
                //    (selection as IOSwitchSection).UpdateSwitch();
                //    break;
                //case (int)IOParams.ScoreUpRatio:
                //    (selection as IOBaseSection).CurIndex = IOCanvasModel.Instance.tempCfgData.ScoreUpUnit;
                //    break;
                case (int)IOParams.PrinterMode:
                    (selection as IOBaseSection).CurIndex = IOCanvasModel.Instance.tempCfgData.PrintMode;
                    break;
                //case (int)IOParams.RecordMode:
                //    (selection as IOBaseSection).CurIndex = IOCanvasModel.Instance.tempCfgData.ShowMode;
                //    break;
                case (int)IOParams.PrintTime:
                    (selection as IOBaseSection).CurIndex = IOCanvasModel.Instance.tempCfgData.CheckTime;
                    break;
                case (int)IOParams.PrintDensity:
                    (selection as IOBaseSection).CurIndex = IOCanvasModel.Instance.tempCfgData.PrintLevel;
                    break;
                case (int)IOParams.ClientWinLock:
                    (selection as IOBaseSection).CurIndex = IOCanvasModel.Instance.tempCfgData.PlayerWinLock;
                    break;
                case (int)IOParams.MachineWinLock:
                    (selection as IOBaseSection).CurIndex = IOCanvasModel.Instance.tempCfgData.LostLock;
                    break;
                case (int)IOParams.RoundWinLock:
                    (selection as IOBaseSection).CurIndex = IOCanvasModel.Instance.tempCfgData.LostLockCustom;
                    break;
                //case (int)IOParams.NetSwitch:
                //    (selection as IOBaseSection).CurIndex = IOCanvasModel.Instance.tempNetSwitch;
                //    break;
                case (int)IOParams.OffsetRatio:
                    (selection as IOBaseSection).CurIndex = IOCanvasModel.Instance.tempCfgData.PulseValue;
                    break;
                case (int)IOParams.BetSwitchCount:
                    (selection as IOBaseSection).CurIndex = IOCanvasModel.Instance.tempBetSwitchCount;
                    break;
                case (int)IOParams.CutOdds:
                    (selection as IOBaseSection).CurIndex = IOCanvasModel.Instance.tempCfgData.CutOdds;
                    break;
            }
        }
    }

    private void RefreshProSetting()
    {
        if (IOCanvasModel.Instance.CanEditMachineId)
        {
            if (selectSection.selected)
            {
                IOCanvasModel.Instance.SetTempCfgData(IOCanvasModel.Instance.CfgData);
                int data = selectSection.state == IOSectionState.EditLineId ? IOCanvasModel.Instance.tempCfgData.LineId : IOCanvasModel.Instance.tempCfgData.MachineId;
                selectSection.baseSection.Content = data.ToString();
                SetCurSelect(selectSection.state == IOSectionState.EditLineId ? 0 : 1);
                selectSection.selected = false;
            }
        }
        else
            ReturnToFunction();
    }

    // private void RefreshBetSwitch()
    // {
    //     for (int i = 0; i < selectionList.Count; i++)
    //     {
    //         var selection = selectionList[i];
    //         switch (i)
    //         {
    //             case (int)IOBetSwitch.BetSwitchConnect:
    //                 for (int index = 0; index < IOCanvasModel.Instance.tempBetSwitchList.Length; index++)
    //                     (selection as IOBaseSection).CurIndex = IOCanvasModel.Instance.tempBetSwitchList[index];
    //                 break;
    //             default:
    //                 break;
    //         }
    //     }
    // }

    private void RefreshJackpotSetting()
    {
        for (int i = 0; i < selectionList.Count; i++)
        {
            var selection = selectionList[i];
            switch (i)
            {
                case (int)IOJackpotSetting.JackpotSwitch:
                    (selection as IOBaseSection).CurIndex = IOCanvasModel.Instance.tempJackpotCfg.jackpotSwitch;
                    break;
                case (int)IOJackpotSetting.JpPercent:
                    (selection as IOBaseSection).CurIndex = IOCanvasModel.Instance.tempJackpotCfg.jpPercent;
                    break;
            }
        }
    }

    public void EditPassword()
    {
        switch (selectSection.state)
        {
            case IOSectionState.EditPassword:
                passwordPanel.SetPlaceholderText(Utils.GetLanguage("Enter new password"));
                selectSection.state = IOSectionState.NewPassword;
                break;
            default:
                break;
        }
    }

    private void SelectUp()
    {
        if (!selectSection.selected)
        {
            if (State == IOState.BetSwitch)//新增加,防止小键盘控制移动到其他界面去
            {
                int tempIndxe = selectionList.Count - 2 - IOCanvasModel.Instance.BetSwitchCount;
                int temp = curSelect - 1;
                //if (temp > selectionList.Count - 1) temp = selectionList.Count - 1;
                if (temp < tempIndxe) temp = tempIndxe;

                if (temp < selectionList.Count)
                    SetCurSelect(temp);
                else if (selectionList.Count > 0)
                    SetCurSelect();
            }
            else if (State == IOState.PrintSetting)//新增加,防止小键盘控制移动到其他界面去
            {
                int tempIndxe = selectionList.Count - 2 - IOCanvasModel.Instance.IOPrintSettingDate.Length;
                int temp = curSelect - 1;
                if (temp < tempIndxe) temp = tempIndxe;
                if (temp < selectionList.Count)
                    SetCurSelect(temp);
                else if (selectionList.Count > 0)
                    SetCurSelect();
            }
            else if (State != IOState.Code &&
                 State != IOState.EditPassword &&
                 State != IOState.ProSetting &&
                 State != IOState.AddressSetting)
            {
                int temp = curSelect - 1;
                if (temp > -1)
                    SetCurSelect(temp);
                else if (selectionList.Count > 0)
                {
                    temp = selectionList.Count - 1;
                    SetCurSelect(temp);
                }
            }
            else if (State == IOState.ProSetting)
            {
                int old = curSelect;
                int temp = curSelect - 1;
                if (old == selectionList.Count - 2)
                    SetCurSelect(1);
                else if (temp < 0)
                    SetCurSelect(selectionList.Count - 1);
                else
                    SetCurSelect(temp);
            }
            else
            {
                if (curSelect == 12)
                    SetCurSelect(curSelect - 2);
            }
        }
        else
        {
            int temp;
            switch (selectSection.state)
            {
                case IOSectionState.DateTime:
                    if (curSelect > selectionList.Count - 3)
                    {
                        temp = curSelect - 1;
                        SetCurSelect(temp);
                        if (temp == selectionList.Count - 3)
                            selectSection.baseSection.CurIndex = 4;
                    }
                    else
                    {
                        if (selectSection.baseSection.CurIndex - 1 > -1)
                            selectSection.baseSection.CurIndex--;
                    }
                    break;
                case IOSectionState.SkillMode:
                    selectSection.baseSection.CurIndex--;
                    break;
                case IOSectionState.EditLineId:
                case IOSectionState.EditMacId:
                    int minSelect = 1;
                    temp = curSelect - 3;
                    if (curSelect == selectionList.Count - 1)
                        SetCurSelect(selectionList.Count - 3, false);
                    else if (temp > minSelect)
                        SetCurSelect(temp, false);
                    break;
            }
        }
    }

    private void SelectDown()
    {
        if (!selectSection.selected)
        {
            if (State == IOState.BetSwitch)//新增加
            {
                int tempIndxe = selectionList.Count - 2 - IOCanvasModel.Instance.BetSwitchCount;
                int temp = curSelect + 1;
                if (temp > selectionList.Count - 1) temp = selectionList.Count - 1;
                //else if (temp < tempIndxe) temp = tempIndxe;

                if (temp < selectionList.Count)
                    SetCurSelect(temp);
                else if (selectionList.Count > 0)
                    SetCurSelect();
            }
            else if (State == IOState.PrintSetting)//新增加,防止小键盘控制移动到其他界面去
            {
                int tempIndxe = selectionList.Count - 2 - IOCanvasModel.Instance.IOPrintSettingDate.Length;
                int temp = curSelect + 1;
                if (temp > selectionList.Count - 1) temp = selectionList.Count - 1;
                if (temp < selectionList.Count)
                    SetCurSelect(temp);
                else if (selectionList.Count > 0)
                    SetCurSelect();
            }
            else if (State != IOState.Code &&
                 State != IOState.EditPassword &&
                 State != IOState.ProSetting &&
                 State != IOState.AddressSetting)
            {
                int temp = curSelect + 1;
                if (temp < selectionList.Count)
                    SetCurSelect(temp);
                else if (selectionList.Count > 0)
                    SetCurSelect();
            }
            else if (State == IOState.ProSetting)
            {
                int old = curSelect;
                int temp = curSelect + 1;
                if (old == 1)
                    SetCurSelect(selectionList.Count - 2);
                else if (temp > selectionList.Count - 1)
                    SetCurSelect();
                else
                    SetCurSelect(temp);
            }
            else
            {
                if (curSelect > 8 && curSelect < 12)
                    SetCurSelect(12);
            }
        }
        else
        {
            int temp;
            switch (selectSection.state)
            {
                case IOSectionState.DateTime:
                    if (selectSection.baseSection.CurIndex + 1 > 4)
                    {
                        temp = curSelect + 1;
                        if (temp < selectionList.Count)
                            SetCurSelect(temp);
                    }
                    else
                        selectSection.baseSection.CurIndex++;
                    break;
                case IOSectionState.SkillMode:
                    selectSection.baseSection.CurIndex++;
                    break;
                case IOSectionState.EditLineId:
                case IOSectionState.EditMacId:
                    temp = curSelect + 3;
                    if (temp > selectionList.Count - 1 || temp == selectionList.Count - 2)
                        SetCurSelect(selectionList.Count - 1, false);
                    else
                        SetCurSelect(temp, false);
                    break;
            }
        }
    }

    private void SelectLeft()
    {
        if (selectSection.selected)
            SelectedSectionDownAndLeft(true);
    }

    private void SelectRight()
    {
        if (selectSection.selected)
            SelectedSectionUpAndRight(true);
    }

    private void SelectedSectionUpAndRight(bool changeSkillValue)
    {
        int temp = 0;
        switch (selectSection.state)
        {
            case IOSectionState.GameGear:
                ValueChangeLoop(true, ref IOCanvasModel.Instance.tempCfgData.GameGear, Enum.GetValues(typeof(IOGameGear)).Cast<int>().Min(), Enum.GetValues(typeof(IOGameGear)).Cast<int>().Max());
                break;
            case IOSectionState.NewGameMode:
                ValueChangeLoop(true, ref IOCanvasModel.Instance.tempCfgData.NewGameMode, Enum.GetValues(typeof(IONewGameMode)).Cast<int>().Min(), Enum.GetValues(typeof(IONewGameMode)).Cast<int>().Max());
                break;
            case IOSectionState.CountDown:
                pressCorutine = StartCoroutine(PressEnumerator(
               () => { ValueChange(true, ref IOCanvasModel.Instance.tempCfgData.CountDown, IOCanvasModel.MIN_COUNT_DOWN, IOCanvasModel.MAX_COUNT_DOWN); }));
                break;
            case IOSectionState.MinBet:
                pressCorutine = StartCoroutine(PressEnumerator(
                (int offset) => { ValueChange(true, ref IOCanvasModel.Instance.tempCfgData.MinBet, IOCanvasModel.MIN_MIN_BET, IOCanvasModel.MAX_MIN_BET, offset); }, deffaultOffset: 10));
                break;
            case IOSectionState.MaxBet:
                pressCorutine = StartCoroutine(PressEnumerator(
                (int offset) => { ValueChange(true, ref IOCanvasModel.Instance.tempCfgData.MaxBet, IOCanvasModel.MIN_MAX_BET, IOCanvasModel.MAX_MAX_BET, offset); }, deffaultOffset: 10));
                break;
            case IOSectionState.LimitBetsWins:
                pressCorutine = StartCoroutine(PressEnumerator(
                (int offset) => { ValueChange(true, ref IOCanvasModel.Instance.tempCfgData.LimitBetsWins, IOCanvasModel.MIN_LIMIT_BETS_WINS, IOCanvasModel.MAX_LIMIT_BETS_WINS, offset); }, deffaultOffset: 10));
                break;
            case IOSectionState.CoinRatio:
                pressCorutine = StartCoroutine(PressEnumerator(
                () => { ValueChange(true, ref IOCanvasModel.Instance.tempCfgData.CoinValue, IOCanvasModel.MIN_COIN_RATIO, IOCanvasModel.MAX_COIN_RATIO); }));
                break;
            case IOSectionState.TicketRatio:
                pressCorutine = StartCoroutine(PressEnumerator(
                () =>
                {
                    if (IOCanvasModel.Instance.tempCfgData.TicketValue > 1)
                    {
                        IOCanvasModel.Instance.tempCfgData.TicketValue = IOCanvasModel.Instance.tempCfgData.TicketValue + 1 > IOCanvasModel.MAX_TICKET_VALUE ?
                        IOCanvasModel.Instance.tempCfgData.TicketValue : IOCanvasModel.Instance.tempCfgData.TicketValue + 1;
                    }
                    else
                    {
                        if (IOCanvasModel.Instance.tempCfgData.scoreTicket - 1 < 1)
                            IOCanvasModel.Instance.tempCfgData.TicketValue = 2;
                        else
                            IOCanvasModel.Instance.tempCfgData.scoreTicket--;
                    }
                    (selectSection.baseSection as IOTicketRatioSection).SetCurIndex();
                }));
                break;
            case IOSectionState.RefundMode:
                ValueChangeLoop(true, ref IOCanvasModel.Instance.tempCfgData.TicketMode, 0, 0); //Enum.GetValues(typeof(IORefundMode)).Cast<int>().Min(), Enum.GetValues(typeof(IORefundMode)).Cast<int>().Max());//枚举好像不够数量,无法使用
                break;
            case IOSectionState.SkillMode:
                if (changeSkillValue)
                    pressCorutine = StartCoroutine(PressEnumerator(
                    () =>
                    {
                        SkillValueChange(true,
                        IOCanvasModel.Instance.tempSwitchList,
                        selectSection.baseSection.CurIndex,
                        IOCanvasModel.Instance.SWITCH_LIMIT_MIN[selectSection.baseSection.CurIndex],
                        IOCanvasModel.Instance.SWITCH_LIMIT_MAX[selectSection.baseSection.CurIndex],
                        EventHandle.UPDATE_SWITCH);
                    }));
                else
                    selectSection.baseSection.CurIndex++;
                break;
            case IOSectionState.ScoreUpRatio:
                pressCorutine = StartCoroutine(PressEnumerator(
                () => { ValueChange(true, ref IOCanvasModel.Instance.tempCfgData.ScoreUpUnit, IOCanvasModel.MIN_SCORE_UP_RATIO, IOCanvasModel.MAX_SCORE_UP_RATIO, offset: 1000); }));
                break;
            case IOSectionState.PrinterMode:
                ValueChangeLoop(true, ref IOCanvasModel.Instance.tempCfgData.PrintMode, Enum.GetValues(typeof(IOPrinterMode)).Cast<int>().Min(), Enum.GetValues(typeof(IOPrinterMode)).Cast<int>().Max());
                break;
            case IOSectionState.RecordMode:
                ValueChangeLoop(true, ref IOCanvasModel.Instance.tempCfgData.PrintMode, Enum.GetValues(typeof(IORecordMode)).Cast<int>().Min(), Enum.GetValues(typeof(IORecordMode)).Cast<int>().Max());
                break;
            case IOSectionState.PrintTime:
                pressCorutine = StartCoroutine(PressEnumerator(
                () => { ValueChange(true, ref IOCanvasModel.Instance.tempCfgData.CheckTime, IOCanvasModel.MIN_PRINT_TIME, IOCanvasModel.MAX_PRINT_TIME); }));
                break;
            case IOSectionState.PrintDensity:
                pressCorutine = StartCoroutine(PressEnumerator(
                (int offset) => { ValueChangeLoop(true, ref IOCanvasModel.Instance.tempCfgData.PrintLevel, IOCanvasModel.MIN_PRINT_DENSITY, IOCanvasModel.MAX_PRINT_DENSITY, offset); }));
                break;
            case IOSectionState.ClientWinLock:
                pressCorutine = StartCoroutine(PressEnumerator(
                (int offset) => { ValueChange(true, ref IOCanvasModel.Instance.tempCfgData.PlayerWinLock, IOCanvasModel.MIN_CLIENT_WIN_LOCK, IOCanvasModel.MAX_CLIENT_WIN_LOCK, offset); }, deffaultOffset: 10));
                break;
            case IOSectionState.MachineWinLock:
                pressCorutine = StartCoroutine(PressEnumerator(
                (int offset) => { ValueChange(true, ref IOCanvasModel.Instance.tempCfgData.LostLock, IOCanvasModel.MIN_MACHINE_WIN_LOCK, IOCanvasModel.MAX_MACHINE_WIN_LOCK, offset); }, deffaultOffset: 10));
                break;
            case IOSectionState.RoundWinLock:
                pressCorutine = StartCoroutine(PressEnumerator(
                (int offset) => { ValueChange(true, ref IOCanvasModel.Instance.tempCfgData.LostLockCustom, IOCanvasModel.MIN_ROUND_WIN_LOCK, IOCanvasModel.MAX_ROUND_WIN_LOCK, offset); }, deffaultOffset: 10));
                break;
            case IOSectionState.NetSwitch:
                ValueChangeLoop(true, ref IOCanvasModel.Instance.tempNetSwitch, Enum.GetValues(typeof(IONetSwitch)).Cast<int>().Min(), Enum.GetValues(typeof(IONetSwitch)).Cast<int>().Max());
                break;
            case IOSectionState.OffsetRatio:
                pressCorutine = StartCoroutine(PressEnumerator(
                () => { ValueChange(true, ref IOCanvasModel.Instance.tempCfgData.PulseValue, IOCanvasModel.MIN_OFFSET_RATIO, IOCanvasModel.MAX_OFFSET_RATIO, 10); }));
                break;
            case IOSectionState.PrintOffset:
                pressCorutine = StartCoroutine(PressEnumerator(
                () =>
                {
                    int index = IOCanvasModel.Instance.curPrintOffsetIndex;
                    ValueChange(true, ref IOCanvasModel.Instance.tempIOPrintSettingDate[index], IOCanvasModel.MIN_OFFSET_PRINT, IOCanvasModel.MAX_OFFSET_PRINT);
                }));
                break;
            case IOSectionState.DateTime:
                pressCorutine = StartCoroutine(PressEnumerator(
                    () =>
                    {
                        DateTimeValueChange(true,
                        selectSection.baseSection.CurIndex,
                        EventHandle.UPDATE_DATE_TIME);
                    }));
                break;
            case IOSectionState.Language:
                temp = (int)IOCanvasModel.Instance.curlanguage;
                ValueChangeLoop(true, ref temp, Enum.GetValues(typeof(Language)).Cast<int>().Min(), Enum.GetValues(typeof(Language)).Cast<int>().Max());
                IOCanvasModel.Instance.curlanguage = (Language)(Enum.Parse(typeof(Language), temp.ToString()));
                RefreshFunctionText();
                break;
            case IOSectionState.EditLineId:
            case IOSectionState.EditMacId:
                temp = curSelect + 1;
                if (temp == selectionList.Count - 2)
                {
                    SetCurSelect(selectionList.Count - 1, false);
                }
                else if (temp < selectionList.Count)
                    SetCurSelect(temp, false);
                break;
            case IOSectionState.JackpotSwitch:
                ValueChangeLoop(true, ref IOCanvasModel.Instance.tempJackpotCfg.jackpotSwitch, Enum.GetValues(typeof(IOJackpotSwitch)).Cast<int>().Min(), Enum.GetValues(typeof(IOJackpotSwitch)).Cast<int>().Max());
                break;
            case IOSectionState.JackpotPercent:
                pressCorutine = StartCoroutine(PressEnumerator(
                () => { ValueChange(true, ref IOCanvasModel.Instance.tempJackpotCfg.jpPercent, IOCanvasModel.MIN_JACKPOT_PERCENT, IOCanvasModel.MAX_JACKPOT_PERCENT); }));
                break;
            case IOSectionState.BgmSwitch:
                ValueChangeLoop(true, ref IOCanvasModel.Instance.tempBgmSwitch, Enum.GetValues(typeof(IOBgmSwitch)).Cast<int>().Min(), Enum.GetValues(typeof(IOBgmSwitch)).Cast<int>().Max());
                break;
            case IOSectionState.BgmSelect:
                ValueChangeLoop(true, ref IOCanvasModel.Instance.tempBgmSelectIndex, Enum.GetValues(typeof(IOBgmSelect)).Cast<int>().Min(), Enum.GetValues(typeof(IOBgmSelect)).Cast<int>().Max());
                break;
            // case IOSectionState.SoundVolumScale:
            //     pressCorutine = StartCoroutine(PressEnumerator(
            //     () => { SoundVolumnValueChange(true); }));
            //     break;
            case IOSectionState.CutOdds:
                pressCorutine = StartCoroutine(PressEnumerator(
                () => { ValueChange(true, ref IOCanvasModel.Instance.tempCfgData.CutOdds, IOCanvasModel.MIN_CUTODDS, IOCanvasModel.MAX_CUTODDS,1); }));
                break;
            case IOSectionState.BetSwitchCount:

                pressCorutine = StartCoroutine(PressEnumerator(
               () =>
               {
                   ValueChange(true, ref IOCanvasModel.Instance.tempBetSwitchCount, IOCanvasModel.betSwitch_Limit_Min, IOCanvasModel.betSwitch_Limit_Max);
               }));
                break;
            case IOSectionState.BetSwitchConnect://rightAndUp,长按
                pressCorutine = StartCoroutine(PressEnumerator(
                (int offset) =>
                {
                    //Debug.Log("加,按到了");
                    int betIndex = IOCanvasModel.Instance.curBetSwitchIndex;//当前的下标
                    ValueChange(true, ref IOCanvasModel.Instance.tempBetSwitchList[betIndex], IOCanvasModel.switch_limit_min_zxd, IOCanvasModel.switch_limit_max_zxd, offset*5);
                    for (int i = betIndex + 1; i < IOCanvasModel.Instance.tempBetSwitchList.Length; i++)
                    {
                        if (IOCanvasModel.Instance.tempBetSwitchList[i] < IOCanvasModel.Instance.tempBetSwitchList[betIndex])
                        {
                            IOCanvasModel.Instance.tempBetSwitchList[i] = IOCanvasModel.Instance.tempBetSwitchList[betIndex];
                        }
                    }
                    UpdateBetSwitch();//更新所有betSwitch
                }));
                break;
            default:
                break;
        }
    }

    private void SelectedSectionDownAndLeft(bool vertical)
    {
        int temp = 0;
        switch (selectSection.state)
        {
            case IOSectionState.GameGear:
                ValueChangeLoop(false, ref IOCanvasModel.Instance.tempCfgData.GameGear, Enum.GetValues(typeof(IOGameGear)).Cast<int>().Min(), Enum.GetValues(typeof(IOGameGear)).Cast<int>().Max());
                break;
            case IOSectionState.NewGameMode:
                ValueChangeLoop(false, ref IOCanvasModel.Instance.tempCfgData.NewGameMode, Enum.GetValues(typeof(IONewGameMode)).Cast<int>().Min(), Enum.GetValues(typeof(IONewGameMode)).Cast<int>().Max());
                break;
            case IOSectionState.CountDown:
                pressCorutine = StartCoroutine(PressEnumerator(
                () => { ValueChange(false, ref IOCanvasModel.Instance.tempCfgData.CountDown, IOCanvasModel.MIN_COUNT_DOWN, IOCanvasModel.MAX_COUNT_DOWN); }));
                break;
            case IOSectionState.MinBet:
                pressCorutine = StartCoroutine(PressEnumerator(
                (int offset) => { ValueChange(false, ref IOCanvasModel.Instance.tempCfgData.MinBet, IOCanvasModel.MIN_MIN_BET, IOCanvasModel.MAX_MIN_BET, offset); }, deffaultOffset: 10));
                break;
            case IOSectionState.MaxBet:
                pressCorutine = StartCoroutine(PressEnumerator(
                (int offset) => { ValueChange(false, ref IOCanvasModel.Instance.tempCfgData.MaxBet, IOCanvasModel.MIN_MAX_BET, IOCanvasModel.MAX_MAX_BET, offset); }, deffaultOffset: 10));
                break;
            case IOSectionState.LimitBetsWins:
                pressCorutine = StartCoroutine(PressEnumerator(
                (int offset) => { ValueChange(false, ref IOCanvasModel.Instance.tempCfgData.LimitBetsWins, IOCanvasModel.MIN_LIMIT_BETS_WINS, IOCanvasModel.MAX_LIMIT_BETS_WINS, offset); }, 10));
                break;
            case IOSectionState.CoinRatio:
                pressCorutine = StartCoroutine(PressEnumerator(
                () => { ValueChange(false, ref IOCanvasModel.Instance.tempCfgData.CoinValue, IOCanvasModel.MIN_COIN_RATIO, IOCanvasModel.MAX_COIN_RATIO); }));
                break;
            case IOSectionState.TicketRatio:
                pressCorutine = StartCoroutine(PressEnumerator(
                () =>
                {
                    if (IOCanvasModel.Instance.tempCfgData.TicketValue > 1)
                        IOCanvasModel.Instance.tempCfgData.TicketValue--;
                    else
                    {
                        IOCanvasModel.Instance.tempCfgData.scoreTicket = IOCanvasModel.Instance.tempCfgData.scoreTicket + 1 > IOCanvasModel.MAX_VALUE_TICKET ?
                        IOCanvasModel.MAX_VALUE_TICKET : IOCanvasModel.Instance.tempCfgData.scoreTicket + 1;
                    }
                    (selectSection.baseSection as IOTicketRatioSection).SetCurIndex();
                }));
                break;
            case IOSectionState.RefundMode:
                ValueChangeLoop(false, ref IOCanvasModel.Instance.tempCfgData.TicketMode, 0, 0);// Enum.GetValues(typeof(IORefundMode)).Cast<int>().Min(), Enum.GetValues(typeof(IORefundMode)).Cast<int>().Max());//
                break;
            case IOSectionState.SkillMode:
                if (vertical)
                    pressCorutine = StartCoroutine(PressEnumerator(
                        () =>
                        {
                            SkillValueChange(false,
                            IOCanvasModel.Instance.tempSwitchList,
                            selectSection.baseSection.CurIndex,
                            IOCanvasModel.Instance.SWITCH_LIMIT_MIN[selectSection.baseSection.CurIndex],
                            IOCanvasModel.Instance.SWITCH_LIMIT_MAX[selectSection.baseSection.CurIndex],
                            EventHandle.UPDATE_SWITCH);
                        }));
                else
                    selectSection.baseSection.CurIndex--;
                break;
            case IOSectionState.ScoreUpRatio:
                pressCorutine = StartCoroutine(PressEnumerator(
                () => { ValueChange(false, ref IOCanvasModel.Instance.tempCfgData.ScoreUpUnit, IOCanvasModel.MIN_SCORE_UP_RATIO, IOCanvasModel.MAX_SCORE_UP_RATIO, offset: 1000); }));
                break;
            case IOSectionState.PrinterMode:
                ValueChangeLoop(false, ref IOCanvasModel.Instance.tempCfgData.PrintMode, Enum.GetValues(typeof(IOPrinterMode)).Cast<int>().Min(), Enum.GetValues(typeof(IOPrinterMode)).Cast<int>().Max());
                break;
            case IOSectionState.RecordMode:
                ValueChangeLoop(false, ref IOCanvasModel.Instance.tempCfgData.ShowMode, Enum.GetValues(typeof(IORecordMode)).Cast<int>().Min(), Enum.GetValues(typeof(IORecordMode)).Cast<int>().Max());
                break;
            case IOSectionState.PrintTime:
                pressCorutine = StartCoroutine(PressEnumerator(
                () => { ValueChange(false, ref IOCanvasModel.Instance.tempCfgData.CheckTime, IOCanvasModel.MIN_PRINT_TIME, IOCanvasModel.MAX_PRINT_TIME); }));
                break;
            case IOSectionState.PrintDensity:
                pressCorutine = StartCoroutine(PressEnumerator(
                (int offset) => { ValueChangeLoop(false, ref IOCanvasModel.Instance.tempCfgData.PrintLevel, IOCanvasModel.MIN_PRINT_DENSITY, IOCanvasModel.MAX_PRINT_DENSITY, offset); }));
                break;
            case IOSectionState.ClientWinLock:
                pressCorutine = StartCoroutine(PressEnumerator(
                (int offset) => { ValueChange(false, ref IOCanvasModel.Instance.tempCfgData.PlayerWinLock, IOCanvasModel.MIN_CLIENT_WIN_LOCK, IOCanvasModel.MAX_CLIENT_WIN_LOCK, offset); }, deffaultOffset: 10));
                break;
            case IOSectionState.MachineWinLock:
                pressCorutine = StartCoroutine(PressEnumerator(
                (int offset) => { ValueChange(false, ref IOCanvasModel.Instance.tempCfgData.LostLock, IOCanvasModel.MIN_MACHINE_WIN_LOCK, IOCanvasModel.MAX_MACHINE_WIN_LOCK, offset); }, deffaultOffset: 10));
                break;
            case IOSectionState.RoundWinLock:
                pressCorutine = StartCoroutine(PressEnumerator(
                (int offset) => { ValueChange(false, ref IOCanvasModel.Instance.tempCfgData.LostLockCustom, IOCanvasModel.MIN_ROUND_WIN_LOCK, IOCanvasModel.MAX_ROUND_WIN_LOCK, offset); }, deffaultOffset: 10));
                break;
            case IOSectionState.NetSwitch:
                ValueChangeLoop(false, ref IOCanvasModel.Instance.tempNetSwitch, Enum.GetValues(typeof(IONetSwitch)).Cast<int>().Min(), Enum.GetValues(typeof(IONetSwitch)).Cast<int>().Max());
                break;
            case IOSectionState.OffsetRatio:
                pressCorutine = StartCoroutine(PressEnumerator(
                () => { ValueChange(false, ref IOCanvasModel.Instance.tempCfgData.PulseValue, IOCanvasModel.MIN_OFFSET_RATIO, IOCanvasModel.MAX_OFFSET_RATIO, 10); }));
                break;
            case IOSectionState.PrintOffset:
                pressCorutine = StartCoroutine(PressEnumerator(
                () =>
                {
                    int index = IOCanvasModel.Instance.curPrintOffsetIndex;
                    ValueChange(false, ref IOCanvasModel.Instance.tempIOPrintSettingDate[index], IOCanvasModel.MIN_OFFSET_PRINT, IOCanvasModel.MAX_OFFSET_PRINT);
                }));
                break;
            case IOSectionState.DateTime:
                pressCorutine = StartCoroutine(PressEnumerator(
                () =>
                {
                    DateTimeValueChange(false,
                    selectSection.baseSection.CurIndex,
                    EventHandle.UPDATE_DATE_TIME);
                }));
                break;

            case IOSectionState.Language:
                temp = (int)IOCanvasModel.Instance.curlanguage;
                ValueChangeLoop(false, ref temp, Enum.GetValues(typeof(Language)).Cast<int>().Min(), Enum.GetValues(typeof(Language)).Cast<int>().Max());
                IOCanvasModel.Instance.curlanguage = (Language)(Enum.Parse(typeof(Language), temp.ToString()));
                RefreshFunctionText();
                break;
            case IOSectionState.EditLineId:
            case IOSectionState.EditMacId:
                temp = curSelect - 1;
                if (temp == selectionList.Count - 2)
                    SetCurSelect(selectionList.Count - 3, false);
                else if (temp > 1)
                    SetCurSelect(temp, false);
                break;
            case IOSectionState.JackpotSwitch:
                ValueChangeLoop(false, ref IOCanvasModel.Instance.tempJackpotCfg.jackpotSwitch, Enum.GetValues(typeof(IOJackpotSwitch)).Cast<int>().Min(), Enum.GetValues(typeof(IOJackpotSwitch)).Cast<int>().Max());
                break;
            case IOSectionState.JackpotPercent:
                pressCorutine = StartCoroutine(PressEnumerator(
                () => { ValueChange(false, ref IOCanvasModel.Instance.tempJackpotCfg.jpPercent, IOCanvasModel.MIN_JACKPOT_PERCENT, IOCanvasModel.MAX_JACKPOT_PERCENT); }));
                break;
            case IOSectionState.BgmSwitch:
                ValueChangeLoop(false, ref IOCanvasModel.Instance.tempBgmSwitch, Enum.GetValues(typeof(IOBgmSwitch)).Cast<int>().Min(), Enum.GetValues(typeof(IOBgmSwitch)).Cast<int>().Max());
                break;
            case IOSectionState.BgmSelect:
                ValueChangeLoop(false, ref IOCanvasModel.Instance.tempBgmSelectIndex, Enum.GetValues(typeof(IOBgmSelect)).Cast<int>().Min(), Enum.GetValues(typeof(IOBgmSelect)).Cast<int>().Max());
                break;
            // case IOSectionState.SoundVolumScale:
            //     pressCorutine = StartCoroutine(PressEnumerator(
            //     () => { SoundVolumnValueChange(false); }));
            //     break;
            case IOSectionState.CutOdds:
                pressCorutine = StartCoroutine(PressEnumerator(
                () => 
                {    
                    ValueChange(false, ref IOCanvasModel.Instance.tempCfgData.CutOdds, IOCanvasModel.MIN_CUTODDS, IOCanvasModel.MAX_CUTODDS,1);
                }));
                break;
            case IOSectionState.BetSwitchCount:
                pressCorutine = StartCoroutine(PressEnumerator(
               () =>
               {
                   ValueChange(false, ref IOCanvasModel.Instance.tempBetSwitchCount, IOCanvasModel.betSwitch_Limit_Min, IOCanvasModel.betSwitch_Limit_Max);
               }));
                break;
            case IOSectionState.BetSwitchConnect://leftAndDown,长按
                pressCorutine = StartCoroutine(PressEnumerator(
                    (int offset) =>
                    {
                        int betIndex = IOCanvasModel.Instance.curBetSwitchIndex;//当前的下标
                        //Debug.Log("减,按到了");
                        ValueChange(false, ref IOCanvasModel.Instance.tempBetSwitchList[betIndex], IOCanvasModel.switch_limit_min_zxd, IOCanvasModel.switch_limit_max_zxd, offset*5);
                        for (int i = betIndex - 1; i >= 0; i--)
                        {
                            if (IOCanvasModel.Instance.tempBetSwitchList[i] > IOCanvasModel.Instance.tempBetSwitchList[betIndex])
                            {
                                IOCanvasModel.Instance.tempBetSwitchList[i] = IOCanvasModel.Instance.tempBetSwitchList[betIndex];
                            }
                        }
                        UpdateBetSwitch();//更新所有betSwitch
                    }));
                break;
            default:
                break;
        }
    }

    private IEnumerator PressEnumerator(Action onUpdate)
    {
        pressFlag = false;
        pressing = true;
        float pressTime = 0;
        while (pressing)
        {
            pressTime += 0.02f;
            if (!pressFlag && pressTime > 0.2f)
                pressFlag = true;
            if (pressFlag)
                onUpdate?.Invoke();
            yield return new WaitForSeconds(0.02f);
        }
    }

    private IEnumerator PressEnumerator(Action<int> onUpdate, int deffaultOffset = 1)
    {
        pressFlag = false;
        pressing = true;
        float pressTime = 0;
        float step = 0.02f;
        int times = 0;
        int offset = 1;
        while (pressing)
        {
            pressTime += 0.02f;
            if (!pressFlag && pressTime > 0.2f)
                pressFlag = true;
            if (pressFlag)
            {
                step = 0.12f;
                times++;
                if (times < 21)
                {
                    step = 0.06f;
                    offset = deffaultOffset;
                }
                else if (times < 41)
                    offset = deffaultOffset * 10;
                else if (times < 81)
                    offset = deffaultOffset * 100;
                else
                    offset = deffaultOffset * 1000;
                onUpdate?.Invoke(offset);
            }
            //Debug.Log($"cur times:{times}, offset:{offset}");
            yield return new WaitForSeconds(step);
        }
    }

    private void KeyUp(bool isAdd)
    {
        if (!selectSection.selected || pressCorutine == null) return;
        if (!pressFlag)
            switch (selectSection.state)
            {
                case IOSectionState.GameGear:
                    ValueChangeLoop(isAdd, ref IOCanvasModel.Instance.tempCfgData.GameGear, Enum.GetValues(typeof(IOGameGear)).Cast<int>().Min(), Enum.GetValues(typeof(IOGameGear)).Cast<int>().Max());
                    break;
                case IOSectionState.NewGameMode:
                    ValueChangeLoop(isAdd, ref IOCanvasModel.Instance.tempCfgData.NewGameMode, Enum.GetValues(typeof(IONewGameMode)).Cast<int>().Min(), Enum.GetValues(typeof(IONewGameMode)).Cast<int>().Max());
                    break;
                case IOSectionState.CountDown:
                    ValueChange(isAdd, ref IOCanvasModel.Instance.tempCfgData.CountDown, IOCanvasModel.MIN_COUNT_DOWN, IOCanvasModel.MAX_COUNT_DOWN);
                    break;
                case IOSectionState.MinBet:
                    ValueChange(isAdd, ref IOCanvasModel.Instance.tempCfgData.MinBet, IOCanvasModel.MIN_MIN_BET, IOCanvasModel.MAX_MIN_BET, offset: 10);
                    break;
                case IOSectionState.MaxBet:
                    ValueChange(isAdd, ref IOCanvasModel.Instance.tempCfgData.MaxBet, IOCanvasModel.MIN_MAX_BET, IOCanvasModel.MAX_MAX_BET, offset: 10);
                    break;
                case IOSectionState.LimitBetsWins:
                    ValueChange(isAdd, ref IOCanvasModel.Instance.tempCfgData.LimitBetsWins, IOCanvasModel.MIN_LIMIT_BETS_WINS, IOCanvasModel.MAX_LIMIT_BETS_WINS, offset: 10);
                    break;
                case IOSectionState.CoinRatio:
                    ValueChange(isAdd, ref IOCanvasModel.Instance.tempCfgData.CoinValue, IOCanvasModel.MIN_COIN_RATIO, IOCanvasModel.MAX_COIN_RATIO);
                    break;
                case IOSectionState.TicketRatio:
                    if (isAdd)
                    {
                        if (IOCanvasModel.Instance.tempCfgData.TicketValue > 1)
                        {
                            if (IOCanvasModel.Instance.tempCfgData.TicketValue + 1 > IOCanvasModel.MAX_TICKET_VALUE)
                                IOCanvasModel.Instance.tempCfgData.TicketValue = IOCanvasModel.MAX_TICKET_VALUE;
                            else
                                IOCanvasModel.Instance.tempCfgData.TicketValue++;
                        }
                        else
                        {
                            if (IOCanvasModel.Instance.tempCfgData.scoreTicket - 1 < 1)
                                IOCanvasModel.Instance.tempCfgData.TicketValue = 2;
                            else
                                IOCanvasModel.Instance.tempCfgData.scoreTicket--;
                        }
                    }
                    else
                    {
                        if (IOCanvasModel.Instance.tempCfgData.TicketValue > 1)
                            IOCanvasModel.Instance.tempCfgData.TicketValue--;
                        else
                        {
                            IOCanvasModel.Instance.tempCfgData.scoreTicket = IOCanvasModel.Instance.tempCfgData.scoreTicket + 1 > IOCanvasModel.MAX_VALUE_TICKET ?
                            IOCanvasModel.MAX_VALUE_TICKET : IOCanvasModel.Instance.tempCfgData.scoreTicket + 1;
                        }
                    }

                    (selectSection.baseSection as IOTicketRatioSection).SetCurIndex();
                    break;
                case IOSectionState.SkillMode:
                    SkillValueChange(isAdd, IOCanvasModel.Instance.tempSwitchList, selectSection.baseSection.CurIndex, IOCanvasModel.Instance.SWITCH_LIMIT_MIN[selectSection.baseSection.CurIndex], IOCanvasModel.Instance.SWITCH_LIMIT_MAX[selectSection.baseSection.CurIndex], EventHandle.UPDATE_SWITCH);
                    break;
                case IOSectionState.ScoreUpRatio:
                    ValueChange(isAdd, ref IOCanvasModel.Instance.tempCfgData.ScoreUpUnit, IOCanvasModel.MIN_SCORE_UP_RATIO, IOCanvasModel.MAX_SCORE_UP_RATIO, offset: 1000);
                    break;
                case IOSectionState.RefundMode:
                    //ValueChangeLoop(isAdd, ref IOCanvasModel.Instance.tempCfgData.TicketMode, Enum.GetValues(typeof(IORefundMode)).Cast<int>().Min(), Enum.GetValues(typeof(IORefundMode)).Cast<int>().Max());

                    ValueChangeLoop(isAdd, ref IOCanvasModel.Instance.tempCfgData.TicketMode, 0, 0);
                    break;
                case IOSectionState.PrinterMode:
                    ValueChangeLoop(isAdd, ref IOCanvasModel.Instance.tempCfgData.PrintMode, Enum.GetValues(typeof(IOPrinterMode)).Cast<int>().Min(), Enum.GetValues(typeof(IOPrinterMode)).Cast<int>().Max());
                    break;
                case IOSectionState.RecordMode:
                    ValueChangeLoop(isAdd, ref IOCanvasModel.Instance.tempCfgData.ShowMode, Enum.GetValues(typeof(IORecordMode)).Cast<int>().Min(), Enum.GetValues(typeof(IORecordMode)).Cast<int>().Max());
                    break;
                case IOSectionState.PrintTime:
                    ValueChange(isAdd, ref IOCanvasModel.Instance.tempCfgData.CheckTime, IOCanvasModel.MIN_PRINT_TIME, IOCanvasModel.MAX_PRINT_TIME);
                    break;
                case IOSectionState.PrintDensity:
                    ValueChangeLoop(isAdd, ref IOCanvasModel.Instance.tempCfgData.PrintLevel, IOCanvasModel.MIN_PRINT_DENSITY, IOCanvasModel.MAX_PRINT_DENSITY);
                    break;
                case IOSectionState.ClientWinLock:
                    ValueChange(isAdd, ref IOCanvasModel.Instance.tempCfgData.PlayerWinLock, IOCanvasModel.MIN_CLIENT_WIN_LOCK, IOCanvasModel.MAX_CLIENT_WIN_LOCK, 10);
                    break;
                case IOSectionState.MachineWinLock:
                    ValueChange(isAdd, ref IOCanvasModel.Instance.tempCfgData.LostLock, IOCanvasModel.MIN_MACHINE_WIN_LOCK, IOCanvasModel.MAX_MACHINE_WIN_LOCK, 10);
                    break;
                case IOSectionState.RoundWinLock:
                    ValueChange(isAdd, ref IOCanvasModel.Instance.tempCfgData.LostLockCustom, IOCanvasModel.MIN_ROUND_WIN_LOCK, IOCanvasModel.MAX_ROUND_WIN_LOCK, 10);
                    break;
                case IOSectionState.NetSwitch:
                    ValueChangeLoop(isAdd, ref IOCanvasModel.Instance.tempCfgData.NewGameMode, Enum.GetValues(typeof(IONetSwitch)).Cast<int>().Min(), Enum.GetValues(typeof(IONetSwitch)).Cast<int>().Max());
                    break;
                case IOSectionState.OffsetRatio:
                    ValueChange(isAdd, ref IOCanvasModel.Instance.tempCfgData.PulseValue, IOCanvasModel.MIN_OFFSET_RATIO, IOCanvasModel.MAX_OFFSET_RATIO, 10);
                    break;
                case IOSectionState.PrintOffset://KeyUp,单点
                    int printIndex = IOCanvasModel.Instance.curPrintOffsetIndex;//当前的下标
                    ValueChange(isAdd, ref IOCanvasModel.Instance.tempIOPrintSettingDate[printIndex], IOCanvasModel.MIN_OFFSET_PRINT, IOCanvasModel.MAX_OFFSET_PRINT);
                    break;
                case IOSectionState.DateTime:
                    DateTimeValueChange(isAdd, selectSection.baseSection.CurIndex, EventHandle.UPDATE_DATE_TIME);
                    break;

                case IOSectionState.Language:
                    int temp = (int)IOCanvasModel.Instance.curlanguage;
                    ValueChangeLoop(isAdd, ref temp, Enum.GetValues(typeof(Language)).Cast<int>().Min(), Enum.GetValues(typeof(Language)).Cast<int>().Max());
                    IOCanvasModel.Instance.curlanguage = (Language)(Enum.Parse(typeof(Language), temp.ToString()));
                    RefreshFunctionText();
                    break;
                case IOSectionState.JackpotSwitch:
                    ValueChangeLoop(isAdd, ref IOCanvasModel.Instance.tempJackpotCfg.jackpotSwitch, Enum.GetValues(typeof(IOJackpotSwitch)).Cast<int>().Min(), Enum.GetValues(typeof(IOJackpotSwitch)).Cast<int>().Max());
                    break;
                case IOSectionState.JackpotPercent:
                    ValueChange(isAdd, ref IOCanvasModel.Instance.tempJackpotCfg.jpPercent, IOCanvasModel.MIN_JACKPOT_PERCENT, IOCanvasModel.MAX_JACKPOT_PERCENT);
                    break;
                case IOSectionState.BgmSwitch:
                    ValueChangeLoop(isAdd, ref IOCanvasModel.Instance.tempBgmSwitch, Enum.GetValues(typeof(IOBgmSwitch)).Cast<int>().Min(), Enum.GetValues(typeof(IOBgmSwitch)).Cast<int>().Max());
                    break;
                case IOSectionState.BgmSelect:
                    ValueChangeLoop(isAdd, ref IOCanvasModel.Instance.tempBgmSelectIndex, Enum.GetValues(typeof(IOBgmSelect)).Cast<int>().Min(), Enum.GetValues(typeof(IOBgmSelect)).Cast<int>().Max());
                    break;
                // case IOSectionState.SoundVolumScale:
                //     SoundVolumnValueChange(isAdd);
                //     break;
                case IOSectionState.CutOdds:
                    ValueChange(isAdd, ref IOCanvasModel.Instance.tempCfgData.CutOdds, IOCanvasModel.MIN_CUTODDS, IOCanvasModel.MAX_CUTODDS,1);
                    break;
                case IOSectionState.BetSwitchCount:
                    ValueChange(isAdd, ref IOCanvasModel.Instance.tempBetSwitchCount, IOCanvasModel.betSwitch_Limit_Min, IOCanvasModel.betSwitch_Limit_Max);
                    break;
                case IOSectionState.BetSwitchConnect://KeyUp,单点

                    int betIndex = IOCanvasModel.Instance.curBetSwitchIndex;//当前的下标

                    //调整一个,增加,下标后的小于增加的都增加,减少,下标前的大于减少的都减少
                    if (isAdd)
                    {
                        ValueChange(isAdd, ref IOCanvasModel.Instance.tempBetSwitchList[betIndex], IOCanvasModel.switch_limit_min_zxd, IOCanvasModel.switch_limit_max_zxd, offset:5);
                        for (int i = betIndex + 1; i < IOCanvasModel.Instance.tempBetSwitchList.Length; i++)
                        {
                            if (IOCanvasModel.Instance.tempBetSwitchList[i] < IOCanvasModel.Instance.tempBetSwitchList[betIndex])
                            {
                                IOCanvasModel.Instance.tempBetSwitchList[i] = IOCanvasModel.Instance.tempBetSwitchList[betIndex];
                            }
                        }
                    }
                    else
                    {
                        ValueChange(isAdd, ref IOCanvasModel.Instance.tempBetSwitchList[betIndex], IOCanvasModel.switch_limit_min_zxd, IOCanvasModel.switch_limit_max_zxd, offset:5);
                        for (int i = betIndex - 1; i >= 0; i--)
                        {
                            if (IOCanvasModel.Instance.tempBetSwitchList[i] > IOCanvasModel.Instance.tempBetSwitchList[betIndex])
                            {
                                IOCanvasModel.Instance.tempBetSwitchList[i] = IOCanvasModel.Instance.tempBetSwitchList[betIndex];
                            }
                        }
                    }
                    UpdateBetSwitch();//更新所有betSwitch
                    break;
            }
        pressing = false;
        pressFlag = false;
        StopCoroutine(pressCorutine);
        pressCorutine = null;
    }

    private void OnConfirm()
    {
        IOBaseSection section;
        switch (State)
        {
            case IOState.CheckPermissions:
                break;
            case IOState.SelectFunction:
                switch (curSelect)
                {
                    case (int)IOFunction.Params:
                        InitParamsPanel();
                        break;
                    case (int)IOFunction.Bill:
                        InitBillPanel();
                        break;
                    case (int)IOFunction.Code:
                        InitCodePanel();
                        break;
                    case (int)IOFunction.EditPassword:
                        InitEditPasswordPanel();
                        break;
                    case (int)IOFunction.Language:
                        ChangeCurLanguage();
                        break;
                    case (int)IOFunction.ModifiedDate:
                        InitModeifedDatePanel();
                        break;
                    case (int)IOFunction.JackpotSetting:
                        InitJackpotSettingPanel();
                        break;
                    case (int)IOFunction.JackpotBet:
                        InitJackpotBetPanel();
                        break;
                    case (int)IOFunction.JackpotWins:
                        InitJackpotWinPanel();
                        break;
                    case (int)IOFunction.BgmSetting:
                        InitBgmSettingPanel();
                        break;
                    case (int)IOFunction.ProSetting:
                        if (IOCanvasModel.Instance.CanEditMachineId)
                            InitProSettingPanel();
                        else
                            ExitIOCanvas();
                        break;
                    case (int)IOFunction.PrintSetting:
                        InitPrintSettingPanel();
                        break;
                    case (int)IOFunction.Exit:
                        ExitIOCanvas();
                        break;
                }
                break;
            case IOState.Params:
                section = selectionList[curSelect] as IOBaseSection;
                if (section != null)
                {
                    section.Selected = !section.Selected;
                    selectSection.selected = section.Selected;
                    selectSection.baseSection = section;
                    if (!selectSection.selected)
                        SetCurSelect(curSelect);
                }
                switch (curSelect)
                {
                    case (int)IOParams.GameGear:
                        selectSection.state = IOSectionState.GameGear;
                        break;
                    case (int)IOParams.NewGameMode:
                        selectSection.state = IOSectionState.NewGameMode;
                        break;
                    case (int)IOParams.CountDown:
                        selectSection.state = IOSectionState.CountDown;
                        break;
                    case (int)IOParams.MinBet:
                        selectSection.state = IOSectionState.MinBet;
                        break;
                    case (int)IOParams.MaxBet:
                        selectSection.state = IOSectionState.MaxBet;
                        break;
                    case (int)IOParams.LimitBetsWins:
                        selectSection.state = IOSectionState.LimitBetsWins;
                        break;
                    case (int)IOParams.CoinRatio:
                        selectSection.state = IOSectionState.CoinRatio;
                        break;
                    case (int)IOParams.TicketRatio:
                        selectSection.state = IOSectionState.TicketRatio;
                        break;
                    case (int)IOParams.RefundMode:
                        selectSection.state = IOSectionState.RefundMode;
                        break;
                    //case (int)IOParams.SkillMode:
                    //    selectSection.state = IOSectionState.SkillMode;
                    //    break;
                    //case (int)IOParams.ScoreUpRatio:
                    //    selectSection.state = IOSectionState.ScoreUpRatio;
                    //    break;
                    case (int)IOParams.PrinterMode:
                        selectSection.state = IOSectionState.PrinterMode;
                        break;
                    //case (int)IOParams.RecordMode:
                    //    selectSection.state = IOSectionState.RecordMode;
                    //    break;
                    case (int)IOParams.PrintTime:
                        selectSection.state = IOSectionState.PrintTime;
                        break;
                    case (int)IOParams.PrintDensity:
                        selectSection.state = IOSectionState.PrintDensity;
                        break;
                    case (int)IOParams.ClientWinLock:
                        selectSection.state = IOSectionState.ClientWinLock;
                        break;
                    case (int)IOParams.MachineWinLock:
                        selectSection.state = IOSectionState.MachineWinLock;
                        break;
                    case (int)IOParams.RoundWinLock:
                        selectSection.state = IOSectionState.RoundWinLock;
                        break;
                    //case (int)IOParams.NetSwitch:
                    //    selectSection.state = IOSectionState.NetSwitch;
                    //    break;
                    case (int)IOParams.OffsetRatio:
                        selectSection.state = IOSectionState.OffsetRatio;
                        break;
                    case (int)IOParams.BetSwitchCount:
                        selectSection.state = IOSectionState.BetSwitchCount;
                        break;
                    case (int)IOParams.BetSwitch://zxd 增加
                        InitBetSwitchPanel();
                        break;
                    case (int)IOParams.CutOdds:
                        selectSection.state = IOSectionState.CutOdds;
                        break;
                    case (int)IOParams.Save:
                        SaveConfig();
                        break;
                    case (int)IOParams.Return:
                        IOCanvasModel.Instance.SetTempCfgData(IOCanvasModel.Instance.CfgData);
                        ReturnToFunction();
                        break;
                    case (int)IOParams.OpenBox:
                        OpenBox();
                        break;
                }
                break;
            case IOState.Bill:
                switch (curSelect)
                {
                    case (int)IOBill.Clients:
                        InitClientBillPanelNo1();
                        break;
                    case (int)IOBill.Return:
                        ReturnToFunction();
                        break;
                }
                break;
            case IOState.Clients:
                switch (curSelect)
                {
                    case 0:
                        InitClientBillPanelNo1(true);
                        break;
                    case 1:
                        InitClientBillPanelNo2(true);
                        break;
                    case 2:
                        ReturnToFunction();
                        break;
                }
                break;
            case IOState.Code:
                switch (curSelect)
                {
                    case 12:
                        ReturnToFunction();
                        break;
                }
                break;
            case IOState.EditPassword:
                switch (curSelect)
                {
                    case 12:
                        ReturnToFunction();
                        break;
                }
                break;
            case IOState.DateTime:
                if (curSelect == selectionList.Count - 2)
                    SaveDateTime();
                else if (curSelect == selectionList.Count - 1)
                    CloseModifiedDatePanel(true);
                break;
            case IOState.BetSwitch:
                section = selectionList[curSelect] as IOBaseSection;
                if (section != null)
                {
                    section.Selected = !section.Selected;
                    selectSection.selected = section.Selected;
                    selectSection.baseSection = section;
                    if (!selectSection.selected)
                        SetCurSelect(curSelect);
                }

                if (curSelect == selectionList.Count - 2)
                    SaveBetSwitch();
                else if (curSelect == selectionList.Count - 1)
                    CloseBetSwitchPanel();
                else
                    selectSection.state = IOSectionState.BetSwitchConnect;

                break;
            case IOState.PrintSetting:
                section = selectionList[curSelect] as IOBaseSection;
                if (section != null)
                {
                    section.Selected = !section.Selected;
                    selectSection.selected = section.Selected;
                    selectSection.baseSection = section;
                    if (!selectSection.selected)
                        SetCurSelect(curSelect);
                }
                if (curSelect == selectionList.Count - 2)
                    SaveDatePrintSetting();
                else if (curSelect == selectionList.Count - 1)
                    ClosePrintSettingPanel();
                else
                {
                    selectSection.state = IOSectionState.PrintOffset;
                    IOCanvasModel.Instance.curPrintOffsetIndex = selectionList.Count - 2 - 1 == curSelect ? 1 : 0;
                }
                break;
            case IOState.BillSetting://紙鈔機設置
                if (curSelect == selectionList.Count - 1)//最后一个为 Return
                    ReturnToFunction();
                else
                    OnBillBtnClick(curSelect);
                break;
            case IOState.PrinterSetting://打印機設置
                if (curSelect == selectionList.Count - 2)//倒数第二个为 Return
                    ReturnToFunction();
                else if (curSelect == selectionList.Count - 1)//最后一个为 地址设置
                    OpenAddressSettingPanel();
                else
                    OnPrinterBtnClick(curSelect);
                break;
            case IOState.JackpotSetting:
                section = selectionList[curSelect] as IOBaseSection;
                if (section != null)
                {
                    section.Selected = !section.Selected;
                    selectSection.selected = section.Selected;
                    selectSection.baseSection = section;
                    if (!selectSection.selected)
                        SetCurSelect(curSelect);
                }
                switch (curSelect)
                {
                    case (int)IOJackpotSetting.JackpotSwitch:
                        selectSection.state = IOSectionState.JackpotSwitch;
                        break;
                    case (int)IOJackpotSetting.JpPercent:
                        selectSection.state = IOSectionState.JackpotPercent;
                        break;
                    case (int)IOJackpotSetting.Save:
                        SaveJackpotCfg();
                        break;
                    case (int)IOJackpotSetting.Return:
                        ReturnToFunction();
                        break;
                }
                break;
            case IOState.BgmSetting:
                section = selectionList[curSelect] as IOBaseSection;
                if (section != null)
                {
                    section.Selected = !section.Selected;
                    selectSection.selected = section.Selected;
                    selectSection.baseSection = section;
                    if (!selectSection.selected)
                        SetCurSelect(curSelect);
                }
                switch (curSelect)
                {
                    case (int)IOBgmSetting.BgmSwitch:
                        selectSection.state = IOSectionState.BgmSwitch;
                        break;
                    case (int)IOBgmSetting.BgmSelect:
                        selectSection.state = IOSectionState.BgmSelect;
                        break;
                    case (int)IOBgmSetting.Save:
                        SaveBgmSetting();
                        break;
                    case (int)IOBgmSetting.Return:
                        ReturnToFunction();
                        break;
                }
                break;
            case IOState.ProSetting:
                int data;
                string str;
                switch (curSelect)
                {
                    case 0:
                    case 1:
                        section = selectionList[curSelect] as IOBaseSection;
                        section.Selected = true;
                        selectSection.selected = true;
                        selectSection.baseSection = section;
                        selectSection.state = curSelect == 0 ? IOSectionState.EditLineId : IOSectionState.EditMacId;
                        SetCurSelect(2, false);
                        break;
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                    case 10:
                        data = selectSection.state == IOSectionState.EditLineId ? IOCanvasModel.Instance.tempCfgData.LineId : IOCanvasModel.Instance.tempCfgData.MachineId;
                        str = data != 0 ? data.ToString() + (curSelect - 1).ToString() : (curSelect - 1).ToString();
                        if (str.Length > 9) break;
                        selectSection.baseSection.Content = str;
                        if (selectSection.state == IOSectionState.EditLineId)
                            IOCanvasModel.Instance.tempCfgData.LineId = int.Parse(str);
                        else
                            IOCanvasModel.Instance.tempCfgData.MachineId = int.Parse(str);
                        break;
                    case 11:
                        data = selectSection.state == IOSectionState.EditLineId ? IOCanvasModel.Instance.tempCfgData.LineId : IOCanvasModel.Instance.tempCfgData.MachineId;
                        str = data.ToString().Substring(0, data.ToString().Length - 1);
                        selectSection.baseSection.Content = str;
                        if (selectSection.state == IOSectionState.EditLineId)
                            IOCanvasModel.Instance.tempCfgData.LineId = string.IsNullOrEmpty(str) ? 0 : int.Parse(str);
                        else
                            IOCanvasModel.Instance.tempCfgData.MachineId = string.IsNullOrEmpty(str) ? 0 : int.Parse(str);
                        break;
                    case 12:
                        data = selectSection.state == IOSectionState.EditLineId ? IOCanvasModel.Instance.tempCfgData.LineId : IOCanvasModel.Instance.tempCfgData.MachineId;
                        str = data.ToString() + "0";
                        if (str.Length > 9) break;
                        selectSection.baseSection.Content = str;
                        if (selectSection.state == IOSectionState.EditLineId)
                            IOCanvasModel.Instance.tempCfgData.LineId = int.Parse(str);
                        else
                            IOCanvasModel.Instance.tempCfgData.MachineId = int.Parse(str);
                        break;
                    case 13:
                    case 15:
                        if (selectSection.selected)
                        {
                            IOCanvasModel.Instance.SetTempCfgData(IOCanvasModel.Instance.CfgData);
                            data = selectSection.state == IOSectionState.EditLineId ? IOCanvasModel.Instance.tempCfgData.LineId : IOCanvasModel.Instance.tempCfgData.MachineId;
                            selectSection.baseSection.Content = data.ToString();
                            selectSection.selected = false;
                            SetCurSelect(selectSection.state == IOSectionState.EditLineId ? 0 : 1);
                        }
                        else
                            ReturnToFunction();
                        break;
                    case 14:
                        SaveConfig();
                        break;
                }
                break;
            default:
                break;
        }
    }

    private void OnCancle()
    {
        if (State == IOState.Params)
        {
            var section = selectionList[curSelect] as IOBaseSection;
            if (section != null)
            {
                section.Selected = !section.Selected;
                selectSection.selected = section.Selected;
                selectSection.baseSection = section;
                if (!selectSection.selected)
                    SetCurSelect(curSelect);
            }
        }
        else if (State == IOState.Code
            || State == IOState.EditPassword)
        {
            ReturnToFunction();
        }
        else if (State == IOState.AddressSetting)
        {
            //地址设置覆盖面板:取消(不保存)关闭
            CloseAddressSettingPanel();
        }
    }

    private void SaveConfig()
    {
        IOCanvasModel.Instance.tempCfgData.SwitchBetsUnitMin = IOCanvasModel.Instance.tempSwitchList[0];
        IOCanvasModel.Instance.tempCfgData.SwitchBetsUnitMid = IOCanvasModel.Instance.tempSwitchList[1];
        IOCanvasModel.Instance.tempCfgData.SwitchBetsUnitMax = IOCanvasModel.Instance.tempSwitchList[2];
        //oldSoundVolumScale = IOCanvasModel.Instance.SoundVolumScale;
        IOCanvasModel.Instance.BetSwitchCount = IOCanvasModel.Instance.tempBetSwitchCount;
        //补默认倍率值(未配置位) + 未显示位清零,发送给算法卡的押分数组只保留前 BetSwitchCount 个有效值
        IOCanvasModel.Instance.ApplyBetSwitchCount(IOCanvasModel.Instance.tempBetSwitchCount);
        IOCanvasModel.Instance.tempCfgData.BetSwitch = IOCanvasModel.Instance.BetSwitchList;
        SBoxIdea.WriteConf(IOCanvasModel.Instance.tempCfgData);
    }

    private void OpenBox()
    {
        Debug.LogError("OpenBox");
        SBoxIdea.BattPrinterOpenBox(true, false);
        //换纸操作,重置打印次数统计
        IOCanvasModel.Instance.PrintCount = 0;
    }

    private void SaveJackpotCfg()
    {
        PlayerPrefs.SetInt("JackpotSwitch", IOCanvasModel.Instance.tempJackpotCfg.jackpotSwitch);
        PlayerPrefs.SetInt("JackpotBetPercent", IOCanvasModel.Instance.tempJackpotCfg.betPercent);
        PlayerPrefs.SetInt("JackpotPercent", IOCanvasModel.Instance.tempJackpotCfg.jpPercent);
        var jackpotCfg = new JackpotConfig
        {
            jackpotSwitch = PlayerPrefs.GetInt("JackpotSwitch", 0),
            betPercent = PlayerPrefs.GetInt("JackpotBetPercent", 100),
            jpPercent = PlayerPrefs.GetInt("JackpotPercent", 5)
        };
        IOCanvasModel.Instance.jackpotCfg = jackpotCfg;
        
        oldJackpotConfig.jackpotSwitch = IOCanvasModel.Instance.jackpotCfg.jackpotSwitch;
        oldJackpotConfig.jpPercent = IOCanvasModel.Instance.jackpotCfg.jpPercent;
        oldJackpotConfig.betPercent = IOCanvasModel.Instance.jackpotCfg.betPercent;

        Debug.LogError($"SaveJackpotCfgoldJackpotConfig:{JsonConvert.SerializeObject(oldJackpotConfig)}");
        //if (!ClientWS.Instance.IsConnected && IOCanvasModel.Instance.jackpotCfg.jackpotSwitch == (int)IOJackpotSwitch.JackpotSwitchOn)
        //    NetMgr.Instance.SetNetAutoConnect(false);
        //else if (ClientWS.Instance.IsConnected && IOCanvasModel.Instance.jackpotCfg.jackpotSwitch == (int)IOJackpotSwitch.JackpotSwitchOff)
        //    ClientWS.Instance.CloseSocket();
        if (NetMgr.Instance.CurNetStatus == NET_STATUS.NET_STATUS_DISCONNECTED && IOCanvasModel.Instance.jackpotCfg.jackpotSwitch == (int)IOJackpotSwitch.JackpotSwitchOn)
            NetMgr.Instance.SetNetAutoConnect(false);//后台设置,开关打开,重新连接大厅彩金主机
        else if (NetMgr.Instance.CurNetStatus == NET_STATUS.NET_STATUS_CONNECTED && IOCanvasModel.Instance.jackpotCfg.jackpotSwitch == (int)IOJackpotSwitch.JackpotSwitchOff)
            NetMgr.Instance.CloseSocket();//后台设置,开关关闭,关闭连接

        //开关(状态输入)已变化,直接同步联网彩金状态到分机,不依赖UI刷新
        NetMgr.Instance.SyncNetJackpotState();

        OnRefreshIOCanvas();
        EventCenter.Instance.EventTrigger(EventHandle.CHANGE_NETJP_ICON_ACTIVE);
        IOPopTips.Instance.ShowTips(Utils.GetLanguage("SaveSucceed"));
    }

    private void SaveBgmSetting()//背景音乐设置的保存按钮
    {
        IOCanvasModel.Instance.BgmSwitch = IOCanvasModel.Instance.tempBgmSwitch;
        IOCanvasModel.Instance.BgmSelectIndex = IOCanvasModel.Instance.tempBgmSelectIndex;
        //立即生效:关,停止当前背景音乐;开,播放选中的背景音乐
        if (IOCanvasModel.Instance.BgmSwitch == (int)IOBgmSwitch.BgmSwitchOff)
            AudioManager.Instance.StopMusic();
        else
        {
            int bgmIndex = IOCanvasModel.Instance.BgmSelectIndex;
            if (bgmIndex == (int)IOBgmSelect.Random)
                bgmIndex = Utils.GetRandom(0, 3);
            AudioManager.Instance.PlayMusic($"normalBgm{bgmIndex}");
        }
        IOPopTips.Instance.ShowTips(Utils.GetLanguage("SaveSucceed"));
        //广播背景音乐设置到全部分机,参照 S2C_SoundVolumn
        MsgInfo msgInfo = new MsgInfo()
        {
            cmd = (int)S2C_CMD.S2C_BgmSetting,
            id = -1,
            jsonData = $"{IOCanvasModel.Instance.BgmSwitch}|{IOCanvasModel.Instance.BgmSelectIndex}",
        };
        NetMgr.Instance.SendToAllClient(JsonConvert.SerializeObject(msgInfo));
    }
    //private void BetSwitchValueChange(bool add, int value, int min, int max, int offset = 1)
    //{
    //    value = add ?
    //                value + offset > max ?
    //                max : value + offset
    //                : value - offset < min ?
    //                min : value - offset;
    //    if (offset == 10 && value % 10 != 0)
    //        value = value / 10 * 10 < min ? min : value / 10 * 10;
    //    selectSection.baseSection.CurIndex = value;
    //}
    private void SkillValueChange(bool add, List<int> list, int index, int min, int max, string eventName)
    {
        int offset;
        switch (index)
        {
            case 0:
                offset = 10;
                break;
            case 1:
                offset = 50;
                break;
            case 2:
                offset = 100;
                break;
            default:
                offset = 10;
                break;
        }
        list[index] = add ?
        list[index] + offset > max ?
        max : list[index] + offset
        : list[index] - offset < min ?
        min : list[index] - offset;
        list[index] = list[index] == 11 ? 10 : list[index];
        EventCenter.Instance.EventTrigger(eventName);
    }

    private void DateTimeValueChange(bool add, int index, string eventName)
    {
        var dateTime = IOCanvasModel.Instance.IODateTime;
        int value = 0;
        int min = 1;
        int max = 9999;
        switch (index)
        {
            case 0:
                value = dateTime.Year;
                break;
            case 1:
                value = dateTime.Month;
                max = 12;
                break;
            case 2:
                value = dateTime.Day;
                max = DateTime.DaysInMonth(dateTime.Year, dateTime.Month);
                break;
            case 3:
                value = dateTime.Hour;
                max = 23;
                min = 0;
                break;
            case 4:
                value = dateTime.Minute;
                max = 59;
                min = 0;
                break;
        }

        value = add ? value + 1 > max ? max : value + 1 : value - 1 < min ? min : value - 1;
        switch (index)
        {
            case 0:
                IOCanvasModel.Instance.IODateTime = new DateTime(value, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0);
                break;
            case 1:
                IOCanvasModel.Instance.IODateTime = new DateTime(dateTime.Year, value, dateTime.Day, dateTime.Hour, dateTime.Minute, 0);
                break;
            case 2:
                IOCanvasModel.Instance.IODateTime = new DateTime(dateTime.Year, dateTime.Month, value, dateTime.Hour, dateTime.Minute, 0);
                break;
            case 3:
                IOCanvasModel.Instance.IODateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, value, dateTime.Minute, 0);
                break;
            case 4:
                IOCanvasModel.Instance.IODateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, value, 0);
                break;
        }
        EventCenter.Instance.EventTrigger(eventName);
    }

    private void ValueChangeLoop(bool add, List<int> list, int index, int min, int max, string eventName, int offset = 1)
    {
        list[index] = add ?
        list[index] + offset > max ?
        min : list[index] + offset
        : list[index] - offset < min ?
        max : list[index] - offset;
        EventCenter.Instance.EventTrigger(eventName);
    }

    private void SoundVolumnValueChange(bool add, int offset = 1)
    {
        if (IOCanvasModel.Instance.SoundVolumScale == -1) return;
        IOCanvasModel.Instance.SoundVolumScale = add ?
        IOCanvasModel.Instance.SoundVolumScale + offset > IOCanvasModel.MAX_SOUND_VOLUM_SCALE ?
                IOCanvasModel.MAX_SOUND_VOLUM_SCALE : IOCanvasModel.Instance.SoundVolumScale + offset
                : IOCanvasModel.Instance.SoundVolumScale - offset < IOCanvasModel.MIN_SOUND_VOLUM_SCALE ?
                IOCanvasModel.MIN_SOUND_VOLUM_SCALE : IOCanvasModel.Instance.SoundVolumScale - offset;
        selectSection.baseSection.CurIndex = IOCanvasModel.Instance.SoundVolumScale;

        MsgInfo msgInfo = new MsgInfo()
        {
            cmd = (int)S2C_CMD.S2C_SoundVolumn,
            id = -1,
            jsonData = IOCanvasModel.Instance.SoundVolumScale.ToString(),
        };
        NetMgr.Instance.SendToAllClient(JsonConvert.SerializeObject(msgInfo));
    }

    private void ValueChange(bool add, ref int value, int min, int max, int offset = 1)
    {
        if (value == -1) return;
        if (selectSection.state == IOSectionState.OffsetRatio && value == 1)
        {
            value = add ?
               value + 9 > max ?
               max : value + 9
               : value - offset < min ?
               min : value - offset;
        }
        else if (selectSection.state == IOSectionState.ScoreUpRatio && value == 100)
        {
            value = add ?
               value + 400 > max ?
               max : value + 400
               : value - offset < min ?
               min : value - offset;
        }
        else if (selectSection.state == IOSectionState.ScoreUpRatio && value == 500)
        {
            value = add ?
               value + 500 > max ?
               max : value + 500
               : value - 400 < min ?
               min : value - 400;
        }
        else if (selectSection.state == IOSectionState.ScoreUpRatio && value == 1000)
        {
            value = add ?
               value + offset > max ?
               max : value + offset
               : value - 500 < min ?
               min : value - 500;
        }
        else if (selectSection.state == IOSectionState.MaxBet)
        {
            if (add)
                value = value + offset > max ? max : value + offset;
            else
            {
                if (value == IOCanvasModel.MAX_MAX_BET)
                    value -= 9;
                else
                    value = value - offset < min ? min : value - offset;
            }
        }
        else
        {
            value = add ?
               (value + offset > max ?
                max : value + offset)
                : (value - offset < min ?
                min : value - offset);
            if (offset == 10 && value % 10 != 0)
                value = value / 10 * 10 < min ? min : value / 10 * 10;
        }

        selectSection.baseSection.CurIndex = value;//更新显示的文本的地方
    }

    private void ValueChangeLoop(bool add, ref int value, int min, int max, int offset = 1)
    {
        if (value == -1) return;
        value = add ?
        value + offset > max ?
        min : value + offset
        : value - offset < min ?
        max : value - offset;
        selectSection.baseSection.CurIndex = value;
    }

    private void UpdateBetSwitch()
    {
        //foreach(IOBaseSection item in betSwitchControllerList)
        for (int i = 0; i < betSwitchControllerList.Count; i++)
        {
            IOBaseSection item = betSwitchControllerList[i];
            item.Content = IOCanvasModel.Instance.tempBetSwitchList[i].ToString();
            //或者
            item.CurIndex = IOCanvasModel.Instance.tempBetSwitchList[i];
        }
    }

    private void OnArrowUpPointerDown()
    {
        arrowUpImg.color = new Color(200 / 255f, 200 / 255f, 200 / 255f, 1f);
        SelectedSectionUpAndRight(true);
    }

    private void OnArrowUpPointerUp()
    {
        arrowUpImg.color = Color.white;
        KeyUp(true);
    }

    private void OnArrowDownPointerDown()
    {
        arrowDownImg.color = new Color(200 / 255f, 200 / 255f, 200 / 255f, 1f);
        SelectedSectionDownAndLeft(true);

    }

    private void OnArrowDownPointerUp()
    {
        arrowDownImg.color = Color.white;
        KeyUp(false);
    }
}
