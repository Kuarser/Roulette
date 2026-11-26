using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RouletteDeluxe
{
    // PC input adapter for the original player queue. State-entry purges apply
    // to ordinary and demo play. The queue owns event ordering and purge;
    // uGUI continues to own hit testing and pointer capture.
    public sealed class RouletteSourceInputModule : StandaloneInputModule
    {
        private RouletteGame game;
        private PointerEventData lastPointer;
        private readonly Dictionary<int, Vector2> capturedPositions =
            new Dictionary<int, Vector2>();
        private readonly HashSet<PointerEventData> dispatchingPress =
            new HashSet<PointerEventData>();
        private readonly Dictionary<PointerEventData, bool> deferredReleases =
            new Dictionary<PointerEventData, bool>();

        public void Configure(RouletteGame owner)
        {
            game = owner;
            forceModuleActive = true;
        }

        public override void Process()
        {
            // EventSystem can retain its last current module after removing
            // disabled modules from the candidate list. Respect this adapter's
            // lifecycle before capturing or servicing any game input.
            if (!isActiveAndEnabled) return;
            if (game != null && game.PcAutoPlayStorageFaulted) return;
            if (game != null && !game.PcGameOrdinaryInputAllowedForModule)
            {
                // App's suspension wait still receives events and services
                // IPC. Keep ordinary input for the real resume purge; do not
                // let StandaloneInputModule or hover/drag dispatch bypass it.
                CapturePointerEvents();
                CaptureButton(KeyCode.Space);
                CaptureButton(KeyCode.D);
                CaptureButton(KeyCode.F8);
                game.ServicePcAutoComplete();
                if(game.PcSasRuntimeServicePendingForModule)game.ServiceSourceRoundLifecycle();
                return;
            }
            if (game == null || !game.UsesSourcePlayerInput)
            {
                if (game != null)
                {
                    game.ServiceSourceRoundLifecycle();
                    game.ServiceSourceBetStateEntry();
                    game.ServiceConfiguredAutoPlay();
                    game.ServicePcAutoComplete();
                }
                if (game != null && (game.PcAutoPlayStorageFaulted ||
                    !game.PcGameOrdinaryInputAllowedForModule)) return;
                // 这条分支属于 PaytableTest / OperatorFallback 诊断模式，其指针
                // 事件仍依赖 StandaloneInputModule 的全局采样，保持原行为。
                base.Process();
                return;
            }
            CapturePointerEvents();
            CaptureButton(KeyCode.Space);
            CaptureButton(KeyCode.D);
            CaptureButton(KeyCode.F8);
            // Input arriving at a native state-entry boundary is still queued
            // when CheckAutoPlay/PurgeInputs runs. Dispatch removes its head
            // before invoking a menu or gameplay callback.
            game.ServiceSourceRoundLifecycle();
            game.ServiceSourceBetStateEntry();
            game.ServiceConfiguredAutoPlay();
            game.ServicePcAutoComplete();
            game.ServiceSourcePlayerInput();
            if (game.PcAutoPlayStorageFaulted ||
                !game.PcGameOrdinaryInputAllowedForModule) return;
            // 绑定模式下指针由队列派发，lastPointer 可能残留绑定前的全局采样，
            // 继续 ProcessMove 会让 hover 停在过期位置。
            if (lastPointer != null && !RouletteTouchFeed.Bound) ProcessMove(lastPointer);
        }

        private void CaptureButton(KeyCode key)
        {
            if (Input.GetKeyDown(key))
                game.SourcePlayerInputs.Enqueue(0x200010,
                    () => game.DispatchSourceButton(key));
            if (Input.GetKeyUp(key))
                game.SourcePlayerInputs.Enqueue(OriginalPlayerInputQueue.ButtonRelease);
        }

        private void CapturePointerEvents()
        {
            // 屏幕绑定生效时，指针事件由 MyTouchComponent 按绑定关系投递
            // （RouletteTouchFeed.Forward）。这里不再读全局 Input，否则另一块屏的
            // 点击也会被当成轮盘画布的事件。
            if (RouletteTouchFeed.Bound) return;
            if (input.touchCount > 0)
            {
                for (int i = 0; i < input.touchCount; ++i)
                {
                    Touch touch = input.GetTouch(i);
                    if (touch.type == TouchType.Indirect) continue;
                    bool pressed, released;
                    PointerEventData pointer = GetTouchPointerEventData(touch,
                        out pressed, out released);
                    CapturePointerSample(pointer, pressed, released, true, true,
                        touch.phase == TouchPhase.Canceled);
                }
                return;
            }
            if (!input.mousePresent) return;
            var left = GetMousePointerEventData(0)
                .GetButtonState(PointerEventData.InputButton.Left).eventData;
            bool down = left.PressedThisFrame(), up = left.ReleasedThisFrame();
            CapturePointerSample(left.buttonData, down, up,
                input.GetMouseButton(0) && Cursor.lockState != CursorLockMode.Locked,
                false, false);
            lastPointer = left.buttonData;
        }

        // The diagnostic supplies raw device samples through the same capture
        // path as the PC mouse/touch adapter. It never invokes UI handlers itself.
        internal PointerEventData QueuePointerSample(int pointerId, Vector2 position,
            bool pressed, bool released, bool held, bool touch)
        {
            PointerEventData pointer;
            GetPointerData(pointerId, out pointer, true);
            pointer.position = position;
            pointer.button = PointerEventData.InputButton.Left;
            CapturePointerSample(pointer, pressed, released, held, touch, false);
            return pointer;
        }

        // 设备停止上报时不会有 Ended，按 PurgeInputs 的语义补一次"不派发点击"的
        // 抬起，保证 pointerPress/pointerDrag 被清掉但不会误触下注。
        internal void QueuePointerCancel(int pointerId)
        {
            if (game == null) return;
            PointerEventData pointer;
            GetPointerData(pointerId, out pointer, true);
            game.SourcePlayerInputs.Enqueue(OriginalPlayerInputQueue.TouchRelease,
                () => ReleasePurgedPointer(pointer, true));
        }

        private void CapturePointerSample(PointerEventData pointer, bool pressed,
            bool released, bool held, bool touch, bool canceled)
        {
            Vector2 position = pointer.position, previous;
            // uGUI reuses this PointerEventData for capture and dispatch. A
            // queued event can restore an older position between physical
            // samples, so that object cannot define the next physical delta.
            Vector2 delta = !pressed && capturedPositions.TryGetValue(
                pointer.pointerId, out previous) ? position - previous : Vector2.zero;
            capturedPositions[pointer.pointerId] = position;
            bool moved = !pressed && !released && held && delta.sqrMagnitude > 0;
            if (pressed) game.SourcePlayerInputs.Enqueue(0x200001,
                () => DispatchPointer(pointer, position, delta, true, false, false, touch, false));
            if (moved) game.SourcePlayerInputs.Enqueue(0x200002,
                () => DispatchPointer(pointer, position, delta, false, false, true, touch, false));
            if (released) game.SourcePlayerInputs.Enqueue(OriginalPlayerInputQueue.TouchRelease,
                () => DispatchPointer(pointer, position, delta, false, true, false, touch, canceled),
                () => ReleasePurgedPointer(pointer, touch));
            if (touch && released)
            {
                capturedPositions.Remove(pointer.pointerId);
                // A later physical touch may reuse this finger ID while its
                // old release is still queued. Give the new contact its own
                // capture object without changing the old queued reference.
                RemovePointerData(pointer);
            }
        }

        private void DispatchPointer(PointerEventData pointer, Vector2 position,
            Vector2 delta, bool pressed, bool released, bool moved, bool touch,
            bool noHit)
        {
            pointer.position = position;
            pointer.delta = delta;
            // CVideo::Dispatch resolves the current sprite at dequeue time.
            // A menu opened by an earlier queued down must receive later
            // events even if it was absent when those events were sampled.
            if (noHit) pointer.pointerCurrentRaycast = new RaycastResult();
            else
            {
                eventSystem.RaycastAll(pointer, m_RaycastResultCache);
                pointer.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
                m_RaycastResultCache.Clear();
            }
            if (pressed) dispatchingPress.Add(pointer);
            try
            {
                // The rack-snap prefix can finish a result and purge input
                // before uGUI's OnPointerDown has established capture too.
                game.BeforeSourcePointerDispatch(pressed ? 0x200001U :
                    moved ? 0x200002U : OriginalPlayerInputQueue.TouchRelease);
                if (touch) ProcessTouchPress(pointer, pressed, released);
                else if (pressed || released)
                    ProcessMousePress(new MouseButtonEventData {
                        buttonData = pointer,
                        buttonState = pressed ? PointerEventData.FramePressState.Pressed :
                            PointerEventData.FramePressState.Released
                    });
            }
            finally
            {
                if (pressed)
                {
                    dispatchingPress.Remove(pointer);
                    bool releaseTouch;
                    if (deferredReleases.TryGetValue(pointer, out releaseTouch))
                    {
                        deferredReleases.Remove(pointer);
                        ReleasePurgedPointer(pointer, releaseTouch);
                    }
                }
            }
            if (moved) ProcessDrag(pointer);
            // 绑定模式下 lastPointer 不再由全局 Input 采样，hover 只能在队列派发时
            // 补齐，与 StandaloneInputModule.ProcessTouchEvents 的 move 分支一致。
            if (moved && RouletteTouchFeed.Bound) ProcessMove(pointer);
            if (touch && released && ReferenceEquals(
                GetLastPointerEventData(pointer.pointerId), pointer))
                RemovePointerData(pointer);
        }

        private void ReleasePurgedPointer(PointerEventData pointer, bool touch)
        {
            // ProcessMousePress/ProcessTouchPress assign pointerPress and
            // pointerDrag after invoking OnPointerDown. A down callback can
            // purge an already queued up before those assignments exist.
            if (dispatchingPress.Contains(pointer))
            {
                deferredReleases[pointer] = touch;
                return;
            }
            // Video::PurgeInputs synthesizes a captured release at (0,0)
            // before clearing capture. It does not dispatch a click/drop to
            // the original release location.
            pointer.eligibleForClick = false;
            DispatchPointer(pointer, Vector2.zero, Vector2.zero,
                false, true, false, touch, true);
        }
    }
}
