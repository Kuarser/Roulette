using System;
using System.Collections.Generic;

namespace RouletteDeluxe
{
    // Event-type FIFO corresponding to bally::Video::inputQ. Target lookup,
    // captured sprite identity and platform input delivery remain the adapter's
    // responsibility; releaseCleanup models the native captured-release callback.
    public sealed class OriginalPlayerInputQueue
    {
        public const uint TouchRelease = 0x200004;
        public const uint ButtonRelease = 0x200020;

        private sealed class InputEvent
        {
            public uint Type;
            public Action Dispatch;
            public Action ReleaseCleanup;
        }

        private readonly LinkedList<InputEvent> events = new LinkedList<InputEvent>();

        public int Count { get { return events.Count; } }

        // Video::InputAvailable scans without removing an event. In the
        // rollup WaitInput path a matching down can be behind a release;
        // the subsequent ProcessInputs still dispatches the original head.
        public bool InputAvailable(uint mask, uint value)
        {
            for (LinkedListNode<InputEvent> current = events.First;
                current != null; current = current.Next)
                if ((current.Value.Type & mask) == value) return true;
            return false;
        }

        public void Enqueue(uint eventType, Action dispatch = null, Action releaseCleanup = null)
        {
            events.AddLast(new InputEvent
            {
                Type = eventType,
                Dispatch = dispatch,
                ReleaseCleanup = releaseCleanup
            });
        }

        // 0x08818c90: ProcessInputs unlinks the current head before Dispatch.
        // A callback which purges input therefore cannot purge its own event.
        public bool DispatchOne(Action<uint> observer = null)
        {
            LinkedListNode<InputEvent> current = events.First;
            if (current == null) return false;
            InputEvent input = current.Value;
            events.Remove(current);
            if (observer != null) observer(input.Type);
            if (input.Dispatch != null) input.Dispatch();
            return true;
        }

        // 0x08818d00: match (event.type & filter.mask) == filter.value.
        // Release cleanup precedes unlinking, as in the native captured-sprite
        // release path. Events appended by cleanup remain part of this scan.
        // Recursive removal of the current node from cleanup is not a recovered
        // native contract; adapters should use cleanup only for capture release.
        public bool Purge(uint mask, uint value, Action<uint> releaseCleanup = null)
        {
            bool removed = false;
            LinkedListNode<InputEvent> current = events.First;
            while (current != null)
            {
                if ((current.Value.Type & mask) != value)
                {
                    current = current.Next;
                    continue;
                }

                InputEvent input = current.Value;
                if (input.Type == TouchRelease || input.Type == ButtonRelease)
                {
                    if (input.ReleaseCleanup != null) input.ReleaseCleanup();
                    if (releaseCleanup != null) releaseCleanup(input.Type);
                }

                LinkedListNode<InputEvent> next = current.Next;
                events.Remove(current);
                removed = true;
                current = next;
            }
            return removed;
        }

        // 0x080f9070: release passes always precede the requested filter.
        // Only removal by that final pass can reset configured AutoData and
        // request TurnOnAllSounds; this method does not change active AutoPlay.
        public bool PurgeGameInputs(OriginalAutoPlayData autoData,
            bool demoAvailable, uint mask, uint value, out bool restoreSounds,
            Action<uint> releaseCleanup = null)
        {
            if (autoData == null) throw new ArgumentNullException("autoData");
            bool touchReleased = Purge(uint.MaxValue, TouchRelease, releaseCleanup);
            bool buttonReleased = Purge(uint.MaxValue, ButtonRelease, releaseCleanup);
            bool filteredRemoved = Purge(mask, value, releaseCleanup);
            restoreSounds = autoData.ResetForPurgedInput(demoAvailable, filteredRemoved);
            return touchReleased || buttonReleased || filteredRemoved;
        }
    }
}
