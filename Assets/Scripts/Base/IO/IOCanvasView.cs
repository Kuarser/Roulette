using Newtonsoft.Json;
using SBoxApi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static System.Collections.Specialized.BitVector32;

public class SectionData
{
    public bool selected;
    public IOBaseSection baseSection;
    public IOSectionState state;
}

public partial class IOCanvasView : MonoSingleton<IOCanvasView>
{
    private int curSelect;
    private bool isGetJackpotData;
    private bool betDataCoinValueShow;
    [HideInInspector]
    public SectionData selectSection = new SectionData();
    private IOState State
    {
        set { IOCanvasModel.Instance.state = value; }
        get { return IOCanvasModel.Instance.state; }
    }

    private Text title;
    private Text subtitle;

    private List<IOBaseShow> titleLeftList = new List<IOBaseShow>();
    private List<IOBaseShow> titleRightList = new List<IOBaseShow>();

    #region prefabs
    private GameObject baseShowPrefab;
    private GameObject baseShowPrefab1;
    private GameObject baseShowPrefab2;
    private GameObject baseShowPrefab3;
    private GameObject baseShowPrefab4;
    private GameObject baseShowPrefab5;
    private GameObject baseShowPrefab6;
    private GameObject baseBtnPrefab;
    private GameObject baseBtnPrefab1;
    private GameObject baseBtnPrefab2;
    private GameObject baseTogglePrefab;
    private GameObject baseSectionPrefab;
    private GameObject baseSectionPrefab1;
    private GameObject ticketRatioPrefab;
    private GameObject switchSectionsPrefab;
    private GameObject dateTimeSectionsPrefab;
    private GameObject basePassword;
    private GameObject baseAddress;//地址设置面板
    private GameObject modDate;
    private GameObject modDate2;//zxd 增加

    private RectTransform IOArrowRect;
    private MyButton arrowUp;
    private Image arrowUpImg;
    private MyButton arrowDown;
    private Image arrowDownImg;
    private GameObject modDatePanel;
    private GameObject modDate2Panel;//zxd 增加
    private List<IOBaseBtn> billBtnList = new List<IOBaseBtn>();//纸钞机面板动态按钮
    private List<IOBaseBtn> printerBtnList = new List<IOBaseBtn>();//打印机面板动态按钮
    private Coroutine billRefreshCoroutine;//纸钞机延迟刷新协程(返回时停止)
    private Coroutine printerRefreshCoroutine;//打印机延迟刷新协程(返回时停止)
    private IOBaseShow dateShow;
    private IOBaseShow diffcultyShow;

    private IOBaseBtn scoreShowBtn;
    private List<IOBaseShow> betDataShowList = new List<IOBaseShow>();

    #endregion

    [HideInInspector]
    public IOPasswordPanel passwordPanel;
    private IOAddressPanel addressPanel;//地址设置覆盖面板
    private Transform menuPanel;
    private GridLayoutGroup gridLayout;
    private RectTransform gridLayoutRect;
    private List<IOBaseSelection> selectionList = new List<IOBaseSelection>();
    private List<IOBaseSelection> onlyShowList = new List<IOBaseSelection>();
    private List<GameObject> tempObjList = new List<GameObject>();
    private List<GameObject> clientOnlineStateImgs = new List<GameObject>();
    private IOBaseShow iOBaseShow;
    private Coroutine loadingCoroutine;

    private void Awake()
    {
        ResMgr.Instance.LoadAssetBundle("io", "baseShow0", (obj) => baseShowPrefab = (GameObject)obj);
        ResMgr.Instance.LoadAssetBundle("io", "baseShow1", (obj) => baseShowPrefab1 = (GameObject)obj);
        ResMgr.Instance.LoadAssetBundle("io", "baseShow2", (obj) => baseShowPrefab2 = (GameObject)obj);
        ResMgr.Instance.LoadAssetBundle("io", "baseShow3", (obj) => baseShowPrefab3 = (GameObject)obj);
        ResMgr.Instance.LoadAssetBundle("io", "baseShow4", (obj) => baseShowPrefab4 = (GameObject)obj);
        ResMgr.Instance.LoadAssetBundle("io", "baseShow5", (obj) => baseShowPrefab5 = (GameObject)obj);
        ResMgr.Instance.LoadAssetBundle("io", "baseShow6", (obj) => baseShowPrefab6 = (GameObject)obj);
        ResMgr.Instance.LoadAssetBundle("io", "baseButton0", (obj) => baseBtnPrefab = (GameObject)obj);
        ResMgr.Instance.LoadAssetBundle("io", "baseButton1", (obj) => baseBtnPrefab1 = (GameObject)obj);
        ResMgr.Instance.LoadAssetBundle("io", "baseButton2", (obj) => baseBtnPrefab2 = (GameObject)obj);
        ResMgr.Instance.LoadAssetBundle("io", "baseToggle", (obj) => baseTogglePrefab = (GameObject)obj);
        ResMgr.Instance.LoadAssetBundle("io", "baseSection0", (obj) => baseSectionPrefab = (GameObject)obj);
        ResMgr.Instance.LoadAssetBundle("io", "baseSection1", (obj) => baseSectionPrefab1 = (GameObject)obj);

        ResMgr.Instance.LoadAssetBundle("io", "ticketRatio", (obj) => ticketRatioPrefab = (GameObject)obj);
        ResMgr.Instance.LoadAssetBundle("io", "switchSection", (obj) => switchSectionsPrefab = (GameObject)obj);
        ResMgr.Instance.LoadAssetBundle("io", "dateTimeSection", (obj) => dateTimeSectionsPrefab = (GameObject)obj);
        ResMgr.Instance.LoadAssetBundle("io", "basePassword", (obj) => basePassword = (GameObject)obj);
        ResMgr.Instance.LoadAssetBundle("io", "baseAddress", (obj) => baseAddress = (GameObject)obj);
        ResMgr.Instance.LoadAssetBundle("io", "modDate", (obj) => modDate = (GameObject)obj);
        ResMgr.Instance.LoadAssetBundle("io", "modDate2", (obj) => modDate2 = (GameObject)obj);
        SBoxSandbox.GetDateTime();//初始化时间

        IOCanvasManager.Instance.Init(this);
        title = transform.Find("title/center/title").GetComponent<Text>();
        subtitle = transform.Find("title/center/subtitle").GetComponent<Text>();
        IOArrowRect = transform.Find("IOArrow").GetComponent<RectTransform>();
        arrowUp = IOArrowRect.Find("up").GetComponent<MyButton>();
        arrowUpImg = arrowUp.transform.Find("Image").GetComponent<Image>();
        arrowDown = IOArrowRect.Find("down").GetComponent<MyButton>();
        arrowDownImg = arrowDown.transform.Find("Image").GetComponent<Image>();
        var titleLeft = transform.Find("title/left");
        for (int i = 0; i < titleLeft.childCount; i++)
        {
            var baseShow = titleLeft.GetChild(i).GetComponent<IOBaseShow>();
            titleLeftList.Add(baseShow);
            baseShow.gameObject.SetActive(false);
        }
        var titleRight = transform.Find("title/right");
        for (int i = 0; i < titleRight.childCount; i++)
        {
            var baseShow = titleRight.GetChild(i).GetComponent<IOBaseShow>();
            titleRightList.Add(baseShow);
            baseShow.gameObject.SetActive(false);
        }

        arrowUp.OnPointerDownEvent.AddListener(OnArrowUpPointerDown);
        arrowUp.OnPointerUpEvent.AddListener(OnArrowUpPointerUp);

        arrowDown.OnPointerDownEvent.AddListener(OnArrowDownPointerDown);
        arrowDown.OnPointerUpEvent.AddListener(OnArrowDownPointerUp);

        var clientSection = transform.Find("clientSection");
        for (int i = 0; i < clientSection.childCount; i++)
            clientOnlineStateImgs.Add(clientSection.GetChild(i).Find("online").gameObject);
        //扩展到 MaxMacId 个在线指示: 复用现有 client 节点, 由 clientSection 的 GridLayoutGroup 自动排版
        if (clientSection.childCount > 0 && clientOnlineStateImgs.Count < SBoxModel.MaxMacId)
        {
            Transform template = clientSection.GetChild(0);
            while (clientOnlineStateImgs.Count < SBoxModel.MaxMacId)
            {
                GameObject clone = Instantiate(template.gameObject, clientSection, false);
                int seat = clientOnlineStateImgs.Count + 1;
                clone.name = $"client{seat}";
                Text cloneText = clone.transform.Find("Text")?.GetComponent<Text>();
                if (cloneText != null)
                    cloneText.text = seat.ToString();
                clientOnlineStateImgs.Add(clone.transform.Find("online").gameObject);
            }
        }
        clientOnlineStateImgs[0].SetActive(true);//一号分机是主机(进入设置),常亮
    }

    private void OnEnable()
    {
        isGetJackpotData = false;
        OnRefreshClientDataPanel();
        InitManagerPasswordPanel();
        AddEventListener();
    }

    private void AddEventListener()
    {
        EventCenter.Instance.AddEventListener<KeyCode>(EventHandle.KEY_DOWN, OnKeyDown);
        EventCenter.Instance.AddEventListener<KeyCode>(EventHandle.KEY_UP, OnKeyUp);
        EventCenter.Instance.AddEventListener<ulong>(EventHandle.HARDWARE_KEY_UP, OnHardwareKeyUp);
        EventCenter.Instance.AddEventListener<ulong>(EventHandle.HARDWARE_KEY_DOWN, OnHardwareKeyDown);
        EventCenter.Instance.AddEventListener<ulong>(EventHandle.HARDWARE_KEY_CLICK, OnHardwareClick);

        EventCenter.Instance.AddEventListener<int>(EventHandle.PASSWORD_CUR_SELECT, OnPasswordCurSelect);
        EventCenter.Instance.AddEventListener<int>(EventHandle.PASSWORD_INPUT, OnPasswordInput);
        EventCenter.Instance.AddEventListener(EventHandle.PASSWORD_DELETE, OnPasswordDelete);
        EventCenter.Instance.AddEventListener(EventHandle.PASSWORD_CONFIRM, OnPasswordConfirm);
        EventCenter.Instance.AddEventListener(EventHandle.REFRESH_IOCANVAS, OnRefreshIOCanvas);
        EventCenter.Instance.AddEventListener(EventHandle.INIT_BILL_VIEW, OnInitBillView);
        EventCenter.Instance.AddEventListener(EventHandle.INIT_CODER_VIEW, OnInitCoderView);
        EventCenter.Instance.AddEventListener(EventHandle.GET_SANDBOX_DATE, OnGetSandboxDate);
        EventCenter.Instance.AddEventListener(EventHandle.GET_SBOX_BATTLE_PRINTER_DATE, OnGetSandboxDate);
        EventCenter.Instance.AddEventListener(EventHandle.INSTANTIATE_BILL_DETAL, OnInstantiateBillDetal);
        EventCenter.Instance.AddEventListener<Dictionary<int, List<BetData>>>(EventHandle.GET_NET_JACKPOT_DATA, OnGetNetJackpotData);
        EventCenter.Instance.AddEventListener(EventHandle.REFRESH_CLIENT_DATA_PANEL, OnRefreshClientDataPanel);
        EventCenter.Instance.AddEventListener(EventHandle.GET_SBOX_BILL_LIST, OnGetBillList);
        EventCenter.Instance.AddEventListener(EventHandle.GET_SBOX_BILL_STATE, OnGetBillState);
        EventCenter.Instance.AddEventListener(EventHandle.GET_SBOX_PRINTER_LIST, OnGetPrinterList);
        EventCenter.Instance.AddEventListener(EventHandle.GET_SBOX_PRINTER_STATE, OnGetPrinterState);
    }

    private void RemoveEventListener()
    {
        EventCenter.Instance.RemoveEventListener<KeyCode>(EventHandle.KEY_DOWN, OnKeyDown);
        EventCenter.Instance.RemoveEventListener<KeyCode>(EventHandle.KEY_UP, OnKeyUp);
        EventCenter.Instance.RemoveEventListener<ulong>(EventHandle.HARDWARE_KEY_UP, OnHardwareKeyUp);
        EventCenter.Instance.RemoveEventListener<ulong>(EventHandle.HARDWARE_KEY_DOWN, OnHardwareKeyDown);
        EventCenter.Instance.RemoveEventListener<ulong>(EventHandle.HARDWARE_KEY_CLICK, OnHardwareClick);

        EventCenter.Instance.RemoveEventListener<int>(EventHandle.PASSWORD_CUR_SELECT, OnPasswordCurSelect);
        EventCenter.Instance.RemoveEventListener<int>(EventHandle.PASSWORD_INPUT, OnPasswordInput);
        EventCenter.Instance.RemoveEventListener(EventHandle.PASSWORD_DELETE, OnPasswordDelete);
        EventCenter.Instance.RemoveEventListener(EventHandle.REFRESH_IOCANVAS, OnRefreshIOCanvas);
        EventCenter.Instance.RemoveEventListener(EventHandle.INIT_BILL_VIEW, OnInitBillView);
        EventCenter.Instance.RemoveEventListener(EventHandle.INIT_CODER_VIEW, OnInitCoderView);
        EventCenter.Instance.RemoveEventListener(EventHandle.GET_SANDBOX_DATE, OnGetSandboxDate);
        EventCenter.Instance.RemoveEventListener(EventHandle.GET_SBOX_BATTLE_PRINTER_DATE, OnGetSandboxDate);
        EventCenter.Instance.RemoveEventListener(EventHandle.INSTANTIATE_BILL_DETAL, OnInstantiateBillDetal);
        EventCenter.Instance.RemoveEventListener<Dictionary<int, List<BetData>>>(EventHandle.GET_NET_JACKPOT_DATA, OnGetNetJackpotData);
        EventCenter.Instance.RemoveEventListener(EventHandle.REFRESH_CLIENT_DATA_PANEL, OnRefreshClientDataPanel);
        EventCenter.Instance.RemoveEventListener(EventHandle.GET_SBOX_BILL_LIST, OnGetBillList);
        EventCenter.Instance.RemoveEventListener(EventHandle.GET_SBOX_BILL_STATE, OnGetBillState);
        EventCenter.Instance.RemoveEventListener(EventHandle.GET_SBOX_PRINTER_LIST, OnGetPrinterList);
        EventCenter.Instance.RemoveEventListener(EventHandle.GET_SBOX_PRINTER_STATE, OnGetPrinterState);
    }

    private void OnRefreshClientDataPanel()
    {
        //一号分机是主机(进入设置),常亮,不随状态刷新
        for (int i = 1; i < clientOnlineStateImgs.Count; i++)
            clientOnlineStateImgs[i].SetActive(NetModel.Instance.playerOnlineState[i] != 0);
    }

    private void InitManagerPasswordPanel()
    {
        if (passwordPanel != null)
            return;
        IOCanvasModel.Instance.state = IOState.CheckPermissions;
        passwordPanel = Instantiate(basePassword, transform).GetComponent<IOPasswordPanel>();
        //passwordPanel.SetParams(placeholderStr: Utils.GetLanguage("Please enter password"));
        passwordPanel.SetParams(placeholderStr: "Please enter password");
        passwordPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(30, 450);
        passwordPanel.transform.localScale = Vector3.one;
    }

    public void DestroyManagerPasswordPanel()
    {
        Destroy(passwordPanel.gameObject);
        passwordPanel = null;
    }

    public void InstantiateMenuPanel()
    {
        if (menuPanel == null)
        {
            var trans = new GameObject("menuPanel", typeof(GridLayoutGroup)).transform;
            trans.SetParent(transform);
            trans.localScale = Vector3.one;
            trans.localPosition = Vector3.zero;
            menuPanel = trans;
        }
        InitFunctionPanel();
    }

    private void SetMenuGridLayout()
    {
        gridLayout = menuPanel.GetComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(490f, 150f);
        gridLayout.padding = new RectOffset(0, 0, 0, 0);
        gridLayoutRect = menuPanel.GetComponent<RectTransform>();
        gridLayoutRect.anchorMin = new Vector2(0.5f, 0);
        gridLayoutRect.anchorMax = new Vector2(0.5f, 0);
        gridLayoutRect.sizeDelta = new Vector2(1920f, 950);
        gridLayoutRect.anchoredPosition = new Vector2(0, 470f);
        gridLayout.childAlignment = TextAnchor.UpperCenter;
        gridLayout.spacing = new Vector2(180f, 30f);
        gridLayout.startAxis = GridLayoutGroup.Axis.Vertical;
        gridLayout.constraint = GridLayoutGroup.Constraint.Flexible;
    }

