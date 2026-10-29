using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

//每轮结果(已经运行过的每局结果) 视图
public class RecordListView_SelfAddItem : MonoBehaviour
{
    //
    private bool isInit = false;
    //内部组件
    [SerializeField]
    private bool isVertical = false;//存储格子时,格子排列方式,true:竖着排列(方便增加格子), false:横着排列(不方便增加)
    [SerializeField]
    private string recordItemPrefabName = "RecordItem_SelfAddItem";//格子预制体
    private GridLayoutGroup tempGridLayoutGroup;
    private RecordItemView_SelfAddItem itemPrefab;//格子预制体,

    // 1. 私有字段：只读引用 + 初始化 + 规范命名
    private readonly List<List<RecordItemView_SelfAddItem>> _items = new List<List<RecordItemView_SelfAddItem>>();//格子列表
    // 2. 缓存只读包装结果，避免重复创建
    private IReadOnlyList<IReadOnlyList<RecordItemView_SelfAddItem>> _readOnlyItemsCache;
    // 3. 公共属性：只读 + 缓存 + 空保护
    public IReadOnlyList<IReadOnlyList<RecordItemView_SelfAddItem>> Items
    {
        get
        {
            if (_readOnlyItemsCache == null)
            {
                _readOnlyItemsCache = _items
                    .Select(innerList => (IReadOnlyList<RecordItemView_SelfAddItem>)innerList.AsReadOnly())
                    .ToList()
                    .AsReadOnly();
            }
            return _readOnlyItemsCache;
        }
    }
    // 4. 二维索引器：简化访问 + 边界检查
    public RecordItemView_SelfAddItem this[int row, int col]
    {
        get
        {
            if (row < 0 || row >= _items.Count)
                throw new ArgumentOutOfRangeException(nameof(row), $"行索引 {row} 超出范围（总行数：{_items.Count}）");
            var innerList = _items[row];
            if (col < 0 || col >= innerList.Count)
                throw new ArgumentOutOfRangeException(nameof(col), $"列索引 {col} 超出范围（该行列数：{innerList.Count}）");

            return innerList[col];
        }
    }

    //常量数据
    private const int ROW_MAX = 5;//行最大下标
    private const int COL_MAX = 99;//列最大下标//这是下标的,从0开始算的
    private int col_max = 2;//如果是竖着排列,一开始先显示20列
    private int addColCount = 4;//不够时,一次增加4列,最多不超过 COL_MAX 列

    //辅助变量数据
    private int row;//当前要存入数据的格子的行,
    private int col;//当前要存入数据的格子的列(有时候会比最后一列大)
    private int lastCol;//最后一列,
    private int colValue;//当前列的值,用来判断下一行的值是+1还是-1,或者是0(不变),已经显示的结果的最后一个值
    private bool horizontalWays;//是否有折角
    private int itemCount;//已经存入数据的个数

