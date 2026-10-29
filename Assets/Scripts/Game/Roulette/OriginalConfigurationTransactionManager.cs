using System;
using System.Collections.Generic;
namespace RouletteDeluxe
{
    // ConfigTransMgr.FinishTransaction normal orchestration. XML, persistence,
    // game execution, timer and StartTransaction are required genuine owners.
    public sealed class OriginalConfigurationTransactionManager
    {
        public sealed class ClientMetadata{public byte? SummaryEnabled,ChangeEnabled,AnyGame;public byte[] GameName;}
        public sealed class HostClient{public byte? Ready,SummaryEnabled;}
        public sealed class Host
        {
            public Func<OriginalConfigurationRegistry.MapEntry[]> GetOptionMap;
            public Func<object[]> GetClients,GetHosts;
            public Func<OriginalConfigurationSubscriptions.Client,ClientMetadata> ReadClient;
            public Func<byte[]> CurrentGameName;
            public Func<OriginalSimpleConfigOption[],byte[]> BuildChangesXml;
            public Func<bool,OriginalConfigurationTransactionRecord.ErrorEntry[],byte[]> BuildSummaryXml;
            public Action<object,byte[],bool> SendMessage;
            public Action<bool> SaveConfigOptions,SetGameRestart;
            public Action<HostClient> SendAllChangesToHostClient;
            public Func<byte[],byte[],uint[],bool> StartGame;
            public Action EndTransactionTimer,StartTransaction;
            public Action<object> ClearQueuedTimerEvents,DeleteTimer;
            public Action<object,uint,uint> SetTimerPeriod;
        }
        public sealed class State
        {
            internal readonly List<OriginalConfigurationTransactionRecord> queue=new List<OriginalConfigurationTransactionRecord>();
            internal readonly List<OriginalSimpleConfigOption> pending=new List<OriginalSimpleConfigOption>();
            internal OriginalConfigurationTransactionRecord staged;
            public byte DeferRequested,DeferredObserved;public object Timer;
            public State(OriginalConfigurationTransactionRecord[] queue,OriginalConfigurationTransactionRecord staged,OriginalSimpleConfigOption[] pending,byte deferRequested,byte deferredObserved,object timer)
            {if(queue==null||staged==null||pending==null)throw new ArgumentNullException();foreach(var record in queue){if(record==null)throw new ArgumentNullException();this.queue.Add(record.DeepCopy());}this.staged=staged.DeepCopy();foreach(var key in pending){if(key==null)throw new ArgumentNullException();this.pending.Add(key.DeepCopy());}DeferRequested=deferRequested;DeferredObserved=deferredObserved;Timer=timer;}
            public OriginalConfigurationTransactionRecord[] QueueSnapshot(){var result=new OriginalConfigurationTransactionRecord[queue.Count];for(int i=0;i<result.Length;++i)result[i]=queue[i].DeepCopy();return result;}
            public OriginalConfigurationTransactionRecord StagedSnapshot(){return staged.DeepCopy();}
            public OriginalSimpleConfigOption[] PendingSnapshot(){var result=new OriginalSimpleConfigOption[pending.Count];for(int i=0;i<result.Length;++i)result[i]=pending[i].DeepCopy();return result;}
        }
        private readonly State state;private readonly Host host;private readonly OriginalConfigurationSubscriptions.State subscriptions;
        public OriginalConfigurationTransactionManager(State state,Host host,OriginalConfigurationSubscriptions.State subscriptions)
        {
            if(state==null||subscriptions==null||host==null||host.GetOptionMap==null||host.GetClients==null||host.GetHosts==null||host.ReadClient==null||host.CurrentGameName==null||host.BuildChangesXml==null||host.BuildSummaryXml==null||host.SendMessage==null||host.SaveConfigOptions==null||host.SetGameRestart==null||host.SendAllChangesToHostClient==null||host.StartGame==null||host.EndTransactionTimer==null||host.StartTransaction==null||host.ClearQueuedTimerEvents==null||host.DeleteTimer==null||host.SetTimerPeriod==null)throw new ArgumentException("Persistent state and genuine transaction dependencies required");
            this.state=state;this.subscriptions=subscriptions;this.host=new Host{GetOptionMap=host.GetOptionMap,GetClients=host.GetClients,GetHosts=host.GetHosts,ReadClient=host.ReadClient,CurrentGameName=host.CurrentGameName,BuildChangesXml=host.BuildChangesXml,BuildSummaryXml=host.BuildSummaryXml,SendMessage=host.SendMessage,SaveConfigOptions=host.SaveConfigOptions,SetGameRestart=host.SetGameRestart,SendAllChangesToHostClient=host.SendAllChangesToHostClient,StartGame=host.StartGame,EndTransactionTimer=host.EndTransactionTimer,StartTransaction=host.StartTransaction,ClearQueuedTimerEvents=host.ClearQueuedTimerEvents,DeleteTimer=host.DeleteTimer,SetTimerPeriod=host.SetTimerPeriod};
        }
        private static OriginalConfigurationTransactionRecord ResetRecord()
        {return new OriginalConfigurationTransactionRecord(new byte[6],new OriginalConfigurationTransactionRecord.Request[0],new OriginalConfigurationTransactionRecord.Request[0],new OriginalSimpleConfigOption[0],new OriginalSimpleConfigOption[0],null,false);}
        private static bool Flag(byte? value){if(!value.HasValue)throw new InvalidOperationException("Native flag read requires caller-known storage");return value.Value!=0;}
        private static bool Eq(byte[] a,byte[] b){if(a==null||b==null)throw new InvalidOperationException("Genuine raw game name required");if(a.Length!=b.Length)return false;for(int i=0;i<a.Length;++i)if(a[i]!=b[i])return false;return true;}
        private ClientMetadata Metadata(OriginalConfigurationSubscriptions.Client client){var result=host.ReadClient(client);if(result==null)throw new InvalidOperationException("Genuine client metadata required");return result;}
        private static void Word(byte[] bytes,int at,uint value){for(int i=0;i<4;++i)bytes[at+i]=(byte)(value>>(8*i));}
        private void Send(object client,uint kind,byte[] xml)
        {if(xml==null)throw new InvalidOperationException("Genuine XML builder output required");var message=new byte[checked(xml.Length+9)];Word(message,0,kind);Word(message,4,(uint)message.Length);Array.Copy(xml,0,message,8,xml.Length);host.SendMessage(client,message,false);}
        private OriginalConfigurationRegistry.MapEntry[] Map(){var result=host.GetOptionMap();if(result==null)throw new InvalidOperationException("Genuine option map required");return result;}
        public uint FinishTransaction()
        {
            if(state.queue.Count==0)return 0;
            var first=state.queue[0];if(!first.HasKnownSender)throw new InvalidOperationException("Native sender read requires caller-known storage");object sender=first.Sender;
            var combined=new OriginalSimpleConfigOption[first.optionsA.Length+first.optionsB.Length];Array.Copy(first.optionsA,combined,first.optionsA.Length);Array.Copy(first.optionsB,0,combined,first.optionsA.Length,first.optionsB.Length);
            var summary=OriginalConfigurationTransactionRecord.Request.FromOptions(combined,0,sender,new byte?[]{0,0,null});bool success=first.GetErrorCount()==0,deferred=false;
            if(success&&state.DeferRequested!=0)
            {state.DeferredObserved=1;var copy=first.DeepCopy();copy.SetFlag(0,0);state.queue.RemoveAt(0);state.queue.Insert(0,state.staged.DeepCopy());state.queue.Insert(0,copy);state.staged=ResetRecord();deferred=true;}
            state.DeferRequested=0;state.staged=ResetRecord();first=state.queue[0];
            if(success)
            {
                if(first.Flags[0]!=0||deferred)
                {
                    if(first.Flags[0]!=0&&!deferred)
                    {
                        bool changed=false;foreach(var entry in Map())
                        {var handle=entry.Handle;if(Eq(handle.Primary.Value,handle.Secondary.Value))continue;handle.Primary.SetValue(handle.Secondary.Value);handle.Primary.SetIsSet(true);foreach(var item in subscriptions.SubscriberSnapshot(handle)){var client=item as OriginalConfigurationSubscriptions.Client;if(client!=null)client.AddChange(handle);}changed=true;}
                        if(changed)host.SaveConfigOptions(false);
                    }
                    var clients=host.GetClients();if(clients==null)throw new InvalidOperationException("Genuine clients required");foreach(var item in clients)
                    {
                        var client=item as OriginalConfigurationSubscriptions.Client;if(client==null)continue;var changes=client.GetPendingChanges();if(changes.Length!=0)
                        {var metadata=Metadata(client);if(Flag(metadata.ChangeEnabled)&&(Flag(metadata.AnyGame)||Eq(metadata.GameName,host.CurrentGameName())))Send(client,0x170000,host.BuildChangesXml(changes));}
                        client.ClearPendingChanges();
                    }
                    var hosts=host.GetHosts();if(hosts==null)throw new InvalidOperationException("Genuine hosts required");foreach(var item in hosts){var owner=item as HostClient;if(owner!=null&&Flag(owner.Ready))host.SendAllChangesToHostClient(owner);}
                }
            }
            else
            {
                foreach(var request in first.requestsB)foreach(var source in request.errors)foreach(var target in summary.errors)
                {if(!source.Key.SameIdentity(target.Key))continue;while(source.Value.Count!=0){int last=source.Value.Count-1;target.Value.Add((byte[])source.Value[last].Clone());source.Value.RemoveAt(last);}}
                foreach(var entry in Map())entry.Handle.Secondary.SetValue(entry.Handle.Primary.Value);
            }
            var senderClient=sender as OriginalConfigurationSubscriptions.Client;var senderHost=sender as HostClient;ClientMetadata senderMetadata=null;
            if(senderClient!=null)
            {
                senderMetadata=Metadata(senderClient);if(!Flag(senderMetadata.AnyGame)&&!Eq(senderMetadata.GameName,host.CurrentGameName())&&senderMetadata.GameName.Length!=0)
                {host.SetGameRestart(true);if(host.StartGame((byte[])senderMetadata.GameName.Clone(),new byte[0],new uint[6]))host.EndTransactionTimer();return 0;}
            }
            if((senderHost!=null&&Flag(senderHost.SummaryEnabled))||(senderClient!=null&&Flag(senderMetadata.SummaryEnabled)))Send(sender,0x200000,host.BuildSummaryXml(success,summary.ErrorSnapshot()));
            state.pending.Clear();state.queue.RemoveAt(0);
            if(state.Timer!=null){host.ClearQueuedTimerEvents(state.Timer);host.SetTimerPeriod(state.Timer,0,0);if(state.Timer!=null)host.DeleteTimer(state.Timer);state.Timer=null;}
            if(state.queue.Count!=0)host.StartTransaction();return 1;
        }
    }
}
