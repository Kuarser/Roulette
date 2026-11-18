using Spine.Unity;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum CountDownState
{
    None,//空闲
    Betting,//正在押注,倒计时中
    Dealing,//正在开牌
    Settling,//正在结算
}

public class UIMainView : MonoBehaviour
{
    // UI对象预制体实例
    private SkeletonGraphic shaoLouSkeleton;
    private GameObject countDownSpineAnimation;//沙漏动画
    private GameObject countDownObj;//倒计时文本,方便显隐,可不要
    private GameObject countDownText2Obj;//倒计时提示文本,方便显隐,可不要
    private Text countDownText;//倒计时
    private Text countDownTipText2;//倒计时提示文字

    private Animator countDownAnimation;//派路界面沙漏动画,方便显隐
    private Text countDownText3;//路牌显示时倒计时,
    private Text countDownTipText4;//路牌显提示文字,

    // UI文本组件
    private Text gameText;//第几局,最大100
    private Text roundText;//第几轮,无上限
    private Text gameText2;//第几局,最大100，第二个
    private Text roundText2;//第几轮,无上限，第二个
    private Text seatText;//座位号
    private Text creditText;//账户信用卡,账号存款
    private Text winScoreText;//本局赢分
    private Vector3 winScoreTextScale;//本局赢分文本实例的大小
    private Text minBetText;//最小押注
    private Text limitText;//限红
    public Text jackpotText;//彩金
    private Text luckyTipsText;//幸运座位提示
    private Text luckyTipsText2;//幸运座位提示2

    private Text jackpotTitleText;//幸运座位提示
    private Text seatTitleText;//幸运座位提示

    //辅助
    private CountDownState curCountDownState = CountDownState.None;//当前倒计时状态
    private bool canWinScoreAnimation = true;//能否播放赢分缩放动画
    private int tempWinScore;//临时存储赢分数值
    private int tempWinScoreOld;//临时存储赢分数值
    private Coroutine showWinScoreTimersCoroutine;//显示赢分缩放的携程


    private void Awake()
    {
        InitView();
        InitData();
    }

    private void Start()
    {
        SetView();
        AddEventListener();
    }

    private void AddEventListener()
    {
        // 纯轮盘机台：没有原九门文本可切语言，跳过整段监听。
        if (!View.OriginalBaccaratUiPresent) return;
        //多语言切换
        //Debug.LogError($"AddEventListener on {gameObject.name} id:{GetInstanceID()}");
        EventCenter.Instance.AddEventListener(EventHandle.CHANGE_LANGUAGE, () =>
        {
            //Debug.LogError("CHANGE_LANGUAGE");//修复幸运座位中途切换-狂切有问题
            Language forntLan = IOCanvasModel.Instance.curlanguage == Language.cht?Language.en:Language.cht;
            IOCanvasModel.Instance.languageDic[forntLan].TryGetValue("MistakeTips", out string temp);
            if (luckyTipsText.text == temp || Model.Instance.curRecord.luckyPlayerId == 0)
            {
                luckyTipsText.text = Utils.GetLanguage("MistakeTips");
                luckyTipsText2.text = Utils.GetLanguage("MistakeTips");
            }
            else
            {
                string str = Utils.GetLanguage("Congratulations") + "! ";
                str += IOCanvasModel.Instance.curlanguage == Language.en ? $"Player {Model.Instance.curRecord.luckyPlayerId} " : $"{Model.Instance.curRecord.luckyPlayerId}";
                str += $"{Utils.GetLanguage("WinLuckySeat")}!";
                luckyTipsText.text = str;
                luckyTipsText2.text = str;
            }

            jackpotTitleText.text = Utils.GetLanguage("JackpotText");
            seatTitleText.text = Utils.GetLanguage("SeatNumberText");

            switch (curCountDownState)
            {
                case CountDownState.None:
                    break;
                case CountDownState.Betting:
                    break;
                case CountDownState.Dealing:
                    countDownTipText2.text = Utils.GetLanguage("Dealing");
                    break;
                case CountDownState.Settling:
                    countDownTipText2.text = Utils.GetLanguage("Settlement");
                    break;
                default:
                    break;
            }
        });
    }

