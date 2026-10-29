using SBoxApi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

//幸运游戏 卡片选择显示
public class CardView : MonoBehaviour
{
    [HideInInspector]
    public int index;
    private Transform cardTrans;
    private GameObject cardBtn;
    private GameObject contentObj;//正面内容父节点
    private List<GameObject> contentObjs = new List<GameObject>();//正面内容集合
    private List<GameObject> multipleObj = new List<GameObject>();//正面内容加倍集合

    private int indexInMultiple;//加倍索引,
    private Color defaultColor;
    private Color pressColor;
    Vector3 oriPos;//卡片起始位置
    private bool isInit;//是否已经初始化过
    public TouchTriger touchTriger;

    SpinePlayer _spinePlayer;
    SpinePlayer spinePlayer
    {
        get
        {
            if (this._spinePlayer == null)
                this._spinePlayer = this.GetComponent<SpinePlayer>();
            return this._spinePlayer;
        }
    }

    void Start()
    {
        oriPos = new Vector3(1200, 1000, 0);
        defaultColor = new Color(130f / 255, 127f / 255, 127f / 255, 128f / 255);
        pressColor = new Color(80f / 255, 80f / 255, 80f / 255, 128f / 255);
        InitView();
        InitViewData();
    }

    private void InitView()
    {
        if (isInit) return;
        cardTrans = transform.Find("card");
        touchTriger = cardTrans.GetComponent<TouchTriger>();
        contentObj = transform.Find("card/content").gameObject;
        contentObjs.Add(transform.Find("card/content/mutiple").gameObject);
        contentObjs.Add(transform.Find("card/content/magni").gameObject);
        contentObjs.Add(transform.Find("card/content/allwin").gameObject);

        var times = transform.Find("card/content/times");
        for (int i = 0; i < times.childCount; i++)
            multipleObj.Add(times.GetChild(i).gameObject);

        contentObj.SetActive(false);
        isInit = true;
    }

    private void InitViewData()
    {
    }

    public void ShowCanClickAnimation()//显示能点击的提示动画
    {

    }

    public void SetCardBtn(bool isShow)//控制卡牌按钮显示隐藏
    {
        cardBtn.SetActive(isShow);
        if (!isShow)
            spinePlayer.Show();
    }

    //public void ShowAni2()
    //{
    //    spinePlayer.ShowAnimation();//卡牌翻动 的 spine动画
    //}

    public void ShowAni()
    {
        contentObj.SetActive(true);

        spinePlayer.Show();//卡牌翻动 的 spine动画
                           //AudioManager.Instance.PlaySoundEff("cardRotate");//bug修改:关闭分机小游戏音效
        StartCoroutine(ShowAniR());
    }
    private IEnumerator ShowAniR()
    {
        yield return new WaitForSeconds(1f);//等待start spine 动画播放一半时

        switch (Model.Instance.curRecord.luckyId)
        {
            case 4:
                contentObjs[0].SetActive(true);
                //var index = Model.Instance.curRecord.extraData[Model.Instance.GetPokerData(Model.Instance.curRecord.value).type - 1];
                //var index2 = Model.Instance.curRecord.extraData[Model.Instance.GetPokerData(Model.Instance.curRecord.value).type - 1] - 1;
                if (Model.Instance.curRecord.luckyPlayerId > 0
                    && Model.Instance.curRecord.extraData[0] > 0)
                {
                    multipleObj[Model.Instance.curRecord.extraData[0] - 1].SetActive(true);
                    indexInMultiple = Model.Instance.curRecord.extraData[0];
                }

                //if (Model.Instance.curRecord.luckyPlayerId > 0
                //    && Model.Instance.curRecord.extraData[Model.Instance.GetPokerData(Model.Instance.curRecord.value).type - 1] > 0)
                //    multipleObj[Model.Instance.curRecord.extraData[Model.Instance.GetPokerData(Model.Instance.curRecord.value).type - 1] - 1].SetActive(true);

                break;
            case 5:
                contentObjs[1].SetActive(true);
                break;
            case 7:
                contentObjs[2].SetActive(true);
                break;
        }
        StartCoroutine(PlayResultSFX());//bug修改:关闭分机小游戏音效

        yield return StartCoroutine(ScaleChange(0.5f, 2));
    }

    private IEnumerator PlayResultSFX()
    {
        yield return new WaitForSeconds(2f);
        switch (Model.Instance.curRecord.luckyId)
        {
            case 4:
                if (IOCanvasModel.Instance.curlanguage == Language.en)
                {
                    AudioManager.Instance.PlayVoiceEff("times1");
                    // yield return new WaitForSeconds(1.3f);
                    // AudioManager.Instance.PlayVoiceEff("times3");
                    // yield return new WaitForSeconds(0.95f);
                    // AudioManager.Instance.PlayVoiceEff(indexInMultiple.ToString());
                }
                else
                {
                    AudioManager.Instance.PlayVoiceEff("times1");
                    yield return new WaitForSeconds(0.5f);
                    AudioManager.Instance.PlayVoiceEff(indexInMultiple.ToString());
                    yield return new WaitForSeconds(0.35f);
                    AudioManager.Instance.PlayVoiceEff("times2");
                }
                //AudioManager.Instance.PlayVoiceEff("times");//旧的
                break;
            case 5:
                AudioManager.Instance.PlayVoiceEff("double");
                break;
            case 7:
                AudioManager.Instance.PlayVoiceEff("allWin");
                break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="time">一次缩,或者放,需要时间</param>
    /// <param name="count">几次缩放</param>
    /// <returns></returns>
    private IEnumerator ScaleChange(float time, int count)
    {
        yield return new WaitForSeconds(1f);//等待start spine 动画播放完毕
        float originScale = contentObj.transform.localScale.x;
        float targetScale = contentObj.transform.localScale.x + 0.1f;
        //float time = 0.2f;
        for (int i = 0; i < count; i++)
        {
            yield return StartCoroutine(ScaleOnce(time, originScale, targetScale));
        }
    }
    private IEnumerator ScaleOnce(float time, float originScale, float targetScale)
    {
        float timer = 0f;
        while (timer < time)
        {
            timer += Time.deltaTime;
            float scale = Mathf.Lerp(originScale, targetScale, timer / time);
            contentObj.transform.localScale = new Vector3(scale, scale, 1);
            yield return null;
        }
        timer = 0f;
        while (timer < time)
        {
            timer += Time.deltaTime;
            float scale = Mathf.Lerp(targetScale, originScale, timer / time);
            contentObj.transform.localScale = new Vector3(scale, scale, 1);
            yield return null;
        }
    }

    public void ResetCard()
    {
        if (!isInit) InitView();
        //spinePlayer.Show();
        spinePlayer.Hide();
        transform.localPosition = oriPos;
        foreach (GameObject go in contentObjs)
            go.SetActive(false);
        foreach (GameObject go in multipleObj)
            go.SetActive(false);
        contentObj.SetActive(false);
        Model.Instance.waitSelectCard = false;
    }
}
