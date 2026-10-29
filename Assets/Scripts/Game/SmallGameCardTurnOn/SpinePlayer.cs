using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class SpinePlayer : MonoBehaviour
{
    public GameObject target;
    public string[] animGroup;
    public int groupIdx = 0;
    public bool isAutoPlay = true;

    SkeletonGraphic _skeletonGraphic;
    SkeletonGraphic skeletonGraphic
    {
        get
        {
            if (this._skeletonGraphic == null)
                this._skeletonGraphic = target.GetComponent<SkeletonGraphic>();
            return _skeletonGraphic;
        }
    }


    int curAnimIdx = 0;

    void OnEnable()
    {
        if (this.isAutoPlay)
        {
            this.Show();
        }
    }

    void OnDisable()
    {
        this.skeletonGraphic.AnimationState.ClearTracks();
        this.skeletonGraphic.Initialize(true);
        this.target.SetActive(false);
        this.gameObject.SetActive(false);
    }

    public void ShowAnimation(int index = 0)
    {
        this.skeletonGraphic.AnimationState.ClearTracks();
        var animList = this.animGroup[this.groupIdx].Split('|');
        var name = animList[index];
        this.skeletonGraphic.Initialize(true);
        this.skeletonGraphic.AnimationState.SetAnimation(0, name, name.Contains("Idel") || name.Contains("idle") || name.Contains("Back"));
    }

    public void Show(int idx = 0)
    {
        this.groupIdx = idx;
        this.curAnimIdx = 0;
        this.target.SetActive(true);
        this.gameObject.SetActive(true);
        PlayAnim();
    }

    public void Hide()
    {
        this.groupIdx = 0;
        this.curAnimIdx = 0;
        ShowAnimation();//卡牌翻动 的 spine动画,初始化为背牌动画
        //this.target.SetActive(false);
        //this.gameObject.SetActive(false);
    }

    void OnAnimationComplete(Spine.TrackEntry trackEntry)
    {
        this.skeletonGraphic.AnimationState.Complete -= OnAnimationComplete;
        this.curAnimIdx++;
        PlayAnim();
    }

    void PlayAnim()
    {
        if (this.groupIdx >= this.animGroup.Length)
            return;

        var animList = this.animGroup[this.groupIdx].Split('|');
        if (this.curAnimIdx >= animList.Length)
            return;

        this.skeletonGraphic.AnimationState.ClearTracks();
        var name = animList[this.curAnimIdx];
        this.skeletonGraphic.Initialize(true);
        this.skeletonGraphic.AnimationState.SetAnimation(0, name, name.Contains("Idle") || name.Contains("idle") || name.Contains("Back")).Complete += OnAnimationComplete;
    }
}