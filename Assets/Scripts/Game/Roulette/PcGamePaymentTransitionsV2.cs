using System;
using RouletteDeluxe.SasDraftV1;
namespace RouletteDeluxe
{
    // Pure receipt candidates. The archive owner must save/read back one event
    // before sending its request or installing its associated wallet effect.
    public static class PcGamePaymentTransitionsV2
    {
        private static PcGamePaymentRecordV2 Candidate(PcGamePaymentRecordV2 value,bool applied)
        { if(value==null)throw new PcRoundLifecycleDataException("V2 payment missing");value.Validate(applied);return value.Clone(); }
        private static void Require(bool v,string m){if(!v)throw new PcRoundLifecycleDataException(m);}
        private static PcGamePaymentRecordV2 Checked(PcGamePaymentRecordV2 p,bool applied){p.Validate(applied);return p;}
        public static PcGamePaymentRecordV2 OpenOffer(PcGamePaymentRecordV2 value,bool applied)
        {
            var p=Candidate(value,applied);Require(p.owner==PcGamePaymentOwner.Base&&!p.prepared&&p.baseAward>0&&p.baseOffer==PcGamePaymentOffer.None,"V2 offer cannot open");
            p.baseOffer=PcGamePaymentOffer.Open;return Checked(p,applied);
        }
        public static PcGamePaymentRecordV2 TakeOffer(PcGamePaymentRecordV2 value,bool applied)
        {
            var p=Candidate(value,applied);Require(p.owner==PcGamePaymentOwner.Base&&!p.prepared&&(p.baseOffer==PcGamePaymentOffer.Open||p.baseOffer==PcGamePaymentOffer.Enter),"V2 offer cannot take");
            p.baseOffer=PcGamePaymentOffer.Take;return Checked(p,applied);
        }
        public static PcGamePaymentRecordV2 EnterOffer(PcGamePaymentRecordV2 value,bool applied)
        {
            var p=Candidate(value,applied);Require(p.owner==PcGamePaymentOwner.Base&&!p.prepared&&p.baseOffer==PcGamePaymentOffer.Open,"V2 offer cannot enter");
            p.baseOffer=PcGamePaymentOffer.Enter;return Checked(p,applied);
        }
        public static PcGamePaymentRecordV2 TransferToDouble(PcGamePaymentRecordV2 value,bool applied,string featureId)
        {
            var p=Candidate(value,applied);Require(p.owner==PcGamePaymentOwner.Base&&!p.prepared&&p.baseOffer==PcGamePaymentOffer.Enter,"V2 payment cannot transfer to Double");
            p.owner=PcGamePaymentOwner.Double;p.baseOffer=PcGamePaymentOffer.Transferred;p.featureId=featureId;return Checked(p,applied);
        }
        public static PcGamePaymentRecordV2 Prepare(PcGamePaymentRecordV2 value,bool applied,long finalAward,PcSasObservationV1 route)
        {
            var p=Candidate(value,applied);Require(!p.prepared,"V2 route already frozen");
            var observation=PcGamePaymentRecordV2.DecodeObservation(route,SasObservationPoint.Route);
            p.prepared=true;p.finalAward=finalAward;p.routeObservation=route;
            p.route=observation.Payload.Available?PcGamePaymentRouteV2.Sas:PcGamePaymentRouteV2.Ordinary;
            return Checked(p.Clone(),applied);
        }
        public static PcGamePaymentRecordV2 PrepareQuery(PcGamePaymentRecordV2 value,bool applied)
        {
            var p=Candidate(value,applied);Require(p.route==PcGamePaymentRouteV2.Sas&&!p.queryPrepared,"V2 query cannot prepare");
            p.queryPrepared=true;p.queryBytesBase64=Convert.ToBase64String(SasPaymentPlanning.Query(p.finalAward,p.denomination));return Checked(p,applied);
        }
        public static PcGamePaymentRecordV2 SaveQueryResponse(PcGamePaymentRecordV2 value,bool applied,int response)
        {
            var p=Candidate(value,applied);Require(p.queryPrepared&&!p.queryResponseSaved,"V2 query response cannot save");
            p.queryResponseSaved=true;p.queryResponseValue=response;p.specialClassification=response>0;return Checked(p,applied);
        }
        public static PcGamePaymentRecordV2 PreparePacket(PcGamePaymentRecordV2 value,bool applied,PcSasObservationV1 observation)
        {
            var p=Candidate(value,applied);Require(p.queryResponseSaved&&!p.packetPrepared&&p.finalAward>0,"V2 packet cannot prepare");
            var packet=PcGamePaymentRecordV2.DecodeObservation(observation,SasObservationPoint.Packet);byte multiplier=packet.Payload.Multiplier;
            Require(multiplier!=0,"V2 packet multiplier is zero");long total=checked(p.finalAward*p.denomination);
            var bytes=new byte[28];SasLittleEndian.Write32(bytes,0,0x335000);SasLittleEndian.Write32(bytes,4,28);
            SasLittleEndian.Write64(bytes,8,applied?total/multiplier:total);bytes[16]=multiplier;SasLittleEndian.Write64(bytes,20,applied?total:0);
            p.packetPrepared=true;p.packetObservation=observation;p.packetBytesBase64=Convert.ToBase64String(bytes);return Checked(p.Clone(),applied);
        }
        public static PcGamePaymentRecordV2 SaveDispatchReturn(PcGamePaymentRecordV2 value,bool applied,int returned)
        {
            var p=Candidate(value,applied);Require(p.packetPrepared&&!p.dispatchReturned,"V2 dispatch result cannot save");
            p.dispatchReturned=true;p.dispatchReturnValue=returned;return Checked(p,applied);
        }
        public static PcGamePaymentRecordV2 SaveOrdinaryReply(PcGamePaymentRecordV2 value,bool applied,int reply)
        {
            var p=Candidate(value,applied);Require(p.route==PcGamePaymentRouteV2.Ordinary&&p.finalAward>0&&!p.moneyApplied&&p.replyValue==0,"V2 ordinary reply cannot save");
            p.replyValue=reply;return Checked(p,applied);
        }
        public static PcGamePaymentRecordV2 Apply(PcGamePaymentRecordV2 value,bool applied)
        {var p=Candidate(value,applied);Require(p.prepared&&!p.moneyApplied,"V2 money already applied or unprepared");
            Require(p.route!=PcGamePaymentRouteV2.Ordinary||p.finalAward==0||p.replyValue!=0,"V2 ordinary positive payout awaits accepted reply");
            p.moneyApplied=true;return Checked(p,applied);}
        public static PcGamePaymentRecordV2 NotifyCredit(PcGamePaymentRecordV2 value,bool applied)
        {var p=Candidate(value,applied);Require(!p.creditNotified,"V2 credit already notified");p.creditNotified=true;return Checked(p,applied);}
        public static PcGamePaymentRecordV2 Complete(PcGamePaymentRecordV2 value,bool applied)
        {var p=Candidate(value,applied);Require(!p.completionReceived,"V2 completion already saved");p.completionReceived=true;return Checked(p,applied);}
        public static PcGamePaymentRecordV2 ResolveZero(PcGamePaymentRecordV2 value,bool applied)
        {var p=Candidate(value,applied);Require(!p.zeroResolved,"V2 zero already resolved");p.zeroResolved=true;return Checked(p,applied);}
        public static PcGamePaymentRecordV2 Publish(PcGamePaymentRecordV2 value,bool applied)
        {var p=Candidate(value,applied);Require(!p.historyPublished,"V2 history already published");p.historyPublished=true;return Checked(p,applied);}
    }
}
