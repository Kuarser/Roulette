using System;
using System.Collections.Generic;
namespace RouletteDeluxe.SasDraftV1
{
    public sealed class SasEventReceiptV1
    {
        public readonly SasQueuedHostEventV1 Event;
        public readonly long Generation;
        public SasEventReceiptV1(SasQueuedHostEventV1 item,long generation)
        { if(item==null || generation<0) throw new ArgumentException("Invalid event receipt"); Event=item;Generation=generation; }
    }
    // Pure index only. The entire durable catalogue must be committed with config;
    // this type does not serialize, write storage or restore a deferred disable.
    public sealed class SasEventIndexV1
    {
        private readonly Dictionary<string,Dictionary<string,SasQueuedHostEventV1>> seen =
            new Dictionary<string,Dictionary<string,SasQueuedHostEventV1>>(StringComparer.Ordinal);
        private readonly List<SasEventReceiptV1> durable=new List<SasEventReceiptV1>();
        public SasEventIndexV1(IEnumerable<SasEventReceiptV1> restored)
        {
            if(restored==null) throw new ArgumentNullException("restored");
            long last=-1;
            foreach(var receipt in restored)
            {
                if(receipt==null || receipt.Generation<=last || IsDuplicate(receipt.Event))
                    throw new ArgumentException("Invalid durable event catalogue order or duplicate");
                Add(receipt.Event);durable.Add(receipt);last=receipt.Generation;
            }
        }
        public bool IsDuplicate(SasQueuedHostEventV1 item)
        {
            if(item==null)throw new ArgumentNullException("item");
            Dictionary<string,SasQueuedHostEventV1> byId;SasQueuedHostEventV1 prior;
            if(!seen.TryGetValue(item.Source,out byId) || !byId.TryGetValue(item.Id,out prior)) return false;
            if(prior.Sequence!=item.Sequence)throw new InvalidOperationException("Event ID reused with different sequence");
            byte[] a=prior.CopyWire(),b=item.CopyWire();
            for(int i=0;i<a.Length;i++)if(a[i]!=b[i])throw new InvalidOperationException("Event ID reused with different bytes");
            return true;
        }
        public SasEventReceiptV1[] CandidateDurable(SasQueuedHostEventV1 item,long generation)
        {
            if(IsDuplicate(item))throw new InvalidOperationException("Duplicate cannot create a receipt");
            if(durable.Count>0 && generation<=durable[durable.Count-1].Generation)
                throw new ArgumentException("Receipt generation did not advance");
            var list=new List<SasEventReceiptV1>(durable);list.Add(new SasEventReceiptV1(item,generation));return list.ToArray();
        }
        public void PublishDurable(SasQueuedHostEventV1 item,long generation)
        {
            var candidate=CandidateDurable(item,generation);
            Add(item);durable.Add(candidate[candidate.Length-1]);
        }
        public void PublishVolatile(SasQueuedHostEventV1 item)
        { if(!IsDuplicate(item))Add(item); }
        public SasEventReceiptV1[] CopyDurable() {return durable.ToArray();}
        private void Add(SasQueuedHostEventV1 item)
        {
            Dictionary<string,SasQueuedHostEventV1> byId;
            if(!seen.TryGetValue(item.Source,out byId))
            {byId=new Dictionary<string,SasQueuedHostEventV1>(StringComparer.Ordinal);seen.Add(item.Source,byId);}
            byId.Add(item.Id,item);
        }
    }
}
