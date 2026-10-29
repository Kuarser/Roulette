using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//每轮结果(已经运行过的每局结果) 视图
public class RecordListView : MonoBehaviour
{
    [Tooltip("使用ab包加载格子")]
    public bool isUseAssetBundle = false;//使用ab包加载格子
    public GameObject recordItemPrefab;

    private const int ROW_MAX = 5;
    private const int COL_MAX = 99;//可能需要修改,//这是下标的,从0开始算的

    private int row;
    private int col;
    private int lastCol;        //露珠视图 下标为0行的最后一列

    private int lastColForUseSpace;//最后一列,为了空行
    public int LastCol
    {
        get
        {
            if (lastColForUseSpace <= 16)
                return 0;
            return lastColForUseSpace - 16;
        }
    }
    public int LastCol2
    {
        get
        {
            if (lastColForUseSpace <= 40)
                return 0;
            return lastColForUseSpace - 40;
        }
    }
    private int colValue;//露珠视图最后一次的值
    private bool horizontalWays = false;//视图显示方式,是否右拐
    private List<List<RecordItemView>> items = new List<List<RecordItemView>>();//大路
    public List<List<RecordItemView>> ITEMS
    {
        get
        {
            return items;
        }
    }

    //private string testStr = "[{\r\n\t\"game\": 1,\r\n\t\"round\": 0,\r\n\t\"value\": 53,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 2,\r\n\t\"round\": 0,\r\n\t\"value\": 59,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 3,\r\n\t\"round\": 0,\r\n\t\"value\": 68,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 4,\r\n\t\"round\": 0,\r\n\t\"value\": 18,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 5,\r\n\t\"round\": 0,\r\n\t\"value\": 54,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 6,\r\n\t\"round\": 0,\r\n\t\"value\": 77,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 7,\r\n\t\"round\": 0,\r\n\t\"value\": 54,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 8,\r\n\t\"round\": 0,\r\n\t\"value\": 43,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 9,\r\n\t\"round\": 0,\r\n\t\"value\": 67,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 10,\r\n\t\"round\": 0,\r\n\t\"value\": 74,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 11,\r\n\t\"round\": 0,\r\n\t\"value\": 77,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 12,\r\n\t\"round\": 0,\r\n\t\"value\": 77,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 13,\r\n\t\"round\": 0,\r\n\t\"value\": 45,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 14,\r\n\t\"round\": 0,\r\n\t\"value\": 95,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 15,\r\n\t\"round\": 0,\r\n\t\"value\": 61,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 16,\r\n\t\"round\": 0,\r\n\t\"value\": 29,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 17,\r\n\t\"round\": 0,\r\n\t\"value\": 50,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 18,\r\n\t\"round\": 0,\r\n\t\"value\": 43,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 19,\r\n\t\"round\": 0,\r\n\t\"value\": 51,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 20,\r\n\t\"round\": 0,\r\n\t\"value\": 34,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 21,\r\n\t\"round\": 0,\r\n\t\"value\": 22,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 22,\r\n\t\"round\": 0,\r\n\t\"value\": 61,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 23,\r\n\t\"round\": 0,\r\n\t\"value\": 17,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 24,\r\n\t\"round\": 0,\r\n\t\"value\": 45,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 25,\r\n\t\"round\": 0,\r\n\t\"value\": 59,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 26,\r\n\t\"round\": 0,\r\n\t\"value\": 45,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 27,\r\n\t\"round\": 0,\r\n\t\"value\": 61,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 28,\r\n\t\"round\": 0,\r\n\t\"value\": 76,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 29,\r\n\t\"round\": 0,\r\n\t\"value\": 35,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 30,\r\n\t\"round\": 0,\r\n\t\"value\": 44,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 31,\r\n\t\"round\": 0,\r\n\t\"value\": 76,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 32,\r\n\t\"round\": 0,\r\n\t\"value\": 50,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 33,\r\n\t\"round\": 0,\r\n\t\"value\": 95,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 34,\r\n\t\"round\": 0,\r\n\t\"value\": 58,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 35,\r\n\t\"round\": 0,\r\n\t\"value\": 49,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 36,\r\n\t\"round\": 0,\r\n\t\"value\": 34,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 37,\r\n\t\"round\": 0,\r\n\t\"value\": 43,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 38,\r\n\t\"round\": 0,\r\n\t\"value\": 24,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 39,\r\n\t\"round\": 0,\r\n\t\"value\": 53,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 40,\r\n\t\"round\": 0,\r\n\t\"value\": 43,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 41,\r\n\t\"round\": 0,\r\n\t\"value\": 19,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 42,\r\n\t\"round\": 0,\r\n\t\"value\": 65,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 43,\r\n\t\"round\": 0,\r\n\t\"value\": 74,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 44,\r\n\t\"round\": 0,\r\n\t\"value\": 56,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 45,\r\n\t\"round\": 0,\r\n\t\"value\": 59,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 46,\r\n\t\"round\": 0,\r\n\t\"value\": 17,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 47,\r\n\t\"round\": 0,\r\n\t\"value\": 29,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 48,\r\n\t\"round\": 0,\r\n\t\"value\": 53,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 49,\r\n\t\"round\": 0,\r\n\t\"value\": 54,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 50,\r\n\t\"round\": 0,\r\n\t\"value\": 41,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 51,\r\n\t\"round\": 0,\r\n\t\"value\": 51,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 52,\r\n\t\"round\": 0,\r\n\t\"value\": 66,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 53,\r\n\t\"round\": 0,\r\n\t\"value\": 50,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 54,\r\n\t\"round\": 0,\r\n\t\"value\": 43,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 55,\r\n\t\"round\": 0,\r\n\t\"value\": 36,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 56,\r\n\t\"round\": 0,\r\n\t\"value\": 66,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 57,\r\n\t\"round\": 0,\r\n\t\"value\": 52,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 58,\r\n\t\"round\": 0,\r\n\t\"value\": 18,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 59,\r\n\t\"round\": 0,\r\n\t\"value\": 33,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 60,\r\n\t\"round\": 0,\r\n\t\"value\": 27,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 61,\r\n\t\"round\": 0,\r\n\t\"value\": 51,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 62,\r\n\t\"round\": 0,\r\n\t\"value\": 72,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 63,\r\n\t\"round\": 0,\r\n\t\"value\": 95,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 64,\r\n\t\"round\": 0,\r\n\t\"value\": 70,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 65,\r\n\t\"round\": 0,\r\n\t\"value\": 95,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 66,\r\n\t\"round\": 0,\r\n\t\"value\": 52,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 67,\r\n\t\"round\": 0,\r\n\t\"value\": 75,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 68,\r\n\t\"round\": 0,\r\n\t\"value\": 69,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 69,\r\n\t\"round\": 0,\r\n\t\"value\": 59,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 70,\r\n\t\"round\": 0,\r\n\t\"value\": 18,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 71,\r\n\t\"round\": 0,\r\n\t\"value\": 77,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 72,\r\n\t\"round\": 0,\r\n\t\"value\": 33,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 73,\r\n\t\"round\": 0,\r\n\t\"value\": 52,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 74,\r\n\t\"round\": 0,\r\n\t\"value\": 44,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 75,\r\n\t\"round\": 0,\r\n\t\"value\": 42,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 76,\r\n\t\"round\": 0,\r\n\t\"value\": 54,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 77,\r\n\t\"round\": 0,\r\n\t\"value\": 51,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 78,\r\n\t\"round\": 0,\r\n\t\"value\": 40,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 79,\r\n\t\"round\": 0,\r\n\t\"value\": 54,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 80,\r\n\t\"round\": 0,\r\n\t\"value\": 67,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 81,\r\n\t\"round\": 0,\r\n\t\"value\": 61,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 82,\r\n\t\"round\": 0,\r\n\t\"value\": 60,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 83,\r\n\t\"round\": 0,\r\n\t\"value\": 95,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 84,\r\n\t\"round\": 0,\r\n\t\"value\": 50,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 85,\r\n\t\"round\": 0,\r\n\t\"value\": 58,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 86,\r\n\t\"round\": 0,\r\n\t\"value\": 19,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 87,\r\n\t\"round\": 0,\r\n\t\"value\": 34,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 88,\r\n\t\"round\": 0,\r\n\t\"value\": 18,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 89,\r\n\t\"round\": 0,\r\n\t\"value\": 53,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 90,\r\n\t\"round\": 0,\r\n\t\"value\": 73,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 91,\r\n\t\"round\": 0,\r\n\t\"value\": 20,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 92,\r\n\t\"round\": 0,\r\n\t\"value\": 43,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 93,\r\n\t\"round\": 0,\r\n\t\"value\": 50,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 94,\r\n\t\"round\": 0,\r\n\t\"value\": 65,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 95,\r\n\t\"round\": 0,\r\n\t\"value\": 37,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 96,\r\n\t\"round\": 0,\r\n\t\"value\": 24,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 97,\r\n\t\"round\": 0,\r\n\t\"value\": 95,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 98,\r\n\t\"round\": 0,\r\n\t\"value\": 56,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 99,\r\n\t\"round\": 0,\r\n\t\"value\": 38,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}, {\r\n\t\"game\": 100,\r\n\t\"round\": 0,\r\n\t\"value\": 27,\r\n\t\"luckyPlayerId\": 0,\r\n\t\"luckyId\": 0,\r\n\t\"extraData\": null\r\n}]";

