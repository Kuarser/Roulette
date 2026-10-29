using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
namespace RouletteDeluxe
{
 // Resource ports for recovered GetOption/GetOptionBit/GetOptionStr. Logical
 // ConfigurationOption metadata and ostream formatting remain admitted ports.
 // Caller supplies native control layouts, error transport and persistent owners.
 public sealed class OriginalJurisdictionOptionResources : IDisposable
 {
  private sealed class VectorOwner{internal uint Address;internal OriginalConfigurationOption[] Items;}
  private readonly OriginalNativeMemory memory;
  private readonly OriginalConfigurationMemoryStrings strings;
  private readonly OriginalJurisdictionBinaryReader reader;
  private readonly Func<uint,OriginalConfigurationRegistry> registry;
  private readonly OriginalJurisdictionHelpStringBuffer bufferExtractor;private readonly uint extractionFrame;
  private readonly OriginalCfgIntegerConverter.State errno;
  private readonly Func<OriginalPhysicalJurisdictionDictionary> dictionary;
  private readonly Func<string,string> resolvePath;
  private readonly Func<byte[],byte[]> help;
  private readonly Func<uint> configInstance;
  private readonly uint machine,optionTable;
  private readonly Dictionary<uint,VectorOwner> vectors=new Dictionary<uint,VectorOwner>();
  private readonly Dictionary<uint,byte[]> streams=new Dictionary<uint,byte[]>();
  private bool disposed;
  public int LiveVectors{get{return vectors.Count;}}public int LiveStreams{get{return streams.Count;}}
  public OriginalJurisdictionOptionResources(OriginalNativeMemory memory,OriginalConfigurationMemoryStrings strings,OriginalJurisdictionBinaryReader reader,Func<uint,OriginalConfigurationRegistry> registry,OriginalJurisdictionHelpStringBuffer bufferExtractor,uint extractionFrame,OriginalCfgIntegerConverter.State errno,Func<OriginalPhysicalJurisdictionDictionary> dictionary,Func<string,string> resolvePath,Func<byte[],byte[]> help,Func<uint> configInstance,uint machine,uint optionTable)
  {
   if(memory==null||strings==null||reader==null||registry==null||bufferExtractor==null||extractionFrame==0||errno==null||dictionary==null||resolvePath==null||help==null||configInstance==null||machine==0||optionTable==0)throw new ArgumentException("Persistent option resource owners required");
   this.memory=memory;this.strings=strings;this.reader=reader;this.registry=registry;this.bufferExtractor=bufferExtractor;this.extractionFrame=extractionFrame;this.errno=errno;this.dictionary=dictionary;this.resolvePath=resolvePath;this.help=help;this.configInstance=configInstance;this.machine=machine;this.optionTable=optionTable;
  }
  private void Alive(){if(disposed)throw new ObjectDisposedException("OriginalJurisdictionOptionResources");}
  private void Machine(uint owner){if(owner!=machine)throw new ArgumentException("Live jurisdiction owner required");}
  private void Put(uint destination,byte[] bytes)
  {
   uint source=memory.Allocate((uint)Math.Max(1,bytes.Length));try{memory.WriteBytes(source,bytes);strings.ConstructRange(destination,source,source+(uint)bytes.Length,source);}finally{memory.Free(source);}
  }
  private OriginalConfigurationOption Item(uint owner)
  {
   foreach(var vector in vectors.Values)if(owner>=vector.Address){uint index=(owner-vector.Address)/132;if((owner-vector.Address)%132==0&&index<vector.Items.Length&&vector.Items[index]!=null)return vector.Items[index];}
   throw new InvalidOperationException("Live configuration option required");
  }
  private uint Lookup(int kind,uint method,uint owner,uint name)
  {
   Alive();var live=dictionary();if(live==null||live.Owner!=owner||memory.ReadWord(machine+0x10)!=owner)throw new InvalidOperationException("Live physical jurisdiction dictionary required");
   uint offset=kind==0?0x2cu:kind==1?0x28u:0x24u;if(memory.ReadWord(memory.ReadWord(owner)+offset)!=method)throw new InvalidOperationException("Original dictionary virtual method required");
   return live.GetOptionPointer(kind,strings.Read(name));
  }
  private void Create(uint header,uint path)
  {
   Alive();var scanner=new OriginalConfigurationOption.Vector();if(!scanner.BindScanner(errno).AddAdmittedContent(File.ReadAllBytes(resolvePath(Encoding.ASCII.GetString(strings.Read(path))))))throw new InvalidOperationException("Configuration option template scan failed");
   var items=scanner.Snapshot();if(items.Length==0){memory.WriteWord(header,0);memory.WriteWord(header+4,0);memory.WriteWord(header+8,0);return;}
   uint address=memory.Allocate(checked((uint)items.Length*132));var vector=new VectorOwner{Address=address,Items=items};vectors.Add(address,vector);
   // Only the original fields consumed by GetOption controls are materialized.
   var initialized=new List<uint>();try{for(uint i=0;i<items.Length;++i){uint owner=address+i*132;memory.WriteWord(owner,optionTable);Put(owner+12,items[i].Value);initialized.Add(owner+12);Put(owner+36,items[i].Help);initialized.Add(owner+36);memory.WriteByte(owner+40,items[i].HasHelp?(byte)1:(byte)0);}}
   catch{for(int i=initialized.Count-1;i>=0;--i)strings.Destroy(initialized[i]);foreach(var item in items)item.Dispose();vectors.Remove(address);memory.Free(address);throw;}
   memory.WriteWord(header,address);memory.WriteWord(header+4,address+(uint)items.Length*132);memory.WriteWord(header+8,address+(uint)items.Length*132);
  }
  private void Assign(uint destination,uint source)
  {
   Alive();byte[] value=strings.Read(source);strings.Assign(destination,source);
   foreach(var vector in vectors.Values)for(uint i=0;i<vector.Items.Length;++i){uint owner=vector.Address+i*132;if(destination==owner+12){Item(owner).SetValue(value);return;}if(destination==owner+36){Item(owner).Incorporate(Encoding.ASCII.GetBytes("OptionHelp"),value,0);return;}}
   // Other caller-owned COW destinations are legal, e.g. string settings.
  }
  private void DestroyOption(uint method,uint owner)
  {
   if(method!=memory.ReadWord(optionTable))throw new InvalidOperationException("Original option destructor method required");var item=Item(owner);strings.Destroy(owner+36);strings.Destroy(owner+12);item.Dispose();
   foreach(var vector in vectors.Values)if(owner>=vector.Address&&(owner-vector.Address)%132==0){uint index=(owner-vector.Address)/132;if(index<vector.Items.Length){vector.Items[index]=null;return;}}
  }
  private void Delete(uint address)
  {
   VectorOwner vector;if(!vectors.TryGetValue(address,out vector))throw new InvalidOperationException("Owned option vector allocation required");foreach(var item in vector.Items)if(item!=null)throw new InvalidOperationException("Destroy option elements before their allocation");vectors.Remove(address);memory.Free(address);
  }
  private void DestroyVector(uint header)
  {
   uint address=memory.ReadWord(header);if(address==0)return;VectorOwner vector;if(!vectors.TryGetValue(address,out vector))throw new InvalidOperationException("Live option vector required");for(uint i=0;i<vector.Items.Length;++i)if(vector.Items[i]!=null)DestroyOption(memory.ReadWord(optionTable),address+i*132);Delete(address);memory.WriteWord(header,0);memory.WriteWord(header+4,0);memory.WriteWord(header+8,0);
  }
  private void Register(uint server,uint header,uint prefix)
  {
   Alive();if(server==0)throw new InvalidOperationException("Captured configuration server required");uint address=memory.ReadWord(header);VectorOwner vector;if(!vectors.TryGetValue(address,out vector))throw new InvalidOperationException("Live option registration vector required");
   OriginalConfigurationRegistry target=registry(server);if(target==null)throw new InvalidOperationException("Captured configuration server binding required");target.Register(vector.Items,strings.Read(prefix),new OriginalConfigurationRegistry.Frame());
  }
  private void ConstructStream(uint owner,uint mode){Alive();if(mode!=0x18)throw new ArgumentException("Original in/out stream mode required");streams.Add(owner,new byte[0]);}
  private void Insert(uint ostream,byte[] bytes){Alive();uint owner=ostream-8;byte[] previous;if(!streams.TryGetValue(owner,out previous))throw new InvalidOperationException("Live ostream required");var joined=new byte[checked(previous.Length+bytes.Length)];Array.Copy(previous,joined,previous.Length);Array.Copy(bytes,0,joined,previous.Length,bytes.Length);streams[owner]=joined;}
  private void Extract(uint destination,uint buffer){Alive();byte[] bytes;if(!streams.TryGetValue(buffer-12,out bytes))throw new InvalidOperationException("Live string buffer required");uint source=memory.Allocate((uint)Math.Max(1,bytes.Length));try{memory.WriteBytes(source,bytes);memory.WriteWord(buffer+0x10,source);memory.WriteWord(buffer+0xc,source+(uint)bytes.Length);memory.WriteWord(buffer+0x14,source+(uint)bytes.Length);bufferExtractor.Extract(destination,buffer,extractionFrame);}finally{memory.Free(source);}}
  private void DestroyStream(uint owner){if(!streams.Remove(owner))throw new InvalidOperationException("Live stream required");}
  public void Bind(object host)
  {
   Alive();int kind=host is OriginalJurisdictionOptionBit.Host?0:host is OriginalJurisdictionOptionString.Host?1:host is OriginalJurisdictionOption.Host?2:-1;if(kind<0)throw new ArgumentException("Recovered jurisdiction option host required");
   var ports=new Dictionary<string,Delegate>{
    {"ReadWord",(Func<uint,uint>)memory.ReadWord},{"ReadByte",(Func<uint,byte>)memory.ReadByte},{"WriteWord",(Action<uint,uint>)memory.WriteWord},{"WriteByte",(Action<uint,byte>)memory.WriteByte},
    {"ConstructLiteral",(Action<uint,uint,uint>)strings.Construct},{"CopyString",(Action<uint,uint>)strings.Copy},{"DestroyString",(Action<uint>)strings.Destroy},{"AssignString",(Action<uint,uint>)Assign},
    {"DictionaryLookup",(Func<uint,uint,uint,uint>)((a,b,c)=>Lookup(kind,a,b,c))},{"CreateFromFile",(Action<uint,uint>)Create},{"SetName",(Action<uint,uint>)((a,b)=>Item(a).SetName(strings.Read(b)))},
    {"GetHelpText",(Action<uint,uint,uint>)((a,b,c)=>{Machine(b);Put(a,help(strings.Read(c)));})},{"GetConfigInstance",configInstance},{"Register",(Action<uint,uint,uint>)Register},
    {"InvokeOptionDestructor",(Action<uint,uint>)DestroyOption},{"DestroyVector",(Action<uint>)DestroyVector},{"Delete",(Action<uint>)Delete},
    {"ConstructStringStream",(Action<uint,uint>)ConstructStream},{"DestroyStringStream",(Action<uint>)DestroyStream},{"ExtractString",(Action<uint,uint>)Extract},
    {"ReadInteger",(Func<uint,uint,uint>)((a,b)=>{Machine(a);return unchecked((uint)reader.ReadInteger((int)b));})},{"ReadShort",(Func<uint,uint,uint>)((a,b)=>{Machine(a);return unchecked((uint)reader.ReadShort((int)b));})},{"ReadSignedChar",(Func<uint,uint,uint>)((a,b)=>{Machine(a);return reader.ReadByte((int)b);})},{"ReadInteger64",(Func<uint,uint,ulong>)((a,b)=>{Machine(a);return unchecked((ulong)reader.ReadInteger64((int)b));})},
    {"ReadBit",(Func<uint,uint,uint,uint>)((a,b,c)=>{Machine(a);return reader.ReadBit((int)b,(int)c);})},
    {"InsertInteger",(Action<uint,int>)((a,b)=>Insert(a,Encoding.ASCII.GetBytes(b.ToString(CultureInfo.InvariantCulture))))},{"InsertShort",(Action<uint,int>)((a,b)=>Insert(a,Encoding.ASCII.GetBytes(b.ToString(CultureInfo.InvariantCulture))))},{"InsertUnsignedLong",(Action<uint,uint>)((a,b)=>Insert(a,Encoding.ASCII.GetBytes(b.ToString(CultureInfo.InvariantCulture))))},{"InsertInteger64",(Action<uint,long>)((a,b)=>Insert(a,Encoding.ASCII.GetBytes(b.ToString(CultureInfo.InvariantCulture))))},{"InsertBool",(Action<uint,byte>)((a,b)=>Insert(a,Encoding.ASCII.GetBytes(b!=0?"1":"0")))},{"InsertBytes",(Action<uint,uint,uint>)((a,b,c)=>Insert(a,memory.ReadBytes(b,c)))}
   };
   foreach(var field in host.GetType().GetFields()){Delegate port;if(ports.TryGetValue(field.Name,out port))field.SetValue(host,port);}
   // Fault, native EH and the recovered ReadString callback stay caller-bound.
  }
  public void Dispose()
  {
   if(disposed)return;foreach(uint address in new List<uint>(vectors.Keys)){var vector=vectors[address];for(uint i=0;i<vector.Items.Length;++i)if(vector.Items[i]!=null)DestroyOption(memory.ReadWord(optionTable),address+i*132);Delete(address);}streams.Clear();disposed=true;
  }
 }
}
