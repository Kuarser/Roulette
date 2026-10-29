using System;
using System.Collections.Generic;
using System.Text;
namespace RouletteDeluxe
{
    // SaveConfigOptions and SaveOptionsToFile. Actual NV.Save and gameMgr reset
    // remain mandatory external bindings; these owners do not write cabinet NV.
    public sealed class OriginalConfigurationPersistence
    {
        public sealed class IndexEntry{public byte[] Key;public ushort Index;}
        public sealed class ValueEntry{public uint Key;public byte[] Value;}
        private sealed class BytesComparer:IComparer<byte[]>
        {public int Compare(byte[] a,byte[] b){for(int i=0;i<Math.Min(a.Length,b.Length);++i)if(a[i]!=b[i])return a[i].CompareTo(b[i]);return a.Length.CompareTo(b.Length);}}
        private static byte[] Copy(byte[] value){if(value==null)throw new ArgumentNullException();return (byte[])value.Clone();}
        private static byte[] A(string value){return Encoding.ASCII.GetBytes(value);}
        public sealed class Tables
        {
            private readonly SortedDictionary<byte[],ushort> groups=new SortedDictionary<byte[],ushort>(new BytesComparer()),names=new SortedDictionary<byte[],ushort>(new BytesComparer());
            private readonly SortedDictionary<uint,byte[]> values=new SortedDictionary<uint,byte[]>();
            public Tables(IndexEntry[] groups,IndexEntry[] names,ValueEntry[] values)
            {
                if(groups==null||names==null||values==null)throw new ArgumentNullException();
                foreach(var entry in groups)this.groups.Add(Copy(entry.Key),entry.Index);
                foreach(var entry in names)this.names.Add(Copy(entry.Key),entry.Index);
                foreach(var entry in values)this.values.Add(entry.Key,Copy(entry.Value));
            }
            private static IndexEntry[] Indices(SortedDictionary<byte[],ushort> map)
            {var result=new IndexEntry[map.Count];int i=0;foreach(var pair in map)result[i++]=new IndexEntry{Key=Copy(pair.Key),Index=pair.Value};return result;}
            public IndexEntry[] Groups{get{return Indices(groups);}}public IndexEntry[] Names{get{return Indices(names);}}
            public ValueEntry[] Values{get{var result=new ValueEntry[values.Count];int i=0;foreach(var pair in values)result[i++]=new ValueEntry{Key=pair.Key,Value=Copy(pair.Value)};return result;}}
            public Tables DeepCopy(){return new Tables(Groups,Names,Values);}
            internal void Clear(){groups.Clear();names.Clear();values.Clear();}
            internal ushort Group(byte[] key){ushort index;if(!groups.TryGetValue(key,out index))groups.Add(Copy(key),0);return index;}
            internal ushort Name(byte[] key){ushort index;if(!names.TryGetValue(key,out index))names.Add(Copy(key),0);return index;}
            internal void Group(byte[] key,ushort index){groups[Copy(key)]=index;}
            internal void Name(byte[] key,ushort index){names[Copy(key)]=index;}
            internal void Value(uint key,byte[] value){values[key]=Copy(value);}
        }
        public sealed class NvBuffer
        {
            private readonly List<byte> bytes;
            public uint GroupEnd{get;private set;}public uint NameEnd{get;private set;}
            public NvBuffer(byte[] admitted,uint groupEnd,uint nameEnd){bytes=new List<byte>(Copy(admitted));GroupEnd=groupEnd;NameEnd=nameEnd;}
            public byte[] Bytes{get{return bytes.ToArray();}}
            internal uint Length{get{return (uint)bytes.Count;}}
            internal void Clear(){bytes.Clear();GroupEnd=0;NameEnd=0;}
            internal void Byte(byte value){bytes.Add(value);}
            internal void Raw(byte[] raw,int count){for(int i=0;i<count;++i)bytes.Add(raw[i]);}
            internal void EndGroups(){GroupEnd=Length;}internal void EndNames(){NameEnd=Length;}
            internal void SetGroupEnd(uint value){GroupEnd=value;}internal void SetNameEnd(uint value){NameEnd=value;}
            internal byte ByteAt(int index){return bytes[index];}
        }
        public sealed class State
        {
            internal readonly Tables global,os,bc;
            public Tables Global{get{return global.DeepCopy();}}public Tables Os{get{return os.DeepCopy();}}public Tables Bc{get{return bc.DeepCopy();}}
            public ushort NextGroup{get;internal set;}public ushort NextName{get;internal set;}
            public NvBuffer OsBuffer{get;private set;}public NvBuffer BcBuffer{get;private set;}
            public uint TotalOs{get;internal set;}public uint TotalBc{get;internal set;}public uint UsedOs{get;internal set;}public uint UsedBc{get;internal set;}
            public State(Tables global,Tables os,Tables bc,ushort nextGroup,ushort nextName,NvBuffer osBuffer,NvBuffer bcBuffer,uint totalOs,uint totalBc,uint usedOs,uint usedBc)
            {
                if(global==null||os==null||bc==null||osBuffer==null||bcBuffer==null||ReferenceEquals(osBuffer,bcBuffer))throw new ArgumentException("Distinct actual OS/BC buffer owners and admitted persistence state required");
                this.global=global.DeepCopy();this.os=os.DeepCopy();this.bc=bc.DeepCopy();NextGroup=nextGroup;NextName=nextName;OsBuffer=osBuffer;BcBuffer=bcBuffer;TotalOs=totalOs;TotalBc=totalBc;UsedOs=usedOs;UsedBc=usedBc;
            }
        }
        public sealed class Host
        {
            public Func<OriginalConfigurationRegistry.MapEntry[]> GetOptionMap;
            public Action ForceReset;
            public Action<NvBuffer> SaveOs,SaveBc;
            public Action<uint,byte[]> Fault;
        }
        private readonly State state;private readonly Host host;
        public OriginalConfigurationPersistence(State state,Host host)
        {
            if(state==null||host==null||host.GetOptionMap==null||host.ForceReset==null||host.SaveOs==null||host.SaveBc==null||host.Fault==null)throw new ArgumentException("Genuine option map, gameMgr reset, OS/BC NV.Save and nonreturning fault bindings required");
            this.state=state;this.host=new Host{GetOptionMap=host.GetOptionMap,ForceReset=host.ForceReset,SaveOs=host.SaveOs,SaveBc=host.SaveBc,Fault=host.Fault};
        }
        private static void BigEndian(NvBuffer data,uint value,int count){for(int i=count-1;i>=0;--i)data.Byte((byte)(value>>(i*8)));}
        private static void Capacity(NvBuffer data,uint amount,uint capacity,Action<uint,byte[]> fault,string message)
        {if(unchecked(data.Length+amount)<=capacity)return;fault(0x10,A(message));throw new InvalidOperationException("Native nonreturning Fault binding returned");}
        public static uint Encode(NvBuffer data,Tables tables,uint capacity,Action<uint,byte[]> fault)
        {
            if(data==null||tables==null||fault==null)throw new ArgumentNullException();
            foreach(var entry in tables.Groups)
            {BigEndian(data,entry.Index,2);Capacity(data,(uint)entry.Key.Length,capacity,fault,"Group Data size exceeds max config data size");data.Raw(entry.Key,entry.Key.Length);data.Byte(0);}
            data.EndGroups();
            foreach(var entry in tables.Names)
            {BigEndian(data,entry.Index,2);int count=Math.Min(entry.Key.Length,61);Capacity(data,(uint)count,capacity,fault,"Name Data size exceeds max config data size");data.Raw(entry.Key,count);data.Byte(0);}
            data.EndNames();
            foreach(var entry in tables.Values)
            {BigEndian(data,entry.Key,4);Capacity(data,(uint)entry.Value.Length,capacity,fault,"Value Data size exceeds max config data size");data.Raw(entry.Value,entry.Value.Length);data.Byte(0);}
            return data.Length;
        }
        private static bool Matches(byte[] raw,byte[] text,int count)
        {for(int i=0;i<count;++i){byte actual=i<raw.Length?raw[i]:(byte)0;if(actual!=text[i])return false;}return true;}
        private static bool BankCategory(byte[] category)
        {return Matches(category,A("/bc"),3)||Matches(category,A("/sbd/s"),6)||Matches(category,A("/os/bng/ha\0"),11);}
        public void Save(bool force)
        {
            if(force){host.ForceReset();state.global.Clear();state.os.Clear();state.bc.Clear();state.NextGroup=1;state.NextName=1;}
            state.OsBuffer.Clear();state.BcBuffer.Clear();
            var map=host.GetOptionMap();if(map==null)throw new InvalidOperationException("Actual unique OptionHandles map required");
            foreach(var entry in map)
            {
                var option=entry.Handle.Primary;if(option.Control==10||option.Flags[11]!=0)continue;
                byte[] value=option.Value;if(option.Flags[12]!=0&&value.Length==0)continue;
                byte[] category=option.Category,name=option.Name;ushort group=state.global.Group(category),index=state.global.Name(name);
                if(group==0){group=state.NextGroup;state.global.Group(category,group);state.NextGroup=unchecked((ushort)(state.NextGroup+1));}
                if(index==0){index=state.NextName;state.global.Name(name,index);state.NextName=unchecked((ushort)(state.NextName+1));}
                uint key=((uint)group<<16)|index;state.global.Value(key,value);var tables=BankCategory(category)?state.bc:state.os;
                tables.Group(category,group);tables.Name(name,index);tables.Value(key,value);
            }
            state.TotalOs=268000;state.TotalBc=75000;
            state.UsedOs=Encode(state.OsBuffer,state.os,state.TotalOs,host.Fault);
            state.UsedBc=Encode(state.BcBuffer,state.bc,state.TotalBc,host.Fault);
            host.SaveOs(state.OsBuffer);host.SaveBc(state.BcBuffer);
        }
    }
}
