using System;
using System.Text;
namespace RouletteDeluxe
{
    // Parser083b8630/083b8660/083b8740/083b8ac0. Required factory owns dictionaries.
    public sealed class OriginalMachineConfigurationParser:IDisposable
    {
        public sealed class Dictionary
        {
            public readonly Action<byte[]> AddSymbolsFrom;
            public readonly Func<byte[],bool> HasKey;
            public readonly Func<byte[],byte[]> GetString;
            public readonly Action Delete;
            public Dictionary(Action<byte[]> add,Func<byte[],bool> has,Func<byte[],byte[]> get,Action delete)
            {AddSymbolsFrom=add??throw new ArgumentNullException();HasKey=has??throw new ArgumentNullException();GetString=get??throw new ArgumentNullException();Delete=delete??throw new ArgumentNullException();}
        }
        private readonly Func<Dictionary> create;private Dictionary current;private bool deleted;
        public OriginalMachineConfigurationParser(Func<Dictionary> factory){create=factory??throw new ArgumentNullException();current=create();}
        public Dictionary CurrentDictionary{get{Check();return current;}}
        public sealed class PhysicalFileCounts
        {
            public int Builds{get;private set;}public int Initializations{get;private set;}public int Terminations{get;private set;}public int RootDeletes{get;private set;}public int ParserDeletes{get;private set;}public int ErrorCreates{get;private set;}public int ErrorDeletes{get;private set;}
            internal void Record(OriginalPhysicalCfgParser parser)
            {Builds+=parser.BuildCalls;Initializations+=parser.Initializations;Terminations+=parser.Terminations;RootDeletes+=parser.ParsedDeletes;ParserDeletes+=parser.NativeParserDeletes;ErrorCreates+=parser.ErrorCreates;ErrorDeletes+=parser.ErrorDeletes;}
        }
        public PhysicalFileCounts PhysicalFiles{get;private set;}
        private void Check(){if(deleted)throw new ObjectDisposedException(nameof(OriginalMachineConfigurationParser));}
        private static byte[] Prefix(byte[] raw){if(raw==null)throw new ArgumentNullException();int n=Array.IndexOf(raw,(byte)0);if(n<0)n=raw.Length;var result=new byte[n];Array.Copy(raw,result,n);return result;}
        public bool ParseFile(byte[] path)
        {
            Check();try{current.AddSymbolsFrom(Prefix(path));return true;}
            catch(OriginalCfgDictionaryException){return false;}
            catch(OriginalCfgPathException){return false;} // CFG tree's native std::string failures.
        }
        // Owned C-string transport: null means absent; empty present values remain present.
        public byte[] ReadString(byte[] key)
        {Check();byte[] supplied=Prefix(key);if(!current.HasKey(supplied))return null;return Prefix(current.GetString(Prefix(key)));}
        public bool GetDataString(byte[] key,byte[] destination)
        {
            Check();if(destination==null||destination.Length==0)throw new ArgumentException("Writable native string destination required");destination[0]=0;
            byte[] result=ReadString(key);if(result==null)return false;
            if(result.Length>=destination.Length)throw new ArgumentException("Unchecked native strcpy overflow is outside managed admission");
            Array.Copy(result,destination,result.Length);destination[result.Length]=0;return true;
        }
        public bool ClearData(){Check();if(current!=null)current.Delete();Dictionary next=create();current=next;return true;}
        public void Dispose(){Check();if(current!=null)current.Delete();deleted=true;}
        // Real XML + recovered dictionary transport. Native allocator/destructors remain outside this managed lifetime admission.
        public static OriginalMachineConfigurationParser OpenManaged(Func<byte[],string> resolve,Encoding narrow,OriginalCfgIntegerConverter.State errno,Action<string,OriginalCfgDocument.Node,OriginalCfgStringDictionary> otherTypes=null)
        {
            if(resolve==null||narrow==null)throw new ArgumentNullException();
            return new OriginalMachineConfigurationParser(()=>{
                var values=new OriginalCfgStringDictionary();bool released=false;Action check=()=>{if(released)throw new ObjectDisposedException("CFGHybridDictionary");};
                return new Dictionary(path=>{check();var doc=OriginalCfgDocument.OpenFile(resolve(path),narrow);var root=doc.GetParsedData();values.AddSymbolsFrom(root,errno,otherTypes);root.Delete();doc.Dispose();},
                    key=>{check();return values.HasKey(key);},key=>{check();return values.GetString(key);},()=>{check();released=true;});
            });
        }
        // MachineData files use the original physical file-parser controls in caller memory.
        // Dictionary traversal/value transport is still managed. Closing roots/parser on
        // traversal failure is a diagnostic owner policy, not native EH qualification.
        public static OriginalMachineConfigurationParser OpenPhysical(OriginalNativeMemory memory,Func<byte[],string> resolve,bool atomic,OriginalCfgIntegerConverter.State errno,OriginalPhysicalCfgTypeEnvironment environment=null,Action<string,OriginalCfgDocument.Node,OriginalCfgStringDictionary> otherTypes=null)
        {
            if(memory==null||resolve==null)throw new ArgumentNullException();
            if(environment!=null)environment.Validate(memory,atomic);
            var counts=new PhysicalFileCounts();
            var result=new OriginalMachineConfigurationParser(()=>{
                var values=new OriginalCfgStringDictionary();bool released=false;Action check=()=>{if(released)throw new ObjectDisposedException("CFGHybridDictionary");};
                return new Dictionary(path=>{
                    check();var parser=OriginalPhysicalCfgParser.OpenFile(memory,resolve(path),atomic,environment);OriginalPhysicalCfgParser.ParsedRoot root=null;
                    try{root=parser.GetParsedRoot();values.AddSymbolsFrom(root.Node,errno,otherTypes);}
                    finally{if(root!=null)root.Delete();parser.Dispose();counts.Record(parser);}
                },key=>{check();return values.HasKey(key);},key=>{check();return values.GetString(key);},()=>{check();released=true;});
            });
            result.PhysicalFiles=counts;return result;
        }
        public OriginalMachineDataDefaults.Host BindMachineDefaults(OriginalMachineDataDefaults.Host h,Encoding narrow)
        {
            if(h==null||narrow==null)throw new ArgumentNullException();
            return new OriginalMachineDataDefaults.Host{PrimaryOperation=h.PrimaryOperation,JurisdictionByte=h.JurisdictionByte,JurisdictionInt=h.JurisdictionInt,JurisdictionAmount=h.JurisdictionAmount,JurisdictionText=h.JurisdictionText,ParseFile=path=>{ParseFile(narrow.GetBytes(path));},ParserString=key=>ReadString(narrow.GetBytes(key)),ClearParser=()=>{ClearData();},SystemString=h.SystemString,SystemBool=h.SystemBool,ExtractStreamInt64=h.ExtractStreamInt64,ScanInt64=h.ScanInt64,ScanInt32=h.ScanInt32};
        }
    }
}
