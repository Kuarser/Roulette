using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UsbTouchDataParser;

public class ScreenModel : BaseManager<ScreenModel>
{
    public enum ScreenMatchMode
    {
        MatchWidth,
        MatchHeight,
        MatchWidthOrHeight
    }

    public const float CheckInternal = 0.1f;
    private const float DefaultCameraSize = 5;

    public List<TouchAdaptation> touchAdaptationList;

    public Dictionary<int, Camera> displayCameraDic;

    public Dictionary<int, TouchPoint> display0TouchPointDic;
    public Dictionary<int, TouchPoint> display1TouchPointDic;

    /// <summary>
    /// 存储已开启监听的设备，deviceId - 屏幕信息 的字典映射
    /// </summary>
    public Dictionary<string, TouchAdaptation> listeningDic = new Dictionary<string, TouchAdaptation>();

    public bool HasTouchAdaptations => touchAdaptationList != null && touchAdaptationList.Count > 0;


    public void InitScreenConfig(List<TouchAdaptationJson> screenJsonConfigs)
    {
        touchAdaptationList = new List<TouchAdaptation>();
        foreach (var jsonConfig in screenJsonConfigs)
        {
            TouchAdaptation touchAdaptation = new TouchAdaptation
            {
                display = jsonConfig.display,
                cullingMask = jsonConfig.cullingMask,
                screenHeight = jsonConfig.screenHeight,
                screenWidth = jsonConfig.screenWidth,
                rawMax = new Vector2(jsonConfig.rawMaxX, jsonConfig.rawMaxY),
                deviceId = jsonConfig.deviceId
            };
            touchAdaptationList.Add(touchAdaptation);
            Debug.LogWarning($"屏幕:Display{touchAdaptation.display},高:{touchAdaptation.screenHeight},宽:{touchAdaptation.screenWidth},rawMax:{touchAdaptation.rawMax}");
        }
        touchAdaptationList.Sort((a, b) => a.display.CompareTo(b.display));
    }

    /// <summary>
    /// 获取到的触摸数据大于实际摄像机数量时,初始化摄像机(本项目暂不处理)
    /// </summary>
    public void InitCamera()
    {
        displayCameraDic = new Dictionary<int, Camera>();
        for (int i = 0; i < Camera.allCameras.Length; i++)
        {
            Camera cam = Camera.allCameras[i];
            displayCameraDic[cam.targetDisplay] = cam;
        }
        if (!HasTouchAdaptations) return;
        if (touchAdaptationList != null && touchAdaptationList.Count > 0 && (touchAdaptationList[0].display != 0 || Camera.main.targetDisplay != 0))
        {
            Debug.LogError("Display 0 must exist and be the main camera.");
            return;
        }
    }

    private Camera CreateNewCamera(int display, int cullingMask, int screenWidth, int screenHeight)
    {
        string camName = $"Display{display + 1}Camera";

        GameObject cameraObject = new GameObject(camName);
        Camera camera = cameraObject.AddComponent<Camera>();

        camera.cullingMask = cullingMask;
        SetCameraResolution(camera, screenWidth, screenHeight, display);

        camera.depth = Camera.allCameras[Camera.allCamerasCount - 1].depth + 1;//1//设置比现有depth最大的摄像机的depth都大1
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.orthographic = true;
        camera.orthographicSize = DefaultCameraSize;
        camera.nearClipPlane = -30;
        camera.farClipPlane = 30;
        camera.useOcclusionCulling = false;

        camera.targetDisplay = display;

#if UNITY_5_4_OR_NEWER
        camera.stereoTargetEye = StereoTargetEyeMask.None;
#endif

#if UNITY_5_6_OR_NEWER
        camera.allowHDR = false;
        camera.allowMSAA = false;
#endif
        return camera;
    }

    private void SetCameraResolution(Camera camera, int screenWidth, int screenHeight, int display)//设置摄像机的分辩率
    {
        if (display == 0)
        {
            SetContentScaleFactor(camera, screenWidth, screenHeight, ScreenMatchMode.MatchHeight);
            return;
        }
    }

    private void SetContentScaleFactor(Camera camera, int screenWidth, int screenHeight, ScreenMatchMode matchMode)
    {
        // 计算缩放因子
        float scaleFactor = 1.0f;

        switch (matchMode)
        {
            case ScreenMatchMode.MatchWidth:
                scaleFactor = (float)Screen.width / screenWidth;
                break;
            case ScreenMatchMode.MatchHeight:
                scaleFactor = (float)Screen.height / screenHeight;
                break;
            case ScreenMatchMode.MatchWidthOrHeight:
                float scaleX = (float)Screen.width / screenWidth;
                float scaleY = (float)Screen.height / screenHeight;
                scaleFactor = Mathf.Min(scaleX, scaleY);
                break;
        }

        // 应用缩放因子到摄像机的正交大小
        camera.orthographicSize = DefaultCameraSize / scaleFactor;
    }


    //计算 touchId（确保唯一性，避免与 Unity 原生 fingerId 0-4 冲突）
    public int GetTouchIdWithDisplay(int display, int touchId)
    {
        return 100 * (1 + display) + touchId;
    }
}
