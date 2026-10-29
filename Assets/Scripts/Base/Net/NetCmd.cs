public enum S2C_CMD
{
    S2C_Start = 100,                    //每局游戏开始
    S2C_Config,                         //游戏参数配置
    S2C_UpdateConfig,                   //更新配置
    S2C_UpdateRecord,                   //更新游戏记录
    S2C_HeartHeat,                      //心跳
    S2C_SetRecod,                       //设置记录
    S2C_ResetRecord,                    //重置记录
    S2C_CcountDown,                     //倒计时
    S2C_StopBet,                        //停止下注
    S2C_Reward,                         //开始开奖
    S2C_ShowResult,                     //显示结果
    S2C_State,                          //状态
    S2C_BattleLuckSelect,               //幸运座位选择
    S2C_SetJackpot,                     //设置彩金
    S2C_SBoxSetState,                   //设置状态
    S2C_SBoxPlayerOutState,             //玩家操作状态
    S2C_SystemTips,                     //系统提示
    S2C_HideTips,                       //隐藏提示
    S2C_SoundVolumn,                    //音量
    S2C_ResetView,                      //重置视图
    S2C_ClientCoinCoin,                 //分机分机缓存计数器
    S2C_ClientCoinOut,                  //分机退票缓存计数器
    S2C_ShowClientWinLock,              //显示客户端锁定提示
    S2C_HideClientWinLock,              //隐藏客户端锁定提示
    S2C_WinNetJackpot,                  //网络彩金赢分
    S2C_ChangeNetJPState,               //网络彩金状态改变
    S2C_NetJackpotMinBet,               //网络彩金最小押分
    S2C_WinScoreScaleTween,             //赢分闪烁
    S2C_XiaSanRoadData,                 //下三路数据
    S2C_UpdateClientData,               //更新分机最后押注记录
    S2C_RewardEnd,                      //开牌动画结束,播放结果动画
    S2C_MingPai,                        //开始 明牌 小游戏
    S2C_ShowResultWin,                  //显示本局结果赢分
    S2C_ShowLuckySeatCard,              //显示小游戏幸运座位卡牌
    S2C_SetGameData,                    //设置游戏数据
    S2C_SetbetCoinList,                 //设置押分个数
    S2C_UpdateHistoryData,              //更新历史快照
    S2C_UpdateHistoryData6,             //更新那六个预测
    S2C_SetGameMax,                     //更新局数到最大
    S2C_PauseFadeCredit,                //暂停假积分,冻结分数刷新
    S2C_CancleFadeCredit,               //取消假积分,恢复分数刷新
    S2C_BgmSetting,                     //背景音乐设置
    S2C_SBoxBillInOrderResult = 142,    //分机入钞加分订单已完成回执
    S2C_SBoxRefundPrint = 143,         //主机下发退分小票打印数据
    S2C_MacConflict = 144,              //分机号冲突: 1=冲突分机, 2=主连接警告, 0=冲突解除
    S2C_JackpotHearHeat = 1000,         //彩金心跳
    S2C_WinJackpot,                     //获得彩金
    S2C_Error,                          //错误
    S2C_GetJackpotData,                 //获取彩金数据
    S2C_JackpotMinBet,                  //彩金最小押分

}

//客户端发送给服务器的消息
public enum C2S_CMD
{
    C2S_HeartHeat = 200,                //心跳
    C2S_GetGameConfig,                  //获取游戏配置
    C2S_GetGameState,                   //获取游戏状态

    C2S_HARDWARD_KEYDOWN = 300,         //硬件按键按下
    C2S_HARDWARD_KEYUP,                 //硬件按键抬起
    C2S_SBoxPlayerInState,              //玩家操作状态
    C2S_SBoxCoinIn,                     //硬币进入
    C2S_SBoxCoinOut,                    //硬币出币
    C2S_CoinOutStateChange,             //退票状态改变
    C2S_GetClientCoinIn,                //获取分机投币缓存计数器
    C2S_GetClientCoinOut,               //获取分机退票缓存计数器
    C2S_SetBetIndex,                    //分机通知主机当前选择的押注筹码下标,由主机转发算法卡
    C2S_SBoxBillIn = 309,               //分机纸币入钞,请求主机代其上分
    C2S_SBoxBillInOrder = 310,          //分机纸币入钞(带订单号),请求主机加分
    C2S_SBoxRefund = 311,               //分机退分请求(带分机号),通知主机退币
    C2S_SBoxRefundPrintAck = 312,      //分机打印完成回执(带打印订单号)

    C2S_JackpotHeartHeat = 2000,        //彩金心跳
    C2S_Login,                          //登录
    C2S_JackBet,                        //下注
    C2S_ReceiveJackpot,                 //领取彩金
    C2S_GetJackpotData,                 //获取彩金数据
}