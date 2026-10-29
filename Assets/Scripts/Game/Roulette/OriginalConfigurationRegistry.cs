using System;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
namespace RouletteDeluxe
{
    // ConfigServer.RegisterConfigurationOptions normal orchestration. Transport and
    // server virtual RegisterForChanges remain mandatory genuine caller bindings.
    public sealed class OriginalConfigurationRegistry
    {
        public sealed class Host
        {
            public Func<object[]> GetClients;
            public Func<object[],OriginalSimpleConfigOption,object> FindWaiting;
            public Action<OriginalSimpleConfigOption[],object,bool> RegisterForChanges;
            public Action<OriginalConfigurationOption[]> SendOptionUpdateToHost;
            public Action<byte[]> Warning;
        }
        public sealed class Frame{public int? Maximum,Minimum;}
        public sealed class MapEntry{public OriginalSimpleConfigOption Key;public OriginalConfigurationOptionHandle Handle;}
        private readonly Host host;private readonly OriginalMachineDataNumeric numeric;
        private readonly OriginalConfigurationHandleVector handles;
        private readonly List<OriginalSimpleConfigOption> unclaimed=new List<OriginalSimpleConfigOption>();
        private readonly SortedDictionary<OriginalSimpleConfigOption,OriginalConfigurationOptionHandle> map=new SortedDictionary<OriginalSimpleConfigOption,OriginalConfigurationOptionHandle>(new OriginalSimpleConfigOption.NativeComparer());
        public bool NeedToSave{get;private set;}
        public OriginalConfigurationRegistry(Host host,OriginalCfgIntegerConverter.State errno,OriginalConfigurationHandleVector.State cache)
        {
            if(host==null||host.GetClients==null||host.FindWaiting==null||host.RegisterForChanges==null||host.SendOptionUpdateToHost==null||host.Warning==null)throw new ArgumentException("Genuine configuration server, client, host and warning bindings required");
            this.host=new Host{GetClients=host.GetClients,FindWaiting=host.FindWaiting,RegisterForChanges=host.RegisterForChanges,SendOptionUpdateToHost=host.SendOptionUpdateToHost,Warning=host.Warning};
            numeric=new OriginalMachineDataNumeric(errno);handles=new OriginalConfigurationHandleVector(cache);
        }
        public void AddAdmittedUnclaimed(OriginalSimpleConfigOption option){if(option==null)throw new ArgumentNullException(nameof(option));unclaimed.Add(option.DeepCopy());}
        public OriginalSimpleConfigOption[] UnclaimedSnapshot(){var result=new OriginalSimpleConfigOption[unclaimed.Count];for(int i=0;i<result.Length;++i)result[i]=unclaimed[i].DeepCopy();return result;}
        public OriginalConfigurationOptionHandle[] HandleSnapshot(){return handles.Snapshot();}
        public OriginalConfigurationOptionHandle FindOption(OriginalSimpleConfigOption key)
        {if(key==null)throw new ArgumentNullException(nameof(key));OriginalConfigurationOptionHandle result;return map.TryGetValue(key,out result)?result:null;}
        public MapEntry[] MapSnapshot(){var result=new MapEntry[map.Count];int i=0;foreach(var pair in map)result[i++]=new MapEntry{Key=pair.Key.DeepCopy(),Handle=pair.Value};return result;}
        private static bool Eq(byte[] a,byte[] b){if(a.Length!=b.Length)return false;for(int i=0;i<a.Length;++i)if(a[i]!=b[i])return false;return true;}
        private static byte[] Join(byte[] a,byte[] b){var result=new byte[checked(a.Length+b.Length)];Array.Copy(a,result,a.Length);Array.Copy(b,0,result,a.Length,b.Length);return result;}
        private static byte[] A(string text){return Encoding.ASCII.GetBytes(text);}
        private static int ReadBound(int? value){if(!value.HasValue)throw new InvalidOperationException("Native bound read requires caller-known stack storage");return value.Value;}
        private static bool Duplicate(OriginalConfigurationOption a,OriginalConfigurationOption b)
        {bool compatible=a.Control==b.Control||(a.Control==0&&b.Control!=10)||(a.Control!=10&&b.Control==0);return compatible&&a.Hash==b.Hash&&Eq(a.Name,b.Name)&&Eq(a.Category,b.Category);}
        public uint Register(OriginalConfigurationOption[] input,byte[] categoryPrefix,Frame frame)
        {
            if(input==null||categoryPrefix==null||frame==null)throw new ArgumentNullException();
            var copies=new List<OriginalConfigurationOption>();var updates=new List<OriginalConfigurationOption>();
            try
            {
                foreach(var option in input){if(option==null)throw new ArgumentNullException(nameof(input));copies.Add(option.DeepCopy());}
                int needed=checked(handles.Count+input.Length);if(handles.Capacity<needed)handles.Reserve(checked(needed+25));
                foreach(var option in copies)
                {
                    if(categoryPrefix.Length!=0)option.SetCategory(Join(categoryPrefix,option.Category));
                    byte[] name=option.Name;var shortened=new byte[Math.Min(name.Length,61)];Array.Copy(name,shortened,shortened.Length);
                    var key=new OriginalSimpleConfigOption(shortened,option.Category,new byte[0]);int found=-1;
                    for(int i=0;i<unclaimed.Count;++i)if(unclaimed[i].SameIdentity(key)){found=i;break;}
                    if(found>=0)
                    {byte[] value=unclaimed[found].Value;if(!Eq(value,option.Value)&&option.Flags[1]!=0)option.SetIsSet(true);option.SetValue(value);unclaimed.RemoveAt(found);}
                    bool duplicate=false;foreach(var owner in handles.Snapshot())if(Duplicate(owner.Primary,option)){duplicate=true;break;}if(duplicate)continue;
                    if(option.HasMaximum||option.HasMinimum)
                    {
                        int? maximum=OriginalConfigurationInt32Stream.ExtractTarget(option.Maximum,frame.Maximum);frame.Maximum=maximum;
                        int? minimum=OriginalConfigurationInt32Stream.ExtractTarget(option.Minimum,frame.Minimum);frame.Minimum=minimum;
                        foreach(var value in option.GetAllowedValues())
                        {
                            int parsed=numeric.ScanInt32(value,0); // Original strtol base10 scalar/errno semantics.
                            // Both native tests use MaximumEnabled, including the lower-bound test.
                            if(option.HasMaximum&&(parsed>ReadBound(maximum)||parsed<ReadBound(minimum)))
                            {
                                option.RemoveAllowedValue(value);
                                byte[] message=Join(Join(A("Warning: Allowed value "),value),A(" not within Min Max range of "+ReadBound(minimum).ToString(CultureInfo.InvariantCulture)+" - "+ReadBound(maximum).ToString(CultureInfo.InvariantCulture)+"\n"));host.Warning(message);
                            }
                        }
                    }
                    var handle=new OriginalConfigurationOptionHandle(option);
                    if(option.Control!=10){var mapped=OriginalSimpleConfigOption.FromConfiguration(option);if(!map.ContainsKey(mapped))map.Add(mapped,handle);}
                    handles.Insert(handle);updates.Add(option.DeepCopy());
                    if(option.Control!=10)
                    {
                        while(true)
                        {
                            object[] clients=host.GetClients();if(clients==null)throw new InvalidOperationException("Genuine client vector required");
                            object client=host.FindWaiting((object[])clients.Clone(),OriginalSimpleConfigOption.FromConfiguration(option));if(client==null)break;
                            host.RegisterForChanges(new[]{OriginalSimpleConfigOption.FromConfiguration(option)},client,false);
                        }
                    }
                }
                if(updates.Count!=0){host.SendOptionUpdateToHost(updates.ToArray());NeedToSave=true;}
                return (uint)copies.Count;
            }
            finally{foreach(var option in updates)option.Dispose();foreach(var option in copies)option.Dispose();}
        }
    }
}
