using GameUtil;
using Newtonsoft.Json;
using SBoxApi;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using ZXing.QrCode.Internal;

public class View : MonoBehaviour
{
    //子view
    //主界面
    private UIMainView uiMainView;
    //帮助界面
    private UIHelpView uiHelpView;
    //历史记录界面
    private UIHistoryView uiHistoryView;
    //压分结算界面
    private UIBetSettlementView uiBetSettlementView;
    //牌路界面
    public UIRoadMapView uiRoadMapView;
    //抢庄界面
    public UIBankerView uiBankerView;
    //触发器界面
    private UITriggerView uiTriggerView;
    //声音界面
    private UIAudioView uiAudioView;
    //主界面各种路
    private UIRecordView uiRecordView;

    //轮盘表现（装饰性，display 1 / PopCanvas）


    //netjacpot
    private GameObject nomalHelpContentObj;
    private Image netJPIcon;//是否是联网jp图片
    private NetJackpotView netJackpotView;
    public List<Text> betsMinOfNetJackpotTexts = new List<Text>();


    #region 变量
    //押注情况 文本
    private List<Text> winCountTexts = new List<Text>();//本轮押注中奖次数文本

    //UI动画
    private Animator poker3;//发牌动画组
    private Animator pokerTurn;//翻牌动画组
    private Animator poker3_2;
    private Animator pokerTurn_2;
    private Animator poker3_3;
    private Animator pokerTurn_3;
    private Animator poker3_4;
    private Animator pokerTurn_4;
    private Animator poker3_5;
    private Animator pokerTurn_5;
    private Animator poker3_6;
    private Animator pokerTurn_6;


    private Text CardDeckRemainCards;//剩余卡牌数

    // UI对象预制体实例
    private GameObject IOCanvasObj;
    private GameObject billCanvasObj;
    private SkeletonGraphic fireworkObj;//彩金特效
    private SkeletonGraphic mingPaiObj;//明牌logo特效

    // 视图列表
    private List<CardHover> betCoinViews = new List<CardHover>();//压分硬币视图
    public List<SymbolView> symbolViewsList = new List<SymbolView>();//押注视图和结果显示视图
    private GameObject symbolListObj;
    private List<CardView> cards = new List<CardView>();//幸运游戏卡片选择视图
    public GameObject settingMask;

    //spine 动画
    private GameObject target;
    SkeletonGraphic _skeletonGraphic;
    SkeletonGraphic skeletonGraphic
    {
        get
        {
            if (this._skeletonGraphic == null)
            {
                this._skeletonGraphic = target.GetComponent<SkeletonGraphic>();
            }
            return _skeletonGraphic;
        }
    }
    private string[] animList = new string[3] { "idle", "animation1", "animation2" };//,待机,发牌,发筹码
    int curAnimIdx = 0;//默认待机
    bool isPlayOver = false;//是否播放完毕

    //荷官发牌动画: 下标=牌位槽位(与 Poker3_1~Poker3_6 一一对应); poker3_2/poker3_3 共用 fp23
    private string[] dealerDealAnims = { "fp1", "fp23", "fp23", "fp4", "fp5", "fp6" };
    bool isDealing = false;//是否正在发牌:发牌期间荷官动画不可被其他动作打断

    private bool isFirstRefreshText = true;//是否是第一次刷新文本,只使用一次
    private MacInfoView macInfoView;

    // 原九门（Baccarat）界面是否在位：UICanvas 下 Center 节点缺失即视为已清除。
    // 纯轮盘机台会删除 UICanvas 下全部子物体，清除后原九门界面逻辑全部短路，
    // 只保留轮盘（PopCanvas / RouletteDeluxe）与设置/账单等非游戏功能。
    // 懒探测：UIView 组件的 Awake 可能早于 View.Awake，因此不能依赖初始化赋值顺序。
    private static bool? originalBaccaratUiPresentCache;
    public static bool OriginalBaccaratUiPresent
    {
        get
        {
            if (originalBaccaratUiPresentCache.HasValue) return originalBaccaratUiPresentCache.Value;
            GameObject uiCanvas = GameObject.Find("UICanvas");
            bool present = uiCanvas != null && uiCanvas.transform.Find("Center") != null;
            originalBaccaratUiPresentCache = present;
            Debug.Log("ROULETTE_ORIGINAL_UI_PRESENT: " + present);
            return present;
        }
    }
    #endregion




    private void Awake()
    {
        Controller.Instance.view = this;//额,V和C建立联系
        InitView();
    }

    private void Start()
    {
        InitViewData();
        SetNetJPIcon();
        if (Camera.allCameras.Length < 2)
        {
            Debug.LogError("摄像机数量少于2,不能实现双屏点击!!!");
            Debug.LogWarning("当前检测到的摄像机有:");
            foreach (var item in Camera.allCameras)
            {
                Debug.LogWarning($"name:{item.name},tag:{item.tag}");
            }
        }
        //Debug.Log("主游戏初始化完成,InitViewData completion!");
        EventCenter.Instance.EventTrigger(EventHandle.INIT_VIEW_FINISH);
    }
    public IEnumerator AutoBet()
    {
        PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_A, true);
        yield return new WaitForSeconds(0.5f);
        PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_A, false);
    }
    private float autoBetOffset = 2f;
    private float autoBetTime = 2f;
    private bool isAutoBet = false;
    private void Update()//dabao打包检查快捷触发
    {
        if (isAutoBet)
        {
            autoBetTime -= Time.deltaTime;
            if (autoBetTime < 0)
            {
                autoBetTime = autoBetOffset;
                StartCoroutine(AutoBet());
            }
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            List<JackBetInfo> betInfos = new List<JackBetInfo>();
            JackBetInfo betInfo = new JackBetInfo
            {
                gameType = IOCanvasModel.GameType,
                seat = 1,
                bet = 99999 * 100,
                betPercent = IOCanvasModel.Instance.JackpotCfg.betPercent,
                scoreRate = int.Parse(System.Math.Round(1000f / IOCanvasModel.Instance.CfgData.CoinValue).ToString()),
                JPPercent = IOCanvasModel.Instance.jackpotCfg.jpPercent,
            };
                betInfos.Add(betInfo);
            MsgInfo msgInfo = new MsgInfo
            {
                cmd = (int)C2S_CMD.C2S_JackBet,
                id = IOCanvasModel.Instance.CfgData.MachineId,
                jsonData = JsonConvert.SerializeObject(betInfos)
            };
            if (msgInfo.jsonData != null && msgInfo.jsonData.Length > 0)
                Debug.LogError($"C2S_JackBet:{msgInfo.jsonData}");//联网彩金中奖
            NetMgr.Instance.SendToServer(JsonConvert.SerializeObject(msgInfo));
        }
        if (Input.GetKeyDown(KeyCode.Space))
            Model.Instance.testMode = -1;
        if (Input.GetKeyDown(KeyCode.A))
            Model.Instance.testMode = 0;
        if (Model.Instance.canBidBanker)//只有不在明牌途中可以更改
        {
            if (Input.GetKeyDown(KeyCode.S))
                Model.Instance.testMode = 1;
            if (Input.GetKeyDown(KeyCode.D))
                Model.Instance.testMode = 2;
            if (Input.GetKeyDown(KeyCode.F))
                Model.Instance.testMode = 3;
            if (Input.GetKeyDown(KeyCode.G))
                Model.Instance.testMode = 4;
        }
        // if (Input.GetKeyDown(KeyCode.F1))
        // {
        //     Model.Instance.AddErrorCode(0, true);
        // }
        if (Input.GetKeyDown(KeyCode.M))
        {
            Controller.Instance.testLittleGame++;
            if(Controller.Instance.testLittleGame >= 6)Controller.Instance.testLittleGame = 0;
            Debug.LogError($"Controller.Instance.testLittleGame:{Controller.Instance.testLittleGame}");
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
                PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_SCORE_UP, true);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
                PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_SCORE_UP, false);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
                PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_SCORE_DOWN, true);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
                PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_SCORE_DOWN, false);
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            WinJackpotInfo winJackpotInfo = new WinJackpotInfo()
            {
                macId = 1,
                seat = 1,
                win = 600,
                jackpotId = 1,
                orderId = 1,
                time = -1
            };
            Controller.Instance.OnWinNetJackpot(winJackpotInfo);
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            WinJackpotInfo winJackpotInfo = new WinJackpotInfo()
            {
                macId = 1,
                seat = 3,
                win = 600,
                jackpotId = 1,
                orderId = 1,
                time = -1
            };
            Controller.Instance.OnWinNetJackpot(winJackpotInfo);
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            // 纯轮盘机台没有 UICanvas 界面，调试用的缩放打印跳过。
            if (!OriginalBaccaratUiPresent) return;
            var UICanvasS = transform.Find("UICanvas").GetComponent<CanvasScaler>();
            Debug.LogError(
                $"UI Scale Mode: {UICanvasS.uiScaleMode}\n" +
                $"Reference Resolution: ({UICanvasS.referenceResolution.x}, {UICanvasS.referenceResolution.y})\n" +
                $"Screen Match Mode: {UICanvasS.screenMatchMode}\n" +
                $"Match: {UICanvasS.matchWidthOrHeight}\n" +
                $"Scale Factor: {UICanvasS.scaleFactor}\n" +
                $"Reference Pixels Per Unit: {UICanvasS.referencePixelsPerUnit}"
            );

            var PopCanvasS = transform.Find("PopCanvas").GetComponent<CanvasScaler>();
            Debug.LogError(
                $"UI Scale Mode: {PopCanvasS.uiScaleMode}\n" +
                $"Reference Resolution: ({PopCanvasS.referenceResolution.x}, {PopCanvasS.referenceResolution.y})\n" +
                $"Screen Match Mode: {PopCanvasS.screenMatchMode}\n" +
                $"Match: {PopCanvasS.matchWidthOrHeight}\n" +
                $"Scale Factor: {PopCanvasS.scaleFactor}\n" +
                $"Reference Pixels Per Unit: {PopCanvasS.referencePixelsPerUnit}"
            );
        }

    }



