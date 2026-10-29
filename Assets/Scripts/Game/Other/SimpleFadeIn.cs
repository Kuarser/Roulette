using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleFadeIn : MonoBehaviour
{
    [Header("缩放参数")]
    [Tooltip("起始缩放值")]
    public float startScale = 0f;
    [Tooltip("中间峰值缩放值")]
    public float peakScale = 1.2f;
    [Tooltip("最终缩放值")]
    public float endScale = 1f;

    [Header("时间参数")]
    [Tooltip("从起始值到峰值的时间(秒)")]
    public float timeToPeak = 0.3f;
    [Tooltip("从峰值到最终值的时间(秒)")]
    public float timeToEnd = 0.2f;
    
    [Header("缓动曲线")]
    [Tooltip("控制动画的加速和减速方式")]
    public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private RectTransform rectTransform;
    private float animationTimer = 0f;
    private enum AnimationState { Idle, ScalingUp, ScalingDown, Finished }
    private AnimationState currentState = AnimationState.Idle;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        // 重置动画状态
        rectTransform.localScale = Vector3.one * startScale;
        animationTimer = 0f;
        currentState = AnimationState.ScalingUp;
    }

    void Update()
    {
        if (currentState == AnimationState.Idle || currentState == AnimationState.Finished)
            return;

        animationTimer += Time.deltaTime;
        float currentScale = 0f;

        switch (currentState)
        {
            case AnimationState.ScalingUp:
                // 计算从startScale到peakScale的进度
                float progressUp = Mathf.Clamp01(animationTimer / timeToPeak);
                float curvedProgressUp = animationCurve.Evaluate(progressUp);
                currentScale = Mathf.Lerp(startScale, peakScale, curvedProgressUp);
                
                // 检查是否完成上升阶段
                if (progressUp >= 1f)
                {
                    currentScale = peakScale; // 确保精确值
                    animationTimer = 0f;
                    currentState = AnimationState.ScalingDown;
                }
                break;

            case AnimationState.ScalingDown:
                // 计算从peakScale到endScale的进度
                float progressDown = Mathf.Clamp01(animationTimer / timeToEnd);
                float curvedProgressDown = animationCurve.Evaluate(progressDown);
                currentScale = Mathf.Lerp(peakScale, endScale, curvedProgressDown);
                
                // 检查是否完成下降阶段
                if (progressDown >= 1f)
                {
                    currentScale = endScale; // 确保精确值
                    currentState = AnimationState.Finished;
                }
                break;
        }

        // 应用当前缩放值
        rectTransform.localScale = Vector3.one * currentScale;
    }

    // 提供公共方法以便外部控制
    public void PlayScaleAnimation()
    {
        gameObject.SetActive(true);
    }
}
