using System;
namespace RouletteDeluxe
{
    // LoadOptionsFromFile content and callback boundaries; native map allocation/EH remains external.
    public sealed class OriginalConfigurationLoadFile
    {
        public sealed class Temporary
        {
            public readonly object Owner;private readonly byte[] text;
            public byte[] Text{get{return (byte[])text.Clone();}}
            public uint Length{get{return (uint)text.Length;}}
            public Temporary(object owner,byte[] text)
            {if(owner==null||text==null||Array.IndexOf(text,(byte)0)>=0)throw new ArgumentException("Actual NUL-terminated constructor result and temporary owner required");Owner=owner;this.text=(byte[])text.Clone();}
        }
        public sealed class Host
        {
            public Func<OriginalConfigurationNvData,uint,byte> ReadByte;
            public Func<OriginalConfigurationNvData,uint,Temporary> ConstructCString;
            public Action<Temporary> DestroyTemporary;
        }
        private readonly OriginalConfigurationPersistence.State state;private readonly Host host;
        public OriginalConfigurationLoadFile(OriginalConfigurationPersistence.State state,Host h)
        {
            if(state==null||h==null||h.ReadByte==null||h.ConstructCString==null||h.DestroyTemporary==null)throw new ArgumentException("Actual persistence state, native byte access and CString temporary lifetime bindings required");
            this.state=state;host=new Host{ReadByte=h.ReadByte,ConstructCString=h.ConstructCString,DestroyTemporary=h.DestroyTemporary};
        }
        private Temporary Text(OriginalConfigurationNvData data,uint at)
        {var result=host.ConstructCString(data,at);if(result==null)throw new InvalidOperationException("Actual CString temporary admission required");return result;}
        public void Load(OriginalConfigurationNvData data,bool bank)
        {
            if(data==null)throw new ArgumentNullException("data");var tables=bank?state.bc:state.os;uint boundary=data.Buffer.GroupEnd,cursor=0;
            if(boundary!=0)
            {
                do
                {
                    byte hi=host.ReadByte(data,cursor),lo=host.ReadByte(data,unchecked(cursor+1));var text=Text(data,unchecked(cursor+2));uint length=text.Length;ushort id=(ushort)((hi<<8)|lo);bool advance=state.NextGroup<=id;
                    tables.Group(text.Text,id);if(advance)state.NextGroup=unchecked((ushort)(id+1));cursor=unchecked(cursor+length+3);host.DestroyTemporary(text);boundary=data.Buffer.GroupEnd;
                }while(cursor<boundary);
            }
            cursor=boundary;boundary=data.Buffer.NameEnd;
            if(cursor<boundary)
            {
                do
                {
                    byte hi=host.ReadByte(data,cursor),lo=host.ReadByte(data,unchecked(cursor+1));var text=Text(data,unchecked(cursor+2));uint length=text.Length;ushort id=(ushort)((hi<<8)|lo);bool advance=state.NextName<=id;
                    tables.Name(text.Text,id);if(advance)state.NextName=unchecked((ushort)(id+1));cursor=unchecked(cursor+length+3);host.DestroyTemporary(text);boundary=data.Buffer.NameEnd;
                }while(cursor<boundary);
            }
            cursor=boundary;
            while(cursor<data.Buffer.Length)
            {
                uint key=(uint)host.ReadByte(data,cursor)<<24;key|=(uint)host.ReadByte(data,unchecked(cursor+1))<<16;key|=host.ReadByte(data,unchecked(cursor+3));key|=(uint)host.ReadByte(data,unchecked(cursor+2))<<8;
                if(key==0)return;var text=Text(data,unchecked(cursor+4));uint length=text.Length;tables.Value(key,text.Text);cursor=unchecked(cursor+length+5);host.DestroyTemporary(text);
            }
        }
    }
}