#region 初始化

    private void InitView()
    {
        var UIView = transform.Find("UIViewManager");
        uiMainView = UIView.GetComponent<UIMainView>();
        //帮助界面
        uiHelpView = UIView.GetComponent<UIHelpView>();
        //历史记录界面
        uiHistoryView = UIView.GetComponent<UIHistoryView>();
        //牌路界面
        uiRoadMapView = UIView.GetComponent<UIRoadMapView>();
        //抢庄界面
        uiBankerView = UIView.GetComponent<UIBankerView>();
        //压分结算界面
        uiBetSettlementView = UIView.GetComponent<UIBetSettlementView>();
        uiAudioView = UIView.GetComponent<UIAudioView>();
        uiTriggerView = UIView.GetComponent<UITriggerView>();
        uiRecordView = UIView.GetComponent<UIRecordView>();

        var UICanvas = transform.Find("UICanvas");

        // 纯轮盘机台会删除 UICanvas 下全部子物体，原九门界面逻辑在这里整体短路。
        if (OriginalBaccaratUiPresent && UICanvas != null)
        {
            poker3 = UICanvas.Find("Center/AllCardDeck/Poker3_1").GetComponent<Animator>();//卡牌发牌动画
            pokerTurn = UICanvas.Find("Center/AllCardDeck/Poker3_1/Item/Poker").GetComponent<Animator>();//卡牌翻牌展示动画
            poker3_2 = UICanvas.Find("Center/AllCardDeck/Poker3_2").GetComponent<Animator>();
            pokerTurn_2 = UICanvas.Find("Center/AllCardDeck/Poker3_2/Item/Poker").GetComponent<Animator>();
            poker3_3 = UICanvas.Find("Center/AllCardDeck/Poker3_3").GetComponent<Animator>();
            pokerTurn_3 = UICanvas.Find("Center/AllCardDeck/Poker3_3/Item/Poker").GetComponent<Animator>();
            poker3_4 = UICanvas.Find("Center/AllCardDeck/Poker3_4").GetComponent<Animator>();
            pokerTurn_4 = UICanvas.Find("Center/AllCardDeck/Poker3_4/Item/Poker").GetComponent<Animator>();
            poker3_5 = UICanvas.Find("Center/AllCardDeck/Poker3_5").GetComponent<Animator>();
            pokerTurn_5 = UICanvas.Find("Center/AllCardDeck/Poker3_5/Item/Poker").GetComponent<Animator>();
            poker3_6 = UICanvas.Find("Center/AllCardDeck/Poker3_6").GetComponent<Animator>();
            pokerTurn_6 = UICanvas.Find("Center/AllCardDeck/Poker3_6/Item/Poker").GetComponent<Animator>();

            CardDeckRemainCards = UICanvas.Find("Center/AllCardDeck/CardDeck/RemainCardsNumber").GetComponent<Text>();
            CardDeckRemainCards.text = "";

            // P3: 隐藏百家乐卡片视觉（只关 Renderer/Graphic，保留 Animator/协程/网络时序）
            HideCardVisuals(UICanvas.Find("Center/AllCardDeck"));

            // P4: 隐藏路单/抢庄（子节点级；保留组件/对象与方法调用）
            HideObsoleteBaccaratUi(UICanvas);

            var symbolList = UICanvas.Find("Center/SymbolList");
            symbolListObj = symbolList.gameObject;
            for (int i = 0; i < symbolList.childCount; i++)//Baccarat※ 需要九个押分区
            {
                var symbol = symbolList.GetChild(i).GetComponent<SymbolView>();
                symbol.index = i;
                symbolViewsList.Add(symbol);//中奖视图
            }
            //symbolViewsListParent = symbolList.GetComponent<CanvasGroup>();

            var countParent = UICanvas.Find("Top/Right/BG/betCountInfo");
            for (int i = 0; i < countParent.childCount; i++)
            {
                winCountTexts.Add(countParent.GetChild(i).GetComponent<Text>());//本轮押注中奖次数文本
                winCountTexts[i].text = "0";
            }

            //显示betSwitch
            UpdateBetSwitch();

            //spine 动画
            target = transform.Find("UICanvas/Top/Center/heguangdonghua").gameObject;

            netJPIcon = UICanvas.Find("Bottom/netJackpotIcon").GetComponent<Image>();
            netJPIcon.material = new Material(Shader.Find("UI/Unlit/ImageGray"));
        }

    #region 小游戏的资源显示,一开始可以先全部注释,先写好主游戏再说
        var popCanvas = transform.Find("PopCanvas");

        // P1: 装饰性轮盘台面（display 1 / PopCanvas），只读展示，不参与下注/结算
        var rouletteGo = new GameObject("RouletteDeluxeGame");
        rouletteGo.transform.SetParent(transform, false);
        var rouletteGame = rouletteGo.AddComponent<RouletteDeluxe.RouletteGame>();
        RouletteFlowBridge.Attach(rouletteGame);
        if (RouletteFsmLoopSmoke.Requested)
            rouletteGo.AddComponent<RouletteFsmLoopSmoke>();


        var cardListParent = popCanvas.Find("cardList");
        for (int i = 0; i < cardListParent.childCount; i++)
        {
            var cardView = cardListParent.GetChild(i).GetComponent<CardView>();
            cardView.index = i;
            cards.Add(cardView);
        }

        settingMask = popCanvas.Find("SettingMask").gameObject;
        fireworkObj = popCanvas.Find("JackpotSpine/jackpot").GetComponent<SkeletonGraphic>();//彩金特效//firework
        fireworkObj.gameObject.SetActive(false);
        mingPaiObj = popCanvas.Find("showCardLogo_new/mingpai").GetComponent<SkeletonGraphic>();
        mingPaiObj.gameObject.SetActive(false);


    #endregion

    #region netJackpot资源显示
        var helpDialog = popCanvas.Find("helpPanel").gameObject;//帮助视图显示
        helpDialog.SetActive(false);
        nomalHelpContentObj = helpDialog.transform.Find("BG").gameObject;
        var betsLimitOfNetJackpotParent = nomalHelpContentObj.transform.Find("parent");
        for (int i = 0; i < betsLimitOfNetJackpotParent.childCount; i++)
            betsMinOfNetJackpotTexts.Add(betsLimitOfNetJackpotParent.GetChild(i).GetComponent<Text>());

        netJackpotView = popCanvas.Find("netJackpot").GetComponent<NetJackpotView>();//联网彩金
    #endregion


        //给所有点击按钮添加点击音效
        var btns = this.GetComponentsInChildren<Button>(true);
        foreach (var btn in btns)
        {
            btn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlaySoundEff("buttonClick");
            });
        }
    }


    private static void HideCardVisuals(Transform allCardDeck)
    {
        if (allCardDeck == null) return;
        for (int i = 0; i < allCardDeck.childCount; i++)
        {
            var child = allCardDeck.GetChild(i);
            if (child.name == "GameInfo") continue; // 保留局数/轮数
            var renderers = child.GetComponentsInChildren<Renderer>(true);
            for (int r = 0; r < renderers.Length; r++) renderers[r].enabled = false;
            var graphics = child.GetComponentsInChildren<Graphic>(true);
            for (int g = 0; g < graphics.Length; g++) graphics[g].enabled = false;
        }
    }

    private static void HideObsoleteBaccaratUi(Transform uiCanvas)
    {
        if (uiCanvas == null) return;

        // 路单：大路 / 珠盘路 / 路单页数据（保留 RecordViewCountDown 倒计时）
        HidePath(uiCanvas, "Top/Left/BG/record");
        HidePath(uiCanvas, "Top/Right/record2");
        HidePath(uiCanvas, "Top/Right/RecordView/BG/RecordList");
        HidePath(uiCanvas, "Top/Right/RecordView/BG/record");
        HidePath(uiCanvas, "Top/Right/RecordView/BG/Tip");
        HidePath(uiCanvas, "Top/Right/RecordView/BG/Info");
        HidePath(uiCanvas, "Top/Right/RecordView/Symbol_New");
        HidePath(uiCanvas, "Top/BigRoad");
        HidePath(uiCanvas, "Top/ZhuPanRoad");
        HidePath(uiCanvas, "Top/Bigeye");
        HidePath(uiCanvas, "Top/SmallRoad");
        HidePath(uiCanvas, "Top/Cockroach");

        // 抢庄：先释放触摸状态，再隐藏
        HidePath(uiCanvas, "Center/BankerView");
        var qiangzhuang = uiCanvas.Find("Bottom/qiangzhuangBtn");
        if (qiangzhuang != null)
        {
            var trigger = qiangzhuang.GetComponent<MyTriggerDetection>();
            if (trigger != null) trigger.ExecutePointerExit();
            qiangzhuang.gameObject.SetActive(false);
        }
    }

    private static void HidePath(Transform root, string path)
    {
        var target = root.Find(path);
        if (target != null) target.gameObject.SetActive(false);
    }

    private void UpdateCardRemainText(int count)
    {
        if (!OriginalBaccaratUiPresent) return;
        CardDeckRemainCards.text = count.ToString();//剩余卡牌数量
    }

    private void FirstRefreshText()
    {
        if (!isFirstRefreshText) return;
        isFirstRefreshText = false;
        //Debug.Log("数据初始化,只执行一次");

        //FirstRefreshBancker();

        //canWinScoreAnimation = true;//没有弄懂有什么用

        //minBetText.text = IOCanvasModel.Instance.CfgData.MinBet.ToString();//最小押注
        //limitText.text = IOCanvasModel.Instance.tempCfgData.LimitBetsWins.ToString();//限红

        UpdateCardRemainText(Model.Instance.RemainCards);//剩余卡牌数量

        //winScoreText.text = PlayerMgr.Instance.localPlayer.outState.wins.ToString();//需要PlayerMgr已经初始化了
        Controller.Instance.HideTips();

        PlayerMgr.Instance.localPlayer.betUnitIdx = 0;
        ShowBetCoinViewActive();//显示修改
    }

    private void InitViewData()
    {
        //先隐藏动画
        for (int i = 0; i < symbolViewsList.Count; i++)
            symbolViewsList[i].HideLightIcon();//先隐藏全部symbol中奖特效特效

        mingPaiObj.gameObject.SetActive(false);//隐藏明牌logo动画
        fireworkObj.gameObject.SetActive(false);//隐藏彩金特效

        // 卡牌动画属于原九门界面，纯轮盘机台下不存在。
        if (OriginalBaccaratUiPresent)
        {
            poker3.gameObject.SetActive(false);//卡牌动画隐藏
            poker3_2.gameObject.SetActive(false);
            poker3_3.gameObject.SetActive(false);
            poker3_4.gameObject.SetActive(false);
            poker3_5.gameObject.SetActive(false);
            poker3_6.gameObject.SetActive(false);
            pokerTurn.gameObject.SetActive(false);
            pokerTurn_2.gameObject.SetActive(false);
            pokerTurn_3.gameObject.SetActive(false);
            pokerTurn_4.gameObject.SetActive(false);
            pokerTurn_5.gameObject.SetActive(false);
            pokerTurn_6.gameObject.SetActive(false);
        }

        SetRecords();
    }



    public void InitQRCode()
    {
        //qrPanel.transform.Find("mask/qrCode").GetComponent<RawImage>().texture = IOTModel.Instance.GetQRTexture(Color.black);
    }
    #endregion


