using System;
namespace RouletteDeluxe
{
 // Compose query -> native reply parser -> complete fill_insert. All remaining
 // owners, native addresses, helpers and transport are still supplied bindings.
 public sealed class OriginalConfigurationQueryBinding
 {
  private readonly OriginalConfigurationHostGet query;
  public OriginalConfigurationQueryBinding(OriginalConfigurationHostGet.Host q,OriginalConfigurationReplyParser.Host p,OriginalConfigurationNativeDefault defaults,OriginalConfigurationFillInsert fill,OriginalConfigurationNativeCopy copy,Func<object,uint> responseAddress,Func<object,uint> replyStringAddress)
  {
   if(q==null||p==null||defaults==null||fill==null||copy==null||responseAddress==null||replyStringAddress==null)throw new ArgumentException("Actual query/parser/default/fill/copy owners and response/string address resolvers required");
   var parser=new OriginalConfigurationReplyParser(new OriginalConfigurationReplyParser.Host{
    FrameAddress=p.FrameAddress,CreateCString=p.CreateCString,Tokenize=p.Tokenize,InitializeEmptyString=p.InitializeEmptyString,WriteWord=p.WriteWord,CopyString=p.CopyString,EraseAllowedTree=p.EraseAllowedTree,EraseOrderTree=p.EraseOrderTree,InvokeConfigurationDestructor=p.InvokeConfigurationDestructor,ReadWord=p.ReadWord,StringLength=p.StringLength,ParseData=p.ParseData,Incorporate=p.Incorporate,FillInsert=fill.Insert,DestroyString=p.DestroyString,DestroyStringVector=p.DestroyStringVector,DestroyConfiguration=p.DestroyConfiguration,Free=p.Free,ReadRepresentation=p.ReadRepresentation,AtomicDecrement=p.AtomicDecrement,DestroyRepresentation=p.DestroyRepresentation,IsNativeUnwind=p.IsNativeUnwind
   },defaults);
   Action<object,object> build=(response,text)=>{
    uint builder=responseAddress(response),slot=replyStringAddress(text);
    if(builder==0||slot==0)throw new InvalidOperationException("Actual query response builder/reply string address admission required");
    parser.Build(builder,slot);
   };
   query=new OriginalConfigurationHostGet(new OriginalConfigurationHostGet.Host{
    CreateBuilder=q.CreateBuilder,ResetBuilderVtable=q.ResetBuilderVtable,InitializeRequestString=q.InitializeRequestString,AssignString=q.AssignString,CopyString=q.CopyString,CopyCString=q.CopyCString,StringLength=q.StringLength,StringData=q.StringData,AllocateArray=q.AllocateArray,ReadArrayWord=q.ReadArrayWord,ReadBuilderWord=q.ReadBuilderWord,ReadOutputWord=q.ReadOutputWord,WriteArrayWord=q.WriteArrayWord,WriteBuilderWord=q.WriteBuilderWord,WriteOutputWord=q.WriteOutputWord,ReadArrayByte=q.ReadArrayByte,CopyBytes=q.CopyBytes,SendRequest=q.SendRequest,BuildFromMsg=build,DeleteArray=q.DeleteArray,DestroyString=q.DestroyString,DestroyResponseBuilder=q.DestroyResponseBuilder,ReadRepresentation=q.ReadRepresentation,ThreadActive=q.ThreadActive,AtomicDecrement=q.AtomicDecrement,Decrement=q.Decrement,DestroyRepresentation=q.DestroyRepresentation,Allocate=q.Allocate,Free=q.Free,ThrowBadAlloc=q.ThrowBadAlloc,EndCatch=q.EndCatch,Rethrow=q.Rethrow,Fault=q.Fault,BeginCatch=q.BeginCatch,ResetConfigurationVtable=q.ResetConfigurationVtable,InvokeConfigurationDestructor=q.InvokeConfigurationDestructor,IsNativeUnwind=q.IsNativeUnwind
   },copy);
  }
  public object GetConfiguration(object output,object client,object category){return query.GetConfiguration(output,client,category);}
 }
}
