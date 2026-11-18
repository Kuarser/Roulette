using UnityEngine;

public sealed class RouletteFlowBridge
{
    public static RouletteFlowBridge Instance { get; private set; }

    private readonly RouletteDeluxe.RouletteGame game;

    private RouletteFlowBridge(RouletteDeluxe.RouletteGame game)
    {
        this.game = game;
        game.FlowBetOpened += OnBetOpened;
        game.FlowSpinCommitted += OnSpinCommitted;
        game.FlowRoundClosed += OnRoundClosed;
    }

    public static RouletteFlowBridge Attach(RouletteDeluxe.RouletteGame game)
    {
        if (game == null) return Instance;
        if (Instance != null && Instance.game == game) return Instance;
        Instance = new RouletteFlowBridge(game);
        Debug.Log("ROULETTE_FSM_FLOW_MODE: " + Model.Instance.rouletteFlow);
        return Instance;
    }

    public bool OpenBetting()
    {
        if (!Model.Instance.rouletteFlow || game == null) return false;
        return game.HostOpenBetting();
    }

    public bool PlayOneRoundForSmoke(int spot)
    {
        if (!Model.Instance.rouletteFlow || game == null) return false;
        return game.HostSmokePlay(spot);
    }

    /// <summary>
    /// 消费设置请求。轮盘模式不播宿主开奖协程，EndGameChangeState 不会执行。
    /// 只要轮盘不在运行中（未转动/无待结算/无付款待处理）就立即进入设置，
    /// 用 FSMSystem 强制重置到 Setting，不依赖 Prepare→Setting 的时机。
    /// </summary>
    public bool TryEnterSetting()
    {
        if (!Model.Instance.rouletteFlow) return false;
        if (!Model.Instance.wait4Setting) return false;
        if (game == null || !game.HostIsIdleForSettings()) return false;
        if (FSMSystem.Instance.CurrentStateID == StateID.Setting) return false;

        Model.Instance.wait4Setting = false;
        Debug.Log("ROULETTE_FSM_FLOW: " +
            FSMSystem.Instance.CurrentStateID + " -> Setting (roulette idle)");
        // 进入设置前把轮盘压回空闲，避免设置期间仍在接收押分输入。
        game.HostEnterSettings();
        FSMSystem.Instance.ForceState(StateID.Setting);
        return true;
    }

    private static bool InState(StateID id)
    {
        return FSMSystem.Instance.CurrentStateID == id;
    }

    private void OnBetOpened()
    {
        if (!Model.Instance.rouletteFlow || !InState(StateID.Prepare)) return;
        Debug.Log("ROULETTE_FSM_FLOW: Prepare -> Bet");
        FSMSystem.Instance.PerformTransition(Transition.Bet);
    }

    private void OnSpinCommitted()
    {
        if (!Model.Instance.rouletteFlow || !InState(StateID.Bet)) return;
        Debug.Log("ROULETTE_FSM_FLOW: Bet -> Reward");
        FSMSystem.Instance.PerformTransition(Transition.Reward);
    }

    private void OnRoundClosed()
    {
        if (!Model.Instance.rouletteFlow || !InState(StateID.Reward)) return;
        Debug.Log("ROULETTE_FSM_FLOW: Reward -> EndRound");
        FSMSystem.Instance.PerformTransition(Transition.EndRound);
    }
}
