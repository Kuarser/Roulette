
public class FirstStartManager : FSMBaseManager
{
    public FirstStartManager()
    {
        FirstStartState state = new FirstStartState();
        FSMSystem.Instance.AddState(state);
        FSMSystem.Instance.SetCurrentState(state);
    }
}
