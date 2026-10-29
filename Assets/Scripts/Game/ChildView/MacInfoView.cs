using SBoxApi;
using UnityEngine;
using UnityEngine.UI;

public class MacInfoView : MonoBehaviour
{
    private Text versionText;

    private Button settingBtn;
    private Text settingBtnText;

    private Button settingBtn2;
    // private Text settingBtn2Text;

    // private Button settingBtn3;
    // private Text settingBtn3Text;

    // private Button settingBtn4;
    // private Text settingBtn4Text;

    private Button accountBtn;
    private Text accountBtnText;


    private void Awake()
    {
        //Debug.Log("macInfoView 执行了 Awake");
        versionText = transform.Find("VersionText").GetComponent<Text>();
        settingBtn = transform.Find("settingBtn").GetComponent<Button>();
        settingBtnText = settingBtn.transform.Find("Text").GetComponent<Text>();
        settingBtn2 = transform.Find("settingBtn2").GetComponent<Button>();
        // settingBtn2Text = settingBtn2.transform.Find("Text").GetComponent<Text>();
        // settingBtn3 = transform.Find("settingBtn3").GetComponent<Button>();
        // settingBtn3Text = settingBtn3.transform.Find("Text").GetComponent<Text>();
        // settingBtn4 = transform.Find("settingBtn4").GetComponent<Button>();
        // settingBtn4Text = settingBtn4.transform.Find("Text").GetComponent<Text>();
        accountBtn = transform.Find("accountBtn").GetComponent<Button>();
        accountBtnText = accountBtn.transform.Find("Text").GetComponent<Text>();
    }

    private void Start()
    {
        settingBtn.onClick.AddListener(() =>
        {
            Controller.Instance.view.OnSettingBtnClick();
        });
        settingBtn2.onClick.AddListener(() =>
        {
            Controller.Instance.view.OnSettingBtn2Click();
        });
        // settingBtn3.onClick.AddListener(() =>
        // {
        //     Controller.Instance.view.OnSettingBtn3Click();
        // });
        // settingBtn4.onClick.AddListener(() =>
        // {
        //     Controller.Instance.view.OnSettingBtn4Click();
        // });
        accountBtn.onClick.AddListener(() =>
        {
            Controller.Instance.view.OnBillBtnClick();
        });

        EventCenter.Instance.AddEventListener(EventHandle.CHANGE_LANGUAGE, RefreshText);
    }

    private void OnEnable()
    {
        versionText.text = $"{IOCanvasModel.Instance.CfgData.MachineId},Version: {PlayerPrefs.GetString("CurVersion", "1.0.0")}";
        settingBtnText.text = Utils.GetLanguage("SettingBtn");
        // settingBtn2Text.text = Utils.GetLanguage("SettingBtn");
        // settingBtn3Text.text = Utils.GetLanguage("SettingBtn");
        // settingBtn4Text.text = Utils.GetLanguage("SettingBtn");
        accountBtnText.text = Utils.GetLanguage("AccountBtn");
        EventCenter.Instance.AddEventListener<SBoxConfData>(SBoxEventHandle.SBOX_READ_CONF, OnReadConfig);
    }

    private void RefreshText()
    {
        settingBtnText.text = Utils.GetLanguage("SettingBtn");
        // settingBtn2Text.text = Utils.GetLanguage("SettingBtn");
        // settingBtn3Text.text = Utils.GetLanguage("SettingBtn");
        // settingBtn4Text.text = Utils.GetLanguage("SettingBtn");
        accountBtnText.text = Utils.GetLanguage("AccountBtn");
    }

    private void OnReadConfig(SBoxConfData sBoxConfData)
    {
        versionText.text = $"{sBoxConfData.MachineId},Version: {PlayerPrefs.GetString("CurVersion", "1.0.0")}";
    }

    private void OnDisable()
    {
        EventCenter.Instance.RemoveEventListener<SBoxConfData>(SBoxEventHandle.SBOX_READ_CONF, OnReadConfig);
    }
}
