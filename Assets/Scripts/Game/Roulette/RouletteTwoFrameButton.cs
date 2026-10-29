using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RouletteDeluxe
{
    internal sealed class RouletteTwoFrameButton : MonoBehaviour,
        IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private RawImage image;
        private Texture2D normal, pressed;

        public void Configure(Button button, Texture2D normalFrame,
            Texture2D pressedFrame)
        {
            image = button.targetGraphic as RawImage;
            normal = normalFrame;
            pressed = pressedFrame;
            if (image != null) image.texture = normal;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (image != null && pressed != null) image.texture = pressed;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (image != null) image.texture = normal;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (image != null) image.texture = normal;
        }
    }
}
