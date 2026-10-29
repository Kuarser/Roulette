using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RouletteDeluxe
{
    // Cabinet chip, neighbor and French-bet controls act on touch-down.
    // Ordinary table bet spots use their separate press/release handler.
    internal sealed class RoulettePointerDownAction : MonoBehaviour,
        IPointerDownHandler, ISubmitHandler
    {
        private Button button;
        private Action select;

        public void Configure(Button owner, Action onSelect)
        {
            button = owner;
            select = onSelect;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (button == null || !button.IsActive() || !button.IsInteractable() ||
                eventData == null ||
                eventData.button != PointerEventData.InputButton.Left) return;
            select?.Invoke();
        }

        public void OnSubmit(BaseEventData eventData)
        {
            if (button == null || !button.IsActive() || !button.IsInteractable()) return;
            select?.Invoke();
        }
    }

    // Game::HandleEvent forwards both touch-down (0x200001) and captured
    // movement (0x200002) for PLAY, ProView volume and SEE PAYS.
    internal sealed class RouletteCapturedMainTouchAction : MonoBehaviour,
        IPointerDownHandler, IInitializePotentialDragHandler, IDragHandler,
        ISubmitHandler
    {
        private Button button;
        private Action play;

        public void Configure(Button owner, Action action)
        {
            button = owner;
            play = action;
        }

        private void Invoke(PointerEventData eventData)
        {
            if (button == null || !button.IsActive() || !button.IsInteractable() ||
                eventData == null ||
                eventData.button != PointerEventData.InputButton.Left) return;
            play?.Invoke();
        }

        public void OnPointerDown(PointerEventData eventData) { Invoke(eventData); }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (eventData != null) eventData.useDragThreshold = false;
        }

        public void OnDrag(PointerEventData eventData) { Invoke(eventData); }

        public void OnSubmit(BaseEventData eventData)
        {
            if (button == null || !button.IsActive() || !button.IsInteractable()) return;
            play?.Invoke();
        }
    }

    // The original Pays navigation handles both 0x200001 touch-down and
    // 0x200002 movement captured by the same sprite. Other table controls
    // intentionally do not acquire Unity drag capture through this class.
    internal sealed class RoulettePaysNavigationAction : MonoBehaviour,
        IPointerDownHandler, IInitializePotentialDragHandler, IDragHandler,
        ISubmitHandler
    {
        private Button button;
        private Action navigate;

        public void Configure(Button owner, Action action)
        {
            button = owner;
            navigate = action;
        }

        private void Invoke(PointerEventData eventData)
        {
            if (button == null || !button.IsActive() || !button.IsInteractable() ||
                eventData == null ||
                eventData.button != PointerEventData.InputButton.Left) return;
            navigate?.Invoke();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Invoke(eventData);
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            // ProView forwards captured movement without a pixel threshold.
            if (eventData != null) eventData.useDragThreshold = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Invoke(eventData);
        }

        public void OnSubmit(BaseEventData eventData)
        {
            if (button == null || !button.IsActive() || !button.IsInteractable())
                return;
            navigate?.Invoke();
        }
    }
}
