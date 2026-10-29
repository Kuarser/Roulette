public class BetManager : FSMBaseManager
{
    public BetManager()
    {
        BetState state = new BetState();
        FSMSystem.Instance.AddState(state);
    }
}
