using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//每局结果(小格子) 视图
public class RecordItemView_SelfAddItem : MonoBehaviour
{
    //外部资源
    [SerializeField]
    private List<Sprite> records = new List<Sprite>();//格子图片 0,1,2分别对应闲,庄,和的图片,蓝,红,绿//需要排好序

    [Tooltip("图片在 Resources 文件夹中的路径,如果 Records 集合为空,会通过该路径加载图片")]
    public string recordPath = "record_new/record";

    //内部组件
    private Image image;//格子图片
    private List<GameObject> minigames = new List<GameObject>();//小游戏//需要排好序,0红:明牌,1绿:幸运座位,2黄:彩金

    //辅助变量
    private bool isInit = false;
    private int value = -1;
    public int Value
    {
        get => value;
    }



    private void Awake()
    {
        Init();
        ResetData();
    }

    public void SetValue(int curValue, int luckyId)
    {
        UpdateShow(curValue);
        UpdateMiniGame(luckyId);
    }

    public void ResetData()
    {
        value = -1;//重置数据
        //隐藏格子图片和小游戏,重置数据
        image.color = new Color(1, 1, 1, 0);//隐藏格子图片
        for (int i = 0; i < minigames.Count; i++)
        {
            minigames[i].SetActive(false);
        }
    }

    private void Init()
    {
        if (isInit) return;
        isInit = true;

        if (image == null)
        {
            image = transform.Find("icon").GetComponent<Image>();//这是 RecordItem 预制体的路径
        }
        if (minigames.Count <= 0)
        {
            //0红:明牌,1绿:幸运座位,2黄:彩金,
            var luckSeat = transform.Find("luckSeat");//这是 RecordItem 预制体的路径,幸运座位的父物体(需要排好序)
            for (int i = 0; i < luckSeat.childCount; i++)
                minigames.Add(luckSeat.GetChild(i).gameObject);
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
    }

    private void UpdateShow(int result)
    {
        if (value != -1)
        {
            Debug.LogError($"报错,该格子已经存储了数据,已经存储数据:{value},现在又想存的数据:{result}");
        }
        value = result;
        switch (result)
        {
            case 0:
            case 1:
            case 2:
                image.sprite = records[result];
                image.color = Color.white;
                //Debug.Log(image.transform.parent.gameObject.name.ToString());
                break;
            default:
                image.color = new Color(1, 1, 1, 0);//是其他结果,隐藏
                break;
        }
    }

    private void UpdateMiniGame(int luckyId)
    {
        for (int i = 0; i < minigames.Count; i++)
        {
            minigames[i].SetActive(false);
        }
        switch (luckyId)//旧的是乱的-kuarser 已修正
        {
            case 4:
            case 5:
            case 7:
                SetMinigamesActive(0);//幸运座位
                break;
            case 6:
                SetMinigamesActive(2);//明牌
                break;
            case 8:
                SetMinigamesActive(1);//彩金
                break;
        }
    }
    public void SetHeBiao()
    {
        SetMinigamesActive(3);//右上角和标
    }
    private void SetMinigamesActive(int index, bool isFlag = true)
    {
        if (minigames.Count > index)//下标越界检测
        {
            minigames[index].SetActive(isFlag);
        }
        else
        {
            Debug.LogError("可能数组越界了,可能数组没有一个值");
        }
    }
}
