using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventTrigger))]
public class MyTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public UnityAction pointerEnter;

    public UnityAction pointerExit;


    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerExit?.Invoke();
    }
}
