using System.Collections;
using System.Collections.Generic;
//using System.Security.Policy;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class LanguageSpriteChangeOfImage : MonoBehaviour
{
    #region 预加载图集的方式,推荐,只需要在图集里添加对应的多语言图片就行了,不需要每个ui都挂载一次,比较方便维护,但是需要注意命名规范,比如chs_HelpPanel02, en_HelpPanel02等
    public string spriteName;//精灵名字,需要和图集里对应的图片名字一致,比如chs_HelpPanel02, en_HelpPanel02等
    private Image image;
    string[] tempStrs;
    string tempImage;

    private string SpriteName
    {
        get
        {
            if (spriteName == null || spriteName.Equals(""))
            {
                if (image == null)
                    image = GetComponent<Image>();
                if (image.sprite != null)
                    spriteName = image.sprite.name;
                else
                {
                    spriteName = this.gameObject.name;
                    Debug.LogError("注意!没有给 spriteName 赋值");
                }
            }
            tempStrs = spriteName.Split(new char[] { '_' }, 2); //只分成两个部分
            tempImage = "Sprite" + tempStrs[tempStrs.Length - 1];
            return tempImage;
            //return spriteName;
        }
    }

    private void Awake()
    {
        image = GetComponent<Image>();
        OnChangeLanguage();
    }

    private void OnEnable()
    {
        EventCenter.Instance.AddEventListener(EventHandle.CHANGE_LANGUAGE, OnChangeLanguage);
        OnChangeLanguage();//有些挂载对象有时候是一直隐藏的,所以打开时,通知已经结束了,就没有接收到通知,就不知道变了多语言,所以需要自己再次确认是否更改多语言
    }

    private void OnDisable()
    {
        EventCenter.Instance.RemoveEventListener(EventHandle.CHANGE_LANGUAGE, OnChangeLanguage);
    }

    private void OnChangeLanguage()
    {
        image.sprite = LanguageSpriteManager.Instance.GetSprite4TPAtlas(Utils.GetLanguage(SpriteName));
        image.SetNativeSize();
    }
    #endregion

}
