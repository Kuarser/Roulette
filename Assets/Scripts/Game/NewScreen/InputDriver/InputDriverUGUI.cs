using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static UsbTouchDataParser;



public class InputDriverUGUI : BaseManager<InputDriverUGUI>
{
    Camera curCamera;//当前事件响应摄像机
    GameObject curHitUI;
    Canvas curMaxSortOrderCanvas;//最高层级的实时命中物体所在的canvas
    Canvas[] canvases;

    //private int touchesCount = 0;
    private TouchInfo[] touches;
    private int touchesTotalCount;//一个屏幕10个触摸点,一般双屏20个触摸点足够用了

    private Dictionary<int, TouchInfoData> touchInfoDic;
    private List<int> activeTouchIdList;//存储的是编号,激活的触摸点编号列表,编号对应 displayDataMap 的 key

    public System.Action scrollSnapAlignStartCallBack;
    public System.Action scrollSnapAlignEndCallBack;

    public void Init()
    {
        //触摸点信息有:触摸点信息1:ScreenModel.Instance.touchPointDic  和  触摸点信息2:touchInfoDic;应该是冗余的,后续可以优化掉一个//TODO:

        // 初始化 touchPointDic 初始20个触摸点位 

        //ScreenModel.Instance.display0TouchPointDic = Enumerable.Range(64, 10)//后台设置 用大屏是64-74;
        //    .ToDictionary(id => id, id => new TouchPoint { id = id, phase = TouchPhase.Canceled });
        ScreenModel.Instance.display0TouchPointDic = Enumerable.Range(1, 10)//后台设置 用小屏全是1
            .ToDictionary(id => id, id => new TouchPoint { id = id, phase = TouchPhase.Canceled });
        ScreenModel.Instance.display1TouchPointDic = Enumerable.Range(64, 10)
            .ToDictionary(id => id, id => new TouchPoint { id = id, phase = TouchPhase.Canceled });

        touchesTotalCount = 20;//一个屏幕10个触摸点,一般双屏20个触摸点足够用了
        touches = new TouchInfo[touchesTotalCount];
        for (int i = 0; i < touches.Length; i++)
            touches[i] = new TouchInfo();

        touchInfoDic = new Dictionary<int, TouchInfoData>();//触摸点信息2
        activeTouchIdList = new List<int>();

        canvases = GameObject.FindObjectsOfType<Canvas>();
        curHitUI = null;
    }

    private void TriggerSliderHandleMove(Slider slider, Vector2 clickPosition, Camera curCamera)
    {
        // 获取 Slider 的 RectTransform
        RectTransform sliderRect = slider.GetComponent<RectTransform>();

        // 将点击位置转换为 Slider 的本地坐标
        Vector2 localPoint;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(sliderRect,
            clickPosition, curCamera, out localPoint))
            return;

        // 计算 Slider 的值
        float normalizedValue;
        if (slider.direction == Slider.Direction.LeftToRight || slider.direction == Slider.Direction.RightToLeft)
        {
            normalizedValue = Mathf.Clamp01((localPoint.x - sliderRect.rect.xMin) / sliderRect.rect.width);
            if (slider.direction == Slider.Direction.RightToLeft)
                normalizedValue = 1 - normalizedValue;
        }
        else
        {
            normalizedValue = Mathf.Clamp01((localPoint.y - sliderRect.rect.yMin) / sliderRect.rect.height);
            if (slider.direction == Slider.Direction.TopToBottom)
                normalizedValue = 1 - normalizedValue;
        }

        // 设置 Slider 的值
        float newValue = Mathf.Lerp(slider.minValue, slider.maxValue, normalizedValue);
        slider.value = newValue;

