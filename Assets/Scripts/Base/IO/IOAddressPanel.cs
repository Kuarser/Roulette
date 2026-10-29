using SBoxApi;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static SBoxApi.SBoxSandbox;

/// <summary>
/// 地址设置输入面板：26 键(QWERTY)+顶上数字行,共 4 行 × 10 列(40 键),
/// 行内空缺处补 3 个符号("."、"_"、空格)与大写锁定;底部操作行为 删除/取消/确认。
/// 确认后由回调把文本写入本地 PlayerPrefs。模式参考 IOPasswordPanel。
/// </summary>
public class IOAddressPanel : MonoBehaviour
{
    private GameObject baseBtnPrefabs;

    private int curSelect;
    private InputField inputField;
    private Transform btnArea;
    private Transform actionArea;
    private List<IOBaseBtnSelection> btnList = new List<IOBaseBtnSelection>();
    private List<UnityAction> btnActions = new List<UnityAction>();
    private Text placeholderText;
    private Text titleText;
    private IOBaseBtn capsBtn;
    private Color capsNormalColor = Color.white;
    private bool inputEnable = true;
    private bool isUpper = false;
    private UnityAction<string> onConfirm;
    private UnityAction onCancel;

    // 键盘布局：4 行 × 10 列（顶上数字行 + QWERTY）；行内空缺补：大写锁定、"."、"_"、空格
    private static readonly string[] KeyRow0 = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
    private static readonly string[] KeyRow1 = { "q", "w", "e", "r", "t", "y", "u", "i", "o", "p" };
    private static readonly string[] KeyRow2 = { "a", "s", "d", "f", "g", "h", "j", "k", "l", "CAPS" };
    private static readonly string[] KeyRow3 = { "z", "x", "c", "v", "b", "n", "m", ".", "_", "SPACE" };
    private const int GridCount = 40;// 键盘区按键数(4 行 × 10 列)
    // 操作行(删除/取消/确认)按下"上"时回到 z 行的对应列
    private static readonly int[] ActionUpTarget = { 32, 34, 37 };

    private void Awake()
    {
        ResMgr.Instance.LoadAssetBundle("io", "baseButton0", (obj) => baseBtnPrefabs = (GameObject)obj);
        inputField = transform.Find("input").GetComponent<InputField>();
        placeholderText = inputField.placeholder.GetComponent<Text>();
        titleText = transform.Find("title").GetComponent<Text>();
        btnArea = transform.Find("btnArea");
        if (titleText != null)
            titleText.text = Utils.GetLanguage("AddressSetting");
    }

    private void OnEnable()
    {
        EventCenter.Instance.AddEventListener<KeyCode>(EventHandle.KEY_DOWN, OnKeyCode);
        EventCenter.Instance.AddEventListener<ulong>(EventHandle.HARDWARE_KEY_DOWN, OnHardwareDown);
        EventCenter.Instance.AddEventListener<ulong>(EventHandle.HARDWARE_KEY_CLICK, OnHardwareClick);

        BuildKeyboard();
        BuildActionRow();
        SetCurSelect(0);
    }

    private void OnDisable()
    {
        EventCenter.Instance.RemoveEventListener<KeyCode>(EventHandle.KEY_DOWN, OnKeyCode);
        EventCenter.Instance.RemoveEventListener<ulong>(EventHandle.HARDWARE_KEY_DOWN, OnHardwareDown);
        EventCenter.Instance.RemoveEventListener<ulong>(EventHandle.HARDWARE_KEY_CLICK, OnHardwareClick);
    }

    public void SetParams(string placeholderStr, UnityAction<string> confirmCallback, UnityAction cancelCallback, string initText = "", float localScal = 1f)
    {
        inputField.contentType = InputField.ContentType.Standard;
        inputField.characterLimit = 32;// 输入范围(字符上限),可按需调整
        inputField.text = initText ?? "";
        placeholderText.fontSize = IOCanvasModel.Instance.curlanguage == Language.cht ? 55 : 58;
        placeholderText.text = Utils.GetLanguage(placeholderStr);
        onConfirm = confirmCallback;
        onCancel = cancelCallback;
        transform.localScale = Vector3.one * localScal;
    }

    public void SetCurSelect(int index)
    {
        curSelect = index;
        for (int i = 0; i < btnList.Count; i++)
            btnList[i].SetSelected(i == index);
        RefreshCapsColor();// 保证大写锁定态高亮不被选中态覆盖
    }

    private void RefreshCapsColor()
    {
        if (capsBtn == null)
            return;
        //大写锁定高亮:直接改标题文字颜色(IOBaseBtn.Color 的 setter 对 style==0 是空操作)
        capsBtn.titleText.color = isUpper ? IOCanvasModel.Instance.textSelectedColor : capsNormalColor;
    }

    private void BuildKeyboard()
    {
        BuildRow(KeyRow0);
        BuildRow(KeyRow1);
        BuildRow(KeyRow2);
        BuildRow(KeyRow3);
    }

    private void BuildRow(string[] codes)
    {
        for (int c = 0; c < codes.Length; c++)
        {
            string code = codes[c];
            int index = btnList.Count;
            if (code == "CAPS")
            {
                capsBtn = InstantiateKey(Utils.GetLanguage("CapsLock"), () => { SetCurSelect(index); ToggleCaps(); }, 34, btnArea);
                capsNormalColor = capsBtn.titleText.color;
            }
            else if (code == "SPACE")
            {
                InstantiateKey(Utils.GetLanguage("SPACE"), () => { SetCurSelect(index); AppendChar(' '); }, 40, btnArea);
            }
            else
            {
                char ch = code[0];
                bool isLetter = ch >= 'a' && ch <= 'z';
                string label = isLetter ? char.ToUpperInvariant(ch).ToString() : code;
                int fontSize = isLetter ? 60 : 70;
                InstantiateKey(label, () => { SetCurSelect(index); AppendChar(isLetter && isUpper ? char.ToUpperInvariant(ch) : ch); }, fontSize, btnArea);
            }
        }
    }

