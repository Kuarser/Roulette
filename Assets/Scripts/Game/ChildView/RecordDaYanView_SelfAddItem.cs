using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class RecordDaYanView_SelfAddItem : MonoBehaviour
{
    private const int ROWINDEX_MAX = 5;
    private const int COLINDEX_MAX = 99;
    private int col_max = 2;//如果是竖着排列,一开始先显示20列
    private int addColCount = 4;//不够时,一次增加4列,最多不超过 COL_MAX 列


    private int[] recordViewColNumDaYan = new int[COLINDEX_MAX + 1];//大路每列个数
    private List<List<int>> tempDaluRecordDateList = new List<List<int>>();//存大路数据,没有右拐的版本
    private int daluCurCol;//大路数据当前值显示所在的列
    private int daluCurRow;//大路数据当前值显示所在的行
    private int daluCurValue;//大路数据当前值,如果是特殊值,继承上一个当前值
    private int daluDataCount;//大路数据个数

    private List<List<RecordDaYanItem_SelfAddItem>> recordDaYanItems = new List<List<RecordDaYanItem_SelfAddItem>>();//大眼路格子视图
    //private List<int> curValueList = new List<int>();

    int colIndex = 0;//当前格子的行列下标
    int rowIndex = 0;//当前格子的列下标
    int lastCol = 0;//0行的最后一列
    int colValue = -1;//大眼路最新的一个值,最后的一个值//lastOne
    private bool horizontalWays = false;//视图显示方式,是否右拐
    private bool isInit;

    private int lastColForUseSpace;//最后一列,为了空行
    public int LastCol
    {
        get
        {
            if (lastColForUseSpace <= 27)
                return 0;
            return lastColForUseSpace - 27;
        }
    }
    public int LastCol2
    {
        get
        {
            if (lastColForUseSpace <= 27)
                return 0;
            return lastColForUseSpace - 27;
        }
    }


    //大眼路,小路,小强路,都差不多逻辑,通过修改下面来替换
    [Tooltip("预制件")]
    public GameObject recordDaYanItemPrefab;
    [Tooltip("间隔列,0是大眼路,1是小路,2是小强路")]
    public int intervalCol = 0;//0是大眼路,1是小路,2是小强路
    [Tooltip("起点行下标")]
    private int startRow = 1;
    [Tooltip("起点列下标")]
    private int startCol = 1;


    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        if (isInit) return;
        isInit = true;

        InitData();

        LoadAndGetItems();
    }

    private void InitData()
    {
        rowIndex = 0;
        colIndex = 0;
        lastCol = 0;
        lastColForUseSpace = 0;
        colValue = -1;
        //startCol = startCol + intervalCol;
        //Debug.LogError($"startCol:{startCol}");
        for (int i = 0; i < recordViewColNumDaYan.Length; i++)
        {
            recordViewColNumDaYan[i] = -1;
        }

        daluDataCount = 0;
        daluCurRow = 0;
        daluCurCol = 0;
    }

    private void LoadAndGetItems()
    {
        int index = 0;
        var prefab = recordDaYanItemPrefab;
        for (int j = 0; j <= col_max; j++)
        {
            recordDaYanItems.Add(new List<RecordDaYanItem_SelfAddItem>());
            for (int i = 0; i <= ROWINDEX_MAX; i++)
            {
                var item = Instantiate(prefab);
                item.transform.SetParent(transform);
                item.SetActive(true);
                item.transform.localScale = Vector3.one;
                item.transform.localPosition = Vector3.zero;
                item.gameObject.name = $"RecordItem_{index}";
                var itemScript = item.GetComponent<RecordDaYanItem_SelfAddItem>();
                //itemScript.Value = -1;
                recordDaYanItems[j].Add(itemScript);
                index++;
            }
            //Debug.Log($"items:{items[i].Count}");
        }
    }

    private void AddItems()//格子不够,新增加格子
    {
        string luName = intervalCol == 0 ? "大眼路" : intervalCol == 1 ? "小路" : intervalCol == 2 ? "小强路" : "报错,没有这个路";
        //Debug.LogError($"{luName}的格子不够,增加格子");
        int index = 0;
        for (int j = col_max + 1; j <= col_max + addColCount; j++)
        {
            recordDaYanItems.Add(new List<RecordDaYanItem_SelfAddItem>());
            for (int i = 0; i <= ROWINDEX_MAX; i++)
            {
                var item = Instantiate(recordDaYanItemPrefab);
                item.transform.SetParent(transform);
                item.SetActive(true);
                item.transform.localScale = Vector3.one;
                item.transform.localPosition = Vector3.zero;
                item.gameObject.name = $"RecordItem_{index}";
                var itemScript = item.GetComponent<RecordDaYanItem_SelfAddItem>();
                //itemScript.Value = -1;//初始化格子数据,显示空格子
                itemScript.InitView();
                recordDaYanItems[j].Add(itemScript);
                index++;
            }
        }
        col_max += addColCount;//增加4列,多加4*6 个格子
    }

    //更新显示界面
    private void ShowRecord(int curValue)
    {

        if (colValue == -1)//不可能为-1
        {
            //起手
            lastCol = rowIndex = colIndex = 0;
            colValue = curValue;
            lastColForUseSpace = 0;//存最后一列,换列的更要存
        }
        else
        {
            if (colValue == curValue)//上一个和当前这个相同,可以在同一列显示
            {
                rowIndex++;//同方连续
                if (rowIndex > ROWINDEX_MAX)//一列存满了相同的,换列显示
                {
                    //换方开新列
                    rowIndex = ROWINDEX_MAX;
                    colIndex++;
                }
                else if ((rowIndex <= ROWINDEX_MAX//※kuarser修改中rowIndex + 1->rowIndex
                    && recordDaYanItems[colIndex][rowIndex].Value != -1)
                    || recordDaYanItems[colIndex][rowIndex].Value != -1
                    || horizontalWays)//顶头折角
                {
                    //换列
                    rowIndex--;
                    colIndex++;
                    horizontalWays = true;
                }
                else if (colIndex - 1 >= 0//※kuarser修改中
                        && (recordDaYanItems[colIndex - 1][rowIndex].Value == curValue)
                        && (recordDaYanItems[colIndex - 1][rowIndex - 1].Value == -1)
                        )//判断显示的前一列同一行,数据的前两列及以上同一行是否是一样的值,有则折角(,导致分不清现在这个值是哪一列的)
                {
                    rowIndex--;//空一行
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

        if (colIndex >= col_max)//格子不够,增加格子,增加4列,多加4*6 个格子,直到满足显示需要的列数
        {
            AddItems();
        }
        if (colIndex >= recordDaYanItems.Count)//格子不够,增加格子,增加4列,多加4*6 个格子,直到满足显示需要的列数
        {
            AddItems();
            Debug.LogError("额外保险,如果用到了,反而有问题");
        }
        if(colIndex<0||colIndex>recordDaYanItems.Count)
        {
            Debug.LogError("col越界");
        }
        else if (rowIndex < 0 || rowIndex > recordDaYanItems[colIndex].Count)
        {
            Debug.LogError("row越界");
        }
            recordDaYanItems[colIndex][rowIndex].SetValue(curValue);
    }


    private void UpdateRecordDate()
    {
        //只计算当个大眼路的值

        int curValue = -1;

        int i = rowIndex + 1;
        int j = colIndex;

        if (i > ROWINDEX_MAX)
        {
            i = 0;
            j++;
        }

        if (j < startCol + intervalCol ||
        (j == startCol + intervalCol && i < startRow))//起始行列,跳过不处理,直接进入下一行循环
        {
            return;
        }

        //if (tempDaluRecordDateList[j][i] == 2)//和,绿色,不参与大眼路计算
        //{
        //    continue;
        //}

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
            //如果是其他行,比较前一列同一行是否有值,或者是否是空了2行了,如果有值或者空了2行了,则为红色;否则为蓝色
            if (tempDaluRecordDateList[j - 1 - intervalCol][i - 1] == -1 || i < tempDaluRecordDateList[j - 1 - intervalCol].Count)//前一列同一行是否有值,或者是否是空了2行了
            {
                curValue = 0;
            }
            else
            {
                curValue = 1;
            }
        }

        //存值
        ShowRecord(curValue);

    }

    //根据大路图计算大眼路数据
    private void UpdateRecordAllDatas()
    {
        //SetRecord(items);//计算每一列的个数
        int curValue = -1;

        for (int i = startCol + intervalCol; i < tempDaluRecordDateList.Count; i++)//列
        {
            int tempSpaceRow = 0;//空行个数
            for (int j = 0; j < tempDaluRecordDateList[i].Count; j++)//行
            {
                if (j < startRow)//起始行列,跳过不处理,直接进入下一行循环
                {
                    continue;
                }

                //if (tempDaluRecordDateList[j][i] == 2)//和,绿色,不参与大眼路计算
                //{
                //    continue;
                //}

                curValue = GetDayanRecord(i, ref tempSpaceRow, j);

                //存值
                ShowRecord(curValue);
                //curValueList.Add(curValue);
            }
        }
    }

    private int GetDayanRecord(int j, ref int tempSpaceRow, int i)
    {
        int curValue;
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
            //如果是其他行,比较前一列同一行是否有值,或者是否是空了2行了,如果有值或者空了2行了,则为红色;否则为蓝色
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

        return curValue;
    }

    public void ShowDayanRecord()
    {
        Model.Instance.CalculateOtherRoadData();

        int curValue = -1;
        switch (intervalCol)
        {
            case 0:
                for (int i = 0; i < Model.Instance.bigEyeRoadList.Count; i++)
                {
                    for (int j = 0; j < Model.Instance.bigEyeRoadList[i].Count; j++)
                    {
                        curValue = Model.Instance.bigEyeRoadList[i][j];
                        ShowRecord(curValue);
                    }
                }
                break;
            case 1:
                for (int i = 0; i < Model.Instance.smallRoadList.Count; i++)
                {
                    for (int j = 0; j < Model.Instance.smallRoadList[i].Count; j++)
                    {
                        //Debug.LogError($"小路数据:{Model.Instance.SmallRoadList[i][j]}");
                        curValue = Model.Instance.smallRoadList[i][j];
                        ShowRecord(curValue);
                    }
                }
                break;
            case 2:
                for (int i = 0; i < Model.Instance.dragonRoadList.Count; i++)
                {
                    for (int j = 0; j < Model.Instance.dragonRoadList[i].Count; j++)
                    {
                        //Debug.LogError($"小强路数据:{Model.Instance.dragonRoadList[i][j]}");
                        curValue = Model.Instance.dragonRoadList[i][j];
                        ShowRecord(curValue);
                    }
                }
                break;
            default: Debug.LogError($"没有这个路,intervalCol:{intervalCol}"); break;
        }
    }

    //设置 大眼路
    public void SetRecords(List<RecordStruct> recordList)
    {
        ResetRecords();//重置一下每列个数
        SetDaLu(recordList);//获取大路图的每列个数
        UpdateRecordAllDatas();

    }

    public void UpdateRecords(List<RecordStruct> recordList, int gameCount)
    {
        //Debug.LogError("1:");
        //LogDaLuData();//本地存储的大路数据
        //更新大路 数据
        UpdateDaLu(recordList, gameCount);
        //Debug.LogError("2:");
        //LogDaLuData();//本地存储的大路数据
        //NextValueCheck(ExchangeValue(recordList[recordList.Count - 1].value));

        //更新大眼路 显示
        //UpdateRecordDate();
    }

    private void UpdateDaLu(List<RecordStruct> records, int gameCount)
    {
        if (gameCount == 1)
        {
            daluCurCol = 0;
            daluCurRow = 0;

            daluDataCount = 0;
        }
        //简单判断一下两个 大路值 是否相同
        int resultValue = ExchangeValue(records[gameCount - 1].result);
        if (tempDaluRecordDateList[daluCurCol][daluCurRow] != resultValue || gameCount != daluDataCount + 1)
        {
            //不同值,或者显示对不上
            SetDaLu(records);
        }
        else
        {
            //相同,在旧的大路 显示 后面加上最新的值
            int currentValue = ExchangeValue(records[records.Count - 1].result);//获取最新的值
            if (currentValue == 2)//如果值==2,直接在显示后面+值
            {
                daluCurRow++;
                if (daluCurRow > ROWINDEX_MAX)
                {
                    daluCurRow--;
                    daluCurCol++;
                }
            }
            else
            {
                if (daluCurValue == 2)
                {
                    daluCurValue = currentValue;
                }
                //当前值≠上一列最后一个值 且 当前值≠特殊值2
                bool needSwitchColumn = currentValue != daluCurValue;

                if (needSwitchColumn)
                {
                    daluCurValue = currentValue;
                    daluCurCol++;
                    daluCurRow = -1;
                }
            }
            SafeAddValueToDaLuColumn(daluCurCol, currentValue);
            daluCurRow++;
        }
    }

    void NextValueCheck(int nextValue)
    {
        //下局指示
        if (daluCurValue == nextValue)//下一局和上一局一样
        {
            //同列
            daluCurRow++;
            if (daluCurRow > ROWINDEX_MAX)
            {
                daluCurCol++;
                daluCurRow--;
            }
        }
        else
        {
            //换下一列
            daluCurRow = 0;
            daluCurCol++;
        }
    }

    //重置 大眼路
    private void ResetRecords()
    {

        InitData();

        for (int i = 0; i < recordDaYanItems.Count; i++)
        {
            for (int j = 0; j < recordDaYanItems[i].Count; j++)
            {
                recordDaYanItems[i][j].ResetData();//大眼路隐藏
            }
        }

        for (int i = 0; i < tempDaluRecordDateList.Count; i++)
        {
            tempDaluRecordDateList[i].Clear();//不拐弯大路数据清空
        }
        tempDaluRecordDateList.Clear();

    }

    private void GetDaluLast()
    {

    }
    /// <summary>
    /// 庄闲和结果 → 路单值：0红(庄) 1蓝(闲) 2绿(和)
    /// </summary>
    private int ExchangeValue(int result)
    {
        return result == 1 ? 0 : result == 2 ? 1 : 2;
    }

    /// <summary>
    /// 基于珠盘路原始数据生成「不右拐的大路数据」（不依赖外部大路数据）
    /// </summary>
    /// <param name="recordList">珠盘路原始数据列表（不能为空）</param>
    /// <exception cref="ArgumentNullException">入参为null时抛出</exception>
    private void SetDaLu(List<RecordStruct> recordList)
    {
        // ========== 1. 入参校验（防御性编程） ==========
        if (recordList == null)
            throw new ArgumentNullException(nameof(recordList), "珠盘路原始数据列表不能为空");

        // ========== 2. 初始化大路数据容器（封装复用） ==========
        //InitializeDaLuList();
        tempDaluRecordDateList = new List<List<int>>();

        // ========== 3. 核心配置（语义化常量，便于维护） ==========
        int currentDaLuColumnIndex = 0; // 当前大路列索引（替代原daluColIndex）
        int lastColumnLastValue = -1;   // 上一列最后一个值（关键：不再是“上一条值”）
        const int DefaultInitValue = -1; // 默认初始值（魔法值语义化）
        const int SpecialValue = 2;     // 特殊值（2，可根据业务命名，比如kongValue/neutralValue）

        daluCurCol = 0;
        daluCurRow = -1;
        daluDataCount = 0;

        // ========== 4. 生成不右拐的大路数据（核心逻辑） ==========
        foreach (var record in recordList)
        {
            int currentValue = ExchangeValue(record.result); // 转换当前值

            // 首次记录：初始化上一条值
            if (lastColumnLastValue == DefaultInitValue)
            {
                lastColumnLastValue = currentValue;
            }
            // 非首次记录：判断是否需要切换列（不右拐逻辑）
            else
            {
                if (lastColumnLastValue == 2 && currentValue != 2)
                {
                    lastColumnLastValue = currentValue;
                }
                //当前值≠上一列最后一个值 且 当前值≠特殊值2
                bool needSwitchColumn = currentValue != lastColumnLastValue && currentValue != SpecialValue;

                if (needSwitchColumn)
                {
                    currentDaLuColumnIndex++;
                    lastColumnLastValue = currentValue;
                    daluCurCol++;
                    daluCurRow = -1;
                }
                // 空分支删除，避免代码冗余
            }

            // ========== 5. 安全添加当前值到对应列（封装复用，避免越界） ==========
            SafeAddValueToDaLuColumn(currentDaLuColumnIndex, currentValue);
            daluCurRow++;
            daluDataCount++;
            daluCurValue = lastColumnLastValue;
        }

        // ========== 6. 打印大路数据日志（性能优化：StringBuilder替代+=） ==========
        LogDaLuData();
    }

    #region 辅助方法（封装复用，降低主逻辑复杂度）
    /// <summary>
    /// 初始化大路数据列表（确保不为null）
    /// </summary>
    private void InitializeDaLuList()
    {
        if (tempDaluRecordDateList == null)
            tempDaluRecordDateList = new List<List<int>>();
        else
        {
            for (int i = 0; i < tempDaluRecordDateList.Count; i++)
            {
                tempDaluRecordDateList[i].Clear();
            }
            tempDaluRecordDateList.Clear();
        }
    }

    /// <summary>
    /// 安全地将值添加到指定的大路列（自动补全缺失列，避免索引越界/空引用）
    /// </summary>
    /// <param name="columnIndex">目标列索引</param>
    /// <param name="value">要添加的值</param>
    private void SafeAddValueToDaLuColumn(int columnIndex, int value)
    {
        // 自动补全缺失的列（比如列索引3，但列表只有1列 → 补全列1、2）
        while (columnIndex >= tempDaluRecordDateList.Count)
        {
            tempDaluRecordDateList.Add(new List<int>());
        }
        //while (columnIndex > tempDaluRecordDateList.Count) columnIndex--;

        // 确保目标列不为null，然后添加值
        var targetColumn = tempDaluRecordDateList[columnIndex] ?? new List<int>();
        targetColumn.Add(value);
        tempDaluRecordDateList[columnIndex] = targetColumn;
        recordViewColNumDaYan[columnIndex]++;
    }

    /// <summary>
    /// 打印大路数据日志（性能优化：StringBuilder替代字符串+=）
    /// </summary>
    private void LogDaLuData()
    {
        if (tempDaluRecordDateList == null || tempDaluRecordDateList.Count == 0)
        {
            //Debug.LogError("大路不拐弯数据：空");
            return;
        }

        // 用StringBuilder减少GC，适合大量字符串拼接
        StringBuilder sb = new StringBuilder();
        for (int columnIndex = 0; columnIndex < tempDaluRecordDateList.Count; columnIndex++)
        {
            var columnData = tempDaluRecordDateList[columnIndex];
            if (columnData == null || columnData.Count == 0)
            {
                sb.AppendLine("空列");
                continue;
            }

            // 拼接当前列的所有值（用Join替代循环+=）
            string columnContent = string.Join(",", columnData);
            sb.AppendLine($"{columnContent}"); // 替代原"\n\b换行"（\b无意义，删除）
        }

        //Debug.LogError($"大路不拐弯数据：\n{sb.ToString()}");
        //Debug.LogError($"当前大路最后一个数据显示在下标:{daluCurRow}行,{daluCurCol}列,是第{daluDataCount}局,值是{daluCurValue}");
    }
    #endregion

    #region 算法 计算 各种路
    List<List<int>> daluList;
    List<List<int>> dayanluList;
    List<List<int>> xiaoluList;
    List<List<int>> xiaoqiangluList;

    void SetLuData(ref List<List<int>> luList)
    {

    }

    internal void ResetRecord()
    {
        Init();
        rowIndex = 0;
        colIndex = 0;
        lastCol = 0; lastColForUseSpace = 0;
        colValue = -1;

        for (int i = 0; i < recordDaYanItems.Count; i++)
        {
            for (int j = 0; j < recordDaYanItems[i].Count; j++)
            {
                recordDaYanItems[i][j].ResetData();
            }
        }

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
                ShowRecord(curValue);
            }
        }

    }

    #endregion
}


