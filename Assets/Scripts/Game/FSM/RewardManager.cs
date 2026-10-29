public class RewardManager : FSMBaseManager
{
    public RewardManager()
    {
        RewardState state = new RewardState();
        FSMSystem.Instance.AddState(state);
    }
}
