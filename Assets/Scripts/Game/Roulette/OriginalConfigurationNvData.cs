using System;
namespace RouletteDeluxe
{
    // NVConfigOptionData ReadData/WriteData. Slot null represents genuinely
    // unadmitted native stack memory, rather than an invented zero byte.
    public sealed class OriginalConfigurationNvData
    {
        public sealed class ByteSlot{public byte? Value;public ByteSlot(byte? value){Value=value;}}
        public sealed class Stream
        {
            public Action<byte[]> Write;
            public Action<ByteSlot[]> Read;
        }
        public byte Flag;
        public OriginalConfigurationPersistence.NvBuffer Buffer{get;private set;}
        public OriginalConfigurationNvData(OriginalConfigurationPersistence.NvBuffer buffer,byte flag)
        {if(buffer==null)throw new ArgumentNullException(nameof(buffer));Buffer=buffer;Flag=flag;}
        private static byte[] Word(uint value,int count){var result=new byte[count];for(int i=0;i<count;++i)result[i]=(byte)(value>>(i*8));return result;}
        private static ByteSlot[] Slots(uint value,int count){var raw=Word(value,count);var result=new ByteSlot[count];for(int i=0;i<count;++i)result[i]=new ByteSlot(raw[i]);return result;}
        private static byte Known(ByteSlot slot){if(!slot.Value.HasValue)throw new InvalidOperationException("Native unread stack byte requires caller-known storage");return slot.Value.Value;}
        private static uint Value(ByteSlot[] slots){uint result=0;for(int i=0;i<slots.Length;++i)result|=(uint)Known(slots[i])<<(i*8);return result;}
        public void WriteData(Stream stream)
        {
            if(stream==null||stream.Write==null)throw new ArgumentException("Genuine Stream.Write binding required");
            if(Flag==0){stream.Write(Word(Buffer.GroupEnd,4));stream.Write(Word(Buffer.NameEnd,4));stream.Write(Word(Buffer.Length,4));}
            else{var group=Word(Buffer.GroupEnd,2);var name=Word(Buffer.NameEnd,2);stream.Write(group);stream.Write(name);}
            uint at=0;
            if(Buffer.Length!=0)do{byte current=Buffer.ByteAt((int)at);++at;stream.Write(new[]{current});}while(at<Buffer.Length);
            if(Flag!=0)while(at<75000){++at;stream.Write(new byte[]{0});}
        }
        public void ReadData(Stream stream,byte? admittedFirstByte)
        {
            if(stream==null||stream.Read==null)throw new ArgumentException("Genuine Stream.Read binding required");Buffer.Clear();uint count;ByteSlot current;
            if(Flag==0)
            {
                var group=Slots(0,4);try{stream.Read(group);}finally{Buffer.SetGroupEnd(Value(group));}
                var name=Slots(0,4);try{stream.Read(name);}finally{Buffer.SetNameEnd(Value(name));}
                var length=Slots(0,4);stream.Read(length);count=Value(length);current=new ByteSlot(admittedFirstByte);
            }
            else
            {
                var group=Slots(0,2);var name=Slots(0,2);stream.Read(group);stream.Read(name);Buffer.SetGroupEnd(Value(group));Buffer.SetNameEnd(Value(name));count=75000;current=new ByteSlot((byte)Value(name));
            }
            for(uint at=0;at<count;++at){stream.Read(new[]{current});Buffer.Byte(Known(current));}
        }
    }
}
