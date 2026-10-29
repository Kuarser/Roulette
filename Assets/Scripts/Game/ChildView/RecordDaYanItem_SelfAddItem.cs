using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecordDaYanItem_SelfAddItem : MonoBehaviour
{
    private Image image;
    [Tooltip("图片在 Resources 文件夹中的路径,如果下面的 Records 集合为空,会通过该路径加载图片")]
    public string recordPath = string.Empty;
    [SerializeField]
    private List<Sprite> records = new List<Sprite>();

    private bool isInit = false;

    private int value = -1;
    public int Value
    {
        get => value;
        //set
        //{
        //    this.value = value;
        //    UpdateShow(value);
        //}
    }


    private void Awake()
    {
        InitView();
    }

    public void SetValue(int curValue)
    {
        UpdateShow(curValue);
    }

    public void ResetData()
    {
        UpdateShow(-1);
    }

    public void InitView()
    {
        if (isInit) return;
        isInit = true;

        if (image == null)
        {
            //image = transform.Find("Icon").GetComponent<Image>();//这是 RecordItem 预制体的路径
            image = transform.GetComponent<Image>();
        }

        if (records.Count <= 0)
        {
            for (int i = 0; i < 3; i++)
                ResMgr.Instance.LoadAsync<Sprite>(recordPath + i.ToString(), (sprite) =>
                {
                    if (sprite == null)
                        Debug.LogError($"在Resources 文件夹中找不到图片");
                    records.Add(sprite);
                });// Resources.LoadAsync 资源加载 icon 的sprite并存储,用于替换
        }
        ResetData();
    }

    private void UpdateShow(int result)
    {
        if (!isInit)
        {
            InitView();
        }
        switch (result)
        {
            case 0:
            case 1:
                //case 2:
                if (image == null)
                {
                    Debug.LogError("图片找不到");
                    //Init();
                }
                image.sprite = records[result];
                image.color = Color.white;//显示格子图片
                value = result;
                break;
            default:
                image.color = new Color(1, 1, 1, 0);//隐藏格子图片
                value = -1;
                break;
        }
    }
}
