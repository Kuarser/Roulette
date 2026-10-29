using GameUtil;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class MacPopTips : MonoSingleton<MacPopTips>
{
    private Text text;
    private float showTime;
    private bool isShow = true;
    private DelayTimer showTimer;
    private RectTransform rect;
    private string currentKey;//当前显示提示的key,切换语言时据此重译文本

    private void Awake()
    {
        text = transform.Find("Text").GetComponent<Text>();
        rect = transform.GetComponent<RectTransform>();
        EventCenter.Instance.AddEventListener(EventHandle.CHANGE_LANGUAGE, OnChangeLanguage);
    }

    /// <summary>
    /// 切换语言时,仅把当前显示的提示文本更新为对应语言(保持显示状态/不计时/不广播)
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
        rect.anchoredPosition = Vector3.zero;
        Show(str, time, false);
    }

    public void ShowSystemTips(string str, float time = 3, string key = null)
    {
        if (text.text == str && isShow) return;
        rect.anchoredPosition = new Vector2(0, 0);
        Show(str, time, true);
        currentKey = key;
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
        currentKey = null;
    }
}
