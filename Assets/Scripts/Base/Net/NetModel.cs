using System.Collections.Generic;

public class NetModel : BaseManager<NetModel>
{
    //6 个分机位的在线状态，下标 = macId - 1，数量与 SBoxModel.MaxMacId 保持一致
    public List<int> playerOnlineState = new List<int> { 0, 0, 0, 0, 0, 0 };
}