    private void InitView()
    {
        // 纯轮盘机台：UICanvas 子物体已全部清除，只保留 PopCanvas 上的幸运座位提示。
        if (!View.OriginalBaccaratUiPresent)
        {
            var popCanvasOnly = GameObject.Find("PopCanvas").transform;
            if (popCanvasOnly != null)
            {
                var luckyTips2 = popCanvasOnly.Find("LuckyTips/Text");
                if (luckyTips2 != null) luckyTipsText2 = luckyTips2.GetComponent<Text>();
            }
            return;
        }

        var UICanvas = GameObject.Find("UICanvas").transform;
        countDownSpineAnimation = UICanvas.Find("Top/CountDown/shaoloudonghua").gameObject;
        shaoLouSkeleton = countDownSpineAnimation.GetComponent<SkeletonGraphic>();
        countDownObj = UICanvas.Find("Top/CountDown/CountDownText").gameObject;
        countDownText2Obj = UICanvas.Find("Top/CountDown/CountDownTipText").gameObject;
        countDownText = countDownObj.GetComponent<Text>();//倒计时文本
        countDownText.text = "";//倒计时
        countDownTipText2 = countDownText2Obj.GetComponent<Text>();//倒计时提示文本
        countDownTipText2.text = "";//倒计时提示文字

        seatText = UICanvas.Find("Top/Seat/Text").GetComponent<Text>();//座位号文本
        //seatText.text = "";
        seatText.text = SBoxModel.Instance.macId.ToString();
        seatTitleText = UICanvas.Find("Top/Seat/title").GetComponent<Text>();//座位号标题文本,用于多语言翻译

        jackpotText = UICanvas.Find("Center/Jackpot/Text").GetComponent<Text>();//本地彩金文本
        jackpotText.text = "";//彩金
        jackpotTitleText = UICanvas.Find("Center/Jackpot/title").GetComponent<Text>();//本地彩金标题文本,用于多语言翻译
        luckyTipsText = UICanvas.Find("Bottom/Center/LuckyTips/Text").GetComponent<Text>();//幸运座位提示文本,或者桌面提示文本,用于多语言翻译

        gameText = UICanvas.Find("Center/AllCardDeck/GameInfo/game/gameText").GetComponent<Text>();//第几局文本
        gameText.text = "";//第几局,最大100
        roundText = UICanvas.Find("Center/AllCardDeck/GameInfo/round/roundText").GetComponent<Text>();//第几轮文本
        roundText.text = "";//第几轮,无上限

        gameText2 = UICanvas.Find("Top/Right/BG/GameInfo2/game/gameText").GetComponent<Text>();//第几局文本
        gameText2.text = "";//第几局,最大100
        roundText2 = UICanvas.Find("Top/Right/BG/GameInfo2/round/roundText").GetComponent<Text>();//第几轮文本
        roundText2.text = "";//第几轮,无上限

        creditText = UICanvas.Find("Bottom/Right/playerInfo/credit").GetComponent<Text>();//总分文本//Top/Right/BG/playerInfo/credit
        creditText.text = "";//账户信用卡,账号存款
        winScoreText = UICanvas.Find("Bottom/Right/playerInfo/winScore").GetComponent<Text>();//赢分文本//Top/Right/BG/playerInfo/winScore
        winScoreText.text = "";//本局赢分
        winScoreTextScale = winScoreText.transform.localScale;

        minBetText = UICanvas.Find("Top/CountDown/MinBetText").GetComponent<Text>();//最小押注文本//Top/Right/BG/betInfo/MinBet
        minBetText.text = "";//最小押注
        limitText = UICanvas.Find("Top/CountDown/LimitText").GetComponent<Text>();//限红
        limitText.text = "";//限红


        var popCanvas = GameObject.Find("PopCanvas").transform;
        countDownText3 = UICanvas.Find("Top/Right/RecordView/BG/RecordViewCountDown/CountDownText").GetComponent<Text>();//派路界面的倒计时
        countDownText3.text = "";
        countDownAnimation = UICanvas.Find("Top/Right/RecordView/BG/RecordViewCountDown/hourglass").GetComponent<Animator>();//派路界面的沙漏动画
        countDownTipText4 = UICanvas.Find("Top/Right/RecordView/BG/RecordViewCountDown/CountDownTipText").GetComponent<Text>();//派路界面的提示文本
        countDownTipText4.text = "";
        luckyTipsText2 = popCanvas.Find("LuckyTips/Text").GetComponent<Text>();//幸运座位提示文本2,或者桌面提示文本,用于多语言翻译

    }

    private void InitData()
    {
    }

    private void SetView()//初始化设置
    {
        // 纯轮盘机台：只有 PopCanvas 上的 luckyTipsText2 存在。
        if (!View.OriginalBaccaratUiPresent)
        {
            if (luckyTipsText2 != null)
                luckyTipsText2.text = Utils.GetLanguage("MistakeTips");
            return;
        }

        //UpdateView();
        ChangeCountDownState(CountDownState.None);

        luckyTipsText.text = Utils.GetLanguage("MistakeTips");
        luckyTipsText2.text = Utils.GetLanguage("MistakeTips");
        jackpotTitleText.text = Utils.GetLanguage("JackpotText");
        seatTitleText.text = Utils.GetLanguage("SeatNumberText");
    }