    bool isInit = false;

    //public bool isAddDaYanLu = false;
    //public RecordDaYanView daYanView;



    private void Start()
    {
        if (recordItemPrefab == null)
        {
            isUseAssetBundle = true;
        }
        init();
    }
    void init()
    {
        if (isInit) return;
        isInit = true;
        //Debug.Log($"item.name:{this.transform.name}");
        row = 0;
        col = 0;
        lastCol = 0; lastColForUseSpace = 0;
        colValue = -1;

        int index = 0;

        //for (int i = 0; i <= ROW_MAX; i++)//旧的,需要去 unity UI界面 上手动挂载格子
        //{
        //    items.Add(new List<RecordItemView>());
        //    for (int j = 0; j <= COL_MAX; j++)
        //    {
        //        items[i].Add(transform.GetChild(index).GetComponent<RecordItemView>());
        //        index++;
        //    }
        //}

        if (isUseAssetBundle)
        {
            //ResMgr.Instance.LoadAssetBundle("game", "RecordItem", (obj) =>
            //{
            //    //新的,代码自动实例化 ab包预制件 并挂载格子
            //    var prefab = obj;
            //    for (int i = 0; i <= ROW_MAX; i++)
            //    {
            //        items.Add(new List<RecordItemView>());
            //        for (int j = 0; j <= COL_MAX; j++)
            //        {
            //            var item = Instantiate(prefab) as GameObject;
            //            item.transform.SetParent(transform);
            //            item.transform.localScale = Vector3.one;
            //            item.transform.localPosition = Vector3.zero;
            //            item.gameObject.name = $"RecordItem_{index}";
            //            var itemScript = item.GetComponent<RecordItemView>();
            //            itemScript.Value = -1;
            //            items[i].Add(itemScript);
            //            index++;
            //        }
            //        // Debug.Log($"items:{items[i].Count}");
            //    }
            //});
        }
        else
        {//新的,代码自动实例化 物体预制件 并挂载格子
            if (items.Count > 0)
                items.Clear();
            var prefab = recordItemPrefab;
            for (int i = 0; i <= ROW_MAX; i++)
            {
                items.Add(new List<RecordItemView>());
                for (int j = 0; j <= COL_MAX; j++)
                {
                    var item = Instantiate(prefab);
                    item.transform.SetParent(transform);
                    item.SetActive(true);
                    item.transform.localScale = Vector3.one;
                    item.transform.localPosition = Vector3.zero;
                    item.gameObject.name = $"RecordItem_{index}";
                    var itemScript = item.GetComponent<RecordItemView>();
                    itemScript.Value = -1;
                    items[i].Add(itemScript);
                    index++;
                }
                //Debug.Log($"items:{items[i].Count}");
            }
        }
    }

