using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class UIRoadMapView : MonoBehaviour
{
    // private GameObject RecordViewPanel;//牌路界面 [临时禁用] PopCanvas/RecordView 2026-07-31
    // private Button recordBtn;//牌路界面显示按钮 [临时禁用] PopCanvas/RecordView 2026-07-31
    // private Button recordBackBtn;//牌路界面返回按钮 [临时禁用] PopCanvas/RecordView 2026-07-31
    private CanvasGroup symbolViewsListParent;
    private List<SymbolView> symbolViewsList2 = new List<SymbolView>();//押注视图和结果显示视图
    //private ScrollRect recordViewPanelRecordListRect;//露珠界面试图的滚动列表
    private Image Bg_half;


    //牌路界面的大眼路视图列表
    //private List<RecordDaYanView> recordViewPanelRecordDaYanView = new List<RecordDaYanView>();//大眼路,小路,小强路等视图的列表集合,旧
    private List<ScrollRect> recordViewPanelRecordDaYanRect = new List<ScrollRect>();

    private List<RecordDaYanView_SelfAddItem> recordDaYanView_SelfAddItems = new List<RecordDaYanView_SelfAddItem>();//牌路界面的大眼路,小路,小强路等视图的列表集合,优化版,够用就行,不够再加,不用新建一大堆,浪费空间

    //牌路界面的数据视图列表

    private List<RecordView> recordViewPanelRecordViewList = new List<RecordView>();//本轮结果显示视图  //珠盘路


    private RecordListView_SelfAddItem recordListView_SelfAddItem;//露珠界面视图,大路视图,优化版,

    //牌路界面的数据列表
    private Text recordViewPanelGameText;//第几局,最大100
    private Text recordViewPanelRoundText;//第几轮,无上限
    //private List<Text> recordViewPanelCountTexts = new List<Text>();//本轮押注中奖次数文本
    //牌路界面新UI
    private Animator recordViewPanelHourglassAnimator;//沙漏动画
    private List<Image> blueList = new List<Image>();
    private List<Image> redList = new List<Image>();
    private GameObject helpPanelRoad;
    private Button helpCloseBtn;
    // private Button helpBtn; // [临时禁用] 位于 PopCanvas/RecordView 内 2026-07-31

    private void Awake()
    {
        InitView();
    }

    void Start()
    {

    }

    private void InitView()
    {
        var UICanvas = GameObject.Find("UICanvas").transform;
        var popCanvas = GameObject.Find("PopCanvas").transform;

        var symbolList = UICanvas.Find("Center/SymbolList");
        symbolViewsListParent = symbolList.GetComponent<CanvasGroup>();

        // [临时禁用] PopCanvas/RecordView 路单弹窗面板 2026-07-31(RecordViewPanel 初始化与 recordBtn/recordBackBtn 开关监听)
        // RecordViewPanel = popCanvas.Find("RecordView").gameObject;
        // RecordViewPanel.SetActive(false);
        // recordBtn = UICanvas.Find("Bottom/RightUp/pailuBtn").GetComponent<Button>();
        // recordBtn.onClick.RemoveAllListeners();
        // recordBtn.onClick.AddListener(() =>
        // {
        //     ChangePanelActive(RecordViewPanel);
        //     Controller.Instance.view.ChangeSymbolListObjActive();
        //     symbolViewsListParent.alpha = 0;
        //     BGHalfSetAlpha(1f);
        //     AudioManager.Instance.PlaySoundEff("buttonClick");
        // });
        // recordBackBtn = RecordViewPanel.transform.Find("BG/BackBtn").GetComponent<Button>();
        // recordBackBtn.onClick.RemoveAllListeners();
        // recordBackBtn.onClick.AddListener(() =>
        // {
        //     ChangePanelActive(RecordViewPanel);
        //     Controller.Instance.view.ChangeSymbolListObjActive();
        //     symbolViewsListParent.alpha = 1;
        //     BGHalfSetAlpha(0f);
        //     AudioManager.Instance.PlaySoundEff("buttonClick");
        // });

        Bg_half = UICanvas.Find("BG_half").GetComponent<Image>();//半透明背景图片,用于牌路界面和历史记录界面
        if (!Bg_half.gameObject.activeSelf)
            Bg_half.gameObject.SetActive(true);
        BGHalfSetAlpha(0f);


        //牌路界面数据
        var recordParent2 = UICanvas.Find("Top/Right/RecordView/BG/RecordList");
        for (int i = 0; i < recordParent2.childCount; i++)
        {
            recordViewPanelRecordViewList.Add(recordParent2.GetChild(i).GetComponent<RecordView>());//牌路界面的100结果显示视图
            recordViewPanelRecordViewList[i].HideRecordView();
        }

        recordListView_SelfAddItem = UICanvas.Find("Top/Right/RecordView/BG/record/ScrollView/Viewport/Content/recordList2_SelfAddItem").GetComponent<RecordListView_SelfAddItem>();
        //recordViewPanelRecordListRect = UICanvas.Find("Top/Right/RecordView/BG/record/ScrollView").GetComponent<ScrollRect>();

        var blueParent = UICanvas.Find("Top/Right/RecordView/BG/Tip/blue");//player
        for (int i = 0; i < blueParent.childCount; i++)
        {
            blueList.Add(blueParent.GetChild(i).GetComponent<Image>());//牌路界面提示的蓝色圆点
            blueList[i].gameObject.SetActive(false);
        }
        var redParent = UICanvas.Find("Top/Right/RecordView/BG/Tip/red");//banker
        for (int i = 0; i < redParent.childCount; i++)
        {
            redList.Add(redParent.GetChild(i).GetComponent<Image>());//牌路界面提示的红色圆点
            redList[i].gameObject.SetActive(false);
        }

        recordViewPanelGameText = UICanvas.Find("Top/Right/RecordView/BG/Info/game").GetComponent<Text>();//第几局文本
        recordViewPanelGameText.text = "";
        recordViewPanelRoundText = UICanvas.Find("Top/Right/RecordView/BG/Info/round").GetComponent<Text>();//第几轮文本
        recordViewPanelRoundText.text = "";

        // var countParent2 = UICanvas.Find("Top/Right/RecordView/BG/Info/Count");
        // for (int i = 0; i < countParent2.childCount; i++)
        // {
        //     recordViewPanelCountTexts.Add(countParent2.GetChild(i).GetComponent<Text>());//本轮押注中奖次数文本
        //     recordViewPanelCountTexts[i].text = "0";
        // }

        recordViewPanelRecordDaYanRect.Add(UICanvas.Find("Top/Right/RecordView/BG/record/Scroll View1").GetComponent<ScrollRect>());
        recordViewPanelRecordDaYanRect.Add(UICanvas.Find("Top/Right/RecordView/BG/record/Scroll View2").GetComponent<ScrollRect>());
        recordViewPanelRecordDaYanRect.Add(UICanvas.Find("Top/Right/RecordView/BG/record/Scroll View3").GetComponent<ScrollRect>());

        recordDaYanView_SelfAddItems.Add(UICanvas.Find("Top/Right/RecordView/BG/record/Scroll View1/Viewport/Content/recordDayan_SelfAddItem").GetComponent<RecordDaYanView_SelfAddItem>());
        recordDaYanView_SelfAddItems.Add(UICanvas.Find("Top/Right/RecordView/BG/record/Scroll View2/Viewport/Content/recordXiaoLu_SelfAddItem").GetComponent<RecordDaYanView_SelfAddItem>());
        recordDaYanView_SelfAddItems.Add(UICanvas.Find("Top/Right/RecordView/BG/record/Scroll View3/Viewport/Content/recordXiaoQiangLu_SelfAddItem").GetComponent<RecordDaYanView_SelfAddItem>());


        //牌路界面新的压分界面 [临时禁用] 2026-07-31
        // var symbolList2 = RecordViewPanel.transform.Find("Symbol_New/SymbolList");
        // for (int i = 0; i < symbolList2.childCount; i++)
        // {
        //     var symbol = symbolList2.GetChild(i).GetComponent<SymbolView>();
        //     symbol.index = i;
        //     // symbolViewsList2.Add(symbol);//中奖视图
        //     // symbolViewsList2[i].HideLightIcon();//先隐藏全部symbol中奖特效特效
        // }
        //路单说明界面
        helpPanelRoad = popCanvas.transform.Find("helpPanelRoad").gameObject;
        // helpBtn = RecordViewPanel.transform.Find("BG/ExplanationBtn").GetComponent<Button>(); // [临时禁用] PopCanvas/RecordView 内 2026-07-31
        // helpBtn.onClick.RemoveAllListeners();
        // helpBtn.onClick.AddListener(() =>
        // {
        //     ChangeHelpPanelActive();
        //     AudioManager.Instance.PlaySoundEff("buttonClick");
        // });
        helpCloseBtn = helpPanelRoad.transform.Find("closeBtn").GetComponent<Button>();
        helpCloseBtn.onClick.RemoveAllListeners();
        helpCloseBtn.onClick.AddListener(() =>
        {
            ChangeHelpPanelActive();
            AudioManager.Instance.PlaySoundEff("buttonClick");
        });
    }
    public void ChangeHelpPanelActive()
    {
        helpPanelRoad.SetActive(!helpPanelRoad.activeSelf);
                Model.Instance.myTriggerDetectionCanEnterCount = helpPanelRoad.activeSelf ? Model.Instance.myTriggerDetectionCanEnterCount + 1 : Model.Instance.myTriggerDetectionCanEnterCount - 1;//总结界面显示触摸区域不能响应,总结界面隐藏触摸区域能响应

    }
    private void ChangePanelActive(GameObject objPanel)
    {
        if (!objPanel.activeSelf)
        {
            objPanel.SetActive(true);
            objPanel.transform.localScale = Vector3.zero;
        }
        if (objPanel.transform.localScale.Equals(Vector3.zero))
        {
            objPanel.transform.localScale = Vector3.one;
            // recordViewPanelRecordListRect.horizontalNormalizedPosition = 0;//控制滑动条默认在最左边,最开始//路牌界面数据
            for (int i = 0; i < recordViewPanelRecordDaYanRect.Count; i++)
            {
                recordViewPanelRecordDaYanRect[i].horizontalNormalizedPosition = 0;
            }
        }
        else
        {
            objPanel.transform.localScale = Vector3.zero;
        }
    }

    private void BGHalfSetAlpha(float num)//透明度控制背景遮罩,显示隐藏
    {
        Color tempColor = Bg_half.color;
        tempColor.a = num;
        Bg_half.color = tempColor;
    }

    public void SpecialAllWin()//压分全中,押注区域全亮
    {
        //symbolViewsList2.ForEach(item => item.ShowLightIcon());
    }
    public void ShowResultWin(int result)//中奖押注区域亮
    {
        //symbolViewsList2[result].ShowLightIcon();
    }
    public void HideResultWin(int result)//中奖押注区域不亮
    {
        //symbolViewsList2[result].HideLightIcon();
    }

    public void UpdateRoundAndGameText()//更新轮,局
    {
        recordViewPanelRoundText.text = Model.Instance.curRound.ToString();//轮数
        recordViewPanelGameText.text = Model.Instance.curGame.ToString();//局数
    }

    public void UpdateAllBetText(int index, int value)
    {
        //symbolViewsList2[index].SetAllBet(value);
    }

    public void UpdateBetText(int index, int value)
    {
        //symbolViewsList2[index].SetBet(value);
    }

    public void UpdateResultWinTotalBet(int index, int value)
    {
        //symbolViewsList2[index].SetSymbolView(value);//显示所有玩家押注
    }

    public void ResetPredict()
    {
        for (int i = 0; i < redList.Count; i++)
            redList[i].gameObject.SetActive(false);
        for (int i = 0; i < blueList.Count; i++)
            blueList[i].gameObject.SetActive(false);
    }

    public void ShowPredict()//本局预测结果
    {
        for (int i = 0; i < recordDaYanView_SelfAddItems.Count; i++)
        {
            int temp = Model.Instance.lastValueYuce[i];
            int temp2 = -1;
            if (temp != -1)
                temp2 = temp == 0 ? 1 : 0;//输入形参:1:庄;0:闲;//结果:0:庄,1:闲,-1:无结果

            redList[i].sprite = Resources.Load<Sprite>($"NextTip/" + i + "_" + temp2);
            blueList[i].sprite = Resources.Load<Sprite>($"NextTip/" + i + "_" + temp);
            if (redList[i].sprite != null)
            {
                redList[i].gameObject.SetActive(true);
            }
            else
            {
                redList[i].gameObject.SetActive(false);
            }
            if (blueList[i].sprite != null)
            {
                blueList[i].gameObject.SetActive(true);
            }
            else
            {
                blueList[i].gameObject.SetActive(false);
            }
        }
        
        MsgInfo msgInfo = new MsgInfo
        {
            cmd = (int)S2C_CMD.S2C_UpdateHistoryData6,
            id = -1,
            jsonData = JsonConvert.SerializeObject(Model.Instance.lastValueYuce)
        };
        NetMgr.Instance.SendToAllClient(JsonConvert.SerializeObject(msgInfo));
    }

    // public void ShowRecordView(RecordStruct record)//显示一局的中奖结果
    // {
    //     //牌路界面数据 的100局结果显示视图
    //     var recordView2 = recordViewPanelRecordViewList[record.game - 1];
    //     recordView2.SetRecordView(record);
    //     recordView2.gameObject.SetActive(true);
    // }

    // public void UpdateRecordTypeCountText(int index, int count)//更新 一种中奖结果的 类型的个数
    // {
    //     recordViewPanelCountTexts[index].text = count.ToString();
    // }

    public void SetRecords(List<List<RecordItemView>> daluRecords)
    {
        if (Model.Instance.records == null) return;
        if (Model.Instance.records.Count <= 0)
        {
            // recordViewPanelRecordListRect.horizontalNormalizedPosition = 0;//控制滑动条默认在最左边,最开始//路牌界面数据
        }

        recordListView_SelfAddItem.SetRecords(Model.Instance.records);
        for (int i = 0; i < recordDaYanView_SelfAddItems.Count; i++)
        {
            recordDaYanView_SelfAddItems[i].ShowDayanRecord();
            recordViewPanelRecordDaYanRect[i].horizontalNormalizedPosition = 0;
        }
    }

    public void UpdateRecords()//arg:List<List<RecordItemView>> daluRecords
    {
        //recordListView_SelfAddItem.SetRecords(Model.Instance.records);
        //recordViewPanelRecordListRect.transform.GetComponent<ScrollSnapAlign>().RefreshPos(recordListView_SelfAddItem.LastCol2);
        
        for (int i = 0; i < recordViewPanelRecordDaYanRect.Count; i++)
        {
            recordViewPanelRecordDaYanRect[i].transform.GetComponent<ScrollSnapAlign>().RefreshPos(
                i == 0 ? recordDaYanView_SelfAddItems[i].LastCol2 : recordDaYanView_SelfAddItems[i].LastCol);
        }
    }

    public void RefreshOnNewRound()
    {
        recordListView_SelfAddItem.ResetRecords();
        for (int i = 0; i < recordDaYanView_SelfAddItems.Count; i++)
            recordDaYanView_SelfAddItems[i].ResetRecord();

        ResetPredict();

        recordViewPanelRecordViewList.ForEach(view => view.HideRecordView());//隐藏格子中的icon//牌路界面的 100局结果显示视图

        //recordViewPanelCountTexts.ForEach(text => text.text = "0");//牌路界面数据
    }


    // public void ShowZhuPanLu()
    // {//珠盘路用 records 来 显示,不用 BeadPlateRoad ,因为有小游戏判断,钻石显示判断
    //     var records = Model.Instance.records;
    //     for (int i = 0; i < records.Count; i++)
    //     {
    //         var record = records[i];
    //         ShowRecordView(record);
    //     }
    // }

    public void ShowDaLu()
    {
        //大路用 records 来 显示,不用 BigRoadList ,因为有小游戏判断,钻石显示判断
        recordListView_SelfAddItem.SetRecords(Model.Instance.records);
    }

    public void ShowDaYanLu()
    {
        recordDaYanView_SelfAddItems[0].SetRecord(Model.Instance.bigEyeRoadList);
    }

    public void ShowXiaoLu()
    {
        recordDaYanView_SelfAddItems[1].SetRecord(Model.Instance.smallRoadList);
    }

    public void ShowXiaoQiangLu()
    {
        recordDaYanView_SelfAddItems[2].SetRecord(Model.Instance.dragonRoadList);
    }

}
