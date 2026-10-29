using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.U2D;

public class LanguageSpriteManager : BaseManager<LanguageSpriteManager>
{
    #region 预加载 多语言图集,not use
    private Dictionary<Language, SpriteAtlas> atlasDict;

    void PreLoadAllAtlas4Language()
    {
        atlasDict = new Dictionary<Language, SpriteAtlas>();
        atlasDict.Add(Language.cht, Resources.Load<SpriteAtlas>("Atlas/CHT"));
        atlasDict.Add(Language.en, Resources.Load<SpriteAtlas>("Atlas/EN"));
    }

    // 获取当前语言的图片
    public Sprite GetSprite4Language(string spriteName)
    {
        return atlasDict[IOCanvasModel.Instance.curlanguage].GetSprite(spriteName);
    }
    #endregion




    #region 预加载 unity 自带的图集 SpriteAtlas
    // 已预加载的图集缓存
    private Dictionary<string, SpriteAtlas> _atlasDict = new Dictionary<string, SpriteAtlas>();
    // 所有要预加载的图集名称（可扩展多语言）
    private string[] _loadAtlasNames = {
        "1_main_Atlas",     // 你的图集名字（可加多个）
        "3_popcanvas_Atlas",
    };


    /// <summary>
    /// 预加载所有配置的图集
    /// </summary>
    public void PreLoadAllAtlas()
    {
        foreach (string atlasName in _loadAtlasNames)
        {
            SpriteAtlas atlas = Resources.Load<SpriteAtlas>($"Atlas/{atlasName}");

            if (atlas != null)
            {
                _atlasDict.Add(atlasName, atlas);
                Debug.Log($"图集预加载成功：{atlasName}");
            }
            else
            {
                Debug.LogError($"找不到图集：Atlas/{atlasName}");
            }
        }
    }

    /// <summary>
    /// 根据精灵名字 获取精灵（从预加载的图集中）
    /// </summary>
    public Sprite GetSprite(string spriteName)
    {
        foreach (var atlas in _atlasDict.Values)
        {
            Sprite sprite = atlas.GetSprite(spriteName);
            if (sprite != null)
                return sprite;
        }

        Debug.LogWarning($"找不到精灵：{spriteName}");
        return null;
    }
    #endregion



    #region 预加载 TexturePacker（codeandweb） 图集
    // 所有精灵缓存：key=图片名, value=精灵
    private Dictionary<string, Sprite> _spriteDict = new Dictionary<string, Sprite>();

    // 要预加载的图集名称（不用写后缀 .tpsheet / .png）
    public string[] atlasNames =
    {
        "Atlas/1_main_Atlas",      // 例子：ui.tpsheet + ui.png
        "Atlas/3_popcanvas_Atlas",    // 例子：icon.tpsheet + icon.png
    };


    /// <summary>
    /// 预加载所有配置的图集
    /// </summary>
    public void PreLoadAllAtlas4TPAtlas()
    {
        foreach (string atlasName in atlasNames)
        {
            LoadTPAtlas(atlasName);
        }

        Debug.Log($"✅ 预加载完成，总精灵数量：{_spriteDict.Count}");
    }