    /// <summary>
    /// 庄闲和结果 → 路单值：0红(庄) 1蓝(闲) 2绿(和)
    /// </summary>
    private int ExchangeValue(int result)
    {
        return result == 1 ? 0 : result == 2 ? 1 : 2;
    }

    public void SetRecord(List<RecordStruct> recordList)
    {
        init();

        if (Model.Instance.records == null) return;

        ResetRecord();
        for (int i = 0; i < recordList.Count; i++)
            UpdateRecord(recordList[i].result, recordList[i].luckyId);

        //Debug.LogError("执行了几次,为True?" + isAddDaYanLu);

        //if (isAddDaYanLu)
        //    daYanView.UpdateRecordDaYan2(items);

    }

    public void ResetRecord()
    {
        init();
        row = 0;
        col = 0;
        lastCol = 0; lastColForUseSpace = 0;
        colValue = -1;
        for (int i = 0; i < items.Count; i++)
            for (int j = 0; j < items[i].Count; j++)
                items[i][j].Value = -1;
    }
    public void UpdateRecords()
    {
        init();
        SetRecord(Model.Instance.records);
    }
    public void UpdateRecord(int value, int luckyId)
    {
        if (items == null || items.Count <= 0)
        {
            init();
            return;
        }

        var curValue = ExchangeValue(value);//获取值,红色,蓝色还是绿色
        if (colValue == -1)
        {
            //起手
            lastCol = row = col = 0;
            lastColForUseSpace = 0;
            colValue = curValue;
        }
        else
        {
            if (colValue == 2)//(上一把)最后一把是绿色
            {
                if (curValue != 2)//这一把不是绿色,保存新的值
                    colValue = curValue;
                row++;//同方连续
                if (row > ROW_MAX)//row:下标;ROW_MAX:下标//一列存满了横着换列存
                {
                    //同方连续
                    row = ROW_MAX;
                    col++;
                }
                else if (
                    row  <= ROW_MAX &&//※kuarser修改中row + 1->row
                    ((items[row ][col].Value == curValue)
                    || items[row ][col].Value == 2
                    || items[row ][col].Value == colValue)
                    || items[row][col].Value != -1
                    || horizontalWays)//有下一个,并且{下一个和当前这个是同一种颜色,或者下一个是绿色,或者下一个和上个是同一个颜色}, 换一列显示
                {
                    //受阻右拐
                    row--;
                    col++;
                    horizontalWays = true;
                }
                lastColForUseSpace = col;//存最后一列,换列的更要存
            }
            else
            {
                if (colValue == curValue
                    || curValue == 2)//上一个和当前这个相同,或者当前这个是绿色,可以在同一列显示
                {
                    row++;//同方连续
                    if (row > ROW_MAX)//一列存满了相同的,换列显示
                    {
                        //换方开新列
                        row = ROW_MAX;
                        col++;
                    }
                    else if (row  <= ROW_MAX &&//※kuarser修改中row + 1->row
                        ((items[row ][col].Value == curValue)
                        || items[row ][col].Value == 2
                        || items[row ][col].Value == colValue)
                        || items[row][col].Value != -1
                        || horizontalWays)
                    {
                        //换列
                        row--;
                        col++;
                        horizontalWays = true;
                    }
                    lastColForUseSpace = col;//存最后一列,换列的更要存
                }
                else //上一个和当前这个不相同,换列显示
                {
                    row = 0;
                    lastCol++;
                    colValue = curValue;
                    horizontalWays = false;
                    col = lastCol;
                    if (lastColForUseSpace < col)
                        lastColForUseSpace = col; //存最后一列,不是最后一列就要更新为最后一列
                }
            }
        }
        if (row < items.Count && col < items[row].Count
            && row >= 0 && col >= 0)
        {
            items[row][col].Value = curValue;
            items[row][col].UpdateMiniGame(luckyId);//对不上,增加了
        }
        else
        {
            Debug.LogError($"为什么会越界?{row},{col},{items?.Count},{items?[row]?.Count}");
        }
    }
}
