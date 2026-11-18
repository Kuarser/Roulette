using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHistoryView : MonoBehaviour
{

    private GameObject HistoryRecordPanel;//历史记录界面
    private Button historyBtn;
    private Button historyCloseBtn;


    void Awake()
    {
        InitView();
    }

    void Start()
    {
        HistoryRecordPanel.transform.localScale = Vector3.zero;//历史押注隐藏
    }

    private void InitView()
    {
        var popCanvas = GameObject.Find("PopCanvas").transform;
        HistoryRecordPanel = popCanvas.Find("recordDialog3").gameObject;//历史记录界面
        historyCloseBtn = HistoryRecordPanel.transform.Find("closeBtn").GetComponent<Button>();
        historyCloseBtn.onClick.RemoveAllListeners();
        historyCloseBtn.onClick.AddListener(() =>
        {
            ChangeHistoryRecordPanelActive();
            AudioManager.Instance.PlaySoundEff("buttonClick");
        });

        // 纯轮盘机台：历史按钮在原九门界面底部，已随 UICanvas 子物体一起删除。
        if (!View.OriginalBaccaratUiPresent) return;

        var UICanvas = GameObject.Find("UICanvas").transform;
        historyBtn = UICanvas.Find("Bottom/Left/HistoryRecordBtn").GetComponent<Button>();
        historyBtn.onClick.RemoveAllListeners();
        historyBtn.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySoundEff("buttonClick");
            if(PlayerMgr.Instance.localPlayer.totalDataStruct == null || PlayerMgr.Instance.localPlayer.totalDataStruct.Count == 0)//暂无历史记录
            {
                PopTips.Instance.ShowSystemTips(Utils.GetLanguage("HistoryNotFound"),time:3,sendToClient:false);
                return;
            }
            ChangeHistoryRecordPanelActive();
        });

        ////给所有点击按钮添加点击音效
        //var btns = this.GetComponentsInChildren<Button>(true);
        //foreach (var btn in btns)
        //{
        //    btn.onClick.AddListener(() =>
        //    {
        //        AudioManager.Instance.PlaySoundEff("buttonClick");
        //    });
        //}
    }

    private void ChangeHistoryRecordPanelActive()
    {
        if (!HistoryRecordPanel.activeSelf)
        {
            HistoryRecordPanel.SetActive(true);
            HistoryRecordPanel.transform.localScale = Vector3.zero;
        }
        if (HistoryRecordPanel.transform.localScale.Equals(Vector3.zero))
        {
            HistoryRecordPanel.transform.localScale = Vector3.one;
            HistoryRecordPanel.GetComponent<RecordDialogView>().OpenFirstPage();
            Model.Instance.myTriggerDetectionCanEnterCount++;
        }
        else
        {
            HistoryRecordPanel.transform.localScale = Vector3.zero;
            Model.Instance.myTriggerDetectionCanEnterCount--;
        }
    }

    public void ResetHistoryRecord()
    {
        HistoryRecordPanel.GetComponent<RecordDialogView>().Reset();
    }
}
