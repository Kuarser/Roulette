using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RouletteDeluxe
{
    // The original NeighborsGadget receives a BetSpot hover event and then
    // changes its selected number; the wheel graphic is not a 37-way input.
    public sealed class RouletteNeighborHover : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IDragHandler
    {
        public Action OnEnter;
        public Action<PointerEventData> OnExit;
        public Action<PointerEventData> OnDragMove;

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnEnter?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnExit?.Invoke(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            OnDragMove?.Invoke(eventData);
        }
    }
}
