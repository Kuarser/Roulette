using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecordDaYanItem : MonoBehaviour
{
    private Image image;
    public List<Sprite> records = new List<Sprite>();
    private bool isInit = false;

    private int value = -1;
    public int Value
    {
        get => value;
        set
        {
            this.value = value;
            UpdateShow(value);
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
        image = GetComponent<Image>();//这是 RecordItem2 预制体的路径
        //if (image == null)
        //{
        //    image = transform.Find("icon").GetComponent<Image>();//这是 RecordItem 预制体的路径
        //}
    }

    private void UpdateShow(int result)
    {
        Init();
        switch (result)
        {
            case 0:
            case 1:
            //case 2:
                image.sprite = records[result];
                image.color = Color.white;
                break;
            default:
                image.color = new Color(1, 1, 1, 0);
                break;
        }
    }
}
