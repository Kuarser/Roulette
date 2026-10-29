using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyLineDrawerManager : MonoBehaviour
{
    // 每个触摸点ID对应一个LineRenderer
    private Dictionary<int, LineRenderer> lineRenderers = new Dictionary<int, LineRenderer>();

    // 线材质
    public Material lineMaterial;

    //private bool isMouseDrawing = false; // 记录鼠标是否正在画线

    public MyTouchComponent myTouchComponent;
    public Camera curCamera;
    //private Camera[] allCameras;
    //private bool isChangeCamera = false;


    private void Start()
    {
        if (myTouchComponent == null)
        {
            myTouchComponent = GameObject.FindObjectOfType<MyTouchComponent>();
        }

        curCamera = myTouchComponent.CurCamera;
        //Debug.Log($"当前摄像机为:{curCamera},{myTouchComponent.CurCamera}");

        //allCameras = Camera.allCameras;
        //if (allCameras != null && allCameras.Length > 0)
        //    curCamrea = allCameras[0];
        //else
        //curCamera = Camera.main;

        //换摄像机
        //GameObject tempButton = new GameObject();
        //tempButton.name = "切换摄像机划线";
        ////GameObject.FindObjectsOfTypeAll<Canvas>();
        //tempButton.transform.SetParent(GameObject.FindObjectOfType<Canvas>().transform);
        //tempButton.transform.localPosition = Vector3.zero;
        //tempButton.AddComponent<RectTransform>();
        //tempButton.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 200);
        //tempButton.AddComponent<Image>();
        //tempButton.AddComponent<Button>();
        //tempButton.GetComponent<Button>().onClick.AddListener(() =>
        //{
        //    isChangeCamera = !isChangeCamera;
        //    if (isChangeCamera)
        //        curCamrea = allCameras[1];
        //    else
        //        curCamrea = allCameras[0];
        //});

        //用键盘换,说明是编辑模式;触屏模式下不需要切换摄像机,自动识别点击的屏幕是那个摄像机的
    }


    //    private void Update()
    //    {
    //#if UNITY_EDITOR

    //        if (Input.GetMouseButtonDown(0))
    //        {
    //            isMouseDrawing = true;
    //            BeginLine(-1, Input.mousePosition);
    //        }
    //        else if (Input.GetMouseButton(0) && isMouseDrawing)
    //        {
    //            AddPoint(-1, Input.mousePosition);
    //        }
    //        else if (Input.GetMouseButtonUp(0) && isMouseDrawing)
    //        {
    //            EndLine(-1, Input.mousePosition);
    //            isMouseDrawing = false;
    //        }
    //        //别的地方控制摄像机切换了，这里就不需要再控制了
    //        //if (Input.GetKeyDown(KeyCode.RightAlt))
    //        //{
    //        //    isChangeCamera = !isChangeCamera;
    //        //    if (isChangeCamera)
    //        //        curCamrea = allCameras[1];
    //        //    else
    //        //        curCamrea = allCameras[0];
    //        //}

    //#else
    //        if (Input.touchCount > 0)
    //        {
    //            //Debug.Log("进来");
    //            foreach (Touch touch in Input.touches)
    //            {
    //                switch (touch.phase)
    //                {
    //                    case TouchPhase.Began:
    //                        BeginLine(touch.fingerId, touch.position);
    //                        break;
    //                    case TouchPhase.Moved:
    //                    case TouchPhase.Stationary:
    //                        AddPoint(touch.fingerId, touch.position);
    //                        break;
    //                    case TouchPhase.Ended:
    //                    case TouchPhase.Canceled:
    //                        EndLine(touch.fingerId);
    //                        break;
    //                }
    //            }
    //        }
    //#endif
    //    }



    // 创建一条新线
    public void BeginLine(int touchId, Vector2 startPos)
    {
        if (lineRenderers.ContainsKey(touchId))
            return;

        GameObject lineObj = new GameObject($"Line_{touchId}");
        lineObj.transform.SetParent(this.transform);
        if (curCamera == null)
            curCamera = myTouchComponent.CurCamera;
        else
        {
            if (curCamera != myTouchComponent.CurCamera)
            {
                curCamera = myTouchComponent.CurCamera;
                Debug.LogError("画新线,换了一个摄像机了");
            }
        }
        lineObj.layer = curCamera.gameObject.layer;
        //lineObj.layer = myTouchComponent.CurCamera.gameObject.layer;
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.material = lineMaterial != null ? lineMaterial : new Material(Shader.Find("Sprites/Default"));
        lr.positionCount = 1;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.useWorldSpace = true;
        lr.SetPosition(0, ScreenToWorld(startPos));
        lineRenderers[touchId] = lr;
    }

    // 添加点
    public void AddPoint(int touchId, Vector2 pos)
    {
        //if (curCamera != null && curCamera != myTouchComponent.CurCamera)
        //{
        //    Debug.Log($"加不了点?{curCamera},{myTouchComponent.CurCamera},两者不同?");
        //    return;
        //}
        if (!lineRenderers.TryGetValue(touchId, out var lr))
            return;
        lr.positionCount++;
        lr.SetPosition(lr.positionCount - 1, ScreenToWorld(pos));
    }

    // 擦除线
    public void EndLine(int touchId, Vector2 pos)
    {
        if (lineRenderers.TryGetValue(touchId, out var lr))
        {
            Destroy(lr.gameObject);
            lineRenderers.Remove(touchId);
        }
    }

    // 屏幕坐标转世界坐标（假设z=0，适用于2D UI画线）
    private Vector3 ScreenToWorld(Vector2 screenPos)
    {
        var cam = curCamera;
        //var cam = myTouchComponent.CurCamera;
        if (cam != null)
            return cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
        return new Vector3(screenPos.x, screenPos.y, 0f);
    }
}