#region 后台按钮 点击事件回调

    /// <summary>
    /// 设置按钮 的点击回调,进入后台设置界面
    /// </summary>
    public void OnSettingBtnClick()
    {
        if (FSMSystem.Instance.CurrentStateID == StateID.Error)
            FSMSystem.Instance.PerformTransition(Transition.Setting);
        else if (Model.Instance.rouletteFlow)
        {
            // 轮盘模式不经过打单环节，任何时刻都只登记设置请求，
            // 由 RouletteGame.Update 在轮盘空闲时消费后 ForceState(Setting)。
            CloseBillCanvas();
            Model.Instance.wait4Setting = true;
            Model.Instance.RemoveErrorCode(0, false);
            PopTips.Instance.ShowSystemTips($"About to enter settings", -1);
            Debug.Log("ROULETTE_FSM_FLOW: setting requested, waiting for roulette idle");
        }
        else
        {
            if (FSMSystem.Instance.CurrentStateID == StateID.Print && Model.Instance.PrinterError())
            {
                Controller.Instance.HideTips();
                FSMSystem.Instance.PerformTransition(Transition.Setting);
            }
            //手动打印, 且正在等待打印按钮被点击(验证倒计时期间不可直接进入设置)
            else if (IOCanvasModel.Instance.tempCfgData.NewGameMode == 1
                && SBoxIdea.PrinterStateCustom() != 1
                && !Model.Instance.printerPrinting
                && !Model.Instance.checking
                && Model.Instance.startingNewRound)
                FSMSystem.Instance.PerformTransition(Transition.Setting);
            else
            {
                CloseBillCanvas();
                Model.Instance.wait4Setting = true;
                Model.Instance.RemoveErrorCode(0, false);
                PopTips.Instance.ShowSystemTips($"About to enter settings", -1);
            }
        }
    }

    /// <summary>
    /// 设置按钮2 的点击回调(测试打印)
    /// </summary>
    public void OnSettingBtn2Click()
    {
        TestPrinterMessage();
    }

    /// <summary>
    /// 测试打印:调用 SBoxSandbox.PrinterMessage 打印一行文字(并切纸),用于验证打印接口
    /// </summary>
    public void TestPrinterMessage()
    {
        SBoxSandbox.PrinterMessage("Printer Message Test " + DateTime.Now.ToString("HH:mm:ss"));
    }

    // /// <summary>
    // /// 设置按钮3 的点击回调
    // /// </summary>
    // public void OnSettingBtn3Click()
    // {
    //     var AllCardDeck = transform.Find("UICanvas/Center/AllCardDeck").gameObject;
    //     AllCardDeck?.SetActive(!AllCardDeck.activeSelf);
    // }

    // /// <summary>
    // /// 设置按钮4 的点击回调
    // /// </summary>
    // public void OnSettingBtn4Click()
    // {
    //     var Top = transform.Find("UICanvas/Top").gameObject;
    //     Top?.SetActive(!Top.activeSelf);
    // }

    /// <summary>
    /// 账单按钮 的点击回调,打开或者关闭后台账单界面
    /// </summary>
    public void OnBillBtnClick()
    {
        if (billCanvasObj != null && billCanvasObj.activeSelf)
        {
            CloseBillCanvas();
            return;
        }
        if (FSMSystem.Instance.CurrentStateID == StateID.Setting
            || Model.Instance.wait4Setting)
            return;
        InitBillCanvas();
    }

    /// <summary>
    /// 回调,打开或关闭第三方支付界面
    /// </summary>
    public void OnEnterClick()//没用,没有资源
    {
        //if (!IOTModel.Instance.LinkIOT) return;
        //qrPanel.SetActive(!qrPanel.activeSelf);
    }
#endregion


