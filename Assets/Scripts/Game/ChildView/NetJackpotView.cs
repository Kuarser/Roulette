using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetJackpotView : MonoBehaviour
{
    public RectTransform rect;
    public Animator netJackpotAni;
    public Image netJackpotImg;
    public Text netJackpotText;
    public Text netJackpotNum;
    public List<Sprite> netJackpotBG = new List<Sprite>();


    public void InitPlayAni()
    {
        netJackpotAni.Play("netJackpotViewDefault", 0, 0f);
    }

    public void WinNetJackpotAni(WinJackpotInfo winJackpotInfo)
    {
        Debug.LogError("WinNetJackpotAni");
        rect.anchoredPosition = Vector2.zero;
        rect.localScale = Vector2.one;
        netJackpotAni.SetInteger("state", 0);
        //netJackpotAni.SetBool("state",true);
        netJackpotImg.gameObject.SetActive(true);
        netJackpotImg.sprite = netJackpotBG[winJackpotInfo.jackpotId];
        netJackpotText.text = $"恭喜 {winJackpotInfo.seat} 号玩家!";
        //netJackpotNum.text = "中联机奖励" + "0";
        netJackpotNum.text = "中联机奖励" + (winJackpotInfo.win / 100 * IOCanvasModel.Instance.CfgData.CoinValue).ToString();
        //StartCoroutine(NetJackpotNumChange(winJackpotInfo.win / 100 * IOCanvasModel.Instance.CfgData.CoinValue));
        StartCoroutine(DelayHideWinNetJackpot(winJackpotInfo.seat));
    }

    private IEnumerator NetJackpotNumChange(int target)
    {
        yield return new WaitForSeconds(1);
        int curNum = 0;
        int step = target / 60;
        while (curNum < target)
        {
            curNum += step;
            netJackpotNum.text = "中联机奖励" + (curNum > target ? target.ToString() : curNum.ToString());
            yield return new WaitForSeconds(0.02f);
        }
        netJackpotNum.text = "中联机奖励" + target.ToString();
    }

    private IEnumerator DelayHideWinNetJackpot(int seat)
    {

        if (seat == SBoxModel.Instance.macId)
        {
            yield return new WaitForSeconds(7);
            AsyncActionUtils.ApplyAnchoredMovement(this, transform, Vector2.zero, new Vector2(511, -564), 1.5f, TweenUtils.VectorTweenLinear);
            AsyncActionUtils.ApplyScaling(this, transform, Vector3.one, Vector3.zero, 1.5f, TweenUtils.VectorTweenLinear);
            yield return new WaitForSeconds(2);
        }
        else
            yield return new WaitForSeconds(9);
        netJackpotAni.SetInteger("state", 1);
        yield return new WaitForSeconds(1.5f);
        netJackpotImg.gameObject.SetActive(false);
        if (Model.Instance.winJackpotInfos.Count > 0)
            WinNetJackpotAni(Model.Instance.winJackpotInfos.Dequeue());
        else
            Model.Instance.showingNetJackpot = false;
    }
}
