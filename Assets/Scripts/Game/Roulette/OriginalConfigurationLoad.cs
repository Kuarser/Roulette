using System;
using System.Text;
namespace RouletteDeluxe
{
    public sealed class OriginalConfigurationLoad
    {
        public sealed class Host
        {
            public Func<byte[],OriginalConfigurationLoadFile.Temporary> ConstructPath;
            public Action<OriginalConfigurationNvData,byte[]> Open;
            public Action<OriginalConfigurationLoadFile.Temporary> DestroyPath;
            public Func<Exception,bool> IsNativeUnwind;
            public Action<OriginalConfigurationNvData,bool> LoadFile;
            public Func<byte[],byte[]> StreamValue;
            public Action<uint,byte[]> Fault;
        }
        private readonly OriginalConfigurationPersistence.State state;private readonly OriginalConfigurationRegistry registry;
        private readonly OriginalConfigurationNvData os,bc;private readonly Host host;
        public OriginalConfigurationLoad(OriginalConfigurationPersistence.State state,OriginalConfigurationRegistry registry,OriginalConfigurationNvData os,OriginalConfigurationNvData bc,Host h)
        {
            if(state==null||registry==null||os==null||bc==null||ReferenceEquals(os,bc)||!ReferenceEquals(state.OsBuffer,os.Buffer)||!ReferenceEquals(state.BcBuffer,bc.Buffer)||h==null||h.ConstructPath==null||h.Open==null||h.DestroyPath==null||h.IsNativeUnwind==null||h.LoadFile==null||h.StreamValue==null||h.Fault==null)throw new ArgumentException("Actual shared persistence, registry, distinct NV owners and load/path/stream/fault bindings required");
            this.state=state;this.registry=registry;this.os=os;this.bc=bc;host=new Host{ConstructPath=h.ConstructPath,Open=h.Open,DestroyPath=h.DestroyPath,IsNativeUnwind=h.IsNativeUnwind,LoadFile=h.LoadFile,StreamValue=h.StreamValue,Fault=h.Fault};
        }
        private void Open(OriginalConfigurationNvData owner,byte flag,string path)
        {
            owner.Flag=flag;var temporary=host.ConstructPath(Encoding.ASCII.GetBytes(path));if(temporary==null)throw new InvalidOperationException("Actual path temporary admission required");
            try{host.Open(owner,temporary.Text);}catch(Exception ex){if(host.IsNativeUnwind(ex))host.DestroyPath(temporary);throw;}
            host.DestroyPath(temporary);
        }
        private static bool Empty(OriginalConfigurationPersistence.Tables tables)
        {return tables.Groups.Length==0&&tables.Names.Length==0&&tables.Values.Length==0;}
        private void Merge()
        {
            state.global.Clear();foreach(var entry in state.os.Groups)state.global.Group(entry.Key,entry.Index);foreach(var entry in state.os.Names)state.global.Name(entry.Key,entry.Index);foreach(var entry in state.os.Values)state.global.Value(entry.Key,entry.Value);
            foreach(var entry in state.bc.Groups)state.global.Group(entry.Key,entry.Index);foreach(var entry in state.bc.Names)state.global.Name(entry.Key,entry.Index);foreach(var entry in state.bc.Values)state.global.Value(entry.Key,entry.Value);
        }
        private static byte[] Find(OriginalConfigurationPersistence.IndexEntry[] entries,ushort id)
        {foreach(var entry in entries)if(entry.Index==id)return entry.Key;return new byte[0];}
        public void Load()
        {
            Open(os,0,"/nvram_os/agk/configOptionData");Open(bc,1,"/nvram_bc/SuperConfig/BankProgConfigOptionData");
            bool loadOs=os.Buffer.NameEnd!=0;if(!loadOs&&bc.Buffer.NameEnd==0)return;
            if(!Empty(state.os)||!Empty(state.bc)||registry.UnclaimedSnapshot().Length!=0){host.Fault(16,Encoding.ASCII.GetBytes("Aborted recovery of configuration data"));throw new InvalidOperationException("Native nonreturning Fault binding returned");}
            if(loadOs)host.LoadFile(os,false);if(bc.Buffer.NameEnd!=0)host.LoadFile(bc,true);Merge();
            foreach(var entry in state.global.Values)
            {
                byte[] category=Find(state.global.Groups,(ushort)(entry.Key>>16)),name=Find(state.global.Names,(ushort)entry.Key);var option=new OriginalSimpleConfigOption(new byte[0],new byte[0],new byte[0]);
                var value=host.StreamValue(entry.Value);if(value==null)throw new InvalidOperationException("Actual stringstream value result admission required");option.SetValue(value);option.SetName(name);option.SetCategory(category);registry.AddAdmittedUnclaimed(option);
            }
        }
    }
}
