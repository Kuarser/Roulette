using SBoxApi;
using UnityEngine;

public class TouchTriger : MonoBehaviour
{
    public SBOX_BATTLE_PLAYER_STATE_IN code;
    public bool isCheckLuckPlayer = false;//inspector 面板界面赋值
    bool isPress = false;

    private void Start()
    {
        var tool = GetComponent<MyTriggerDetection>();
        tool.pointerEnter = PointerDown;
        tool.pointerExit = PointerUp;
        if (!Application.isEditor)
        {
            EventCenter.Instance.AddEventListener(EventHandle.LOOP_CHECK, LoopCheck);
            Debug.Log("开启监听");
        }
    }

    private void PointerDown()
    {
        if (Model.Instance.rouletteFlow && IsBetKey()) return;
        //if (isCheckLuckPlayer)
        //{
        //    Debug.Log("测试,isCheckLuckPlayer满足条件,会导致无法执行下一步");
        //}
        //if (Model.Instance.curRecord.luckyPlayerId != SBoxModel.Instance.macId)
        //{
        //    Debug.Log($"测试,满足条件,中奖号{Model.Instance.curRecord.luckyPlayerId}不等于 座位号{SBoxModel.Instance.macId},会导致无法执行下一步");
        //}
        if (isCheckLuckPlayer
            && Model.Instance.curRecord.luckyPlayerId != SBoxModel.Instance.macId)
            return;
        isPress = true;
        if (this.code == SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_PAYOUT)
        {
            //Debug.Log("@@@,因为分机爆机这时候点击了退票来解决分机爆机问题,执行ReconnectInit,重新初始化");
            if (!SandboxController.Instance.CheckCoinOutIsEnough())
                return;
        }
        Debug.Log("执行选牌");
        PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)this.code, true);
        //Model.Instance.ChangeTouchBtnState((int)this.code, true);
        AudioManager.Instance.PlaySoundEff("buttonClick");
    }

    private void PointerUp()
    {
        // if (isCheckLuckPlayer
        //     && Model.Instance.curRecord.luckyPlayerId != SBoxModel.Instance.macId)
        //     return;

        isPress = false; 
        PlayerMgr.Instance.localPlayer.LocalPlayerChangeInState((int)this.code, false);
        //Model.Instance.ChangeTouchBtnState((int)this.code, false);
    }
    public void PointerUpPublic()
    {
        PointerUp();
    }

    // 轮盘模式下九门押分键不参与算法卡投注。
    private bool IsBetKey()
    {
        return code == SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_A ||
               code == SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_B ||
               code == SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_C ||
               code == SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_D ||
               code == SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_E ||
               code == SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_F ||
               code == SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_G ||
               code == SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_H ||
               code == SBOX_BATTLE_PLAYER_STATE_IN.STATE_IN_KEY_I;
    }
    private void LoopCheck()
    {
        if (Input.touchCount == 0 && isPress)
            PointerUp();
    }


    private void OnDestroy()
    {
        EventCenter.Instance.RemoveEventListener(EventHandle.LOOP_CHECK, LoopCheck);
    }
}
