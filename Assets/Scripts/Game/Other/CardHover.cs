
using SBoxApi;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardHover : MonoBehaviour
{
    public float scaleFactor = 1.2f; // 放大倍数
    public float scaleSpeed = 8f;    // 缩放速度
    public Sprite OriginSprite = null;   //原图
    public Sprite lightSprite = null;    //光亮图
    [HideInInspector]
    public int index = 0;//挂载初始化时赋值

    private Color originalColor;
    private Vector3 originalScale;
    private bool isHovered = false;
    private Image icon;
    private Text betText;
    private MyTriggerDetection trigger;
    bool isInit = false;


    void Start()
    {
        Init();
    }
    void Init()
    {
        if (isInit) return;
        isInit = true;
        originalScale = transform.localScale;

        icon = transform.Find("BG").GetComponent<Image>();
        originalColor = icon.color;

        betText = transform.Find("Num").GetComponent<Text>();

        if (trigger == null)
        {
            trigger = transform.GetComponent<MyTriggerDetection>();
        }

        trigger.pointerEnter += OnExecutePointerEnter;
        trigger.pointerExit += OnExecutePointerExit;
    }

    public bool SetIndex(int value)
    {
        index = value;
        Init();
        if (index >= Model.Instance.betCoinList.Count)//kuarser--
        {
            //越界
            this.gameObject.SetActive(false);
            return false;
        }
        this.gameObject.SetActive(true);
        betText.text = "$" + Model.Instance.betCoinList[index];
        
        if (betText.text.Length >= 4)
        {
            float tempScale = 3.6f/betText.text.Length;
            var tempscale = betText.transform.localScale;
            tempscale.x = tempScale;
            betText.transform.localScale = tempscale;
        }
        return true;
    }

    void Update()
    {
        //isHovered 控制 缩放
        Vector3 targetScale = isHovered ? originalScale * scaleFactor : originalScale;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
        Color targetColor = isHovered ? originalColor : originalColor * 0.8f;
        icon.color = Color.Lerp(icon.color, targetColor, Time.deltaTime * scaleSpeed);
        icon.sprite = isHovered ? lightSprite : OriginSprite;
        betText.color = Color.Lerp(betText.color, targetColor, Time.deltaTime * scaleSpeed);
    }

    void OnExecutePointerEnter()
    {
        AudioManager.Instance.PlaySoundEff("buttonClick");
        if (Model.Instance.winLockPlayers.
            Contains(PlayerMgr.Instance.localPlayer.macId))
            return;
        PlayerMgr.Instance.localPlayer.betUnitIdx = index;
        //告知算法卡当前正在选择的押注筹码下标
        SBoxIdea.SetBetIndex(PlayerMgr.Instance.localPlayer.macId, index);
        isHovered = true;
        EventCenter.Instance.EventTrigger(EventHandle.PLAYER_INSTATE_CHANGE);
        EventCenter.Instance.EventTrigger(EventHandle.REFERESH_BET_COIN);
        //Debug.Log($"你选了什么硬币下标:{PlayerMgr.Instance.localPlayer.betUnitIdx}");
    }

    void OnExecutePointerExit()
    {
    }

    public void SetSelectActive()//显示隐藏 手指选择动画
    {
        isHovered = PlayerMgr.Instance.localPlayer.betUnitIdx == index;
    }

}
