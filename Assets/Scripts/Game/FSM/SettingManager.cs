public class SettingManager : FSMBaseManager
{
    public SettingManager()
    {
        SettingState state = new SettingState();
        FSMSystem.Instance.AddState(state);
    }
}