    public void ClearMenuBtn()
    {
        selectionList.ForEach(baseBtn =>
        {
            if (baseBtn != null)
                Destroy(baseBtn.gameObject);
        });
        selectionList.Clear();
        onlyShowList.ForEach(baseShow =>
        {
            if (baseShow != null)
                Destroy(baseShow.gameObject);
        });
        dateShow = null;
        diffcultyShow = null;
        onlyShowList.Clear();
        tempObjList.ForEach(temp =>
        {
            if (temp != null)
                Destroy(temp);
        });
        tempObjList.Clear();
    }

    public void InitFunctionPanel()
    {
        //oldSoundVolumScale = IOCanvasModel.Instance.SoundVolumScale;
        oldJackpotConfig.jackpotSwitch = IOCanvasModel.Instance.jackpotCfg.jackpotSwitch;
        oldJackpotConfig.jpPercent = IOCanvasModel.Instance.jackpotCfg.jpPercent;
        oldJackpotConfig.betPercent = IOCanvasModel.Instance.jackpotCfg.betPercent;
        SetMenuGridLayout();
        title.text = Utils.GetLanguage("Menu");
        subtitle.text = "";
        int tempFontSize = /*IOCanvasModel.Instance.curlanguage == Language.en ? 50 : 66*/50;//一直用50吧,不然切换多语言,又会导致字体变回去从而超出边框
        InstantiateBaseBtn(IOFunction.Params.ToString(), InitParamsPanel, fontSize: tempFontSize, scale: 1);
        InstantiateBaseBtn(IOFunction.Bill.ToString(), InitBillPanel, fontSize: tempFontSize, scale: 1);
        InstantiateBaseBtn(IOFunction.Code.ToString(), InitCodePanel, fontSize: tempFontSize, scale: 1);
        InstantiateBaseBtn(IOCanvasModel.Instance.curlanguage.ToString(), ChangeCurLanguage, fontSize: tempFontSize, scale: 1);
        InstantiateBaseBtn(IOFunction.EditPassword.ToString(), InitEditPasswordPanel, fontSize: tempFontSize, scale: 1);
        InstantiateBaseBtn(IOFunction.ModifiedDate.ToString(), InitModeifedDatePanel, fontSize: tempFontSize, scale: 1);
        InstantiateBaseBtn(IOFunction.JackpotSetting.ToString(), InitJackpotSettingPanel, fontSize: tempFontSize, scale: 1);//net jackpot(联网彩金)设置,暂时去掉
        InstantiateBaseBtn(IOFunction.JackpotBet.ToString(), InitJackpotBetPanel, fontSize: tempFontSize, scale: 1);
        InstantiateBaseBtn(IOFunction.JackpotWins.ToString(), InitJackpotWinPanel, fontSize: tempFontSize, scale: 1);
        if (IOCanvasModel.Instance.CanEditMachineId)
            InstantiateBaseBtn(IOParams.ProSetting.ToString(), InitProSettingPanel, fontSize: tempFontSize, scale: 1);
        InstantiateBaseBtn(IOFunction.PrintSetting.ToString(), InitPrintSettingPanel, fontSize: tempFontSize, scale: 1);
        InstantiateBaseBtn(IOFunction.BgmSetting.ToString(), InitBgmSettingPanel, fontSize: tempFontSize, scale: 1);
        InstantiateBaseBtn(IOFunction.BillSetting.ToString(), InitBillSettingPanel, fontSize: tempFontSize, scale: 1);
        InstantiateBaseBtn(IOFunction.PrinterSetting.ToString(), InitPrinterSettingPanel, fontSize: tempFontSize, scale: 1);
        InstantiateBaseBtn(IOFunction.Exit.ToString(), ExitIOCanvas, fontSize: tempFontSize, scale: 1);
        curSelect = 0;
        SetCurSelect();
    }
public int oldSoundVolumScale = 0;
    public void InitParamsPanel()
    {
        ClearMenuBtn();
        SetParamsGridLayout();
        IOCanvasModel.Instance.SetTempCfgData(IOCanvasModel.Instance.CfgData);
        title.text = Utils.GetLanguage(IOFunction.Params.ToString());
        subtitle.text = "";

        //InstantiateEasyShow("PlaceMode", (Utils.GetEnumNames(typeof(IOPlaceMode))[IOCanvasModel.Instance.tempCfgData.PlaceType]));
        InstantiateBaseSection(IOParams.GameGear.ToString(), Utils.GetEnumNames(typeof(IOGameGear)), IOCanvasModel.Instance.tempCfgData.GameGear, onClick: () => { OnSectionClick((int)IOParams.GameGear, IOSectionState.GameGear); });
        InstantiateBaseSection(IOParams.NewGameMode.ToString(), Utils.GetEnumNames(typeof(IONewGameMode)), IOCanvasModel.Instance.tempCfgData.NewGameMode, onClick: () => { OnSectionClick((int)IOParams.NewGameMode, IOSectionState.NewGameMode); });
        InstantiateBaseSection(IOParams.CountDown.ToString(), IOCanvasModel.Instance.tempCfgData.CountDown, onClick: () => { OnSectionClick((int)IOParams.CountDown, IOSectionState.CountDown); }, ioParams: IOParams.CountDown);
        InstantiateBaseSection(IOParams.MinBet.ToString(), IOCanvasModel.Instance.tempCfgData.MinBet, onClick: () => { OnSectionClick((int)IOParams.MinBet, IOSectionState.MinBet); });
        InstantiateBaseSection(IOParams.MaxBet.ToString(), IOCanvasModel.Instance.tempCfgData.MaxBet, onClick: () => { OnSectionClick((int)IOParams.MaxBet, IOSectionState.MaxBet); });
        InstantiateBaseSection(IOParams.LimitBetsWins.ToString(), IOCanvasModel.Instance.tempCfgData.LimitBetsWins, onClick: () => { OnSectionClick((int)IOParams.LimitBetsWins, IOSectionState.LimitBetsWins); });
        InstantiateBaseSection(IOParams.CoinRatio.ToString(), IOCanvasModel.Instance.tempCfgData.CoinValue, onClick: () => { OnSectionClick((int)IOParams.CoinRatio, IOSectionState.CoinRatio); }, ioParams: IOParams.CoinRatio);
        InstantiateTicketRatioSection(IOParams.TicketRatio.ToString(), onClick: () => { OnSectionClick((int)IOParams.TicketRatio, IOSectionState.TicketRatio); });
        //退票模式
        IOCanvasModel.Instance.tempCfgData.TicketMode = 0;
        InstantiateBaseSection(IOParams.RefundMode.ToString(), Utils.GetEnumNames(typeof(IORefundMode)), 0/*IOCanvasModel.Instance.tempCfgData.TicketMode*/, onClick: () => { OnSectionClick((int)IOParams.RefundMode, IOSectionState.RefundMode); });
        //InstantiateEasyShow("RefundMode", Utils.GetEnumNames(typeof(IORefundMode))[0]);//只有一个,所以0下标//或者只显示退票模式不能点击

        InstantiateBaseSection(IOParams.PrintDensity.ToString(), IOCanvasModel.Instance.tempCfgData.PrintLevel, onClick: () => { OnSectionClick((int)IOParams.PrintDensity, IOSectionState.PrintDensity); });
#if ScoreUp
        InstantiateBaseSection(IOParams.ScoreUpRatio.ToString(), IOCanvasModel.Instance.tempCfgData.ScoreUpUnit, onClick: () => { OnSectionClick((int)IOParams.ScoreUpRatio, IOSectionState.ScoreUpRatio); });
#endif
        InstantiateBaseSection(IOParams.PrinterMode.ToString(), Utils.GetEnumNames(typeof(IOPrinterMode)), IOCanvasModel.Instance.tempCfgData.PrintMode, onClick: () => { OnSectionClick((int)IOParams.PrinterMode, IOSectionState.PrinterMode); });
        InstantiateBaseSection(IOParams.PrintTime.ToString(), IOCanvasModel.Instance.tempCfgData.CheckTime, onClick: () => { OnSectionClick((int)IOParams.PrintTime, IOSectionState.PrintTime); }, ioParams: IOParams.PrintTime);
        //InistantiateSwitchSection(IOParams.SkillMode.ToString(), IOCanvasModel.Instance.switchList, onClick: SwitchSectionClick);//去掉倍率切换
        InstantiateBaseSection(IOParams.ClientWinLock.ToString(), IOCanvasModel.Instance.tempCfgData.PlayerWinLock, onClick: () => { OnSectionClick((int)IOParams.ClientWinLock, IOSectionState.ClientWinLock); });
        InstantiateBaseSection(IOParams.MachineWinLock.ToString(), IOCanvasModel.Instance.tempCfgData.LostLock, onClick: () => { OnSectionClick((int)IOParams.MachineWinLock, IOSectionState.MachineWinLock); }, contentOffset:IOCanvasModel.Instance.curlanguage != Language.en?0:70);
        InstantiateBaseSection(IOParams.RoundWinLock.ToString(), IOCanvasModel.Instance.tempCfgData.LostLockCustom, onClick: () => { OnSectionClick((int)IOParams.RoundWinLock, IOSectionState.RoundWinLock); });
        //InstantiateBaseSection(IOParams.NetSwitch.ToString(), Utils.GetEnumNames(typeof(IONetSwitch)), IOCanvasModel.Instance.tempNetSwitch, onClick: () => { OnSectionClick((int)IOParams.NetSwitch, IOSectionState.NetSwitch); });
        InstantiateBaseSection(IOParams.OffsetRatio.ToString(), IOCanvasModel.Instance.tempCfgData.PulseValue, onClick: () => { OnSectionClick((int)IOParams.OffsetRatio, IOSectionState.OffsetRatio); });
        //InstantiateBaseSection(IOParams.SoundVolumScale.ToString(), IOCanvasModel.Instance.SoundVolumScale, onClick: () => { OnSectionClick((int)IOParams.SoundVolumScale, IOSectionState.SoundVolumScale); });
        //zxd 新增加
        InstantiateBaseSection(IOParams.BetSwitchCount.ToString(), IOCanvasModel.Instance.tempBetSwitchCount, onClick: () => { OnSectionClick((int)IOParams.BetSwitchCount, IOSectionState.BetSwitchCount); }, ioParams: IOParams.BetSwitchCount);
        InstantiateBaseBtn(IOParams.BetSwitch.ToString(), InitBetSwitchPanel, fontSize: IOCanvasModel.Instance.defaultFontSize, scale: 0.8f);
        InstantiateBaseSection(IOParams.CutOdds.ToString(), IOCanvasModel.Instance.tempCfgData.CutOdds, onClick: () => { OnSectionClick((int)IOParams.CutOdds, IOSectionState.CutOdds); }, ioParams: IOParams.CutOdds);

        //oldSoundVolumScale = IOCanvasModel.Instance.SoundVolumScale;

        InstantiateBlank();
        InstantiateBlank();
        GameObject parent = new GameObject();
        tempObjList.Add(parent);
        parent.AddComponent<HorizontalLayoutGroup>();
        parent.transform.SetParent(transform, false);
        var parentRect = parent.GetComponent<RectTransform>();
        parentRect.anchoredPosition = new Vector2(0, -430);
        parentRect.sizeDelta = new Vector2(975, 100);
        parentRect.localScale = Vector3.one;
        InstantiateBaseBtn("Save", SaveConfig, parent: parent.transform, style: 2, scale: 1);
        InstantiateBaseBtn("Return", ReturnToFunction, parent: parent.transform, style: 2, scale: 1);
        InstantiateBaseBtn("OpenDoor", OpenBox, parent: parent.transform, style: 2, scale: 1);
        curSelect = 0;
        SetCurSelect();

        GameObject paramsDescr = new GameObject();
        tempObjList.Add(paramsDescr);
        var paramsDescrText = paramsDescr.AddComponent<Text>();
        paramsDescrText.raycastTarget = false;//新增加,不要射线检测
        paramsDescr.transform.SetParent(transform, false);
        var paramsDescrRect = paramsDescr.GetComponent<RectTransform>();
        paramsDescrRect.anchoredPosition = IOCanvasModel.Instance.curlanguage == Language.en ? new Vector2(707, -430) : new Vector2(716, -480);
        paramsDescrRect.sizeDelta = new Vector2(458, 200);
        paramsDescrRect.localScale = Vector3.one;
        paramsDescrText.text = Utils.GetLanguage("ParamsDescr");
        paramsDescrText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        paramsDescrText.fontStyle = FontStyle.Bold;
        paramsDescrText.fontSize = 30;
        paramsDescrText.color = IOCanvasModel.Instance.textNormalColor;

        State = IOState.Params;
    }

    private void SetParamsGridLayout()
    {
        gridLayout.startAxis = GridLayoutGroup.Axis.Vertical;
        gridLayout.childAlignment = TextAnchor.UpperLeft;
        gridLayout.spacing = new Vector2(182.5f, 8f);
        gridLayout.cellSize = new Vector2(454, 92.5f);
        gridLayout.padding = new RectOffset(0, 0, 0, 0);
        gridLayoutRect.sizeDelta = new Vector2(1620, 705);
        gridLayoutRect.anchoredPosition = new Vector2(-90, 548);

    }
    void InitBetSwitchPanel()
    {
        Debug.Log("显示betSwitch 界面");
        SetBetSwitchPanel();
    }

    private void InitBillPanel()
    {
        ClearMenuBtn();
        SetBillPanelGridLayout();
        State = IOState.Bill;
        title.text = Utils.GetLanguage("BillShow");
        subtitle.text = $"({Utils.GetLanguage("TotalBill")})";
        SBoxIdea.GetAccount();
    }

    private void OnInitBillView()
    {
        ClearMenuBtn();
        IOCanvasModel.Instance.curBillPage = 0;
#if ScoreUp
        var parent0 = InstantiateBillParent();
        InstantiateEasyShow(Utils.GetLanguage("AllScoreUp"), IOCanvasModel.Instance.billData.scoreUp.ToString(), parent: parent0, scale: 1, fontSize: 54);
        InstantiateEasyShow(Utils.GetLanguage("AllScoreDown"), IOCanvasModel.Instance.billData.scoreDown.ToString(), parent: parent0, scale: 1, fontSize: 54);
        InstantiateEasyShow(Utils.GetLanguage("AllScore"), IOCanvasModel.Instance.billData.credit.ToString(), parent: parent0, scale: 1, fontSize: 54);
        int profit0 = IOCanvasModel.Instance.billData.scoreUp - IOCanvasModel.Instance.billData.scoreDown;
        InstantiateEasyShow(Utils.GetLanguage("AllProfit"), profit0.ToString(), parent: parent0, scale: 1, fontSize: 54);
#endif
        var parent1 = InstantiateBillParent();
        InstantiateEasyShow(Utils.GetLanguage("AllBet"), IOCanvasModel.Instance.billData.bets.ToString(), parent: parent1, scale: 1, fontSize: 54);
        InstantiateEasyShow(Utils.GetLanguage("AllWin"), IOCanvasModel.Instance.billData.wins.ToString(), parent: parent1, scale: 1, fontSize: 54);
        InstantiateEasyShow(Utils.GetLanguage("AllScore"), IOCanvasModel.Instance.billData.credit.ToString(), parent: parent1, scale: 1, fontSize: 54);
        int profit1 = IOCanvasModel.Instance.billData.bets - IOCanvasModel.Instance.billData.wins;
        InstantiateEasyShow(Utils.GetLanguage("AllProfit"), profit1.ToString(), parent: parent1, scale: 1, fontSize: 54);

        var parent2 = InstantiateBillParent();
        InstantiateEasyShow(Utils.GetLanguage("AllCoinIn"), IOCanvasModel.Instance.billData.coinIn.ToString(), parent: parent2, scale: 1f, fontSize: 54);
        InstantiateEasyShow(Utils.GetLanguage("AllCollectTicket"), IOCanvasModel.Instance.billData.ticketOut.ToString(), parent: parent2, scale: 1f, fontSize: 54);
        InstantiateEasyShow(Utils.GetLanguage("AllScore"), IOCanvasModel.Instance.billData.credit.ToString(), parent: parent2, scale: 1f, fontSize: 54);
        int profit2 = IOCanvasModel.Instance.billData.coinIn - IOCanvasModel.Instance.billData.ticketOut;
        InstantiateEasyShow(Utils.GetLanguage("AllProfit"), profit2.ToString(), parent: parent2, scale: 1f, fontSize: 54);


        GameObject parent = new GameObject();
        tempObjList.Add(parent);
        parent.AddComponent<HorizontalLayoutGroup>();
        parent.transform.SetParent(transform, false);
        var parentRect = parent.GetComponent<RectTransform>();
        parentRect.anchoredPosition = new Vector2(0, -370);
        parentRect.sizeDelta = new Vector2(875, 100);
        parent.transform.localScale = Vector3.one;
#if ScoreUp
        InstantiateBaseBtn("Clients", InitClientBillPanelNo0, parent: parent.transform, style: 2, scale: 1);
#else
        InstantiateBaseBtn("Clients", InitClientBillPanelNo1, parent: parent.transform, style: 2, scale: 1);
#endif
        int tempFontSize = IOCanvasModel.Instance.curlanguage == Language.en ? 30 : 45;
        InstantiateBaseBtn("Return", ReturnToFunction, parent: parent.transform, style: 2, scale: 1);
        InstantiateBillDetal();

        curSelect = 0;
        SetCurSelect();
    }

