using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIRecordView : MonoBehaviour
{
    private List<RecordView> recordViewList = new List<RecordView>();//本轮结果显示视图//100局结果显示视图//珠盘路
    private ScrollRect recordListZhuPanRect;
    private ScrollRect recordListRect;

    private RecordListView_SelfAddItem recordListView_SelfAddItem;//大路视图,优化版,够用就行,不够再加,不用新建一大堆,浪费空间


    private void Awake()
    {
        InitView();
    }


    void InitView()
    {
        var UICanvas = GameObject.Find("UICanvas").transform;
        var recordParent = UICanvas.Find("Top/Right/record2/ScrollView/Viewport/Content/RecordList");
        for (int i = 0; i < recordParent.childCount; i++)
        {
            recordViewList.Add(recordParent.GetChild(i).GetComponent<RecordView>());//100局结果显示视图
        }
        recordListZhuPanRect = UICanvas.Find("Top/Right/record2/ScrollView").GetComponent<ScrollRect>();
        recordListRect = UICanvas.Find("Top/Left/BG/record/ScrollView").GetComponent<ScrollRect>();
        recordListView_SelfAddItem = UICanvas.Find("Top/Left/BG/record/ScrollView/Viewport/Content/recordList_SelfAddItem").GetComponent<RecordListView_SelfAddItem>();
    }

    public void ShowRecordView(RecordStruct record, bool isShow = false)//100局结果显示视图
    {
        var recordView = recordViewList[record.game - 1];
        recordView.SetRecordView(record);//赢分动画
        if (isShow)
            recordView.gameObject.SetActive(true);
    }

    public void ShowSetRecordBySelfAddItem()
    {
        recordListView_SelfAddItem.SetRecords(Model.Instance.records);
    }

    public void ResetRecordView()
    {
        recordListView_SelfAddItem.ResetRecords();

        recordViewList.ForEach(view => view.HideRecordView());//隐藏格子中的icon//100局结果显示视图
    }

    public void UpdateRecordView()
    {
        recordListView_SelfAddItem.SetRecords(Model.Instance.records);
        recordListRect.transform.GetComponent<ScrollSnapAlign>().RefreshPos(recordListView_SelfAddItem.LastCol);
        //int tempCol = Math.Ceiling(Model.Instance.curRecord.game/6.0)>5?(int)Math.Ceiling(Model.Instance.curRecord.game/6.0)-5:0;
        //recordListZhuPanRect.transform.GetComponent<ScrollSnapAlign>().RefreshPos(tempCol);//回归珠盘路不能动的模式
    }

    public void SetRecordView()
    {
        if (Model.Instance.records == null || Model.Instance.records.Count <= 0) return;
        recordListView_SelfAddItem.SetRecords(Model.Instance.records);
    }

    public void ShowZhuPanLu()
    {
        //珠盘路用 records 来 显示,不用 BeadPlateRoad ,因为有小游戏判断,钻石显示判断
        var records = Model.Instance.records;
        for (int i = 0; i < records.Count; i++)
        {
            var record = records[i];
            ShowRecordView(record, true);
        }
    }

    public void ShowDaLu()
    {
        //大路用 records 来 显示,不用 BigRoadList ,因为有小游戏判断,钻石显示判断
        recordListView_SelfAddItem.SetRecords(Model.Instance.records);
    }
}
