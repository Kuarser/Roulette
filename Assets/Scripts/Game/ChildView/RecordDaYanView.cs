
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecordDaYanView : MonoBehaviour
{
    private const int ROWINDEX_MAX = 5;
    private const int COLINDEX_MAX = 99;
    private int[] recordViewColNumDaYan = new int[COLINDEX_MAX + 1];//大眼路使用 ,  大路每列个数
    //private DaLu[] tempDaluDateArr = new DaLu[COLINDEX_MAX + 1];
    //private List<DaLu> tempDaluDateList = new List<DaLu>();
    private List<List<int>> tempDaluRecordDateList = new List<List<int>>();//存大路数据,没有右拐的版本

    private List<List<RecordDaYanItem>> recordDaYanItems = new List<List<RecordDaYanItem>>();
    private bool isInit;

    private List<int> curValueList = new List<int>();


    //大眼路的下标
    int colIndex = 0;
    int rowIndex = 0;
    int lastCol = 0;//0行的最后一列
    int colValue = -1;//大眼路最新的一个值,最后的一个值//lastOne
    private bool horizontalWays = false;//视图显示方式,是否右拐

    private int lastColForUseSpace;//最后一列,为了空行
    public int LastCol
    {
        get
        {
            if (lastColForUseSpace <= 18)
            {
                return 0;
            }
            return lastColForUseSpace - 18;
        }
    }
    public int LastCol2
    {
        get
        {
            if (lastColForUseSpace <= 26)
            {
                return 0;
            }
            return lastColForUseSpace - 26;
        }
    }


    //大眼路,小路,小强路,都差不多逻辑,通过修改下面来替换
    [Tooltip("预制件")]
    public GameObject recordDaYanItemPrefab;
    [Tooltip("间隔列,0是大眼路,1是小路,2是小强路")]
    public int intervalCol = 0;//0是大眼路,1是小路,2是小强路
    [Tooltip("起点行下标")]
    private int startRow = 2 - 1;
    [Tooltip("起点列下标")]
    private int startCol = 2 - 1;


    private void Awake()
    {
        Init();
    }

    void Init()
    {
        if (isInit) return;
        isInit = true;

        rowIndex = 0;
        colIndex = 0;
        lastCol = 0; lastColForUseSpace = 0;
        colValue = -1;

        startCol = startCol + intervalCol;

        int index = 0;
        var prefab = recordDaYanItemPrefab;
        for (int i = 0; i <= ROWINDEX_MAX; i++)
        {
            recordDaYanItems.Add(new List<RecordDaYanItem>());
            for (int j = 0; j <= COLINDEX_MAX; j++)
            {
                var item = Instantiate(prefab);
                item.transform.SetParent(transform);
                item.SetActive(true);
                item.transform.localScale = Vector3.one;
                item.transform.localPosition = Vector3.zero;
                item.gameObject.name = $"RecordItem_{index}";
                var itemScript = item.GetComponent<RecordDaYanItem>();
                itemScript.Value = -1;
                recordDaYanItems[i].Add(itemScript);
                index++;
            }
            //Debug.Log($"items:{items[i].Count}");
        }
    }

    public void UpdateRecord(int curValue)
    {
        if (recordDaYanItems == null || recordDaYanItems.Count <= 0)
        {
            Init();
            return;
        }
        if (colValue == -1)
        {
            //起手
            lastCol = rowIndex = colIndex = 0;
            colValue = curValue;
            lastColForUseSpace = 0;//存最后一列,换列的更要存
        }
        else
        {
            if (colValue == curValue)//上一个和当前这个相同,或者当前这个是绿色,可以在同一列显示
            {
                rowIndex++;//同方连续
                if (rowIndex > ROWINDEX_MAX)//一列存满了相同的,换列显示
                {
                    //换方开新列
                    rowIndex = ROWINDEX_MAX;
                    colIndex++;
                }
                else if (rowIndex <= ROWINDEX_MAX &&//※kuarser修改中rowIndex + 1->rowIndex
                    ((recordDaYanItems[rowIndex][colIndex].Value == curValue)
                    || recordDaYanItems[rowIndex][colIndex].Value == colValue)
                    || recordDaYanItems[rowIndex][colIndex].Value != -1
                    || horizontalWays)
                {
                    //换列
                    rowIndex--;
                    colIndex++;
                    horizontalWays = true;
                }
                lastColForUseSpace = colIndex;//存最后一列,换列的更要存
            }
            else //上一个和当前这个不相同,换列显示
            {
                rowIndex = 0;
                lastCol++;
                colValue = curValue;
                horizontalWays = false;
                colIndex = lastCol;
                if (lastColForUseSpace < colIndex)
                    lastColForUseSpace = colIndex; //存最后一列,不是最后一列就要更新为最后一列
            }
        }

        //Debug.Log($"rowIndex:{rowIndex},colIndex:{colIndex},颜色:{curValue}");
        if (rowIndex < recordDaYanItems.Count)
            if (colIndex < recordDaYanItems[rowIndex].Count)
                recordDaYanItems[rowIndex][colIndex].Value = curValue;
            else
                Debug.LogError("colIndex 越界了");
        else
            Debug.LogError("rowIndex 越界了");

    }

    public int GetRecordDaYanLast(int flag)
    {
        // flag: 0=庄(红), 1=闲(蓝), 2=和(绿)
        if (tempDaluRecordDateList == null || tempDaluRecordDateList.Count == 0)
            return -1;

        int lastColIdx = tempDaluRecordDateList.Count - 1;

        // 找到最后一个有值的列
        while (lastColIdx >= 0 && tempDaluRecordDateList[lastColIdx].Count == 0)
            lastColIdx--;

        if (lastColIdx < startCol)
            return -1;

        int lastRowIdx = tempDaluRecordDateList[lastColIdx].Count - 1;
        if (lastRowIdx < 0)
            return -1;

        int lastValue = tempDaluRecordDateList[lastColIdx][lastRowIdx];

        // 判断下一个值
        if (flag == lastValue)
        {
            // 相同，同行+1
            int nextRow = lastRowIdx + 1;
            int prevColIdx = lastColIdx - 1 - intervalCol;

            if (prevColIdx >= 0 && nextRow < tempDaluRecordDateList[prevColIdx].Count)
                return 0; // 前一列同一行有值
            return 1; // 前一列同一行无值
        }
        else
        {
            // 不同，列+1，行=0
            int prevColNum = (lastColIdx - 1 >= 0) ? recordViewColNumDaYan[lastColIdx - 1] : -1;
            int prevPrevColNum = (lastColIdx - 2 - intervalCol >= 0) ? recordViewColNumDaYan[lastColIdx - 2 - intervalCol] : -1;

            if (prevColNum == prevPrevColNum && prevColNum != -1)
                return 0; // 前两列个数一致
            return 1; // 前两列个数不一致
        }
    }

    public int GetRecordDaYanLast_old(int flag)//假设下一个值是闲,大眼路小路小强路的值;是庄,大眼路小路小强路的值//-1:没有值,0:红色,庄家,1:蓝色,闲家
    {
        //flag,1是庄,红色,0是闲,蓝色,2是和,绿色;
        Init();
        if (tempDaluRecordDateList == null || tempDaluRecordDateList.Count <= 0) return -1;

        int curValue = -1;
        int j = tempDaluRecordDateList.Count - 1;//最后一列
        int i = tempDaluRecordDateList[j].Count - 1;//最后一行

        while (tempDaluRecordDateList[j].Count <= 0)//如果最后一列没有值了,那么就退回前一列,直到找到有值的列为止,如果都没有值了,那么就直接退出
        {
            j--;
            if (j < 0)
            {
                return curValue;
            }
            i = tempDaluRecordDateList[j].Count - 1;
        }

        //一开始,如果不够行,不够列,那么直接退出
        if (j < startCol)
        {
            return curValue;
        }

        //现在最后一个值是庄,求下一个值是庄,相同;则是同一列,行+1;如果现在最后一个值是庄,求下一个值是闲,不同;则是列+1,行=0;
        //现在最后一个值是闲,求下一个值是庄,不同;则是列+1,行=0;如果现在最后一个值是闲,求下一个值是闲,相同;则是同一列,行+1;

        if (flag == tempDaluRecordDateList[j][i])
        {
            //相同,同一列,行+1;
            int tempI = i + 1;
            //比较假设的下一个值和对应的前一列同一行的值,如果前一列同一行有值,或者空了2行了,那么为0;否则为1
            if (tempI - 2 > tempDaluRecordDateList[j - 1 - intervalCol].Count
                || tempI < tempDaluRecordDateList[j - 1 - intervalCol].Count)//前一列同一行是否有值,或者是否是空了2行了
            {
                curValue = 0;
            }
            else
            {
                curValue = 1;
            }
        }
        else
        {
            //不同,列+1,行=0;
            int tempJ = j + 1;
            //如果假设的下一个值是第一行,比较前两列的个数是否一致
            if (recordViewColNumDaYan[tempJ - 1] == recordViewColNumDaYan[tempJ - 2 - intervalCol])//假设新增加了一列,比较前两列的个数
            {
                //同一高度,都有值,为红色;否则为蓝色
                curValue = 0;/*0表示红色,1表示蓝色*/
            }
            else
            {
                curValue = 1;
            }
        }
        return curValue;
    }

    public void UpdateRecordDaYan2()
    {
        Init();
        //SetRecord(items);//计算每一列的个数
        int curValue = -1;

        for (int j = startCol; j < tempDaluRecordDateList.Count; j++)//列
        {
            int tempSpaceRow = 0;//空行个数
            for (int i = 0; i < tempDaluRecordDateList[j].Count; i++)//行
            {
                if (j == startCol && i < startRow)
                {
                    i = startRow;
                    if (i >= tempDaluRecordDateList[j].Count)
                        break;
                }

                //if (tempDaluRecordDateList[j][i] == -1) break;
                //如果是第一行,比较前两列的个数是否一致
                if (i == 0)
                {
                    if (recordViewColNumDaYan[j - 1] == recordViewColNumDaYan[j - 2 - intervalCol])//比较前两列的个数
                    {
                        //同一高度,都有值,为红色;否则为蓝色
                        curValue = 0;/*0表示红色,1表示蓝色*/
                    }
                    else
                    {
                        curValue = 1;
                    }
                }
                else
                {
                    //if (tempDaluRecordDateList[j-1][i] != -1)
                    if (tempSpaceRow > 0 || i < tempDaluRecordDateList[j - 1 - intervalCol].Count)//前一列同一行是否有值,或者是否是空了2行了
                    {
                        curValue = 0;
                    }
                    else
                    {
                        tempSpaceRow++;
                        curValue = 1;
                    }
                }

                if (tempDaluRecordDateList[j][i] == 2)
                {
                    continue;
                }

                //存值
                UpdateRecord(curValue);
                curValueList.Add(curValue);
            }
        }

        //Debug.Log("%%%%%%%%%%%%%%%%%%%%%%%%%%%大眼路:没有自动换行");
        //for (int i = 0; i < curValueList.Count; i++)
        //{
        //    Debug.Log($"{i}-------------------{curValueList[i]}--------------------");
        //}
    }
    public void SetRecord(List<List<int>> items)//没用
    {
        Init();
        ResetRecord();//重置一下每列个数

        //
        for (int i = 0; i < items.Count; i++)
        {
            for (int j = 0; j < items[i].Count; j++)
            {
                int curValue = items[i][j];
                //存值
                UpdateRecord(curValue);
                //curValueList.Add(curValue);
            }
        }

    }
    public void SetRecord(List<List<RecordItemView>> items)//没用
    {
        Init();
        ResetRecord();//重置一下每列个数
        SetDaLu(items);//获取大路图的每列个数
        UpdateRecordDaYan2();

        //Debug.Log("------------------start大路图的每列个数---------------------");
        //for (int i = 0; i < recordViewColNumDaYan.Length; i++)
        //{
        //    if (recordViewColNumDaYan[i] <= 0) break;
        //    Debug.Log($"+++++++++++++大路图的第{i + 1}列个数:  {recordViewColNumDaYan[i]}");
        //}

        //Debug.Log("**************************************分割点*****************************************");
        //for (int i = 0; i < tempDaluDateArr.Length; i++)
        //{
        //    if (tempDaluDateArr[i].colTotalNum > 0)
        //        Debug.Log($"{i}列的长度:{tempDaluDateArr[i].colTotalNum},右拐点:({tempDaluDateArr[i].turnRightRow},{tempDaluDateArr[i].turnRightCol})");
        //}

        //for (int i = 0; i < tempDaluRecordDateList.Count; i++)
        //{
        //    string tempInt = "";
        //    for (int j = 0; j < tempDaluRecordDateList[i].Count; j++)
        //    {
        //        Debug.Log($"({i},{j})={tempDaluRecordDateList[i][j]}");
        //        tempInt += tempDaluRecordDateList[i][j].ToString() + ",";
        //    }
        //    Debug.Log($"一列:{tempInt}");
        //}
        //Debug.Log("------------------end大路图的每列个数---------------------");
    }

    public void ResetRecord()//没用
    {
        Init();
        for (int i = 0; i < recordViewColNumDaYan.Length; i++)
        {
            recordViewColNumDaYan[i] = -1;
        }
        rowIndex = 0;
        colIndex = 0;
        lastCol = 0; lastColForUseSpace = 0;
        colValue = -1;

        curValueList.Clear();
        for (int i = 0; i < recordDaYanItems.Count; i++)
        {
            for (int j = 0; j < recordDaYanItems[i].Count; j++)
            {
                recordDaYanItems[i][j].Value = -1;
            }
        }

        for (int i = 0; i < tempDaluRecordDateList.Count; i++)
        {
            for (int j = 0; j < tempDaluRecordDateList[i].Count; j++)
            {
                tempDaluRecordDateList[i].Clear();
            }
            tempDaluRecordDateList.Clear();
        }

        //for (int i = 0; i < tempDaluDateArr.Length; i++)
        //{
        //    tempDaluDateArr[i] = new DaLu();
        //}

    }

    private void SetDaLu(List<List<RecordItemView>> items)//没用
    {
        if (items == null) return;

        /* 逻辑:先找出大路一共有多少列,
         *      每列比较,找出该列的所有相同值,
         *      如果同一列没值了,说明这一列找完了;
         *      如果同一列的值不同了,说明是碰到了前面列的折角,那么向右开始比较,找出所有相同值,         
         *      最后值不同,说明找完了一列     
         *      
         * 有一点问题:因为隔一列就是相同值,向右开始比较时,可能会出现第二列太短,第一列和第三列又折角连接了,
         *              导致第一列会拿了第三列的值(是可能会出现,没有确定,可以修改右拐代码,不让第一列和第三列折角连接,就不会出现问题)
        */

        //for (int i = 0; i < tempDaluRecordDateList.Count; i++)//列
        //{
        //    for (int j = 0; j < tempDaluRecordDateList[i].Count; j++)//列里面的每行
        //    {
        //        tempDaluRecordDateList[i].Clear();
        //    }
        //    tempDaluRecordDateList.Clear();
        //}

        //tempDaluDateList.Clear();
        for (int j = 0; j < items[0].Count; j++)//每一列
        {
            tempDaluRecordDateList.Add(new List<int>());
            //第一行
            int tempValue = items[0][j].Value;//每一列的起始颜色,红色,蓝色?(防止,有一列存储到其他列去了),
            if (tempValue == -1)
            {
                //大路没有了
                break;
            }
            if (tempValue != 2)//是绿色球,不存储//TODO:
                tempDaluRecordDateList[j].Add(tempValue);//存储列的第一个值
            //Debug.Log($"({j},{0})存储的值,{tempValue}");
            //会有一种情况,这个第一个值是绿球怎么办?这里没有做处理,需要做处理//TODO:


            //其他行
            int colCount = 1;//每一列的数量//如果前面判断通过了,那么默认有1个
            int i = 1;//行号,行下标
            for (; i < items.Count; i++)//行
            {
                if (items[i][j].Value == 2)//是绿色球不存储//TODO:
                {
                    continue;
                }

                if (tempValue == 2)
                {
                    tempValue = items[i][j].Value;
                }

                if (items[i][j].Value != tempValue && items[i][j].Value != 2)
                {
                    //同一列,不是绿球,值却不同,说明是右拐的值
                    break;
                }
                colCount++;
                tempDaluRecordDateList[j].Add(items[i][j].Value);//存储同一列的其他值
                //Debug.Log($"({j},{i})存储的值,{items[i][j].Value}");
            }

            i--;//右拐了,所以退回一行
            int tempJ = j + 1;//临时列号,用于计算右拐的个数
            while (tempJ <= COLINDEX_MAX && (items[i][tempJ].Value == tempValue || items[i][tempJ].Value == 2))//右拐的也是列的个数,也累加计算
            {
                colCount++;
                tempDaluRecordDateList[j].Add(items[i][tempJ].Value);//存储同一列的其他值
                //Debug.Log($"({j},{i})存储的值,{items[i][tempJ].Value}");
                tempJ++;
            }
            recordViewColNumDaYan[j] = colCount;//存储大路图的每一列的个数
            //DaLu tempDaluDate = new DaLu { colTotalNum = colCount, isTurnRight = (tempJ != j + 1), turnRightRow = i, turnRightCol = j };
            //tempDaluDateArr[j] = tempDaluDate;
            //tempDaluDateList.Add(tempDaluDate);

        }
    }
}

class DaLu
{
    public int colTotalNum;//大路 每列数量
    public bool isTurnRight;//大路 是否右拐
    public int turnRightRow;//大路 右拐开始的行号,行下标
    public int turnRightCol;//大路 右拐开始的列号,列下标
}