    private void InstantiateBillDetal()
    {
        DateTime dt = IOCanvasModel.Instance.IODateTime;
        var str = GetDateString(dt);

        string str0 = IOCanvasModel.Instance.coderData != null ? IOCanvasModel.Instance.GetDate(IOCanvasModel.Instance.coderData.RemainMinute) : "";//剩余时间
        var baseShow0 = InstantiateEasyShow(str0, "", parent: transform, fontSize: 35);
        float x = 950;
        baseShow0.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, -340);

        dateShow = InstantiateEasyShow(str, "", parent: transform, fontSize: 35);
        dateShow.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, -390);

        string str1 = $"ID:{IOCanvasModel.Instance.CfgData.LineId}/{IOCanvasModel.Instance.CfgData.MachineId}:{IOCanvasModel.Instance.CfgData.CoinValue}";
        var baseShow1 = InstantiateEasyShow(str1, "", parent: transform, fontSize: 35);
        baseShow1.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, -440);

        string tempStr = IOCanvasModel.Instance.winLockBalance / 10000 > 0 ? $"<color=#F8DF1B>{IOCanvasModel.Instance.winLockBalance / 10000}</color>" : $"<color=#FF0000>{IOCanvasModel.Instance.winLockBalance / 10000}</color>";
        string str2 = $"{PlayerPrefs.GetString("CurVersion", "1.0.0")}/{IOCanvasModel.Instance.sBoxVersion}/{IOCanvasModel.Instance.CfgData.difficulty}/{IOCanvasModel.Instance.CfgData.GameGear + 1}/{tempStr}";
        diffcultyShow = InstantiateEasyShow(str2, "", parent: transform, fontSize: 35);
        diffcultyShow.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, -490);
    }

    private void OnInstantiateBillDetal()//zxd
    {
        //只有打开查询账单页才会刷新,他的子页不刷新
        //zxd add
        if (!IOCanvasModel.Instance.isShowInstantiateBillDetal)
        {
            IOCanvasModel.Instance.isShowInstantiateBillDetal = true;
            SBoxIdea.RequestCoder(0);
            IOCanvasModel.Instance.isBillPanelNeedCoderData = true;
            return;
        }
        IOCanvasModel.Instance.isShowInstantiateBillDetal = false;
        InstantiateBillDetal();
    }

    private string GetDateString(DateTime dt)
    {
        string str = dt.Year.ToString();
        if (IOCanvasModel.Instance.curlanguage == Language.cht)
        {
            str += Utils.GetLanguage("Year");
            str += dt.Month.ToString();
            str += Utils.GetLanguage("Month");
            str += dt.Day.ToString();
            str += Utils.GetLanguage("Day1");
        }
        else
        {
            str += ", ";
            str += (Month)(dt.Month - 1) + ". ";
            str += dt.Day.ToString() + " ";
        }
        str += dt.Hour.ToString();
        str += ":";
        str += dt.Minute < 10 ? "0" + dt.Minute : dt.Minute.ToString();
        return str;
    }

    private void RefreshDiffcultyShow()
    {
        string tempStr = IOCanvasModel.Instance.winLockBalance / 10000 > 0 ? $"<color=#F8DF1B>{IOCanvasModel.Instance.winLockBalance / 10000}</color>" : $"<color=#FF0000>{IOCanvasModel.Instance.winLockBalance / 10000}</color>";
        string str2 = $"{PlayerPrefs.GetString("CurVersion", "1.0.0")}/{IOCanvasModel.Instance.sBoxVersion}/{IOCanvasModel.Instance.CfgData.difficulty}/{IOCanvasModel.Instance.CfgData.GameGear + 1}/{tempStr}";
        diffcultyShow.SetContent(str2, new List<string> { });
    }

    private void RefreshDateShow()
    {
        if (dateShow == null) return;
        DateTime dt = IOCanvasModel.Instance.IODateTime;
        var str = GetDateString(dt);
        dateShow.SetContent(str, new List<string> { });
    }

    private Transform InstantiateBillParent()
    {
        GameObject parent = new GameObject();
        tempObjList.Add(parent);
        parent.AddComponent<VerticalLayoutGroup>();
        parent.transform.SetParent(menuPanel, false);
        parent.transform.localScale = Vector3.one;
        return parent.transform;
    }

    private void SetBillPanelGridLayout()
    {
        gridLayout.startAxis = GridLayoutGroup.Axis.Vertical;
        gridLayout.childAlignment = TextAnchor.MiddleCenter;
        gridLayout.padding = new RectOffset(-100, 200, 0, 0);//原来:(100, 0, 0, 0)
        gridLayout.spacing = new Vector2(500, 37);
        gridLayout.cellSize = new Vector2(100, 600);         //原来:(200, 600, 0, 0)
        gridLayoutRect.sizeDelta = new Vector2(1885, 280);
        gridLayoutRect.anchoredPosition = new Vector2(0, 608);
    }
#if ScoreUp
    private void InitClientBillPanelNo0()
    {
        ClearMenuBtn();
        SetBillPanelNo1GridLayout();

        IOCanvasModel.Instance.curBillPage = 0;
        title.text = Utils.GetLanguage("BillShow");
        subtitle.text = $"({Utils.GetLanguage("ScoreUp")}/{Utils.GetLanguage("ScoreDown")})";
        List<string> titleList = new List<string>
        {
            Utils.GetLanguage("MacId"),
            Utils.GetLanguage("ScoreUp"),
            Utils.GetLanguage("ScoreDown"),
            Utils.GetLanguage("Profit"),
            Utils.GetLanguage("AllScore"),
        };
        InstantiateBaseShow("", titleList, contentAlignment: TextAnchor.MiddleCenter, style: 1, fontSize: 50, scale: 1);

        for (int i = 0; i < IOCanvasModel.Instance.billData.playerAccountList.Count; i++)
        {
            var playerAccountData = IOCanvasModel.Instance.billData.playerAccountList[i];
            List<string> contentList = new List<string>
            {
                playerAccountData.PlayerId.ToString(),
                playerAccountData.ScoreIn.ToString(),
                playerAccountData.ScoreOut.ToString(),
                (playerAccountData.ScoreIn - playerAccountData.ScoreOut).ToString(),
                playerAccountData.Credit.ToString(),
            };

            InstantiateBaseShow("", contentList, contentAlignment: TextAnchor.MiddleCenter, style: 1, fontSize: 50, scale: 1);
        }
        List<string> totalList = new List<string>
        {
            Utils.GetLanguage("Total"),
            IOCanvasModel.Instance.billData.scoreUp.ToString(),
            IOCanvasModel.Instance.billData.scoreDown.ToString(),
            (IOCanvasModel.Instance.billData.scoreUp - IOCanvasModel.Instance.billData.scoreDown).ToString(),
            IOCanvasModel.Instance.billData.credit.ToString(),
        };
        InstantiateBaseShow("", totalList, contentAlignment: TextAnchor.MiddleCenter, style: 1, fontSize: 50, scale: 1);

        var parent = InstantiateBillBtnParent();

        InstantiateBaseToggle($"{Utils.GetLanguage("ScoreUp")} / {Utils.GetLanguage("ScoreDown")}", InitClientBillPanelNo0, parent: parent);
        InstantiateBaseToggle($"{Utils.GetLanguage("CoinIn")} / {Utils.GetLanguage("TicketOut")}", InitClientBillPanelNo1, parent: parent);
        InstantiateBaseToggle($"{Utils.GetLanguage("ProfitPage")}", InitClientBillPanelNo2, parent: parent);
        int tempFontSize = IOCanvasModel.Instance.curlanguage == Language.en ? 30 : 45;
        InstantiateBaseBtn("Return", ReturnToFunction, parent: parent.transform, style: 2, scale: 1);//新增 fontSize: tempFontSize

        InstantiateBillDetal();

        curSelect = 0;
        SetCurSelect(setColor: true);
        State = IOState.Clients;
    }
