using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RouletteDeluxe
{
    // The original main-screen event handler dismisses an active deck
    // confirmation before dispatching the touched sprite's own action.
    internal sealed class RouletteMainTouchCancel : MonoBehaviour,
        IPointerDownHandler
    {
        private Action cancel;

        public void Configure(Action callback) { cancel = callback; }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData == null ||
                eventData.button != PointerEventData.InputButton.Left) return;
            Button button = GetComponent<Button>();
            if (button != null && (!button.IsActive() || !button.IsInteractable()))
                return;
            cancel?.Invoke();
        }
    }
}
