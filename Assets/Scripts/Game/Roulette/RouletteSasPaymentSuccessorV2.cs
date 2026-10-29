using RouletteDeluxe.SasDraftV1;
namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private static void RequireSasPaymentSuccessorV2(PcGamePaymentRecordV2 before,PcGamePaymentRecordV2 after,bool applied)
        {
            if(before==null||after==null)throw new PcRoundLifecycleDataException("Payment successor missing");
            before.Validate(applied);after.Validate(applied);PcGamePaymentRecordV2 expected;
            if(before.owner!=after.owner)
                expected=PcGamePaymentTransitionsV2.TransferToDouble(before,applied,after.featureId);
            else if(before.baseOffer!=after.baseOffer)
            {
                if(after.baseOffer==PcGamePaymentOffer.Open)expected=PcGamePaymentTransitionsV2.OpenOffer(before,applied);
                else if(after.baseOffer==PcGamePaymentOffer.Take)expected=PcGamePaymentTransitionsV2.TakeOffer(before,applied);
                else if(after.baseOffer==PcGamePaymentOffer.Enter)expected=PcGamePaymentTransitionsV2.EnterOffer(before,applied);
                else throw new PcRoundLifecycleDataException("Unsupported payment offer successor");
            }
            else if(!before.prepared&&after.prepared)expected=PcGamePaymentTransitionsV2.Prepare(before,applied,after.finalAward,after.routeObservation);
            else if(!before.queryPrepared&&after.queryPrepared)expected=PcGamePaymentTransitionsV2.PrepareQuery(before,applied);
            else if(!before.queryResponseSaved&&after.queryResponseSaved)expected=PcGamePaymentTransitionsV2.SaveQueryResponse(before,applied,after.queryResponseValue);
            else if(!before.packetPrepared&&after.packetPrepared)expected=PcGamePaymentTransitionsV2.PreparePacket(before,applied,after.packetObservation);
            else if(!before.dispatchReturned&&after.dispatchReturned)expected=PcGamePaymentTransitionsV2.SaveDispatchReturn(before,applied,after.dispatchReturnValue);
            else if(!before.moneyApplied&&after.moneyApplied)expected=PcGamePaymentTransitionsV2.Apply(before,applied);
            else if(!before.creditNotified&&after.creditNotified)expected=PcGamePaymentTransitionsV2.NotifyCredit(before,applied);
            else if(!before.completionReceived&&after.completionReceived)expected=PcGamePaymentTransitionsV2.Complete(before,applied);
            else if(!before.zeroResolved&&after.zeroResolved)expected=PcGamePaymentTransitionsV2.ResolveZero(before,applied);
            else if(!before.historyPublished&&after.historyPublished)expected=PcGamePaymentTransitionsV2.Publish(before,applied);
            else if(before.route==PcGamePaymentRouteV2.Ordinary&&before.prepared&&before.finalAward>0&&before.replyValue==0&&!before.moneyApplied)
                expected=PcGamePaymentTransitionsV2.SaveOrdinaryReply(before,applied,after.replyValue);
            else throw new PcRoundLifecycleDataException("Payment successor has no permitted event");
            if(WriteValidatedSasPaymentV2(expected)!=WriteValidatedSasPaymentV2(after))
                throw new PcRoundLifecycleDataException("Payment successor changed more than its one event");
        }
    }
}