#region 游戏主运行逻辑显示

    /// <summary>单张扑克动画控制参数</summary>
    private class PokerPlayOptions
    {
        public bool needReset = true;     // 是否重置动画
        public bool needTexture = true;   // 是否贴牌面
        public bool needShowFront = true; // 是否翻面
        public bool needMove = true;      // 是否移动
        public float turnSpeed = 1f;      // 翻牌动画速度
        public float resetWait = 0f;      // 重置完成后停留秒数:背面出现动画播放时间,0=不停留
        public float turnWait = 0f;       // 翻面后停留秒数:翻面动画播放时间,0=不停留
        public float interval = 0f;       // 发牌启动间隔:相邻两张牌协程启动的间隔秒数,0=同时启动
    }

    /// <summary>
    /// 单张扑克完整播放逻辑:重置→贴牌面→翻面→关闭移动,一个完整流程
    /// 不同物体的播放逻辑分开:poker=发牌动画物体逻辑, turn=翻牌动画物体逻辑
    /// 差异对象与动画控制均由参数传入
    /// </summary>
    private IEnumerator PlayPokerCard(Animator poker, Animator turn, PokerData data, PokerPlayOptions options)
    {
        if (!OriginalBaccaratUiPresent) yield break;
        //---- 发牌物体(poker)+翻牌物体(turn) 重置逻辑 ----
        if (options.needReset)
        {
            if (!poker.isInitialized)
                poker.Rebind();
            if (!turn.isInitialized)
                turn.Rebind();
            poker.gameObject.SetActive(true);
            poker.SetBool("isShowFront", false);
            turn.gameObject.SetActive(true);
            turn.SetFloat("speed", options.turnSpeed);
            turn.SetBool("show", false);
        }
        //重置后短暂停留:让背面出现动画播放
        if (options.resetWait > 0f)
            yield return new WaitForSeconds(options.resetWait);
        //---- 发牌物体(poker) 贴牌面逻辑 ----
        if (options.needTexture)
        {
            var pokerSprite = poker.transform.Find("Item/Poker/pukepai").GetComponent<SkinnedMeshRenderer>().materials[1];
            if (pokerSprite != null)
            {
                pokerSprite.SetTexture("_MainTex", ResMgr.Instance.Load<Texture>($"poker/{data.type}_{data.value}"));
            }
        }
        //---- 发牌物体(poker) 翻面逻辑 ----
        if (options.needShowFront)
            poker.SetBool("isShowFront", true);
        //翻面后短暂停留:让翻面动画播放
        if (options.turnWait > 0f)
            yield return new WaitForSeconds(options.turnWait);
        //---- 发牌物体(poker) 关闭移动逻辑 ----
        if (options.needMove)
        {
            poker.SetTrigger("isMove");
            poker.SetBool("isShowFront", false);
        }
    }

    private IEnumerator DealPoker(RecordStruct record)//开始发扑克牌携程:完整流程唯一调用点
    {
        if (!OriginalBaccaratUiPresent) yield break;
        //明牌局(小游戏6):不走常规发牌,发牌+翻牌统一由 ShowPokerCard 按张数同时处理
        if (record.luckyId == 6)
            yield break;

        Debug.LogError("DealPoker!");
        AudioManager.Instance.PlaySoundEff("showCard");//发牌音效
        isDealing = true;//发牌期间锁定荷官动画,禁止被其他动作打断

        //发牌阶段只发前四张(庄家2张+闲家2张),全部背面发出不掀开,牌面留到开牌阶段展示
        //补牌(第5/6张)由开牌阶段 PokerAniTest 按后端 cardnum 决定是否发出
        //value 数组固定6位: [0..2]庄家三槽位 [3..5]闲家三槽位
        Animator[] pokers = { poker3, poker3_2, poker3_3, poker3_4, poker3_5, poker3_6 };//发牌动画组:前3庄家位后3闲家位
        Animator[] turns = { pokerTurn, pokerTurn_2, pokerTurn_3, pokerTurn_4, pokerTurn_5, pokerTurn_6 };//翻牌动画组:前3庄家位后3闲家位
        PokerPlayOptions options = new PokerPlayOptions
        {
            needShowFront = false,//发牌阶段不翻面:玩家看不到牌面,掀开时再展示
            resetWait = 0.5f,//重置后停留0.5秒:背面出现动画播放(原WaitForSeconds)
            turnWait = 0.5f,//贴面后停留0.5秒(原WaitForSeconds)
            interval = 0.5f,//发牌启动间隔0.5秒
        };

        //前四张交替发牌: 闲第1张(槽位4) → 庄第1张(槽位0) → 闲第2张(槽位3) → 庄第2张(槽位1)
        int[] dealOrder = { 4, 0, 3, 1 };//发牌槽位顺序: 闲-庄-闲-庄
        for (int i = 0; i < dealOrder.Length && dealOrder[i] < pokers.Length; i++)
        {
            int slot = dealOrder[i];//槽位索引与 value 索引一致: [0..2]庄 [3..5]闲
            PokerData data = slot < record.value.Length ? Model.Instance.GetPokerData(record.value[slot]) : new PokerData();
            StartCoroutine(PlayPokerCard(pokers[slot], turns[slot], data, options));//飞牌并行:不同牌位互不等待
            yield return StartCoroutine(PlayDealerDealAnim(slot));//荷官动画串行:按牌位槽位播放,本张完整播完才发下一张
        }

        isDealing = false;//发牌结束,解除锁定
        PlaySpineAnim(0);//荷官回到待机
        isPlayOver = true;//发牌结束,允许发筹码等其他荷官动画
    }

    /// <summary>
    /// 百家乐单张牌点数：A=1，2~9=面值，10/J/Q/K=0
    /// </summary>
    private static int GetBaccaratCardPoint(int pokerValue)
    {
        PokerData pokerData = Model.Instance.GetPokerData(pokerValue);
        int rank = pokerData.value;          // 低4位: 1=A, 2~10, 11=J, 12=Q, 13=K
        return rank >= 10 ? 0 : rank;
    }

    /// <summary>
    /// 计算百家乐某方当前总点数的个位（模10）
    /// </summary>
    /// <param name="record">本局记录</param>
    /// <param name="isBanker">true=庄家(value[0..2]), false=闲家(value[3..5])</param>
    /// <param name="cardCount">取前几张（开两张=2；补牌后=3）</param>
    private static int GetBaccaratPointsUnits(RecordStruct record, bool isBanker, int cardCount)
    {
        int baseIdx = isBanker ? 0 : 3;
        int total = 0;
        for (int i = 0; i < cardCount && baseIdx + i < record.value.Length; i++)
            total += GetBaccaratCardPoint(record.value[baseIdx + i]);
        return total % 10;
    }

    /// <summary>
    /// 结算语音关键字：result 1=庄赢 2=闲赢 3=和
    /// </summary>
    private static string GetResultVoiceKey(int result)
    {
        switch (result)
        {
            case 1: return "banker_wins";
            case 2: return "player_wins";
            default: return "tie";
        }
    }

    /// <summary>
    /// 开四张牌后播报对子：庄对/闲对（仅有对子时播报）
    /// </summary>
    private IEnumerator PlayPairVoice(RecordStruct recordStruct)
    {
        if (recordStruct.pair == null) yield break;
        if (recordStruct.pair.Length > 0 && recordStruct.pair[0] == 1)
        {
            AudioManager.Instance.PlayVoiceEff("banker_pair");//庄对
            yield return new WaitForSeconds(1.2f);
        }
        if (recordStruct.pair.Length > 1 && recordStruct.pair[1] == 1)
        {
            AudioManager.Instance.PlayVoiceEff("player_pair");//闲对
            yield return new WaitForSeconds(1.2f);
        }
        yield break;
    }

    /// <summary>
    /// 确定胜负后的播报：庄家点数 → 闲家点数 → 胜负（顺序播报）
    /// </summary>
    private IEnumerator PlaySettlementVoice(RecordStruct recordStruct)
    {
        int bankerCount = recordStruct.cardnum != null && recordStruct.cardnum.Length > 0 ? recordStruct.cardnum[0] : 2;
        int playerCount = recordStruct.cardnum != null && recordStruct.cardnum.Length > 1 ? recordStruct.cardnum[1] : 2;

        //庄家几点
        AudioManager.Instance.PlayVoiceEff("banker");
        yield return new WaitForSeconds(1f);
        AudioManager.Instance.PlayVoiceEff("point_" + GetBaccaratPointsUnits(recordStruct, true, bankerCount));
        yield return new WaitForSeconds(1.2f);
        //闲家几点
        AudioManager.Instance.PlayVoiceEff("player");
        yield return new WaitForSeconds(1f);
        AudioManager.Instance.PlayVoiceEff("point_" + GetBaccaratPointsUnits(recordStruct, false, playerCount));
        yield return new WaitForSeconds(1.2f);
        //胜负：庄赢/闲赢/和
        AudioManager.Instance.PlayVoiceEff(GetResultVoiceKey(recordStruct.result));
    }

    /// <summary>
    /// 其余四门中奖播报：小老虎/大老虎/幸运7/幸运8（单局至多中一门，命中即播，与结算画面并行）
    /// </summary>
    private void PlaySideBetVoice(RecordStruct recordStruct)
    {
        if (recordStruct.spresult == null) return;
        if (recordStruct.spresult.Length > 0 && recordStruct.spresult[0] == 1)
            AudioManager.Instance.PlayVoiceEff("small_tiger");//小老虎
        else if (recordStruct.spresult.Length > 1 && recordStruct.spresult[1] == 1)
            AudioManager.Instance.PlayVoiceEff("big_tiger");//大老虎
        else if (recordStruct.spresult.Length > 2 && recordStruct.spresult[2] == 1)
            AudioManager.Instance.PlayVoiceEff("lucky_7");//幸运7
        else if (recordStruct.spresult.Length > 3 && recordStruct.spresult[3] == 1)
            AudioManager.Instance.PlayVoiceEff("lucky_8");//幸运8
    }

    private IEnumerator PokerAniTest(RecordStruct recordStruct)
    {
        if (!OriginalBaccaratUiPresent) yield break;
        //开牌顺序: 先掀开闲家两张 → 再掀开庄家两张 → 闲家补牌(需要时)掀开第三张 → 庄家补牌(需要时)掀开第三张
        //补牌与否由后端 cardnum 决定: [0]庄家张数 [1]闲家张数, 3=补第三张
        int bankerCount = recordStruct.cardnum != null && recordStruct.cardnum.Length > 0 ? recordStruct.cardnum[0] : 2;
        int playerCount = recordStruct.cardnum != null && recordStruct.cardnum.Length > 1 ? recordStruct.cardnum[1] : 2;

        //掀开闲家两张
        yield return StartCoroutine(PlaySlotRevealAnimation(pokerTurn_5));
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(PlaySlotRevealAnimation(pokerTurn_4));
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(PlaySlotRevealAnimation(pokerTurn));
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(PlaySlotRevealAnimation(pokerTurn_2));
        yield return new WaitForSeconds(0.5f);
        //开四张牌之后播报对子: 庄对/闲对（有对子才播报）
        yield return PlayPairVoice(recordStruct);
        //③ 闲家补牌: cardnum[1]==3 时发闲家第三张(槽位5)并掀开
        if (playerCount == 3)
        {
            AudioManager.Instance.PlayVoiceEff("player_draws");//闲家补牌
            yield return DealAndReveal(poker3_6, pokerTurn_6, recordStruct, 5);
            yield return new WaitForSeconds(1f);
        }
        //④ 庄家补牌: cardnum[0]==3 时发庄家第三张(槽位2)并掀开
        if (bankerCount == 3)
        {
            AudioManager.Instance.PlayVoiceEff("banker_draws");//庄家补牌
            yield return DealAndReveal(poker3_3, pokerTurn_3, recordStruct, 2);
            yield return new WaitForSeconds(1f);
        }

        //加速翻牌※打包恢复dabao
        yield return new WaitForSeconds(2f);//加速翻牌※打包恢复dabao
        yield return PlayResultWin1(recordStruct);//播放中奖
    }

    /// <summary>
    /// 补牌流程: 背面发出第三张牌 → 等待飞到位 → 掀开
    /// </summary>
    private IEnumerator DealAndReveal(Animator poker, Animator turn, RecordStruct record, int valueIdx)
    {
        if (!OriginalBaccaratUiPresent) yield break;
        PokerData data = valueIdx < record.value.Length ? Model.Instance.GetPokerData(record.value[valueIdx]) : new PokerData();
        PokerPlayOptions options = new PokerPlayOptions
        {
            needShowFront = false,//补牌同样背面发出,掀开时才展示牌面
            resetWait = 0.5f,
            turnWait = 0.5f,
            interval = 0f,
        };
        isDealing = true;//发牌期间锁定荷官动画,禁止被其他动作打断
        Coroutine card = StartCoroutine(PlayPokerCard(poker, turn, data, options));//飞牌并行
        yield return StartCoroutine(PlayDealerDealAnim(valueIdx));//荷官动画串行:按牌位槽位播放,本张完整播完
        yield return card;//等飞牌协程结束
        isDealing = false;//发牌结束,解除锁定
        PlaySpineAnim(0);//荷官回到待机
        yield return new WaitForSeconds(1f);//等待牌飞到位
        yield return StartCoroutine(PlaySlotRevealAnimation(turn,true));//掀开第三张
    }

    private IEnumerator PlaySlotRevealAnimation(Animator turnAnim, bool isRd = false)
    {
        if (!OriginalBaccaratUiPresent) yield break;
        // turnAnim.SetFloat("speed", 0.1f);
        // turnAnim.SetBool("show", true);
        // yield return new WaitForSeconds(0.35f);
        // turnAnim.SetFloat("speed", 0.04f);
        // yield return new WaitForSeconds(3f);
        // turnAnim.SetFloat("speed", 0.01f);
        // yield return new WaitForSeconds(1.75f);
        // //第一段落下
        // turnAnim.SetFloat("speed", -0.3f);
        // yield return new WaitForSeconds(0.75f);
        // //第二段掀起
        // turnAnim.SetFloat("speed", 0.2f);
        // yield return new WaitForSeconds(2.15f);//1.3f
        // turnAnim.SetFloat("speed", 0.04f);
        // yield return new WaitForSeconds(2.25f);

        // //yield return new WaitForSeconds(0.75f);
        // turnAnim.SetFloat("speed", 0.01f);
        // yield return new WaitForSeconds(2f);
        // turnAnim.SetFloat("speed", 1f);
        // yield return new WaitForSeconds(0.5f);
        turnAnim.SetBool("show", true);
        if (isRd)
        {
            turnAnim.SetFloat("speed", 0.12f);
            yield return new WaitForSeconds(2f);
            turnAnim.SetFloat("speed", -0.4f);
            yield return new WaitForSeconds(0.6f);
        }
        //加速翻牌※打包恢复dabao
        turnAnim.SetFloat("speed", 1.5f);
        yield return new WaitForSeconds(0.7f);
    }

    IEnumerator PlayResultWin1(RecordStruct recordStruct)
    {
        NetMgr.Instance.SendToAllClient(JsonConvert.SerializeObject(new MsgInfo
        {
            cmd = (int)S2C_CMD.S2C_RewardEnd,
            id = -1,
            jsonData = JsonConvert.SerializeObject(Model.Instance.curRecord)
        }));
        SBoxIdea.BattleLeadStop();//押注结束        ,出赢分
        
        Debug.Log($"本局游戏赢分1:{PlayerMgr.Instance.localPlayer.outState.wins}");

        //"正在结算"
        uiMainView.ChangeCountDownState(CountDownState.Settling);


        if (recordStruct.result == 3) Model.Instance.isJoker = true;   // 和局标志（原皇冠功能转移，开"和"时为 true）

        bool[] wins = Model.CalcWinDoors(recordStruct);
        Debug.LogError($"result{recordStruct.result}");
        if (recordStruct.luckyId == 7 && Model.Instance.curRecord.luckyPlayerId == SBoxModel.Instance.macId)//不是自己中7奖不闪 - kuarser
        {
            //大满贯
            for (int i = 0; i < symbolViewsList.Count; i++)
            {
                symbolViewsList[i].ShowLightIcon();//显示结果中奖闪烁
                //symbolViewsList2[i].ShowLightIcon();//显示结果中奖闪烁
            }
            uiRoadMapView.SpecialAllWin();
        }
        else
        {
            for (int i = 0; i < wins.Length && i < symbolViewsList.Count; i++)
            {
                if (wins[i])
                    symbolViewsList[i].ShowLightIcon();//显示结果中奖闪烁(可多门同亮)
            }
            uiRoadMapView.ShowResultWin(recordStruct.result - 1);


            //AudioManager.Instance.PlayVoiceEff($"result{recordStruct.result}");//播放中奖语音
        }
        SetPokerValue(recordStruct.value[0]);//设置扑克牌显示值(庄家第一张)

        if (PlayerMgr.Instance.localPlayer.outState != null)
        {
            bool isWin = PlayerMgr.Instance.localPlayer.outState.bets[recordStruct.result - 1] > 0
                || recordStruct.result == 3;   // 和局强制灯光闪烁（原皇冠功能转移）
            SandboxController.Instance.LightFlashing(recordStruct.result, isWin);
        }
        Model.Instance.records.Add(recordStruct);
        UpdateRecord(recordStruct, Model.Instance.curGame);
        //确定胜负后：庄家点数→闲家点数→胜负（顺序播报）
        yield return PlaySettlementVoice(recordStruct);
        yield return new WaitForSeconds(1.5f);
        //其余四门中奖：非阻塞播报，可与结算画面并行
        PlaySideBetVoice(recordStruct);
        yield break;
    }


    public void CountDownStart()
    {
        // 纯轮盘机台：原九门倒计时/发牌都不播，轮盘自己管押分窗口。
        if (!OriginalBaccaratUiPresent) return;

        Model.Instance.curCountDown = IOCanvasModel.Instance.CfgData.CountDown;//实际

        if (Application.isEditor)//编辑器测试用
            Model.Instance.curCountDown = 10;

        uiMainView.ChangeCountDownState(CountDownState.Betting);//正在倒计时,显示倒计时文本等
        uiRoadMapView.ShowPredict();//显示预测的本局结果

        UpdateCardRemainText(--Model.Instance.RemainCards);//剩余卡牌数文本更新
        StartCoroutine(CountDownEnumerator());
        StartCoroutine(DealPoker(Model.Instance.curRecord));

    }
    private IEnumerator CountDownEnumerator()//开始倒计时携程
    {
        if (!OriginalBaccaratUiPresent) yield break;
        while (Model.Instance.curCountDown > 0)
        {
            yield return new WaitForSeconds(1f);//1f加速到0.2f※打包恢复dabao
            Model.Instance.curCountDown--;
            if (Model.Instance.curCountDown == 5)
                AudioManager.Instance.PlaySoundEff("countDown");//倒计时还剩5秒快结束了,倒计时警报音效

            //正在倒计时,每次更新倒计时文本
            uiMainView.UpdateCountDownText();
            EventCenter.Instance.EventTrigger(EventHandle.COUNTDOWN_FINISH);
        }
        yield return new WaitForSeconds(1f);//为0秒等待

        EventCenter.Instance.EventTrigger(EventHandle.BET_STOP_FINISH);
    }

    public void HideSymbolView()
    {
        // 纯轮盘机台没有九门押注闪光，同时 roadMap 也不在位。
        if (!OriginalBaccaratUiPresent) return;
        for (int i = 0; i < symbolViewsList.Count; i++)
        {
            symbolViewsList[i].HideLightIcon();
            uiRoadMapView.HideResultWin(i);
        }
    }

    /// <summary>
    /// 重置卡牌动画
    /// </summary>
    public void ResetPokerCard()
    {
        // 纯轮盘机台没有卡牌位，动画重置无事可做。
        if (!OriginalBaccaratUiPresent) return;
        //重置动画
        Debug.LogError("ResetPokerCard");
        pokerTurn.SetBool("show", false);
        pokerTurn.gameObject.SetActive(false);
        poker3.gameObject.SetActive(false);

        pokerTurn_2.SetBool("show", false);
        pokerTurn_2.gameObject.SetActive(false);
        poker3_2.gameObject.SetActive(false);

        pokerTurn_3.SetBool("show", false);
        pokerTurn_3.gameObject.SetActive(false);
        poker3_3.gameObject.SetActive(false);

        pokerTurn_4.SetBool("show", false);
        pokerTurn_4.gameObject.SetActive(false);
        poker3_4.gameObject.SetActive(false);

        pokerTurn_5.SetBool("show", false);
        pokerTurn_5.gameObject.SetActive(false);
        poker3_5.gameObject.SetActive(false);

        pokerTurn_6.SetBool("show", false);
        pokerTurn_6.gameObject.SetActive(false);
        poker3_6.gameObject.SetActive(false);
    }

    /// <summary>
    /// 明牌局开牌:按本局实际张数(cardnum)同时飞出全部牌并立即翻开
    /// 荷官发牌动画只播一次,之后所有牌同一时间飞出、同一时间掀开
    /// </summary>
    public IEnumerator ShowPokerCard(RecordStruct recordStruct)
    {
        // 纯轮盘机台没有卡牌位，没有发牌动画可播。
        if (!OriginalBaccaratUiPresent) yield break;

        int bankerCount = recordStruct.cardnum != null && recordStruct.cardnum.Length > 0 ? recordStruct.cardnum[0] : 2;
        int playerCount = recordStruct.cardnum != null && recordStruct.cardnum.Length > 1 ? recordStruct.cardnum[1] : 2;

        Animator[] pokers = { poker3, poker3_2, poker3_3, poker3_4, poker3_5, poker3_6 };//发牌动画组:前3庄家位后3闲家位
        Animator[] turns = { pokerTurn, pokerTurn_2, pokerTurn_3, pokerTurn_4, pokerTurn_5, pokerTurn_6 };//翻牌动画组:前3庄家位后3闲家位

        //本局实际张数:庄家前 bankerCount 张(槽位0..) + 闲家前 playerCount 张(槽位3..)
        List<int> slots = new List<int>();
        for (int i = 0; i < bankerCount && i < 3; i++)
            slots.Add(i);
        for (int i = 0; i < playerCount && i < 3; i++)
            slots.Add(3 + i);

        //荷官发牌动画只播一次,与飞牌同时开始播放,无需等待荷官播完
        isDealing = true;//发牌期间锁定荷官动画,禁止被其他动作打断
        StartCoroutine(PlayDealerDealAnimThenIdle(0));

        //同时飞出全部牌(背面飞出)
        PokerPlayOptions options = new PokerPlayOptions
        {
            needShowFront = false,//先背面飞出,飞出后立即翻开
            needMove = true,
            resetWait = 0f,
            turnWait = 0f,
            interval = 0f,
        };
        for (int i = 0; i < slots.Count; i++)
        {
            int slot = slots[i];
            PokerData data = slot < recordStruct.value.Length ? Model.Instance.GetPokerData(recordStruct.value[slot]) : new PokerData();
            StartCoroutine(PlayPokerCard(pokers[slot], turns[slot], data, options));//飞牌并行:全部牌同时飞
        }
        //马上翻开:全部牌同时掀开
        for (int i = 0; i < slots.Count; i++)
        {
            int slot = slots[i];
            turns[slot].SetFloat("speed", 1f);
            turns[slot].SetBool("show", true);
        }
        yield return null;//交出一帧,让飞牌/翻牌与荷官动画开始播放
    }

    //[已退役] 旧"逐张报牌"语音(花色+value点数), 与百家乐播报冲突, 原调用点已改为结算语音。
    //花色语音资源: spade/heart/club/diamond; 点数语音: value_{1-13}
    //若后续需要逐张报牌可参考以下原实现:
    //public IEnumerator PlayPokerSFX(int pokerValue)
    //{
    //    PokerData pokerData = Model.Instance.GetPokerData(pokerValue);
    //    string typeStr = "";
    //    switch (pokerData.type)
    //    {
    //        case 1: typeStr = "spade"; break;
    //        case 2: typeStr = "heart"; break;
    //        case 3: typeStr = "club"; break;
    //        case 4: typeStr = "diamond"; break;
    //    }
    //    if (IOCanvasModel.Instance.curlanguage == Language.en)
    //    {
    //        AudioManager.Instance.PlayVoiceEff($"value_{pokerData.value}");
    //        yield return new WaitForSeconds(1f);
    //        if (pokerData.type < 5) AudioManager.Instance.PlayVoiceEff(typeStr);
    //    }
    //    else
    //    {
    //        if (pokerData.type < 5)
    //        {
    //            AudioManager.Instance.PlayVoiceEff(typeStr);
    //            yield return new WaitForSeconds(0.7f);
    //        }
    //        AudioManager.Instance.PlayVoiceEff($"value_{pokerData.value}");
    //    }
    //    yield return null;
    //}

    public void SetPokerValue(int pokerValue)//没用了,
    {
        //没啥用,功能被夺出去了,希望以后有空能梳理好集成回来
        string str = Convert.ToString(pokerValue, 16).ToUpper();
        var pokerData = Model.Instance.GetPokerData(pokerValue);
    }


