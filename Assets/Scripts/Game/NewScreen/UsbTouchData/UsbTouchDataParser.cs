using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UsbTouchDataParser;

public class UsbTouchDataParser
{
    [System.Serializable]
    public class TouchPoint
    {
        /// <summary>
        /// 点击坐标
        /// </summary>
        public Vector2 pos = Vector2.zero;

        /// <summary>
        /// 按下/抬起
        /// </summary>
        public bool isTouching;

        /// <summary>
        /// 触摸状态
        /// </summary>
        public TouchPhase phase;

        /// <summary>
        /// 触摸点ID
        /// </summary>
        public int id;

        /// <summary>
        /// 触摸点所在的 usb 设备ID
        /// </summary>
        public string deviceId;

        /// <summary>
        /// 
        /// </summary>
        public int display;
    }


    /// <summary>
    /// 直接将 byte[] 转换为字符串显示
    /// </summary>
    public string ByteArrayToHex(byte[] data)
    {
        return System.BitConverter.ToString(data).Replace("-", " ");
    }



    #region new
    public Dictionary<int, TouchPoint> ParseAllTouchData(byte[] data, TouchAdaptation adaptation, bool invertX, bool invertY)
    {

        if (data == null
           //|| data.Length < 55//55 是大屏幕的长度,52是小屏幕的长度
           || adaptation == null ||
           adaptation.rawMax == default || adaptation.rawMax.x <= 0f || adaptation.rawMax.y <= 0f)
        {
            if (data == null)
            {
                Debug.Log("data 为空");
            }
            if (data.Length < 55)
            {
                Debug.Log($"data 长度不足: {data.Length}");
            }
            if (adaptation == null)
            {
                Debug.Log("adaptation 为空");
            }
            else if (adaptation.rawMax == default || adaptation.rawMax.x <= 0f || adaptation.rawMax.y <= 0f)
            {
                Debug.Log($"adaptation rawMax 数据异常: {adaptation.rawMax}");
            }
            Debug.LogError("Call ParseAllTouchData Return");
            return new Dictionary<int, TouchPoint>();
        }
        // 数据布局：
        // [0] ReportID(0x04, 可忽略)
        // [1] Flags（0x40=按下/在触摸；0x00=抬起）
        // [2..3] X（小端 u16，需屏蔽最高位）
        // [4..5] Y（小端 u16）
        // [6] Flags（0x41=按下/在触摸；0x00=抬起）
        // [7..8] X（小端 u16，需屏蔽最高位）
        // [9..10] Y（小端 u16）
        // 一直重复

        int curTouchPointCount = data.Length > 55 ? data[55] : 1;//不是55 的,可能用不到多点了,所以不用存一共有的触摸点个数
        int startIndex = 1;//data 触摸点 起始下标
        int interval = 5;//data 一个触摸点 数据间隔(长度:1字节 判断是否按下,4字节 存储坐标)
        int oneTouchIndex = 1;//一个触摸点 的累加
        Dictionary<int, TouchPoint> touchPointDic = new Dictionary<int, TouchPoint>();//所有触摸点

        int[] values = Array.ConvertAll(data, value => (int)value);

        #region 旧款触摸控件
        //if (data.Length == 52 && data[startIndex] == 0x01)//小屏幕
        //{
        //    if (data[51] < 1)
        //    {
        //        return touchPointDic;
        //    }

        //    TouchPoint touchPoint = new TouchPoint();
        //    int id = data[1];
        //    touchPoint.id = data[1];
        //    touchPoint.isTouching = data[1] != 0;

        //    int xRaw = (int)(data[3] | (data[4] << 8));
        //    int y = (int)(data[5] | (data[6] << 8));
        //    int x = xRaw & 0x7FFF;
        //    touchPoint.pos = ToUnityPosition(new Vector2(x, y), adaptation, invertX, invertY);
        //    touchPointDic[id] = touchPoint;
        //    return touchPointDic;
        //}
        #endregion

        if (data.Length == 54 && (data[1] == 0x10 || data[1] == 0x11))//小屏幕
        {
            if (data[1] == 0x10 || data[53] < 1)
            {
                //0x10 表示抬起，返回空字典以通知本地触摸点结束
                return touchPointDic;
            }

            TouchPoint touchPoint = new TouchPoint();
            int id = 1;
            touchPoint.id = id;
            touchPoint.isTouching = true;

            int rawX = data[2] | (data[3] << 8);
            int rawY = (data[4] | (data[5] << 8));
            touchPoint.pos = ToUnityPosition(new Vector2(rawX, rawY), adaptation, invertX, invertY);
            touchPointDic[id] = touchPoint;
            return touchPointDic;
        }



        for (int i = 0; i < curTouchPointCount; i++)
        {
            if (data[startIndex] < 0x40 || data[startIndex] > 0x49)
            {
                //Debug.Log("触摸数据不是大屏的或者是大屏触摸结束,返回空");
                return new Dictionary<int, TouchPoint>();
            }
            TouchPoint touchPoint = new TouchPoint();
            int id = data[startIndex];
            touchPoint.id = id;
            touchPoint.isTouching = id != 0;

            oneTouchIndex = startIndex;
            int xRaw = (int)(data[++oneTouchIndex] | (data[++oneTouchIndex] << 8));
            int y = (int)(data[++oneTouchIndex] | (data[++oneTouchIndex] << 8));
            int x = xRaw & 0x7FFF;
            touchPoint.pos = ToUnityPosition(new Vector2(x, y), adaptation, invertX, invertY);
            startIndex += interval;
            touchPointDic[id] = touchPoint;
        }
        return touchPointDic;
    }

