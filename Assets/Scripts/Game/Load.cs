using IOT;
using Newtonsoft.Json;
using SBoxApi;
using Spine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using static UsbTouchDataParser;

/// <summary>
/// 游戏加载流程控制器，负责初始化硬件、读取配置、注册事件监听、预加载资源等 
/// </summary>
public class Load : MonoBehaviour
{
    // 游戏描述文本，用于显示当前加载进度
    private Text descText;

    /// <summary>
    /// 启动时初始化，注册事件监听，开始硬件检测和SBox初始化
    /// </summary>
    private void Start()
    {

        // 获取用于显示描述的UI文本组件
        descText = GameObject.Find("DescText")?.GetComponent<Text>();

        // 注册SBox配置读取事件监听
        EventCenter.Instance.AddEventListener<SBoxConfData>(SBoxEventHandle.SBOX_READ_CONF, OnReadConfig);

        // 注册SBox版本号事件监听，将版本号存入Model
        EventCenter.Instance.AddEventListener<string>(SBoxEventHandle.SBOX_IDEA_VERSION, (version) => { IOCanvasModel.Instance.sBoxVersion = version; });

        // 注册IOT设备注册事件监听，处理物联网设备注册
        EventCenter.Instance.AddEventListener<List<QrCodeData>>(IOTEventHandle.REGISTER_DEV, OnIOTRegisterDev);

        // 显示硬件检测提示
        if (descText != null)
            descText.text = "正在检测硬件...";

        // 初始化SBox硬件，连接指定IP
        SBoxInit.Instance.Init("192.168.3.191", OnInitSBox);
    }

    /// <summary>
    /// SBox初始化完成回调，开始读取配置和版本号
    /// </summary>
    private void OnInitSBox()
    {
        if (descText != null)
            descText.text = "正在读取游戏配置...";
        ReadScreenConfig();
    }

    private void ReadScreenConfig()
    {
        string path = string.Empty;
        if (Application.isEditor)
            path = Application.dataPath + "/StreamingAssets/screenConfig.json";
        else
            path = Application.persistentDataPath + "/screenConfig.json";
        if (File.Exists(path))
        {
            List<TouchAdaptationJson> screenJsonConfigs = JsonConvert.DeserializeObject<List<TouchAdaptationJson>>(File.ReadAllText(path));
            ScreenModel.Instance.InitScreenConfig(screenJsonConfigs);
        }

        // 读取SBox配置
        SBoxIdea.ReadConf();
        // 读取SBox版本号
        SBoxIdea.Version();

        //读取历史记录数据库
        SQLite.Instance.Init();
    }

    /// <summary>
    /// SBox配置读取完成事件回调，保存配置到Model并初始化奖池配置
    /// </summary>
    /// <param name="sBoxConfData">读取到的SBox配置数据</param>
    private void OnReadConfig(SBoxConfData sBoxConfData)
    {
        // 保存配置数据到Model
        IOCanvasModel.Instance.CfgData = sBoxConfData;
        IOCanvasModel.Instance.netSwitch = PlayerPrefs.GetInt("NetSwitch", 0);
        IOCanvasModel.Instance.tempNetSwitch = IOCanvasModel.Instance.netSwitch;

        // 初始化奖池配置
        var jackpotCfg = new JackpotConfig
        {
            jackpotSwitch = PlayerPrefs.GetInt("JackpotSwitch", 1),
            betPercent = PlayerPrefs.GetInt("JackpotBetPercent", 100),
            jpPercent = PlayerPrefs.GetInt("JackpotPercent", 5)
        };

        IOCanvasModel.Instance.JackpotCfg = jackpotCfg;

        // 读取打印次数设置
        IOCanvasModel.Instance.PrintCount = PlayerPrefs.GetInt("printCount", 0);
        IOCanvasModel.Instance.PrintCountMax = PlayerPrefs.GetInt("printCountMax", 5);

        //IOCanvasModel.Instance.JackpotCfg.jackpotSwitch = 0;//测试后台设置关闭联网彩金

        //配置 BetSwitch（数据来源：算法卡 20002 命令，显示个数 = 非0值个数）
        IOCanvasModel.Instance.UpdateBetSwitchFromConf(sBoxConfData.BetSwitch);

        Debug.Log($"配置的BetSwitch:{IOCanvasModel.Instance.BetSwitchCount},{JsonConvert.SerializeObject(IOCanvasModel.Instance.BetSwitchList)}");
        EventCenter.Instance.EventTrigger(EventHandle.UPDATE_BET_SWITCH);

        //if (IOCanvasModel.Instance.BetSwitchCount == 0)
        //{
        //    IOCanvasModel.Instance.BetSwitchCount = 6;
        //}
        //if (IOCanvasModel.Instance.BetSwitchList == null || IOCanvasModel.Instance.BetSwitchList.Length == 0)
        //{
        //    Debug.LogError("未配置本地BetSwitch,重新配置");
        //    IOCanvasModel.Instance.BetSwitchList = new int[IOCanvasModel.betSwitch_Limit_Max];

        //    //如果没有配置本地BetSwitch,默认全部开启
        //    for (int i = 0; i < IOCanvasModel.Instance.BetSwitchList.Length; i++)
        //        IOCanvasModel.Instance.BetSwitchList[i] = 1;
        //}

        // 配置读取完成后续处理
        OnSBoxReadConfigComplete();
    }

