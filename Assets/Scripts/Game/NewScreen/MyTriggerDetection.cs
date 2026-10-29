using UnityEngine;
using UnityEngine.Events;
using static UsbTouchDataParser;

public class MyTriggerDetection : MonoBehaviour
{
    private RectTransform rect;

    private float checkTimer;
    private bool hadEnter;
    //额,不知道怎么给别人用,就写到model 单例里面去了
    //private bool canEnter;//现在能压住吗?如果是其他界面在上方,就不能压住;如果是当前界面在上方,就能压住

    public UnityAction pointerEnter;
    public UnityAction pointerExit;

    private void Awake()
    {
        //canEnter = true;
        hadEnter = false;
        rect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        checkTimer += Time.deltaTime;
        if (checkTimer >= ScreenModel.CheckInternal)
        {
            checkTimer = 0;
            if (Model.Instance.myTriggerDetectionCanEnterCount != 0)
            {
                //Debug.Log($"Model.Instance.myTriggerDetectionCanEnterCount:{Model.Instance.myTriggerDetectionCanEnterCount}");
                if (hadEnter)
                    ExecutePointerExit();
                return;
            }
            //Model.Instance.myTriggerDetectionCanEnterCount = 0;
            bool isIn = false;
            //Debug.LogError("屏幕1触摸点数量:" + ScreenModel.Instance.display0TouchPointDic.Values.Count + ",屏幕2触摸点数量:" + ScreenModel.Instance.display1TouchPointDic.Values.Count);
            foreach (TouchPoint touchPoint in ScreenModel.Instance.display1TouchPointDic.Values)
            {
                //Debug.Log($"1屏:触摸点:{touchPoint.pos}");
                if (touchPoint.phase == TouchPhase.Canceled) continue;
                if (IsScreenPointInUI(rect, touchPoint.pos, ScreenModel.Instance.displayCameraDic[1]) && touchPoint.phase != TouchPhase.Ended)
                {
                    //Debug.Log($"触摸到了{this.gameObject.name}");
                    isIn = true;
                    break;
                }
            }
            if (isIn)
                ExecutePointerEnter();
            else
                ExecutePointerExit();
        }
    }

    public void ExecutePointerEnter()
    {
        if (pointerEnter != null && !hadEnter)
        {
            //Debug.Log("能执行?");
            pointerEnter?.Invoke();
            hadEnter = true;
        }
    }

    public void ExcutePointerClick()
    {

    }

    public void ExecutePointerPressing()
    {

    }

    public void ExecutePointerLongPress()
    {

    }

    public void ExecutePointerExit()
    {
        if (pointerExit != null && hadEnter)
        {
            pointerExit?.Invoke();
            hadEnter = false;
        }
    }

    /// <summary>
    /// 判断屏幕坐标点是否在指定 RectTransform 内
    /// </summary>
    public bool IsScreenPointInUI(RectTransform rectTransform, Vector2 screenPoint, Camera uiCamera = null)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPoint, uiCamera);
    }

    //public void SetCanEnter(bool can)
    //{
    //    canEnter = can;
    //}
}
