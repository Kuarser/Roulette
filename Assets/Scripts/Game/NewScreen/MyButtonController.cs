using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MyButtonController : MonoBehaviour
{
    public Button cancelBingdingButtons;

    private void Start()
    {
        if (cancelBingdingButtons == null)
        {
            //cancelBingdingButtons = GameObject.FindObjectsOfType<Button>().FirstOrDefault(b => b.gameObject.name == "cancelBingdingBtn");
            //cancelBingdingButtons= GameObject.Find("cancelBingdingBtn")?.GetComponent<Button>();
            cancelBingdingButtons = transform.Find("cancelBingdingBtn")?.GetComponent<Button>();

        }
        if (cancelBingdingButtons == null)
        {
            // 1. 创建 Canvas（如果场景中没有）
            Canvas tempCanvas = this.transform.GetComponentInParent<Canvas>();
            if (tempCanvas == null)
            {
                Debug.LogError("报错!!!没有Canvas.");
                GameObject canvasGO = new GameObject("Canvas");
                tempCanvas = canvasGO.AddComponent<Canvas>();
                tempCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<CanvasScaler>();
                canvasGO.AddComponent<GraphicRaycaster>();
            }

            // 2. 创建 Button 对象
            GameObject go = new GameObject("cancelBingdingBtn");
            go.transform.SetParent(tempCanvas.transform, false);

            // 3. 添加 Image 组件（Button 需要背景）
            Image image = go.AddComponent<Image>();
            image.color = Color.cyan; // 按钮背景颜色

            // 4. 添加 Button 组件
            Button button = go.AddComponent<Button>();

            // 5. 设置按钮位置和大小
            RectTransform rectTransform = go.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 60);
            rectTransform.anchoredPosition = new Vector2(0, -280);

            // 6. 创建按钮文字
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(go.transform, false);
            Text text = textGO.AddComponent<Text>();
            text.text = "取消监听";
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 30;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.black;
            text.raycastTarget = false;
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.sizeDelta = rectTransform.sizeDelta;

            cancelBingdingButtons = go.GetComponent<Button>();
        }
        if (cancelBingdingButtons != null)
        {
            //Debug.Log("有取消监听按钮创建成功");
            cancelBingdingButtons.onClick.AddListener(() => OnClick(cancelBingdingButtons));
            cancelBingdingButtons.onClick.AddListener(FindObjectOfType<MyTouchComponent>().CancelBingding);
        }
        else
        {
            Debug.LogError("没有取消监听按钮,创建失败,不正常");
        }
    }
    void OnClick(Button btn)
    {
        Debug.LogError($"按钮事件响应:点击了按钮: {btn.gameObject.name} ,按钮的text: {btn.GetComponentInChildren<Text>().text} ");
    }
}