#endregion


#region 实时刷新数据显示,主游戏内容

    public void ShowBetCoinViewActive()//选择硬币界面显示,
    {
        // 押注筹码属于原九门界面。
        if (!OriginalBaccaratUiPresent) return;
        if (betCoinViews == null || betCoinViews.Count <= 0)
        {
            Debug.LogError("初始化高亮放大显示 第一个硬币失败!");
            return;
        }

        for (int i = 0; i < betCoinViews.Count; i++)
        {
            betCoinViews[i].SetSelectActive();//只显示选中的bet 的手指图标 按钮
        }
    }
    public void ChangeSymbolListObjActive()
    {
        if (!OriginalBaccaratUiPresent) return;
        symbolListObj.SetActive(!symbolListObj.activeSelf);
    }
    public void UpdateRoundAndGameText()//因为原来的用了字体,所以"R:"没有对应的,所以不显示,所以可以写
    {
        // 纯轮盘机台没有轮局文本。
        if (!OriginalBaccaratUiPresent) return;
        uiMainView.UpdateRoundAndGameText();
        uiRoadMapView.UpdateRoundAndGameText();
    }

    /// <summary>
    /// 设置本轮record结果面板
    /// </summary>
    public void SetRecord()
    {
        if (!OriginalBaccaratUiPresent)
        {
            // 纯轮盘机台：只维护统计计数，不碰任何原九门界面。
            for (int i = 0; i < Model.Instance.records.Count; i++)
            {
                bool[] doorWins = Model.CalcWinDoors(Model.Instance.records[i]);
                for (int j = 0; j < doorWins.Length; j++)
                    if (doorWins[j]) Model.Instance.recordTypeCounts[j]++;
            }
            return;
        }

        uiRecordView.ShowSetRecordBySelfAddItem();
        for (int i = 0; i < Model.Instance.records.Count; i++)
        {
            var record = Model.Instance.records[i];
            uiRecordView.ShowRecordView(record, true);
            //uiRoadMapView.ShowRecordView(record);

            bool[] wins = Model.CalcWinDoors(record);
            for (int j = 0; j < wins.Length; j++)
            {
                if (wins[j]) Model.Instance.recordTypeCounts[j]++;
            }
        }
        for (int i = 0; i < winCountTexts.Count; i++)//Baccarat※ 增加新的统计目标九个
        {
            //按理说,他们是存储同一种数据,所以应该是同样的list个数,可以在一个循环里
            winCountTexts[i].text = Model.Instance.recordTypeCounts[i].ToString();//每种类型的中奖次数
            //uiRoadMapView.UpdateRecordTypeCountText(i, Model.Instance.recordTypeCounts[i]);
        }
    }

    //第一次设置record 露珠界面
    public void SetRecords()
    {
        if (!OriginalBaccaratUiPresent) return;
        if (Model.Instance.records == null || Model.Instance.records.Count == 0) return;
        uiRecordView.SetRecordView();
    }

    //刷新record 露珠界面
    public void UpdateRecords()
    {
        if (!OriginalBaccaratUiPresent) return;
        uiRecordView.UpdateRecordView();

        Model.Instance.SetAllRoadData();

        RefreshAllRecords();
        uiRoadMapView.UpdateRecords();
    }

    /// <summary>
    /// 重置record结果显示视图
    /// </summary>
    public void RefreshOnNewRound()
    {
        for (int i = 0; i < Model.Instance.recordTypeCounts.Length; i++)
            Model.Instance.recordTypeCounts[i] = 0;

        // 界面刷新只在原九门界面在位时执行：纯轮盘机台下这些组件字段为 null。
        if (OriginalBaccaratUiPresent)
        {
            uiRecordView.ResetRecordView();
            winCountTexts.ForEach(text => text.text = "0");
            uiRoadMapView.RefreshOnNewRound();
        }

        Model.Instance.records.Clear();

        Model.Instance.lastValueYuce = new int[] {-1,-1,-1};

        NetMgr.Instance.SendToAllClient(JsonConvert.SerializeObject(new MsgInfo
        {
            cmd = (int)S2C_CMD.S2C_ResetRecord,
            id = -1,
            jsonData = "",
        }));
    }

    public void SetLuckyTips(string str, float timer = 3f)
    {
        if (!OriginalBaccaratUiPresent) return;
        uiMainView.SetLuckyTips(str, timer);

    }

    /// <summary>
    /// 设置彩金数值
    /// </summary>
    public void SetJackpot(int jackpot)
    {
        //uiMainView.UpdateJackpotText(jackpot);//轮盘暂时屏蔽
    }

    /// <summary>
    /// 刷新所有玩家的总押分
    /// </summary>
    public void UpdateAllBets()
    {
        if (!OriginalBaccaratUiPresent) return;
        for (int i = 0; i < symbolViewsList.Count; i++)//symbolViewsList.Count
        {
            symbolViewsList[i].SetAllBet(Model.Instance.totalBets[i]);
            uiRoadMapView.UpdateAllBetText(i, Model.Instance.totalBets[i]);
        }
    }

    /// <summary>
    /// 一直刷新玩家信息面板
    /// </summary>
    public void RefreshPlayerInfo()
    {
        if (!OriginalBaccaratUiPresent) return;
        FirstRefreshText();
        uiMainView.ShowWinScore();//手动刷新分数


        //uiMainView.UpdateView();//轮盘暂时屏蔽

        // for (int i = 0; i < symbolViewsList.Count; i++)//轮盘暂时屏蔽
        // {
        //     symbolViewsList[i].SetBet(PlayerMgr.Instance.localPlayer.outState.bets[i]);
        //     symbolViewsList[i].SetSymbolView(Model.Instance.totalBets[i]);//显示所有玩家押注

        //     uiRoadMapView.UpdateBetText(i, PlayerMgr.Instance.localPlayer.outState.bets[i]);
        //     uiRoadMapView.UpdateResultWinTotalBet(i, Model.Instance.totalBets[i]);

        //     if (FSMSystem.Instance.CurrentStateID == StateID.Bet
        //         && PlayerMgr.Instance.localPlayer.outState.credit > 0
        //         && symbolViewsList[i].isBettingNow)
        //         Controller.Instance.PlaySpineAnimation();//播放荷官发筹码押注动画
        // }

        // uiBankerView.UpdateBankerText();//轮盘暂时屏蔽
    }




    /// <summary>
    /// 更新结果记录并同步到所有客户端,更新结果记录面板视图
    /// </summary>
    public void UpdateRecord(RecordStruct record, int gameIndex)
    {
        uiRecordView.ShowRecordView(record);

        ////牌路界面数据
        //uiRoadMapView.ShowRecordView(record);//有问题,可能有问题

        UpdateRecords();

        MsgInfo msg = new MsgInfo()
        {
            cmd = (int)S2C_CMD.S2C_UpdateRecord,
            id = -1,
            jsonData = JsonConvert.SerializeObject(record),
        };
        NetMgr.Instance.SendToAllClient(JsonConvert.SerializeObject(msg));

        bool[] wins = Model.CalcWinDoors(record);
        for (int i = 0; i < wins.Length; i++)
        {
            if (!wins[i]) continue;
            Model.Instance.recordTypeCounts[i]++;
            winCountTexts[i].text = Model.Instance.recordTypeCounts[i].ToString();
            //uiRoadMapView.UpdateRecordTypeCountText(i, Model.Instance.recordTypeCounts[i]);
        }
    }
    #endregion


    #region 视图显示,小游戏
    /// <summary>
    /// 播放小游戏幸运座位声音
    /// </summary>
    /// <param name="playerId">几号玩家中</param>
    public void PlayLuckySeatSound(int playerId)
    {
        StartCoroutine(PlayLuckySFX(playerId));
    }

    private IEnumerator PlayLuckySFX(int playerId)
    {
        if (IOCanvasModel.Instance.curlanguage == Language.en)
        {
            AudioManager.Instance.PlayVoiceEff("congratulation");
            yield return new WaitForSeconds(1.9f);
            AudioManager.Instance.PlayVoiceEff(playerId.ToString());
            yield return new WaitForSeconds(1.1f);
            AudioManager.Instance.PlayVoiceEff("luckySeat");
        }
        else
        {
            AudioManager.Instance.PlayVoiceEff("congratulation");
            yield return new WaitForSeconds(0.7f);
            AudioManager.Instance.PlayVoiceEff(playerId.ToString());
            yield return new WaitForSeconds(0.3f);
            AudioManager.Instance.PlayVoiceEff("luckySeat");
        }
        yield return null;
    }

    /// <summary>
    /// 显示彩金,包括彩金logo 和 特效 和 金币音效
    /// </summary>
    public IEnumerator ShowCoinEff()
    {
        AudioManager.Instance.PlaySoundEff("coinEff", true);
        //logoList[0].SetActive(true);
        Model.Instance.myTriggerDetectionCanEnterCount++;//播放彩金动画时,触摸区域不能响应触摸事件
        fireworkObj.gameObject.SetActive(true);
        string animationName = IOCanvasModel.Instance.curlanguage == Language.en?"yw":"zw";
        fireworkObj.AnimationState.SetAnimation(0,"Star_"+animationName,false);
        yield return new WaitForSeconds(2.1f);
        fireworkObj.AnimationState.SetAnimation(0,"Idel_"+animationName,true);
        yield return new WaitForSeconds(2f);
        fireworkObj.AnimationState.SetAnimation(0,"Over_"+animationName,false);
        yield return new WaitForSeconds(0.3f);
        fireworkObj.gameObject.SetActive(false);
    }
    public void ShowLuckySeatCard()//显示幸运座位卡牌选择动画
    {
        AsyncActionUtils.ApplyLocalMovement(this, cards[0].transform, new Vector3(-1600, 0, 0), new Vector3(-600, 0, 0), 1, TweenUtils.VectorTweenLinear);
        AsyncActionUtils.ApplyLocalMovement(this, cards[1].transform, new Vector3(0, 640, 0), new Vector3(0, 0, 0), 1, TweenUtils.VectorTweenLinear);
        AsyncActionUtils.ApplyLocalMovement(this, cards[2].transform, new Vector3(1600, 0, 0), new Vector3(600, 0, 0), 1, TweenUtils.VectorTweenLinear,
            onComplete: () => Model.Instance.waitSelectCard = true);
        Timer.DelayAction(1, () =>
        {
            //过一秒后显示 能点击选择的提示动画
            for (int i = 0; i < cards.Count; i++)
                cards[i].ShowCanClickAnimation();
        });

    }
    public void ShowLuckySeatCardResult(int selectIndex)
    {
        if (Model.Instance.curRecord != null)
            cards[selectIndex].ShowAni();
    }
    public void ResetCardList()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].ResetCard();
            cards[i].touchTriger.PointerUpPublic();//kuarser：增加触发PointerUp
        }
    }
