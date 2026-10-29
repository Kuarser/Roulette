public class ErrorManager : FSMBaseManager
{
    public ErrorManager()
    {
        ErrorState state = new ErrorState();
        FSMSystem.Instance.AddState(state);
    }
}
