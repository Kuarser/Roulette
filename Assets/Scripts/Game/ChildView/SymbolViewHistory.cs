using Newtonsoft.Json.Linq;
using SBoxApi;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SymbolViewHistory : MonoBehaviour
{
    private int lastBet;//上个bet
    private int nowBet; //当前bet
    private Text betText;
    private Text allBetText;
    private GameObject lightIcon;
    public List<Sprite> sprites = new List<Sprite>();//硬币图片集合

    [SerializeField]
    private float allBetTextPosY = -45; //allbet 文本y轴位置初始值,0个时不显示,1个时在y轴-45显示,每个间隔7.5,
    private float intervalY = 7.5f;
    private RectTransform allBetTextRect;


    private int[] coinsNum = new int[6];


    [HideInInspector]
    public int index = -1;//卡牌序号,卡牌标识//通过外界初始化脚本时赋值
    [HideInInspector]
    public bool isBettingNow;//是押注时间//用于荷官发牌动画判断

    private GameObject parentCoin;//金币父物体
    private List<GameObject> childrenCoins = new List<GameObject>();//金币柱子
    private List<Image> childrenCoinImages = new List<Image>();//金币图片柱子
    private int childreCoinsIndex = 0;//当前金币柱子索引
    private int[] coinsTypeNum = new int[6];//每种硬币类型数量

    private bool isInit = false;
    private void Awake()
    {
        Init();
    }
    int count = 0;
    private void Init()
    {
        if (isInit) return;
        isInit = true;
        betText = this.transform.Find("bet").GetComponent<Text>();
        allBetText = this.transform.Find("allBet").GetComponent<Text>();
        allBetTextRect = allBetText.transform.GetComponent<RectTransform>();
        lightIcon = this.transform.Find("light").gameObject;
        lightIcon.SetActive(false);//默认隐藏

        parentCoin = this.transform.Find("coinsParent").gameObject;
        for (int i = 0; i < parentCoin.transform.childCount; i++)
        {
            //Debug.Log($"第{count}次");
            count++;
            var trans = parentCoin.transform.GetChild(i);
            childrenCoins.Add(trans.gameObject);
            childrenCoinImages.Add(trans.GetComponent<Image>());
            childrenCoins[i].SetActive(false);//隐藏金币柱子
        }
    }
    public void SetBet(int bet)
    {
        if (bet == 0)
        {
            this.betText.text = "";
        }
        else
        {
            this.betText.text = bet.ToString();
        }
    }
    public void SetSymbolView(int bet)
    {
        if (bet == lastBet)
        {
            isBettingNow = false;
            return;
        }
        lastBet = bet;
        isBettingNow = true;

        SetAllBet(bet);
        return;
    }
    public void HideSymbolView()
    {
        if (this.gameObject.activeInHierarchy)
        {
            this.gameObject.SetActive(false);
        }
    }
    public void ShowLightIcon()
    {
        lightIcon.SetActive(true);
    }
    public void HideLightIcon()
    {
        lightIcon.SetActive(false);
    }
    public void UpdateBetText(string betText)
    {
        this.betText.text = betText;
    }
    public void UpdateAllBetText(string allBetText)
    {
        this.allBetText.text = allBetText;
    }

    private static float betSoundInterval = 0;
    public void SetAllBet(int value)
    {
        if (value == 0)
        {
            allBetText.text = "";
        }
        else
        {
            allBetText.text = value.ToString();
        }
        int count = 0;
        //childrenCoins.ForEach((coin) => coin.SetActive(false));
        if (value > 0)
            count += 1;
        if (value > 100)
        {
            if (value < 1000)
                count += (value - 100) / 200;
            else
                count += 5;
        }
        if (value > 1000)
        {
            if (value < 3000)
                count += (value - 1000) / 400;
            else
                count += 5;
        }
        if (value > 3000)
        {
            if (value < 6000)
                count += (value - 3000) / 600;
            else
                count += 5;
        }
        if (value > 6000)
        {
            if (value < 12000)
                count += (value - 6000) / 800;
            else
                count += 5;
        }
        count = count > 10 ? 10 : count;
        int tempValue = value;
        for (int i = 0; i < childrenCoins.Count; i++)
        {
            childrenCoins[i].SetActive(i < count);
            int index = GetSpriteIndex(tempValue);//应该显示的硬币图片索引
            childrenCoinImages[i].sprite = sprites[index];
            tempValue -= Model.Instance.betCoinList[index];
        }
        Vector2 tempPos = betText.transform.localPosition;
        float offset = -44.7f;
        betText.transform.localPosition = new Vector2(tempPos.x,offset+5f*(count-1));
        //调整allbet文本位置,根据金币柱子数量上下浮动调整(其实要调的是bet的位置)
        // Vector3 tempPos2 = allBetTextRect.localPosition;
        // int tempCount = count < childrenCoins.Count ? count : childrenCoins.Count;
        // tempPos2.y = allBetTextPosY + (tempCount - 1) * intervalY;
        // allBetTextRect.localPosition = tempPos2;
    }
    private int GetSpriteIndex(int bet)
    {
        int index = 0;
        for (int i = Model.Instance.betCoinList.Count - 1; i >= 0; i--)
        {
            if (bet >= Model.Instance.betCoinList[i])//压分大于等于当前硬币面值
            {
                index = i;
                break;
            }
        }
        return index;
    }
}
