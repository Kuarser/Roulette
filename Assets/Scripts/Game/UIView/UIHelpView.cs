using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHelpView : MonoBehaviour
{
    private GameObject helpPanel;//帮助界面
    private Button helpBtn;
    private Button helpCloseBtn;

    private void Awake()
    {
        InitView();
    }

    void Start()
    {
        InitViewData();
    }

    private void InitView()
    {
        var UICanvas = GameObject.Find("UICanvas").transform;
        var popCanvas = GameObject.Find("PopCanvas").transform;
        helpPanel = popCanvas.Find("helpPanel").gameObject;//帮助界面
        helpBtn = UICanvas.Find("Bottom/Left/HelpBtn").GetComponent<Button>();
        helpBtn.onClick.RemoveAllListeners();
        helpBtn.onClick.AddListener(() =>
        {
            ChangeHelpPanelActive();
            AudioManager.Instance.PlaySoundEff("buttonClick");
        });
        helpCloseBtn = helpPanel.transform.Find("closeBtn").GetComponent<Button>();
        helpCloseBtn.onClick.RemoveAllListeners();
        helpCloseBtn.onClick.AddListener(() =>
        {
            ChangeHelpPanelActive();
            AudioManager.Instance.PlaySoundEff("buttonClick");
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
    private void InitViewData()
    {
        helpPanel.SetActive(false);
    }

    public void ChangeHelpPanelActive()
    {
        helpPanel.SetActive(!helpPanel.activeSelf);
        Model.Instance.myTriggerDetectionCanEnterCount = helpPanel.activeSelf ?
            Model.Instance.myTriggerDetectionCanEnterCount + 1 : Model.Instance.myTriggerDetectionCanEnterCount - 1;

        EventCenter.Instance.EventTrigger<bool>(EventHandle.HELP_DIALOG_ABOUT_NET_JACKPOT, helpPanel.activeSelf);//联网彩金的说明
    }
}