#endif
    private void InitClientBillPanelNo0(bool isShow)
    {
        if (!isShow) return;
        ClearMenuBtn();
        SetBillPanelNo1GridLayout();

        IOCanvasModel.Instance.curBillPage = 0;
        title.text = Utils.GetLanguage("BillShow");
        subtitle.text = $"({Utils.GetLanguage("ScoreUp")}/{Utils.GetLanguage("ScoreDown")})";
        List<string> titleList = new List<string>
        {
            Utils.GetLanguage("MacId"),
            Utils.GetLanguage("ScoreUp"),
            Utils.GetLanguage("ScoreDown"),
            Utils.GetLanguage("Profit"),
            Utils.GetLanguage("AllScore"),
        };
        InstantiateBaseShow("", titleList, contentAlignment: TextAnchor.MiddleCenter, style: 1, fontSize: 50, scale: 1);

        for (int i = 0; i < IOCanvasModel.Instance.billData.playerAccountList.Count; i++)
        {
            var playerAccountData = IOCanvasModel.Instance.billData.playerAccountList[i];
            List<string> contentList = new List<string>
            {
                playerAccountData.PlayerId.ToString(),
                playerAccountData.ScoreIn.ToString(),
                playerAccountData.ScoreOut.ToString(),
                (playerAccountData.ScoreIn - playerAccountData.ScoreOut).ToString(),
                playerAccountData.Credit.ToString(),
            };

            InstantiateBaseShow("", contentList, contentAlignment: TextAnchor.MiddleCenter, style: 1, fontSize: 50, scale: 1);
        }
        List<string> totalList = new List<string>
        {
            Utils.GetLanguage("Total"),
            IOCanvasModel.Instance.billData.scoreUp.ToString(),
            IOCanvasModel.Instance.billData.scoreDown.ToString(),
            (IOCanvasModel.Instance.billData.scoreUp - IOCanvasModel.Instance.billData.scoreDown).ToString(),
            IOCanvasModel.Instance.billData.credit.ToString(),
        };
        InstantiateBaseShow("", totalList, contentAlignment: TextAnchor.MiddleCenter, style: 1, fontSize: 50, scale: 1);

        var parent = InstantiateBillBtnParent();

        InstantiateBaseToggle($"{Utils.GetLanguage("ScoreUp")} / {Utils.GetLanguage("ScoreDown")}", InitClientBillPanelNo0, parent: parent);
        InstantiateBaseToggle($"{Utils.GetLanguage("CoinIn")} / {Utils.GetLanguage("TicketOut")}", InitClientBillPanelNo1, parent: parent);
        InstantiateBaseToggle($"{Utils.GetLanguage("ProfitPage")}", InitClientBillPanelNo2, parent: parent);
        int tempFontSize = IOCanvasModel.Instance.curlanguage == Language.en ? 30 : 45;
        InstantiateBaseBtn("Return", ReturnToFunction, parent: parent.transform, style: 2, scale: 1);//新增 fontSize: tempFontSize

        InstantiateBillDetal();

        curSelect = 0;
        SetCurSelect(setColor: true);
        State = IOState.Clients;
    }

    private void InitClientBillPanelNo1()
    {
        ClearMenuBtn();
        SetBillPanelNo1GridLayout();

        IOCanvasModel.Instance.curBillPage = 0;
        title.text = Utils.GetLanguage("BillShow");
        subtitle.text = $"({Utils.GetLanguage("CoinIn")}/{Utils.GetLanguage("TicketOut")})";
        List<string> titleList = new List<string>
        {
            Utils.GetLanguage("MacId"),
            Utils.GetLanguage("CoinIn"),
            Utils.GetLanguage("TicketOut"),
            Utils.GetLanguage("Profit"),
            Utils.GetLanguage("AllScore"),
        };
        InstantiateBaseShow("", titleList, contentAlignment: TextAnchor.MiddleCenter, style: 1, scale: 1, fontSize: 50);

        for (int i = 0; i < IOCanvasModel.Instance.billData.playerAccountList.Count; i++)
        {
            var playerAccountData = IOCanvasModel.Instance.billData.playerAccountList[i];
            List<string> contentList = new List<string>
            {
                playerAccountData.PlayerId.ToString(),
                playerAccountData.CoinIn.ToString(),
                playerAccountData.CoinOut.ToString(),
                (playerAccountData.CoinIn - playerAccountData.CoinOut).ToString(),
                playerAccountData.Credit.ToString(),
            };

            InstantiateBaseShow("", contentList, contentAlignment: TextAnchor.MiddleCenter, style: 1, scale: 1, fontSize: 50);
        }
        List<string> totalList = new List<string>
        {
            Utils.GetLanguage("Total"),
            IOCanvasModel.Instance.billData.coinIn.ToString(),
            IOCanvasModel.Instance.billData.ticketOut.ToString(),
            (IOCanvasModel.Instance.billData.coinIn - IOCanvasModel.Instance.billData.ticketOut).ToString(),
            IOCanvasModel.Instance.billData.credit.ToString(),
        };
        InstantiateBaseShow("", totalList, contentAlignment: TextAnchor.MiddleCenter, style: 1, scale: 1, fontSize: 50);

        var parent = InstantiateBillBtnParent();
#if ScoreUp
        InstantiateBaseToggle($"{Utils.GetLanguage("ScoreUp")} / {Utils.GetLanguage("ScoreDown")}", InitClientBillPanelNo0, parent: parent);
#endif
        InstantiateBaseToggle($"{Utils.GetLanguage("CoinIn")} / {Utils.GetLanguage("TicketOut")}", InitClientBillPanelNo1, parent: parent);
        InstantiateBaseToggle($"{Utils.GetLanguage("ProfitPage")}", InitClientBillPanelNo2, parent: parent);
        InstantiateBaseBtn("Return", ReturnToFunction, parent: parent.transform, style: 2, scale: 1);

        InstantiateBillDetal();
#if ScoreUp
        curSelect = 1;
#else
        curSelect = 0;
#endif
        SetCurSelect(curSelect,setColor: true);
        State = IOState.Clients;
    }

    private void InitClientBillPanelNo1(bool isShow)
    {
        if (!isShow) return;

        ClearMenuBtn();
        SetBillPanelNo1GridLayout();

        IOCanvasModel.Instance.curBillPage = 0;
        title.text = Utils.GetLanguage("BillShow");
        subtitle.text = $"({Utils.GetLanguage("CoinIn")}/{Utils.GetLanguage("TicketOut")})";
        List<string> titleList = new List<string>
        {
            Utils.GetLanguage("MacId"),
            Utils.GetLanguage("CoinIn"),
            Utils.GetLanguage("TicketOut"),
            Utils.GetLanguage("Profit"),
            Utils.GetLanguage("AllScore"),
        };
        InstantiateBaseShow("", titleList, contentAlignment: TextAnchor.MiddleCenter, style: 1, scale: 1, fontSize: 50);

        for (int i = 0; i < IOCanvasModel.Instance.billData.playerAccountList.Count; i++)
        {
            var playerAccountData = IOCanvasModel.Instance.billData.playerAccountList[i];
            List<string> contentList = new List<string>
            {
                playerAccountData.PlayerId.ToString(),
                playerAccountData.CoinIn.ToString(),
                playerAccountData.CoinOut.ToString(),
                (playerAccountData.CoinIn - playerAccountData.CoinOut).ToString(),
                playerAccountData.Credit.ToString(),
            };

            InstantiateBaseShow("", contentList, contentAlignment: TextAnchor.MiddleCenter, style: 1, scale: 1, fontSize: 50);
        }
        List<string> totalList = new List<string>
        {
            Utils.GetLanguage("Total"),
            IOCanvasModel.Instance.billData.coinIn.ToString(),
            IOCanvasModel.Instance.billData.ticketOut.ToString(),
            (IOCanvasModel.Instance.billData.coinIn - IOCanvasModel.Instance.billData.ticketOut).ToString(),
            IOCanvasModel.Instance.billData.credit.ToString(),
        };
        InstantiateBaseShow("", totalList, contentAlignment: TextAnchor.MiddleCenter, style: 1, scale: 1, fontSize: 50);

        var parent = InstantiateBillBtnParent();
#if ScoreUp
        InstantiateBaseToggle($"{Utils.GetLanguage("ScoreUp")} / {Utils.GetLanguage("ScoreDown")}", InitClientBillPanelNo0, parent: parent);
#endif
        InstantiateBaseToggle($"{Utils.GetLanguage("CoinIn")} / {Utils.GetLanguage("TicketOut")}", InitClientBillPanelNo1, parent: parent);
        InstantiateBaseToggle($"{Utils.GetLanguage("ProfitPage")}", InitClientBillPanelNo2, parent: parent);
        InstantiateBaseBtn("Return", ReturnToFunction, parent: parent, style: 2, scale: 1);

        InstantiateBillDetal();

#if ScoreUp
        curSelect = 1;
#else
        curSelect = 0;
#endif
        SetCurSelect(curSelect,setColor: true);
        State = IOState.Clients;
    }

    private void SetBillPanelNo1GridLayout()
    {
        gridLayout.startAxis = GridLayoutGroup.Axis.Vertical;
        gridLayout.childAlignment = TextAnchor.UpperCenter;
        gridLayout.padding = new RectOffset(0, 0, 0, 0);
        gridLayout.cellSize = new Vector2(1450, 13);
        gridLayout.spacing = new Vector2(40, 47);
        gridLayoutRect.sizeDelta = new Vector2(1600, 780f);
        gridLayoutRect.anchoredPosition = new Vector2(0, 576);
    }

    private void InitClientBillPanelNo2(bool isShow)
    {
        if (!isShow) return;
        ClearMenuBtn();
        SetBillPanelNo1GridLayout();

        IOCanvasModel.Instance.curBillPage = 1;
        title.text = Utils.GetLanguage("BillShow");
        subtitle.text = $"({Utils.GetLanguage("ProfitPage")})";
        List<string> titleList = new List<string>
        {
            Utils.GetLanguage("MacId"),
            Utils.GetLanguage("AllBet"),
            Utils.GetLanguage("AllWin"),
            Utils.GetLanguage("AllProfit"),
            Utils.GetLanguage("Taxes"),
        };
        InstantiateBaseShow("", titleList, contentAlignment: TextAnchor.MiddleCenter, style: 1, scale: 1, fontSize: 50);

        int taxesCount = 0;
        for (int i = 0; i < IOCanvasModel.Instance.billData.playerAccountList.Count; i++)
        {
            var playerAccountData = IOCanvasModel.Instance.billData.playerAccountList[i];
            int temp = SBoxIdea.PlayerScoreInfo().ContainsKey(i+1)?SBoxIdea.PlayerScoreInfo()[i+1]:0;
            taxesCount += temp;
            List<string> contentList = new List<string>
            {
                playerAccountData.PlayerId.ToString(),
                playerAccountData.Bets.ToString(),
                playerAccountData.Wins.ToString(),
                (playerAccountData.Bets - playerAccountData.Wins).ToString(),
                temp.ToString(),
            };
            // if (playerAccountData.Bets != 0)
            //     //contentList.Add(string.Format("{0:F}", (float)(playerAccountData.Bets - playerAccountData.Wins) / (float)playerAccountData.Bets));
            //     contentList.Add(string.Format("{0:F}", (float)playerAccountData.Wins / (float)playerAccountData.Bets));//个体(单个分机的)RTP公式修改=总赢积分/总(压)下注积分
            // else
            //     contentList.Add("0");

            InstantiateBaseShow("", contentList, contentAlignment: TextAnchor.MiddleCenter, style: 1, scale: 1, fontSize: 50);
        }

        List<string> totalList = new List<string>
        {
            Utils.GetLanguage("Total"),
            IOCanvasModel.Instance.billData.bets.ToString(),
            IOCanvasModel.Instance.billData.wins.ToString(),
            (IOCanvasModel.Instance.billData.bets - IOCanvasModel.Instance.billData.wins).ToString(),
            taxesCount.ToString(),
        };
        InstantiateBaseShow("", totalList, contentAlignment: TextAnchor.MiddleCenter, style: 1, scale: 1, fontSize: 50);

        var parent = InstantiateBillBtnParent();
#if ScoreUp
        InstantiateBaseToggle($"{Utils.GetLanguage("ScoreUp")} / {Utils.GetLanguage("ScoreDown")}", InitClientBillPanelNo0, parent: parent);//InitClientBillPanelNo0 上下分
#endif
        InstantiateBaseToggle($"{Utils.GetLanguage("CoinIn")} / {Utils.GetLanguage("TicketOut")}", InitClientBillPanelNo1, parent: parent);//InitClientBillPanelNo1 投退币
        InstantiateBaseToggle($"{Utils.GetLanguage("ProfitPage")}", InitClientBillPanelNo2, parent: parent);
        InstantiateBaseBtn("Return", ReturnToFunction, parent: parent, style: 2, scale: 1);

        InstantiateBillDetal();

#if ScoreUp
        curSelect = 2;
#else
        curSelect = 1;
#endif
        SetCurSelect(curSelect, true);
        State = IOState.Clients;
    }

    private Transform InstantiateBillBtnParent()
    {
        GameObject parent = new GameObject();
        tempObjList.Add(parent);
        var horizontalLayout = parent.AddComponent<HorizontalLayoutGroup>();
        horizontalLayout.spacing = 280;
        parent.AddComponent<ToggleGroup>();
        parent.transform.SetParent(transform, false);
        var rect = parent.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(692, 100);
        rect.anchoredPosition = new Vector2(-240, -430);
        rect.localScale = Vector3.one * 1.2f;
        return parent.transform;
    }

    private void InitCodePanel()
    {
        ClearMenuBtn();
        SetCodePanelGridLayout();

        title.text = Utils.GetLanguage(IOFunction.Code.ToString());
        subtitle.text = "";
        State = IOState.Code;

        SBoxIdea.RequestCoder(0);
    }

    private void OnInitCoderView()
    {
        ClearMenuBtn();
        if (passwordPanel != null)
        {
            Destroy(passwordPanel.gameObject);
            passwordPanel = null;
        }
        if (addressPanel != null)
        {
            Destroy(addressPanel.gameObject);
            addressPanel = null;
        }

        InstantiateBaseShow($"A.{Utils.GetLanguage("AllBet")}", new List<string> { IOCanvasModel.Instance.coderData.Bets.ToString() }, style: 4, scale: 1, fontSize: 40);
        InstantiateBaseShow($"B.{Utils.GetLanguage("AllWin")}", new List<string> { IOCanvasModel.Instance.coderData.Wins.ToString() }, style: 4, scale: 1, fontSize: 40);
        InstantiateBaseShow($"C.{Utils.GetLanguage("MachineId")}", new List<string> { IOCanvasModel.Instance.coderData.MachineId.ToString() }, style: 4, scale: 1, fontSize: 40);
        InstantiateBaseShow($"D.{Utils.GetLanguage("Frequency")}", new List<string> { IOCanvasModel.Instance.coderData.CoderCount.ToString() }, style: 4, scale: 1, fontSize: 40);
        InstantiateBaseShow($"E.{Utils.GetLanguage("Code1")}", new List<string> { IOCanvasModel.Instance.coderData.CheckValue.ToString() }, style: 4, scale: 1, fontSize: 40);
        InstantiateBaseShow(Utils.GetLanguage("TimeLeft"), new List<string> { IOCanvasModel.Instance.GetDate(IOCanvasModel.Instance.coderData.RemainMinute) }, style: 4, scale: 1, fontSize: 40);

        passwordPanel = Instantiate(basePassword, transform).GetComponent<IOPasswordPanel>();
        passwordPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(468, 555);
        passwordPanel.SetParams(InputField.ContentType.Standard, "Please enter code", localScal: 0.7f);
        selectionList.AddRange(passwordPanel.GetSelectionList());
        InstantiateBlank();
        InstantiateBlank();
        GameObject parent = new GameObject();
        tempObjList.Add(parent);
        parent.AddComponent<HorizontalLayoutGroup>();
        parent.transform.SetParent(menuPanel, false);
        parent.transform.localScale = Vector3.one;
        InstantiateBaseBtn("Return", ReturnToFunction, parent: parent.transform, style: 2, scale: 1f);
        var baseShow2 = InstantiateEasyShow(Utils.GetLanguage("ClickCancleTips"), "", parent: transform, fontSize: 40);
        baseShow2.GetComponent<RectTransform>().anchoredPosition = IOCanvasModel.Instance.curlanguage == Language.en ? new Vector2(-28, -340) : new Vector2(205, -340);

        State = IOState.Code;
        curSelect = 0;
        SetCurSelect();
        InstantiateBillDetal();
        IOPopTips.Instance.transform.SetAsLastSibling();
    }
