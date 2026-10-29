using System;
using System.Collections.Generic;
using System.Text;
namespace RouletteDeluxe
{
    public sealed class OriginalSystemConfigurationFault:Exception
    {
        public readonly int NativeCode=0x10;public readonly byte[] Key;
        public OriginalSystemConfigurationFault(string operation,byte[] key):base(operation){Key=(byte[])key.Clone();}
    }
    // Native SystemConfiguration08e03fb0/08e041f0/08e03710. Raw std::string bytes
    // preserve length and embedded NUL; map[] misses insert empty values.
    public class OriginalSystemConfiguration
    {
        public sealed class SharedState
        {
            public byte DefaultsLoaded;
            public OriginalSystemConfiguration GlobalOwner;
            internal readonly List<byte[]> TrueValues=new List<byte[]>();
            public int TrueValueCount{get{return TrueValues.Count;}}
        }
        public sealed class Entry{public byte[] Key,Value;}
        private sealed class BytesComparer:IEqualityComparer<byte[]>,IComparer<byte[]>
        {
            public bool Equals(byte[] a,byte[] b){if(a.Length!=b.Length)return false;for(int i=0;i<a.Length;++i)if(a[i]!=b[i])return false;return true;}
            public int GetHashCode(byte[] b){unchecked{int h=17;for(int i=0;i<b.Length;++i)h=h*31+b[i];return h;}}
            public int Compare(byte[] a,byte[] b){for(int i=0;i<Math.Min(a.Length,b.Length);++i)if(a[i]!=b[i])return a[i].CompareTo(b[i]);return a.Length.CompareTo(b.Length);}
        }
        private static readonly BytesComparer comparer=new BytesComparer();
        private readonly Dictionary<byte[],byte[]> options=new Dictionary<byte[],byte[]>(comparer);
        private readonly SharedState shared;private readonly Func<byte[]> readGameExecutorName;
        private byte[] currentService=new byte[0];
        public OriginalSystemConfiguration(SharedState state,Func<byte[]> gameExecutorName)
        {
            shared=state??throw new ArgumentNullException(nameof(state));readGameExecutorName=gameExecutorName??throw new ArgumentNullException(nameof(gameExecutorName));
            shared.GlobalOwner=this;
            foreach(string token in new[]{"True","Enabled","Yes","On","y"})shared.TrueValues.Add(Encoding.ASCII.GetBytes(token));
        }
        private static byte[] Copy(byte[] bytes){if(bytes==null)throw new ArgumentNullException(nameof(bytes));return (byte[])bytes.Clone();}
        public byte[] CurrentService{get{return Copy(currentService);}set{currentService=Copy(value);}}
        public int Count{get{return options.Count;}}
        public Entry[] Snapshot()
        {
            var keys=new List<byte[]>(options.Keys);keys.Sort(comparer);var result=new Entry[keys.Count];
            for(int i=0;i<keys.Count;++i)result[i]=new Entry{Key=Copy(keys[i]),Value=Copy(options[keys[i]])};return result;
        }
        private byte[] Index(byte[] key)
        {byte[] value;if(!options.TryGetValue(key,out value)){value=new byte[0];options.Add(Copy(key),value);}return value;}
        private static byte[] Scoped(byte[] key,byte[] scope)
        {var b=new byte[checked(key.Length+scope.Length+1)];Array.Copy(key,b,key.Length);b[key.Length]=45;Array.Copy(scope,0,b,key.Length+1,scope.Length);return b;}
        public void AddConfigurable(byte[] suppliedKey,byte[] suppliedValue)
        {
            byte[] key=Copy(suppliedKey),value=Copy(suppliedValue);
            if(shared.DefaultsLoaded!=0&&Index(key).Length==0)throw new OriginalSystemConfigurationFault("Undefined configurable",key);
            if(currentService.Length!=0)
            {
                if(shared.GlobalOwner==null)throw new InvalidOperationException("Native global SystemConfiguration owner missing");
                key=Scoped(key,shared.GlobalOwner.currentService); // Native tests this owner but reads the global owner.
            }
            options[key]=value;
        }
        public virtual byte[] GetOption(byte[] suppliedKey,byte[] suppliedScope)
        {
            byte[] key=Copy(suppliedKey),scope=Copy(suppliedScope);
            if(scope.Length==0)scope=Copy(readGameExecutorName());
            byte[] found=Index(Scoped(key,scope));
            if(found.Length==0){found=Index(key);if(found.Length==0)throw new OriginalSystemConfigurationFault("Unknown configuration",key);}
            return Copy(found);
        }
        private static int Lower(byte b){return b>=65&&b<=90?b+32:b;}
        public byte GetOptionBool(byte[] key,byte[] scope)
        {
            byte[] value=GetOption(Copy(key),Copy(scope));
            foreach(byte[] token in shared.TrueValues)
            {
                if(value.Length!=token.Length)continue;bool match=true;
                for(int i=0;i<value.Length;++i)if(Lower(value[i])!=Lower(token[i])){match=false;break;}
                if(match)return 1;
            }
            return 0;
        }
        // Binds only the two genuine SystemConfiguration reads. Other configuration
        // owners/conversions retain their caller-provided bindings.
        public OriginalMachineDataDefaults.Host BindMachineDefaults(OriginalMachineDataDefaults.Host supplied)
        {
            if(supplied==null)throw new ArgumentNullException(nameof(supplied));
            return new OriginalMachineDataDefaults.Host{PrimaryOperation=supplied.PrimaryOperation,JurisdictionByte=supplied.JurisdictionByte,JurisdictionInt=supplied.JurisdictionInt,JurisdictionAmount=supplied.JurisdictionAmount,JurisdictionText=supplied.JurisdictionText,ParseFile=supplied.ParseFile,ParserString=supplied.ParserString,ClearParser=supplied.ClearParser,ExtractStreamInt64=supplied.ExtractStreamInt64,ScanInt64=supplied.ScanInt64,ScanInt32=supplied.ScanInt32,SystemString=(key,scope)=>GetOption(Encoding.UTF8.GetBytes(key),Encoding.UTF8.GetBytes(scope)),SystemBool=(key,scope)=>GetOptionBool(Encoding.UTF8.GetBytes(key),Encoding.UTF8.GetBytes(scope))};
        }
    }
}