        // 触发 onValueChanged 事件
        slider.onValueChanged.Invoke(newValue);
    }


    /// <summary>
    /// 滚动列表拖动
    /// </summary>
    /// <param name="scrollRect">滚动列表</param>
    /// <param name="clickPosition">实时坐标点</param>
    /// <param name="originPosition">起源坐标点</param>
    private void TriggerScrollViewMove(ScrollRect scrollRect, Vector2 clickPosition, Vector3 offPosition, Camera curCamera)
    {
        if (scrollRect == null)
        {
            return;
        }

        // 获取 ScrollRect 的 RectTransform
        RectTransform scrollRectTransform = scrollRect.GetComponent<RectTransform>();
        RectTransform viewport = scrollRect.viewport != null ? scrollRect.viewport : scrollRectTransform;
        RectTransform content = scrollRect.content != null ? scrollRect.content : scrollRectTransform;



        // 将点击位置转换为 ScrollRect 的本地坐标
        Vector2 localPoint;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(scrollRectTransform, clickPosition, curCamera, out localPoint))
        {
            return;
        }

        if (!scrollRect.vertical)
        {
            offPosition.y = 0;
            localPoint.y = 0;
        }
        if (!scrollRect.horizontal)
        {
            offPosition.x = 0;
            localPoint.x = 0;
        }

        content.localPosition = new Vector3(localPoint.x + offPosition.x, localPoint.y + offPosition.y, 0);

    }

    private void TriggerScrollBarMove(Scrollbar scrollBar, Vector2 clickPosition, Vector2 officePosition, Camera curCamera)
    {
        // 获取 scrollBar 的 RectTransform
        RectTransform scrollRect = scrollBar.GetComponent<RectTransform>();

        // 将点击位置转换为 scrollBar 的本地坐标
        Vector2 localPoint;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(scrollRect, clickPosition, curCamera, out localPoint))
        {
            return;
        }


        // 计算 Scrollbar 的值
        float normalizedValue;
        if (scrollBar.direction == Scrollbar.Direction.LeftToRight || scrollBar.direction == Scrollbar.Direction.RightToLeft)//如果是水平滚动条
        {
            normalizedValue = Mathf.Clamp01((localPoint.x - scrollRect.rect.xMin) / scrollRect.rect.width);
            if (scrollBar.direction == Scrollbar.Direction.RightToLeft)
            {
                normalizedValue = 1 - normalizedValue;
            }
        }
        else
        {
            //垂直滚动条
            normalizedValue = Mathf.Clamp01((localPoint.y - scrollRect.rect.yMin) / scrollRect.rect.height);
            if (scrollBar.direction == Scrollbar.Direction.TopToBottom)
            {
                normalizedValue = 1 - normalizedValue;
            }
        }

        // 设置 scrollBar 的值
        float newValue = Mathf.Lerp(0, 1, normalizedValue);
        scrollBar.value = newValue;

        // 触发 onValueChanged 事件
        scrollBar.onValueChanged.Invoke(newValue);
    }


    public void SetInput(TouchPoint curTouchPoint, Camera targetCamera)
    {
        if (targetCamera == null) return;
        int targetDisplay = targetCamera.targetDisplay;

        curHitUI = null;
        foreach (var canvas in canvases)//canvases 只能在构造函数获取一次,不能每次SetInput 都获取,会掉帧,所以必须canvas 数量固定,并且一开始都获取好
        {
            if (!canvas.isActiveAndEnabled || canvas.worldCamera != targetCamera) continue;
            var tempHitUI = UGUIRaycastByCanvasFound(canvas, targetCamera, curTouchPoint.pos);
            if (tempHitUI == null) continue;
            if (curHitUI != null && curMaxSortOrderCanvas.sortingOrder >= canvas.sortingOrder)
                continue;
            curHitUI = tempHitUI;
            curMaxSortOrderCanvas = canvas;
        }
        int touchId = curTouchPoint.id;

        TouchInfo touchInfo = GetOrAllocateTouchInfo(touchId);
        if (touchInfo == null) return;

        touchInfo.exHitUI = touchInfo.hitUI;//上一个触摸对象
        touchInfo.hitUI = curHitUI;//当前触摸对象

        curCamera = targetCamera;

        #region 根据激活触摸点数id 来设置触摸点坐标
        if (!touchInfoDic.TryGetValue(touchId, out TouchInfoData touchInfoData))
        {
            touchInfoData = new TouchInfoData();
            touchInfoDic[touchId] = touchInfoData;
        }
        touchInfoData.touchInfo = touchInfo;
        touchInfoData.touchPoint = curTouchPoint;

        if (!activeTouchIdList.Contains(touchId))
            activeTouchIdList.Add(touchId);
        #endregion

        HandleInput();
    }

    /// <summary>
    /// 处理输入
    /// </summary>
    public void HandleInput()
    {
        for (int i = activeTouchIdList.Count - 1; i >= 0; i--)
        {
            int touchId = activeTouchIdList[i];
            TouchInfoData touchInfoData = touchInfoDic[touchId];

            HandleSingleTouchInput(touchInfoData);

            // 如果触摸结束，从活跃列表移除
            if (touchInfoData.touchPoint.phase == TouchPhase.Ended)
                activeTouchIdList.RemoveAt(i);
        }
    }

    /// <summary>
    /// 单个触摸点的UI处理
    /// </summary>
    /// <param name="touchInfoData"></param>
    private void HandleSingleTouchInput(TouchInfoData touchInfoData)
    {
        TouchInfo touchInfo = touchInfoData.touchInfo;
        TouchPoint touchPoint = touchInfoData.touchPoint;
        GameObject hitUI = touchInfo.hitUI;

        if (hitUI == null) return;

        switch (touchPoint.phase)
        {
            case TouchPhase.Began:
                touchInfo.curCamera = curCamera;//存储按下时的摄像机
                touchInfo.scrollViewFirstClickPos = touchPoint.pos;//存储按下时坐标位置
                touchInfo.touchBegan = hitUI;

                #region 处理 MyButton
                touchInfo.tempHitUIMyButton = hitUI.GetComponent<MyButton>();
                if (touchInfo.tempHitUIMyButton != null)
                {
                    touchInfo.tempHitUIMyButton.OnPointerDown();
                    break;
                }
                #endregion

                #region 处理 Slider
                touchInfo.tempHitUIParentSlider = hitUI.GetComponentInParent<Slider>();
                if (touchInfo.tempHitUIParentSlider != null)
                {
                    TriggerSliderHandleMove(touchInfo.tempHitUIParentSlider, touchPoint.pos, touchInfo.curCamera);
                    break;
                }
                #endregion

                #region 处理 ScrollView
                touchInfo.tempHitUIParentScrollRect = hitUI.GetComponentInParent<ScrollRect>();
                if (touchInfo.tempHitUIParentScrollRect != null)
                {
                    scrollSnapAlignStartCallBack?.Invoke();//点击到ScrollView ,可能会拖动,先关闭scrollView 自动对齐
                    var tempScrollRect = touchInfo.tempHitUIParentScrollRect.GetComponent<RectTransform>();
                    var tempContent = touchInfo.tempHitUIParentScrollRect.content;

                    if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(tempScrollRect,
                        touchInfo.scrollViewFirstClickPos, touchInfo.curCamera, out touchInfo.localPoint2))
                        Debug.LogWarning("无法将点击位置转换为 ScrollRect 的本地坐标");

                    touchInfo.tempOffPos = new Vector3(tempContent.localPosition.x - touchInfo.localPoint2.x, tempContent.localPosition.y - touchInfo.localPoint2.y, 0);
                }
                #endregion

                break;
            case TouchPhase.Moved:
                //处理 MyButton
                var myButton = touchInfo.tempHitUIMyButton;
                if (myButton != null && myButton != hitUI)
                {
                    myButton.OnPointerUp();
                }
                //处理 slider
                var slider = touchInfo.tempHitUIParentSlider;
                if (slider != null)
                {
                    TriggerSliderHandleMove(slider, touchPoint.pos, touchInfo.curCamera);
                    break;
                }

                //处理 ScrollView(ScrollRect)
                var scrollRect = touchInfo.tempHitUIParentScrollRect;
                if (scrollRect != null)
                {
                    var scrollBar = touchInfo.tempHitUIParentScrollbar;
                    if (scrollBar != null)
                        TriggerScrollBarMove(scrollBar, touchPoint.pos, touchInfo.tempOffPos, touchInfo.curCamera);//处理 scrollBar
                    else
                        TriggerScrollViewMove(scrollRect, touchPoint.pos, touchInfo.tempOffPos, touchInfo.curCamera);
                    break;
                }
                break;
            case TouchPhase.Ended:
                if (hitUI == touchInfo.touchBegan)
                {
                    //处理 Button
                    var button = hitUI.GetComponent<Button>();
                    if (button != null)
                        button.onClick.Invoke();
                    else
                    {
                        //后台系统的button
                        var buttonParent = hitUI.transform.parent.GetComponent<Button>();//后台系统都是用的父节点button,子节点图片的
                        if (buttonParent != null)
                        {
                            IOCanvasModel.Instance.isTouchClick = true;
                            IOCanvasModel.Instance.switchTouchPointPos = touchPoint.pos;
                            IOCanvasModel.Instance.dateTimeTouchPointPos = touchPoint.pos;
                            buttonParent?.onClick.Invoke();
                        }
                    }

                    //处理Toggle
                    var toggleParent = hitUI.transform.parent.GetComponent<Toggle>();//后台系统使用
                    if (toggleParent != null)
                        toggleParent.isOn = !toggleParent.isOn;

                    //处理 MyButton
                    myButton = hitUI?.GetComponent<MyButton>();
                    myButton?.OnPointerUp();

                }
                //处理 ScrollView 
                scrollSnapAlignEndCallBack?.Invoke();//拖动结束后开启ScrollView 自动对齐

                //释放触摸点,重置触摸点数据
                touchInfo.Button = -1;
                touchInfo.TouchId = -1;
                touchInfo.hitUI = null;
                touchInfo.touchBegan = null;
                touchInfo.tempHitUIParentSlider = null;
                touchInfo.tempHitUIParentScrollRect = null;
                break;
            case TouchPhase.Canceled:
                Debug.LogError("TouchPhase.Canceled状态未完善但已触发,如果该操作不手动执行重启请检查代码");
                break;
        }
    }

    /// <summary>
    /// 获取一个 canvas 上点击位置的最上层 UI 元素
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="eventCamera"></param>
    /// <param name="pointerPosition"></param>
    /// <returns></returns>
    GameObject UGUIRaycastByCanvasFound(Canvas canvas, Camera eventCamera, Vector2 pointerPosition)
    {
        var foundGraphics = GraphicRegistry.GetRaycastableGraphicsForCanvas(canvas);
        if (foundGraphics == null || foundGraphics.Count == 0)
            return null;
        List<Graphic> graphicList = new List<Graphic>();
        for (int i = 0; i < foundGraphics.Count; i++)
        {
            Graphic graphic = foundGraphics[i];
            if (!graphic.raycastTarget || graphic.canvasRenderer.cull || graphic.depth == -1)
                continue;

            if (!RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, pointerPosition, eventCamera, graphic.raycastPadding))
                continue;


            if (eventCamera != null && eventCamera.WorldToScreenPoint(graphic.rectTransform.position).z > eventCamera.farClipPlane)
                continue;

            if (graphic.Raycast(pointerPosition, eventCamera))
                graphicList.Add(graphic);
        }
        graphicList.Sort((g1, g2) => g2.depth.CompareTo(g1.depth));
        GameObject temp = graphicList.Count > 0 ? graphicList[0].gameObject : null;
        return temp;
    }



    /// <summary>
    /// 获取或分配 Stage._touches 槽位
    /// </summary>
    private TouchInfo GetOrAllocateTouchInfo(int touchId)
    {
        TouchInfo stageTouch = null;

        // 查找是否已有该 touchId
        for (int j = 0; j < touches.Length; j++)
        {
            if (touches[j].TouchId == touchId)
            {
                stageTouch = touches[j];
                return stageTouch;
            }
        }

        // 如果没有找到，找空闲槽位（touchId == -1）
        for (int j = 0; j < touches.Length; j++)
        {
            if (touches[j].TouchId == -1)
            {
                stageTouch = touches[j];
                stageTouch.TouchId = touchId;
                return stageTouch;
            }
        }

        return null;
    }
}