using Newtonsoft.Json;
using UnityEngine;

public class UsbDataReceiver : MonoBehaviour
{
    public delegate void ReceivedHandle(string deviceId, byte[] bytes);
    public ReceivedHandle OnReceivedHandle;

    public void OnUsbDataReceived(string json)
    {
        if (string.IsNullOrEmpty(json)) return;

        UsbDataResultDto result = null;
        try
        {
            result = JsonConvert.DeserializeObject<UsbDataResultDto>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"OnUsbDataReceived: 解析JSON失败: {e.Message}");
            return;
        }

        if (result == null)
        {
            Debug.LogError("OnUsbDataReceived: 解析结果为空");
            return;
        }

        if (!result.success)
        {
            Debug.LogError($"USB 数据接收失败，设备ID: {result.deviceId}, 错误: {result.errorMessage}");
            return;
        }

        if (!string.IsNullOrEmpty(result.data))
        {
            try
            {
                byte[] bytes = System.Convert.FromBase64String(result.data);
                OnReceivedHandle?.Invoke(result.deviceId, bytes);
            }
            catch (System.FormatException)
            {
                Debug.LogError("Base64 解码失败：数据格式不正确");
            }
        }
    }
}