    /// <summary>
    /// 加载 .tpsheet 图集
    /// </summary>
    private void LoadTPAtlas(string atlasPath)
    {
        // 加载 图集数据 + 大图
        //TextAsset tpsheet = Resources.Load<TextAsset>($"{atlasPath}");
        //Texture2D tex = Resources.Load<Texture2D>($"{atlasPath}");
        TextAsset tpsheet = Resources.Load<TextAsset>($"{atlasPath}");
        Texture2D tex = Resources.Load<Texture2D>($"{atlasPath}");


        if (tpsheet == null || tex == null)
        {
            Debug.LogError($"❌ 图集缺失：{atlasPath}");
            return;
        }

        // 解析 .tpsheet
        StringReader reader = new StringReader(tpsheet.text);
        string line;

        int i = 0;
        while ((line = reader.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#") || line.StartsWith(":"))
                continue;

            //string rectLine = reader.ReadLine();
            //Debug.Log($"数量:{i++}," + line.Trim());
            string[] rectParts = line.Trim().Split(';');

            if (rectParts.Length < 11) continue;

            // 图片名
            //string spriteName = Path.GetFileNameWithoutExtension(line.Trim());
            string spriteName = rectParts[0].Trim();

            // 坐标 x y w h
            float x = float.Parse(rectParts[1]);
            float y = float.Parse(rectParts[2]);
            float w = float.Parse(rectParts[3]);
            float h = float.Parse(rectParts[4]);

            //中心点 pivot
            float pivotX = float.Parse(rectParts[5]);
            float pivotY = float.Parse(rectParts[6]);

            // 是否 旋转
            //bool rotated = bool.Parse(reader.ReadLine().Trim());
            //bool rotated = float.Parse(rectParts[7]); 
            bool rotated = false;

            //留白 padding
            float paddingLeft = float.Parse(rectParts[7]);
            float paddingTop = float.Parse(rectParts[8]);
            float paddingRight = float.Parse(rectParts[9]);
            float paddingBottom = float.Parse(rectParts[10]);


            // 生成精灵
            Rect rect = new Rect(x, /*tex.height - y - h*/y, w, h);
            //Debug.Log($"图片:{spriteName},宽:{x},高:{tex.height - y - h}");
            Sprite sprite = Sprite.Create(tex, rect, new Vector2(pivotX, pivotY), 100f);
            sprite.name = spriteName;


            // 存入字典
            if (!_spriteDict.ContainsKey(spriteName))
                _spriteDict.Add(spriteName, sprite);
        }
    }

    /// <summary>
    /// 外部调用：通过名字获取精灵（预加载后超快）
    /// </summary>
    public Sprite GetSprite4TPAtlas(string spriteName)
    {
        if (_spriteDict.TryGetValue(spriteName, out Sprite sprite))
            return sprite;

        Debug.LogWarning($"⚠️ 找不到精灵：{spriteName}");
        return null;
    }

    /// <summary>
    /// 外部调用：通过索引获取精灵（预加载后超快），适合按顺序命名的图集，如：icon_0, icon_1, ...，不推荐使用，容易出错，建议用名字获取
    /// </summary>
    /// <param name="spriteIndex"></param>
    /// <returns></returns>
    public Sprite GetSprite4TPAtlas(int spriteIndex)
    {
        foreach (var sprite in _spriteDict.Values)
        {
            if (spriteIndex-- == 0)
            {
                //Debug.Log($"宽:{sprite.rect.x},高:{sprite.rect.y}");
                return sprite;
            }
        }

        Debug.LogWarning($"⚠️ 找不到精灵：{spriteIndex}");
        return null;
    }

    /// <summary>
    /// 卸载当前图集，释放内存
    /// </summary>
    public void UnloadAtlas()
    {
        // 销毁所有动态创建的 Sprite
        foreach (var sprite in _spriteDict.Values)
        {
            if (sprite != null)
            {
                GameObject.Destroy(sprite);
            }
        }

        // 清空字典
        _spriteDict.Clear();

        // 强制释放内存
        Resources.UnloadUnusedAssets();
        System.GC.Collect();

        Debug.Log("图集已卸载，内存已释放");
    }

    #endregion

    #region 其他

    // 直接 Resources 文件夹 预加载 精灵图片
    //Resources.LoadAll<Sprite>("Sprites/CN");
    //Resources.LoadAll<Sprite>("Sprites/EN");

    // 直接 Resources 文件夹 预加载 精灵图集
    //Dictionary<LanguageType, SpriteAtlas> atlasDict = new Dictionary<LanguageType, SpriteAtlas>();
    //atlasDict.Add(LanguageType.CN, Resources.Load<SpriteAtlas>("Atlas/CN"));
    //atlasDict.Add(LanguageType.EN, Resources.Load<SpriteAtlas>("Atlas/EN"));
    //atlasDict.Add(LanguageType.TW, Resources.Load<SpriteAtlas>("Atlas/TW"));



    // 根据图片名字从 预加载精灵图集 拿当前语言精灵
    //public Sprite GetLangSprite(string spriteName)
    //{
    //return atlasDict[currentLang].GetSprite(spriteName);
    //}

    //private static Dictionary<string, Sprite> langSpriteDic;
    //void PreLoadLangSprite()
    //{
    //    langSpriteDic = new();
    //    Sprite[] all = Resources.LoadAll<Sprite>($"SpriteLang/{nowLang}");
    //    foreach (var s in all) langSpriteDic.Add(s.name, s);
    //}
    #endregion
}