    private void BuildActionRow()
    {
        // 底部操作行：三个按钮宽度不等,故用 HorizontalLayoutGroup + LayoutElement 由代码定制宽度
        GameObject actionObj = new GameObject();
        actionObj.AddComponent<HorizontalLayoutGroup>();
        actionObj.transform.SetParent(transform, false);
        actionArea = actionObj.transform;
        var actionRect = actionObj.GetComponent<RectTransform>();
        actionRect.anchoredPosition = new Vector2(0, -400);
        actionRect.sizeDelta = new Vector2(1240, 100);
        actionRect.localScale = Vector3.one;
        var hlg = actionObj.GetComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;
        hlg.spacing = 40;

        CreateActionKey(Utils.GetLanguage("DELETE"), 360, () => { SetCurSelect(40); OnDeleteClick(); });
        CreateActionKey(Utils.GetLanguage("Cancel"), 360, () => { SetCurSelect(41); OnCancelClick(); });
        CreateActionKey(Utils.GetLanguage("CONFIRM"), 420, () => { SetCurSelect(42); OnConfirmClick(); });
    }

    private void CreateActionKey(string label, float width, UnityAction action)
    {
        var btn = InstantiateKey(label, action, 45, actionArea);
        var layout = btn.gameObject.AddComponent<LayoutElement>();
        layout.preferredWidth = width;
        layout.preferredHeight = 100;
    }

    private IOBaseBtn InstantiateKey(string label, UnityAction action, int fontSize, Transform parent)
    {
        var btn = Instantiate(baseBtnPrefabs, parent).GetComponent<IOBaseBtn>();
        btn.ShowBg = true;
        btn.titleText.text = label;
        btn.titleText.alignment = TextAnchor.MiddleCenter;
        btn.titleText.fontSize = fontSize;
        if (IOCanvasModel.Instance.mouseClick)
            btn.button.onClick.AddListener(action);
        btnList.Add(btn);
        btnActions.Add(action);
        return btn;
    }

    private void AppendChar(char c)
    {
        if (inputField.characterLimit > 0 && inputField.text.Length >= inputField.characterLimit)
            return;
        inputField.text += c;
    }

    private void ToggleCaps()
    {
        isUpper = !isUpper;
        RefreshCapsColor();
    }

    private void OnDeleteClick()
    {
        if (!string.IsNullOrEmpty(inputField.text))
            inputField.text = inputField.text.Substring(0, inputField.text.Length - 1);
    }

    private void OnCancelClick()
    {
        onCancel?.Invoke();
    }

    private void OnConfirmClick()
    {
        onConfirm?.Invoke(inputField.text);
    }

    private void ActivateCurKey()
    {
        if (curSelect >= 0 && curSelect < btnActions.Count)
            btnActions[curSelect]?.Invoke();
    }

    private void OnHardwareDown(ulong sboxSwitch)
    {
        if (!inputEnable) return;
        switch (sboxSwitch)
        {
            case SBOX_SWITCH.SWITCH_KEYBOARD_UP: SelectUp(); break;
            case SBOX_SWITCH.SWITCH_KEYBOARD_LEFT: SelectLeft(); break;
            case SBOX_SWITCH.SWITCH_KEYBOARD_DOWN: SelectDown(); break;
            case SBOX_SWITCH.SWITCH_KEYBOARD_RIGHT: SelectRight(); break;
        }
    }

    private void OnHardwareClick(ulong sboxSwitch)
    {
        if (!inputEnable) return;
        if (sboxSwitch == SBOX_SWITCH.SWITCH_KEYBOARD_CONFIRM)
            ActivateCurKey();
    }

    private void OnKeyCode(KeyCode keyCode)
    {
        if (!inputEnable) return;
        switch (keyCode)
        {
            case KeyCode.W: SelectUp(); break;
            case KeyCode.A: SelectLeft(); break;
            case KeyCode.S: SelectDown(); break;
            case KeyCode.D: SelectRight(); break;
            case KeyCode.F:
                ActivateCurKey();
                break;
        }
    }

    private void SelectUp()
    {
        if (curSelect >= GridCount)// 操作行 -> 回到 z 行对应列
        {
            int actionIndex = Mathf.Clamp(curSelect - GridCount, 0, ActionUpTarget.Length - 1);
            SetCurSelect(ActionUpTarget[actionIndex]);
        }
        else if (curSelect - 10 >= 0)
        {
            SetCurSelect(curSelect - 10);
        }
    }

    private void SelectDown()
    {
        if (curSelect + 10 < GridCount)
            SetCurSelect(curSelect + 10);
        else if (curSelect < GridCount)
            SetCurSelect(GridCount);// 进入操作行(默认"删除")
    }

    private void SelectLeft()
    {
        if (curSelect >= GridCount)
        {
            if (curSelect - 1 >= GridCount)
                SetCurSelect(curSelect - 1);
        }
        else if (curSelect % 10 != 0)
        {
            SetCurSelect(curSelect - 1);
        }
    }

    private void SelectRight()
    {
        if (curSelect >= GridCount)
        {
            if (curSelect + 1 < btnList.Count)
                SetCurSelect(curSelect + 1);
        }
        else if (curSelect % 10 != 9)
        {
            SetCurSelect(curSelect + 1);
        }
    }
}
