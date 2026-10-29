using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using ZXing.Common;
using ZXing;

public static class Utils
{
    static System.Random rand;
    public static void SetRandomSeed()
    {
        rand = new System.Random((int)System.DateTime.Now.Ticks);
    }
    public static int GetRandom(int from, int to)
    {
        if (rand == null) { SetRandomSeed(); }
        return rand.Next(from, to);
    }

    public static byte[] ToByteArray(string str)
    {
        byte[] send = System.Text.Encoding.UTF8.GetBytes(str);
        byte[] old = send;
        send = new byte[old.Length + 5];
        System.BitConverter.GetBytes(old.Length + 1).CopyTo(send, 0);
        send[4] = 0;
        old.CopyTo(send, 5);
        return send;
    }

    public static string LocalIP()
    {
        string AddressIP = string.Empty;
        string IP = "";
        IPAddress[] ips = Dns.GetHostAddresses(Dns.GetHostName());   //Dns.GetHostName()获取本机名Dns.GetHostAddresses()根据本机名获取ip地址组
        foreach (IPAddress ip in ips)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                IP = ip.ToString();  //ipv4
            }
        }
        return IP;
    }

    public static bool HasLocalIP()
    {
        IPAddress[] ips = Dns.GetHostAddresses(Dns.GetHostName());
        foreach (IPAddress ip in ips)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
            {
                if (ip.ToString().StartsWith("192.168."))
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 指定Post地址使用Get 方式获取全部字符串
    /// </summary>
    /// <param name="url">请求后台地址</param>
    /// <param name="dic">key:参数名,value:值</param>
    /// <returns></returns>
    public static string Post(string url, Dictionary<string, string> dic)
    {
        string result = "";
        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
        req.Method = "POST";
        req.ContentType = "application/x-www-form-urlencoded";
        #region 添加Post 参数
        StringBuilder builder = new StringBuilder();
        int i = 0;
        foreach (var item in dic)
        {
            if (i > 0)
                builder.Append("&");
            builder.AppendFormat("{0}={1}", item.Key, item.Value);
            i++;
        }
        byte[] data = Encoding.UTF8.GetBytes(builder.ToString());
        req.ContentLength = data.Length;
        using (Stream reqStream = req.GetRequestStream())
        {
            reqStream.Write(data, 0, data.Length);
            reqStream.Close();
        }
        #endregion
        HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
        Stream stream = resp.GetResponseStream();
        //获取响应内容
        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
        {
            result = reader.ReadToEnd();
        }
        return result;
    }

    public static long GetTimeStamp()
    {
        DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
        long timeStamp = (long)(DateTime.Now - startTime).TotalMilliseconds;
        return timeStamp;
    }

    public static int GetEnumLength<T>(T enumType)
    {
        string[] enumLength = Enum.GetNames(enumType.GetType());
        return enumLength.Length;
    }

    /// <summary>
    /// 按长度分割字符串，汉字按一个字符算
    /// </summary>
    /// <param name="SourceString"></param>
    /// <param name="Length"></param>
    /// <returns></returns>
    public static List<string> SplitLength(string SourceString, int Length)
    {
        List<string> list = new List<string>();
        for (int i = 0; i < SourceString.Trim().Length; i += Length)
        {
            if ((SourceString.Trim().Length - i) >= Length)
                list.Add(SourceString.Trim().Substring(i, Length));
            else
                list.Add(SourceString.Trim().Substring(i, SourceString.Trim().Length - i));
        }
        return list;
    }

    public static void FileWriteByCreate(string content, string outFilePath)
    {
        FileStream fs = new FileStream(outFilePath, FileMode.Create);
        StreamWriter sw = new StreamWriter(fs);
        sw.Write(content);
        sw.Flush();
        sw.Close();
        fs.Close();
    }

    public static void SaveObjectToJsonFile<T>(T data, string path)
    {
        TextWriter tw = new StreamWriter(path);
        if (tw == null)
        {
            Debug.LogError("Cannot write to " + path);
            return;
        }

        string jsonStr = JsonConvert.SerializeObject(data);
        tw.Write(jsonStr);
        tw.Flush();
        tw.Close();
    }

    public static T LoadObjectFromJsonFile<T>(string path)
    {
        TextReader reader = new StreamReader(path);
        if (reader == null)
        {
            Debug.LogError("Cannot find " + path);
            reader.Close();
            return default(T);
        }

        T data = JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
        if (data == null)
        {
            Debug.LogError("Cannot read data from " + path);
        }

        reader.Close();
        return data;
    }
    private static void ReadSprite()
    {


    }

    private static void ReadLanguage()
    {
        //如果是新的热更代码, 则读取持久化目录下的语言文件
        if (File.Exists(GetLanguagePath()))
        {
            string languageJson = File.ReadAllText(GetLanguagePath());
            IOCanvasModel.Instance.languageDic = JsonConvert.DeserializeObject<Dictionary<Language, Dictionary<string, string>>>(languageJson);
        }
        //旧的热更代码, 读取StreamingAssets目录下的语言文件
        else
        {
            string localPath;
            if (Application.platform == RuntimePlatform.Android)
                localPath = Application.streamingAssetsPath + "/language.json";
            else
                localPath = "file:///" + Application.streamingAssetsPath + "/language.json";
            UnityWebRequest www = UnityWebRequest.Get(localPath);
            var operation = www.SendWebRequest();
            while (!operation.isDone)
            { }
            if (www.result == UnityWebRequest.Result.ConnectionError
                || www.result == UnityWebRequest.Result.ProtocolError)
                Debug.LogError(www.error);
            else
                IOCanvasModel.Instance.languageDic = JsonConvert.DeserializeObject<Dictionary<Language, Dictionary<string, string>>>(www.downloadHandler.text);
        }

        //读取ab包的多语言文件
        //ResMgr.Instance.LoadAssetBundle("game", "language", (obj) =>
        //{
        //    var textAsset = obj as TextAsset;
        //    IOCanvasModel.Instance.languageDic = JsonConvert.DeserializeObject<Dictionary<Language, Dictionary<string, string>>>(textAsset.text);
        //});
    }

    private static string GetLanguagePath()
    {
        string path = Application.dataPath + "/StreamingAssets/language.json";
        if (!Application.isEditor)
            path = Application.persistentDataPath + "/language.json";
        return path;
    }

    public static string GetLanguage(string str)
    {
        if (IOCanvasModel.Instance.languageDic == null)
            ReadLanguage();
        IOCanvasModel.Instance.languageDic[IOCanvasModel.Instance.curlanguage].TryGetValue(str, out string temp);
        if (!string.IsNullOrEmpty(temp))
            return temp;

        //后台设置赋值有时候有多层 Utils.GetLanguage(str) ,如:Utils.GetLanguage(str=Utils.GetLanguage(str)),导致找不到对应的翻译,所以这个报错不准确,暂时不用了
        //if(!str.Equals("SpriteHelpPanel02"))
        //Debug.LogError($"没有找到 {str} 对应的{IOCanvasModel.Instance.curlanguage} 翻译,");
        return str;
    }

    public static Sprite GetSprit(string spriteName)//TODO:
    {
        if (IOCanvasModel.Instance.spriteDic == null)
            ReadSprite();
        IOCanvasModel.Instance.languageDic[IOCanvasModel.Instance.curlanguage].TryGetValue(spriteName, out string tempName);
        string tempSpriteName = tempName;
        IOCanvasModel.Instance.spriteDic[IOCanvasModel.Instance.curlanguage].TryGetValue(tempSpriteName, out Sprite temp);
        if (temp != null)
            return temp;
        Debug.LogError("为空,没有精灵图");
        return null;
    }

    public static string[] GetEnumNames(Type enumType)
    {
        string[] names = Enum.GetNames(enumType);
        for (int i = 0; i < names.Length; i++)
            names[i] = GetLanguage(names[i]);
        return names;
    }
    public static int times;
    public static int SBoxCoinCount(int curCount, int change)
    {
        curCount += change;
        //return curCount & 0xff;

        if (curCount > 255)
        {
            curCount -= 256;
            times++;
        }
        return curCount;
    }

    public static int UnityCoinCount(int curCount, int change)
    {
        curCount += change;
        if (curCount > 8191)
            curCount -= 8192;
        return curCount;
    }


    /// <summary>
    /// 生成二维码
    /// 经测试：能生成任意尺寸的正方形
    /// </summary>
    /// <param name="content"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public static Texture2D GenerateQRImageWithColor(string content, Color color, int width = 512, int height = 512)
    {
        BitMatrix bitMatrix;
        Texture2D texture = GenerateQRImageWithColor(content, width, height, color, out bitMatrix);
        return texture;
    }

    /// <summary>
    /// 生成二维码
    /// 经测试：能生成任意尺寸的正方形
    /// </summary>
    /// <param name="content"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    private static Texture2D GenerateQRImageWithColor(string content, int width, int height, Color color, out BitMatrix bitMatrix)
    {
        // 编码成color32
        MultiFormatWriter writer = new MultiFormatWriter();
        Dictionary<EncodeHintType, object> hints = new Dictionary<EncodeHintType, object>();
        //设置字符串转换格式，确保字符串信息保持正确
        hints.Add(EncodeHintType.CHARACTER_SET, "UTF-8");
        // 设置二维码边缘留白宽度（值越大留白宽度大，二维码就减小）
        hints.Add(EncodeHintType.MARGIN, 1);
        hints.Add(EncodeHintType.ERROR_CORRECTION, ZXing.QrCode.Internal.ErrorCorrectionLevel.M);
        //实例化字符串绘制二维码工具
        bitMatrix = writer.encode(content, BarcodeFormat.QR_CODE, width, height, hints);

        // 转成texture2d
        int w = bitMatrix.Width;
        int h = bitMatrix.Height;
        Texture2D texture = new Texture2D(w, h);
        for (int x = 0; x < h; x++)
        {
            for (int y = 0; y < w; y++)
            {
                if (bitMatrix[x, y])
                {
                    texture.SetPixel(y, x, color);
                }
                else
                {
                    texture.SetPixel(y, x, Color.white);
                }
            }
        }
        texture.Apply();
        return texture;
    }


    /// <summary>
    /// 压缩字符串
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string CompressString(string text)
    {
        // 将字符串转换为字节数组
        byte[] buffer = Encoding.UTF8.GetBytes(text);

        // 创建内存流用于存储压缩后的数据
        using (MemoryStream ms = new MemoryStream())
        {
            // 使用GZipStream压缩数据并写入内存流
            using (System.IO.Compression.GZipStream zipStream = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionLevel.Optimal, true))
            {
                zipStream.Write(buffer, 0, buffer.Length);
            }

            // 将内存流中的压缩数据转换为Base64字符串
            byte[] compressedData = ms.ToArray();
            return Convert.ToBase64String(compressedData);
        }
    }

    /// <summary>
    /// 解压字符串
    /// </summary>
    /// <param name="compressedText"></param>
    /// <returns></returns>
    public static string DecompressString(string compressedText)
    {
        // 将Base64字符串转换为字节数组
        byte[] compressedData = Convert.FromBase64String(compressedText);

        // 创建内存流用于读取压缩数据
        using (MemoryStream inputStream = new MemoryStream(compressedData))
        {
            // 创建内存流用于存储解压后的数据
            using (MemoryStream outputStream = new MemoryStream())
            {
                // 使用GZipStream解压数据
                using (System.IO.Compression.GZipStream zipStream = new System.IO.Compression.GZipStream(inputStream, System.IO.Compression.CompressionMode.Decompress))
                {
                    // 将解压后的数据复制到输出流
                    zipStream.CopyTo(outputStream);
                }

                // 将输出流中的数据转换为字符串
                byte[] decompressedData = outputStream.ToArray();
                return Encoding.UTF8.GetString(decompressedData);
            }
        }
    }
}