#endregion


#region 后台界面相关
    /// <summary>
    /// 初始化后台界面
    /// </summary>
    public void InitIOCanvas()
    {
        CloseBillCanvas();
        // 轮盘模式不参与算法卡本轮协议，进去设置时不要请求结束本轮。
        if (!Model.Instance.rouletteFlow
            && !Model.Instance.wait4Print
            && !Model.Instance.checking)
            SBoxIdea.BattleEndRound();
        Model.Instance.wait4Setting = false;
        if (IOCanvasObj == null)
        {
            if (macInfoView == null)
            {
                macInfoView = FindObjectOfType<MacInfoView>();
            }
            ResMgr.Instance.LoadAssetBundle("io", "IOCanvas", (obj) =>
            {
                IOCanvasObj = Instantiate(obj) as GameObject;
                IOCanvasObj.transform.SetParent(macInfoView.transform);
                IOCanvasObj.transform.localPosition = Vector3.zero;
                IOCanvasObj.transform.localScale = macInfoView.transform.GetComponent<RectTransform>().rect.width / IOCanvasObj.transform.GetComponent<RectTransform>().rect.width * Vector3.one;
            });
        }
        else
            IOCanvasObj.SetActive(true);
    }
    public void RefreshIOCanvas()
    {
        if (!OriginalBaccaratUiPresent) return;
        uiMainView.UpdateView();
    }

    public void CloseBillCanvas()
    {
        billCanvasObj?.SetActive(false);
    }

    public void InitBillCanvas()
    {
        if (billCanvasObj == null)
        {
            if (macInfoView == null)
            {
                macInfoView = FindObjectOfType<MacInfoView>();
            }
            ResMgr.Instance.LoadAssetBundle("io", "BillCanvas", (obj) =>
            {
                billCanvasObj = Instantiate(obj) as GameObject;
                billCanvasObj.transform.SetParent(macInfoView.transform);
                billCanvasObj.transform.localPosition = Vector3.zero;
                billCanvasObj.transform.localScale = macInfoView.transform.GetComponent<RectTransform>().rect.width / billCanvasObj.transform.GetComponent<RectTransform>().rect.width * Vector3.one;
            });
        }
        else
            billCanvasObj.SetActive(true);
    }

    #endregion


    #region 其他按钮

    public void ChangeHelpPanelActive()
    {
        if (!OriginalBaccaratUiPresent) return;
        uiHelpView.ChangeHelpPanelActive();

    }
    
    public void ResetHistoryRecord()
    {
        if (!OriginalBaccaratUiPresent) return;
        uiHistoryView.ResetHistoryRecord();
    }

    #endregion


    #region spine 动画控制

    public void PlaySpineAnim2(int index)
    {
        if (!OriginalBaccaratUiPresent) return;
        if (isDealing) return;//发牌期间荷官动画不可被其他动作(发筹码等)打断
        if (isPlayOver)
        {
            PlaySpineAnim(index);
        }
    }
    private void PlaySpineAnim(int index = -1)
    {
        if (index != 0)
            isPlayOver = false;
        if (index != -1)
        {
            curAnimIdx = index;
        }
        skeletonGraphic.AnimationState.ClearTracks();
        var name = animList[curAnimIdx];
        skeletonGraphic.Initialize(true);
        if (name.Contains("idle"))
            skeletonGraphic.AnimationState.SetAnimation(0, name, true);
        skeletonGraphic.AnimationState.SetAnimation(0, name, false).Complete += OnAnimationComplete;
    }

    private void OnAnimationComplete(Spine.TrackEntry trackEntry)//回到待机
    {
        skeletonGraphic.AnimationState.Complete -= OnAnimationComplete;
        curAnimIdx = 0;
        PlaySpineAnim(0);
        isPlayOver = true;
    }

    /// <summary>
    /// 播放荷官单张发牌动画并等待其完整播完(串行门控:本张播完才允许发下一张)
    /// 发牌期间不挂 OnAnimationComplete,避免中途回待机打断发牌序列
    /// </summary>
    private IEnumerator PlayDealerDealAnim(int dealIndex)
    {
        if (skeletonGraphic == null || dealIndex < 0 || dealIndex >= dealerDealAnims.Length)
            yield break;

        skeletonGraphic.AnimationState.ClearTracks();
        skeletonGraphic.Initialize(true);
        var trackEntry = skeletonGraphic.AnimationState.SetAnimation(0, dealerDealAnims[dealIndex], false);
        yield return new WaitForSpineAnimationComplete(trackEntry);
    }

    /// <summary>
    /// 播放一次荷官发牌动画后回到待机(不阻塞调用方:供明牌局与飞牌并行播放)
    /// </summary>
    private IEnumerator PlayDealerDealAnimThenIdle(int dealIndex)
    {
        yield return StartCoroutine(PlayDealerDealAnim(dealIndex));
        PlaySpineAnim(0);//荷官回到待机
        isDealing = false;//发牌结束,解除锁定
        isPlayOver = true;//发牌结束,允许发筹码等其他荷官动画
    }

    public void UpdateBetSwitch()
    {
        if (!OriginalBaccaratUiPresent) return;
        var UICanvas = transform.Find("UICanvas");
        var betCoinParent = UICanvas.Find("Bottom/Center/BetList");
        for (int i = 0; i < betCoinParent.childCount; i++)
        {
            var betCoin = betCoinParent.GetChild(i).GetComponent<CardHover>();
            if (betCoin.SetIndex(i))
                betCoinViews.Add(betCoin);
        }
    }

    #endregion

    #region netJackpot
    public void SetNetJPIcon()
    {
        Model.Instance.netJackpotState = IOCanvasModel.Instance.jackpotCfg.jackpotSwitch == (int)IOJackpotSwitch.JackpotSwitchOn ? (NetMgr.Instance.CurNetStatus == NET_STATUS.NET_STATUS_CONNECTED ? 2 : 1) : 0;
        // 联网彩金图标挂在原九门界面底部，纯轮盘机台下不存在。
        if (!OriginalBaccaratUiPresent || netJPIcon == null) return;
        netJPIcon.gameObject.SetActive(Model.Instance.netJackpotState > 0);
        int setGray = Model.Instance.netJackpotState == 2 ? 0 : 1;
        netJPIcon.material.SetFloat("_SetGray", setGray);
    }

    public void ShowWinNetJackpot(WinJackpotInfo winJackpotInfo)
    {        
        Debug.LogError("ShowWinNetJackpot");
        if(winJackpotInfo.seat != SBoxModel.Instance.macId) return; 
        if (Model.Instance.showingNetJackpot)
        {
            Model.Instance.winJackpotInfos.Enqueue(winJackpotInfo);
            return;
        }
        Model.Instance.showingNetJackpot = true;
        netJackpotView.WinNetJackpotAni(winJackpotInfo);
    }

    /// <summary>
    /// 结算后处理所有中奖信息:多台分机可同时中联网彩金,统一并行处理,避免只保留最后一条
    /// </summary>
    private IEnumerator ProcessWinNetJackpots()
    {
        //取快照并清空(快照+清空在首个 yield 前原子完成,重入安全)
        List<WinJackpotInfo> jackpotBatch = new List<WinJackpotInfo>(Controller.Instance.winJackpotInfoSave);
        Controller.Instance.winJackpotInfoSave.Clear();
        Controller.Instance.isHaveNetJackpot = false;
        if (jackpotBatch.Count == 0) yield break;//无中奖,不产生额外的等待

        //先过滤掉不在 playerDic 的中奖,后续通知与加分共用同一份列表
        List<WinJackpotInfo> validBatch = new List<WinJackpotInfo>();
        foreach (var winJackpotInfo in jackpotBatch)
        {
            if (!PlayerMgr.Instance.playerDic.TryGetValue(winJackpotInfo.seat, out Player player))
            {
                Debug.LogError($"未找到中奖分机:{winJackpotInfo.seat}");
                continue;
            }
            validBatch.Add(winJackpotInfo);
        }
        if (validBatch.Count == 0) yield break;

        //并行通知:所有中奖者一次性各自下发/播动画
        foreach (var winJackpotInfo in validBatch)
        {
            Player player = PlayerMgr.Instance.playerDic[winJackpotInfo.seat];
            MsgInfo msgInfo = new MsgInfo
            {
                cmd = (int)S2C_CMD.S2C_WinNetJackpot,
                id = player.macId,
                jsonData = JsonConvert.SerializeObject(winJackpotInfo)
            };
            if (msgInfo.id != 1)//不是自己本机中奖,通知
            {
                NetMgr.Instance.SendToClient(player.client, JsonConvert.SerializeObject(msgInfo));
            }
            else
                ShowWinNetJackpot(winJackpotInfo);//本机中奖走现有队列顺序播放
        }

        //只统一等待一次(并行,而非每个中奖各等9秒)
        yield return new WaitForSeconds(9);

        //逐个累加各中奖者总分
        foreach (var winJackpotInfo in validBatch)
        {
            SBoxWinNetJackpotInfo sBoxWinNetJackpotInfo = new SBoxWinNetJackpotInfo
            {
                PlayerId = winJackpotInfo.seat,
                JackpotType = winJackpotInfo.jackpotId,
                JackpotWins = winJackpotInfo.win / 100 * IOCanvasModel.Instance.CfgData.CoinValue
            };
            Debug.Log("中联网彩金,更新总分:" + sBoxWinNetJackpotInfo.JackpotWins);
            SBoxIdea.SetPlayerWinNetJackpotInfo(sBoxWinNetJackpotInfo);
        }
        yield return new WaitForSeconds(1f);
    }
    public void ShowHlepContentAboutNetJackpot(bool flag)
    {
        if (flag)
        {
            bool showBetsMinOfNetJackpotObj = IOCanvasModel.Instance.netJackpotMinBets != null && IOCanvasModel.Instance.netJackpotMinBets.Count > 0 && Model.Instance.netJackpotState > 1;
            // betsMinOfNetJackpotTexts[0].transform.parent.gameObject.SetActive(showBetsMinOfNetJackpotObj);//区分联网彩金
            var temp = nomalHelpContentObj.GetComponentsInChildren<Image>(true);
            foreach (var child in temp)
            {
                if(child.gameObject.name.Contains("Njack"))
                    child.gameObject.SetActive(!showBetsMinOfNetJackpotObj);
                if(child.gameObject.name.Contains("Yjack"))
                    child.gameObject.SetActive(showBetsMinOfNetJackpotObj);
            }
            if (showBetsMinOfNetJackpotObj)
            {
                for (int i = 0; i < IOCanvasModel.Instance.netJackpotMinBets.Count; i++)
                    betsMinOfNetJackpotTexts[i].text = (IOCanvasModel.Instance.netJackpotMinBets[i] * IOCanvasModel.Instance.CfgData.CoinValue).ToString();
            }
        }
    }
    #endregion

    #region forController
    private void HideBeforeShowResult()
    {
        mingPaiObj.gameObject.SetActive(false);//明牌logo隐藏
        if (Model.Instance.curRecord.luckyId == 8)
        {
            Model.Instance.myTriggerDetectionCanEnterCount--;//彩金播放完成,触摸区域能响应事件了
            fireworkObj.gameObject.SetActive(false);//彩金特效隐藏
            AudioManager.Instance.StopSoundEff("coinEff");
        }
    }

    public void ShowResult(RecordStruct recordStruct)
    {
        Debug.LogError("View-ShowResult");
        HideBeforeShowResult();
        StartCoroutine(ShowResultCoroutine(recordStruct));
    }

    public void ShowResultSpecialFinish(RecordStruct recordStruct)
    {
        StartCoroutine(ShowResultSpecialFinishCoroutine(recordStruct));
    }
    //特殊结束
    private IEnumerator ShowResultSpecialFinishCoroutine(RecordStruct recordStruct)
    {
        // 纯轮盘机台不走九门开奖流程。
        if (!OriginalBaccaratUiPresent) yield break;
        ShowResultFinish();


        int result2 = recordStruct.result;
        if (result2 == 3) Model.Instance.isJoker = true;   // 和局标志（原皇冠功能转移，开"和"时为 true）
        ShowResultWin(result2);

        SBoxIdea.BattleLeadStop();//出赢分
        Debug.Log($"本局游戏赢分2:{PlayerMgr.Instance.localPlayer.outState.wins}");

        SetPokerValue(recordStruct.value[0]);//增加
        AudioManager.Instance.PlayVoiceEff(GetResultVoiceKey(recordStruct.result));//特殊结束/明牌局：仅播结算语音

        yield return new WaitForSeconds(2f);//等待赢分缩放时间

        ShowBetSettlementPanel();
        //FakeCredit 这里恢复刷新分数
        Model.Instance.fadeCredit = false;//恢复本机刷新分数
        MsgInfo fadeMsg = new MsgInfo//通知分机恢复刷新分数
        {
            cmd = (int)S2C_CMD.S2C_CancleFadeCredit,
            id = -1,
        };
        NetMgr.Instance.SendToAllClient(JsonConvert.SerializeObject(fadeMsg));
        yield return new WaitForSeconds(5);//等待结算页面时间5
        HideBetSettlementPanel();//隐藏赢分结算界面

        foreach (var player in PlayerMgr.Instance.playerDic.Values)
        {
            var temp = SaveRecordSnapShot(player);//记录快照++++++++++++++++++++
            if (temp.playerData.selfBetTextList.Sum() > 0 ||                //限制1是自己总押
                (temp.bankerDataStruct.roleBetTotalText != 0 &&             //限制2是庄家看全台总押
                temp.bankerDataStruct.BankerSeat == player.macId) )                                                                                                       //限制2不是庄家看自己总押分
                {        
                    if (player.totalDataStruct.Count >= 10)
                        player.totalDataStruct.RemoveAt(0);
                    player.totalDataStruct.Add(temp);
                    player.OnUpdateSQLRecord2();//存储到本地数据库中++++++++++++++++++++
                    Debug.LogError($"保存了{player.macId}号分机的数据");
                //关联playerID与List，后推送到SQL中
            }
            else
            {
                Debug.LogError($"跳过了{player.macId}号分机的数据");
            }       
            MsgInfo MsgHis = new MsgInfo//发送到分机
            {
                cmd = (int)S2C_CMD.S2C_UpdateHistoryData,
                id = -1,
                jsonData = JsonConvert.SerializeObject(player.totalDataStruct),
            };
            NetMgr.Instance.SendToClient(player.client,JsonConvert.SerializeObject(MsgHis));
        }
        SBoxIdea.MovePlayerScore(new List<int>());//赢分累加
        yield return new WaitForSeconds(4);
        
        //Debug.LogError("彩金后移显示");
        if (Controller.Instance.isHaveNetJackpot)//彩金后移显示
        {
            yield return StartCoroutine(ProcessWinNetJackpots());
        }
        int result = recordStruct.result;
        HideResultWin(result);
        //告诉分机重置 - kuarser新增
        MsgInfo msg = new MsgInfo
        {
            cmd = (int)S2C_CMD.S2C_ResetView,
            id = -1,
            jsonData = JsonConvert.SerializeObject(recordStruct),
        };
        NetMgr.Instance.SendToAllClient(JsonConvert.SerializeObject(msg));

        ChangeCountDownState4Controller(CountDownState.None);

        ResetPokerCard();//重置动画

        HideSymbolView();

        Model.Instance.flashing = false;
        SandboxController.Instance.TurnOffBetLight();
        SandboxController.Instance.TurnOnTheLight(SBOX_SWITCH.SWITCH_ESC);

        Debug.LogError($"特殊结束-轮:{Model.Instance.curRecord.round}局:{Model.Instance.curRecord.game}");
        EndGameChangeState();
    }

    public void ChangeCountDownState4Controller(CountDownState curState)
    {
        if (!OriginalBaccaratUiPresent) return;
        uiMainView.ChangeCountDownState(curState);
    }

    public IEnumerator PokerAnimation4Controller(RecordStruct recordStruct)
    {
        // 纯轮盘机台没有卡牌动画。
        if (!OriginalBaccaratUiPresent) yield break;
        yield return PokerAniTest(recordStruct);
    }

    public void RefreshBanker()
    {
        if (!OriginalBaccaratUiPresent) return;
        uiBankerView.RefreshBancker();
    }

    public void HideBetSettlementPanel()
    {
        uiBetSettlementView.ChangeBetSettlementPanelActive(false);//隐藏赢分结算界面

    }

    public void ShowBetSettlementPanel()
    {
        uiBetSettlementView.ChangeBetSettlementPanelActive(true);//显示赢分结算界面
    }

    public void ShowLuckySeatAboutAllWin(int luckyId, int resultIndex)
    {
        if (!OriginalBaccaratUiPresent) return;
        if (luckyId == 7)
        {
            //大满贯
            for (int i = 0; i < symbolViewsList.Count; i++)
            {
                symbolViewsList[i].HideLightIcon();
                //symbolViewsList2[i].HideLightIcon();
            }
            uiRoadMapView.SpecialAllWin();
        }
        else
        {
            symbolViewsList[resultIndex - 1].HideLightIcon();
            //symbolViewsList2[result - 1].HideLightIcon();
            uiRoadMapView.ShowResultWin(resultIndex - 1);
        }
    }

    public void ShowResultFinish()
    {
        // 明牌 logo 属于 PopCanvas，纯轮盘机台下仍在位，需保留隐藏动作。
        if (mingPaiObj != null) mingPaiObj.gameObject.SetActive(false);//隐藏明牌logo
        setBidBankerBtngrey(false);//重置庄家按钮为可用状态
        Model.Instance.canBidBanker = true;
        SandboxController.Instance.TurnOnEnterLight();//可抢庄,抢庄键灯亮
        PlayerMgr.Instance.localPlayer.ResetBankerKeyState();
        ResetCardList();//隐藏小游戏卡片
    }

    public void ShowResultWin(int resultIndex)
    {
        if (!OriginalBaccaratUiPresent) return;
        symbolViewsList[resultIndex - 1].ShowLightIcon();//显示结果中奖闪烁
                                                         //symbolViewsList2[resultIndex - 1].ShowLightIcon();//显示结果中奖闪烁
        uiRoadMapView.ShowResultWin(resultIndex - 1);
    }

    public void HideResultWin(int result)
    {
        if (!OriginalBaccaratUiPresent) return;
        symbolViewsList[result - 1].HideLightIcon();
        uiRoadMapView.HideResultWin(result - 1);

    }
    public IEnumerator ShowMingPaiLogo()
    {
        mingPaiObj.gameObject.SetActive(true);//显示明牌logo 
        string animationName = IOCanvasModel.Instance.curlanguage == Language.en?"yw":"zw";
        mingPaiObj.AnimationState.SetAnimation(0,"Star_"+animationName,false);
        yield return new WaitForSeconds(1.1f);
        mingPaiObj.AnimationState.SetAnimation(0,"Idel_"+animationName,true);
        yield return new WaitForSeconds(2f);
        mingPaiObj.AnimationState.SetAnimation(0,"Over_"+animationName,false);
        yield return new WaitForSeconds(1.1f);
        mingPaiObj.gameObject.SetActive(false); 
    }

    public float GetPokerTurnAnimationTime()
    {
        // 纯轮盘机台没有翻牌动画，返回 0 让调用方的等待直接跳过。
        if (!OriginalBaccaratUiPresent || pokerTurn == null) return 0f;
        return pokerTurn.GetCurrentAnimatorStateInfo(0).length;
    }

    public void RefreshAllRecords()
    {
        if (!OriginalBaccaratUiPresent) return;
        //主界面
        uiRecordView.ShowDaLu();
        uiRecordView.ShowZhuPanLu();

        //二级界面
        //uiRoadMapView.ShowZhuPanLu();
        uiRoadMapView.ShowDaLu();
        uiRoadMapView.ShowDaYanLu();
        uiRoadMapView.ShowXiaoLu();
        uiRoadMapView.ShowXiaoQiangLu();
    }

    public void ShowTypeCount()
    {
        if (!OriginalBaccaratUiPresent) return;
        for (int i = 0; i < winCountTexts.Count; i++)
        {
            //按理说,他们是存储同一种数据,所以应该是同样的list个数,可以在一个循环里
            winCountTexts[i].text = Model.Instance.recordTypeCounts[i].ToString();//每种类型的中奖次数
            //uiRoadMapView.UpdateRecordTypeCountText(i, Model.Instance.recordTypeCounts[i]);
        }
    }
    //正常结束
    private IEnumerator ShowResultCoroutine(RecordStruct recordStruct)
    {
        // 纯轮盘机台不走九门开奖流程。
        if (!OriginalBaccaratUiPresent) yield break;
        Debug.LogError("ShowResultCoroutine");
        ChangeCountDownState4Controller(CountDownState.Dealing);


        yield return StartCoroutine(PokerAnimation4Controller(recordStruct));

        MsgInfo msgInfo = new MsgInfo
        {
            cmd = (int)S2C_CMD.S2C_ShowResultWin,
            id = -1,
            jsonData = JsonConvert.SerializeObject(recordStruct),
        };
        NetMgr.Instance.SendToAllClient(JsonConvert.SerializeObject(msgInfo));


        yield return new WaitForSeconds(2f);

        RefreshBanker();
        ShowBetSettlementPanel();
        //FakeCredit 这里恢复刷新分数(正常结束对称恢复)
        Model.Instance.fadeCredit = false;//恢复本机刷新分数
        MsgInfo fadeMsg = new MsgInfo//通知分机恢复刷新分数
        {
            cmd = (int)S2C_CMD.S2C_CancleFadeCredit,
            id = -1,
        };
        NetMgr.Instance.SendToAllClient(JsonConvert.SerializeObject(fadeMsg));
        yield return new WaitForSeconds(5);//等待结算页面时间※
        HideBetSettlementPanel();//隐藏赢分结算界面

        foreach (var player in PlayerMgr.Instance.playerDic.Values)
        {
            var temp = SaveRecordSnapShot(player);//记录快照++++++++++++++++++++
            if (temp.playerData.selfBetTextList.Sum() > 0 ||                //限制1是自己总押
                (temp.bankerDataStruct.roleBetTotalText != 0 &&             //限制2是庄家看全台总押
                temp.bankerDataStruct.BankerSeat == player.macId) )                                                                                                       //限制2不是庄家看自己总押分
                {        
                    if (player.totalDataStruct.Count >= 10)
                        player.totalDataStruct.RemoveAt(0);
                    player.totalDataStruct.Add(temp);
                    player.OnUpdateSQLRecord2();//存储到本地数据库中++++++++++++++++++++
                    Debug.LogError($"保存了{player.macId}号分机的数据");
                //关联playerID与List，后推送到SQL中
            }
            else
            {
                Debug.LogError($"跳过了{player.macId}号分机的数据");
            }       
            MsgInfo MsgHis = new MsgInfo//发送到分机
            {
                cmd = (int)S2C_CMD.S2C_UpdateHistoryData,
                id = -1,
                jsonData = JsonConvert.SerializeObject(player.totalDataStruct),
            };
            NetMgr.Instance.SendToClient(player.client,JsonConvert.SerializeObject(MsgHis));
        }

        SBoxIdea.MovePlayerScore(new List<int>());//把赢分累加给总分
        if (PlayerMgr.Instance.localPlayer.outState.wins > 0)
        {
            AudioManager.Instance.PlaySoundEff("coinEff", true);
        }
        yield return new WaitForSeconds(3);//等待赢分移动时间
        AudioManager.Instance.StopSoundEff("coinEff");

        //Debug.LogError("彩金后移显示");
        if (Controller.Instance.isHaveNetJackpot)//彩金后移显示
        {
            yield return StartCoroutine(ProcessWinNetJackpots());
        }
        //告诉分机重置
        MsgInfo msg = new MsgInfo
        {
            cmd = (int)S2C_CMD.S2C_ResetView,
            id = -1,
            jsonData = JsonConvert.SerializeObject(recordStruct),
        };
        NetMgr.Instance.SendToAllClient(JsonConvert.SerializeObject(msg));
        ShowResultFinish();//newPos

        ////停止押注
        ChangeCountDownState4Controller(CountDownState.None);
        //重置动画
        ResetPokerCard();
        HideSymbolView();

        int result = recordStruct.result;
        ShowLuckySeatAboutAllWin(recordStruct.luckyPlayerId, result);//显示中奖闪烁
        //OriginPos
        HideSymbolView();//隐藏中奖闪烁

        //好像和硬件机器的灯光有关
        Model.Instance.flashing = false;
        SandboxController.Instance.TurnOffBetLight();
        SandboxController.Instance.TurnOnTheLight(SBOX_SWITCH.SWITCH_ESC);

        Debug.LogError($"正常结束-轮:{Model.Instance.curRecord.round}局:{Model.Instance.curRecord.game}");
        EndGameChangeState();
    }

    //结束本局游戏,根据条件转换状态
    private void EndGameChangeState()
    {
        // 轮盘模式：原九门开奖结束不再推进 FSM，状态推进由 RouletteFlowBridge 负责。
        if (Model.Instance.rouletteFlow) return;
        if (Model.Instance.wait4Setting)
            FSMSystem.Instance.PerformTransition(Transition.Setting);
        else
        {
            if (FSMSystem.Instance.CurrentStateID != StateID.Error)
            {
                if (Model.Instance.errorCodeList.Count > 0)
                {
                    FSMSystem.Instance.PerformTransition(Transition.Error);
                    SBoxIdea.BattleEndRound();
                }
                else
                    FSMSystem.Instance.PerformTransition(Transition.Prepare);
            }
        }
    }
    public void setBidBankerBtngrey(bool isGrey)
    {
        if (!OriginalBaccaratUiPresent) return;
        uiBankerView.setBidBankerBtngrey(isGrey);
    }
    public TotalDataStruct SaveRecordSnapShot(Player player)//写在这里是因为这个函数需要调用UI数据,所以放在View里,如果放在Model里就不好调用UI数据- kuarser
    {
        int ParseIntSafe(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0;
            if (int.TryParse(text, out var value))
                return value;
            return 0;
        }

        //保存快照，抢庄部分-kuarser
        var bankerData = Model.Instance.banckerData;
        BankerDataStruct bankerDataStruct = new BankerDataStruct()
        {
            BankerSeat = bankerData != null ? bankerData.BankerPlayerId : 0,
            Priority = bankerData != null ? bankerData.Priority : new int[6],
            PriorityLights = bankerData != null ? bankerData.PriorityLights : new int[6],
            CanGrabMask = bankerData != null ? bankerData.CanGrabMask : 0,
            EstimateBankerProfit = bankerData != null ? bankerData.EstimateBankerProfit : 0,
            EstimatePlayerProfit = bankerData != null ? bankerData.EstimatePlayerProfit : 0,
            WaterRateBasisPoints = bankerData != null ? bankerData.WaterRateBasisPoints : 0,
            TotalBets = bankerData != null ? bankerData.TotalBets : 0,
            TotalWins = bankerData != null ? bankerData.TotalWins : 0,
            BankerWins = bankerData != null ? bankerData.BankerWins : 0,
            BankerProfit = bankerData != null ? bankerData.BankerProfit : 0,
            Cut = bankerData != null ? bankerData.Cut : 0,
            CutOdds = bankerData != null ? bankerData.CutOdds : 0,
            roleBetTotalTextList = Model.Instance.totalBets,
            backerWinText = bankerData != null ? bankerData.BankerWins : 0,
            backerProfitText = bankerData != null ? bankerData.BankerProfit : 0,
            backerCutText = bankerData != null ? bankerData.Cut : 0,
            backerCutOddsText = bankerData != null ? bankerData.CutOdds : 0,
            roleBetTotalText = bankerData != null ? bankerData.TotalBets : 0,
            roleWinTotalText = bankerData != null ? bankerData.TotalWins : 0
        };
        playerData playerData = new playerData()
        {
            selfBetTextList = player.outState.bets,
            selfWinScore = player.outState.wins,
            selfCredit = bankerDataStruct.BankerSeat == player.macId ?
            player.outState.credit - player.outState.wins - bankerDataStruct.backerProfitText :
            player.outState.credit - player.outState.wins,
        };
        TotalDataStruct totalDataStruct = new TotalDataStruct()
        {
            bankerDataStruct = bankerDataStruct,
            recordStruct = Model.Instance.curRecord,
            playerData = playerData,
            // 纯轮盘机台没有彩金文本，历史快照里的彩金值记为 0。
            jackpotValue = OriginalBaccaratUiPresent && uiMainView != null && uiMainView.jackpotText != null
                ? ParseIntSafe(uiMainView.jackpotText.text) : 0,
        };
        return totalDataStruct;
    }
    #endregion

}