    public void RefreshView()//全部重新加载,重置刷新界面
    {

    }

    public void UpdateView()//更新部分数据
    {
        if (!View.OriginalBaccaratUiPresent) return;
        minBetText.text = IOCanvasModel.Instance.CfgData.MinBet.ToString();//最小押注
        limitText.text = IOCanvasModel.Instance.tempCfgData.LimitBetsWins.ToString();//限红
        winScoreText.text = PlayerMgr.Instance.localPlayer.outState.wins.ToString();//需要PlayerMgr已经初始化了
        if (!Model.Instance.fadeCredit)//非暂停期间显示实际分
        {
            creditText.text = PlayerMgr.Instance.localPlayer.outState.credit.ToString();
        }
        else//暂停期间显示假分数 = 实际分 - 抢庄盈利(可能为负),投币/上分/下分改变credit时差值实时变化,抢庄盈利加分两者同增差值不变
        {
            int bankerProfit = Model.Instance.curRecord != null && 
            Model.Instance.curRecord.bankerID == SBoxModel.Instance.macId ? Model.Instance.curRecord.bankerProfit : 0;
            creditText.text = (PlayerMgr.Instance.localPlayer.outState.credit - bankerProfit).ToString();
            //Debug.LogError($"bankerID{Model.Instance.curRecord.bankerID}  bankerProfit:{Model.Instance.curRecord.bankerProfit}");
        }
        //Debug.LogError($"creditText:{PlayerMgr.Instance.localPlayer.outState.credit}+{PlayerMgr.Instance.localPlayer.outState.wins}");
    }

    public void UpdateWinScoreText()
    {
        if (!View.OriginalBaccaratUiPresent) return;
        winScoreText.text = PlayerMgr.Instance.localPlayer.outState.wins.ToString();
        Debug.LogError($"winScoreText:{PlayerMgr.Instance.localPlayer.outState.wins}");
    }

    public void UpdateCountDownText()
    {
        if (!View.OriginalBaccaratUiPresent) return;
        countDownText3.text = Model.Instance.curCountDown.ToString();
        countDownText.text = Model.Instance.curCountDown.ToString();
    }

    public void UpdateRoundAndGameText()
    {
        if (!View.OriginalBaccaratUiPresent) return;
        roundText.text = /*"R:" +*/ (Model.Instance.curRound).ToString();//轮数
        gameText.text = /*"C:" + */(Model.Instance.curGame).ToString();//局数
        roundText2.text = /*"R:" +*/ (Model.Instance.curRound).ToString();//轮数
        gameText2.text = /*"C:" + */(Model.Instance.curGame).ToString();//局数
    }

    public void UpdateJackpotText(int jackpot)
    {
        if (!View.OriginalBaccaratUiPresent) return;
        jackpotText.text = jackpot.ToString();
    }

