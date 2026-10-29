using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using static UsbTouchDataParser;

public class MyTouchComponent : MonoBehaviour
{
    private float checkTimer;

    private bool isInited = false;

    private Camera curCamera;
    public Camera CurCamera
    {
        get
        {
            if (curCamera == null)
            {
                if (Camera.allCamerasCount > 0)
                    curCamera = Camera.allCameras[0];
            }
            return curCamera;
        }
    } //供划线组件使用,只读

    private Coroutine setLastTouchPointCancelCoroutine;

    private void Start()
    {
        ScreenModel.Instance.InitCamera();
        InitAndroidUsbTouch();
        InputDriverUGUI.Instance.Init();

#if UNITY_ANDROID && !UNITY_EDITOR
        var usbDeviceInfoDtos = GetAllUsbDevices();
        for (int i = 0; i < usbDeviceInfoDtos.Length; i++)
            Debug.LogError($"当前连接的USB设备 {i} : {JsonConvert.SerializeObject(usbDeviceInfoDtos[i])}");
        List<string> deviceIds = GetUsbTouchDeviceIds();
        for (int i = 0; i < deviceIds.Count; i++)
            Debug.LogError($"当前可用的USB触摸设备 {i} : {deviceIds[i]}");
#endif


        if (ScreenModel.Instance.HasTouchAdaptations)
        {
            Debug.Log("存在屏幕配置缓存, 使用缓存配置");
            foreach (var item in ScreenModel.Instance.touchAdaptationList)
            {
                Debug.LogError($"Display{item.display} - 触摸设备：{item.deviceId} 绑定中...");
                StartListeningFormAdaptation(item, item.deviceId);
            }
            isInited = true;
        }
        else
        {
            Debug.Log("无屏幕配置缓存, 添加预设配置");
            StartCoroutine(LoadDefaultScreenConfig());
        }

    }

    private IEnumerator LoadDefaultScreenConfig()
    {
        string path = string.Empty;
        if (Application.isEditor)
            path = Application.dataPath + "/StreamingAssets/screenConfig.json";
        else
            path = Application.persistentDataPath + "/screenConfig.json";
        string localPath = string.Empty;
        if (Application.platform == RuntimePlatform.Android)
            localPath = Application.streamingAssetsPath + "/screenConfig.json";
        else
            localPath = "file:///" + Application.streamingAssetsPath + "/screenConfig.json";
        UnityWebRequest www = UnityWebRequest.Get(localPath);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.ConnectionError
        || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"读取屏幕配置错误:{www.error}");
        }
        else if (www.isDone)
        {
            var data = www.downloadHandler.data;
            string jsonStr = System.Text.Encoding.UTF8.GetString(data);
            File.WriteAllText(path, jsonStr);
            List<TouchAdaptationJson> screenJsonConfigs = JsonConvert.DeserializeObject<List<TouchAdaptationJson>>(jsonStr);
            ScreenModel.Instance.InitScreenConfig(screenJsonConfigs);
        }
    }

    //退出应用时会自动调用
    public void OnDestroy()
    {
        DisposeTouchDevice();
    }

    public void Update()
    {
        InputKeyBoard();

#if UNITY_EDITOR
        InputMouse();
#endif

        checkTimer += Time.deltaTime;
        if (checkTimer >= ScreenModel.CheckInternal && isInited)
        {
            checkTimer = 0;
            foreach (var item in ScreenModel.Instance.listeningDic.Keys)
                LoopCheckAdaptation(item);
        }
    }


    private void OnGUI()
    {
        // 初始化样式
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleLeft
        };

        // 绘制主按钮 停止监听usb
        if (GUI.Button(new Rect(10, 10, 150, 50), "停止監聽usb", buttonStyle))
            CancelBingding();
        // 绘制主按钮 绑定usb和屏幕
        if (GUI.Button(new Rect(160, 10, 150, 50), "綁定usb和屏幕", buttonStyle))
            Bingding();
        // 绘制主按钮 监听所有usb
        if (GUI.Button(new Rect(310, 10, 150, 50), "監聽所有usb", buttonStyle))
            StartAllListening();
        // 绘制主按钮 反向绑定usb和屏幕
        if (GUI.Button(new Rect(460, 10, 150, 50), "反向綁定usb和屏幕", buttonStyle))
            Bingding(true);
    }

    private void OnApplicationQuit()
    {
        CancelBingding();
    }

    public void CancelBingding()
    {
        StopAllListening();
    }

    public void Bingding(bool isReverse = false)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        List<string> deviceIds = GetUsbTouchDeviceIds();
        for (int i = 0; i < deviceIds.Count; i++)
            Debug.LogError($"当前可用的USB触摸设备 {i} : {deviceIds[i]}");
        if (isReverse)
            deviceIds.Reverse();
        int succeedCounter = 0;
        for (int i = 0; i < ScreenModel.Instance.touchAdaptationList.Count; i++)
        {
            if (BindingTouchDevice(ScreenModel.Instance.touchAdaptationList[i], deviceIds[i]))
                succeedCounter++;
        }
        if (succeedCounter == ScreenModel.Instance.touchAdaptationList.Count)
        {
            isInited = true;
            UpdateScreenConfig();
        }
