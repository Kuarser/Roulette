
public class PrintManager : FSMBaseManager
{
    public PrintManager()
    {
        PrintState state = new PrintState();
        FSMSystem.Instance.AddState(state);
    }
}
