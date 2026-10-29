/**
 * @file    
 * @author  Huang Wen <Email:ww1383@163.com, QQ:214890094, WeChat:w18926268887>
 * @version 1.0
 *
 * @section LICENSE
 *
 * Permission is hereby granted, free of charge, to any person obtaining a
 * copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included
 * in all copies or substantial portions of the Software.
 *
 * @section DESCRIPTION
 *
 * This file is ...
 */
using Hal;
//using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace SBoxApi
{
    /// <summary>
    /// IDEA-Battle 九门百家乐角色索引。
    /// 该顺序必须与算法卡的下注数组、赔率数组和结果标志保持一致。
    /// </summary>
    public enum SBoxBattleRole
    {
        Banker = 0,                 // 庄
        Player = 1,                 // 闲
        Tie = 2,                    // 和
        BankerPair = 3,             // 庄对
        PlayerPair = 4,             // 闲对
        SmallTiger6 = 5,            // 小老虎6：庄两张牌6点并且庄赢
        BigTiger6 = 6,              // 大老虎6：庄三张牌6点并且庄赢
        Dragon7 = 7,                // 龙7：闲三张牌7点并且闲赢
        Bear8 = 8,                  // 熊8：闲三张牌8点并且闲赢
        Count = 9
    }

    public class SBoxBattleLeadData
    {
        public int result;

        public int LuckPlayerId;                    // 幸运玩家ID（1~10）
        public int LuckId;                          // 小游戏中奖类型
                                                    // 0：未中小游戏
                                                    // 1：保留
                                                    // 2：保留
                                                    // 3：保留
                                                    // 4：加倍
                                                    // 5：翻倍
                                                    // 6：明牌
                                                    // 7：大满贯（押分全中）
                                                    // 8：彩金
        public int BankerProfit;                    // 庄家盈利
        public int BankerID;                        // 庄家ID
        public int[] Data;                          // 小游戏相应参数；Data[0~9]为原有小游戏数据
                                                    // 1，2，3: Data[0~9]保留
                                                    // 4: Data[0~4]为每个基础角色的加倍倍数
                                                    // 5: Data[0~4]为每个基础角色的翻倍倍数
                                                    // 6: Data[0]为明牌的值
                                                    // 8: Data[0~9]为本局每个玩家所中的彩金值
        public SBoxBattlePlayerPayoutData[] PlayerPayouts; // 10个玩家的九门得分和总得分
        public bool HasPlayerPayouts;               // 是否包含10×9得分扩展数据
    }

    /// <summary>
    /// 单个玩家本局九门派彩数据。
    /// RoleWins的索引对应SBoxBattleRole，TotalWin包含九门得分和小游戏得分。
    /// </summary>
    public class SBoxBattlePlayerPayoutData
    {
        public int PlayerId;                        // 玩家ID，1~10
        public int[] RoleWins = new int[(int)SBoxBattleRole.Count]; // 九门得分
        public int TotalWin;                        // 本局总得分，包含小游戏得分
    }

    public class SBoxBattleBankerPlayerInfo
    {
        public int PlayerId;                        // 玩家ID, 有效值大于0
        public bool permission;                     // 是否有资格成为庄家
        public bool request;                        // 是否抢庄
    }

    public enum SBoxBattleBankerRequestState
    {
        None = 0,
        PendingContinuation = 1,
        RequestedButCannotPay = 2,
        RequestedAndCanPay = 3,
    }

    public class SBoxBattleBankerData
    {
        public int state;                           // 状态：0空闲，1抢庄中，2抢庄完成，3已完成本局结算
        public int BankerPlayerId;                  // 庄家机台号，协议显示值1~6
        public int TotalBets;                       // 本局庄、闲两门总押分
        public int TotalWins;                       // 本局庄、闲两门总得分
        public int BankerWins;                      // 抢庄毛盈利：总押分减总得分
        public int BankerProfit;                    // 抢庄实利：抢庄毛盈利减水费
        public int Cut;                             // 本局水费；和局时按规则为0
        public int WaterRateBasisPoints;            // 水费率，basis points；200表示2%
        public double CutOdds;                      // 水费率，WaterRateBasisPoints / 10000.0
        public int EstimateBankerProfit;             // 按庄赢预估的抢庄实利
        public int EstimatePlayerProfit;             // 按闲赢预估的抢庄实利
        public int[] Priority = new int[6];          // 抢庄优先顺序，协议显示机台号1~6
        public int[] PriorityLights = new int[6];   // 按Priority顺序输出的灯状态
        public int CanGrabMask;                     // 当前余额可承担抢庄的位图，bit0~bit5对应机台1~6
        public int RequestMask;                     // 已进入Priority的位图；取消抢庄后清零
        public int ConfirmedMask;                   // 已按抢庄键确认的位图；连庄候选初始为0

        // 状态判定：RequestMask表示是否在队列，CanGrabMask表示当前是否付得起，二者独立。
        // request=1 && confirmed=0：连庄候选待确认；request=1 && canGrab=0：已抢庄但当前余额不足。
        public bool IsRequested(int playerId)
        {
            return playerId >= 1 && playerId <= 6 &&
                (RequestMask & (1 << (playerId - 1))) != 0;
        }

        public bool IsConfirmed(int playerId)
        {
            return playerId >= 1 && playerId <= 6 &&
                (ConfirmedMask & (1 << (playerId - 1))) != 0;
        }

        public bool CanGrab(int playerId)
        {
            return playerId >= 1 && playerId <= 6 &&
                (CanGrabMask & (1 << (playerId - 1))) != 0;
        }

        public SBoxBattleBankerRequestState GetRequestState(int playerId)
        {
            if (!IsRequested(playerId)) return SBoxBattleBankerRequestState.None;
            if (!IsConfirmed(playerId)) return SBoxBattleBankerRequestState.PendingContinuation;
            return CanGrab(playerId) ?
                SBoxBattleBankerRequestState.RequestedAndCanPay :
                SBoxBattleBankerRequestState.RequestedButCannotPay;
        }

        // 当前20040协议不再输出旧版permission/request表，保留字段仅为兼容旧调用方。
        public SBoxBattleBankerPlayerInfo[] BankerRequstTable = new SBoxBattleBankerPlayerInfo[10];
    }

    public class SBoxBattleStateData
    {
        public int result;                          // 0：正常，其它值：协议错误或状态码
        // 九门总押分，索引对应 SBoxBattleRole：庄、闲、和、庄对、闲对、小老虎6、大老虎6、龙7、熊8。
        public int[] TotalBets = new int[(int)SBoxBattleRole.Count];
        public int LuckPlayerId;                    // 非负时，幸运玩家ID
        public int LuckSelectIndex;                 // 非负时，玩家幸运选择索引
        public int PlayerWinLock;                   // 分机爆机位图：bit0对应玩家1，置1表示该玩家爆机
        public int rfu1;                            // 保留
        public int rfu2;                            // 保留
        public SBoxBattleBankerData BankerData;     // 抢庄数据
    }

    // 
    public enum SBOX_BATTLE_PLAYER_STATE_IN //Baccarat※需要修改对应位
    {
        STATE_IN_KEY_A = (1 << 0),
        STATE_IN_KEY_B = (1 << 1),
        STATE_IN_KEY_C = (1 << 2),
        STATE_IN_KEY_D = (1 << 3),
        STATE_IN_KEY_E = (1 << 4),
        STATE_IN_KEY_PAYOUT = (1 << 5),
        STATE_IN_KEY_SCORE_UP = (1 << 6),
        STATE_IN_KEY_SCORE_DOWN = (1 << 7),
        STATE_IN_KEY_ROOT = (1 << 8),
        STATE_IN_KEY_SWITCH = (1 << 9),
        STATE_IN_KEY_ESC = (1 << 10),
        STATE_IN_KEY_SET = (1 << 11),               //抢庄键

        STATE_IN_STATE_COIN_OUT_TIMEOUT = (1 << 14), // 退币器超时置1，否则置0
        STATE_IN_STATE_EXIST = (1 << 15),            // 本玩家在线时，这位置1，否则置0
        STATE_IN_KEY_F = (1 << 16),
        STATE_IN_KEY_G = (1 << 17),
        STATE_IN_KEY_H = (1 << 18),
        STATE_IN_KEY_I = (1 << 19),
    }

    public class SBoxBattlePlayerInState
    {
        public int PlayerId;                        // 玩家ID
        public int LIndex;                          // 0~255的循环索引号
                                                    // 在BattlePlayerInState函数主动调用发送本玩家数据时，LIndex循环累加
                                                    // 在BattlePlayerInState函数每300毫秒被动调用本玩家数据时，LIndex不改变
        public int BetUnit;                         // rfu 改为BetUnit, 押注单
        public int state;                           // 玩家状态信息，见SBOX_BATTLE_PLAYER_STATE_IN
        public int LNumberCoinIn;                   // 0~255的循环投币计数器
        public int LNumberCoinOut;                  // 0~255的循环退币计数器
    }

    public class SBoxBattlePlayerOutState
    {
        public int PlayerId;                        // 玩家ID
        public int LIndex;                          // 0~255的循环索引号
        public int rfu;                             // 保留字段
        public int credit;                          // 玩家账户分
        public int wins;                            // 玩家本局得分
        public int state;                           // 玩家状态信息
        public int seconds;                         // 倒计时，单位为秒
        // 九门押分，索引对应 SBoxBattleRole。
        public int[] bets = new int[(int)SBoxBattleRole.Count];
        // 九门赔率原始值，协议通常按10倍整数传输；BattleResetGameR会转换为double。
        public int[] odds = new int[(int)SBoxBattleRole.Count];
    }

    public partial class SBoxIdea
    {
        private static bool bOutStateListener = false;

        /**
		 *  @brief          读取游戏状态
		 *  @param          无
		 *  @return         sBoxPacket.data[0] = 0：成功
		 *                  sBoxPacket.data[0] < 0：发送参数错误
		 *                  sBoxPacket.data[0] > 0：状态码
		 *  				                        1：正在打印，其它：故障
		 *  @details        
		 */
        public static void BattleGetState()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20040, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, BattleGetStateR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void BattleGetStateR(SBoxPacket sBoxPacket)
        {
            //Debug.LogWarning($"BattleGetStateR 被调用, 收到{sBoxPacket}");
            if (sBoxPacket == null || sBoxPacket.data == null || sBoxPacket.data.Length < 1)
            {
                return;
            }

            SBoxBattleStateData sBoxBattleStateData = new SBoxBattleStateData();
            int pos = 0;
            // 首个字段是本次协议返回的角色数量。旧版本少于5门时仍按5门兼容解析，
            // IDEA-Battle 当前标准数据为9门百家乐角色。
            int role = sBoxPacket.data[pos++];
            
            if(role < 5) role = 5;
            if(role > sBoxBattleStateData.TotalBets.Length)
            {
                // 数据超过九门协议范围时拒绝继续解析，避免错位读取或数组越界。
                return;
            }
            if (sBoxPacket.data.Length < pos + role + 5)
            {
                // 角色总押分、幸运选择和保留字段不完整。
                return;
            }
            
            for(int i = 0;i < sBoxBattleStateData.TotalBets.Length; i++)
            {
            	sBoxBattleStateData.TotalBets[i] = 0;
            }
            
            sBoxBattleStateData.result = 0;
            for (int i = 0; i < role; i++)
            {
                sBoxBattleStateData.TotalBets[i] = sBoxPacket.data[pos++];
            }
            sBoxBattleStateData.LuckPlayerId = sBoxPacket.data[pos++];
            sBoxBattleStateData.LuckSelectIndex = sBoxPacket.data[pos++];
            sBoxBattleStateData.PlayerWinLock = sBoxPacket.data[pos++];   // pos = 8
            sBoxBattleStateData.rfu1 = sBoxPacket.data[pos++];
            sBoxBattleStateData.rfu2 = sBoxPacket.data[pos++];

            const int bankerFieldCount = 23; // 原有 data[15..37] 字段，后面可选追加2个位图
            if (sBoxPacket.data.Length >= pos + bankerFieldCount)
            {
                sBoxBattleStateData.BankerData = new SBoxBattleBankerData();
                sBoxBattleStateData.BankerData.state = sBoxPacket.data[pos++];
                sBoxBattleStateData.BankerData.BankerPlayerId = sBoxPacket.data[pos++];
                sBoxBattleStateData.BankerData.TotalBets = sBoxPacket.data[pos++];
                sBoxBattleStateData.BankerData.TotalWins = sBoxPacket.data[pos++];
                sBoxBattleStateData.BankerData.BankerWins = sBoxPacket.data[pos++];
                sBoxBattleStateData.BankerData.WaterRateBasisPoints = sBoxPacket.data[pos++];
                sBoxBattleStateData.BankerData.CutOdds =
                    sBoxBattleStateData.BankerData.WaterRateBasisPoints / 10000.0;
                sBoxBattleStateData.BankerData.Cut = sBoxPacket.data[pos++];
                sBoxBattleStateData.BankerData.BankerProfit = sBoxPacket.data[pos++];
                sBoxBattleStateData.BankerData.EstimateBankerProfit = sBoxPacket.data[pos++];
                sBoxBattleStateData.BankerData.EstimatePlayerProfit = sBoxPacket.data[pos++];

                for (int i = 0; i < sBoxBattleStateData.BankerData.Priority.Length; i++)
                {
                    sBoxBattleStateData.BankerData.Priority[i] = sBoxPacket.data[pos++];
                }
                for (int i = 0; i < sBoxBattleStateData.BankerData.PriorityLights.Length; i++)
                {
                    sBoxBattleStateData.BankerData.PriorityLights[i] = sBoxPacket.data[pos++];
                }
                sBoxBattleStateData.BankerData.CanGrabMask = sBoxPacket.data[pos++];
                // 新版20040追加：RequestMask表示是否仍在priority中，
                // ConfirmedMask表示是否已经按键确认；旧版响应没有这两个字段时保持0。
                if (sBoxPacket.data.Length >= pos + 2)
                {
                    sBoxBattleStateData.BankerData.RequestMask = sBoxPacket.data[pos++];
                    sBoxBattleStateData.BankerData.ConfirmedMask = sBoxPacket.data[pos++];
                }
            }

            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_BATTLE_GET_STATE, sBoxBattleStateData);
        }

        /**
		 *  @brief          读取当前轮数和局数
		 *  @param          无
		 *  @return         sBoxPacket.data[0] = 0：成功
		 *                  sBoxPacket.data[0] < 0：发送参数错误
		 *                  sBoxPacket.data[0] > 0：状态码
		 *                  sBoxPacket.data[1]：当前是第几轮游戏
		 *                  sBoxPacket.data[2]：当前是第几局游戏
		 *  @details        
		 */
        public static void BattleGameNumber()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20041, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, BattleGameNumberR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void BattleGameNumberR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_BATTLE_GAME_NUMBER, sBoxPacket.data);
        }

        /**
		 *  @brief          读取已开奖的结果
		 *  @param          无
		 *  @return         sBoxPacket.data[0] = 0：成功
		 *                  sBoxPacket.data[0] < 0：发送参数错误
		 *                  sBoxPacket.data[0] > 0：状态码
		 *  				sBoxPacket.data[1~100]: 结果
		 *  				
		 *  @details        
		 */
        public static void BattleGetCompletedGame()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20042, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, BattleGetCompletedGameR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void BattleGetCompletedGameR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_BATTLE_GET_COMPLETED_GAME, sBoxPacket.data);
        }



        /**
		 *  @brief          重置指令，用于复原押分控台的显示。
		 *                  返回当前百家乐九门的赔率表。
		 *  @param          无
		 *  @return         odds[0~8]，协议值除以10后得到实际赔率。
		 *                  索引对应 SBoxBattleRole：庄、闲、和、庄对、闲对、
		 *                  小老虎6、大老虎6、龙7、熊8。
		 *  @details        赔率由算法卡返回，客户端只负责按协议解析，不在此处修改赔率。
		 */
        public static void BattleResetGame()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20043, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, BattleResetGameR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void BattleResetGameR(SBoxPacket sBoxPacket)
        {
            double[] odds = new double[sBoxPacket.data.Length - 1];

            for (int i = 0; i < odds.Length; i++)
            {
                odds[i] = (double)sBoxPacket.data[i + 1] / (double)10.0;
            }

            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_BATTLE_RESET_GAME, odds);
        }


        /**
		 *  @brief          开启新的一轮（100局），这时候算法卡会启动打印路单的过程
		 *  @param          无
		 *  @return         SBoxPacket.data[0] = 0：成功
		 *                  SBoxPacket.data[0] < 0：发送参数错误
		 *                  SBoxPacket.data[0] > 0：状态码
		 *  @details        
		 */
        public static void BattleNewRound()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20044, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, BattleNewRoundR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void BattleNewRoundR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_BATTLE_NEW_ROUND, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          中止当轮，同时会把当轮的所有开奖结果返回（100局），用于显示出来
		 *  @param          无
		 *  @return         SBoxPacket.data[0] = 0：成功
		 *                  SBoxPacket.data[0] < 0：发送参数错误
		 *                  SBoxPacket.data[0] > 0：状态码
		 *                  SBoxPacket.data[1~100]：开奖结果
		 *  @details        
		 */
        public static void BattleEndRound()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20045, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, BattleEndRoundR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void BattleEndRoundR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_BATTLE_END_ROUND, sBoxPacket.data);
        }

        /**
		 *  @brief          请求本局百家乐开奖结果。
		 *  @return         data[0]：状态；data[1]：轮号；data[2]：局号；
		 *                  data[3..5]：庄三张牌；data[6..8]：闲三张牌；
		 *                  data[9]：庄闲和；data[10]：庄对；data[11]：闲对；
		 *                  data[12]：小老虎6；data[13]：大老虎6；
		 *                  data[14]：龙7；data[15]：熊8；
		 *                  data[16]：庄牌数；data[17]：闲牌数；data[18]：是否明牌。
		 *  @details        牌编码高4位为花色，低4位为点数；对子字段为0/1。
		 */
        public static void BattleRequestResult(int rfu)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20046, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, BattleRequestResultR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void BattleRequestResultR(SBoxPacket sBoxPacket)
        {
            if (sBoxPacket == null || sBoxPacket.data == null || sBoxPacket.data.Length < 19)
            {
                return;
            }
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_BATTLE_REQUEST_RESULT, sBoxPacket.data);
        }


        /**
		 *  @brief          开牌结束，由于以上开牌有动画过程，需在这里发出开牌结束指令，用于控台进行开牌，中奖的显示
		 *  @return         sBoxPacket.data[0] = 0：成功
		 *                  sBoxPacket.data[0] < 0：发送参数错误
		 *                  sBoxPacket.data[0] > 0：状态码
		 *                  
		 *  @details        
		 */
        public static void BattleLeadStop()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20048, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, BattleLeadStopR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void BattleLeadStopR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_BATTLE_LEAD_STOP, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          开牌
		 *  @return         sBoxBattleLeadData.result = 0：成功
		 *                  sBoxBattleLeadData.result < 0：发送参数错误
		 *                  sBoxBattleLeadData.result > 0：状态码
		 *                  
		 *  @details        
		 */
        public static void BattleLeadStart()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20047, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, BattleLeadStartR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void BattleLeadStartR(SBoxPacket sBoxPacket)
        {
            if (sBoxPacket == null || sBoxPacket.data == null || sBoxPacket.data.Length < 3)
            {
                return;
            }

            SBoxBattleLeadData sBoxBattleLeadData = new SBoxBattleLeadData();

            sBoxBattleLeadData.result = 0;
            sBoxBattleLeadData.BankerProfit = sBoxPacket.data[0];
            if(sBoxBattleLeadData.BankerProfit!=0)
                sBoxBattleLeadData.BankerID = (sBoxPacket.data[1] >> 8)+1;
            else
                sBoxBattleLeadData.BankerID = 0;
            sBoxBattleLeadData.LuckId = sBoxPacket.data[1] & 0xff;
            sBoxBattleLeadData.LuckPlayerId = sBoxPacket.data[2];

            sBoxBattleLeadData.Data = new int[sBoxPacket.data.Length - 3];
            for (int i = 3; i < sBoxPacket.data.Length; i++)
            {
                sBoxBattleLeadData.Data[i - 3] = sBoxPacket.data[i];
            }

            const int luckDataCount = 10;
            const int payoutFieldsPerPlayer = (int)SBoxBattleRole.Count + 1;
            const int payoutDataCount = 10 * payoutFieldsPerPlayer;
            if (sBoxBattleLeadData.Data.Length >= luckDataCount + payoutDataCount)
            {
                sBoxBattleLeadData.PlayerPayouts = new SBoxBattlePlayerPayoutData[10];
                for (int player = 0; player < sBoxBattleLeadData.PlayerPayouts.Length; player++)
                {
                    SBoxBattlePlayerPayoutData payout = new SBoxBattlePlayerPayoutData();
                    payout.PlayerId = player + 1;
                    int payoutOffset = luckDataCount + player * payoutFieldsPerPlayer;
                    for (int role = 0; role < (int)SBoxBattleRole.Count; role++)
                    {
                        payout.RoleWins[role] = sBoxBattleLeadData.Data[payoutOffset + role];
                    }
                    payout.TotalWin = sBoxBattleLeadData.Data[payoutOffset + (int)SBoxBattleRole.Count];
                    sBoxBattleLeadData.PlayerPayouts[player] = payout;
                }
                sBoxBattleLeadData.HasPlayerPayouts = true;
            }

            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_BATTLE_LEAD_START, sBoxBattleLeadData);
        }

        /**
		 *  @brief          显示小游戏
		 *  @param          
		 *  @return         sBoxPacket.data[0] = 0：成功
		 *                  sBoxPacket.data[0] < 0：发送参数错误
		 *                  sBoxPacket.data[0] > 0：状态码
		 *                  
		 *  @details        
		 */
        public static void BattLuckShow()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20049, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, BattLuckShowR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void BattLuckShowR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_BATTLE_LUCK_SHOW, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          开小游戏
		 *  @return         sBoxPacket.data[0] = 0：成功
		 *                  sBoxPacket.data[0] < 0：发送参数错误
		 *                  sBoxPacket.data[0] > 0：状态码
		 *                  
		 *  @details        
		 */
        public static void BattLuckPrize()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20050, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, BattLuckPrizeR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void BattLuckPrizeR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_BATTLE_LUCK_PRIZE, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          打印机开箱
		 *  @param          PaperDoor 换纸门，true：开门，false：不动作
		 *                  TicketDoor 取单门，true：开门，false：不动作
		 *  @return         sBoxPacket.data[0] = 0：成功
		 *                  sBoxPacket.data[0] < 0：发送参数错误
		 *                  sBoxPacket.data[0] > 0：状态码
		 *                  
		 *  @details        
		 */
        public static void BattPrinterOpenBox(bool PaperDoor, bool TicketDoor)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20051, source: 1, target: 2, size: 2);

            if (PaperDoor == true)
            {
                sBoxPacket.data[0] = 1;
            }
            else
            {
                sBoxPacket.data[0] = 0;
            }

            if (TicketDoor == true)
            {
                sBoxPacket.data[1] = 1;
            }
            else
            {
                sBoxPacket.data[1] = 0;
            }

            SBoxIOEvent.AddListener(sBoxPacket.cmd, BattPrinterOpenBoxR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void BattPrinterOpenBoxR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_BATTLE_PRINTER_OPEN_BOX, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          玩家产生的状态数据
		 *  @param          sBoxPlayerInState 见SBoxPlayerInState的描述
		 *  @return          
		 *  @details        本函数，在对应玩家（多个玩家时，需要拼接数据）数据变化时直接调用
		 *                  同时需要设定一个定时器，平时没变化时，每300毫秒自动调用
		 */
        public static void BattlePlayerInState(List<SBoxBattlePlayerInState> sBoxPlayerInState)
        {
            if (sBoxPlayerInState == null || sBoxPlayerInState.Count > 20)
            {
                return;
            }

            // idea_cmd.c 的20052按每台2个整数接收：
            // packed(PlayerId[4], LIndex[4], BetUnit[4], CoinIn[8], CoinOut[8]) + state。
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20052, source: 1, target: 2, size: sBoxPlayerInState.Count * 2);
            int pos = 0;

            for (int i = 0; i < sBoxPlayerInState.Count; i++)
            {
                SBoxBattlePlayerInState state = sBoxPlayerInState[i];
                int packed = ((state.PlayerId & 0x0f) << 16) |
                    ((state.LIndex & 0x0f) << 20) |
                    ((state.BetUnit & 0x0f) << 24) |
                    (state.LNumberCoinIn & 0xff) |
                    ((state.LNumberCoinOut & 0xff) << 8);

                sBoxPacket.data[pos++] = packed;
                sBoxPacket.data[pos++] = state.state;
            }
            SBoxIOStream.Write(sBoxPacket);
        }

        /**
		 *  @brief          玩家产生的状态数据
		 *  @param          sBoxPlayerInState 见SBoxPlayerInState的描述
		 *  @return          
		 *  @details        本函数，在对应玩家（多个玩家时，需要拼接数据）数据变化时直接调用
		 *                  同时需要设定一个定时器，平时没变化时，每300毫秒自动调用
		 */
        public static void BattlePlayerOutStateListener()
        {
            if(bOutStateListener != true)
            {
                bOutStateListener = true;
                SBoxIOEvent.AddListener(20053, OutStateMessageR);
            }
        }

        private static void OutStateMessageR(SBoxPacket sBoxPacket)
        {
            const int roleCount = (int)SBoxBattleRole.Count;
            const int fieldsPerPlayer = roleCount * 2 + 4;
            List<SBoxBattlePlayerOutState> sBoxBattlePlayerOutState = new List<SBoxBattlePlayerOutState>();

            if (sBoxPacket == null || sBoxPacket.data == null ||
                sBoxPacket.data.Length == 0 ||
                (sBoxPacket.data.Length % fieldsPerPlayer) != 0)
            {
                return;
            }

            int pos = 0;
            while (pos < sBoxPacket.data.Length)
            {
                int packed = sBoxPacket.data[pos++];
                SBoxBattlePlayerOutState state = new SBoxBattlePlayerOutState();

                // 与 box_device_packet_ex_send() 对应：
                // PlayerId+1[4]、idx[4]、AddonData[8]、seconds[16]。
                state.PlayerId = packed & 0x0f;
                state.LIndex = (packed >> 4) & 0x0f;
                state.rfu = (packed >> 8) & 0xff;
                state.seconds = (packed >> 16) & 0xffff;
                state.credit = sBoxPacket.data[pos++];
                state.wins = sBoxPacket.data[pos++];
                state.state = sBoxPacket.data[pos++];

                for (int i = 0; i < roleCount; i++)
                {
                    state.bets[i] = sBoxPacket.data[pos++];
                }
                for (int i = 0; i < roleCount; i++)
                {
                    state.odds[i] = sBoxPacket.data[pos++];
                }
                sBoxBattlePlayerOutState.Add(state);
            }

            if (sBoxBattlePlayerOutState.Count > 0)
            {
                EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_BATTLE_PLAYER_OUT_STATE, sBoxBattlePlayerOutState);
            }
        }

        /**
         *  @brief          读取所有玩家总押注
         *  @param          
         *  @return         data[0] < 0：失败
         *                  data[0] = 0：data[1~10]为对应玩家当前总押注
         *  @details        
         */
        public static void GetPlayerTotalBet()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20055, source: 1, target: 2, size: 2);
            int pos = 0;
            sBoxPacket.data[pos++] = 0;
            sBoxPacket.data[pos++] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, GetPlayerTotalBetR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void GetPlayerTotalBetR(SBoxPacket sBoxPacket)
        {
        		EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_BATTLE_PLAYER_TOTAL_BET_GET, sBoxPacket.data);
        }

        /**
         *  @brief          读取打印机电机偏移
         *  @param          
         *  @return         sBoxPacket.data[0] = 0：成功
         *                  sBoxPacket.data[1]    ：电机从后向前转向所需要的偏移
         *                  sBoxPacket.data[2]    ：电机从前向后转向所需要的偏移
         *                  
         *  @details        
         */
        public static void BattPrinterOffsetGet()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20056, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, BattPrinterOffsetGetR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void BattPrinterOffsetGetR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_BATTLE_PRINTER_OFFSET_GET, sBoxPacket.data);
        }

        /**
         *  @brief          设定打印机电机偏移
         *  @param          hs 电机从后向前转向所需要的偏移
         *  @param          ls 电机从前向后转向所需要的偏移
         *  @return         sBoxPacket.data[0] = 0：成功
         *  @details        
         */
        public static void BattPrinterOffsetSet(int hs, int ls)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20057, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = hs;
            sBoxPacket.data[1] = ls;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, BattPrinterOffsetSetR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void BattPrinterOffsetSetR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_BATTLE_PRINTER_OFFSET_SET, sBoxPacket.data[0]);
        }
    }
}
