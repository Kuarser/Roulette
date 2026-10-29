using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecordDialogView : MonoBehaviour
{
    private bool isInit = false;//是否已经初始化过
    //private Button closeBtn;//关闭按钮
    Text textPage;//页码

    private Button btnLastPage;  //上一页视图按钮
    private Button btnNextPage;  //下一页视图按钮

    private GameObject itemPrefab;//预制体
    private Transform content;//位置
    //private Transform prefabParents;//位置

    private int page = 1;//当前页
    private int dataCount = 0;//当前页的数据数量

    private Stack<Transform> pool = new Stack<Transform>();//列表

    //private List<int> toggleIndexList;//页码标点 下标集合
    private List<GameObject> toggleParentList;//页码标点 下标集合
    private List<Transform> toggleTransformList;//页码选中 集合


    private void ShowToggle(int pageNum)//根据页码数显示对应的 页码标点
    {
        int index = pageNum - 1;
        for (int i = 0; i < toggleTransformList.Count; i++)
        {
            if (i == index)
                toggleTransformList[i].localScale = Vector3.one;
            else
                toggleTransformList[i].localScale = Vector3.zero;
        }
    }

    private void ShowToggle2(int totalPageNum)//根据总页数,显示对应个数的页码标点-kuarser：谁写的只开不关=_=，打码之后会出问题
    {
        int index = totalPageNum - 1;
        for (int i = 0; i < toggleParentList.Count; i++)
        {
            if (i <= index)
                toggleParentList[i].SetActive(true);
            else
                toggleParentList[i].SetActive(false);
        }
    }


    private Transform GetObject()//实例化数据
    {
        if (pool.Count == 0)
        {
            var tran = Instantiate(itemPrefab).transform;
            tran.transform.SetParent(content);
            tran.transform.SetAsLastSibling();
            tran.transform.localScale = Vector3.one;
            tran.transform.localPosition = new Vector3(0,0,-500);//太低会被卡牌阻挡
            return tran;
        }
        Transform candidate = pool.Pop();
        candidate.SetAsLastSibling();
        //candidate.gameObject.SetActive(true);
        //candidate.SetParent(content);
        //candidate.localScale = Vector3.one;
        return candidate;
    }
    private void ReturnObject(Transform trans)//放回对象池
    {
        //对不上,增加下面3行
        //trans.SendMessage("ScrollCellReturn", SendMessageOptions.DontRequireReceiver);
        trans.gameObject.SetActive(false);
        //trans.SetParent(prefabParents);

        pool.Push(trans);
    }
    /// <summary>
    /// 为子项提供数据，刷新子项的显示内容。
    /// </summary>
    /// <param name="trans">子项的 Transform</param>
    /// <param name="idx">子项的索引</param>
    private void ProvideData(Transform trans, int idx)
    {
        int tempCount = Model.Instance.GetRecordListCount();
        // 确保索引在数据范围内
        idx = idx % tempCount;
        //Debug.Log($"1本页下标:{idx},本页数量:{tempCount}");

        // 调用子项的初始化方法，设置数据
        var temp = trans.GetComponent<LoopRecordItemView>();
        List<TotalDataStruct> tmep = PlayerMgr.Instance.playerDic[SBoxModel.Instance.macId].totalDataStruct;//Model.Instance.GetRecordListByPage(page) as List<RecordData>;
        if(tmep.Count==0)return;
        if(page == 0)page = Model.Instance.maxPage;
        if(page == Model.Instance.maxPage+1)page = 1;
        temp.InitViewData2(tmep[tmep.Count-page]);//tmep[idx]
        //Debug.LogError($"{Model.Instance.maxPage}:{PlayerMgr.Instance.playerDic[SBoxModel.Instance.macId].totalDataStruct.Count}");
        //Debug.Log($"2本页下标:{idx},本页数量:{Model.Instance.GetRecordListCount()}");


        // 更新当前页码
        page = Model.Instance.recordListPage;
    }

    private void Awake()
    {
        InitView();
    }

    private void Start()
    {
        InitViewData();
    }

    private void InitView()
    {
        if (isInit) return;
        isInit = true;
        //closeBtn = transform.Find("closeBtn").GetComponent<Button>();//关闭按钮

        btnLastPage = transform.Find("btnLastPage").GetComponent<Button>();//上一页视图按钮
        btnNextPage = transform.Find("btnNextPage").GetComponent<Button>();//下一页视图按钮
        textPage = transform.Find("textPage").GetComponent<Text>();//页码

        itemPrefab = transform.Find("recordList/loopRecordItem").gameObject;
        content = transform.Find("recordList/Viewport/Content");
        //prefabParents = transform.Find("recordList/Parents");

        toggleTransformList = new List<Transform>();
        toggleParentList = new List<GameObject>();
        var toggleParent = transform.Find("toggle");
        for (int i = 0; i < toggleParent.childCount; i++)
        {
            var parent = toggleParent.GetChild(i).gameObject;
            toggleParentList.Add(parent);
            parent.SetActive(false);

            var child = toggleParent.GetChild(i).Find("choose");
            toggleTransformList.Add(child);
            child.localScale = Vector3.zero;
            if (i == 0)
                child.localScale = Vector3.one;
        }

    }
    void InitViewData()
    {
        InitView();

        btnLastPage.onClick.RemoveAllListeners();
        btnNextPage.onClick.RemoveAllListeners();

        btnLastPage.onClick.AddListener(() =>
        {
            page -= 1;
            Show();
            if (Model.Instance.maxPage == 0)
            {
                textPage.text = $"1/1";
            }
            else
            {
                textPage.text = $"{Model.Instance.recordListPage}/{Model.Instance.maxPage}";
            }
            AudioManager.Instance.PlaySoundEff("buttonClick");
        });
        btnNextPage.onClick.AddListener(() =>
        {
            page += 1;
            Show();
            if (Model.Instance.maxPage == 0)
            {
                textPage.text = $"1/1";
            }
            else
            {
                textPage.text = $"{Model.Instance.recordListPage}/{Model.Instance.maxPage}";
            }
            AudioManager.Instance.PlaySoundEff("buttonClick");
        });

        //closeBtn.onClick.AddListener(() => { transform.localScale = Vector3.zero; Model.Instance.myTriggerDetectionCanEnter = true; });
    }

    public void OpenFirstPage()
    {
        InitView();
        page = 1;
        Show();
        if (Model.Instance.maxPage == 0)
        {
            textPage.text = $"1/1";
        }
        else
        {
            textPage.text = $"{Model.Instance.recordListPage}/{Model.Instance.maxPage}";
        }
        if (!this.gameObject.activeInHierarchy)
            this.gameObject.SetActive(true);
        transform.localScale = Vector3.one;
    }
    private void Show()
    {
        //先初始化界面
        for (int i = 0; i < content.childCount; i++)
        {
            ReturnObject(content.GetChild(i));
        }

        //显示
        dataCount = Model.Instance.GetRecordListCountByPage(page);//一页中有几个数据
        for (int i = 0; i < dataCount; i++)
        {
            ProvideData(GetObject(), i);//实例化并赋值
        }

        ShowToggle2(Model.Instance.maxPage);
        ShowToggle(page);
    }

    public void Reset()
    {
        isInit = false;
        InitView();
        page = 1;
        Show();
    }
}