#else
        Debug.Log("编辑器模式下,无法绑定");
#endif
    }
    public void StartAllListening()
    {
        Debug.Log("已经绑定了,现在重新开启监听");
        foreach (var item in ScreenModel.Instance.touchAdaptationList)
        {
            StartListeningFormAdaptation(item, item.deviceId);
        }
    }


    #region 配置管理

    public List<string> GetUsbTouchDeviceIds()
    {
        List<string> deiviceIds = new List<string>();
        UsbDeviceInfoDto[] devcies = GetAllUsbDevices();
        for (int i = 0; i < devcies.Length; i++)
        {
            if (!IsTouchDevice(devcies[i])) continue;
            deiviceIds.Add(devcies[i].deviceId);
        }
        return deiviceIds;
    }
    private bool IsTouchDevice(UsbDeviceInfoDto usbDeviceInfoDto)
    {
        if (usbDeviceInfoDto == null) return false;
        for (int i = 0; i < usbDeviceInfoDto.interfaces.Length; i++)
        {
            if (usbDeviceInfoDto.interfaces[i].interfaceClass == 3)//HID设备
                return true;
        }
        return false;
    }

    /// <summary>
    /// 绑定
    /// 设置屏幕与USB触摸设备的关联映射
    /// </summary>
    /// <param name="display">屏幕编号</param>
    /// <param name="newDeviceId">关联的设备Id，解绑就给空</param>
    /// <returns></returns>
    public bool BindingTouchDevice(TouchAdaptation touchAdaptation, string newDeviceId)
    {
        Debug.Log($"newDeviceId:{newDeviceId}");
        try
        {
            if (touchAdaptation == null)
            {
                Debug.LogError("touchAdaptation is null");
                return false;
            }

            if (touchAdaptation.deviceId == newDeviceId)
            {
                Debug.LogWarning("请勿重复绑定");
                return true;
            }
            string oldDeviceId = touchAdaptation.deviceId;

            if (!string.IsNullOrEmpty(oldDeviceId))
            {
                StopListening(oldDeviceId);
                if (ScreenModel.Instance.listeningDic.ContainsKey(oldDeviceId))
                    ScreenModel.Instance.listeningDic.Remove(oldDeviceId);
            }

            bool listenResult = StartListeningFormAdaptation(touchAdaptation, newDeviceId);
            if (!listenResult) return false;

            Debug.Log($"关联屏幕：Display{touchAdaptation.display} - 触摸设备：{newDeviceId} 绑定成功");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"保存屏幕配置失败：{e}");
            return false;
        }
    }

    private void UpdateScreenConfig()
    {
        string path = string.Empty;
        if (Application.isEditor)
            path = Application.dataPath + "/StreamingAssets/screenConfig.json";
        else
            path = Application.persistentDataPath + "/screenConfig.json";
        List<TouchAdaptationJson> touchAdaptationJsonList = new List<TouchAdaptationJson>();
        for (int i = 0; i < ScreenModel.Instance.touchAdaptationList.Count; i++)
        {
            TouchAdaptationJson touchAdaptationJson = new TouchAdaptationJson
            {
                display = ScreenModel.Instance.touchAdaptationList[i].display,
                cullingMask = ScreenModel.Instance.touchAdaptationList[i].cullingMask,
                screenHeight = ScreenModel.Instance.touchAdaptationList[i].screenHeight,
                screenWidth = ScreenModel.Instance.touchAdaptationList[i].screenWidth,
                rawMaxX = (int)ScreenModel.Instance.touchAdaptationList[i].rawMax.x,
                rawMaxY = (int)ScreenModel.Instance.touchAdaptationList[i].rawMax.y,
                deviceId = ScreenModel.Instance.touchAdaptationList[i].deviceId
            };
            touchAdaptationJsonList.Add(touchAdaptationJson);
        }
        string jsonStr = JsonConvert.SerializeObject(touchAdaptationJsonList);
        File.WriteAllText(path, jsonStr);

        Debug.LogError("初始化监听完成, 并保存配置");
    }

    //根据关联映射开启指定usb设备监听
    private bool StartListeningFormAdaptation(TouchAdaptation adaptation, string deviceId)
    {
        if (string.IsNullOrEmpty(deviceId)) return false;
        var startListeningResult = StartListening(deviceId, adaptation.screenWidth, adaptation.screenHeight);
        if (!startListeningResult.success)
        {
            Debug.LogError($"开启usb触摸监听失败: Display{adaptation.display} - 触摸设备：{adaptation.deviceId} - error：{startListeningResult.error}");
            return false;
        }
        ScreenModel.Instance.listeningDic[deviceId] = adaptation;
        adaptation.deviceId = deviceId;
        Debug.LogError($"开启usb触摸监听成功: Display{adaptation.display} - 触摸设备：{adaptation.deviceId}");
        return true;
    }

    #endregion

    #region 输入处理
    private void InputKeyBoard()
    {
        if (Input.GetKeyDown(KeyCode.F1))
            Bingding();//默认绑定
        if (Input.GetKeyDown(KeyCode.F2))
            StopAllListening();//取消监听
        if (Input.GetKeyDown(KeyCode.F3))
        {
            StopAllListening();
            Bingding(true);//反向绑定
        }
    }

    private int curDisplay = 0;
    Vector3 tempInputMousePos = Vector3.zero;
    private void InputMouse()
    {
        //编辑器下切换display，同一时间只会有一个屏幕处理按键处理
        if (ScreenModel.Instance.displayCameraDic != null && Input.GetKeyDown(KeyCode.RightAlt))
        {
            curDisplay++;
            if (curDisplay >= ScreenModel.Instance.displayCameraDic.Count)
                curDisplay = 0;
            curCamera = ScreenModel.Instance.displayCameraDic[curDisplay];//供编辑器鼠标模拟触摸点使用
            Debug.Log($"使用鼠标模拟触摸点,当前响应屏幕：Display{curDisplay + 1},事件响应摄像机：{curCamera}");
        }
        if (Input.GetMouseButtonDown(0))
        {
            //Debug.Log("鼠标点击 按下");
            InputMouseDriver(true, TouchPhase.Began, curDisplay);
            tempInputMousePos = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            //Debug.Log("鼠标点击 移动");
            if (tempInputMousePos.Equals(Input.mousePosition))
            {
                InputMouseDriver(true, TouchPhase.Stationary, curDisplay);//按下后静止不动
            }
            else
            {
                InputMouseDriver(true, TouchPhase.Moved, curDisplay);//按下后移动
            }
            tempInputMousePos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            //Debug.Log("鼠标点击 弹起");
            InputMouseDriver(false, TouchPhase.Ended, curDisplay);
            tempInputMousePos = Vector3.zero;
        }
    }

    private void InputMouseDriver(bool isDown, TouchPhase touchPhase, int display = 0)
    {
        //鼠标点击转为触摸信息，走触摸处理
        UsbTouchDataParser.TouchPoint touchPoint = new UsbTouchDataParser.TouchPoint()
        {
            pos = Input.mousePosition,
            isTouching = isDown,
            phase = touchPhase,
            id = 1,
            deviceId = "EditorMouse",//一般用来求 display 的,没用
            display = display
        };
        if(display==0)
            ScreenModel.Instance.display0TouchPointDic[0] = touchPoint;//用于 MyTriggerDetection //鼠标模拟的触摸点 一般只有一个,所以只存1
        else
            ScreenModel.Instance.display1TouchPointDic[0] = touchPoint;//用于 MyTriggerDetection //鼠标模拟的触摸点 一般只有一个,所以只存1

        ScreenModel.Instance.displayCameraDic.TryGetValue(display, out Camera cameraEntity);
        InputDriverUGUI.Instance.SetInput(touchPoint, cameraEntity); //editor 鼠标模拟触摸点输入处理

    }
    #endregion



    #region AndroidTouchComponent

    private const string UsbControllerClass = "com.android.systemHelper.usb.UsbController";
    private AndroidJavaClass sUsbControllerClass;

    private UsbDataReceiver receiver;
    private UsbTouchDataParser dataParser;



    private void InitAndroidUsbTouch()
    {
        GameObject go = new GameObject("UsbDataReceiver");
        receiver = go.AddComponent<UsbDataReceiver>();
        receiver.OnReceivedHandle += OnHandleUsbData;

        dataParser = new UsbTouchDataParser();

        var result = Init();
        if (!result)
            Debug.LogError("UsbPlugin init failed ");
    }

    private void DisposeTouchDevice()
    {
        if (Application.platform != RuntimePlatform.Android)
        {
            return;
        }

        Shutdown();

        if (receiver != null && receiver.gameObject != null)
        {
            receiver.OnReceivedHandle -= OnHandleUsbData;
            GameObject.Destroy(receiver.gameObject);
        }

        sUsbControllerClass = null;

        receiver = null;
    }

    /// <summary>
    /// 获取usb 触摸数据
    /// </summary>
    /// <param name="deviceId">usb 输入设备屏幕的名称</param>
    /// <param name="data">usb 输入数据</param>
    private void OnHandleUsbData(string deviceId, byte[] data)
    {
        ScreenModel.Instance.listeningDic.TryGetValue(deviceId, out TouchAdaptation adaptation);
        if (adaptation == null) return;

        if (setLastTouchPointCancelCoroutine != null)
        {
            StopCoroutine(setLastTouchPointCancelCoroutine);
            setLastTouchPointCancelCoroutine = null;
        }

        // 解析触摸点数据
        Dictionary<int, TouchPoint> curTouchPointDic = dataParser.ParseAllTouchData(data, adaptation, true, true);

        if (adaptation.display == 0)
        {
            foreach (TouchPoint touchPoint in ScreenModel.Instance.display0TouchPointDic.Values)
            {
                touchPoint.deviceId = deviceId;
                touchPoint.display = ScreenModel.Instance.listeningDic[deviceId].display;
                UpdateTouchPointPhase(touchPoint, curTouchPointDic);//用传入数据更新本地触摸点的状态
            }
        }
        else
        {
            foreach (TouchPoint touchPoint in ScreenModel.Instance.display1TouchPointDic.Values)
            {
                touchPoint.deviceId = deviceId;
                touchPoint.display = ScreenModel.Instance.listeningDic[deviceId].display;
                UpdateTouchPointPhase(touchPoint, curTouchPointDic);//用传入数据更新本地触摸点的状态
            }
        }

        if (curTouchPointDic.Count == 1 && !curTouchPointDic.Values.ElementAt(0).isTouching)
            setLastTouchPointCancelCoroutine = StartCoroutine(SetLastTouchPointCancel(adaptation.display));
    }

    private IEnumerator SetLastTouchPointCancel(int display)
    {
        yield return null;
        if (display == 0)
            foreach (var touchPoint in ScreenModel.Instance.display0TouchPointDic.Values)
                touchPoint.phase = TouchPhase.Canceled;
        else
            foreach (var touchPoint in ScreenModel.Instance.display1TouchPointDic.Values)
                touchPoint.phase = TouchPhase.Canceled;
        setLastTouchPointCancelCoroutine = null;
    }

    private void LoopCheckAdaptation(string deviceId)
    {
        ScreenModel.Instance.listeningDic.TryGetValue(deviceId, out TouchAdaptation adaptation);
        if (adaptation == null)
        {
            Debug.LogError("adaptation can not be null");
            return;
        }

        int display = adaptation.display;

        ScreenModel.Instance.displayCameraDic.TryGetValue(display, out Camera cameraEntity);
        if (cameraEntity == null)
        {
            Debug.LogError($"未找到Display{display}对应的相机");
            return;
        }
        else
            curCamera = cameraEntity;

        foreach (TouchPoint touchPoint in ScreenModel.Instance.display0TouchPointDic.Values)
        {
            if (touchPoint.phase != TouchPhase.Moved
                || string.IsNullOrEmpty(touchPoint.deviceId)
                || !touchPoint.deviceId.Equals(deviceId)) continue;
            InputDriverUGUI.Instance.SetInput(touchPoint, cameraEntity);//move事件持续触发
        }

        foreach (TouchPoint touchPoint in ScreenModel.Instance.display1TouchPointDic.Values)
        {
            if (touchPoint.phase != TouchPhase.Moved
                || string.IsNullOrEmpty(touchPoint.deviceId)
                || !touchPoint.deviceId.Equals(deviceId)) continue;
            InputDriverUGUI.Instance.SetInput(touchPoint, cameraEntity);//move事件持续触发
        }
    }


    /// <summary>
    /// 根据usb 触摸点数据 更新本地的触摸点字典
    /// 注:以下关于pos的赋值位置不要轻易改动
    /// 更不要在ended中给pos赋默认值,会导致抬起事件位置错误
    /// </summary>
    /// <param name="touchPoint">本地的触摸点字典的一个触摸点,更新的</param>
    /// <param name="curTouchPointDic">usb 所有触摸点,对比的</param>
    private void UpdateTouchPointPhase(TouchPoint touchPoint, Dictionary<int, TouchPoint> curTouchPointDic)
    {
        if (curTouchPointDic.ContainsKey(touchPoint.id))
        {
            var curPoint = curTouchPointDic[touchPoint.id];
            touchPoint.isTouching = true;

            if (touchPoint.phase == TouchPhase.Canceled || touchPoint.phase == TouchPhase.Ended)
            {
                touchPoint.phase = TouchPhase.Began;
                //触发事件前需要传入新位置
                touchPoint.pos = curPoint.pos;
                OnAdaptationStateChange(touchPoint);
            }
            else
            {
                touchPoint.phase = (touchPoint.pos == curPoint.pos) ? TouchPhase.Stationary : TouchPhase.Moved;
                //要做移动事件区分, 先区分后赋值
                touchPoint.pos = curPoint.pos;
            }
        }
        else if (touchPoint.phase != TouchPhase.Canceled)
        {
            touchPoint.isTouching = false;
            if (touchPoint.phase == TouchPhase.Ended)
                touchPoint.phase = TouchPhase.Canceled;
            else
            {
                touchPoint.phase = TouchPhase.Ended;
                OnAdaptationStateChange(touchPoint);
            }
        }
    }

    private void OnAdaptationStateChange(TouchPoint touchPoint)
    {
        ScreenModel.Instance.displayCameraDic.TryGetValue(touchPoint.display, out Camera cameraEntity);
        InputDriverUGUI.Instance.SetInput(touchPoint, cameraEntity);//bengin 和 ended 事件触发
    }

    #region Android
    public bool Init()
    {
        sUsbControllerClass = new AndroidJavaClass(UsbControllerClass);
        return CallControllerStatic<bool>("init");
    }

    /// <summary>
    /// 获取Usb设备信息
    /// </summary>
    /// <returns></returns>
    private UsbDeviceInfoDto[] GetAllUsbDevices()
    {
        string json = CallControllerStatic<string>("getConnectedDevices");
        if (string.IsNullOrEmpty(json))
        {
            return new UsbDeviceInfoDto[0];
        }
        string wrappedJson = "{\"devices\":" + json + "}";
        var wrapper = JsonConvert.DeserializeObject<UsbDeviceListWrapper>(wrappedJson);
        if (wrapper == null || wrapper.devices == null)
        {
            return new UsbDeviceInfoDto[0];
        }
        return wrapper.devices;
    }

    /// <summary>
    /// 开启Usb设备数据监听
    /// </summary>
    /// <param name="deviceId">设备Id</param>
    /// <param name="interfaceIndex">接口下标</param>
    /// <param name="endpointIndex">端点下标</param>
    /// <returns></returns>
    public StartListeningResult StartListening(string deviceId, int interfaceIndex, int endpointIndex, int screenWidth, int screenHeight)
    {
        if (string.IsNullOrEmpty(deviceId))
        {
            return new StartListeningResult() { success = false, error = "deviceId 不能为空" };
        }
        var resultJson = CallControllerStatic<string>("startListening", deviceId, interfaceIndex, endpointIndex);
        var result = JsonConvert.DeserializeObject<StartListeningResult>(resultJson);
        return result;
    }
    public StartListeningResult StartListening(string deviceId, int screenWidth, int screenHeight)
    {
        // 默认使用0号位接口配置配置、0号位端点配置
        return StartListening(deviceId, 0, 0, screenWidth, screenHeight);
    }

    /// <summary>
    /// 停止Usb设备数据监听
    /// </summary>
    /// <param name="deviceId">设备Id</param>
    /// <returns></returns>
    public bool StopListening(string deviceId)
    {
        if (string.IsNullOrEmpty(deviceId))
            return false;
        var isSuccess = CallControllerStatic<bool>("stopListening", deviceId);
        return isSuccess;
    }

    /// <summary>
    /// 检查Usb设备是否正在监听
    /// </summary>
    /// <param name="deviceId">设备Id</param>
    /// <returns></returns>
    public bool IsListening(string deviceId)
    {
        if (string.IsNullOrEmpty(deviceId))
        {
            return false;
        }
        return CallControllerStatic<bool>("isListening", deviceId);
    }

    void StopAllListening()
    {
        Debug.LogError("取消监听");
        foreach (var temp in GetUsbTouchDeviceIds())
        {
            if (IsListening(temp))
                StopListening(temp);
        }
    }

    /// <summary>
    /// 释放Usb控制器资源
    /// </summary>
    public void Shutdown()
    {
        CallControllerStatic("shutdown");
    }

    private T CallControllerStatic<T>(string methodName, params object[] args)
    {
        if (!EnsureAndroid())
        {
            return default;
        }
        try
        {
            return sUsbControllerClass.CallStatic<T>(methodName, args);
        }
        catch (AndroidJavaException e)
        {
            Debug.LogError($"UsbController.{methodName} call failed: {e.Message}");
            return default;
        }
    }
    private void CallControllerStatic(string methodName)
    {
        if (!EnsureAndroid())
        {
            return;
        }
        try
        {
            sUsbControllerClass.CallStatic(methodName);
        }
        catch (AndroidJavaException e)
        {
            Debug.LogError($"UsbController.{methodName} call failed: {e.Message}");
            return;
        }
    }

    private bool EnsureAndroid()
    {
        if (Application.platform != RuntimePlatform.Android)
        {
            return false;
        }
        if (sUsbControllerClass == null)
        {
            return false;
        }
        return true;
    }
    #endregion

    #endregion
}
