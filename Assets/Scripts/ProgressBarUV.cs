using UnityEngine;
using UnityEngine.UI;

public class ProgressBarUV : MonoBehaviour
{
    [Tooltip("滑动条")]
    public Slider slider;
    [Tooltip("滑动条 平铺填补图片")]
    public RawImage barRawImage;
    [Tooltip("滑动条 进度")]
    public Text sliderProgressText;
    public float barSpeed = 1.2f;
    public float step = 0.001f;
    private float target;

    public void SetProgress(float progress, bool forceRefresh = false)
    {
        if (forceRefresh)
        {
            target = progress;
            slider.value = progress;
            sliderProgressText.text = progress.ToString() + "%";
        }
        else
            target = progress;
    }

    private void Update()
    {
        if (slider.value < target)
        {
            slider.value += step;
            sliderProgressText.text = ((int)(slider.value * 100)).ToString() + "%";
        }
        Rect uvRect = barRawImage.uvRect;
        uvRect.x -= barSpeed * Time.deltaTime;
        barRawImage.uvRect = uvRect;
    }

}
