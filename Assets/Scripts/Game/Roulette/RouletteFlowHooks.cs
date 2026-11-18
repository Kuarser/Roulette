using System;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        // 轮盘正常流程对外暴露的 3 个节点，宿主只订阅、不改轮盘规则。
        public event Action FlowBetOpened;
        public event Action FlowSpinCommitted;
        public event Action FlowRoundClosed;

        internal void RaiseFlowBetOpened()
        {
            Action handler = FlowBetOpened;
            if (handler != null) handler();
        }

        internal void RaiseFlowSpinCommitted()
        {
            Action handler = FlowSpinCommitted;
            if (handler != null) handler();
        }

        internal void RaiseFlowRoundClosed()
        {
            Action handler = FlowRoundClosed;
            if (handler != null) handler();
        }

        /// <summary>宿主请求打开下一局押分窗口；成功返回 true。</summary>
        public bool HostOpenBetting()
        {
            if (session == null) return false;
            if (!SourceCreditMeterNonzero) return false;
            if (sourceGameplayState == SourceGameplayState.Bet)
            {
                // 已经处于押分状态时补发一次事件，避免 FSM 停在 Prepare。
                RaiseFlowBetOpened();
                return true;
            }
            RequestSourceBetState();
            return true;
        }

        /// <summary>
        /// 轮盘是否处于可安全进入后台设置的空闲状态：
        /// 未转动、无待结算结果、不在双倍流程、无收币/付款待处理。
        /// </summary>
        public bool HostIsIdleForSettings()
        {
            if (session == null) return true;
            if (spinning) return false;
            if (session.SpinPending) return false;
            if (session.InDoubleUp) return false;
            if (winRackActive) return false;
            if (ResultDisplayActive) return false;
            if (pcCollectPending) return false;
            if (SourceRoundBlocksOrdinaryInput) return false;
            return true;
        }

        /// <summary>
        /// 宿主进入后台设置时调用：把轮盘压回空闲，避免设置期间仍在收押分输入。
        /// 不结算、不改信用，只关闭桌面输入并清掉待提交的押分状态。
        /// </summary>
        public void HostEnterSettings()
        {
            if (session == null) return;
            sourceTableInputEnabled = false;
            sourceBetStateEntryPending = false;
            if (sourceGameplayState != SourceGameplayState.Playing)
                sourceGameplayState = SourceGameplayState.Idle;
            PurgeSourcePlayerInputs();
        }

        /// <summary>仅冒烟使用：押 1 个筹码并提交转动；只有真正开始转动才返回 true。</summary>
        internal bool HostSmokePlay(int spot)
        {
            if (session == null || spinning || SourceRoundBlocksOrdinaryInput) return false;
            if (session.TotalBet == 0) Bet(spot);
            if (session.TotalBet == 0) return false;
            if (session.SpinPending) return true;
            BeginSpin();
            // BeginSpin 可能被 configuredAutoPlayCheckPending 等守卫拒绝，必须
            // 以 SpinPending 为准，否则调用方会误以为已提交而停止重试。
            return session.SpinPending || spinning;
        }
    }
}
