using System;
using System.IO;
using System.Threading;
namespace RouletteDeluxe.SasDraftV1
{
 // PC checkpoint storage only. Full-round FrozenInputs validation belongs to admission.
 public sealed class SasIntentCheckpointFileStoreV1
 {
  readonly string path,scope,round,spin;
  readonly int owner=Thread.CurrentThread.ManagedThreadId;
  public bool Faulted {get;private set;}
  public SasIntentCheckpointFileStoreV1(string filePath,string expectedScope,string roundId,string spinId)
  {
   if(string.IsNullOrWhiteSpace(filePath)||string.IsNullOrWhiteSpace(expectedScope)||string.IsNullOrWhiteSpace(roundId)||string.IsNullOrWhiteSpace(spinId)||roundId==spinId)throw new ArgumentException("Intent storage identity required");
   path=Path.GetFullPath(filePath);scope=expectedScope;round=roundId;spin=spinId;
   if(!Directory.Exists(Path.GetDirectoryName(path)))throw new DirectoryNotFoundException("Intent directory must be explicitly provisioned");
  }
  void Healthy(){if(Thread.CurrentThread.ManagedThreadId!=owner)throw new InvalidOperationException("Intent storage owner mismatch");if(Faulted)throw new InvalidOperationException("Intent storage faulted; cold recovery required");}
  FileStream Lease(){return new FileStream(path+".lock",FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None);}
  SasIntentCheckpointV1 Read(){byte[] bytes;try{bytes=File.ReadAllBytes(path);}catch(FileNotFoundException){return null;}return SasIntentCheckpointCodecV1.Decode(bytes,scope,round,spin);}
  public SasIntentCheckpointV1 Load(){Healthy();try{using(var lease=Lease())return Read();}catch{Faulted=true;throw;}}
  public SasIntentCheckpointV1 Save(long expectedRevision,SasIntentCheckpointV1 candidate)
  {
   Healthy();string temporary=null;
   try
   {
    if(candidate==null||expectedRevision< -1||candidate.Scope!=scope||candidate.RoundId!=round||candidate.SpinId!=spin||candidate.Revision!=checked(expectedRevision+1))throw new ArgumentException("Intent candidate identity or revision differs");
    using(var lease=Lease())
    {
     var current=Read();if((current==null?-1:current.Revision)!=expectedRevision)throw new InvalidOperationException("Durable intent revision changed");
     if(current==null){if(candidate.Stage!=SasRoundIntentStageV1.EvaluatedSaved)throw new InvalidOperationException("Initial intent requires evaluated stage");}
     else SasIntentCheckpointCommitterV1.RequireSuccessor(current,candidate);
     byte[] bytes=SasIntentCheckpointCodecV1.Encode(candidate);temporary=path+".candidate."+Guid.NewGuid().ToString("N");
     using(var file=new FileStream(temporary,FileMode.CreateNew,FileAccess.Write,FileShare.None)){file.Write(bytes,0,bytes.Length);file.Flush(true);}
     if(current==null)File.Move(temporary,path);else File.Replace(temporary,path,null);temporary=null;
     byte[] saved=File.ReadAllBytes(path);if(saved.Length!=bytes.Length)throw new IOException("Intent readback length differs");for(int i=0;i<bytes.Length;i++)if(saved[i]!=bytes[i])throw new IOException("Intent readback differs");
     return SasIntentCheckpointCodecV1.Decode(saved,scope,round,spin);
    }
   }
   catch{Faulted=true;throw;}
   finally{if(temporary!=null){try{File.Delete(temporary);}catch(IOException){}catch(UnauthorizedAccessException){}}}
  }
  // The archive owner must independently prove this exact ReadyCommit was
  // consumed before asking for deletion. A different or corrupt journal stays.
  public void DeleteVerifiedReady(SasIntentCheckpointV1 expected)
  {
   Healthy();try
   {
    if(expected==null||expected.Stage!=SasRoundIntentStageV1.ReadyCommit||expected.Scope!=scope||expected.RoundId!=round||expected.SpinId!=spin)
     throw new InvalidOperationException("Intent cleanup lacks matching ReadyCommit");
    using(var lease=Lease())
    {
     var saved=Read();if(saved==null)throw new InvalidOperationException("Intent disappeared before verified cleanup");
     byte[] a=SasIntentCheckpointCodecV1.Encode(saved),b=SasIntentCheckpointCodecV1.Encode(expected);
     if(a.Length!=b.Length)throw new InvalidOperationException("Intent changed before verified cleanup");
     for(int i=0;i<a.Length;i++)if(a[i]!=b[i])throw new InvalidOperationException("Intent changed before verified cleanup");
     File.Delete(path);if(File.Exists(path))throw new IOException("Intent cleanup readback differs");
    }
   }
   catch{Faulted=true;throw;}
  }
 }
}
