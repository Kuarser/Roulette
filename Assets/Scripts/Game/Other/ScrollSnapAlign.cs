using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class ScrollSnapAlign : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    public float snapSpeed = 10f;       // 对齐速度
    public float spacing = 200f;        // 元素间距（像素）
    public bool horizontal = true;      // 是否水平滚动
    [HideInInspector]
    public float delayTime = 0.5f;           //延迟时间
    [HideInInspector]
    public int lastCol;//最后显示有露珠的列数


    private ScrollRect scrollRect;
    private RectTransform content;
    //[SerializeField]
    private bool isDragging = false;            //是否正在拖动
    //private bool isAutoMove = false;            //是否正在自动回正
    //private bool CanChangeDragState = false;               //是否能修改拖动状态
    //private Coroutine delayC;
    private Vector2 pos;

    private bool isInit = false;

    void Awake()
    {
        Init();
    }
    void Init()
    {
        if (isInit) return;
        isInit = true;
        scrollRect = GetComponent<ScrollRect>();
        content = scrollRect.content;
        pos = content.anchoredPosition;
        //EventCenter.Instance.AddEventListener("", OnBeginDrag);
        //EventCenter.Instance.AddEventListener("", OnEndDrag);
        InputDriverUGUI.Instance.scrollSnapAlignStartCallBack += OnBeginDrag;
        InputDriverUGUI.Instance.scrollSnapAlignEndCallBack += OnEndDrag;
    }

    //private void Start()
    //{
    //    scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
    //}

    public void RefreshPos(int col)//调整位置※ col=0？
    {
        Init();
        //Debug.Log($"原位置到:{content.anchoredPosition},{content.anchoredPosition.x}");
        if (horizontal)
        {
            //float targetX = Mathf.Round(content.anchoredPosition.x / spacing) * spacing;
            float x = spacing * col * -1;
            content.anchoredPosition = new Vector2(x, content.anchoredPosition.y);
        }
        else
        {
            //float targetY = Mathf.Round(content.anchoredPosition.y / spacing) * spacing;
            float y = spacing * col * -1;
            content.anchoredPosition = new Vector2(content.anchoredPosition.x, y);
        }
        //Debug.Log($"现位置到:{content.anchoredPosition},{content.anchoredPosition.x}");
    }


    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Y))
        //{
        //    Debug.Log($"移动到第{lastCol}行");
        //    RefreshPos(lastCol);
        //}

        // 拖拽中不吸附
        if (isDragging) return;

        //isAutoMove = true;
        // 自动吸附到最近的元素
        if (horizontal)
        {
            float targetX = Mathf.Round(content.anchoredPosition.x / spacing) * spacing;
            content.anchoredPosition = new Vector2(
                Mathf.Lerp(content.anchoredPosition.x, targetX, Time.deltaTime * snapSpeed),
                content.anchoredPosition.y
            );
            //if (content.anchoredPosition.x >= targetX - 0.5f && content.anchoredPosition.x <= targetX + 0.5f)
            //{
            //    isAutoMove = false;
            //}
        }
        else
        {
            float targetY = Mathf.Round(content.anchoredPosition.y / spacing) * spacing;
            content.anchoredPosition = new Vector2(
                content.anchoredPosition.x,
                Mathf.Lerp(content.anchoredPosition.y, targetY, Time.deltaTime * snapSpeed)
            );
            //if (content.anchoredPosition.y >= targetY - 0.5f && content.anchoredPosition.y <= targetY + 0.5f)
            //{
            //    isAutoMove = false;
            //}
        }
        //Debug.LogError($"真正位置到:{content.anchoredPosition},{content.anchoredPosition.x}");
    }
    //void OnDestroy()
    //{
    //    // 解绑事件，防止内存泄漏
    //    if (scrollRect != null)
    //    {
    //        scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
    //    }
    //}




    /// <summary>
    /// 滚动位置变化时调用
    /// </summary>
    //private void OnScrollValueChanged(Vector2 pos)
    //{
    //    // 这里可以做实时对齐或其他逻辑
    //    Debug.Log($"水平滚动: {pos.x:F2}, 垂直滚动: {pos.y:F2}");

    //if (isAutoMove) { Debug.LogError("现在真正自动对齐,"); return; }
    ////拖动后1秒内重新拖动重新延迟
    //////方法一：
    ////CancelInvoke(nameof(OnEndDrag));
    ////Invoke(nameof(OnEndDrag), delayTime);
    ////方法二:
    //if (delayC != null)
    //    StopCoroutine(delayC);
    //delayC = StartCoroutine(OnEndDragIE());
    ////方法三:
    ////time 计时器
    //}

    //IEnumerator OnEndDragIE()
    //{
    //    yield return new WaitForSeconds(delayTime);
    //    OnEndDrag();
    //}

    // 拖拽开始
    public void OnBeginDrag()
    {
        isDragging = true;
    }

    // 拖拽结束
    public void OnEndDrag()
    {
        isDragging = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //Debug.Log("拖拽中," + eventData.pointerDrag.name);
        OnBeginDrag();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //Debug.Log("结束拖拽," + eventData.pointerDrag.name);
        OnEndDrag();
    }
}