    #endregion

    /// <summary>
	/// 将设备原始坐标映射为 Unity 屏幕像素坐标。
	/// 输入raw为原始坐标（TouchPoint.pos），输出为屏幕像素。
	/// </summary>
    private Vector2 ToUnityPosition(Vector2 raw, TouchAdaptation adaptation, bool invertX, bool invertY)
    {
        //设备原始坐标是从左上角开始的, Unity屏幕坐标是从左下角开始的
        float rawX = raw.x;
        float rawY = raw.y;

        //设备原始坐标的最大值
        float maxX = adaptation.rawMax.x;
        float maxY = adaptation.rawMax.y;

        // 判断是否为竖屏（高度大于宽度）
        bool isPortrait = adaptation.screenHeight > adaptation.screenWidth;

        /* 原始坐标点:
         * 竖屏,,是横屏向右转90度
         * 竖屏时，原生触摸点一般以(横屏左上角)竖屏右上角为原点，需要转换为(横屏左下角)竖屏左上角为原点
         * 需要交换 X 和 Y 坐标
         * 因为竖屏设备的数据中，X和Y的位置是交换的
         *
         *
         *屏幕坐标点:
         * 横屏时，原生触摸点一般以左下角为原点，后续走和竖屏一样的转换依旧会以左下角为原点
         */

        // 竖屏时，一般以右上角为原点
        // 
        if (isPortrait)//暂时不处理
        {
            rawX = raw.y;
            rawY = raw.x;

            maxX = adaptation.rawMax.y;
            maxY = adaptation.rawMax.x;

            // 交换后，需要翻转新的X轴（原Y轴）
            rawX = maxX - rawX;
        }

        //Debug.LogWarning($"屏幕: display{adaptation.display}，原生点：({rawX}，{rawY})");

        float nx = maxX > 0 ? Mathf.Clamp01(rawX / maxX) : 0f;
        float ny = maxY > 0 ? Mathf.Clamp01(rawY / maxY) : 0f;


        if (invertX)
        {
            nx = 1f - nx;
        }
        if (invertY)
        {
            ny = 1f - ny;
        }

        // 转换为Unity屏幕坐标，左下角为原点
        Vector2 tempPos = new Vector2(adaptation.screenWidth - (nx * adaptation.screenWidth), ny * adaptation.screenHeight);
        //Debug.LogWarning($"屏幕坐标:{tempPos}");
        return tempPos;
    }
}
