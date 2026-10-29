using IOT;
using System.Collections.Generic;

public class SBoxModel:BaseManager<SBoxModel>
{
    public bool isSboxReady;
    public bool isSboxSandboxReady;
    public int macId;
    public const int MaxMacId = 6;//分机号上限
}

public class CoinInData
{
    public int id;
    public int value;
}

public class CoinOutData
{
    public int id;
    public int count;
    public int type;
}
