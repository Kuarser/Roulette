using System;
namespace RouletteDeluxe
{
 // Original __do_global_ctors_aux09692630. Actual table and function owners required.
 public sealed class OriginalConfigurationConstructorTraversal
 {
  private readonly Func<uint,uint> readWord;
  private readonly Action<uint> invoke;
  public OriginalConfigurationConstructorTraversal(Func<uint,uint> readWord,Action<uint> invoke)
  {if(readWord==null||invoke==null)throw new ArgumentException("Actual original constructor table memory and function dispatch required");this.readWord=readWord;this.invoke=invoke;}
  public void Run()
  {
   uint at=0x09b08f40,method=readWord(at);if(method==0xffffffff)return;
   do{at=unchecked(at-4);invoke(method);method=readWord(at);}while(method!=0xffffffff);
  }
 }
}
