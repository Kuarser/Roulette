using System;
using System.Threading;
namespace RouletteDeluxe.SasDraftV1
{
    // Output-only coordinator candidate; caller must prove native Idle ownership.
    public sealed class SasHostConfigCoordinatorV3
    {
        private readonly int owner=Thread.CurrentThread.ManagedThreadId;
        private readonly string scope;
        private readonly Func<long,SasConfigSnapshotV2,SasConfigSnapshotV2> save;
        private readonly SasHostFifoV1 fifo=new SasHostFifoV1();
        private SasEventIndexV1 index;
        private bool servicing;
        private SasIdleCompletedReceiptV1 idleReceipt;
        private SasConfigEnvelopeV1 durableConfig;
        public SasPayload Payload {get;private set;}
        public bool PendingDisable {get;private set;}
        public long Generation {get;private set;}
        public bool Faulted {get;private set;}
        public int PendingMessages {get{return fifo.PendingCount;}}
        public SasHostConfigCoordinatorV3(string expectedScope,SasConfigSnapshotV2 restored,
            Func<long,SasConfigSnapshotV2,SasConfigSnapshotV2> persistence)
        {
            if(string.IsNullOrWhiteSpace(expectedScope)||persistence==null ||
                (restored!=null && restored.Config.Scope!=expectedScope))throw new ArgumentException("Invalid coordinator initialization");
            scope=expectedScope;save=persistence;
            Payload=restored==null?SasPayload.Disabled():restored.Config.Payload;
            Generation=restored==null?-1:restored.Config.Generation;
            index=new SasEventIndexV1(restored==null?new SasEventReceiptV1[0]:restored.CopyReceipts());
            PendingDisable=false;
            durableConfig=restored==null?null:restored.Config;
            idleReceipt=restored==null?null:restored.IdleReceipt;
        }
        public SasConfigObservationReceiptV1 CaptureObservation(SasObservationPoint point)
        {
            if(Thread.CurrentThread.ManagedThreadId!=owner || servicing || Faulted || durableConfig==null)
                throw new InvalidOperationException("Observation requires healthy owner boundary and durable configuration");
            return new SasConfigObservationReceiptV1(new SasObservation(point,Generation,Payload,PendingDisable),
                durableConfig.LastDurableEventSource,durableConfig.LastDurableEventId,durableConfig.LastDurableEventSequence);
        }
        public void Enqueue(SasQueuedHostEventV1 item){fifo.Enqueue(item);}
        public int ServiceMessages(int nativeMainState,int limit)
        {
            if(Thread.CurrentThread.ManagedThreadId!=owner)throw new InvalidOperationException("Owner thread required");
            if(servicing)throw new InvalidOperationException("Recursive coordinator dispatch rejected");
            if(Faulted)throw new InvalidOperationException("Coordinator faulted");
            if(nativeMainState<0 || limit<0)throw new ArgumentException("Explicit native state and nonnegative limit required");
            servicing=true;
            try{return fifo.Service(limit,item=>Apply(item,nativeMainState));}
            catch{Faulted=true;throw;}
            finally{servicing=false;}
        }
        public int ServiceIdleEntry(string entryId,bool selected,bool suspended,bool runtimeReady,
            bool nativeIdleProven,bool beyondBegin)
        {
            if(Thread.CurrentThread.ManagedThreadId!=owner || servicing)throw new InvalidOperationException("Invalid Idle dispatcher ownership");
            if(Faulted)throw new InvalidOperationException("Coordinator faulted");
            if(string.IsNullOrWhiteSpace(entryId))throw new ArgumentException("Idle entry identity required");
            if(!selected || suspended || !runtimeReady || !nativeIdleProven)return 0;
            servicing=true;
            try
            {
                if(idleReceipt!=null && idleReceipt.IdleEntryId==entryId)return 0;
                if(beyondBegin)throw new InvalidOperationException("Prepared SAS Idle lacks its completion receipt");
                int writes=0;
                if(Payload.PendingEnable)
                {CommitIdle(SasConfigRules.ActivateAtIdle(Payload,PendingDisable,1),null);writes++;}
                if(PendingDisable)
                {CommitIdle(SasConfigRules.Receive(Payload,PendingDisable,1,SasHostMessage.Decode(SasHostMessage.Encode(false,0,0,0,0))),null);writes++;}
                long next=checked(Generation+1);
                CommitIdle(new SasConfigTransition(Payload,PendingDisable,true),
                    new SasIdleCompletedReceiptV1(scope,entryId,next,SasConfigSnapshotCodecV2.Digest(Payload.CopyBytes())));
                return writes+1;
            }
            catch{Faulted=true;throw;}
            finally{servicing=false;}
        }
        private void CommitIdle(SasConfigTransition transition,SasIdleCompletedReceiptV1 completion)
        {
            long next=checked(Generation+1);
            var prior=durableConfig;
            var config=new SasConfigEnvelopeV1(scope,next,transition.Payload,
                prior==null?"pc-bootstrap":prior.LastDurableEventSource,
                prior==null?"initial-zero":prior.LastDurableEventId,
                prior==null?0:prior.LastDurableEventSequence,
                prior==null?SasConfigSnapshotCodecV2.Digest(new byte[0]):prior.LastDurableEventSha256);
            var candidate=new SasConfigSnapshotV2(config,index.CopyDurable(),completion??idleReceipt);
            var readback=save(Generation,candidate);
            if(readback==null)throw new InvalidOperationException("Missing Idle readback");
            byte[] expected=SasConfigSnapshotCodecV2.Encode(candidate),actual=SasConfigSnapshotCodecV2.Encode(readback);
            if(expected.Length!=actual.Length)throw new InvalidOperationException("Idle readback differs");
            for(int i=0;i<expected.Length;i++)if(expected[i]!=actual[i])throw new InvalidOperationException("Idle readback differs");
            Payload=readback.Config.Payload;Generation=next;durableConfig=readback.Config;
            PendingDisable=transition.RuntimePendingDisable;idleReceipt=readback.IdleReceipt;
        }
        private void Apply(SasQueuedHostEventV1 item,int state)
        {
            if(index.IsDuplicate(item))return;
            var transition=SasConfigRules.Receive(Payload,PendingDisable,state,SasHostMessage.Decode(item.CopyWire()));
            if(!transition.SaveRequired)
            {index.PublishVolatile(item);PendingDisable=transition.RuntimePendingDisable;return;}
            long next=checked(Generation+1);
            var config=new SasConfigEnvelopeV1(scope,next,transition.Payload,item.Source,item.Id,item.Sequence,SasConfigSnapshotCodecV2.Digest(item.CopyWire()));
            var candidate=new SasConfigSnapshotV2(config,index.CandidateDurable(item,next),idleReceipt);
            var readback=save(Generation,candidate);
            if(readback==null)throw new InvalidOperationException("Missing persisted snapshot");
            byte[] expected=SasConfigSnapshotCodecV2.Encode(candidate),actual=SasConfigSnapshotCodecV2.Encode(readback);
            if(expected.Length!=actual.Length)throw new InvalidOperationException("Readback differs");
            for(int i=0;i<expected.Length;i++)if(expected[i]!=actual[i])throw new InvalidOperationException("Readback differs");
            // Validate everything before publishing any live field.
            new SasEventIndexV1(readback.CopyReceipts());
            // Preserve process-only events across durable saves.
            index.PublishDurable(item,next);
            Payload=readback.Config.Payload;Generation=next;PendingDisable=transition.RuntimePendingDisable;durableConfig=readback.Config;
        }
    }
}

