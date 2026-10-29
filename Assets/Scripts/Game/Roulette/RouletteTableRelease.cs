using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RouletteDeluxe
{
    // RouletteTable::HandleTableEvent arms the table on touch-down and
    // applies the wager at the BetSpot under touch-up, which may differ.
    internal sealed class RouletteTableRelease : MonoBehaviour,
        IPointerDownHandler, IPointerUpHandler, IDragHandler, ISubmitHandler
    {
        private Button button;
        private Action place;
        private Action release;
        private Action down;
        private Action move;
        private bool pressed;
        private int pointerId;
        public int SpotIndex { get; private set; }

        public void Configure(Button owner, int spotIndex, Action onPlace,
            Action onRelease = null, Action onDown = null,
            Action onMove = null)
        {
            button = owner;
            SpotIndex = spotIndex;
            place = onPlace;
            release = onRelease;
            down = onDown;
            move = onMove;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (pressed && eventData != null && eventData.pointerId == pointerId)
                move?.Invoke();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            pressed = button != null && button.IsActive() &&
                button.IsInteractable() && eventData != null &&
                eventData.button == PointerEventData.InputButton.Left;
            if (pressed)
            {
                pointerId = eventData.pointerId;
                eventData.useDragThreshold = false;
                down?.Invoke();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!pressed || eventData == null || eventData.pointerId != pointerId)
                return;
            pressed = false;
            // HandleTableEvent returns BetMarker to its off-table (-1000,
            // -1000) origin on touch-up, including a release off the table.
            release?.Invoke();
            if (button == null || !button.IsActive() || !button.IsInteractable() ||
                EventSystem.current == null) return;
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, hits);
            if (hits.Count == 0) return;
            RouletteTableRelease target = hits[0].gameObject
                .GetComponentInParent<RouletteTableRelease>();
            if (target == null || target.button == null ||
                !target.button.IsActive() || !target.button.IsInteractable()) return;
            target.place?.Invoke();
        }

        public void OnSubmit(BaseEventData eventData)
        {
            if (button == null || !button.IsActive() || !button.IsInteractable())
                return;
            place?.Invoke();
        }

        private void OnDisable() { pressed = false; }
    }
}
