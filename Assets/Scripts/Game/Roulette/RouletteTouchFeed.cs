using System.Collections.Generic;
using UnityEngine;

namespace RouletteDeluxe
{
    // 轮盘画布是 ScreenSpaceOverlay + targetDisplay，原生 GraphicRaycaster 在
    // Android / Editor 上无法判定事件归属（Display.RelativeMouseAt 恒返回零），
    // 于是任意屏幕的指针都会命中这块画布。
    //
    // 这里沿用 NewScreen 既有的"屏幕绑定"语义：触摸来自哪台 USB 设备，就只属于
    // 该设备绑定的 display。绑定生效时由 MyTouchComponent 把该屏触摸点投递给轮盘
    // 输入队列，同时 RouletteSourceInputModule 不再消费全局 Input。
    internal static class RouletteTouchFeed
    {
        // 轮盘画布所属 display；-1 表示画布尚未创建。
        internal static int TargetDisplay = -1;

        private static RouletteSourceInputModule module;
        // 已按下但尚未抬起的触点，用于丢弃可疑的重复抬起与未捕获的移动。
        private static readonly HashSet<int> capturedPointers = new HashSet<int>();
        private static Vector2 lastPressPosition;
        private static int pressFrame = -1;

        internal static void Attach(RouletteSourceInputModule value, int targetDisplay)
        {
            module = value;
            TargetDisplay = targetDisplay;
            capturedPointers.Clear();
            pressFrame = -1;
        }

        internal static void Detach(RouletteSourceInputModule value)
        {
            if (module != value) return;
            module = null;
            TargetDisplay = -1;
            capturedPointers.Clear();
            pressFrame = -1;
        }

        // 绑定模式：只在该屏确实能收到"专属"触摸时才接管输入。
        // 任何一环不成立都退回旧的全局 Input 行为，避免整块台面点不动。
        internal static bool Bound
        {
            get
            {
                if (module == null || TargetDisplay < 0) return false;
                if (FindAdaptation(TargetDisplay) == null) return false;
                if (Application.isEditor)
                {
                    // 编辑器只有鼠标模拟，RightAlt 在 curDisplay 上轮换。只有该屏
                    // 真的存在相机时才可能把鼠标路由过去，否则保持旧行为。
                    Dictionary<int, Camera> cameras = ScreenModel.Instance.displayCameraDic;
                    return cameras != null && cameras.ContainsKey(TargetDisplay);
                }
                // 打包端以"该屏正在接收 USB 触摸"为准；绑定失败时同样退回旧行为。
                foreach (KeyValuePair<string, TouchAdaptation> pair in
                    ScreenModel.Instance.listeningDic)
                {
                    if (pair.Value != null && pair.Value.display == TargetDisplay)
                        return true;
                }
                return false;
            }
        }

        private static TouchAdaptation FindAdaptation(int display)
        {
            List<TouchAdaptation> list = ScreenModel.Instance.touchAdaptationList;
            if (list == null) return null;
            for (int i = 0; i < list.Count; ++i)
            {
                TouchAdaptation adaptation = list[i];
                if (adaptation != null && adaptation.display == display)
                    return adaptation;
            }
            return null;
        }

        // MyTouchComponent 的唯一入口：只转发归属到轮盘画布所在屏的触点。
        internal static void Forward(UsbTouchDataParser.TouchPoint point)
        {
            if (point == null || module == null || TargetDisplay < 0) return;
            if (point.display != TargetDisplay) return;
            // 未进入绑定模式时模块仍消费全局 Input，这里再投递会重复派发。
            if (!Bound) return;

            bool pressed = point.phase == TouchPhase.Began;
            bool released = point.phase == TouchPhase.Ended;
            bool canceled = point.phase == TouchPhase.Canceled;
            bool held = !pressed && !released && !canceled;

            if (pressed)
            {
                // 同一 id 重复按下说明上一轮的抬起丢了，直接覆盖。
                capturedPointers.Add(point.id);
                lastPressPosition = point.pos;
                pressFrame = Time.frameCount;
            }
            else if (released)
            {
                // setLastTouchPointCancel 与设备重复上报都会补发结束态，只放行一次。
                if (!capturedPointers.Remove(point.id)) return;
            }
            else if (canceled)
            {
                // 设备停止上报时不会有 Ended，按 PurgeInputs 的语义收口，不派发点击。
                if (!capturedPointers.Remove(point.id)) return;
                module.QueuePointerCancel(point.id);
                return;
            }
            else if (!capturedPointers.Contains(point.id))
            {
                // 未捕获的移动点（例如按下发生在上一局）不进入队列。
                return;
            }

            module.QueuePointerSample(point.id, point.pos, pressed, released, held, true);
        }

        // 供原先直接读裸 Input.mousePosition 的调用点使用。
        // 绑定模式下只有画布所在屏的按下才算数；未绑定时调用方继续读 Input。
        internal static bool TryGetPress(out Vector2 position)
        {
            position = lastPressPosition;
            // 允许一帧错位：触摸回调可能晚于游戏 Update 落到主循环。
            return pressFrame >= 0 && Time.frameCount <= pressFrame + 1;
        }

        // 是否还有未抬起的触点。绑定模式下裸 Input 看不到触摸，抬起判定要用这里。
        internal static bool AnyPointerCaptured
        {
            get { return capturedPointers.Count > 0; }
        }
    }
}