    /// <summary>
    /// IOT设备注册事件回调，保存二维码数据并标记已连接
    /// </summary>
    /// <param name="qrCodeDatas">注册的二维码数据列表</param>
    private void OnIOTRegisterDev(List<QrCodeData> qrCodeDatas)
    {
        IOTModel.Instance.qrCodeDatas = qrCodeDatas;
        IOTModel.Instance.LinkIOT = true;
    }

    /// <summary>
    /// SBox配置读取完成后的流程，包括语言设置、网络初始化、预加载音效等
    /// </summary>
    private void OnSBoxReadConfigComplete()
    {
        // 设置当前语言
        IOCanvasModel.Instance.CurLanguage = Language.cht;//(Language)(Enum.Parse(typeof(Language), PlayerPrefs.GetInt("CurLanguage", 0).ToString()));

        // 移除配置读取事件监听
        EventCenter.Instance.RemoveEventListener<SBoxConfData>(SBoxEventHandle.SBOX_READ_CONF, OnReadConfig);

        // 初始化网络消息控制器
        NetMessageController.Instance.Init();

        // 如果后台设置netJackpot 开关为开,则作为分机连接彩金主机
        if (IOCanvasModel.Instance.jackpotCfg.jackpotSwitch == (int)IOJackpotSwitch.JackpotSwitchOn)
            NetMgr.Instance.SetNetAutoConnect(false);

        // 预加载音效资源
        PreloadSound();

        // 预加载多语言精灵图片
        PreloadSprite();//TODO:

        // 启动协程，等待玩家信息并加载主场景
        StartCoroutine(WaitForPlayerInfo());
    }

    /// <summary>
    /// 协程：获取玩家信息并异步加载主场景和资源
    /// </summary>
    private IEnumerator WaitForPlayerInfo()
    {
        // 从SBox获取玩家信息
        PlayerMgr.Instance.GetPlayerInfosFromSBox();
        yield return null;

        // 更新描述文本
        if (descText != null)
            descText.text = "正在载入...";

        //进度条加载完毕触发器,不能用,会出现依赖问题,base ab 包 依赖于assembly_cSharp
        //EventCenter.Instance.EventTrigger(EventHandle.UPDATE_LOAD_SLIDER, 1f);
        //yield return new WaitForSeconds(1f);
        GameObject.Find("main/Slider").GetComponent<Slider>().value = 1f;
        GameObject.Find("main/Slider/Handle Slide Area/ProgressText").GetComponent<Text>().text = "100%";

        // 异步加载游戏场景及相关资源
        ScenesMgr.Instance.LoadSceneAsyn("game", () =>
        {
            // 加载UI资源并初始化控制器
            ResMgr.Instance.LoadAssetBundle("game", "ui", (obj) =>
            {
                Controller.Instance.Init();
                Instantiate(obj);
            });
            // 加载macInfo资源
            ResMgr.Instance.LoadAssetBundle("game", "macInfo", (obj) =>
            {
                GameObject macInfo = (GameObject)Instantiate(obj);
                ResMgr.Instance.LoadAssetBundle("game", "DoubleScreenControll", (obj) =>
                {
                    Instantiate(obj, macInfo.transform);
                });
            });
        });
    }
    private void OnSBoxVersion(string version)
    {
        IOCanvasModel.Instance.sBoxVersion = version;
    }

    /// <summary>
    /// 预加载游戏所需音效资源
    /// </summary>
    private void PreloadSound()
    {
        // 读取音量设置
        IOCanvasModel.Instance.SoundVolumScale = PlayerPrefs.GetInt("soundVolumScale", 100);

        // 读取背景音乐设置
        IOCanvasModel.Instance.BgmSwitch = PlayerPrefs.GetInt("bgmSwitch", 1);
        IOCanvasModel.Instance.BgmSelectIndex = PlayerPrefs.GetInt("bgmSelectIndex", 3);

        // 预加载普通背景音乐
        for (int i = 0; i < 3; i++)
            AudioManager.Instance.GetClipByName($"bgm/normalBgm{i}");

        // 预加载奖励背景音乐
        for (int i = 0; i < 2; i++)
            AudioManager.Instance.GetClipByName($"bgm/rewardBgm{i}");

        // 预加载音效
        AudioManager.Instance.GetClipByName("sfx/coinEff");
        AudioManager.Instance.GetClipByName("sfx/countDown");

        // 预加载语音资源
        for (int i = 1; i < 11; i++)
            AudioManager.Instance.GetClipByName($"voice/ch_{i}");
        AudioManager.Instance.GetClipByName("voice/ch_allWin");
        AudioManager.Instance.GetClipByName("voice/ch_coinParty");
        AudioManager.Instance.GetClipByName("voice/ch_double");
        AudioManager.Instance.GetClipByName("voice/ch_luckySeat");
        for (int i = 1; i < 5; i++)
            AudioManager.Instance.GetClipByName($"voice/ch_result{i}");
        AudioManager.Instance.GetClipByName("voice/ch_showCard");
        AudioManager.Instance.GetClipByName("voice/ch_times");
    }

    private void PreloadSprite()//TODO:
    {
        LanguageSpriteManager.Instance.PreLoadAllAtlas4TPAtlas();
    }
}