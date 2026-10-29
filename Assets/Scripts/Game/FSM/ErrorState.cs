
using System.Collections;
using Newtonsoft.Json;
using SBoxApi;
using UnityEngine;

public class ErrorState : FSMState
{
    public ErrorState()
    {
        stateID = StateID.Error;
        AddTransition(Transition.EndRound, StateID.EndRound);
        AddTransition(Transition.Setting, StateID.Setting);
    }

    public override void DoBeforeEntering()
    {
        string errStr = "";
        if (Model.Instance.errorCodeList.Count > 0)
            errStr = $"{Model.Instance.GetErrorStr(Model.Instance.errorCodeList[0])}";
        PopTips.Instance.ShowSystemTips(errStr, -1);
        AddEventListener();
    }

    public override void DoBeforeLeaving()
    {
        RemoveEventListener();
    }

        private void AddEventListener()
    {
        EventCenter.Instance.AddEventListener<int[]>(SBoxEventHandle.SBOX_BATTLE_END_ROUND, OnBattleEndRound);//结束本轮游戏
    }

    private void RemoveEventListener()
    {
        EventCenter.Instance.RemoveEventListener<int[]>(SBoxEventHandle.SBOX_BATTLE_END_ROUND, OnBattleEndRound);
    }

    private void OnBattleEndRound(int[] data)
    {
        for (int i = Model.Instance.curGame+1; i < Model.MAX_GAME + 1; i++)
        {
            RecordStruct record = new RecordStruct
            {
                game = i,
                result = data[i] & 0x03,
                pair = new int[2]{data[i] >> 2 & 0x01, data[i] >> 3 & 0x01},
                spresult = new int[4]{data[i] >> 4 & 0x01, data[i] >> 5 & 0x01, data[i] >> 6 & 0x01, data[i] >> 7 & 0x01},
                luckyId = 0,
            };
            Model.Instance.records.Add(record);//再存储
        }
        Model.Instance.curGame = Model.MAX_GAME;//结束本轮,强制局数置为最大
        Controller.Instance.SetGameData();//ui显示局数

        //计算数字模型的珠盘路,大路,大眼路,小路,小强路
        Model.Instance.SetAllRoadData();
        //ui显示各种路
        Controller.Instance.ViewShowAllRoad();

        //计算各种类型个数
        Model.Instance.SetRecordTypeCount();
        //ui显示类型个数
        Controller.Instance.ViewShowTypeCount();

        //通知分机同步,并延迟5秒后转换为prepare状态
        MsgInfo msgInfo = new MsgInfo
        {
            cmd = (int)S2C_CMD.S2C_SetRecod,
            id = -1,
            jsonData = JsonConvert.SerializeObject(Model.Instance.records),
        };
        NetMgr.Instance.SendToAllClient(JsonConvert.SerializeObject(msgInfo));

        if (IOCanvasModel.Instance.CfgData.PrintMode == 0)
            MonoMgr.Instance.StartCoroutine(ErrorDelayActive());
        else
            Model.Instance.RemoveErrorCode(0);
    }

    private IEnumerator ErrorDelayActive()
    {
        yield return new WaitForSeconds(5);
        Model.Instance.RemoveErrorCode(0);
    }
}