public JackpotConfig oldJackpotConfig = new JackpotConfig();
    private void InitJackpotSettingPanel()
    {
        ClearMenuBtn();
        //IOCanvasModel.Instance.SetTempCfgData(IOCanvasModel.Instance.CfgData);
        IOCanvasModel.Instance.SetTempJackpotCfg(IOCanvasModel.Instance.JackpotCfg);//重进复原数据
        title.text = Utils.GetLanguage(IOFunction.JackpotSetting.ToString());

        InstantiateBaseSection(IOJackpotSetting.JackpotSwitch.ToString(), Utils.GetEnumNames(typeof(IOJackpotSwitch)), IOCanvasModel.Instance.tempJackpotCfg.jackpotSwitch, onClick: () => { OnSectionClick((int)IOJackpotSetting.JackpotSwitch, IOSectionState.JackpotSwitch); }, fontSize: 55);
        InstantiateBaseSection(IOJackpotSetting.JpPercent.ToString(), IOCanvasModel.Instance.tempJackpotCfg.jpPercent, onClick: () => { OnSectionClick((int)IOJackpotSetting.JpPercent, IOSectionState.JackpotPercent); }, fontSize: 55);
        InstantiateEasyShow("ScoreRate", (1f / IOCanvasModel.Instance.CfgData.CoinValue).ToString(), fontSize: 55);

        oldJackpotConfig.jackpotSwitch = IOCanvasModel.Instance.jackpotCfg.jackpotSwitch;
        oldJackpotConfig.jpPercent = IOCanvasModel.Instance.jackpotCfg.jpPercent;
        oldJackpotConfig.betPercent = IOCanvasModel.Instance.jackpotCfg.betPercent;
        Debug.LogError($"oldJackpotConfig:{JsonConvert.SerializeObject(oldJackpotConfig)}");

        GameObject parent = new GameObject();
        tempObjList.Add(parent);
        parent.AddComponent<HorizontalLayoutGroup>();
        parent.transform.SetParent(transform, false);
        var parentRect = parent.GetComponent<RectTransform>();
        parentRect.anchoredPosition = new Vector2(0, -430);
        parentRect.sizeDelta = new Vector2(975, 100);
        parentRect.localScale = Vector3.one;
        InstantiateBaseBtn("Save", SaveJackpotCfg, parent: parent.transform, style: 2, scale: 1);
        InstantiateBaseBtn("Return", ReturnToFunction, parent: parent.transform, style: 2, scale: 1);

        State = IOState.JackpotSetting;

        curSelect = 0;
        SetCurSelect();
        IOPopTips.Instance.transform.SetAsLastSibling();
    }

    private void InitBgmSettingPanel()//背景音乐设置
    {
        ClearMenuBtn();
        title.text = Utils.GetLanguage(IOFunction.BgmSetting.ToString());
        subtitle.text = "";
        IOCanvasModel.Instance.tempBgmSwitch = IOCanvasModel.Instance.BgmSwitch;//重进复原数据
        IOCanvasModel.Instance.tempBgmSelectIndex = IOCanvasModel.Instance.BgmSelectIndex;

        InstantiateBaseSection(IOBgmSetting.BgmSwitch.ToString(), Utils.GetEnumNames(typeof(IOBgmSwitch)), IOCanvasModel.Instance.tempBgmSwitch, onClick: () => { OnSectionClick((int)IOBgmSetting.BgmSwitch, IOSectionState.BgmSwitch); }, fontSize: 55, contentOffset: 250);
        InstantiateBaseSection(IOBgmSetting.BgmSelect.ToString(), Utils.GetEnumNames(typeof(IOBgmSelect)), IOCanvasModel.Instance.tempBgmSelectIndex, onClick: () => { OnSectionClick((int)IOBgmSetting.BgmSelect, IOSectionState.BgmSelect); }, fontSize: 55, contentOffset: 250);

        GameObject parent = new GameObject();
        tempObjList.Add(parent);
        parent.AddComponent<HorizontalLayoutGroup>();
        parent.transform.SetParent(transform, false);
        var parentRect = parent.GetComponent<RectTransform>();
        parentRect.anchoredPosition = new Vector2(0, -430);
        parentRect.sizeDelta = new Vector2(975, 100);
        parentRect.localScale = Vector3.one;
        InstantiateBaseBtn("Save", SaveBgmSetting, parent: parent.transform, style: 2, scale: 1);
        InstantiateBaseBtn("Return", ReturnToFunction, parent: parent.transform, style: 2, scale: 1);

        State = IOState.BgmSetting;

        curSelect = 0;
        SetCurSelect();
        IOPopTips.Instance.transform.SetAsLastSibling();
    }

    private void InitJackpotBetPanel()
    {
        title.text = Utils.GetLanguage(IOFunction.JackpotBet.ToString());
        ClearMenuBtn();
        SetJackpotBetPanelGridLayout();
        State = IOState.JackpotBet;

        if (!isGetJackpotData)
        {
            IOPopTips.Instance.ShowTips(Utils.GetLanguage("Loading"), -1);
            NetJPLoginInfo loginInfo = new NetJPLoginInfo()
            {
                gameType = IOCanvasModel.GameType,
                macId = IOCanvasModel.Instance.CfgData.MachineId,
            };
            MsgInfo msgInfo = new MsgInfo
            {
                cmd = (int)C2S_CMD.C2S_GetJackpotData,
                id = IOCanvasModel.Instance.CfgData.MachineId,
                jsonData = JsonConvert.SerializeObject(loginInfo)
            };
            NetMgr.Instance.SendToServer(JsonConvert.SerializeObject(msgInfo));
            loadingCoroutine = StartCoroutine(LoadingCoroutine());
        }
        else
            InitJackpotBetPanelView();
    }

    private IEnumerator LoadingCoroutine()
    {
        yield return new WaitForSeconds(5);
        IOPopTips.Instance.ShowTips(Utils.GetLanguage("LoadFault"), 3);
        yield return new WaitForSeconds(3);
        loadingCoroutine = null;
        ReturnToFunction();
    }

    private void InitJackpotBetPanelView()
    {
        betDataCoinValueShow = false;
        GameObject parent = new GameObject();
        tempObjList.Add(parent);
        parent.AddComponent<HorizontalLayoutGroup>();
        parent.transform.SetParent(transform, false);
        var parentRect = parent.GetComponent<RectTransform>();
        parentRect.anchoredPosition = new Vector2(0, 370);
        parentRect.sizeDelta = new Vector2(1455, 100);
        parentRect.localScale = Vector3.one;
        
        InstantiateBaseShow("ScoreRate", new List<string> { (int.Parse((System.Math.Round(1000f / IOCanvasModel.Instance.CfgData.CoinValue)).ToString()) / 1000f).ToString() }, parent: parent.transform, style: 5, scale: 1, fontSize: 50);
        InstantiateBaseShow("JpPercent", new List<string> { IOCanvasModel.Instance.JackpotCfg.jpPercent + "‰" }, parent: parent.transform, style: 5, scale: 1, fontSize: 50);

        List<string> titleList = new List<string>
        {
            Utils.GetLanguage("AllBetScore"),
            Utils.GetLanguage("TurnInScore"),
            Utils.GetLanguage("AllWinScore"),
        };

        iOBaseShow = InstantiateBaseShow(Utils.GetLanguage("MacId"), titleList, style: 5, scale: 1, fontSize: 50);
        for (int i = 0; i < IOCanvasModel.Instance.betDataList.Count; i++)
        {
            var betData = IOCanvasModel.Instance.betDataList[i];

            var baseShow = InstantiateBaseShow(betData.seatId.ToString(), new List<string>
            {
                (betData.bet / 100f).ToString(),
                betData.bet / 100000f * betData.jpPercent + "",
                (betData.win / 100f * IOCanvasModel.Instance.CfgData.CoinValue).ToString()
            }, style: 5, scale: 1f, fontSize: 50);
            betDataShowList.Add(baseShow);
        }

        GameObject btnParent = new GameObject();
        tempObjList.Add(btnParent);
        btnParent.AddComponent<HorizontalLayoutGroup>();
        btnParent.transform.SetParent(transform, false);
        var btnParentRect = btnParent.GetComponent<RectTransform>();
        btnParentRect.anchoredPosition = new Vector2(0, -430);
        btnParentRect.sizeDelta = new Vector2(975, 100);
        scoreShowBtn = InstantiateBaseBtn("CoinValueShow", ChangeBetDataShow, parent: btnParent.transform, style: 2, scale: 1);
        InstantiateBaseBtn("Return", ReturnToFunction, parent: btnParent.transform, style: 2, scale: 1);
    }

    private void InitJackpotWinPanel()
    {
        title.text = Utils.GetLanguage(IOFunction.JackpotWins.ToString());
        ClearMenuBtn();
        SetJackpotWinPanelGridLayout();
        State = IOState.JackpotWins;
        if (!isGetJackpotData)
        {
            IOPopTips.Instance.ShowTips(Utils.GetLanguage("Loading"), -1);
            NetJPLoginInfo loginInfo = new NetJPLoginInfo()
            {
                gameType = IOCanvasModel.GameType,
                macId = IOCanvasModel.Instance.CfgData.MachineId,
            };
            MsgInfo msgInfo = new MsgInfo
            {
                cmd = (int)C2S_CMD.C2S_GetJackpotData,
                id = IOCanvasModel.Instance.CfgData.MachineId,
                jsonData = JsonConvert.SerializeObject(loginInfo)
            };
            NetMgr.Instance.SendToServer(JsonConvert.SerializeObject(msgInfo));
            loadingCoroutine = StartCoroutine(LoadingCoroutine());
        }
        else
            InitJackpotWinPanelView();
    }

    private void SetJackpotWinPanelGridLayout()
    {
        gridLayout.padding = new RectOffset(0, 0, 0, 0);
        gridLayoutRect = menuPanel.GetComponent<RectTransform>();
        gridLayoutRect.anchorMin = new Vector2(0.5f, 0);
        gridLayoutRect.anchorMax = new Vector2(0.5f, 0);
        gridLayoutRect.sizeDelta = new Vector2(1870f, 890f);
        gridLayoutRect.anchoredPosition = new Vector2(0, 470f);
        gridLayout.childAlignment = TextAnchor.UpperCenter;
        gridLayout.spacing = new Vector2(0f, 0);
        gridLayout.cellSize = new Vector2(1600, 83);
        gridLayout.startAxis = GridLayoutGroup.Axis.Vertical;
    }

    private void InitJackpotWinPanelView()
    {
        List<string> titleList = new List<string>
        {
            Utils.GetLanguage("MacId"),
            Utils.GetLanguage("Grand"),
            Utils.GetLanguage("Times"),
            Utils.GetLanguage("Major"),
            Utils.GetLanguage("Times"),
            Utils.GetLanguage("Minor"),
            Utils.GetLanguage("Times"),
            Utils.GetLanguage("Mini"),
            Utils.GetLanguage("Times"),
        };
        InstantiateBaseShow("", titleList, style: 6, fontSize: 36, scale: 1);
        for (int i = 0; i < IOCanvasModel.Instance.betDataList.Count; i++)
        {
            var betData = IOCanvasModel.Instance.betDataList[i];
            List<string> contentList = new List<string>
            {
                betData.seatId.ToString(),
                betData.grandWin / 100f + "",
                betData.grandTimes.ToString(),
                betData.majorWin / 100f + "",
                betData.majorTimes.ToString(),
                betData.minorWin / 100f + "",
                betData.minorTimes.ToString(),
                betData.miniWin / 100f + "",
                betData.miniTimes.ToString(),
            };
            InstantiateBaseShow("", contentList, style: 6, fontSize: 36, scale: 1);
        }
        GameObject btnParent = new GameObject();
        tempObjList.Add(btnParent);
        btnParent.AddComponent<HorizontalLayoutGroup>();
        btnParent.transform.SetParent(transform, false);
        var btnParentRect = btnParent.GetComponent<RectTransform>();
        btnParentRect.anchoredPosition = new Vector2(0, -430);
        btnParentRect.sizeDelta = new Vector2(975, 100);
        InstantiateBaseBtn("Return", ReturnToFunction, parent: btnParent.transform, style: 2, scale: 1);
    }

    private void ChangeBetDataShow()
    {
        if (!isGetJackpotData) return;
        betDataCoinValueShow = !betDataCoinValueShow;
        scoreShowBtn.titleText.text = betDataCoinValueShow ? Utils.GetLanguage("ScoreShow") : Utils.GetLanguage("CoinValueShow");
        
        var temp = betDataCoinValueShow?Utils.GetLanguage("AllBetCoin"):Utils.GetLanguage("AllBetScore");
        var temp2 = betDataCoinValueShow?Utils.GetLanguage("TurnInCoin"):Utils.GetLanguage("TurnInScore");
        var temp3 = betDataCoinValueShow?Utils.GetLanguage("AllWinCoin"):Utils.GetLanguage("AllWinScore");
        List<string> titleList = new List<string>
        {
            temp,
            temp2,
            temp3,
        };
        iOBaseShow.SetContent(Utils.GetLanguage("MacId"),titleList);
        for (int i = 0; i < IOCanvasModel.Instance.betDataList.Count; i++)
        {
            var betData = IOCanvasModel.Instance.betDataList[i];
            betDataShowList[i].SetContent(betData.seatId.ToString(), new List<string>
            {
                betDataCoinValueShow ? (betData.bet / 100f / IOCanvasModel.Instance.CfgData.CoinValue).ToString() : (betData.bet / 100f).ToString(),
                betDataCoinValueShow ?  betData.bet / 100000f * betData.jpPercent / IOCanvasModel.Instance.CfgData.CoinValue + "": betData.bet / 100000f * betData.jpPercent + "",
                betDataCoinValueShow ? (betData.win / 100f).ToString() : (betData.win / 100f * IOCanvasModel.Instance.CfgData.CoinValue).ToString()
            });
        }
    }

    private void SetJackpotBetPanelGridLayout()
    {
        gridLayout.padding = new RectOffset(144, 0, 0, 0);
        gridLayoutRect = menuPanel.GetComponent<RectTransform>();
        gridLayoutRect.anchorMin = new Vector2(0.5f, 0);
        gridLayoutRect.anchorMax = new Vector2(0.5f, 0);
        gridLayoutRect.sizeDelta = new Vector2(1870f, 890f);
        gridLayoutRect.anchoredPosition = new Vector2(0, 400f);
        gridLayout.childAlignment = TextAnchor.UpperCenter;
        gridLayout.spacing = new Vector2(0f, 0);
        gridLayout.cellSize = new Vector2(1600, 73);
        gridLayout.startAxis = GridLayoutGroup.Axis.Vertical;
    }

    private void OnGetNetJackpotData(Dictionary<int, List<BetData>> betDataDic)
    {
        isGetJackpotData = true;
        if (loadingCoroutine != null)
        {
            StopCoroutine(loadingCoroutine);
            loadingCoroutine = null;
        }
        IOCanvasModel.Instance.betDataDic = betDataDic;
        IOPopTips.Instance.HideTips();
        IOCanvasModel.Instance.DealWithBetDataDic();
        if (State == IOState.JackpotBet)
            InitJackpotBetPanelView();
        else if (State == IOState.JackpotWins)
            InitJackpotWinPanelView();

    }

    private void OnGetSandboxDate()
    {
        switch (State)
        {
            case IOState.DateTime:
                PopModifiedDatePanel();
                break;
            case IOState.PrintSetting:
                ShowPrintSettingPanel();
                break;
            case IOState.Bill:
            case IOState.Code:
                RefreshDateShow();
                break;
            default:
                break;
        }
    }

    private void SetCodePanelGridLayout()
    {
        gridLayout.startAxis = GridLayoutGroup.Axis.Vertical;
        gridLayout.childAlignment = TextAnchor.UpperCenter;
        gridLayout.padding = new RectOffset(0, 0, 15, 0);
        gridLayout.spacing = new Vector2(40, 20);
        gridLayout.cellSize = new Vector2(1280, 70);//原来:new Vector2(780, 70)
        gridLayoutRect.sizeDelta = new Vector2(1000, 820);
        gridLayoutRect.anchoredPosition = new Vector2(0, 460);
    }

    private void ChangeCurLanguage()
    {
        var temp = (int)IOCanvasModel.Instance.curlanguage;
        temp = temp + 1 > Enum.GetValues(typeof(Language)).Cast<int>().Max() ? 0 : temp + 1;
        IOCanvasModel.Instance.CurLanguage = (Language)(Enum.Parse(typeof(Language), temp.ToString()));
        selectionList[3].title = IOCanvasModel.Instance.curlanguage.ToString();
        RefreshFunctionText();
        PlayerPrefs.SetInt("CurLanguage", (int)IOCanvasModel.Instance.curlanguage);
        PlayerPrefs.GetInt("CurLanguage");
        EventCenter.Instance.EventTrigger(EventHandle.CHANGE_LANGUAGE);
    }

    private void InitEditPasswordPanel()
    {
        ClearMenuBtn();
        if (passwordPanel != null)
        {
            Destroy(passwordPanel.gameObject);
            passwordPanel = null;
        }
        if (addressPanel != null)
        {
            Destroy(addressPanel.gameObject);
            addressPanel = null;
        }
        SetEditPasswordGridLayout();
        title.text = Utils.GetLanguage(IOFunction.EditPassword.ToString());

        var baseShow1 = InstantiateEasyShow(Utils.GetLanguage("EditPasswordShow"), "", parent: transform, scale: 1);
        baseShow1.GetComponent<RectTransform>().anchoredPosition = new Vector3(55, 375);
        for (int i = 0; i < 9; i++)
            InstantiateBlank();
        passwordPanel = Instantiate(basePassword, transform).GetComponent<IOPasswordPanel>();
        passwordPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 515);
        passwordPanel.SetParams(InputField.ContentType.Password, "Please enter new password", localScal: 0.7f);
        selectionList.AddRange(passwordPanel.GetSelectionList());
        InstantiateBaseBtn("Return", ReturnToFunction, style: 2, scale: 1);
        var baseShow2 = InstantiateEasyShow(Utils.GetLanguage("ClickCancleTips"), "", parent: transform, fontSize: 40);
        baseShow2.GetComponent<RectTransform>().anchoredPosition = IOCanvasModel.Instance.curlanguage == Language.en ? new Vector2(-28, -340) : new Vector2(205, -340);
        State = IOState.EditPassword;
        curSelect = 0;
        SetCurSelect();
        selectSection.state = IOSectionState.EditPassword;
        IOPopTips.Instance.transform.SetAsLastSibling();
        //OnSectionClick(curSelect, IOSectionState.EditPassword);
        //passwordPanel.Focuse(true);
    }

    private void InitProSettingPanel()
    {
        ClearMenuBtn();
        if (passwordPanel != null)
        {
            Destroy(passwordPanel.gameObject);
            passwordPanel = null;
        }
        if (addressPanel != null)
        {
            Destroy(addressPanel.gameObject);
            addressPanel = null;
        }
        SetProSettingGridLayout();
        title.text = Utils.GetLanguage(IOFunction.ProSetting.ToString());
        InstantiateEasySection(Utils.GetLanguage("LineId"), IOCanvasModel.Instance.tempCfgData.LineId.ToString(),
                onClick: () => OnEditIdSectionClick(0, IOSectionState.EditLineId));
        InstantiateEasySection(Utils.GetLanguage("MachineId"), IOCanvasModel.Instance.tempCfgData.MachineId.ToString(),
            onClick: () => OnEditIdSectionClick(1, IOSectionState.EditMacId));
        InstantiateKeyBoard();


        GameObject parent = new GameObject();

        tempObjList.Add(parent);
        parent.AddComponent<HorizontalLayoutGroup>();
        parent.transform.SetParent(transform, false);
        var parentRect = parent.GetComponent<RectTransform>();
        parentRect.anchoredPosition = new Vector2(0, -430);
        parentRect.sizeDelta = new Vector2(975, 100);
        InstantiateBaseBtn("Save", SaveConfig, parent: parent.transform, style: 2, scale: 1);
        InstantiateBaseBtn("Return", () =>
        {
            if (selectSection.selected)
            {
                IOCanvasModel.Instance.SetTempCfgData(IOCanvasModel.Instance.CfgData);
                int data = selectSection.state == IOSectionState.EditLineId ? IOCanvasModel.Instance.tempCfgData.LineId : IOCanvasModel.Instance.tempCfgData.MachineId;
                selectSection.baseSection.Content = data.ToString();
                selectSection.selected = false;
                if (IOCanvasModel.Instance.permissions == 3)
                    SetCurSelect(selectSection.state == IOSectionState.EditLineId ? 0 : 1);
                else
                    SetCurSelect();
            }
            else
                ReturnToFunction();
        }, parent: parent.transform, style: 2, scale: 1);
        parent.transform.localScale = Vector3.one;

        //InstantiateBaseBtn("Save", () => {
        //    SaveConfig();
        //}, style: 2);
        //InstantiateBaseBtn("Return", () => {
        //    if (selectSection.selected)
        //    {
        //        IOCanvasModel.Instance.SetTempCfgData(IOCanvasModel.Instance.CfgData);
        //        int data = selectSection.state == IOSectionState.EditLineId ? IOCanvasModel.Instance.tempCfgData.LineId : IOCanvasModel.Instance.tempCfgData.MachineId;
        //        selectSection.baseSection.Content = data.ToString();
        //        selectSection.selected = false;
        //        if (IOCanvasModel.Instance.permissions == 3)
        //            SetCurSelect(selectSection.state == IOSectionState.EditLineId ? 0 : 1);
        //        else
        //            SetCurSelect();
        //    }
        //    else
        //        ReturnToFunction();
        //}, style: 2);
        State = IOState.ProSetting;
        curSelect = 0;
        SetCurSelect();
        IOPopTips.Instance.transform.SetAsLastSibling();
    }

    private void SetProSettingGridLayout()
    {
        gridLayout.startAxis = GridLayoutGroup.Axis.Vertical;
        gridLayout.childAlignment = TextAnchor.UpperCenter;
        gridLayout.padding = new RectOffset(0, 0, 0, 0);
        if (IOCanvasModel.Instance.permissions == 3)
            gridLayout.spacing = new Vector2(40, 19);
        else
            gridLayout.spacing = new Vector2(40, 29.5f);
        gridLayout.cellSize = new Vector2(600, 76.5f);
        gridLayoutRect.sizeDelta = new Vector2(845, 954);
        gridLayoutRect.anchoredPosition = new Vector2(0, 502);
    }

    private void InitModeifedDatePanel()
    {
        State = IOState.DateTime;
        SBoxSandbox.GetDateTime();
    }

    private void ExitIOCanvas()
    {
        if (IOCanvasModel.Instance.isExitIOCanvas)
            IOCanvasModel.Instance.isExitIOCanvas = false;
        curSelect = 0;
        gameObject.SetActive(false);
        ClearMenuBtn();
        EventCenter.Instance.EventTrigger(EventHandle.LEAVE_SETTING);
    }
    public DateTime oldDateTime;
    public void PopModifiedDatePanel()
    {
        //Debug.LogError("PopModifiedDatePanel");
        oldDateTime = IOCanvasModel.Instance.IODateTime;
        modDatePanel = Instantiate(modDate, transform);
        var rect = modDatePanel.GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        rect.localScale = Vector3.one;
        var parent = modDatePanel.transform;
        var dateSection = InistantiateDateTimeSection("", onClick: DateTimeSectionClick, parent: parent);
        var dateTimeRect = dateSection.GetComponent<RectTransform>();
        dateTimeRect.anchoredPosition = new Vector2(-18f, 27f);
        GameObject parent1 = new GameObject();
        parent1.AddComponent<HorizontalLayoutGroup>();
        parent1.transform.SetParent(parent, false);
        parent1.transform.localScale = Vector3.one;
        var parentRect1 = parent1.GetComponent<RectTransform>();
        parentRect1.anchoredPosition = new Vector2(0, -120);
        parentRect1.sizeDelta = new Vector2(705, 85);
        InstantiateBaseBtn("Save", SaveDateTime, parent: parent1.transform, style: 2, scale: 1);
        InstantiateBaseBtn("Return", () => CloseModifiedDatePanel(true), parent: parent1.transform, style: 2, scale: 1);

        State = IOState.DateTime;
        curSelect = selectionList.Count - 3;
        SetCurSelect(curSelect);

        dateSection.Selected = true;
        selectSection.selected = dateSection.Selected;
        selectSection.baseSection = dateSection;
        selectSection.state = IOSectionState.DateTime;

        selectSection.baseSection.CurIndex = 0;


        //DefaultDateTimeSectionClick();
    }


    private List<IOBaseSection> betSwitchControllerList = new List<IOBaseSection>();
    public void SetBetSwitchPanel()//zxd//新的切换压分倍率
    {
        modDate2Panel = Instantiate(modDate2, transform);
        var rect = modDate2Panel.GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        rect.localScale = Vector3.one;
        var parent = modDate2Panel.transform;

        Debug.LogError($"L:{IOCanvasModel.Instance.tempBetSwitchList.Length}");
        Debug.LogError($"C:{IOCanvasModel.Instance.tempBetSwitchCount}");
        Debug.LogError($"LJS{JsonConvert.SerializeObject(IOCanvasModel.Instance.tempBetSwitchList)}");
        //没有保存重置倍率赋值
        //IOCanvasModel.Instance.tempBetSwitchCount = IOCanvasModel.Instance.betSwitchCount;//这个暂时不同步
        for (int i = 0; i < IOCanvasModel.Instance.tempBetSwitchList.Length; i++)
            IOCanvasModel.Instance.tempBetSwitchList[i] = IOCanvasModel.Instance.BetSwitchList[i];

        State = IOState.BetSwitch;
        var parent2 = parent.Find("BG");

        betSwitchControllerList.Clear();
        for (int i = 0; i < IOCanvasModel.Instance.tempBetSwitchCount; i++)
        {
            //正着数不清楚,直接倒着数吧,每次创建都是在最后一位,最后一位后创建就又加了一位,就又有一个是新的最后一位
            int tempIndex = selectionList.Count;
            int tempI = i;
            if (i > 0 && IOCanvasModel.Instance.tempBetSwitchList[i] < IOCanvasModel.Instance.tempBetSwitchList[i - 1])
            {
                IOCanvasModel.Instance.tempBetSwitchList[i] = IOCanvasModel.Instance.tempBetSwitchList[i - 1];
            }
            IOBaseSection item = InstantiateBetSwitchSection(Utils.GetLanguage("Bet") + i, defaultValue: IOCanvasModel.Instance.tempBetSwitchList[i], onClick: () =>
        {
            IOCanvasModel.Instance.curBetSwitchIndex = tempI;
            OnSectionClick(tempIndex, IOSectionState.BetSwitchConnect);
        }, trans: parent2, fontSize: 50);
            betSwitchControllerList.Add(item);
        }

        GameObject parent1 = new GameObject();
        parent1.AddComponent<HorizontalLayoutGroup>();
        parent1.transform.SetParent(parent, false);
        parent1.transform.localScale = Vector3.one;
        var parentRect1 = parent1.GetComponent<RectTransform>();
        parentRect1.anchoredPosition = new Vector2(0, -345);
        parentRect1.sizeDelta = new Vector2(705, 85);
        InstantiateBaseBtn("Save", SaveBetSwitch, parent: parent1.transform, style: 2, scale: 1);
        InstantiateBaseBtn("Return", CloseBetSwitchPanel, parent: parent1.transform, style: 2, scale: 1);

        curSelect = selectionList.Count - 2 - IOCanvasModel.Instance.tempBetSwitchCount;//减2个按钮+1个数据(暂时1个)
        SetCurSelect(curSelect);//默认选中该界面的第一个

    }

    //void OnBetSwitchSectionClick()
    //{

    //    var dateSection = selectionList[curSelect] as IOBaseSection;
    //    dateSection.Selected = true;
    //    selectSection.selected = dateSection.Selected;
    //    selectSection.baseSection = dateSection;
    //    selectSection.state = IOSectionState.BetSwitchConnect;
    //    selectSection.baseSection.CurIndex = 0;



    //    if (State == IOState.BetSwitch)
    //    {
    //        IOArrowRect.gameObject.SetActive(true);
    //        IOArrowRect.anchoredPosition = dateSection.transform.localPosition;

    //        IOArrowRect.anchoredPosition += new Vector2(100, 0);

    //        IOArrowRect.SetAsLastSibling();
    //    }
    //}

    private void SaveDateTime()//时间设置的保存按钮
    {
        Debug.Log("保存时间设置");
        //if (!Application.isEditor)
        SBoxSandbox.SetDateTime(IOCanvasModel.Instance.IODateTime);
    }

    public void CloseModifiedDatePanel(bool isresetDate = false)//时间设置的关闭按钮
    {
        if(isresetDate)IOCanvasModel.Instance.IODateTime = oldDateTime;
        Destroy(modDatePanel);
        IOArrowRect.gameObject.SetActive(false);
        State = IOState.SelectFunction;
        int removeIdx = selectionList.Count - 3;
        for (int i = 0; i < 3; i++)
            selectionList.RemoveAt(removeIdx);
        selectSection.selected = false;
        SetCurSelect();
    }


    private void InitPrintSettingPanel()
    {
        State = IOState.PrintSetting;
        SBoxIdea.BattPrinterOffsetGet();
    }

    public void ShowPrintSettingPanel()
    {
        modDatePanel = Instantiate(modDate, transform);
        var rect = modDatePanel.GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        rect.localScale = Vector3.one;
        var parent = modDatePanel.transform;

        GameObject parent2 = new GameObject();
        parent2.AddComponent<HorizontalLayoutGroup>();
        parent2.transform.SetParent(parent, false);
        parent2.transform.localScale = Vector3.one;
        var parentRect2 = parent2.GetComponent<RectTransform>();
        parentRect2.anchoredPosition = new Vector2(0, 20);
        parentRect2.sizeDelta = new Vector2(900, 85);

        State = IOState.PrintSetting;
        //创建偏移设置
        IOCanvasModel.Instance.tempIOPrintSettingDate = new int[IOCanvasModel.Instance.IOPrintSettingDate.Length];
        for (int i = 0; i < IOCanvasModel.Instance.IOPrintSettingDate.Length; i++)
            IOCanvasModel.Instance.tempIOPrintSettingDate[i] = IOCanvasModel.Instance.IOPrintSettingDate[i];
        string[] nameStr = new string[2] { "PrintForWardOffset", "PrintBackwardOffset" };

        for (int i = 0; i < IOCanvasModel.Instance.tempIOPrintSettingDate.Length; i++)
        {
            int tempI = i;
            int tempIndex = selectionList.Count;
            IOBaseSection item = InstantiateBetSwitchSection(Utils.GetLanguage(nameStr[i]), defaultValue: IOCanvasModel.Instance.tempIOPrintSettingDate[i],style: 0,contentOffset:0, onClick: () =>
            {
                IOCanvasModel.Instance.curPrintOffsetIndex = tempI;
                OnSectionClick(tempIndex, IOSectionState.PrintOffset);
            }, trans: parentRect2, fontSize: 50);
        }

        GameObject parent1 = new GameObject();
        parent1.AddComponent<HorizontalLayoutGroup>();
        parent1.transform.SetParent(parent, false);
        parent1.transform.localScale = Vector3.one;
        var parentRect1 = parent1.GetComponent<RectTransform>();
        parentRect1.anchoredPosition = new Vector2(0, -120);
        parentRect1.sizeDelta = new Vector2(705, 85);
        InstantiateBaseBtn("Save", SaveDatePrintSetting, parent: parent1.transform, style: 2, scale: 1);
        InstantiateBaseBtn("Return", () => ClosePrintSettingPanel(), parent: parent1.transform, style: 2, scale: 1);

        curSelect = selectionList.Count - 2 - IOCanvasModel.Instance.tempIOPrintSettingDate.Length;
        SetCurSelect(curSelect);
    }

    private void SaveDatePrintSetting()
    {
        IOCanvasModel.Instance.IOPrintSettingDate[0] = IOCanvasModel.Instance.tempIOPrintSettingDate[0];
        IOCanvasModel.Instance.IOPrintSettingDate[1] = IOCanvasModel.Instance.tempIOPrintSettingDate[1];
        int hs = IOCanvasModel.Instance.IOPrintSettingDate[0];
        int ls = IOCanvasModel.Instance.IOPrintSettingDate[1];
        SBoxIdea.BattPrinterOffsetSet(hs, ls);
    }

    public void ClosePrintSettingPanel()
    {
        Destroy(modDatePanel);
        IOArrowRect.gameObject.SetActive(false);
        State = IOState.SelectFunction;
        int count = 2 + IOCanvasModel.Instance.IOPrintSettingDate.Length;
        int removeIdx = selectionList.Count - count;
        for (int i = 0; i < count; i++)
            selectionList.RemoveAt(removeIdx);
        selectSection.selected = false;
        SetCurSelect();
    }

    private void InitBillSettingPanel()//紙鈔機設置入口
    {
        ClearMenuBtn();
        title.text = Utils.GetLanguage(IOFunction.BillSetting.ToString());
        subtitle.text = "";
        billBtnList.Clear();
        State = IOState.BillSetting;
        SBoxSandbox.BillListGet();
    }

    private void OnGetBillList()
    {
        if (State == IOState.BillSetting)
            ShowBillSettingPanel();
    }

    private void OnGetBillState()
    {
        if (State == IOState.BillSetting)
            {
                billRefreshCoroutine = StartCoroutine(RefreshBillSelectedUI());
            }
    }

    public void ShowBillSettingPanel()
    {
        SetPrinterAndBillPanelGridLayout();
        //刚进入界面,读取当前纸钞机状态
        IOCanvasModel.Instance.IOBillSelectIndex = SBoxSandbox.BillState();

        int count = Mathf.Min(IOCanvasModel.Instance.IOBillList.Count, IOCanvasModel.MAX_BILL_BTN);
        for (int i = 0; i < count; i++)
        {
            int index = i;
            //型号名直接来自算法卡,GetLanguage 查不到时会原样返回,故可直接传入
            IOBaseBtn btn = InstantiateBaseBtn(IOCanvasModel.Instance.IOBillList[i], () => OnBillBtnClick(index), fontSize: 55, scale: 1);
            billBtnList.Add(btn);
        }

        //Return 按钮,布局与背景音乐设置一致
        GameObject parent = new GameObject();
        tempObjList.Add(parent);
        parent.AddComponent<HorizontalLayoutGroup>();
        parent.transform.SetParent(transform, false);
        var parentRect = parent.GetComponent<RectTransform>();
        parentRect.anchoredPosition = new Vector2(0, -430);
        parentRect.sizeDelta = new Vector2(975, 100);
        parentRect.localScale = Vector3.one;
        InstantiateBaseBtn("Return", ReturnToFunction, parent: parent.transform, style: 2, scale: 1);

        //同步当前选中状态
        SetBillSelectedUI(IOCanvasModel.Instance.IOBillSelectIndex);

        curSelect = 0;
        SetCurSelect();
        IOPopTips.Instance.transform.SetAsLastSibling();
    }

    private void OnBillBtnClick(int index)
    {
        //编号规则:与 BillListGet 列表下标一致,从 0 开始;选中态等回包后由 BillState 刷新
        SBoxSandbox.BillSelect(index);
    }

    private IEnumerator RefreshBillSelectedUI()//等待后重新读取 BillState,同步选中状态并提示(保存中提示已在 manager 处理)
    {
        if (billBtnList.Count == 0)
            yield break;
        yield return new WaitForSeconds(4f);
        int state = SBoxSandbox.BillState();
        SetBillSelectedUI(state);
        if (state >= 0)
            IOPopTips.Instance.ShowTips(Utils.GetLanguage("BillSetSucceed"));
        else if (state == -1)
            IOPopTips.Instance.ShowTips(Utils.GetLanguage("BillNotSelected"));
        else if (state == -2)
            IOPopTips.Instance.ShowTips(Utils.GetLanguage("BillNotConnected"));
    }

    private void SetBillSelectedUI(int index)
    {
        IOCanvasModel.Instance.IOBillSelectIndex = index;
        for (int i = 0; i < billBtnList.Count; i++)
            billBtnList[i].SetSelected(i == index);
    }

    private void InitPrinterSettingPanel()//打印機設置入口
    {
        ClearMenuBtn();
        title.text = Utils.GetLanguage(IOFunction.PrinterSetting.ToString());
        subtitle.text = "";
        printerBtnList.Clear();
        State = IOState.PrinterSetting;
        SBoxSandbox.PrinterListGet();
    }

    private void OnGetPrinterList()
    {
        if (State == IOState.PrinterSetting)
            ShowPrinterSettingPanel();
    }

    private void OnGetPrinterState()
    {
        if (State == IOState.PrinterSetting)
            {
                printerRefreshCoroutine = StartCoroutine(RefreshPrinterSelectedUI());
            }
    }

    private void SetPrinterAndBillPanelGridLayout()
    {
        gridLayout = menuPanel.GetComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(650f, 150f);
        gridLayout.padding = new RectOffset(0, 0, 0, 0);
        gridLayoutRect = menuPanel.GetComponent<RectTransform>();
        gridLayoutRect.anchorMin = new Vector2(0.5f, 0);
        gridLayoutRect.anchorMax = new Vector2(0.5f, 0);
        gridLayoutRect.sizeDelta = new Vector2(1920f, 950);
        gridLayoutRect.anchoredPosition = new Vector2(0, 470f);
        gridLayout.childAlignment = TextAnchor.UpperCenter;
        gridLayout.spacing = new Vector2(100f, 50f);
        gridLayout.startAxis = GridLayoutGroup.Axis.Vertical;
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 2;
    }

    public void ShowPrinterSettingPanel()
    {
        SetPrinterAndBillPanelGridLayout();
        //刚进入界面,读取当前打印机状态
        IOCanvasModel.Instance.IOPrinterSelectIndex = SBoxSandbox.PrinterState();

        int count = Mathf.Min(IOCanvasModel.Instance.IOPrinterList.Count, IOCanvasModel.MAX_PRINTER_BTN);
        for (int i = 0; i < count; i++)
        {
            int index = i;
            //型号名直接来自算法卡,GetLanguage 查不到时会原样返回,故可直接传入
            IOBaseBtn btn = InstantiateBaseBtn(IOCanvasModel.Instance.IOPrinterList[i], () => OnPrinterBtnClick(index), fontSize: 55, scale: 1);
            printerBtnList.Add(btn);
        }

        //Return 按钮,布局与背景音乐设置一致
        GameObject parent = new GameObject();
        tempObjList.Add(parent);
        parent.AddComponent<HorizontalLayoutGroup>();
        parent.transform.SetParent(transform, false);
        var parentRect = parent.GetComponent<RectTransform>();
        parentRect.anchoredPosition = new Vector2(0, -430);
        parentRect.sizeDelta = new Vector2(975, 100);
        parentRect.localScale = Vector3.one;
        InstantiateBaseBtn("Return", ReturnToFunction, parent: parent.transform, style: 2, scale: 1);
        int addressBtnFontSize = IOCanvasModel.Instance.curlanguage == Language.en ? 35 : 52;//英文文案较长,用较小字号
        InstantiateBaseBtn("AddressSetting", OpenAddressSettingPanel, parent: parent.transform, style: 2, scale: 1, fontSize: addressBtnFontSize);//与返回横向并排,等高

        //同步当前选中状态
        SetPrinterSelectedUI(IOCanvasModel.Instance.IOPrinterSelectIndex);

        curSelect = 0;
        SetCurSelect();
        IOPopTips.Instance.transform.SetAsLastSibling();
    }

    private void OnPrinterBtnClick(int index)
    {
        //编号规则:与 PrinterListGet 列表下标一致,从 0 开始;选中态等回包后由 PrinterState 刷新
        SBoxSandbox.PrinterSelect(index);
    }

    /// <summary>
    /// 打开地址设置覆盖面板:铺满整个窗口,完全遮住下方打印机面板并拦截点击
    /// </summary>
    public void OpenAddressSettingPanel()
    {
        if (addressPanel != null)
            return;
        //先切状态再实例化:Instantiate 会同步执行 OnEnable,避免面板初始化时仍处于旧状态
        State = IOState.AddressSetting;
        addressPanel = Instantiate(baseAddress, transform).GetComponent<IOAddressPanel>();
        addressPanel.transform.SetAsLastSibling();//置顶渲染,盖住当前面板
        string address = PlayerPrefs.HasKey("PrinterAddress") ? PlayerPrefs.GetString("PrinterAddress") : "";
        addressPanel.SetParams("EnterAddress", OnAddressConfirm, CloseAddressSettingPanel, address);
        IOPopTips.Instance.transform.SetAsLastSibling();//提示保持在最上层
    }

    /// <summary>
    /// 关闭地址设置覆盖面板,回到打印机设置面板
    /// </summary>
    public void CloseAddressSettingPanel()
    {
        if (addressPanel == null)
            return;
        Destroy(addressPanel.gameObject);
        addressPanel = null;
        State = IOState.PrinterSetting;
        //光标回到"地址设置"按钮(selectionList 最后一个)
        SetCurSelect(selectionList.Count - 1);
    }

    /// <summary>
    /// 地址设置确认:写入本地 PlayerPrefs 并关闭面板
    /// </summary>
    private void OnAddressConfirm(string address)
    {
        PlayerPrefs.SetString("PrinterAddress", address ?? "");
        PlayerPrefs.Save();
        CloseAddressSettingPanel();
    }

    private IEnumerator RefreshPrinterSelectedUI()//等待后重新读取 PrinterState,同步选中状态并提示(保存中提示已在 manager 处理)
    {
        if (printerBtnList.Count == 0)
            yield break;
        yield return new WaitForSeconds(4f);
        int state = SBoxSandbox.PrinterState();
        SetPrinterSelectedUI(state);
        if (state >= 0)
            IOPopTips.Instance.ShowTips(Utils.GetLanguage("PrinterSetSucceed"));
        else if (state == -1)
            IOPopTips.Instance.ShowTips(Utils.GetLanguage("PrinterNotSelected"));
        else if (state == -2)
            IOPopTips.Instance.ShowTips(Utils.GetLanguage("PrinterNotConnected"));
    }

    private void SetPrinterSelectedUI(int index)
    {
        IOCanvasModel.Instance.IOPrinterSelectIndex = index;
        for (int i = 0; i < printerBtnList.Count; i++)
            printerBtnList[i].SetSelected(i == index);
    }


    private void SaveBetSwitch()
    {
        for (int i = 0; i < IOCanvasModel.Instance.tempBetSwitchCount; i++)//betSwitchList 数组是最大个数的,但是 tempBetSwitchCount 可能不是最大个数的
        {
            IOCanvasModel.Instance.BetSwitchList[i] = IOCanvasModel.Instance.tempBetSwitchList[i];
        }
        IOCanvasModel.Instance.BetSwitchCount = IOCanvasModel.Instance.tempBetSwitchCount;
        //补默认倍率值(未配置位) + 未显示位清零,保证与算法卡20002回包一致
        IOCanvasModel.Instance.ApplyBetSwitchCount(IOCanvasModel.Instance.tempBetSwitchCount);
        Debug.LogError($"BetSwitchList:{JsonConvert.SerializeObject(IOCanvasModel.Instance.BetSwitchList)}");
        //弹个提示
        IOPopTips.Instance.ShowTips(Utils.GetLanguage("SaveSucceed"));
        //写回算法卡(20003),与读取方向一致
        IOCanvasModel.Instance.tempCfgData.BetSwitch = IOCanvasModel.Instance.BetSwitchList;
        SBoxIdea.WriteConf(IOCanvasModel.Instance.tempCfgData);

    }

    public void CloseBetSwitchPanel()
    {
        Destroy(modDate2Panel);
        IOArrowRect.gameObject.SetActive(false);
        State = IOState.Params;
        //下面用于键盘操控
        //betSwitch 界面的实例化个数,n个数据,2个按钮
        int tempCount = 2 + IOCanvasModel.Instance.tempBetSwitchCount;
        int removeIdx = selectionList.Count - tempCount;
        for (int i = 0; i < tempCount; i++)
            selectionList.RemoveAt(removeIdx);
        //selectSection.selected = false;
        SetCurSelect();
    }

    private void SetEditPasswordGridLayout()
    {
        gridLayout.startAxis = GridLayoutGroup.Axis.Vertical;
        gridLayout.childAlignment = TextAnchor.UpperCenter;
        gridLayout.padding = new RectOffset(0, 0, 20, 0);
        gridLayout.spacing = new Vector2(40, 16);
        gridLayout.cellSize = new Vector2(600, 75);
        gridLayoutRect.sizeDelta = new Vector2(845, 960);
        gridLayoutRect.anchoredPosition = new Vector2(0, 505);
    }

    private void ReturnToFunction()
    {
        //返回时停止纸钞机/打印机的延迟刷新协程(避免其恢复时操作已销毁的按钮)
        if (billRefreshCoroutine != null) { StopCoroutine(billRefreshCoroutine); billRefreshCoroutine = null; }
        if (printerRefreshCoroutine != null) { StopCoroutine(printerRefreshCoroutine); printerRefreshCoroutine = null; }
        //IOCanvasModel.Instance.SoundVolumScale = oldSoundVolumScale;
        IOCanvasModel.Instance.jackpotCfg.jackpotSwitch = oldJackpotConfig.jackpotSwitch;
        IOCanvasModel.Instance.jackpotCfg.jpPercent = oldJackpotConfig.jpPercent;
        IOCanvasModel.Instance.jackpotCfg.betPercent = oldJackpotConfig.betPercent;
        if (passwordPanel != null)
        {
            Destroy(passwordPanel.gameObject);
            passwordPanel = null;
        }
        if (addressPanel != null)
        {
            Destroy(addressPanel.gameObject);
            addressPanel = null;
        }
        scoreShowBtn = null;
        betDataShowList.Clear();
        for (int i = 0; i < titleLeftList.Count; i++)
            titleLeftList[i].gameObject.SetActive(false);
        for (int i = 0; i < titleRightList.Count; i++)
            titleRightList[i].gameObject.SetActive(false);
        IOArrowRect.gameObject.SetActive(false);
        ClearMenuBtn();
        InitFunctionPanel();
        State = IOState.SelectFunction;
    }

    private void OnSectionClick(int index, IOSectionState sectionState)
    {
        curSelect = index;
        var section = selectionList[curSelect] as IOBaseSection;

        for (int i = 0; i < selectionList.Count; i++)
            selectionList[i].Color = IOCanvasModel.Instance.textNormalColor;

        section.Selected = true;
        selectSection.selected = section.Selected;
        selectSection.baseSection = section;
        selectSection.state = sectionState;
        if (!selectSection.selected)
            SetCurSelect(curSelect, false);

        if (State == IOState.Params || State == IOState.DateTime || State == IOState.JackpotSetting || State == IOState.BetSwitch || State == IOState.PrintSetting || State == IOState.BgmSetting)
        {
            IOArrowRect.gameObject.SetActive(true);
            IOArrowRect.anchoredPosition = section.transform.localPosition;

            switch (State)
            {
                case IOState.PrintSetting:
                    IOArrowRect.anchoredPosition += new Vector2(150, 20);
                    break;
                case IOState.BgmSetting://背景音乐设置,布局同JackpotSetting
                    IOArrowRect.anchoredPosition += new Vector2(180, -70);
                    break;
                case IOState.BetSwitch:
                    if (curSelect > 10)
                        IOArrowRect.anchoredPosition += new Vector2(185, 10);
                    else
                        IOArrowRect.anchoredPosition += new Vector2(255, 10);
                    break;
                case IOState.DateTime:
                    IOArrowRect.anchoredPosition += new Vector2(410, 0);
                    break;
                case IOState.Params:
                    if (curSelect > 10)
                        IOArrowRect.anchoredPosition += new Vector2(185, 10);
                    else
                        IOArrowRect.anchoredPosition += new Vector2(255, 10);
                    break;
                case IOState.JackpotSetting:
                    IOArrowRect.anchoredPosition += new Vector2(180, -70);
                    break;
                default:
                    break;
            }



            //if (selectSection.state != IOSectionState.DateTime && State == IOState.Params)
            //{
            //    if (curSelect > 10)
            //        IOArrowRect.anchoredPosition += IOCanvasModel.Instance.curlanguage == Language.ch ? new Vector2(-185, 20) : new Vector2(-185, 20);
            //    else
            //        IOArrowRect.anchoredPosition += IOCanvasModel.Instance.curlanguage == Language.ch ? new Vector2(-75, 20) : new Vector2(-75, 20);
            //}
            //else if (selectSection.state == IOSectionState.DateTime)
            //    IOArrowRect.anchoredPosition += new Vector2(410, 0);

            IOArrowRect.SetAsLastSibling();
        }
    }

    private void OnEditIdSectionClick(int index, IOSectionState sectionState)
    {
        if (!IOCanvasModel.Instance.CanEditMachineId) return;
        IOBaseSection section = selectionList[index] as IOBaseSection;
        section.Selected = true;
        selectSection.selected = true;
        selectSection.baseSection = section;
        selectSection.state = sectionState;
        int setSelectIdx = IOCanvasModel.Instance.permissions == 3 ? 2 : 1;//如果有线号,设备号两个,就从2开始,如果只有线号或者设备号其中一个就从1开始

        SetCurSelect(setSelectIdx, false);
    }

    private void EditIdNumBtnClick(int num)
    {
        if (!selectSection.selected) return;
        int data = selectSection.state == IOSectionState.EditLineId ? IOCanvasModel.Instance.tempCfgData.LineId : IOCanvasModel.Instance.tempCfgData.MachineId;
        string str = data != 0 ? data.ToString() + num.ToString() : num.ToString();
        if (str.Length > 9) return;
        if (IOCanvasModel.Instance.permissions == 3)
        {
            if (num == 0)
                SetCurSelect(12);
            else
                SetCurSelect(num + 1);
        }
        else
            if (num == 0)
            SetCurSelect(11);
        else
            SetCurSelect(num);
        selectSection.baseSection.Content = str;
        if (selectSection.state == IOSectionState.EditLineId)
            IOCanvasModel.Instance.tempCfgData.LineId = int.Parse(str);
        else
            IOCanvasModel.Instance.tempCfgData.MachineId = int.Parse(str);
    }

    private void EditIdDeleteBtnClick()
    {
        if (!selectSection.selected) return;
        SetCurSelect(11);
        int data = selectSection.state == IOSectionState.EditLineId ? IOCanvasModel.Instance.tempCfgData.LineId : IOCanvasModel.Instance.tempCfgData.MachineId;
        string str = data.ToString().Substring(0, data.ToString().Length - 1);
        selectSection.baseSection.Content = str;
        if (selectSection.state == IOSectionState.EditLineId)
            IOCanvasModel.Instance.tempCfgData.LineId = string.IsNullOrEmpty(str) ? 0 : int.Parse(str);
        else
            IOCanvasModel.Instance.tempCfgData.MachineId = string.IsNullOrEmpty(str) ? 0 : int.Parse(str);
    }

    private void InstantiateBlank()
    {
        InstantiateEasyShow("", "");
    }

    private IOBaseShow InstantiateEasyShow(string title, string content, bool showBg = false, TextAnchor contentAlignment = TextAnchor.MiddleLeft, int fontSize = 30, Transform parent = null, float scale = 1f)
    {
        fontSize = fontSize == 30 ? IOCanvasModel.Instance.defaultFontSize : fontSize;
        return InstantiateBaseShow(title, new List<string>() { content }, showBg, contentAlignment: contentAlignment, fontSize: fontSize, parent: parent, scale: scale);
    }

    private IOBaseShow InstantiateBaseShow(string title, List<string> contentList, bool showBg = false, Transform parent = null, TextAnchor contentAlignment = TextAnchor.MiddleLeft, int fontSize = 30, int style = 0, float scale = 1.75f)
    {
        IOBaseShow baseShow;
        switch (style)
        {
            case 0:
                baseShow = Instantiate(baseShowPrefab).GetComponent<IOBaseShow>();
                break;
            case 1:
                baseShow = Instantiate(baseShowPrefab1).GetComponent<IOBaseShow>();
                break;
            case 2:
                baseShow = Instantiate(baseShowPrefab2).GetComponent<IOBaseShow>();
                break;
            case 3:
                baseShow = Instantiate(baseShowPrefab3).GetComponent<IOBaseShow>();
                break;
            case 4:
                baseShow = Instantiate(baseShowPrefab4).GetComponent<IOBaseShow>();
                break;
            case 5:
                baseShow = Instantiate(baseShowPrefab5).GetComponent<IOBaseShow>();
                break;
            case 6:
                baseShow = Instantiate(baseShowPrefab6).GetComponent<IOBaseShow>();
                break;
            default:
                baseShow = Instantiate(baseShowPrefab).GetComponent<IOBaseShow>();
                break;
        }
        if (parent)
            baseShow.SetParentAndReset(parent);
        else
            baseShow.SetParentAndReset(menuPanel);
        baseShow.ShowBg = showBg;
        baseShow.title = title;
        if (baseShow.titleText != null)
        {
            baseShow.titleText.fontSize = fontSize;
            baseShow.titleText.text = Utils.GetLanguage(title);
        }
        for (int i = 0; i < contentList.Count; i++)
        {
            baseShow.contentList[i].text = contentList[i];
            baseShow.contentList[i].alignment = contentAlignment;
            baseShow.contentList[i].gameObject.SetActive(true);
            baseShow.contentList[i].fontSize = fontSize;
        }
        onlyShowList.Add(baseShow);
        baseShow.transform.localScale = Vector3.one * scale;
        return baseShow;
    }

    private IOBaseBtn InstantiateBaseBtn(string str, UnityAction unityAction, bool showBg = true, TextAnchor textAnchor = TextAnchor.MiddleCenter, Transform parent = null, int style = 1, int fontSize = 20, float scale = 1.75f)
    {
        IOBaseBtn baseBtn;
        switch (style)
        {
            case 0:
                baseBtn = Instantiate(baseBtnPrefab).GetComponent<IOBaseBtn>();
                break;
            case 1:
                baseBtn = Instantiate(baseBtnPrefab1).GetComponent<IOBaseBtn>();
                break;
            case 2:
                baseBtn = Instantiate(baseBtnPrefab2).GetComponent<IOBaseBtn>();
                break;
            default:
                baseBtn = Instantiate(baseBtnPrefab).GetComponent<IOBaseBtn>();
                break;
        }
        if (parent)
            baseBtn.SetParentAndReset(parent);
        else
            baseBtn.SetParentAndReset(menuPanel);
        baseBtn.ShowBg = showBg;
        baseBtn.style = style;
        baseBtn.title = str;
        baseBtn.titleText.fontSize = fontSize == 20 ? (style == 2 ? 52 : fontSize) : fontSize;
        baseBtn.titleText.text = Utils.GetLanguage(str);
        baseBtn.titleText.alignment = textAnchor;
        baseBtn.transform.localScale = scale * Vector3.one;
        baseBtn.AddListener(unityAction);
        selectionList.Add(baseBtn);//增加
        return baseBtn;
    }

    private IOBaseToggle InstantiateBaseToggle(string str, UnityAction<bool> unityAction, bool showBg = true, TextAnchor textAnchor = TextAnchor.MiddleCenter, Transform parent = null, ToggleGroup toggleGroup = null, float scale = 1)
    {
        IOBaseToggle baseToggle = Instantiate(baseTogglePrefab).GetComponent<IOBaseToggle>();
        if (parent)
            baseToggle.SetParentAndReset(parent);
        else
            baseToggle.SetParentAndReset(menuPanel);
        if (toggleGroup != null)
            baseToggle.toggle.group = toggleGroup;
        baseToggle.ShowBg = showBg;
        baseToggle.style = 1;
        baseToggle.title = str;
        baseToggle.titleText.fontSize = IOCanvasModel.Instance.curlanguage == Language.en ? 30 : 45;
        baseToggle.titleText.text = str;
        baseToggle.titleText.alignment = textAnchor;
        baseToggle.AddListener(unityAction);
        baseToggle.transform.localScale = Vector3.one * scale;
        selectionList.Add(baseToggle);
        return baseToggle;
    }

    private void InstantiateEasySection(string title, string content, UnityAction onClick = null, bool showBg = false, int style = 0, Transform parent = null)
    {
        string[] strings = new string[] { content };
        InstantiateBaseSection(title, strings, onClick: onClick, showBg: showBg, style: style, parent: parent, fontSize: 50);
    }

    private void InstantiateBaseSection(string title, string[] contents, int index = 0, UnityAction onClick = null, bool showBg = false, int style = 0, Transform parent = null, int fontSize = -1, int contentOffset = 0)
    {
        IOBaseSection baseSelection = style == 0
            ? Instantiate(baseSectionPrefab).GetComponent<IOBaseSection>()
            : Instantiate(baseSectionPrefab1).GetComponent<IOBaseSection>();
        if (parent)
            baseSelection.SetParentAndReset(parent);
        else
            baseSelection.SetParentAndReset(menuPanel);
        baseSelection.ShowBg = showBg;
        baseSelection.title = title;
        baseSelection.titleText.text = Utils.GetLanguage(title);
        baseSelection.titleText.fontSize = fontSize == -1 ? IOCanvasModel.Instance.defaultFontSize : fontSize;
        baseSelection.contentText.fontSize = fontSize == -1 ? IOCanvasModel.Instance.defaultFontSize : fontSize;
        if(contentOffset != 0)//※群体操作
        {
            var temp = baseSelection.contentText.transform.localPosition;
            temp.x+=contentOffset;
            baseSelection.contentText.transform.localPosition = temp;
        }
        baseSelection.contents = contents;
        baseSelection.CurIndex = index;
        baseSelection.AddListener(onClick);
        baseSelection.transform.localScale = Vector3.one;
        selectionList.Add(baseSelection);
    }

    private void InstantiateBaseSection(string title, int defaultValue = 0, UnityAction onClick = null, bool showBg = false, int style = 0, Transform trans = null, IOParams ioParams = IOParams.Return, int fontSize = -1, int contentOffset = 0)
    {
        IOBaseSection baseSelection = style == 0
            ? Instantiate(baseSectionPrefab).GetComponent<IOBaseSection>()
            : Instantiate(baseSectionPrefab1).GetComponent<IOBaseSection>();
        if (trans)
            baseSelection.SetParentAndReset(trans);
        else
            baseSelection.SetParentAndReset(menuPanel);
        baseSelection.ShowBg = showBg;
        baseSelection.title = title;
        baseSelection.titleText.text = Utils.GetLanguage(title);
        baseSelection.titleText.fontSize = fontSize == -1 ? IOCanvasModel.Instance.defaultFontSize : fontSize;
        baseSelection.contentText.fontSize = fontSize == -1 ? IOCanvasModel.Instance.defaultFontSize : fontSize;
        if(contentOffset != 0)//※群体操作
        {
            var temp = baseSelection.contentText.transform.localPosition;
            temp.x+=contentOffset;
            baseSelection.contentText.transform.localPosition = temp;
        }
        baseSelection.ioParams = ioParams;
        baseSelection.CurIndex = defaultValue;
        baseSelection.AddListener(onClick);
        baseSelection.transform.localScale = Vector3.one;
        selectionList.Add(baseSelection);
    }

    private IOBaseSection InstantiateBetSwitchSection(string title, int defaultValue = 0, UnityAction onClick = null, bool showBg = false, int style = 0, Transform trans = null, IOParams ioParams = IOParams.Return, int fontSize = -1, int contentOffset = -100)
    {
        IOBaseSection baseSelection = style == 0
            ? Instantiate(baseSectionPrefab).GetComponent<IOBaseSection>()
            : Instantiate(baseSectionPrefab1).GetComponent<IOBaseSection>();
        if (trans)
            baseSelection.SetParentAndReset(trans);
        else
            baseSelection.SetParentAndReset(menuPanel);
        baseSelection.ShowBg = showBg;
        baseSelection.title = title;
        baseSelection.titleText.text = Utils.GetLanguage(title);
        baseSelection.titleText.fontSize = fontSize == -1 ? IOCanvasModel.Instance.defaultFontSize : fontSize;
        baseSelection.contentText.fontSize = fontSize == -1 ? IOCanvasModel.Instance.defaultFontSize : fontSize;

        if(contentOffset != -100)//※群体操作
        {
            var temp = baseSelection.contentText.transform.localPosition;
            temp.x+=contentOffset;
            baseSelection.contentText.transform.localPosition = temp;
        }

        baseSelection.ioParams = ioParams;
        baseSelection.CurIndex = defaultValue;//betSwitch 
        baseSelection.AddListener(onClick);
        baseSelection.transform.localScale = Vector3.one;
        selectionList.Add(baseSelection);
        return baseSelection;
    }

    private void InstantiateTicketRatioSection(string title, UnityAction onClick = null, bool showBg = false)
    {
        IOBaseSection baseSelection = Instantiate(ticketRatioPrefab).GetComponent<IOBaseSection>();
        baseSelection.SetParentAndReset(menuPanel);
        baseSelection.ShowBg = showBg;
        baseSelection.title = title;
        baseSelection.titleText.text = Utils.GetLanguage(title);
        baseSelection.titleText.fontSize = IOCanvasModel.Instance.defaultFontSize;
        baseSelection.contentText.fontSize = IOCanvasModel.Instance.defaultFontSize;
        (baseSelection as IOTicketRatioSection).SetCurIndex();
        baseSelection.AddListener(onClick);
        baseSelection.transform.localScale = Vector3.one;
        selectionList.Add(baseSelection);
    }

    private void InistantiateSwitchSection(string title, List<int> orignalList, UnityAction onClick = null, bool showBg = false)
    {
        IOSwitchSection switchSection = Instantiate(switchSectionsPrefab).GetComponent<IOSwitchSection>();
        switchSection.SetParentAndReset(menuPanel);
        switchSection.ShowBg = showBg;
        switchSection.title = title;
        switchSection.titleText.text = Utils.GetLanguage(title);
        switchSection.titleText.fontSize = IOCanvasModel.Instance.defaultFontSize;
        switchSection.OriganlList = orignalList;
        //switchSection.DataList = list;
        switchSection.AddListener(onClick);
        switchSection.transform.localScale = Vector3.one;
        selectionList.Add(switchSection);
    }

    private void InstantiateKeyBoard()
    {
        GameObject parent = new GameObject();
        tempObjList.Add(parent);
        var gridLayoutGroup = parent.AddComponent<GridLayoutGroup>();
        gridLayoutGroup.cellSize = new Vector2(217.5f, 106.5f);
        gridLayoutGroup.spacing = new Vector2(121f, 45f);
        gridLayoutGroup.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
        parent.transform.SetParent(transform, false);
        for (int i = 0; i < 12; i++)
        {
            if (i < 9)
            {
                int num = i + 1;
                InstantiateBaseBtn(num.ToString(), () => EditIdNumBtnClick(num), true, TextAnchor.MiddleCenter, fontSize: 72, parent: parent.transform, style: 0, scale: 1f);
            }
            else
            {
                int fontSize = IOCanvasModel.Instance.curlanguage == Language.en ? 52 : 80;
                switch (i)
                {
                    case 9:
                        InstantiateBaseBtn(Utils.GetLanguage("DELETE"), EditIdDeleteBtnClick, true, TextAnchor.MiddleCenter, fontSize: fontSize, parent: parent.transform, style: 0, scale: 1f);
                        break;
                    case 10:
                        InstantiateBaseBtn("0", () => EditIdNumBtnClick(0), true, TextAnchor.MiddleCenter, fontSize: 72, parent: parent.transform, style: 0, scale: 1f);
                        break;
                    case 11:
                        InstantiateBaseBtn(Utils.GetLanguage("CONFIRM"), () =>
                        {
                            if (!selectSection.selected) return;
                            SetCurSelect(0);
                            selectSection.selected = false;
                            //SaveConfig();
                        }, true, TextAnchor.MiddleCenter, fontSize: fontSize, parent: parent.transform, style: 0, scale: 1f);
                        break;
                }
            }
        }
        var parentRect = parent.GetComponent<RectTransform>();
        parentRect.sizeDelta = new Vector2(1000, 535);
        parentRect.anchoredPosition = new Vector2(0, 24f);
        parentRect.localScale = Vector3.one;
    }

    private IODateTimeSection InistantiateDateTimeSection(string title, UnityAction onClick = null, bool showBg = false, Transform parent = null, float scale = 1f)
    {
        IODateTimeSection dateTimeSection = Instantiate(dateTimeSectionsPrefab).GetComponent<IODateTimeSection>();
        if (parent)
            dateTimeSection.SetParentAndReset(parent);
        else
            dateTimeSection.SetParentAndReset(menuPanel);
        dateTimeSection.ShowBg = showBg;
        dateTimeSection.title = title;
        dateTimeSection.titleText.text = Utils.GetLanguage(title);
        dateTimeSection.AddListener(onClick);
        dateTimeSection.InitDateTimeView();
        dateTimeSection.transform.localScale = Vector3.one * scale;
        selectionList.Add(dateTimeSection);
        return dateTimeSection;
    }

    private void SwitchSectionClick()//只能用于鼠标
    {
        //OnSectionClick((int)IOParams.SkillMode, IOSectionState.SkillMode);
        Debug.LogError(Input.mousePosition.x);
        if (Input.mousePosition.x <= 1100)
            selectSection.baseSection.CurIndex = 0;
        else if (Input.mousePosition.x > 1100 && Input.mousePosition.x < 1170)
            selectSection.baseSection.CurIndex = 1;
        else
            selectSection.baseSection.CurIndex = 2;

        //#if !UNITY_EDITOR && UNITY_ANDROID 
        if (IOCanvasModel.Instance.isTouchClick)
        {
            float num = Screen.width / 1280f;//比例系数
            if (IOCanvasModel.Instance.switchTouchPointPos.x <= 1100 * num)
                selectSection.baseSection.CurIndex = 0;
            else if (IOCanvasModel.Instance.switchTouchPointPos.x > 1100 * num && IOCanvasModel.Instance.switchTouchPointPos.x < 1170 * num)
                selectSection.baseSection.CurIndex = 1;
            else
                selectSection.baseSection.CurIndex = 2;
        }
        //#endif
    }



    private void DateTimeSectionClick()
    {
        int idx = selectionList.Count - 3;
        OnSectionClick(idx, IOSectionState.DateTime);
        if (Input.mousePosition.x <= 530)
            selectSection.baseSection.CurIndex = 0;
        else if (Input.mousePosition.x > 530 && Input.mousePosition.x <= 640)
            selectSection.baseSection.CurIndex = 1;
        else if (Input.mousePosition.x > 640 && Input.mousePosition.x <= 740)
            selectSection.baseSection.CurIndex = 2;
        else if (Input.mousePosition.x > 740 && Input.mousePosition.x <= 800)
            selectSection.baseSection.CurIndex = 3;
        else
            selectSection.baseSection.CurIndex = 4;

        if (IOCanvasModel.Instance.isTouchClick)
        {
            float num = Screen.width / 1280f;//比例系数
            if (IOCanvasModel.Instance.dateTimeTouchPointPos.x <= 530 * num)
                selectSection.baseSection.CurIndex = 0;
            else if (IOCanvasModel.Instance.dateTimeTouchPointPos.x > 530 * num && IOCanvasModel.Instance.dateTimeTouchPointPos.x <= 640 * num)
                selectSection.baseSection.CurIndex = 1;
            else if (IOCanvasModel.Instance.dateTimeTouchPointPos.x > 640 * num && IOCanvasModel.Instance.dateTimeTouchPointPos.x <= 740 * num)
                selectSection.baseSection.CurIndex = 2;
            else if (IOCanvasModel.Instance.dateTimeTouchPointPos.x > 740 * num && IOCanvasModel.Instance.dateTimeTouchPointPos.x <= 800 * num)
                selectSection.baseSection.CurIndex = 3;
            else /*if (IOCanvasModel.Instance.dateTimeTouchPointPos.x > 800 && IOCanvasModel.Instance.dateTimeTouchPointPos.x <= 860)*/
                selectSection.baseSection.CurIndex = 4;
        }
    }

    public void SetCurSelect(int index = 0, bool setColor = true)
    {
        if (setColor)
        {
            for (int i = 0; i < selectionList.Count; i++)
                selectionList[i].Color = IOCanvasModel.Instance.textNormalColor;

            selectionList[index].Color = IOCanvasModel.Instance.textSelectedColor;
        }
        curSelect = index;
        if ((State == IOState.Code && index == 12)
            || ((State == IOState.EditPassword && index == 12)))
            passwordPanel.SetCurSelect(index, false);
        if (State == IOState.ProSetting)
        {
            int offset = 1;
            for (int i = offset + 1; i < selectionList.Count - 1; i++)
                selectionList[i].transform.Find("Select").gameObject.SetActive(false);//中间的数字全部先设置为默认状态
            for (int i = 0; i < selectionList.Count; i++)
            {
                if (selectionList[i] != selectSection.baseSection)
                    selectionList[i].Color = IOCanvasModel.Instance.textNormalColor;//除了选中的,其他没被选择的字体颜色设置为默认状态
            }
            if (curSelect > offset && curSelect < selectionList.Count - 2)
                selectionList[curSelect].transform.Find("Select").gameObject.SetActive(true);
            else
                selectionList[index].Color = IOCanvasModel.Instance.textSelectedColor;
        }
    }

    private void RefreshFunctionText()
    {
        title.text = Utils.GetLanguage("Menu");
        subtitle.text = "";

        for (int i = 0; i < selectionList.Count; i++)
        {
            if (selectionList[i] is IOBaseSection)
            {
                IOBaseSection baseSection = selectionList[i] as IOBaseSection;
                baseSection.titleText.text = Utils.GetLanguage(baseSection.title);
                for (int j = 0; j < baseSection.contents.Length; j++)
                    baseSection.contents[j] = Utils.GetLanguage(baseSection.contents[j]);
            }
            else if (selectionList[i] is IOBaseBtn)
            {
                IOBaseBtn baseBtn = selectionList[i] as IOBaseBtn;
                baseBtn.titleText.text = Utils.GetLanguage(baseBtn.title);
            }
        }
    }

    private void OnDisable()
    {
        RemoveEventListener();
    }
}