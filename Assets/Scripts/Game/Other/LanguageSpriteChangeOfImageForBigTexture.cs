using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LanguageSpriteChangeOfImageForBigTexture : MonoBehaviour
{

    #region 手动挂载精灵图片的方式,不推荐,因为每新增加一个ui就需要把每种语言的图片都挂载一次,而且还要保证命名规范,比较麻烦

    public List<Sprite> sprites;//多语言精灵

    private Image image;
    private string spriteName;
    string[] tempStrs;
    string temp;

    private string SpriteName
    {
        get
        {
            if (spriteName == null)
            {
                Debug.LogError("报错,spriteName为空");
            }
            //tempStrs = spriteName.Split('_');//按照下划线分割字符串
            tempStrs = spriteName.Split(new char[] { '_' }, 2); //只分成两个部分
            temp = "Sprite" + tempStrs[tempStrs.Length - 1];
            return temp;
        }
    }

    private void Awake()
    {
        image = GetComponent<Image>();
        if (image.sprite != null)
        {
            spriteName = image.sprite.name;
            ChangeTo(Utils.GetLanguage(SpriteName));
        }
    }

    private void OnEnable()
    {
        EventCenter.Instance.AddEventListener(EventHandle.CHANGE_LANGUAGE, OnChangeLangugue);
        ChangeTo(Utils.GetLanguage(SpriteName));//有时候是一直隐藏的,所以打开时,通知已经完毕了,就没有接收到通知,就不知道变了多语言,所以需要自己再次确认是否更改多语言
    }

    private void OnDisable()
    {
        EventCenter.Instance.RemoveEventListener(EventHandle.CHANGE_LANGUAGE, OnChangeLangugue);
    }

    private void OnChangeLangugue()
    {
        ChangeTo(Utils.GetLanguage(SpriteName));
    }

    public void ChangeTo(string name)
    {
        //Debug.Log($"name:{name}");
        image.sprite = null;
        foreach (Sprite sprite in sprites)
        {
            if (sprite == null)
            {
                if (name != "SpriteHelpPanel02"&& name != "en_HelpPanel02")
                    Debug.LogError($"没有挂载{name} 的 {IOCanvasModel.Instance.curlanguage}语言的 精灵图片");
                continue;
            }
            if (sprite.name.Equals(name))//如果精灵名字和传入的名字相同,就替换图片//不比较了,直接用传入的名字去找
            {
                image.sprite = sprite;
                image.SetNativeSize();
            }
        }
        if (image.sprite == null)
        {
            if(name != "SpriteHelpPanel02" && name != "en_HelpPanel02")
            Debug.LogWarning($"{name}找不到对应的翻译,改用直接赋值");

            switch (IOCanvasModel.Instance.curlanguage)
            {
                case Language.cht://简体
                    if (sprites.Count > 0)
                        image.sprite = sprites[0];
                    break;
                case Language.en://英文
                    if (sprites.Count > 1)
                        image.sprite = sprites[1];
                    break;
                default:
                    //image.sprite = sprites[0];
                    Debug.LogError($"{IOCanvasModel.Instance.curlanguage}翻译还没有做");
                    break;
            }
            image.SetNativeSize();
        }
    }
    #endregion
}