    public void ChangeCountDownState(CountDownState countDownState)//更改倒计时状态
    {
        if (!View.OriginalBaccaratUiPresent) return;
        curCountDownState = countDownState;

        switch (curCountDownState)
        {
            case CountDownState.None:
                //countDownText2Obj.SetActive(false);
                countDownTipText2.gameObject.SetActive(false);
                countDownTipText4.gameObject.SetActive(false);

                countDownSpineAnimation.SetActive(true);
                shaoLouSkeleton.AnimationState.ClearTracks();
                shaoLouSkeleton.startingAnimation = null;
                shaoLouSkeleton.timeScale = 0;
                countDownAnimation.gameObject.SetActive(true);
                countDownAnimation.enabled = false;
                countDownAnimation.speed = 0;//播放派路界面沙漏动画
                break;
            case CountDownState.Betting:
                //countDownText2Obj.SetActive(false);
                countDownTipText2.gameObject.SetActive(false);
                countDownTipText4.gameObject.SetActive(false);

                //if (!countDownObj.activeInHierarchy)
                //    countDownObj.SetActive(true);
                if (!countDownText.gameObject.activeInHierarchy)
                    countDownText.gameObject.SetActive(true);
                if (!countDownText3.gameObject.activeInHierarchy)
                    countDownText3.gameObject.SetActive(true);

                countDownSpineAnimation.SetActive(true);
                shaoLouSkeleton.startingAnimation = "animation";
                shaoLouSkeleton.timeScale = 1;
                shaoLouSkeleton.AnimationState.SetAnimation(0, "animation", true);
                countDownAnimation.gameObject.SetActive(true);
                countDownAnimation.enabled = true;
                countDownAnimation.speed = 1;//播放派路界面沙漏动画
                if(countDownAnimation.gameObject.activeSelf)
                    countDownAnimation.Play("hourglassAnimation", 0, 0);//播放派路界面沙漏动画

                UpdateCountDownText();//正在押注,倒计时中
                break;
            case CountDownState.Dealing:
                //countDownObj.SetActive(false);
                countDownText.gameObject.SetActive(false);
                countDownText3.gameObject.SetActive(false);

                countDownSpineAnimation.SetActive(false);
                countDownAnimation.gameObject.SetActive(false);

                //if (!countDownText2Obj.activeInHierarchy)
                //    countDownText2Obj.SetActive(true);
                if (!countDownTipText2.gameObject.activeInHierarchy)
                    countDownTipText2.gameObject.SetActive(true);
                if (!countDownTipText4.gameObject.activeInHierarchy)
                    countDownTipText4.gameObject.SetActive(true);

                countDownTipText2.text = Utils.GetLanguage("Dealing");//"正在开牌"
                countDownTipText4.text = Utils.GetLanguage("Dealing");//"正在开牌"
                break;
            case CountDownState.Settling:
                //countDownObj.SetActive(false);
                countDownText.gameObject.SetActive(false);
                countDownText3.gameObject.SetActive(false);

                countDownSpineAnimation.SetActive(false);
                countDownAnimation.gameObject.SetActive(false);

                //if (!countDownText2Obj.activeInHierarchy)
                //    countDownText2Obj.SetActive(true);
                if (!countDownTipText2.gameObject.activeInHierarchy)
                    countDownTipText2.gameObject.SetActive(true);
                if (!countDownTipText4.gameObject.activeInHierarchy)
                    countDownTipText4.gameObject.SetActive(true);

                countDownTipText2.text = Utils.GetLanguage("Settlement");//"正在结算"
                countDownTipText4.text = Utils.GetLanguage("Settlement");//"正在结算"
                break;
            default:
                break;
        }

    }

    public void SetLuckyTips(string str, float timer)
    {
        // 纯轮盘机台：只剩 PopCanvas 上的 luckyTipsText2。
        if (!View.OriginalBaccaratUiPresent)
        {
            if (luckyTipsText2 == null) return;
            Color onlyColor = luckyTipsText2.color;
            luckyTipsText2.text = str;
            luckyTipsText2.color = Color.white;
            StartCoroutine(DelayHideLuckyTips(timer, () =>
            {
                if (luckyTipsText2 != null)
                {
                    luckyTipsText2.text = Utils.GetLanguage("MistakeTips");
                    luckyTipsText2.color = onlyColor;
                }
            }));
            return;
        }

        Color tempColor = luckyTipsText.color;
        luckyTipsText.text = str;
        luckyTipsText2.text = str;
        luckyTipsText.color = Color.white;
        luckyTipsText2.color = Color.white;
        StartCoroutine(DelayHideLuckyTips(timer, () =>
        {
            luckyTipsText.text = Utils.GetLanguage("MistakeTips");
            luckyTipsText2.text = Utils.GetLanguage("MistakeTips");
            luckyTipsText.color = tempColor;
            luckyTipsText2.color = tempColor;
        }));
    }

    private IEnumerator DelayHideLuckyTips(float timer, System.Action callback = null)
    {
        yield return new WaitForSeconds(timer);
        callback?.Invoke();
    }

    public void SetWinScoreTextScale()
    {
        if (!View.OriginalBaccaratUiPresent) return;
        winScoreText.transform.localScale = winScoreTextScale;
    }

