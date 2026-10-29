using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIAudioView : MonoBehaviour
{
    //声音,音量
    private Button btnAudio;
    private Button btnAudioMute;
    private Image btnAudioIcon;
    private Image btnAudioMuteIcon;
    private Button btnAudioMuteHide;
    private Slider sliderAudio;
    private GameObject wndAudioSlider;
    private Text sliderNumText;
    private float sliderNumValue;

    //private float qiangZhuangInterval = 0;


    private void Awake()
    {
        InitView();
    }

    void Start()
    {
        btnAudio.onClick.RemoveAllListeners();
        btnAudio.onClick.AddListener(OnClickAudio);

        btnAudioMute.onClick.RemoveAllListeners();
        btnAudioMute.onClick.AddListener(OnClickAudiMute);

        btnAudioMuteHide.onClick.RemoveAllListeners();
        btnAudioMuteHide.onClick.AddListener(OnClickAudiMuteHide);

        sliderAudio.onValueChanged.RemoveAllListeners();
        sliderAudio.onValueChanged.AddListener(OnClickAudioSliderValueChange);


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

    private void InitView()
    {
        // 音量设置UI
        var UICanvase = GameObject.Find("UICanvas").transform;
        btnAudioIcon = UICanvase.Find("Bottom/Left/AudioBtn/content2").GetComponent<Image>();
        btnAudioMuteIcon = UICanvase.Find("Bottom/Left/AudioBtn/ContentIcon_Mute").GetComponent<Image>();
        btnAudioIcon.gameObject.SetActive(true);
        btnAudioMuteIcon.gameObject.SetActive(false);

        this.wndAudioSlider = UICanvase.Find("Bottom/Voice").gameObject;//音量滑动条父节点
        this.btnAudio = UICanvase.Find("Bottom/Left/AudioBtn").GetComponent<Button>();//音量显示按钮


        this.btnAudioMute = UICanvase.Find("Bottom/Voice/AudioMuteBtn").GetComponent<Button>();//显示静音按钮,音量为0
        this.btnAudioMute.gameObject.SetActive(false);//不使用了

        this.btnAudioMuteHide = UICanvase.Find("Bottom/Voice/AudioMuteBGBtn").GetComponent<Button>();//隐藏静音按钮,音量>0
        this.btnAudioMuteHide.gameObject.SetActive(false);//不使用了

        this.sliderAudio = UICanvase.Find("Bottom/Voice/Slider").GetComponent<Slider>();//音量滑动条
        this.sliderAudio.value = AudioManager.Instance.SoundVolumScale;

        this.sliderNumText = this.sliderAudio.transform.Find("Handle Slide Area/Handle/Text").GetComponent<Text>();
        this.sliderNumText.text = ((int)Math.Round(this.sliderAudio.value * 100)).ToString();

    }


    void OnClickAudio()
    {
        this.wndAudioSlider.SetActive(!this.wndAudioSlider.activeSelf);
        if (this.wndAudioSlider.activeSelf)
        {
            CancelInvoke("DelayCloseWndAudioSlier");
            Invoke("DelayCloseWndAudioSlier", 3f);
        }
        AudioManager.Instance.PlaySoundEff("buttonClick");
    }
    void OnClickAudiMute()
    {
        //this.wndAudioSlider.SetActive(!this.wndAudioSlider.activeSelf);
        sliderNumValue = this.sliderAudio.value;
        this.sliderAudio.value = 0;
        OnClickAudioSliderValueChange(this.sliderAudio.value);
        if (this.wndAudioSlider.activeSelf)
        {
            CancelInvoke("DelayCloseWndAudioSlier");
            Invoke("DelayCloseWndAudioSlier", 3f);
        }
        AudioManager.Instance.PlaySoundEff("buttonClick");
    }
    void OnClickAudiMuteHide()
    {
        this.sliderAudio.value = sliderNumValue;
        OnClickAudioSliderValueChange(this.sliderAudio.value);
        if (this.wndAudioSlider.activeSelf)
        {
            CancelInvoke("DelayCloseWndAudioSlier");
            Invoke("DelayCloseWndAudioSlier", 3f);
        }
        AudioManager.Instance.PlaySoundEff("buttonClick");
    }

    void OnClickAudioSliderValueChange(float val)
    {
        this.btnAudioIcon.gameObject.SetActive(val > 0);//音量大于0,还是显示喇叭
        this.btnAudioMuteIcon.gameObject.SetActive(val <= 0);//音量小于0,显示静音喇叭

        //this.btnAudio.gameObject.SetActive(val > 0);
        //this.btnAudioMute.gameObject.SetActive(val <= 0);
        //this.btnAudioMute.gameObject.SetActive(val > 0);//不用了

        AudioManager.Instance.SoundVolumScale = val;
        if (this.wndAudioSlider.activeSelf)
        {
            CancelInvoke("DelayCloseWndAudioSlier");
            Invoke("DelayCloseWndAudioSlier", 3f);
        }
        this.sliderNumText.text = ((int)Math.Round(this.sliderAudio.value * 100)).ToString();

        //if (Time.time - qiangZhuangInterval < 0.8f)
        //{
        //    Debug.Log("多次点击,点击过快,不播放音效");
        //    return;
        //}
        //qiangZhuangInterval = Time.time;
        //AudioManager.Instance.PlaySoundEff("buttonClick");
    }

    void DelayCloseWndAudioSlier()
    {
        this.wndAudioSlider.SetActive(false);
    }

    void UpdateSliderAudio()
    {
        this.sliderAudio.value = AudioManager.Instance.SoundVolumScale;
    }

}