    private int lastColForUseSpace;//最后一列,为了空行
    public int LastCol//最后一列显示不下了,方便移动滑动条,显示从最后一列往前显示
    {
        get
        {
            if (lastColForUseSpace <= 29)
                return 0;
            return lastColForUseSpace - 29;
        }
    }
    public int LastCol2
    {
        get
        {
            if (lastColForUseSpace <= 5)
                return 0;
            return lastColForUseSpace - 5;
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
        InitData();

        tempGridLayoutGroup = GetComponent<GridLayoutGroup>();//控制格子显示 排列方式
        if (isVertical)
        {
            tempGridLayoutGroup.startAxis = GridLayoutGroup.Axis.Vertical;
            LoadAndGetItemsByVertical();
        }
        else
        {
            tempGridLayoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
            LoadAndGetItemsByHorizontal();
        }

        //GetItems();//不用了
    }

    private void InitData()
    {
        row = 0;
        col = 0;
        lastCol = 0;
        colValue = -1;
        horizontalWays = false;
        itemCount = 1;
        lastColForUseSpace = 0;
        tempRow = -1;//重置"最后一颗棋子"坐标,防止重放时首局和局把和标盖到上一轮遗留的格子上
        tempCol = -1;
    }

    private void GetItems()
    {
        int index = 0;
        for (int i = 0; i <= ROW_MAX; i++)//旧的,需要去 unity UI界面上 手动挂载格子,然后现在获取那些挂载的格子
        {
            _items.Add(new List<RecordItemView_SelfAddItem>());
            for (int j = 0; j <= COL_MAX; j++)
            {
                _items[i].Add(transform.GetChild(index).GetComponent<RecordItemView_SelfAddItem>());
                index++;
            }
        }
    }

    private void LoadAndGetItemsByHorizontal()
    {
        //需要制作预制体,并打成game 的 AB包,
        //格子排列方式是横着排列,
        //如果不够,只能增加行数.但是存储行数增加,显示行数不变,导致显示的对应位置变化,不满足界面要求,
        //所以一开始就需要把100列的格子都加载出来,每列6个格子,总共600个格子,太浪费
        int index = 0;
        ResMgr.Instance.LoadAssetBundle("game", recordItemPrefabName, (obj) =>
        {
            //自动实例化ab包并挂载格子
            var prefab = obj;
            for (int i = 0; i <= ROW_MAX; i++)
            {
                _items.Add(new List<RecordItemView_SelfAddItem>());
                for (int j = 0; j <= COL_MAX; j++)
                {
                    var item = Instantiate(prefab) as GameObject;
                    item.transform.SetParent(transform);
                    item.transform.localScale = Vector3.one;
                    item.transform.localPosition = Vector3.zero;
                    item.gameObject.name = $"RecordItem_{index}";
                    var itemScript = item.GetComponent<RecordItemView_SelfAddItem>();
                    //itemScript.Value = -1;//初始化格子数据,显示空格子
                    _items[i].Add(itemScript);
                    index++;
                }
            }
        });
    }

    private void LoadAndGetItemsByVertical()
    {
        //需要制作预制体,并打成game 的 AB包
        //格子排列方式是竖着排列,
        //如果不够,能增加一列格子,并且对应的显示不变,满足界面要求,
        //可以先显示少量格子,不够再增加格子,最多不超过100列
        int index = 0;
        ResMgr.Instance.LoadAssetBundle("game", recordItemPrefabName, (obj) =>
        {
            //实例化并挂载格子
            var prefab = obj;
            for (int j = 0; j <= col_max; j++)
            {
                _items.Add(new List<RecordItemView_SelfAddItem>());
                for (int i = 0; i <= ROW_MAX; i++)
                {
                    var item = Instantiate(prefab) as GameObject;
                    item.transform.SetParent(transform);
                    item.transform.localScale = Vector3.one;
                    item.transform.localPosition = Vector3.zero;
                    item.gameObject.name = $"RecordItem_{index}";
                    var itemScript = item.GetComponent<RecordItemView_SelfAddItem>();
                    //itemScript.Value = -1;//初始化格子数据,显示空格子
                    _items[j].Add(itemScript);
                    index++;
                    //Debug.Log($"items:{items[j][i].name},行列:({j},{i})");
                }
            }
            if (_items[0][0] != null)
                itemPrefab = _items[0][0];
        });
    }

    private void AddItems()//格子不够,新增加格子
    {
        //Debug.Log("格子不够,增加格子");
        int index = 0;
        for (int j = col_max + 1; j <= col_max + addColCount; j++)
        {
            _items.Add(new List<RecordItemView_SelfAddItem>());
            for (int i = 0; i <= ROW_MAX; i++)
            {
                var item = Instantiate(itemPrefab);
                item.transform.SetParent(transform);
                item.transform.localScale = Vector3.one;
                item.transform.localPosition = Vector3.zero;
                item.gameObject.name = $"RecordItem_{index}";
                //item.Value = -1;//初始化格子数据,显示空格子
                _items[j].Add(item);
                index++;
            }
        }
        col_max += addColCount;//增加4列,多加4*6 个格子
        InvalidateCache();
    }

    /// <summary>
    /// 庄闲和结果 → 路单值：0红(庄) 1蓝(闲) 2绿(和)
    /// </summary>
    private int ExchangeValue(int result)
    {
        return result == 1 ? 0 : result == 2 ? 1 : 2;
    }

    private void UpdateRecord(int value, int luckyId)//结果横着更新,需要根据规则换行,换列,并且对格子进行更新显示,需要显示全部格子
    {
        var curValue = ExchangeValue(value);//当前鱼的结果值 换算成 格子值

        if (colValue == -1)
        {
            lastCol = row = col = 0;
            colValue = curValue;
            lastColForUseSpace = 0;
        }
        else
        {
            if (colValue == 2)//只有第一局==2 才能进入,其他任何时候都不满足了
            {
                if (curValue != 2)
                    colValue = curValue;
                row++;
                if (row > ROW_MAX)
                {
                    row = ROW_MAX;
                    col++;
                }
                else if (
                    row <= ROW_MAX &&//※kuarser修改中row + 1->row
                    ((_items[row][col].Value == curValue)
                    || _items[row][col].Value == 2
                    || _items[row][col].Value == colValue)
                    || _items[row][col].Value != -1
                    || horizontalWays)
                {
                    row--;
                    col++;
                    horizontalWays = true;
                }
            }
            else
            {
                if (colValue == curValue
                    || curValue == 2)//如果值相同或者新值是2,同一列,行+1
                {
                    row++;
                    if (row > ROW_MAX)//行超出,列+1
                    {
                        row = ROW_MAX;
                        col++;
                    }
                    else if (col - 1 >= 0
                        && (_items[row][col - 1].Value == curValue
                        || _items[row][col - 1].Value == 2)
                        && (_items[row - 1][col - 1].Value == -1)
                        )//判断显示的前一列同一行,数据的前两列及以上同一行是否是一样的值,有则折角(,导致分不清现在这个值是哪一列的)
                    {
                        row--;//空一行
                        col++;
                        horizontalWays = true;
                    }
                    else if (row <= ROW_MAX &&//※kuarser修改中row + 1->row
                        ((_items[row][col].Value == curValue)
                        || _items[row][col].Value == 2
                        || _items[row][col].Value == colValue)
                        || _items[row][col].Value != -1
                        || horizontalWays)//检查是否会遇到折角,或者说前几列是否有行超出;是则列+1
                    {
                        row--;//故意和上一个折角空一行
                        col++;
                        horizontalWays = true;
                    }
                }
                else
                {
                    row = 0;//值不同,列+1,行=0
                    lastCol++;
                    colValue = curValue;
                    horizontalWays = false;
                    col = lastCol;
                }
            }
        }

        if (lastColForUseSpace < col)
            lastColForUseSpace = col; //存最后一列,不是最后一列就要更新为最后一列
        _items[row][col].SetValue(curValue, luckyId);
        itemCount++;
    }
    public int tempRow = -1;
    public int tempCol = -1;
    //flag:是否需要折角空一行显示
    private void UpdateRecord_Vertical(int value, int luckyId, bool flag = true)//结果竖着更新,可以先显示一半格子,如果不够在增加100格子,不够再加,直到满
    {
        var curValue = ExchangeValue(value);
        if(curValue == 2) {
            if(tempRow==-1) return;
            _items[tempCol][tempRow].SetHeBiao();
            return;
        }
        if (colValue == -1)
        {
            lastCol = row = col = 0;
            lastColForUseSpace = 0;
            colValue = curValue;
        }
        else
        {
            if (colValue == 2)
            {
                if (curValue != 2)
                    colValue = curValue;
                row++;
                if (row > ROW_MAX)
                {
                    row = ROW_MAX;
                    col++;
                }
                else if (
                    row <= ROW_MAX &&//※kuarser修改中row + 1->row
                    ((_items[col][row].Value == curValue)
                    || _items[col][row].Value == 2
                    || _items[col][row].Value == colValue)
                    || _items[col][row].Value != -1
                    || horizontalWays)
                {
                    row--;
                    col++;
                    horizontalWays = true;
                }
                lastColForUseSpace = col;
            }
            else
            {
                if (colValue == curValue
                    || curValue == 2)
                {
                    if (flag)
                    {
                        //这部分是旧的换行,
                        row++;//同方连续
                        if (row > ROW_MAX)//一列存满了相同的,换列显示
                        {
                            //换方开新列
                            row = ROW_MAX;
                            col++;
                        }
                        else if (row <= ROW_MAX &&//※kuarser修改中row + 1->row
                            ((_items[col][row].Value == curValue)
                            || _items[col][row].Value == 2
                            || _items[col][row].Value == colValue)
                            || _items[col][row].Value != -1
                            || horizontalWays)
                        {
                            //换列
                            row--;
                            col++;
                            horizontalWays = true;
                        }
                        lastColForUseSpace = col;//存最后一列,换列的更要存
                    }
                    else
                    {
                        //这部分是新的换行,可能会有空列
                        row++;
                        if (row > ROW_MAX)
                        {
                            row = ROW_MAX;
                            col++;
                        }
                        else if ((row <= ROW_MAX//※kuarser修改中row + 1->row
                            && _items[col][row].Value != -1)
                            || _items[col][row].Value != -1
                            || horizontalWays
                            )//提前判断是否有顶头,有的话空一行再折角
                        {
                            row--;//空一行
                            col++;
                            horizontalWays = true;
                        }
                        else if (col - 1 >= 0
                            && (_items[col - 1][row].Value == curValue
                            || _items[col - 1][row].Value == 2)
                            && (_items[col - 1][row - 1].Value == -1)
                            )//判断显示的前一列同一行,数据的前两列及以上同一行是否是一样的值,有则折角(,导致分不清现在这个值是哪一列的)
                        {
                            row--;//空一行
                            col++;
                            horizontalWays = true;
                        }
                    }
                    lastColForUseSpace = col;
                }
                else
                {
                    row = 0;
                    lastCol++;

                    if (!flag)//会导致有空列
                    {
                        //如果重新换列,还被顶头
                        while (horizontalWays
                            && (_items[lastCol][row ].Value != -1//※kuarser修改中row + 1->row
                            || (_items[lastCol][row ].Value == -1 && _items[lastCol][row + 2].Value != -1)
                            || (_items[lastCol][row ].Value == -1 && _items[lastCol - 1][row].Value == -1 && _items[lastCol - 1][row ].Value != -1)
                            ))
                        {
                            //Debug.LogError("被顶头,向后移一列");
                            lastCol++;
                            if (lastCol >= _items.Count)//格子不够,增加格子,增加4列,多加4*6 个格子,直到满足显示需要的列数
                            {
                                AddItems();
                            }
                        }
                    }

                    colValue = curValue;
                    horizontalWays = false;
                    col = lastCol;
                    if (lastColForUseSpace < col)
                        lastColForUseSpace = col;
                }
            }
        }



        if (col >= col_max)//格子不够,增加格子,增加4列,多加4*6 个格子,直到满足显示需要的列数
        {
            AddItems();
        }
        //if (col >= _items.Count)
        //{
        //    AddItems();
        //}

        _items[col][row].SetValue(curValue, luckyId);
        tempRow = row;
        tempCol = col;
        itemCount++;
    }

    //设置 record ,根据现有珠盘路显示大路
    public void SetRecords(List<RecordStruct> recordList)
    {
        Init();
        if (recordList == null) return;

        ResetRecords();//隐藏原来的界面
        for (int i = 0; i < recordList.Count; i++)
        {
            if (isVertical)
                UpdateRecord_Vertical(recordList[i].result, recordList[i].luckyId);
            else
                UpdateRecord(recordList[i].result, recordList[i].luckyId);
        }
    }

    //重置 record,隐藏界面,重置数据,显示空格子
    public void ResetRecords()
    {
        InitData();
        for (int i = 0; i < _items.Count; i++)
            for (int j = 0; j < _items[i].Count; j++)
                _items[i][j].ResetData();
    }

    //更新 record ,根据最新一局结果更新一局
    public void UpdateRecord(RecordStruct record)
    {
        if (isVertical)
            UpdateRecord_Vertical(record.result, record.luckyId);
        else
            UpdateRecord(record.result, record.luckyId);
    }

    //检测 record 现在显示的结果和当前局数是否匹配
    public bool CheckCorrectRecords(int gameCount)
    {
        //做判断,看是不是漏了之前的结果
        if (gameCount > itemCount)//当前局数大于已经排序的个数
        {
            Debug.Log($"gameCount:{gameCount},itemCount:{itemCount}");
            return false;
        }
        return true;
    }

    // 6. 对外提供的修改方法（封装修改逻辑）
    public void AddInnerList(List<RecordItemView_SelfAddItem> newInnerList)
    {
        if (newInnerList == null) throw new ArgumentNullException(nameof(newInnerList));
        _items.Add(newInnerList);
        InvalidateCache();
    }

    public void ClearAll()
    {
        _items.Clear();
        InvalidateCache();
    }
    // 5. 内部修改集合的方法：标记缓存失效
    private void InvalidateCache()
    {
        _readOnlyItemsCache = null;
    }
}