    public void ShowWinScore()
    {
        if (!View.OriginalBaccaratUiPresent) return;
        tempWinScoreOld = tempWinScore;//获取上一次的赢分
        tempWinScore = PlayerMgr.Instance.localPlayer.outState.wins;//获取本次赢分

        if (!canWinScoreAnimation && tempWinScore != tempWinScoreOld)//停止播放缩放动画
        {
            //if (showWinScoreTimersCoroutine != null)
            //{
            //    StopCoroutine(showWinScoreTimersCoroutine);//停止播放小游戏动画,但是播放赢分的时候不是已经过了播放小游戏的动画的时候了吗?
            //    showTreasureTimersCoroutine = null;
            //}

            //winScoreText.transform.localScale = winScoreTextScale;
            SetWinScoreTextScale();

            canWinScoreAnimation = true;
            tempWinScore = 0;
            tempWinScoreOld = 0;
        }
        if (tempWinScore < tempWinScoreOld)//赢分开始递减,给总分累加
        {
            //HideWinScore();//隐藏赢分框
            //winScoreText.text = PlayerMgr.Instance.localPlayer.outState.wins.ToString();
            return;
        }

        if (canWinScoreAnimation && tempWinScore > 0 && tempWinScore == tempWinScoreOld)//一种
        {
            //播放缩放动画,三个条件分别是: 能播放一次 和 押注中奖了有赢分 和 赢分还没有递减好给总分累加
            canWinScoreAnimation = false;

            //二选一
            //winScoreText.text = PlayerMgr.Instance.localPlayer.outState.wins.ToString();
            UpdateWinScoreText();

            showWinScoreTimersCoroutine = StartCoroutine(ScaleLoop());//赢分文本缩放动画
            //ShowWinScoreByDialog();//赢分框流光动画//另一种
        }
    }

    private IEnumerator ScaleLoop()
    {
        //下面是,赢分文本缩放动画逻辑,中奖了,赢分缩放一下,让玩家的注意力集中到赢分上
        Vector3 minScale = new Vector3(0.8f, 0.8f, 0.8f); // 最小缩放
        Vector3 maxScale = new Vector3(1.2f, 1.2f, 1.2f); // 最大缩放
        float duration = 0.6f; // 缩放所需时间
        float time = 3; //缩放次数
        while (true)
        {
            // 从当前缩放到最大缩放
            yield return StartCoroutine(ScaleTo(winScoreText.gameObject, maxScale, duration));
            // 从当前缩放到最小缩放
            yield return StartCoroutine(ScaleTo(winScoreText.gameObject, minScale, duration));
            time--;
            if (time <= 0)
            {
                yield return StartCoroutine(ScaleTo(winScoreText.gameObject, winScoreTextScale, duration / 2));
                winScoreText.transform.localScale = winScoreTextScale;
                //yield return StartCoroutine(TwoNumTo(creditText, winScoreText, PlayerMgr.Instance.localPlayer.outState.wins, 0, 2f));//手动实现赢分移动动画
                yield break;
            }
        }
    }

    private IEnumerator ScaleTo(GameObject obj, Vector3 targetScale, float time)
    {
        Vector3 startScale = obj.transform.localScale;
        float elapsedTime = 0f;

        while (elapsedTime < time)
        {
            obj.transform.localScale = Vector3.Lerp(startScale, targetScale, elapsedTime / time);
            elapsedTime += UnityEngine.Time.deltaTime;
            yield return null; // 等待下一帧
        }

        obj.transform.localScale = targetScale; // 确保最终值精确
    }

    /// <summary>
    /// 数字累加,或累减
    /// </summary>
    /// <param name="startScale">开始分数</param>
    /// <param name="targetScale">结束分数</param>
    /// <param name="time">分数变化所需时间</param>
    /// <returns></returns>
    private IEnumerator NumTo(Text t, float startScale, float targetScale, float time)
    {
        int result;
        float elapsedTime = 0f;

        while (elapsedTime < time)
        {
            result = (int)Mathf.Lerp(startScale, targetScale, elapsedTime / time);
            t.text = result.ToString();
            elapsedTime += UnityEngine.Time.deltaTime;
            yield return null; // 等待下一帧
        }

        result = (int)targetScale; // 确保最终值精确
        t.text = result.ToString();
    }

    /// <summary>
    /// 两个数字,一个累加,一个累减
    /// </summary>
    /// <param name="startScale">开始分数</param>
    /// <param name="targetScale">结束分数</param>
    /// <param name="time">分数变化所需时间</param>
    /// <returns></returns>
    private IEnumerator TwoNumTo(Text add, Text dec, float startScale, float targetScale, float time)
    {
        int result;
        float elapsedTime = 0f;
        int addNum = int.Parse(add.text);
        int temp;

        while (elapsedTime < time)
        {
            result = (int)Mathf.Lerp(startScale, targetScale, elapsedTime / time);
            dec.text = result.ToString();

            temp = addNum + (int)startScale - result;
            add.text = temp.ToString();

            elapsedTime += UnityEngine.Time.deltaTime;
            yield return null; // 等待下一帧
        }

        result = (int)targetScale; // 确保最终值精确
        dec.text = result.ToString();

        temp = addNum + (int)startScale - result;
        add.text = temp.ToString();
    }

}
