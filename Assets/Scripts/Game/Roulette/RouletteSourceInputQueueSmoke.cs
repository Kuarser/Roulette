using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private IEnumerator SourceInputQueueSmoke()
        {
            yield return null;
            if (demoShowMode != 1 || sourceInputModule == null || session == null ||
                spinning || autoplay || demoMenuVisible || SourcePlayerInputs.Count != 0 ||
                recordedHistoryKey.IndexOf(".Test.", StringComparison.Ordinal) < 0 ||
                pcPlayerVolumeKey == DefaultPcPlayerVolumeKey)
                throw new InvalidOperationException(
                    "Source input diagnostic needs isolated idle demo history and volume");

            string before = PaytableTestLiveFingerprint(session);
            OriginalAutoPlayData savedConfig = configuredAutoPlay.Clone();
            bool savedPending = configuredAutoPlayCheckPending;
            bool savedEnabled = sourceInputModule.enabled;
            bool savedMuted = pcGameSoundsMuted;
            var canvasObject = new GameObject("Source input diagnostic canvas",
                typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32760;
            int checks = 0;
            try
            {
                sourceInputModule.enabled = false;
                configuredAutoPlayCheckPending = false;
                configuredAutoPlay = new OriginalAutoPlayData();
                RouletteSourceInputProbe first = CreateSourceInputProbe(canvas.transform, "First");
                RouletteSourceInputProbe second = CreateSourceInputProbe(canvas.transform, "Second");
                second.gameObject.SetActive(false);
                yield return null;
                Canvas.ForceUpdateCanvases();
                Vector2 center = RectTransformUtility.WorldToScreenPoint(null,
                    first.transform.TransformPoint(((RectTransform)first.transform).rect.center));
                const int mouse = PointerInputModule.kMouseLeftId;

                // Both downs arrive before the first callback opens a covering
                // UI layer. The second down must use the newly visible target.
                first.DownAction = () => second.gameObject.SetActive(true);
                sourceInputModule.QueuePointerSample(mouse, center, true, false, true, false);
                sourceInputModule.QueuePointerSample(mouse, center, false, true, false, false);
                sourceInputModule.QueuePointerSample(mouse, center, true, false, true, false);
                PointerEventData pointer = sourceInputModule.QueuePointerSample(
                    mouse, center, false, true, false, false);
                if (SourcePlayerInputs.Count != 4 || !SourcePlayerInputs.DispatchOne() ||
                    first.DownCount != 1 || second.DownCount != 0)
                    throw new InvalidOperationException("First queued down did not open the new UI layer");
                ++checks;
                yield return null;
                Canvas.ForceUpdateCanvases();
                while (SourcePlayerInputs.DispatchOne()) { }
                if (first.DownCount != 1 || first.UpCount != 1 || first.ClickCount != 0 ||
                    second.DownCount != 1 || second.UpCount != 1 || second.ClickCount != 1)
                    throw new InvalidOperationException("Queued input used its capture-time UI target");
                RequireSourcePointerReleased(pointer);
                ++checks;
                first.DownAction = null;

                // A down callback purges its queued up before uGUI assigns
                // pointerPress. The adapter must finish capture then release
                // it once, without a click or a configured AutoData reset.
                foreach (bool touch in new[] { false, true })
                {
                    configuredAutoPlay.SetOptions(true, true, true, true, true, 0);
                    SetPcGameSoundsMuted(true);
                    int upBefore = second.UpCount, clicksBefore = second.ClickCount;
                    int deckBefore = second.DeckReleases;
                    second.DownAction = () =>
                    {
                        if (SourcePlayerInputs.Count != 1)
                            throw new InvalidOperationException("Dispatch retained its current queue head");
                        PurgeSourcePlayerInputs();
                    };
                    int id = touch ? 93 : mouse;
                    sourceInputModule.QueuePointerSample(id, center, true, false, true, touch);
                    pointer = sourceInputModule.QueuePointerSample(id, center, false, true, false, touch);
                    SourcePlayerInputs.DispatchOne();
                    if (SourcePlayerInputs.Count != 0 || second.UpCount != upBefore + 1 ||
                        second.DeckReleases != deckBefore + 1 ||
                        second.ClickCount != clicksBefore || second.UpPosition != Vector2.zero ||
                        configuredAutoPlay.AutoPlay != 1 || !configuredAutoPlay.Turbo ||
                        !pcGameSoundsMuted)
                        throw new InvalidOperationException("Callback purge lost/repeated its captured release: " + touch);
                    RequireSourcePointerReleased(pointer);
                    if (touch && pointer.pointerEnter != null)
                        throw new InvalidOperationException("Purged touch retained pointerEnter");
                    sourceInputModule.QueuePointerSample(id, center, false, true, false, touch);
                    SourcePlayerInputs.DispatchOne();
                    if (second.UpCount != upBefore + 1 || second.DeckReleases != deckBefore + 1 ||
                        second.ClickCount != clicksBefore)
                        throw new InvalidOperationException("Physical up repeated the purged deck release");
                    ++checks;
                }

                // The release-only purge above preserves config. An additional
                // pending move is removed by pass three and resets it.
                second.DownAction = () => PurgeSourcePlayerInputs();
                configuredAutoPlay.SetOptions(true, true, true, true, true, 0);
                SetPcGameSoundsMuted(true);
                sourceInputModule.QueuePointerSample(mouse, center, true, false, true, false);
                sourceInputModule.QueuePointerSample(mouse, center + Vector2.right,
                    false, false, true, false);
                pointer = sourceInputModule.QueuePointerSample(mouse, center,
                    false, true, false, false);
                SourcePlayerInputs.DispatchOne();
                if (SourcePlayerInputs.Count != 0 || configuredAutoPlay.AutoPlay != 0 ||
                    configuredAutoPlay.Turbo || configuredAutoPlay.ReplenishCredits ||
                    pcGameSoundsMuted)
                    throw new InvalidOperationException("Pending move did not reset config during callback purge");
                RequireSourcePointerReleased(pointer);
                ++checks;
                second.DownAction = null;

                // Dispatch restores old coordinates while newer physical
                // samples are waiting. Sampling an unmoved held pointer must
                // neither append a phantom move nor distort queued deltas.
                second.DragDeltas.Clear();
                Vector2 next = center + new Vector2(20, 0);
                Vector2 last = center + new Vector2(45, 5);
                sourceInputModule.QueuePointerSample(mouse, center, true, false, true, false);
                sourceInputModule.QueuePointerSample(mouse, next, false, false, true, false);
                sourceInputModule.QueuePointerSample(mouse, last, false, false, true, false);
                SourcePlayerInputs.DispatchOne();
                sourceInputModule.QueuePointerSample(mouse, last, false, false, true, false);
                if (SourcePlayerInputs.Count != 2)
                    throw new InvalidOperationException("Queued down created a phantom physical movement");
                ++checks;
                SourcePlayerInputs.DispatchOne();
                sourceInputModule.QueuePointerSample(mouse, last, false, false, true, false);
                if (SourcePlayerInputs.Count != 1)
                    throw new InvalidOperationException("Queued move created a phantom physical movement");
                SourcePlayerInputs.DispatchOne();
                if (second.DragDeltas.Count != 2 || second.DragDeltas[0] != next - center ||
                    second.DragDeltas[1] != last - next)
                    throw new InvalidOperationException("Input backlog changed physical movement deltas");
                ++checks;
                pointer = sourceInputModule.QueuePointerSample(mouse, last, false, true, false, false);
                PurgeSourcePlayerInputs();
                RequireSourcePointerReleased(pointer);
                ++checks;

                // A platform may reuse a finger ID before the old queued up
                // dispatches. The old release must not erase the new contact.
                const int finger = 94;
                sourceInputModule.QueuePointerSample(finger, center, true, false, true, true);
                PointerEventData oldTouch = sourceInputModule.QueuePointerSample(
                    finger, center, false, true, false, true);
                PointerEventData newTouch = sourceInputModule.QueuePointerSample(
                    finger, center, true, false, true, true);
                if (ReferenceEquals(oldTouch, newTouch))
                    throw new InvalidOperationException("Finger ID reuse retained the old capture object");
                while (SourcePlayerInputs.DispatchOne()) { }
                RequireSourcePointerReleased(oldTouch);
                PointerEventData sameNewTouch = sourceInputModule.QueuePointerSample(
                    finger, center, false, true, false, true);
                if (!ReferenceEquals(newTouch, sameNewTouch))
                    throw new InvalidOperationException("Old touch release erased the new contact");
                SourcePlayerInputs.DispatchOne();
                RequireSourcePointerReleased(newTouch);
                ++checks;
            }
            finally
            {
                SourcePlayerInputs.Purge(0, 0);
                canvasObject.SetActive(false);
                Destroy(canvasObject);
                configuredAutoPlay = savedConfig;
                configuredAutoPlayCheckPending = savedPending;
                SetPcGameSoundsMuted(savedMuted);
                sourceInputModule.enabled = savedEnabled;
            }
            if (before != PaytableTestLiveFingerprint(session))
                throw new InvalidOperationException("Source input diagnostic changed funds/history/RNG");
            Debug.Log("ROULETTE_SOURCE_INPUT_QUEUE_SMOKE_OK: " + checks +
                " actual adapter checks; dispatch-time target; callback purge capture cleanup; " +
                "mouse/touch; config pass order; physical delta backlog; finger reuse; session isolation");
            Application.Quit();
        }

        private static RouletteSourceInputProbe CreateSourceInputProbe(Transform parent, string name)
        {
            var node = new GameObject(name, typeof(RectTransform), typeof(Image),
                typeof(Button), typeof(RouletteSourceInputProbe));
            node.transform.SetParent(parent, false);
            RectTransform rect = (RectTransform)node.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(300, 300);
            node.GetComponent<Image>().color = new Color(0.2f, 0.3f, 0.5f, 1);
            RouletteSourceInputProbe probe = node.GetComponent<RouletteSourceInputProbe>();
            node.AddComponent<RouletteDeckReleaseAction>().Configure(node.GetComponent<Button>(),
                () => ++probe.DeckReleases, true);
            return probe;
        }

        private static void RequireSourcePointerReleased(PointerEventData pointer)
        {
            if (pointer.pointerPress != null || pointer.rawPointerPress != null ||
                pointer.pointerDrag != null || pointer.eligibleForClick || pointer.dragging)
                throw new InvalidOperationException("Source release retained uGUI capture/drag/click state");
        }
    }

    internal sealed class RouletteSourceInputProbe : MonoBehaviour,
        IPointerDownHandler, IPointerUpHandler, IPointerClickHandler,
        IInitializePotentialDragHandler, IDragHandler
    {
        internal Action DownAction;
        internal int DownCount, UpCount, ClickCount, DeckReleases;
        internal Vector2 UpPosition;
        internal readonly List<Vector2> DragDeltas = new List<Vector2>();

        public void OnPointerDown(PointerEventData data)
        {
            ++DownCount;
            if (DownAction != null) DownAction();
        }
        public void OnPointerUp(PointerEventData data) { ++UpCount; UpPosition = data.position; }
        public void OnPointerClick(PointerEventData data) { ++ClickCount; }
        public void OnInitializePotentialDrag(PointerEventData data) { data.useDragThreshold = false; }
        public void OnDrag(PointerEventData data) { DragDeltas.Add(data.delta); }
    }
}
