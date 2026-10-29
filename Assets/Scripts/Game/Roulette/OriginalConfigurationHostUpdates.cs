using System;
using System.Collections.Generic;
namespace RouletteDeluxe
{
    // Original host-update senders. Transport and live owner collections must
    // be bound by the caller; no host metadata or transaction state is invented.
    public sealed class OriginalConfigurationHostUpdates
    {
        public sealed class Host
        {
            public Func<object[]> GetHosts;
            public Func<OriginalConfigurationRegistry.MapEntry[]> GetOptionMap;
            public Func<OriginalSimpleConfigOption[]> GetCurrentOptions;
            public Action<object,byte[],bool> SendMessage;
        }
        private readonly Host host;
        public OriginalConfigurationHostUpdates(Host host)
        {
            if(host==null||host.GetHosts==null||host.GetOptionMap==null||host.GetCurrentOptions==null||host.SendMessage==null)throw new ArgumentException("Genuine host vector, unique option map, current transaction options and transport bindings required");
            this.host=new Host{GetHosts=host.GetHosts,GetOptionMap=host.GetOptionMap,GetCurrentOptions=host.GetCurrentOptions,SendMessage=host.SendMessage};
        }
        public static OriginalSimpleConfigOption[] CurrentTransactionOptions(OriginalConfigurationTransactionManager.State state)
        {
            if(state==null)throw new ArgumentNullException(nameof(state));var queue=state.QueueSnapshot();
            if(queue.Length==0)throw new InvalidOperationException("Native current transaction read requires a live queue front");return queue[0].OptionsA;
        }
        private static void Word(byte[] bytes,int at,uint value){for(int i=0;i<4;++i)bytes[at+i]=(byte)(value>>(i*8));}
        private void Send(object endpoint,OriginalConfigurationOption[] first,OriginalConfigurationOption[] second)
        {
            using(var message=new OriginalConfigurationOptionMessage.Dual(first,second))
            {var xml=message.Build();var bytes=new byte[checked(xml.Length+9)];Word(bytes,0,0x170000);Word(bytes,4,(uint)bytes.Length);Array.Copy(xml,0,bytes,8,xml.Length);host.SendMessage(endpoint,bytes,false);}
        }
        public void SendOptionUpdateToHost(OriginalConfigurationOption[] options){SendOptionUpdateToHost(options,new OriginalConfigurationOption[0]);}
        public void SendOptionUpdateToHost(OriginalConfigurationOption[] first,OriginalConfigurationOption[] second)
        {
            if(first==null||second==null)throw new ArgumentNullException();if(first.Length==0&&second.Length==0)return;
            var hosts=host.GetHosts();if(hosts==null)throw new InvalidOperationException("Actual host vector required");
            foreach(var entry in hosts)
            {
                var endpoint=entry as OriginalConfigurationTransactionManager.HostClient;if(endpoint==null)continue;
                if(!endpoint.Ready.HasValue)throw new InvalidOperationException("Native Ready read requires caller-known storage");
                if(endpoint.Ready.Value!=0)Send(endpoint,first,second);
            }
        }
        public void SendAllChangesToHostClient(OriginalConfigurationTransactionManager.HostClient endpoint)
        {
            var options=host.GetCurrentOptions();if(options==null)throw new InvalidOperationException("Actual current transaction first option vector required");if(options.Length==0)return;
            var map=host.GetOptionMap();if(map==null)throw new InvalidOperationException("Actual unique OptionHandles map required");
            var found=new List<OriginalConfigurationOption>();var compare=new OriginalSimpleConfigOption.NativeComparer();
            try
            {
                foreach(var key in options)foreach(var entry in map)if(compare.Compare(key,entry.Key)==0){found.Add(entry.Handle.Primary.DeepCopy());break;}
                if(found.Count!=0)Send(endpoint,found.ToArray(),new OriginalConfigurationOption[0]);
            }
            finally{foreach(var option in found)option.Dispose();}
        }
    }
}
