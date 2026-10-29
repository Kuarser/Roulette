using Newtonsoft.Json.Linq;
using SBoxApi;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SymbolView : MonoBehaviour
{
    private int lastBet;//上个bet
    private int nowBet; //当前bet
    private Text betText;
    private Text allBetText;
    private GameObject lightIcon;

    public MyTriggerDetection trigger;
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

    int count = 0;


    private void Start()
    {
        Init();
    }
    private void Init()
    {
        if (isInit) return;
        isInit = true;
        betText = this.transform.Find("bet").GetComponent<Text>();
        allBetText = this.transform.Find("allBet").GetComponent<Text>();
        allBetTextRect = allBetText.transform.GetComponent<RectTransform>();
        lightIcon = this.transform.Find("light").gameObject;
        lightIcon.SetActive(false);//默认隐藏


        if (trigger == null)
        {
            trigger = this.transform.GetComponentInChildren<MyTriggerDetection>();
        }

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

        //Debug.LogWarning($"下标:{childreCoinsIndex},长度:{childrenCoins.Count},长度:{childrenCoinImages.Count},长度:{sprites.Count}");
        AddEvent();
    }
    private void AddEvent()
    {
        trigger.pointerEnter = OnPointerEnter;
        trigger.pointerExit = OnPointereExit;
    }

    public void InitByUse()
    {
        Init();
    }


    private void OnPointerEnter()
    {
        if (Model.Instance.waitSelectCard) return;
        if (FSMSystem.Instance.CurrentStateID != StateID.Bet) return;
        int inState = 0;
        switch (index)//鱼种类,4个卡牌就隐藏最后一个case,5个就不隐藏 Baccarat※增加九个押分区
        {
            case 0:
                inState = (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_A;
                break;
            case 1:
                inState = (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_B;
                break;
            case 2:
                inState = (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_C;
                break;
            case 3:
                inState = (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_D;
                break;
            case 4:
                inState = (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_E;
                break;
            case 5:
                inState = (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_F;
                break;
            case 6:
                inState = (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_G;
                break;
            case 7:
                inState = (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_H;
                break;
            case 8:
                inState = (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_I;
                break;
            default:
                Debug.LogError("超出了卡片 种类选择!");
                break;
        }

        PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState(inState, true);//主机用
                                                                               //Model.Instance.ChangeInState(inState, true);//分机用

        if (PlayerMgr.Instance.localPlayer == null || PlayerMgr.Instance.localPlayer.outState == null || IOCanvasModel.Instance.CfgData == null || IOCanvasModel.Instance.betDataList == null || Model.Instance.betCoinList == null) return;
        if (PlayerMgr.Instance.localPlayer.outState.credit <= 0
            || (PlayerMgr.Instance.localPlayer.outState.credit <= IOCanvasModel.Instance.CfgData.MinBet && PlayerMgr.Instance.localPlayer.outState.credit <= Model.Instance.betCoinList[PlayerMgr.Instance.localPlayer.betUnitIdx]))
        {
            PopTips.Instance.ShowSystemTips("TotalScoreNotEnough",sendToClient:false, isShowInLittle:false);//软件触摸压分时积分不足,提示
        }
    }

    private void OnPointereExit()
    {
        int inState = (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_A;
        switch (index)//鱼种类//只有4张卡片选择,就隐藏最后一个,5个就不隐藏
        {
            case 0:
                inState = (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_A;
                break;
            case 1:
                inState = (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_B;
                break;
            case 2:
                inState = (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_C;
                break;
            case 3:
                inState = (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_D;
                break;
            case 4:
                inState = (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_E;
                break;
            case 5:
                inState = (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_F;
                break;
            case 6:
                inState = (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_G;
                break;
            case 7:
                inState = (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_H;
                break;
            case 8:
                inState = (int)SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_I;
                break;
            default:
                Debug.LogError("超出了卡片 种类选择!");
                break;
        }
        PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState(inState, false);//主机用
        //Model.Instance.ChangeInState(inState, false);//分机用
        isBettingNow = false;
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

        #region 旧代码
        //数值方面:
        //先清空
        for (int i = 0; i < coinsNum.Length; i++)
        {
            coinsTypeNum[i] = 0;
        }
        //每种硬币类型数量计算
        coinsTypeNum[5] = bet / 500;
        bet %= 500;
        coinsTypeNum[4] = bet / 100;
        bet %= 100;
        coinsTypeNum[3] = bet / 25;
        bet %= 25;
        coinsTypeNum[2] = bet / 10;
        bet %= 10;
        coinsTypeNum[1] = bet / 5;
        bet %= 5;
        coinsTypeNum[0] = bet / 1;

        //显示方面:
        //清空金币柱子
        for (int i = 0; i < childrenCoins.Count; i++)
        {
            if (childrenCoins[i].gameObject.activeSelf == false) break;//已经隐藏了,说明后面的也都是隐藏的,后面就不用再隐藏了
            childrenCoins[i].SetActive(false);//先把硬币全部隐藏
        }
        //根据硬币类型数量,显示对应的金币
        childreCoinsIndex = 0;
        for (int j = coinsTypeNum.Length - 1; j >= 0; j--)
        {
            for (int k = 0; k < coinsTypeNum[j]; k++)
            {
                if (childreCoinsIndex >= childrenCoins.Count)
                {
                    Debug.LogError("金币数量不够!");
                    return;
                }
                childrenCoins[childreCoinsIndex].SetActive(true);
                Debug.LogWarning($"下标:{childreCoinsIndex},长度:{childrenCoins.Count},长度:{childrenCoinImages.Count},长度:{sprites.Count}");
                Debug.LogWarning($"{childrenCoinImages[childreCoinsIndex].sprite == null}图片为空?");
                Debug.Log($"{sprites[j] == null}图片为空?");
                childrenCoinImages[childreCoinsIndex].sprite = sprites[j];
                //childrenCoins[childreCoinsIndex].transform.Find("Num").GetComponent<Text>().text = "$" + coinsBet[j];
                childreCoinsIndex++;
            }
        }
        #endregion
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
        InitByUse();
        lightIcon.SetActive(true);
    }
    public void HideLightIcon()
    {
        InitByUse();
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
    public void SetBet(int bet)
    {
        //Debug.LogError($"押注更新：{bet}");
        if (bet == 0)
        {
            this.betText.text = "";
        }
        else
        {
            if (Time.time - betSoundInterval >= 0.2f && (this.betText.text.Equals("") || bet > int.Parse(this.betText.text)))
            {
                AudioManager.Instance.PlaySoundEff("betSound");
                betSoundInterval = Time.time;
            }
            this.betText.text = bet.ToString();
        }
    }
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
        //Debug.LogError($"硬币数据：{count}");
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
