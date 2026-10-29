using System;
using System.Linq;
using RouletteDeluxe.SasDraftV1;

namespace RouletteDeluxe
{
    public enum PcGamePaymentRouteV2 { Unselected = 0, Ordinary = 1, Sas = 2 }

    // Independent V4 receipt. Validation needs the applied decision from its
    // paired SAS page; it never derives wallet effects from packet lineWins.
    [Serializable]
    public sealed class PcGamePaymentRecordV2
    {
        public int version = 2;
        public string roundArchiveId, spinJournalId, profile, operationId;
        public int priorHistoryCount;
        public long creditBefore, denomination, remainderCents, baseAward;
        public PcGamePaymentOwner owner = PcGamePaymentOwner.Base;
        public PcGamePaymentOffer baseOffer;
        public string featureId;
        public bool prepared;
        public long finalAward;
        public int replyValue;
        public bool moneyApplied, creditNotified, completionReceived, zeroResolved, historyPublished;
        public string sasRoundDigest;
        public PcGamePaymentRouteV2 route;
        public PcSasObservationV1 routeObservation;
        public bool queryPrepared;
        public string queryBytesBase64;
        public bool queryResponseSaved;
        public int queryResponseValue;
        public bool specialClassification, packetPrepared;
        public PcSasObservationV1 packetObservation;
        public string packetBytesBase64;
        public bool dispatchReturned;
        public int dispatchReturnValue;

        public bool IsResolved { get { return prepared && moneyApplied && (finalAward == 0 ? zeroResolved :
            creditNotified && completionReceived && (route == PcGamePaymentRouteV2.Sas ? dispatchReturned : replyValue != 0)); } }

        public long WalletAfter { get { return checked(creditBefore + (moneyApplied ? finalAward : 0)); } }

        private static PcSasObservationV1 CopyObservation(PcSasObservationV1 v)
        {
            return v == null ? null : new PcSasObservationV1 { point=v.point,configGeneration=v.configGeneration,
                configRaw32Base64=v.configRaw32Base64,configRaw32Sha256=v.configRaw32Sha256,eventSource=v.eventSource,
                eventId=v.eventId,eventSequence=v.eventSequence,runtimePendingDisableObserved=v.runtimePendingDisableObserved };
        }
        public PcGamePaymentRecordV2 Clone()
        {
            var copy=(PcGamePaymentRecordV2)MemberwiseClone();copy.routeObservation=CopyObservation(routeObservation);
            copy.packetObservation=CopyObservation(packetObservation);return copy;
        }
        private static void Require(bool condition,string message)
        { if(!condition)throw new PcRoundLifecycleDataException(message); }

        internal static SasObservation DecodeObservation(PcSasObservationV1 value,SasObservationPoint point)
        {
            Require(value!=null && value.point==point.ToString() && value.configGeneration>=0 && value.eventSequence>=0 &&
                !string.IsNullOrWhiteSpace(value.eventSource) && !string.IsNullOrWhiteSpace(value.eventId),"Payment observation identity invalid");
            try
            {
                var bytes=Convert.FromBase64String(value.configRaw32Base64);
                Require(Convert.ToBase64String(bytes)==value.configRaw32Base64 && SasConfigSnapshotCodecV2.Digest(bytes)==value.configRaw32Sha256,"Payment observation digest invalid");
                return new SasObservation(point,value.configGeneration,new SasPayload(bytes),value.runtimePendingDisableObserved);
            }
            catch(Exception e)
            { if(!(e is FormatException || e is ArgumentException))throw;throw new PcRoundLifecycleDataException("Payment observation bytes invalid",e); }
        }
        private static void RequireBytes(string encoded,byte[] expected)
        { Require(encoded==Convert.ToBase64String(expected),"Payment packet bytes differ from frozen fields"); }

