using SBoxApi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UITriggerView : MonoBehaviour
{

    //新功能UI 触发器
    private EventTrigger zhongwenBtn;//中文触发器
    private Image zhongwenBtnImage;
    private EventTrigger pailuBtn;//牌路触发器
    private Image pailuBtnImage;
    //UI 触发器
    private EventTrigger cancelBtn;//取消压分触发器
    private Image cancelBtnImage;

    private EventTrigger qiangzhuangBtn;
    private Image qiangzhuangImage;


    //ui 按钮
    private Button translateBtn;//中英文翻译



    private void Awake()
    {
        InitView();
    }
    private void Start()
    {
        if (!View.OriginalBaccaratUiPresent) return;

        AddAllEventTrigger();
    }

    private void InitView()
    {
        if (!View.OriginalBaccaratUiPresent) return;
        var UICanvas = GameObject.Find("UICanvas").transform;

        cancelBtn = UICanvas.Find("Bottom/RightUp/CancelBtn").GetComponent<EventTrigger>();//取消压分 触发器
        cancelBtnImage = cancelBtn.transform.Find("Icon").GetComponent<Image>();

        zhongwenBtn = UICanvas.Find("Bottom/Left/zhongwenBtn").GetComponent<EventTrigger>();//中文 触发器
        zhongwenBtnImage = zhongwenBtn.transform.Find("Icon").GetComponent<Image>();
        pailuBtn = UICanvas.Find("Bottom/RightUp/pailuBtn").GetComponent<EventTrigger>();//牌路 触发器
        pailuBtnImage = pailuBtn.transform.Find("Icon").GetComponent<Image>();


        translateBtn = UICanvas.Find("Bottom/Left/zhongwenBtn").GetComponent<Button>();
        translateBtn.onClick.RemoveAllListeners();
        translateBtn.onClick.AddListener(() =>
        {
            ChangeTranslate();
            AudioManager.Instance.PlaySoundEff("buttonClick");
        });


        qiangzhuangBtn = UICanvas.Find("Bottom/qiangzhuangBtn").GetComponent<EventTrigger>();//抢庄 触发器
        qiangzhuangImage = qiangzhuangBtn.transform.Find("Icon").GetComponent<Image>();

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

    public void ChangeTranslate()
    {
        if (!View.OriginalBaccaratUiPresent) return;
        if (FSMSystem.Instance.CurrentStateID == StateID.Setting)//设置界面,设置状态中,不能使用该按钮来切换语言
        {
            //如果打开了设置界面,就不允许切换语言
            ClientWinLockTips.Instance.ShowSystemTips($"{Utils.GetLanguage("PleaseUseSettings")}");
            return;
        }
        //flag = !flag;
        //if (flag)
        if (IOCanvasModel.Instance.curlanguage == Language.en)
            IOCanvasModel.Instance.CurLanguage = Language.cht;
        else
            IOCanvasModel.Instance.CurLanguage = Language.en;
        //Debug.LogError("ChangeTranslate");
        EventCenter.Instance.EventTrigger(EventHandle.CHANGE_LANGUAGE);
    }

    private void AddAllEventTrigger()
    {
        if (!View.OriginalBaccaratUiPresent) return;
        //中文
        AddEventTrigger(zhongwenBtn, EventTriggerType.PointerEnter, (data) =>
        {
            zhongwenBtnImage.color = new Color(200 / 255f, 200 / 255f, 200 / 255f);
            //切换中文,英文
        });
        AddEventTrigger(zhongwenBtn, EventTriggerType.PointerExit, (data) =>
        {
            zhongwenBtnImage.color = Color.white;
        });

        //牌路
        AddEventTrigger(pailuBtn, EventTriggerType.PointerEnter, (data) =>
        {
            pailuBtnImage.color = new Color(200 / 255f, 200 / 255f, 200 / 255f);
            //显示界面
        });
        AddEventTrigger(pailuBtn, EventTriggerType.PointerExit, (data) =>
        {
            pailuBtnImage.color = Color.white;
        });

        //取消压分//没用了,改变了trigger,用MyTriggerDetection脚本代替
        AddEventTrigger(cancelBtn, EventTriggerType.PointerEnter, (data) =>
        {
            cancelBtnImage.color = new Color(200 / 255f, 200 / 255f, 200 / 255f);
            PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_ESC, true);

            AudioManager.Instance.PlaySoundEff("buttonClick");
        });
        AddEventTrigger(cancelBtn, EventTriggerType.PointerExit, (data) =>
        {
            cancelBtnImage.color = Color.white;
            PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_ESC, false);
        });

        cancelBtn.GetComponent<MyTriggerDetection>().pointerEnter += () =>
        {
            cancelBtnImage.color = new Color(200 / 255f, 200 / 255f, 200 / 255f);
            PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_ESC, true);
            AudioManager.Instance.PlaySoundEff("buttonClick");
        };
        cancelBtn.GetComponent<MyTriggerDetection>().pointerExit += () =>
        {
            cancelBtnImage.color = Color.white;
            PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_ESC, false);
        };

        //取消压分//没用了,改变了trigger,用MyTriggerDetection脚本代替
        AddEventTrigger(qiangzhuangBtn, EventTriggerType.PointerEnter, (data) =>
        {
            cancelBtnImage.color = new Color(200 / 255f, 200 / 255f, 200 / 255f);
            if(Model.Instance.canBidBanker)//明牌时阻塞抢庄
                PlayerMgr.Instance.localPlayer.SetBankerKeyState(Player.BankerKeySource.Virtual, true);//编辑器模式下用的
            AudioManager.Instance.PlaySoundEff("buttonClick");
            Debug.LogError("点击了 qiangzhuangBtn,EventTrigger");
        });
        AddEventTrigger(qiangzhuangBtn, EventTriggerType.PointerExit, (data) =>
        {
            cancelBtnImage.color = Color.white;
            PlayerMgr.Instance.localPlayer.SetBankerKeyState(Player.BankerKeySource.Virtual, false);
        });
        qiangzhuangBtn.GetComponent<MyTriggerDetection>().pointerEnter += () =>
        {
            qiangzhuangImage.color = new Color(200 / 255f, 200 / 255f, 200 / 255f);
            AudioManager.Instance.PlaySoundEff("buttonClick");
            if(Model.Instance.canBidBanker)//明牌时阻塞抢庄
                PlayerMgr.Instance.localPlayer.SetBankerKeyState(Player.BankerKeySource.Virtual, true);//双屏点击用的
            //Debug.LogError("点击了 qiangzhuangBtn,MyTriggerDetection");
        };
        qiangzhuangBtn.GetComponent<MyTriggerDetection>().pointerExit += () =>
        {
            qiangzhuangImage.color = Color.white;
            PlayerMgr.Instance.localPlayer.SetBankerKeyState(Player.BankerKeySource.Virtual, false);
        };

        ////切换压分倍率
        //AddEventTrigger(switchBtn, EventTriggerType.PointerEnter, (data) =>
        //{
        //    switchBtnImage.color = new Color(200 / 255f, 200 / 255f, 200 / 255f);
        //    PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_SWITCH, true);
        //    //RefreshBetCoin();
        //    //isClickSwitch = true;
        //});
        //AddEventTrigger(switchBtn, EventTriggerType.PointerExit, (data) =>
        //{
        //    switchBtnImage.color = Color.white;
        //    PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_SWITCH, false);
        //    //isClickSwitch = false;
        //});
    }

    private void AddEventTrigger(EventTrigger eventTrigger, EventTriggerType eventType, UnityAction<BaseEventData> unityAction)
    {
        UnityAction<BaseEventData> action = new UnityAction<BaseEventData>(unityAction);
        if (eventTrigger.triggers.Count != 0)
        {
            for (int i = 0; i < eventTrigger.triggers.Count; i++)
            {
                if (eventTrigger.triggers[i].eventID == eventType)
                {
                    eventTrigger.triggers[i].callback.AddListener(action);
                    return;
                }
            }
        }
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventType;
        entry.callback.AddListener(action);
        eventTrigger.triggers.Add(entry);
    }
}
