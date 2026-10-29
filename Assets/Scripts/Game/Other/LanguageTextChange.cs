using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LanguageTextChange : MonoBehaviour
{
    
    [SerializeField]
    private string textName;
    public string TextName
    {
        get
        {
            textName ??= this.gameObject.name;
            return IOCanvasModel.Instance.curlanguage switch
            {
                //锟斤拷锟斤拷
                Language.cht => "cht_" + textName,
                //英锟斤拷
                Language.en => "en_" + textName,
                //锟斤拷锟斤拷
                _ => textName,
            };
            //return IOCanvasModel.Instance.curlanguage == Language.en ? $"en_{textName}" : $"ch_{textName}";
        }
    }
    Text text;


    void Awake()
    {
        text = GetComponent<Text>();
        textName ??= this.gameObject.name;
    }

    // Start is called before the first frame update
    void Start()
    {
        OnChangeLangugue();
    }

    private void OnEnable()
    {
        EventCenter.Instance.AddEventListener(EventHandle.CHANGE_LANGUAGE, OnChangeLangugue);
        text.text = Utils.GetLanguage(textName);
    }

    private void OnDisable()
    {
        EventCenter.Instance.RemoveEventListener(EventHandle.CHANGE_LANGUAGE, OnChangeLangugue);
    }

    private void OnChangeLangugue()
    {
        //锟斤拷锟斤拷一:
        //switch (IOCanvasModel.Instance.curlanguage)
        //{
        //    case Language.cht://锟斤拷锟斤拷
        //        text.text = Utils.GetLanguage("cht_" + textName);
        //        break;
        //    case Language.chs://锟斤拷锟斤拷
        //        text.text = Utils.GetLanguage("chs_" + textName);
        //        break;
        //    case Language.en://英锟斤拷
        //        text.text = Utils.GetLanguage("en_" + textName);
        //        break;
        //    default:
        //        Debug.LogError($"{IOCanvasModel.Instance.curlanguage}锟斤拷锟诫还没锟斤拷锟斤拷");
        //        break;
        //}

        //锟斤拷锟斤拷锟斤拷:
        //text.text = Utils.GetLanguage(TextName);

        text.text = Utils.GetLanguage(textName);
    }
}
