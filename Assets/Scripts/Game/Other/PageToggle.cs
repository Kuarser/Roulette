using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PageToggle : MonoBehaviour
{
    public Button closeBtn;//新增加的,不伦不类的,
    public Button leftBtn;
    public Button rightBtn;
    public GameObject toggleParent;//页码点集合的父节点
    public List<GameObject> pageContent;
    public bool isJack = false;//是不是需要联网彩金说明

    private List<Transform> toggleList;//动态变化
    private bool isInit;//是否初始化
    private int curPageNum;//当前页数
    private int totalPageNum;//总页数
    private int TotalPageNum
    {
        set
        {
            totalPageNum = value;
            if (totalPageNum < 1)
            {
                totalPageNum = 1;
            }
            ShowToggle();
        }
    }

    private string enAddStr = "contentEnAdd";//是英文,多出来的页

    // Start is called before the first frame update
    void Start()
    {
        isInit = false;
        InitView();
        InitViewData();
    }

    void OnEnable()
    {
        EventCenter.Instance.AddEventListener(EventHandle.CHANGE_LANGUAGE, RefreshPage);
        ChangePageNum();
        ShowToggle();
    }

    private void OnDisable()
    {
        EventCenter.Instance.RemoveEventListener(EventHandle.CHANGE_LANGUAGE, RefreshPage);
    }

    private void RefreshPage()//刷新页面显示
    {
        ChangePageNum();
        ShowToggle();
    }

    void ChangePageNum()
    {
        //英文:显示list里面的
        //中文:隐藏list里面的

        //或者,英文:把英文的插入到前面
        //中文:把英文的移动到最后

        switch (IOCanvasModel.Instance.curlanguage)
        {
            case Language.en:
                foreach (var item in pageContent)
                {
                    if (item.name.Contains(enAddStr))
                    {
                        item.gameObject.SetActive(true);
                    }
                }
                break;
            //繁体先都用一张图
            case Language.cht:
                foreach (var item in pageContent)
                {
                    if (item.name.Contains(enAddStr))
                    {
                        item.gameObject.SetActive(false);
                    }
                }
                break;
            default:
                Debug.LogError("还没有编写并处理这种语言");
                break;
        }

        curPageNum = 1;
        if (pageContent == null || pageContent.Count <= 0)
        {
            totalPageNum = 1;
        }
        else
        {
            totalPageNum = pageContent.Count;

            foreach (var item in pageContent)
            {
                if (!item.gameObject.activeInHierarchy)
                    totalPageNum -= 1;
                //if (item.transform.localScale.Equals(Vector3.zero))
                //{
                //    totalPageNum -= 1;
                //}
            }
            if (totalPageNum < 1)
                totalPageNum = 1;
        }

    }

    void InitView()
    {
        if (isInit) return;
        isInit = true;

        ChangePageNum();
        ShowToggle();

    }
    void ShowToggle()
    {

        if (toggleList == null)
        {
            toggleList = new List<Transform>();
        }
        else
        {
            toggleList.Clear();
        }
        for (int i = 0; i < toggleParent.transform.childCount; i++)
        {
            var child = toggleParent.transform.GetChild(i);

            //根据最大页码数显示对应个数的页码点
            if (i < totalPageNum)
            {
                child.gameObject.SetActive(true);
                toggleList.Add(child.Find("choose"));
                toggleList[i].transform.localScale = Vector3.zero;
            }
            else
            {
                child.gameObject.SetActive(false);
            }
        }
        if (toggleList.Count > 0)
        {
            toggleList[0].transform.localScale = Vector3.one;
        }

        ShowPageContent(1);
    }

    void InitViewData()
    {
        //InitView();

        leftBtn.onClick.RemoveAllListeners();
        leftBtn.onClick.AddListener(() =>
        {
            ChangeLastPage();
            AudioManager.Instance.PlaySoundEff("buttonClick");
        });

        rightBtn.onClick.RemoveAllListeners();
        rightBtn.onClick.AddListener(() =>
        {
            ChangeNextPage();
            AudioManager.Instance.PlaySoundEff("buttonClick");
        });

        if (closeBtn != null)
        {
            closeBtn.onClick.AddListener(() =>
            {
                curPageNum = 1;
                ShowPageContent(curPageNum);
                ShowPagePoint(curPageNum);
            });
        }

    }

    void ChangeLastPage()
    {
        if (totalPageNum <= 1) return;
        curPageNum -= 1;
        if (curPageNum < 1) curPageNum = totalPageNum;

        ShowPagePoint(curPageNum);
        ShowPageContent(curPageNum);
    }
    void ChangeNextPage()
    {
        if (totalPageNum <= 1) return;
        curPageNum += 1;
        if (curPageNum > totalPageNum) curPageNum = 1;

        ShowPagePoint(curPageNum);
        ShowPageContent(curPageNum);
    }

    /// <summary>
    /// 刷新显示页码点
    /// </summary>
    /// <param name="index">页码数,从1开始</param>
    private void ShowPagePoint(int page)
    {
        for (int i = 0; i < toggleList.Count; i++)
        {
            toggleList[i].transform.localScale = Vector3.zero;
            if (i == page - 1)
            {
                toggleList[i].transform.localScale = Vector3.one;
            }
        }
        bool showBetsMinOfNetJackpotObj = IOCanvasModel.Instance.netJackpotMinBets != null && IOCanvasModel.Instance.netJackpotMinBets.Count > 0 && Model.Instance.netJackpotState > 1;
        if(isJack && page == 4)
        {
            Controller.Instance.view.betsMinOfNetJackpotTexts[0].transform.parent.gameObject.SetActive(showBetsMinOfNetJackpotObj);//区分联网彩金
        }
        else
        {
            Controller.Instance.view.betsMinOfNetJackpotTexts[0].transform.parent.gameObject.SetActive(false);//区分联网彩金
        }
    }

    /// <summary>
    /// 刷新显示页码内容
    /// </summary>
    /// <param name="index"></param>
    private void ShowPageContent(int page)
    {
        int pageNum = page;
        for (int i = 0; i < pageContent.Count; i++)
        {
            if (!pageContent[i].activeSelf)
            {
                //Debug.LogError("英文的" + pageContent[i].name);
                continue;
            }
            pageContent[i].transform.localScale = Vector3.zero;
            pageNum--;

            if (pageNum == 0)
            {
                pageContent[i].transform.localScale = Vector3.one;
            }
        }
    }


    void NewGameObjectAsImage(Sprite sprite, Transform parent, int page)
    {
        GameObject go = new GameObject();
        go.transform.SetParent(parent);
        //go.transform.parent = transform;
        go.transform.localPosition = Vector3.zero;
        go.AddComponent<Image>();
        go.GetComponent<Image>().sprite = sprite;
        pageContent.Insert(page, go);//第几页 有后续内容
        totalPageNum++;
    }
}
