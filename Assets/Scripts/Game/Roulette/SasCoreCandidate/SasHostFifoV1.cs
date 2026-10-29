using System;
using System.Collections.Generic;
using System.Threading;
namespace RouletteDeluxe.SasDraftV1
{
    public sealed class SasQueuedHostEventV1
    {
        public readonly string Source, Id;
        public readonly long Sequence;
        private readonly byte[] wire;
        public SasQueuedHostEventV1(string source, string id, long sequence, byte[] bytes)
        {
            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(id) || sequence < 0)
                throw new ArgumentException("Host event identity required");
            SasHostMessage.Decode(bytes);
            Source=source; Id=id; Sequence=sequence; wire=(byte[])bytes.Clone();
        }
        public byte[] CopyWire() { return (byte[])wire.Clone(); }
    }
    // Transport only: durable event deduplication belongs to the coordinator.
    public sealed class SasHostFifoV1
    {
        private readonly Queue<SasQueuedHostEventV1> queue=new Queue<SasQueuedHostEventV1>();
        private readonly int owner=Thread.CurrentThread.ManagedThreadId;
        private bool servicing;
        public bool Faulted { get; private set; }
        public int PendingCount { get { lock(queue) return queue.Count; } }
        public void Enqueue(SasQueuedHostEventV1 item)
        {
            if(item==null) throw new ArgumentNullException("item");
            lock(queue) queue.Enqueue(item);
        }
        public int Service(int limit, Action<SasQueuedHostEventV1> apply)
        {
            if(Thread.CurrentThread.ManagedThreadId!=owner)
                throw new InvalidOperationException("Host service requires owner thread");
            if(servicing) throw new InvalidOperationException("Recursive host service rejected");
            if(Faulted) throw new InvalidOperationException("Host service faulted; explicit recovery required");
            if(limit<0 || apply==null) throw new ArgumentException("Invalid service arguments");
            servicing=true;
            try
            {
                int completed=0;
                while(completed<limit)
                {
                    SasQueuedHostEventV1 head;
                    lock(queue) { if(queue.Count==0) break; head=queue.Peek(); }
                    // No queue lock during callbacks; background producers can enqueue.
                    // Removal follows successful coordinator commit/publication only.
                    apply(head);
                    lock(queue) queue.Dequeue();
                    completed++;
                }
                return completed;
            }
            catch { Faulted=true; throw; }
            finally { servicing=false; }
        }
    }
}
