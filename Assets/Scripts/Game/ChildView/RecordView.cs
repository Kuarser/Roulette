using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

internal class RecordView : MonoBehaviour
{
    private Image recordIcon;
    public List<Sprite> recordSprites = new List<Sprite>();

    private List<GameObject> flags = new List<GameObject>();

    private List<GameObject> fourInOne = new List<GameObject>();//4in1 子物体,顺序:小老虎 大老虎 幸运7 幸运8
    private List<GameObject> pairObjs = new List<GameObject>();//pair 子物体,顺序:庄对 闲对


    private Text valueText;//新增加
    private List<Font> textFonts = new List<Font>();
    private Image typeTextImg;

    private bool isInit = false;

    private void Awake()
    {
        Init();
    }
    void Init()
    {
        if (isInit) return;
        isInit = true;
        if (recordIcon == null)
            recordIcon = transform.Find("recordIcon").GetComponent<Image>();

        if (flags.Count <= 0)//其他icon
        {
            var flagParent = transform.Find("flags");//旗帜,彩金,明牌,全中,标识图片
            for (int i = 0; i < flagParent.childCount; i++)
                flags.Add(flagParent.transform.GetChild(i).gameObject);
        }

        if (fourInOne.Count <= 0)//4in1 标识,与 flags 同路径
        {
            var fourInOneParent = transform.Find("4in1");
            for (int i = 0; i < fourInOneParent.childCount; i++)
                fourInOne.Add(fourInOneParent.GetChild(i).gameObject);
        }
        if (pairObjs.Count <= 0)//pair 标识,与 flags 同路径
        {
            var pairParent = transform.Find("pair");
            for (int i = 0; i < pairParent.childCount; i++)
                pairObjs.Add(pairParent.GetChild(i).gameObject);
        }

        //下面全是新增加的
        //typeTextImg = transform.Find("type").GetComponent<Image>();//新增加

        //textFonts = new List<Font>();
        //for (int i = 0; i < 3; i++)
        //    textFonts.Add(ResMgr.Instance.Load<Font>($"fonts/pokerRecord{i}"));

        //valueText = transform.Find("value").GetComponent<Text>();
    }


    private void SetRecordText(RecordStruct record)//新增
    {
        string typeImgStr;
        PokerData pokerData = Model.Instance.GetPokerData(record.value[0]);

        if (pokerData.type == 5)
            typeImgStr = pokerData.value == 14 ? "type5" : "type6";
        else
            typeImgStr = $"type{pokerData.type}";
        if (record.luckyId == 8)
        {
            valueText.font = textFonts[2];
            if (pokerData.type == 5)
                typeImgStr = pokerData.value == 14 ? "type15" : "type16";
            else
                typeImgStr = $"type1{pokerData.type}";
            typeTextImg.sprite = Resources.Load<Sprite>($"pokerTypes/{typeImgStr}");
            typeTextImg.SetNativeSize();
        }
        else
        {
            valueText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (pokerData.type == 2
                || pokerData.type == 4)
                valueText.font = textFonts[0];
            else
                valueText.font = textFonts[1];
            typeTextImg.sprite = Resources.Load<Sprite>($"pokerTypes/{typeImgStr}");
            typeTextImg.SetNativeSize();
        }

        int value = pokerData.value;
        if (pokerData.type < 5)
        {
            if (value == 1)
                valueText.text = "A";
            else if (value > 9)
            {
                switch (value)
                {
                    case 10:
                        valueText.text = "B";
                        break;
                    case 11:
                        valueText.text = "J";
                        break;
                    case 12:
                        valueText.text = "Q";
                        break;
                    case 13:
                        valueText.text = "K";
                        break;
                }
            }
            else
                valueText.text = value.ToString();
        }
        else
            valueText.text = "";
    }

    public void SetRecordView(RecordStruct record)//显示
    {
        Init();
     
        int result = record.result;
        //recordIcon.sprite = Resources.Load<Sprite>($"record/record{result - 1}");//资源加载icon//用不到
        if(result <= 0)return;
        recordIcon.sprite = recordSprites[result - 1];
        //recordIcon.SetNativeSize();//原图标大小显示//资源比框大时别用,

        //if (!recordIcon.isActiveAndEnabled)
        recordIcon.gameObject.SetActive(true);
        this.gameObject.SetActive(true);

        fourInOne.ForEach(x => x.SetActive(false));//先隐藏全部 4in1 标识
        pairObjs.ForEach(x => x.SetActive(false));//先隐藏全部 pair 标识

        if (record.spresult != null)
        {
            for (int i = 0; i < fourInOne.Count && i < record.spresult.Length; i++)
                if (record.spresult[i] == 1) fourInOne[i].SetActive(true);
        }
        if (record.pair != null)
        {
            for (int i = 0; i < pairObjs.Count && i < record.pair.Length; i++)
                if (record.pair[i] == 1) pairObjs[i].SetActive(true);
        }

        flags.ForEach(x => x.SetActive(false));//先隐藏所有标识图片
        if (record.luckyId == 0) return;

        switch (record.luckyId)
        {
            case 4:
            case 5:
            case 7:
                flags[0].SetActive(true);
                break;
            case 6:
                flags[1].SetActive(true);
                break;
            case 8:
                flags[2].SetActive(true);
                break;
        }
    }
    public void HideRecordView()//隐藏
    {
        Init();
        //if (recordIcon.isActiveAndEnabled)
        recordIcon.gameObject.SetActive(false);
        flags.ForEach(x => x.SetActive(false));//先隐藏所有标识图片
        fourInOne.ForEach(x => x.SetActive(false));//先隐藏全部 4in1 标识
        pairObjs.ForEach(x => x.SetActive(false));//先隐藏全部 pair 标识
    }
}