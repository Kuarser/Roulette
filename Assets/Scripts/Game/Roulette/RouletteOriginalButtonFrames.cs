using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RouletteDeluxe
{
    // Four-state iDeck buttons; main-table chips use a separate source state mapping.
    internal sealed class RouletteOriginalButtonFrames : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        private Button button;
        private RawImage image;
        private Texture2D[] frames;
        private bool hover, down, selected;
        private bool pressUsesThirdFrame = true;
        private int pressedFrame = 2;
        private int selectedFrame = 1;
        private bool keepSelectedWhenDisabled;
        private bool useHoverFrame = true;
        private RectTransform depressedLabel;
        private Vector2 neutralLabelPosition;

        public void Configure(Button owner, Texture2D[] states,
            bool usePressFrame = true, bool retainDisabledSelection = false,
            bool showHoverFrame = true, int pressFrameIndex = 2,
            int selectedFrameIndex = 1)
        {
            button = owner;
            image = owner.targetGraphic as RawImage;
            frames = states;
            pressUsesThirdFrame = usePressFrame;
            pressedFrame = pressFrameIndex;
            selectedFrame = selectedFrameIndex;
            keepSelectedWhenDisabled = retainDisabledSelection;
            useHoverFrame = showHoverFrame;
            Apply();
        }

        public void Refresh() { Apply(); }

        public void BindDepressedLabel(RectTransform label)
        {
            depressedLabel = label;
            neutralLabelPosition = label.anchoredPosition;
            Apply();
        }

        public void SetSelected(bool value)
        {
            if (selected == value) return;
            selected = value;
            Apply();
        }

        public void OnPointerEnter(PointerEventData eventData) { hover = true; Apply(); }
        public void OnPointerExit(PointerEventData eventData) { hover = false; down = false; Apply(); }
        public void OnPointerDown(PointerEventData eventData) { down = true; Apply(); }
        public void OnPointerUp(PointerEventData eventData) { down = false; Apply(); }

        public void RestoreOnMove()
        {
            // GameDeckProfile::RestoreDeck clears the depressed frame on a
            // captured deck move, then restores persistent selections.
            hover = false;
            down = false;
            Apply();
        }

        private void LateUpdate() { Apply(); }

        private void Apply()
        {
            if (button == null || image == null || frames == null || frames.Length != 4)
                return;
            bool interactable = button.IsInteractable();
            int state = !interactable ?
                (keepSelectedWhenDisabled ? (selected ? 1 : 2) : 3) :
                down && pressUsesThirdFrame ? pressedFrame :
                selected ? selectedFrame : (hover && useHoverFrame) ? 1 : 0;
            if (image.texture != frames[state]) image.texture = frames[state];
            // Ordinary Button::UpdateImage moves its separate label by
            // (+1,+1) in the source's top-left coordinates while depressed.
            if (depressedLabel != null)
                depressedLabel.anchoredPosition = neutralLabelPosition +
                    (interactable && down && pressUsesThirdFrame ?
                        new Vector2(1f, -1f) : Vector2.zero);
        }
    }
}
