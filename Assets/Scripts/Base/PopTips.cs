using GameUtil;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class PopTips : MonoSingleton<PopTips>
{
    private Text text;
    private float showTime;
    private bool isShow = true;
    private DelayTimer showTimer;
    private RectTransform rect;
    private string currentKey;//当前显示提示的key,用于低优先级提示(如换纸提示)识别;切换语言后凭key仍能判断是否为同一条提示

    private void Awake()
    {
        //单例,初始化需要挂载物体显示,所以显示着但是可以scale=0,也可以在下面被调用的函数里添加判空判断
        text = transform.Find("Text").GetComponent<Text>();
        rect = transform.GetComponent<RectTransform>();
        EventCenter.Instance.AddEventListener(EventHandle.CHANGE_LANGUAGE, OnChangeLanguage);
    }

    /// <summary>
    /// 切换语言时,仅把当前显示的提示文本更新为对应语言(保持显示状态/不计时/不广播)
    /// 依赖 ShowSystemTips 记录的 currentKey,对只传文本未传key的提示无法重译(保持不变)
    /// </summary>
    private void OnChangeLanguage()
    {
        if (!isShow || string.IsNullOrEmpty(currentKey)) return;
        text.text = Utils.GetLanguage(currentKey);
    }

    public void ChangeTips(string str)
    {
        text.text = str;
    }

    public void ShowTips(string str, float time = 3)
    {
        if (text.text == str && isShow) return;
        rect.anchoredPosition = Vector2.zero;//tips位置修改,幸运座位tip位置
        Show(str, time, false);
        currentKey = str;
    }

    public void ShowSystemTips(string str, float time = 3, bool sendToClient = true, bool isChecking = false, int seconds = -1, bool isShowInLittle = true)
    {
        if (text.text == Utils.GetLanguage(str) && isShow) return;//text.text == str
        rect.anchoredPosition = new Vector2(0, 200);//tips位置修改-kuarser
        string show = isChecking?str:Utils.GetLanguage(str);
        Show(show, time, true);
        currentKey = str;
        if(isShowInLittle) MacPopTips.Instance.ShowSystemTips(show, time, str);//小屏幕显示(带key,切换语言时可刷新)
        if(!sendToClient)return;
        TipsStruct tipsStruct = new TipsStruct
        {
            str = str,
            time = time,
            isChecking = isChecking,
            seconds = seconds
        };
        MsgInfo msg = new MsgInfo
        {
            cmd = (int)S2C_CMD.S2C_SystemTips,
            id = -1,
            jsonData = JsonConvert.SerializeObject(tipsStruct)
        };
        NetMgr.Instance.SendToAllClient(JsonConvert.SerializeObject(msg));
    }

    /// <summary>
    /// 当前是否正在显示指定key以外的其他提示
    /// 供低优先级提示(如换纸提醒)判断:仅当没有其他提示占用主屏时才显示
    /// 传入key而非文本,这样切换语言后同一条提示(旧语言文本)也能被识别,从而刷新为新语言
    /// </summary>
    public bool IsShowingOtherTips(string key)
    {
        if (!isShow) return false;
        if (string.IsNullOrEmpty(text.text)) return false;
        return currentKey != key;
    }

    private void Show(string str, float time = 3, bool isSystem = false)
    {
        text.text = str;
        showTime = time;
        if (showTime > -1)
        {
            isShow = true;
            ShowTimer(time);
        }
        else
            isShow = true;
        transform.localScale = isSystem ? Vector3.one * 0.75f : Vector3.one;
    }

    private void ShowTimer(float time)
    {
        if (showTimer == null)
            showTimer = Timer.DelayAction(time, HideTips);
        else
            showTimer.Restart();
    }

    public void HideTips()
    {
        if (showTimer != null)
            showTimer.Cancel();
        MacPopTips.Instance.HideTips();//小屏幕显示
        text.text = "";
        transform.localScale = Vector3.zero;
        isShow = false;
        currentKey = null;
        NetMgr.Instance.SendToAllClient(JsonConvert.SerializeObject(new MsgInfo
        {
            cmd = (int)S2C_CMD.S2C_HideTips,
            id = -1,
        }));
    }
}

public class TipsStruct
{
    public bool isshow;
    public string str;
    public float time;
    public int[] winLockPlayers;
    public bool isChecking = false;
    public int seconds = -1;
}
