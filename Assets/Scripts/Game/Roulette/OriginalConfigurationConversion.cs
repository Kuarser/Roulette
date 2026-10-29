using System;
namespace RouletteDeluxe
{
 // ConvertToSimple 08dfb730 and ConvertToSimpleV 08dfc530. Native return storage,
 // strings, vector headers, insertion helper and unwind bridge must be supplied.
 public sealed class OriginalConfigurationConversion
 {
  public sealed class Host
  {
   public Func<int,uint,object> GetValue,GetCategory,GetName;
   public Action<uint,object> CopyConstructString;
   public Action<object> DestroyTemporaryString,DestroyVector;
   public Action<uint> DestroyField,ReHash,DestroySimple;
   public Func<object,uint,uint> ReadVectorWord;
   public Action<object,uint,uint> WriteVectorWord;
   public Func<int,uint> TemporarySimpleAddress;
   public Action<uint,uint> CopyConstructField;
   public Func<uint,uint> ReadMemoryWord;
   public Action<uint,uint> WriteMemoryWord;
   public Action<object,uint,uint> InsertAux;
   public Func<Exception,bool> IsNativeUnwind;
  }
  private readonly Host host;
  public OriginalConfigurationConversion(Host h)
  {
   if(h==null||h.GetValue==null||h.GetCategory==null||h.GetName==null||h.CopyConstructString==null||h.DestroyTemporaryString==null||h.DestroyVector==null||h.DestroyField==null||h.ReHash==null||h.DestroySimple==null||h.ReadVectorWord==null||h.WriteVectorWord==null||h.TemporarySimpleAddress==null||h.CopyConstructField==null||h.ReadMemoryWord==null||h.WriteMemoryWord==null||h.InsertAux==null||h.IsNativeUnwind==null)throw new ArgumentException("Actual configuration getters/return storage/string/heap/vector/insert-aux/native unwind bindings required");
   host=new Host{GetValue=h.GetValue,GetCategory=h.GetCategory,GetName=h.GetName,CopyConstructString=h.CopyConstructString,DestroyTemporaryString=h.DestroyTemporaryString,DestroyVector=h.DestroyVector,DestroyField=h.DestroyField,ReHash=h.ReHash,DestroySimple=h.DestroySimple,ReadVectorWord=h.ReadVectorWord,WriteVectorWord=h.WriteVectorWord,TemporarySimpleAddress=h.TemporarySimpleAddress,CopyConstructField=h.CopyConstructField,ReadMemoryWord=h.ReadMemoryWord,WriteMemoryWord=h.WriteMemoryWord,InsertAux=h.InsertAux,IsNativeUnwind=h.IsNativeUnwind};
  }
  private static object Known(object o){return o??throw new InvalidOperationException("Actual getter string owner admission required");}
  private void Protected(Action call,Action cleanup){try{call();}catch(Exception ex){if(host.IsNativeUnwind(ex))cleanup();throw;}}
  private T Protected<T>(Func<T> call,Action cleanup){try{return call();}catch(Exception ex){if(host.IsNativeUnwind(ex))cleanup();throw;}}
  public uint ConvertOne(uint destination,uint configuration)
  {
   object value=Known(host.GetValue(0x2c,configuration));
   Action valueCleanup=()=>host.DestroyTemporaryString(value);
   object category=Known(Protected(()=>host.GetCategory(0x28,configuration),valueCleanup));
   Action categoryCleanup=()=>{host.DestroyTemporaryString(category);valueCleanup();};
   object name=Known(Protected(()=>host.GetName(0x24,configuration),categoryCleanup));
   Action temporaryCleanup=()=>{host.DestroyTemporaryString(name);categoryCleanup();};
   Protected(()=>host.CopyConstructString(destination,name),temporaryCleanup);
   Action firstCleanup=()=>{host.DestroyField(destination);temporaryCleanup();};
   Protected(()=>host.CopyConstructString(unchecked(destination+4),value),firstCleanup);
   Action secondCleanup=()=>{host.DestroyField(unchecked(destination+4));firstCleanup();};
   Protected(()=>host.CopyConstructString(unchecked(destination+8),category),secondCleanup);
   Protected(()=>host.ReHash(destination),()=>{host.DestroyField(unchecked(destination+8));secondCleanup();});
   temporaryCleanup();return destination;
  }
  public object ConvertVector(object destination,object configuration)
  {
   if(destination==null||configuration==null)throw new ArgumentException("Actual vector owners required");
   uint sourceEnd=host.ReadVectorWord(configuration,4),source=host.ReadVectorWord(configuration,0);
   host.WriteVectorWord(destination,0,0);host.WriteVectorWord(destination,4,0);host.WriteVectorWord(destination,8,0);
   if(source==sourceEnd)return destination;
   uint iterator=0,temporary=host.TemporarySimpleAddress(0x30);
   do
   {
    Action vectorCleanup=()=>host.DestroyVector(destination);
    Protected(()=>ConvertOne(temporary,source),vectorCleanup);
    uint oldBegin=host.ReadVectorWord(destination,0),end=host.ReadVectorWord(destination,4),capacity=host.ReadVectorWord(destination,8),newBegin;
    Action temporaryCleanup=()=>{host.DestroySimple(temporary);vectorCleanup();};
    if(end==capacity||iterator!=end)
    {Protected(()=>host.InsertAux(destination,iterator,temporary),temporaryCleanup);newBegin=host.ReadVectorWord(destination,0);}
    else
    {
     if(iterator!=0)
     {
      Protected(()=>host.CopyConstructField(iterator,temporary),temporaryCleanup);
      Action firstCleanup=()=>{host.DestroyField(iterator);temporaryCleanup();};
      Protected(()=>host.CopyConstructField(unchecked(iterator+4),unchecked(temporary+4)),firstCleanup);
      Protected(()=>host.CopyConstructField(unchecked(iterator+8),unchecked(temporary+8)),()=>{host.DestroyField(unchecked(iterator+4));firstCleanup();});
      uint hash=host.ReadMemoryWord(unchecked(temporary+12));end=host.ReadVectorWord(destination,4);host.WriteMemoryWord(unchecked(iterator+12),hash);newBegin=host.ReadVectorWord(destination,0);
     }
     else {newBegin=oldBegin;end=0;}
     host.WriteVectorWord(destination,4,unchecked(end+16));
    }
    iterator=unchecked(newBegin+((iterator-oldBegin)&0xfffffff0u)+16);source=unchecked(source+132);
    host.DestroyField(unchecked(temporary+8));host.DestroyField(unchecked(temporary+4));host.DestroyField(temporary);
   }while(source!=sourceEnd);
   return destination;
  }
 }
}
