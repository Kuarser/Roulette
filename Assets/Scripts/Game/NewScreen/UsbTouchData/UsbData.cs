using static UsbTouchDataParser;
using System.Text;

public class UsbDeviceListenInfo
{
    //设备唯一标识
    public string deviceId;
    //接口下标
    public int interfaceIndex;
    //端点下标
    public int endpointIndex;

    public static UsbDeviceListenInfo New(string deviceId, int interfaceIndex, int endpointIndex)
    {
        return new UsbDeviceListenInfo() 
        { 
            deviceId = deviceId,
            interfaceIndex = interfaceIndex,
            endpointIndex = endpointIndex,
        };
    }
}

[System.Serializable]
public class UsbDeviceListWrapper
{
    //设备数组
    public UsbDeviceInfoDto[] devices;
}

[System.Serializable]
public class UsbDeviceInfoDto
{
    //设备唯一标识
    public string deviceId;
    //设备名称
    public string deviceName;
    //厂商编号
    public int vendorId;
    //产品编号
    public int productId;
    //设备类型编号
    public int deviceClass;
    //设备子类型编号
    public int deviceSubclass;
    //设备协议编号
    public int deviceProtocol;
    //设备版本
    public string deviceVersion;
    //厂商名称
    public string manufacturerName;
    //产品名称
    public string productName;
    //序列号
    public string serialNumber;
    //解析类型
    public string resolvedType;
    //接口数组
    public UsbInterfaceInfoDto[] interfaces;
}

[System.Serializable]
public class UsbInterfaceInfoDto
{
    //接口编号
    public int id;
    //可选设置
    public int alternateSetting;
    //接口类型编号
    public int interfaceClass;
    //接口子类型编号
    public int interfaceSubclass;
    //接口协议编号
    public int interfaceProtocol;
    //端点数组
    public UsbEndpointInfoDto[] endpoints;
}

[System.Serializable]
public class UsbEndpointInfoDto
{
    //端点地址
    public int address;
    //端点属性
    public int attributes;
    //方向
    public int direction;
    //端点编号
    public int number;
    //类型
    public int type;
    //最大包长
    public int maxPacketSize;
    //轮询间隔
    public int interval;
}

[System.Serializable]
public class StartListeningResult
{
    //是否成功
    public bool success;
    //错误信息（成功时为空或空串）
    public string error;
}

[System.Serializable]
public class UsbDataResultDto
{
    //设备唯一标识
    public string deviceId;
    //接收成功标记
    public bool success;
    //错误信息
    public string errorMessage;
    //接收数据长度
    public int dataLength;
    //接收数据（Base64编码）
    public string data;
}