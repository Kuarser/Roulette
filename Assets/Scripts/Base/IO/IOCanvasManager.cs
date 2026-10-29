using Newtonsoft.Json;
using SBoxApi;
using System;
using System.Collections.Generic;
using UnityEngine;

public class IOCanvasManager : BaseManager<IOCanvasManager>
{
    public IOCanvasView view;
    public void Init(IOCanvasView view)
    {
        this.view = view;
        IOCanvasModel.Instance.horizontal = true;
        AddEventListener();
    }

    private void AddEventListener()
    {
        EventCenter.Instance.AddEventListener<ulong>(EventHandle.CONFIRM_PASSWORD, OnConfirmPassword);
        EventCenter.Instance.AddEventListener<SBoxPermissionsData>(SBoxEventHandle.SBOX_CHECK_PASSWORD, OnCheckPassword);
        EventCenter.Instance.AddEventListener<SBoxPermissionsData>(SBoxEventHandle.SBOX_CHANGE_PASSWORD, OnChangePassword);
        EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_SANDBOX_SET_DATETIME, OnSetDate);
        EventCenter.Instance.AddEventListener<SBoxPermissionsData>(SBoxEventHandle.SBOX_WRITE_CONF, OnWriteConf);
        EventCenter.Instance.AddEventListener<SBoxConfData>(SBoxEventHandle.SBOX_READ_CONF, OnReadConfig);
        EventCenter.Instance.AddEventListener<SBoxCoderData>(SBoxEventHandle.SBOX_REQUEST_CODER, OnRequestCoder);
        EventCenter.Instance.AddEventListener<SBoxPermissionsData>(SBoxEventHandle.SBOX_CODER, OnSBoxCode);
        EventCenter.Instance.AddEventListener<DateTime>(SBoxEventHandle.SBOX_SANDBOX_GET_DATETIME, OnSandboxGetDateTime);
        EventCenter.Instance.AddEventListener<int[]>(SBoxEventHandle.SBOX_BATTLE_PRINTER_OFFSET_GET, OnSandboxGetPrintSetting);
        EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_BATTLE_PRINTER_OFFSET_SET, OnSandboxSetPrintSetting);
        EventCenter.Instance.AddEventListener<List<string>>(SBoxEventHandle.SBOX_SADNBOX_BILL_LIST_GET, OnSandboxGetBillList);
        EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_SADNBOX_BILL_SELECT, OnSandboxBillSelect);
        EventCenter.Instance.AddEventListener<List<string>>(SBoxEventHandle.SBOX_SADNBOX_PRINTER_LIST_GET, OnSandboxGetPrinterList);
        EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_SADNBOX_PRINTER_SELECT, OnSandboxPrinterSelect);
    }

    private void OnSandboxGetDateTime(DateTime dateTime)
    {
        IOCanvasModel.Instance.IODateTime = dateTime;
        EventCenter.Instance.EventTrigger(EventHandle.GET_SANDBOX_DATE);
    }

    private void OnSandboxSetPrintSetting(int result)
    {
        if (result == 0)
        {
            if (IOCanvasModel.Instance.CfgData.PrintMode == 0)
            {
                IOPopTips.Instance.ShowTips(Utils.GetLanguage("CurrentlyInNoPrintMode"));
            }
            else
                IOPopTips.Instance.ShowTips(Utils.GetLanguage("SavePrintOffsetSucceed"));
        }
        else
        {
            if (IOCanvasModel.Instance.errorCodeListAboutPrint.Count > 0)
            {
                switch (IOCanvasModel.Instance.errorCodeListAboutPrint[0])
                {
                    case 4:
                        IOPopTips.Instance.ShowTips(Utils.GetLanguage("PolicyBoxDisconnect"));
                        break;
                    case 5:
                        IOPopTips.Instance.ShowTips(Utils.GetLanguage("PrinterOutOfPaper"));
                        break;
                    case 6:
                        IOPopTips.Instance.ShowTips($"{Utils.GetLanguage("IllegalOpeningBox")}1");
                        break;
                    case 7:
                        IOPopTips.Instance.ShowTips($"{Utils.GetLanguage("IllegalOpeningBox")}2");
                        break;
                    case 11:
                        IOPopTips.Instance.ShowTips(Utils.GetLanguage("PrinterCutterBroken"));
                        break;
                    case 12:
                        IOPopTips.Instance.ShowTips(Utils.GetLanguage("PrinterDoorNotClosed"));
                        break;
                    default:
                        IOPopTips.Instance.ShowTips(Utils.GetLanguage("ParamsWrong"));
                        break;
                }
            }
            else
                IOPopTips.Instance.ShowTips(Utils.GetLanguage("ParamsWrong"));
        }
    }

    private void OnSandboxGetPrintSetting(int[] data)
    {
        if (data.Length > 0 && data[0] != 0)
        {
            Debug.LogError("报错,data为空/data非法");
            IOPopTips.Instance.ShowTips(Utils.GetLanguage("ParamsWrong"));
            return;
        }
        IOCanvasModel.Instance.IOPrintSettingDate = new int[data.Length - 1];
        Array.Copy(data, 1, IOCanvasModel.Instance.IOPrintSettingDate, 0, IOCanvasModel.Instance.IOPrintSettingDate.Length);
        EventCenter.Instance.EventTrigger(EventHandle.GET_SBOX_BATTLE_PRINTER_DATE);
    }

    private void OnSandboxGetBillList(List<string> billList)
    {
        //空/异常时给个空列表兜底,面板仍可显示(仅剩 Return)
        IOCanvasModel.Instance.IOBillList = billList ?? new List<string>();
        EventCenter.Instance.EventTrigger(EventHandle.GET_SBOX_BILL_LIST);
    }

    private void OnSandboxBillSelect(int result)
    {
        //result: 0=成功, <0=发送参数错误, >0=状态码
        if (result < 0)
        {
            IOPopTips.Instance.ShowTips(Utils.GetLanguage("ParamsWrong"));//发送参数错误:不刷新
            return;
        }
        //等待中提示与状态码提示互斥,提前到此处(避免被视图协程的同帧提示覆盖)
        if (result > 0)
            IOPopTips.Instance.ShowTips($"{Utils.GetLanguage("State")}:{result}", 4f);//状态码
        else
            IOPopTips.Instance.ShowTips(Utils.GetLanguage("BillSettingPW"), 4f);//成功:保存中
        EventCenter.Instance.EventTrigger(EventHandle.GET_SBOX_BILL_STATE);
    }

    private void OnSandboxGetPrinterList(List<string> printerList)
    {
        //空/异常时给个空列表兜底,面板仍可显示(仅剩 Return)
        IOCanvasModel.Instance.IOPrinterList = printerList ?? new List<string>();
        EventCenter.Instance.EventTrigger(EventHandle.GET_SBOX_PRINTER_LIST);
    }

    private void OnSandboxPrinterSelect(int result)
    {
        //result: 0=成功, <0=发送参数错误, >0=状态码
        if (result < 0)
        {
            IOPopTips.Instance.ShowTips(Utils.GetLanguage("ParamsWrong"));//发送参数错误:不刷新
            return;
        }
        //等待中提示与状态码提示互斥,提前到此处(避免被视图协程的同帧提示覆盖)
        if (result > 0)
            IOPopTips.Instance.ShowTips($"{Utils.GetLanguage("State")}:{result}", 4f);//状态码
        else
            IOPopTips.Instance.ShowTips(Utils.GetLanguage("PrinterSettingPW"), 4f);//成功:保存中
        EventCenter.Instance.EventTrigger(EventHandle.GET_SBOX_PRINTER_STATE);
    }

    private void OnSBoxCode(SBoxPermissionsData sBoxPermissionsData)
    {
        Debug.LogError($"OnSBoxCode-result:{sBoxPermissionsData.result}");
        if (sBoxPermissionsData.result < 0)
        {
            IOPopTips.Instance.ShowTips(Utils.GetLanguage("ParamsWrong"));
            return;
        }
        else if (sBoxPermissionsData.result == 1)
        {
            IOPopTips.Instance.ShowTips(Utils.GetLanguage("VerificationCodeRequired"));
            return;
        }
        bool temp = sBoxPermissionsData.permissions % 1000 > 0;
        IOPopTips.Instance.ShowTips(Utils.GetLanguage("CodeSucceed"));
        EventCenter.Instance.EventTrigger(EventHandle.CODE_SUCCEED,temp);
        SBoxIdea.RequestCoder(0);
        SBoxIdea.ReadConf();
    }

    private void OnRequestCoder(SBoxCoderData sBoxCoderData)
    {
        if (sBoxCoderData.result < 0)
        {
            IOPopTips.Instance.ShowTips(Utils.GetLanguage("ParamsWrong"));
            return;
        }
        else if (sBoxCoderData.result > 0)
        {
            IOPopTips.Instance.ShowTips($"{Utils.GetLanguage("Wrong")}:{sBoxCoderData.result}");
            return;
        }

        if (IOCanvasModel.Instance.isBillPanelNeedCoderData)//zxd add
        {
            IOCanvasModel.Instance.isBillPanelNeedCoderData = false;
            EventCenter.Instance.EventTrigger(EventHandle.INSTANTIATE_BILL_DETAL);
            return;
        }

        IOCanvasModel.Instance.coderData = sBoxCoderData;
        EventCenter.Instance.EventTrigger(EventHandle.INIT_CODER_VIEW);
    }

    private void OnReadConfig(SBoxConfData sBoxConfData)//后台配置
    {
        if (sBoxConfData.result < 0)
        {
            IOPopTips.Instance.ShowTips(Utils.GetLanguage("ParamsWrong"));
            return;
        }
        else if (sBoxConfData.result > 0)
        {
            IOPopTips.Instance.ShowTips($"{Utils.GetLanguage("Wrong")}:{sBoxConfData.result}");
            return;
        }
        Debug.LogError("Read:" + JsonConvert.SerializeObject(sBoxConfData));
        IOCanvasModel.Instance.CfgData = sBoxConfData;//已赋值
        //回读算法卡押分倍率,与 Load.cs 启动路径一致,保证保存后/外部改卡后本地数据同步
        IOCanvasModel.Instance.UpdateBetSwitchFromConf(sBoxConfData.BetSwitch);

        IOCanvasModel.Instance.netSwitch = PlayerPrefs.GetInt("NetSwitch", 0);
        IOCanvasModel.Instance.tempNetSwitch = IOCanvasModel.Instance.netSwitch;

        var msgConfig = new MsgConfig//发送后台配置信息
        {
            countDown = sBoxConfData.CountDown,
            coinValue = sBoxConfData.CoinValue,
            ticketValue = sBoxConfData.TicketValue,
            scoreTicket = sBoxConfData.scoreTicket,
            limitBetsWins = sBoxConfData.LimitBetsWins,
            minBet = sBoxConfData.MinBet,//最原始的
            pulseValue = sBoxConfData.PulseValue,
            switchList = IOCanvasModel.Instance.switchList,
            soundVolumnScale = IOCanvasModel.Instance.SoundVolumScale,
            betsMinOfNetJackpots = IOCanvasModel.Instance.netJackpotMinBets,
        };
        Debug.LogError("Send:" + JsonConvert.SerializeObject(msgConfig));
        
        EventCenter.Instance.EventTrigger(EventHandle.UPDATE_BET_SWITCH);

        var msgInfo = new MsgInfo
        {
            cmd = (int)S2C_CMD.S2C_UpdateConfig,
            id = -1,
            jsonData = JsonConvert.SerializeObject(msgConfig),
        };
        NetMgr.Instance.SendToAllClient(JsonConvert.SerializeObject(msgInfo));

        EventCenter.Instance.EventTrigger(EventHandle.REFRESH_IOCANVAS);
    }

    private void OnConfirmPassword(ulong password)
    {
        switch (IOCanvasModel.Instance.state)
        {
            case IOState.CheckPermissions:
                CheckPassword(password);
                break;
            case IOState.Code:
                SBoxIdea.Coder(0, (ulong)password);
                break;
            case IOState.EditPassword:
                switch (view.selectSection.state)
                {
                    case IOSectionState.EditPassword:
                        IOCanvasModel.Instance.tempPassword = password;
                        view.selectSection.state = IOSectionState.NewPassword;
                        view.passwordPanel.SetPlaceholderText(Utils.GetLanguage("Please enter new password again"));
                        break;
                    case IOSectionState.NewPassword:
                        if (IOCanvasModel.Instance.tempPassword == password)
                            ChangePassword(password);
                        else
                        {
                            //Debug.LogError("修改密码错误");
                            IOPopTips.Instance.ShowTips(Utils.GetLanguage("EditFailed"));//提示:修改密码时,修改密码和确认修改密码输入不一样,
                        }
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }
    }

    private void CheckPassword(ulong password)
    {
        SBoxIdea.CheckPassword((int)password);
    }

    private void ChangePassword(ulong password)
    {
        SBoxIdea.ChangePassword((int)password);
    }

    private void OnCheckPassword(SBoxPermissionsData sBoxPermissionsData)
    {
        if (sBoxPermissionsData.result < 0)
        {
            IOPopTips.Instance.ShowTips(Utils.GetLanguage("ParamsWrong"));
            return;
        }
        else if (sBoxPermissionsData.result == 1)
        {
            IOPopTips.Instance.ShowTips(Utils.GetLanguage("VerificationCodeRequired"));
            return;
        }
        IOCanvasModel.Instance.permissions = sBoxPermissionsData.permissions;

        if (Application.isEditor)
            sBoxPermissionsData.permissions = 2;
        if (sBoxPermissionsData.permissions > 0)
        {
            switch (IOCanvasModel.Instance.state)
            {
                case IOState.CheckPermissions:
                    view.DestroyManagerPasswordPanel();
                    view.InstantiateMenuPanel();
                    IOCanvasModel.Instance.state = IOState.SelectFunction;
                    IOCanvasModel.Instance.isInitMenu = true;
                    break;
                case IOState.EditPassword:
                    view.EditPassword();
                    break;
                default:
                    break;
            }
        }
        else
            IOPopTips.Instance.ShowTips($"{Utils.GetLanguage("PasswordWrong")}:{sBoxPermissionsData.result}");
    }

    private void OnChangePassword(SBoxPermissionsData sBoxPermissionsData)
    {
        if (sBoxPermissionsData.result < 0)
        {
            IOPopTips.Instance.ShowTips(Utils.GetLanguage("ParamsWrong"));
            return;
        }
        else if (sBoxPermissionsData.result == 1)
        {
            IOPopTips.Instance.ShowTips(Utils.GetLanguage("VerificationCodeRequired"));
            return;
        }
        IOPopTips.Instance.ShowTips(Utils.GetLanguage("EditSucceed"));
        view.selectSection.selected = false;
        view.passwordPanel.SetPlaceholderText(Utils.GetLanguage("Please enter new password"));
        //view.passwordPanel.Focuse(false);
        view.SetCurSelect();
        view.selectSection.state = IOSectionState.EditPassword;
        IOCanvasModel.Instance.permissions = sBoxPermissionsData.permissions;
    }

    private void OnWriteConf(SBoxPermissionsData sBoxPermissionsData)
    {
        switch (sBoxPermissionsData.result)
        {
            case -1:
                IOPopTips.Instance.ShowTips(Utils.GetLanguage("ParamsWrong"));
                break;
            case 0:
                IOPopTips.Instance.ShowTips(Utils.GetLanguage("SaveSucceed"));

                IOCanvasModel.Instance.netSwitch = IOCanvasModel.Instance.tempNetSwitch;
                PlayerPrefs.SetInt("NetSwitch", IOCanvasModel.Instance.netSwitch);
                break;
            case 1:
                IOPopTips.Instance.ShowTips(Utils.GetLanguage("VerificationCodeRequired"));
                break;
            case 2:
                IOPopTips.Instance.ShowTips(Utils.GetLanguage("NeedCode"));
                break;
            case 3:
                //增加一个提示:需要打时间码
                IOPopTips.Instance.ShowTips(Utils.GetLanguage("NeedToAddTimecodes"));
                break;
        }
        IOCanvasModel.Instance.permissions = sBoxPermissionsData.permissions;
        SBoxIdea.ReadConf();
    }

    private void OnSetDate(int result)
    {
        switch (result)
        {
            case -1:
                IOPopTips.Instance.ShowTips(Utils.GetLanguage("ParamsWrong"));
                break;
            case 0:
                IOPopTips.Instance.ShowTips(Utils.GetLanguage("SaveSucceed"));
                view.CloseModifiedDatePanel();
                break;
        }
    }
}