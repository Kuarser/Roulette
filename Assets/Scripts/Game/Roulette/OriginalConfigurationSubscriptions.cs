using System;
using System.Collections.Generic;
namespace RouletteDeluxe
{
    // Logical normal RegisterForChanges state. Transport, teardown and pending-change
    // queues are separate native owners. No synthetic reply transport is installed.
    public sealed class OriginalConfigurationSubscriptions
    {
        public sealed class Client
        {
            internal readonly List<OriginalSimpleConfigOption> Waiting=new List<OriginalSimpleConfigOption>();
            internal readonly List<OriginalConfigurationOptionHandle> Registered=new List<OriginalConfigurationOptionHandle>();
            private readonly OriginalConfigurationPendingChanges pending;
            // Explicit admitted state, not a reconstruction of ExtendedClientHandle's ctor.
            public Client(OriginalSimpleConfigOption[] waiting,OriginalConfigurationOptionHandle[] registered)
            {if(waiting==null||registered==null)throw new ArgumentNullException();foreach(var key in waiting){if(key==null)throw new ArgumentNullException();Waiting.Add(key.DeepCopy());}foreach(var handle in registered){if(handle==null)throw new ArgumentNullException();Registered.Add(handle);}}
            public Client(OriginalSimpleConfigOption[] waiting,OriginalConfigurationOptionHandle[] registered,OriginalConfigurationOptionHandle[] pending):this(waiting,registered)
            {this.pending=new OriginalConfigurationPendingChanges(pending);}
            private OriginalConfigurationPendingChanges Pending(){if(pending==null)throw new InvalidOperationException("Caller-admitted pending queue required");return pending;}
            public void AddChange(OriginalConfigurationOptionHandle handle){Pending().AddChange(handle);}
            public OriginalSimpleConfigOption[] GetPendingChanges(){return Pending().GetPendingChanges();}
            public void ClearPendingChanges(){Pending().ClearPendingChanges();}
            public OriginalSimpleConfigOption[] WaitingSnapshot(){var result=new OriginalSimpleConfigOption[Waiting.Count];for(int i=0;i<result.Length;++i)result[i]=Waiting[i].DeepCopy();return result;}
            public OriginalConfigurationOptionHandle[] RegisteredSnapshot(){return Registered.ToArray();}
        }
        public sealed class State
        {
            private readonly Dictionary<OriginalConfigurationOptionHandle,List<object>> subscribers=new Dictionary<OriginalConfigurationOptionHandle,List<object>>();
            internal List<object> For(OriginalConfigurationOptionHandle handle){List<object> result;if(!subscribers.TryGetValue(handle,out result)){result=new List<object>();subscribers.Add(handle,result);}return result;}
            public void AddAdmittedSubscriber(OriginalConfigurationOptionHandle handle,object client){if(handle==null||client==null)throw new ArgumentNullException();For(handle).Add(client);}
            public object[] SubscriberSnapshot(OriginalConfigurationOptionHandle handle){if(handle==null)throw new ArgumentNullException();List<object> result;return subscribers.TryGetValue(handle,out result)?result.ToArray():new object[0];}
        }
        private readonly State state;private readonly Func<OriginalSimpleConfigOption,OriginalConfigurationOptionHandle> find;
        private readonly Action<Client,byte[]> reply;
        private readonly List<object> clients=new List<object>();
        public OriginalConfigurationSubscriptions(State state,Func<OriginalSimpleConfigOption,OriginalConfigurationOptionHandle> find,Action<Client,byte[]> reply)
        {if(state==null||find==null||reply==null)throw new ArgumentException("Persistent subscription state, genuine map lookup and reply transport required");this.state=state;this.find=find;this.reply=reply;}
        public void AddAdmittedClient(object client){clients.Add(client);}
        public object[] GetClients(){return clients.ToArray();}
        public object FindWaiting(object[] snapshot,OriginalSimpleConfigOption key)
        {if(snapshot==null||key==null)throw new ArgumentNullException();foreach(var item in snapshot){var client=item as Client;if(client==null)continue;foreach(var pending in client.Waiting)if(pending.SameIdentity(key))return client;}return null;}
        private static bool ContainsReference<T>(List<T> list,T item) where T:class
        {foreach(var entry in list)if(ReferenceEquals(entry,item))return true;return false;}
        private static void Word(byte[] bytes,int offset,uint value){for(int i=0;i<4;++i)bytes[offset+i]=(byte)(value>>(8*i));}
        public uint RegisterForChanges(OriginalSimpleConfigOption[] options,Client client,bool sendReply)
        {
            if(options==null||client==null)throw new ArgumentNullException();uint count=0;
            foreach(var key in options)
            {
                if(key==null)throw new ArgumentNullException(nameof(options));var handle=find(key);
                if(handle!=null)
                {
                    var subscribers=state.For(handle);
                    if(ContainsReference(subscribers,(object)client))continue;
                    if(!ContainsReference(client.Registered,handle))client.Registered.Add(handle);
                    subscribers.Add(client);
                    int at=client.Waiting.FindIndex(item=>item.SameIdentity(key));if(at>=0)client.Waiting.RemoveAt(at);
                    count=unchecked(count+1);
                }
                else if(client.Waiting.FindIndex(item=>item.SameIdentity(key))<0)client.Waiting.Add(key.DeepCopy());
            }
            if(sendReply){var message=new byte[12];Word(message,0,0x50000);Word(message,4,12);Word(message,8,count);reply(client,message);}
            return count;
        }
        public void RegisterFromServer(OriginalSimpleConfigOption[] options,object client,bool sendReply)
        {var owner=client as Client;if(owner==null)throw new ArgumentException("CfgClientHandle owner required");RegisterForChanges(options,owner,sendReply);}
    }
}
