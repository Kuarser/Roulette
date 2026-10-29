using UnityEngine;

/// <summary>
/// 屏幕触摸适配数据实体类
/// 数据库存储 屏幕配置-触摸设备 关联信息
/// </summary>
public class TouchAdaptation
{
    public int display { get; set; }//显示器编号//自定义的的,不是camera那样的targetDisplay

    public int cullingMask { get; set; }//渲染指定层级

    public int screenWidth { get; set; }//屏幕宽度
    public int screenHeight { get; set; }//屏幕高度

    public string deviceId { get; set; }//触摸设备 ID

    public Vector2 rawMax { get; set; }//触摸 Raw 最大值,触摸屏设备原始像素点最大值

}


public class TouchAdaptationJson
{
    public int display;

    public int cullingMask;

    public int screenWidth;

    public int screenHeight;

    public int rawMaxX;

    public int rawMaxY;

    public string deviceId;
}