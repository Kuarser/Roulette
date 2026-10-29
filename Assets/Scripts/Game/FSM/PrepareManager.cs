public class PrepareManager : FSMBaseManager
{
    public PrepareManager()
    {
        PrepareState state = new PrepareState();
        FSMSystem.Instance.AddState(state);
        //FSMSystem.Instance.SetCurrentState(state);//游戏开始入口
    }
}
