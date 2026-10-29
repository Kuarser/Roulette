
public class EndRoundManager : FSMBaseManager
{
    public EndRoundManager()
    {
        EndRoundState state = new EndRoundState();
        FSMSystem.Instance.AddState(state);
    }
}
