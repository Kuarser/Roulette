using GameUtil;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class NetworkErrorTips : MonoSingleton<NetworkErrorTips>
{
    private Text text;
    private float showTime;
    private bool isShow = true;
    private DelayTimer showTimer;
    private RectTransform rect;

    private void Awake()
    {
        //单例,初始化需要挂载物体显示,所以显示着但是scale=0
        //如果报错,问题链接指向这里,去看一下是否是隐藏了物体
        text = transform.Find("Text").GetComponent<Text>();
        rect = transform.GetComponent<RectTransform>();
    }

    public void ChangeTips(string str)
    {
        text.text = str;
    }

    public void ShowTips(string str, float time = 3)
    {
        if (text.text == str && isShow) return;
        rect.anchoredPosition = Vector3.zero;
        Show(str, time, false);
    }

    public void ShowSystemTips(string str, float time = 3)
    {
        if (text.text == str && isShow) return;
        rect.anchoredPosition = new Vector2(0, 260);
        Show(str, time, true);
        // TipsStruct tipsStruct = new TipsStruct //发了两次
        // {
        //     str = str,
        //     time = time
        // };
        // MsgInfo msg = new MsgInfo
        // {
        //     cmd = (int)S2C_CMD.S2C_ShowClientWinLock,
        //     id = -1,
        //     jsonData = JsonConvert.SerializeObject(tipsStruct)
        // };
        // NetMgr.Instance.SendToAllClient(JsonConvert.SerializeObject(msg));
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
        transform.localScale = isSystem ? Vector3.one * 0.85f : Vector3.one;
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
        text.text = "";
        transform.localScale = Vector3.zero;
        isShow = false;
        // NetMgr.Instance.SendToAllClient(JsonConvert.SerializeObject(new MsgInfo
        // {
        //     cmd = (int)S2C_CMD.S2C_HideClientWinLock,
        //     id = -1,
        // }));
    }
}
