using UnityEngine;
using UnityEngine.UI;

class TouchInfoData
{
    public TouchInfo touchInfo;
    public UsbTouchDataParser.TouchPoint touchPoint;
}

/// <summary>
/// 触摸(按下)时的信息,一帧,不变
/// </summary>
public class TouchInfo
{

    //新增加的变量
    public Vector3 tempOffPos;//临时位置偏移量
    public Vector3 scrollViewFirstClickPos;//第一次滚动列表区域按下的位置//可以改成 按下时坐标位置
    public GameObject touchBegan;//按下时选中的物体
    public Camera curCamera;//按下时的事件响应摄像机

    public Vector2 localPoint2;//触摸点 点击位置转换为 ScrollBar 的本地坐标
    public Scrollbar tempHitUIParentScrollbar = null;
    public ScrollRect tempHitUIParentScrollRect = null;
    public Slider tempHitUIParentSlider = null;
    public MyButton tempHitUIMyButton = null;

    /**********************************************************/

    /// <summary>
    /// 触摸的唯一标识符
    /// </summary>
    public int TouchId { get; set; }

    /// <summary>
    /// 当前触摸点的 X 坐标
    /// </summary>
    public float X { get; set; }

    /// <summary>
    /// 当前触摸点的 Y 坐标
    /// </summary>
    public float Y { get; set; }

    /// <summary>
    /// 触摸是否已经开始
    /// </summary>
    public bool Began { get; set; }

    /// <summary>
    /// 当前触摸的按钮（例如鼠标左键0、右键1、中键2、没有键-1）
    /// </summary>
    public int Button { get; set; }

    /// <summary>
    /// 当前触摸的目标对象
    /// </summary>
    public GameObject hitUI { get; set; }

    /// <summary>
    /// 上一次触摸的目标对象
    /// </summary>
    public GameObject exHitUI { get; set; }



    public TouchInfo()
    {
        TouchId = -1; // 默认未分配
    }

    /// <summary>
    /// 开始触摸
    /// </summary>
    public void Begin()
    {
        Began = true;
    }

    /// <summary>
    /// 结束触摸
    /// </summary>
    public void End()
    {
        Began = false;
    }

    /// <summary>
    /// 更新触摸事件
    /// </summary>
    public void UpdateEvent()
    {
        // 更新事件数据（根据具体需求实现）
    }

    /// <summary>
    /// 处理触摸移动
    /// </summary>
    public void Move()
    {
        // 处理移动逻辑（根据具体需求实现）
    }

    /// <summary>
    /// 测试是否点击目标对象
    /// </summary>
    /// <returns>返回点击的目标对象</returns>
    public object ClickTest()
    {
        // 实现点击测试逻辑（根据具体需求实现）
        return hitUI;
    }

    public override string ToString()
    {
        return $"TouchInfo: touchId={TouchId}, pos=({X},{Y}), began={Began}, button={Button}";
    }
}