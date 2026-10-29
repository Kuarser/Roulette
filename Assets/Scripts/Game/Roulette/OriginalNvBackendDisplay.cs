using System;
using System.Collections.Generic;
using System.Text;

namespace RouletteDeluxe
{
    // Raw NV owned strings and admitted C varargs formatting. No .NET printf
    // substitute or native allocator/refcount equivalence is supplied.
    public sealed class OriginalNvBackendDisplay
    {
        public sealed class Argument
        {
            public readonly bool IsText;
            public readonly uint Bits;
            private readonly byte[] text;
            public byte[] Text { get { return text==null?null:(byte[])text.Clone(); } }
            private Argument(bool isText,uint bits,byte[] text){IsText=isText;Bits=bits;this.text=text==null?null:(byte[])text.Clone();}
            public static Argument Word(uint bits){return new Argument(false,bits,null);}
            public static Argument CString(byte[] text){return new Argument(true,0,text);}
        }
        public sealed class State
        {
            public readonly OriginalNvBackendSize.State Storage;
            private byte[] description,current,previous;
            public byte[] Description { get { return (byte[])description.Clone(); } }
            public byte[] CurrentLog { get { return (byte[])current.Clone(); } }
            public byte[] PreviousLog { get { return (byte[])previous.Clone(); } }
            public State(OriginalNvBackendSize.State storage,byte[] description,byte[] current,byte[] previous)
            {
                if(storage==null||description==null||current==null||previous==null)throw new ArgumentException("Actual NV raw string owners required");
                Storage=storage;this.description=(byte[])description.Clone();this.current=(byte[])current.Clone();this.previous=(byte[])previous.Clone();
            }
            internal void Update(byte[] text){previous=(byte[])current.Clone();current=(byte[])text.Clone();}
            internal void SetDescription(byte[] text){description=(byte[])text.Clone();}
        }
        public delegate int Formatter(byte[] buffer,uint capacity,byte[] format,Argument[] arguments);
        public sealed class Host { public Formatter Vsnprintf; public Func<byte> LogDisplayEnabled; }
        private readonly State state;
        private readonly Formatter formatter;
        private readonly Func<byte> logEnabled;
        public OriginalNvBackendDisplay(State state,Host h)
        {
            if(state==null||h==null||h.Vsnprintf==null||h.LogDisplayEnabled==null)throw new ArgumentException("Actual NV string, formatter and global flag owners required");
            this.state=state;formatter=h.Vsnprintf;logEnabled=h.LogDisplayEnabled;
        }
        private static byte[] A(string s){return Encoding.ASCII.GetBytes(s);}
        private static byte[] Join(params byte[][] parts)
        {var result=new List<byte>();foreach(var part in parts)result.AddRange(part);return result.ToArray();}
        private static byte[] Format(byte[] format,Argument[] arguments,Formatter callback)
        {
            if(callback==null||arguments==null)throw new ArgumentException("Actual C varargs formatting binding and argument frame required");
            var buffer=new byte[1024]; // Native memset of 256 words, before vsnprintf.
            callback(buffer,1024,(byte[])format.Clone(),(Argument[])arguments.Clone()); // Return deliberately ignored.
            int length=Array.IndexOf(buffer,(byte)0);
            if(length<0)throw new InvalidOperationException("Native formatted C-string requires admitted terminator");
            var result=new byte[length];Array.Copy(buffer,result,length);return result;
        }
        public byte[] GetName(){return state.Storage.Path;}
        public byte[] GetDisplayName(){return Join(A("Loc  : "),state.Storage.Path,A("\n"));}
        public byte[] GetDisplayInfo(byte[] format,params Argument[] arguments)
        {
            if(format==null)throw new ArgumentException("Native GetDisplayInfo requires readable C format pointer");
            byte[] result=Join(A("Desc : "),state.Description,A("\n"));
            if(format.Length!=0&&format[0]!=0)result=Join(result,A("Error: "),Format(format,arguments,formatter),A("\n"));
            if(logEnabled()!=0&&state.CurrentLog.Length!=0)
                result=Join(result,A("Curr : "),state.CurrentLog,A("\nLast : "),state.PreviousLog,A("\n"));
            return result;
        }
        public void UpdateNvLog(byte[] text)
        {
            if(text==null)throw new ArgumentException("Native log stdstring bytes required");
            if(text.Length!=0)state.Update(text);
        }
        public static byte[] GetFormattedNvLog(byte[] format,Argument[] arguments,Formatter callback)
        {
            if(format==null)return new byte[0];
            return Format(format,arguments,callback); // Even an empty C format calls vsnprintf.
        }
        public byte[] IoDisplayInfo(byte[] format,byte[] text,uint offset,uint count,uint last)
        {
            return text==null?GetDisplayInfo(format,Argument.Word(offset),Argument.Word(count),Argument.Word(last)):
                GetDisplayInfo(format,Argument.CString(text),Argument.Word(offset),Argument.Word(count),Argument.Word(last));
        }
        public byte[] SizeDisplayInfo(byte[] format,int? error,byte[] text)
        {
            return error.HasValue?GetDisplayInfo(format,Argument.Word(unchecked((uint)error.Value)),Argument.CString(text)):
                GetDisplayInfo(format,new Argument[0]);
        }
    }
}
