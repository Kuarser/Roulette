using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using RouletteDeluxe.SasDraftV1;
namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private SasConfigSnapshotFileStoreV3 sasHostConfigStore;
        private SasHostConfigCoordinatorV3 sasHostConfigCoordinator;
        private SasConfigSnapshotV2 sasHostConfigReadback;
        private int sasHostConfigServiceFrame=-1;
        private string PcSasConfigStatePath
        {
            get
            {
                string name="roulette_sas_config_v3.bin";
                if(recordedHistoryKey!=RecordedHistoryKey)
                    using(var sha=SHA256.Create())name="roulette_sas_config_test_"+
                        BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(recordedHistoryKey))).Replace("-","").ToLowerInvariant()+".bin";
                return Path.Combine(Application.persistentDataPath,name);
            }
        }
        private bool InitializePcSasHostConfiguration()
        {
            bool unisolatedSmoke=recordedHistoryKey==RecordedHistoryKey && Environment.GetCommandLineArgs().Any(a=>
                a.StartsWith("-roulette",StringComparison.Ordinal)&&a.EndsWith("Smoke",StringComparison.Ordinal));
            if(temporaryHistory || unisolatedSmoke)return true;
            try
            {
                if(sasHostConfigStore!=null || sasHostConfigCoordinator!=null)throw new InvalidOperationException("SAS configuration already initialized");
                Directory.CreateDirectory(Application.persistentDataPath);
                var store=new SasConfigSnapshotFileStoreV3(PcSasConfigStatePath,recordedHistoryKey);
                var loaded=store.Load();bool restored=loaded!=null;
                if(loaded==null)
                {
                    // Materialize native zero NV independently of Idle. This
                    // carries no invented completed Idle or host event receipt.
                    var initial=new SasConfigEnvelopeV1(recordedHistoryKey,0,SasPayload.Disabled(),
                        "pc-bootstrap","initial-zero",0,SasConfigSnapshotCodecV2.Digest(new byte[0]));
                    loaded=store.Save(-1,new SasConfigSnapshotV2(initial,new SasEventReceiptV1[0]));
                }
                var coordinator=new SasHostConfigCoordinatorV3(recordedHistoryKey,loaded,SavePcSasHostConfiguration);
                sasHostConfigStore=store;sasHostConfigReadback=loaded;sasHostConfigCoordinator=coordinator;
                Debug.Log("ROULETTE_SAS_CONFIG_OPEN: restored="+restored+" generation="+loaded.Config.Generation+
                    " pendingEnable="+coordinator.Payload.PendingEnable+" pendingDisable="+coordinator.PendingDisable);
                return true;
            }
            catch(Exception error){FailPcRoundLifecycle("sas-config-open",error);return false;}
        }
        private SasConfigSnapshotV2 SavePcSasHostConfiguration(long expected,SasConfigSnapshotV2 candidate)
        {
            if(temporaryHistory || sasHostConfigStore==null || sasHostConfigReadback==null || pcRoundLifecycleStorageFaulted ||
                candidate==null || candidate.Config.Scope!=recordedHistoryKey || sasHostConfigReadback.Config.Scope!=recordedHistoryKey ||
                sasHostConfigReadback.Config.Generation!=expected)throw new PcRoundLifecycleDataException("SAS configuration save has no live owner");
            RequireSasIdleConfigOwner();
            var saved=sasHostConfigStore.Save(expected,candidate);
            sasHostConfigReadback=saved;
            if(sasHostConfigSavedForTest!=null)sasHostConfigSavedForTest(saved.Config.Generation);
            RequireSasIdleConfigOwner();return saved;
        }
        internal void EnqueuePcSasHostEventV1(SasQueuedHostEventV1 item)
        {
            // FIFO alone is transport-thread safe. The main-thread service
            // below performs validation, durable deduplication and publication.
            var coordinator=sasHostConfigCoordinator;
            if(coordinator==null)throw new InvalidOperationException("SAS host configuration is not attached");
            coordinator.Enqueue(item);
        }
        private bool ServicePcSasHostMessagesV1()
        {
            if(pcRoundLifecycleStorageFaulted)return false;
            if(temporaryHistory)return true;
            if(sasHostConfigCoordinator==null)return true;
            if(sasHostConfigServiceFrame==Time.frameCount)return true;
            sasHostConfigServiceFrame=Time.frameCount;
            try
            {
                sasHostConfigCoordinator.ServiceMessages((int)sourceGameplayState,16);
                return true;
            }
            catch(Exception error){FailPcRoundLifecycle("sas-config-service",error);return false;}
        }
        private SasConfigSnapshotV2 RequirePcSasHostConfigurationReadback()
        {
            if(temporaryHistory || sasHostConfigCoordinator==null || sasHostConfigStore==null || pcRoundLifecycleStorageFaulted)
                throw new PcRoundLifecycleDataException("SAS observation has no live configuration owner");
            var stored=sasHostConfigStore.Load();
            if(stored==null || !SasConfigSnapshotCodecV2.Encode(stored).SequenceEqual(SasConfigSnapshotCodecV2.Encode(sasHostConfigReadback)))
                throw new PcRoundLifecycleDataException("SAS durable configuration changed before observation");
            return stored;
        }
        private PcSasObservationV1 CapturePcSasLiveObservationV1(SasObservationPoint point)
        {
            try
            {
                RequirePcSasHostConfigurationReadback();
                return sasHostConfigCoordinator.CaptureObservation(point).ToPayload();
            }
            catch(Exception error){FailPcRoundLifecycle("sas-config-observation",error);throw;}
        }
        private void BindSasStartupPaymentTransportV1(IPcSasPaymentAdapterV1 transport)
        {
            if(transport==null || sasHostConfigCoordinator==null)throw new PcRoundLifecycleDataException("SAS transport has no normal-startup configuration");
            BindSasStartupPaymentAdapterV1(new CoordinatedSasPaymentAdapterV1(this,transport));
        }
        private sealed class CoordinatedSasPaymentAdapterV1 : IPcSasPaymentAdapterV1
        {
            private readonly RouletteGame owner;private readonly IPcSasPaymentAdapterV1 transport;
            internal CoordinatedSasPaymentAdapterV1(RouletteGame game,IPcSasPaymentAdapterV1 value){owner=game;transport=value;}
            public PcSasObservationV1 Observe(PcGamePaymentRecordV2 saved,SasObservationPoint point){return owner.CapturePcSasLiveObservationV1(point);}
            public bool TryQuery(PcGamePaymentRecordV2 saved,out int response){return transport.TryQuery(saved,out response);}
            public bool TryDispatch(PcGamePaymentRecordV2 saved,out int returned){return transport.TryDispatch(saved,out returned);}
            public bool TryOrdinaryReply(PcGamePaymentRecordV2 saved,out int reply){return transport.TryOrdinaryReply(saved,out reply);}
            public bool TryNotifyCredit(PcGamePaymentRecordV2 saved){return transport.TryNotifyCredit(saved);}
            public bool TryReceiveCompletion(PcGamePaymentRecordV2 saved){return transport.TryReceiveCompletion(saved);}
        }
    }
}
