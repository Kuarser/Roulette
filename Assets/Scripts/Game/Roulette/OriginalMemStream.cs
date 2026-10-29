using System;
namespace RouletteDeluxe
{
    // libsdg MemStream. NULL slots retain unadmitted native allocation bytes.
    public sealed class OriginalMemStream
    {
        public sealed class Host
        {
            public Func<uint,OriginalConfigurationNvData.ByteSlot[]> Allocate;
            public Func<OriginalConfigurationNvData.ByteSlot[],uint,OriginalConfigurationNvData.ByteSlot[]> Reallocate;
            public Action<OriginalConfigurationNvData.ByteSlot[]> Free;
            public Action AssertAllocation;
        }
        private OriginalConfigurationNvData.ByteSlot[] buffer;private readonly Host host;
        public uint Ownership{get;private set;}public uint Mode{get;private set;}public uint Position{get;private set;}public uint Size{get;private set;}public uint Capacity{get;private set;}
        public OriginalConfigurationNvData.ByteSlot[] Buffer{get{return buffer;}}
        public OriginalMemStream(OriginalConfigurationNvData.ByteSlot[] data,uint size,uint capacity,uint mode,Host host)
        {
            if(host==null||host.Allocate==null||host.Reallocate==null||host.Free==null||host.AssertAllocation==null)throw new ArgumentException("Actual native allocation and assertion bindings required");
            this.host=new Host{Allocate=host.Allocate,Reallocate=host.Reallocate,Free=host.Free,AssertAllocation=host.AssertAllocation};buffer=data;Ownership=0;
            uint admittedCapacity=Math.Max(size,capacity);
            if(data==null){Ownership=1;if(admittedCapacity!=0){buffer=this.host.Allocate(admittedCapacity);if(buffer==null){this.host.AssertAllocation();throw new InvalidOperationException("Native nonreturning allocation assertion returned");}}}
            Capacity=admittedCapacity;Size=size;Position=0;Mode=mode;
        }
        private static void Range(OriginalConfigurationNvData.ByteSlot[] bytes,uint at,uint count)
        {if(bytes==null||((ulong)at+count)>(ulong)bytes.Length)throw new InvalidOperationException("Native memory range requires admitted readable/writable storage");for(uint i=0;i<count;++i)if(bytes[(int)(at+i)]==null)throw new InvalidOperationException("Native allocation byte frame required");}
        public void Read(OriginalConfigurationNvData.ByteSlot[] destination)
        {
            if(destination==null)throw new ArgumentNullException("destination");uint count=(uint)destination.Length;
            if(unchecked(Position+count)>Size)return;
            if(count!=0){Range(buffer,Position,count);Range(destination,0,count);for(uint i=0;i<count;++i)destination[(int)i].Value=buffer[(int)(Position+i)].Value;}
            Position=unchecked(Position+count);
        }
        public void Write(byte[] source)
        {
            if(Mode==0)return;if(source==null)throw new ArgumentNullException("source");uint count=(uint)source.Length;
            if(Capacity<unchecked(Position+count)){Capacity=unchecked(Position+count+1023)&0xfffffc00;buffer=host.Reallocate(buffer,Capacity);if(buffer==null)return;}
            if(count!=0){Range(buffer,Position,count);for(uint i=0;i<count;++i)buffer[(int)(Position+i)].Value=source[(int)i];}
            Position=unchecked(Position+count);if(Size<Position)Size=Position;
        }
        public void Seek(uint position)
        {
            if(Size<position)
            {
                if(Mode==0)return;
                if(Capacity<position){Capacity=unchecked(position+1023)&0xfffffc00;buffer=host.Reallocate(buffer,Capacity);if(buffer==null)return;}
                uint count=unchecked(Capacity-Size);if(count!=0){Range(buffer,Size,count);for(uint i=0;i<count;++i)buffer[(int)(Size+i)].Value=0;}
                Size=position;
            }
            Position=position;
        }
        public void Close(){ReleaseForDestruction(Ownership);}
        internal void ReleaseForDestruction(uint admittedOwnership){if(admittedOwnership!=0&&buffer!=null){host.Free(buffer);buffer=null;}Ownership=1;}
        public void ClearLength(){Size=0;}public void ClearPosition(){Position=0;}
        private byte[] Data()
        {
            if(buffer==null)return null;Range(buffer,0,Size);var result=new byte[checked((int)Size)];for(int i=0;i<result.Length;++i){if(!buffer[i].Value.HasValue)throw new InvalidOperationException("Native unread allocation byte requires caller-known storage");result[i]=buffer[i].Value.Value;}return result;
        }
        public OriginalNv2Save.Cache SaveCache(){return new OriginalNv2Save.Cache(()=>Mode,ClearPosition,ClearLength,()=>Size,Data);}
        public OriginalConfigurationNvData.Stream DataStream(){return new OriginalConfigurationNvData.Stream{Read=Read,Write=Write};}
    }
}
