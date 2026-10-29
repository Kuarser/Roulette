using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RouletteDeluxe
{
    // The game deck receives captured move events even while a button remains
    // held. RestoreDeck clears its depressed state before the release event.
    internal sealed class RouletteDeckMoveState : MonoBehaviour,
        IPointerDownHandler, IInitializePotentialDragHandler, IDragHandler
    {
        private RouletteOriginalButtonFrames frames;
        public bool Moved { get; private set; }

        public void Configure(RouletteOriginalButtonFrames buttonFrames)
        {
            frames = buttonFrames;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Moved = false;
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (eventData != null) eventData.useDragThreshold = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData == null ||
                eventData.button != PointerEventData.InputButton.Left) return;
            Moved = true;
            if (frames != null) frames.RestoreOnMove();
        }
    }

    // SERVICE and COLLECT require the source depressed state on release;
    // AUTOPLAY acts on release even after RestoreDeck has cleared it.
    internal sealed class RouletteDeckReleaseAction : MonoBehaviour,
        IPointerDownHandler, IPointerUpHandler, ISubmitHandler
    {
        private Button button;
        private RouletteDeckMoveState movement;
        private Action release;
        private bool held;
        private bool allowAfterMove;

        public void Configure(Button owner, Action onRelease, bool allowMoved)
        {
            button = owner;
            movement = owner.GetComponent<RouletteDeckMoveState>();
            release = onRelease;
            allowAfterMove = allowMoved;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            held = button != null && button.IsActive() && button.IsInteractable() &&
                eventData != null &&
                eventData.button == PointerEventData.InputButton.Left;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            bool submit = held && button != null && button.IsActive() &&
                button.IsInteractable() && eventData != null &&
                eventData.button == PointerEventData.InputButton.Left &&
                (allowAfterMove || movement == null || !movement.Moved);
            held = false;
            if (submit) release?.Invoke();
        }

        public void OnSubmit(BaseEventData eventData)
        {
            if (button != null && button.IsActive() && button.IsInteractable())
                release?.Invoke();
        }

        private void OnDisable() { held = false; }
    }
}