        public void Validate(bool sasApplied)
        {
            Require(version==2 && SasConfigEnvelopeCodecV3.IsDigest(sasRoundDigest),"V2 payment version or SAS digest invalid");
            // Reuse the frozen ordinary invariant definitions without changing
            // their public shape. SAS completion uses dispatch, not ordinary reply.
            var basic=new PcGamePaymentRecordV1 {roundArchiveId=roundArchiveId,spinJournalId=spinJournalId,profile=profile,
                operationId=operationId,priorHistoryCount=priorHistoryCount,creditBefore=creditBefore,denomination=denomination,
                remainderCents=remainderCents,baseAward=baseAward,owner=owner,baseOffer=baseOffer,featureId=featureId,
                prepared=prepared,finalAward=finalAward,replyValue=route==PcGamePaymentRouteV2.Sas && dispatchReturned ? 1 : replyValue,
                moneyApplied=moneyApplied,creditNotified=creditNotified,completionReceived=completionReceived,
                zeroResolved=zeroResolved,historyPublished=historyPublished};basic.Validate();
            Require(route>=PcGamePaymentRouteV2.Unselected && route<=PcGamePaymentRouteV2.Sas &&
                prepared==(route!=PcGamePaymentRouteV2.Unselected),"Payment route and prepare disagree");
            if(!prepared)Require(routeObservation==null,"Unprepared payment has route observation");
            else
            {
                var observation=DecodeObservation(routeObservation,SasObservationPoint.Route);
                Require((route==PcGamePaymentRouteV2.Sas)==observation.Payload.Available,"Frozen payment route differs from observation");
            }
            if(route!=PcGamePaymentRouteV2.Sas)
            {
                Require(!queryPrepared && string.IsNullOrEmpty(queryBytesBase64) && !queryResponseSaved && queryResponseValue==0 &&
                    !specialClassification && !packetPrepared && packetObservation==null && string.IsNullOrEmpty(packetBytesBase64) &&
                    !dispatchReturned && dispatchReturnValue==0,"Ordinary payment has SAS effects");return;
            }
            Require(replyValue==0,"SAS payment contains an ordinary reply");
            if(queryPrepared)RequireBytes(queryBytesBase64,SasPaymentPlanning.Query(finalAward,denomination));
            else Require(string.IsNullOrEmpty(queryBytesBase64),"Unprepared query contains bytes");
            Require(!queryResponseSaved || queryPrepared,"Query response precedes query prepare");
            Require(queryResponseSaved ? specialClassification==(queryResponseValue>0) : queryResponseValue==0 && !specialClassification,
                "Query classification or absent response invalid");
            if(packetPrepared)
            {
                Require(queryResponseSaved && finalAward>0,"SAS packet precedes query or has zero amount");
                var packet=DecodeObservation(packetObservation,SasObservationPoint.Packet);byte multiplier=packet.Payload.Multiplier;
                Require(multiplier!=0,"SAS packet multiplier is zero");long total=checked(finalAward*denomination);
                var bytes=new byte[28];SasLittleEndian.Write32(bytes,0,0x335000);SasLittleEndian.Write32(bytes,4,28);
                SasLittleEndian.Write64(bytes,8,sasApplied?total/multiplier:total);bytes[16]=multiplier;
                SasLittleEndian.Write64(bytes,20,sasApplied?total:0);RequireBytes(packetBytesBase64,bytes);
            }
            else Require(packetObservation==null && string.IsNullOrEmpty(packetBytesBase64),"Unprepared SAS packet contains evidence");
            Require(!dispatchReturned || packetPrepared,"SAS dispatch precedes packet prepare");
            Require(dispatchReturned || dispatchReturnValue==0,"Absent dispatch contains return value");
            Require(!moneyApplied || (finalAward==0 ? queryResponseSaved : dispatchReturned),"SAS wallet effect precedes query/dispatch");
            Require(finalAward!=0 || (!packetPrepared && !dispatchReturned),"Zero SAS payment contains packet or dispatch");
        }
    }
}
