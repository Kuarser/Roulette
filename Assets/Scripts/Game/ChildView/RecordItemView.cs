using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//每局结果(小格子) 视图
public class RecordItemView : MonoBehaviour
{
    private Image image;
    public List<Sprite> records = new List<Sprite>();
    private List<GameObject> minigames = new List<GameObject>();//对不上,增加了
    private bool isInit = false;

    private int value = -1;
    public int Value
    {
        get => value;
        set
        {
            this.value = value;
            UpdateShow(value);

            if (value == -1)//对不上,增加了
                UpdateMiniGame(0);
        }
    }

    private void Awake()
    {
        Init();
    }
    void Init()
    {
        if (isInit) return;
        isInit = true;
        //image = GetComponent<Image>();//这是 RecordItem2 预制体的路径
        if (image == null)
        {
            image = transform.Find("icon").GetComponent<Image>();//这是 RecordItem 预制体的路径

            //对不上,增加了下面3行
            minigames.Add(transform.Find("luckSeat/Green").gameObject);
            minigames.Add(transform.Find("luckSeat/Yellow").gameObject);
            minigames.Add(transform.Find("luckSeat/Red").gameObject);
        }
    }

    private void UpdateShow(int result)
    {
        Init();
        switch (result)
        {
            case 0:
            case 1:
            case 2:
                image.sprite = records[result];
                image.color = Color.white;
                break;
            default:
                image.color = new Color(1, 1, 1, 0);
                break;
        }
    }
    public void UpdateMiniGame(int luckyId)
    {
        Init();
        for (int i = 0; i < minigames.Count; i++)
        {
            minigames[i].SetActive(false);
        }
        switch (luckyId)
        {
            case 4:
            case 5:
            case 7:
                SetMinigamesActive(0);
                break;
            case 6:
                SetMinigamesActive(2);
                break;
            case 8:
                SetMinigamesActive(1);
                break;
        }
    }
    private void SetMinigamesActive(int index, bool isFlag = true)
    {
        if (minigames.Count > index)
        {
            minigames[index].SetActive(isFlag);
        }
        else
        {
            Debug.LogError("可能数组越界了,可能数组没有一个值");
        }
    }
}
