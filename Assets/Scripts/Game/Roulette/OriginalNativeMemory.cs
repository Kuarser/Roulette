using System;
using System.Collections.Generic;
namespace RouletteDeluxe
{
 // Managed owner for the recovered 32-bit little-endian object layouts.
 // All typed views use the same bytes. Addresses are virtual original-layout addresses.
 public sealed class OriginalNativeMemory
 {
  private sealed class Region{internal uint Address;internal byte[] Data;internal bool Heap;}
  private readonly Dictionary<uint,Region> regions=new Dictionary<uint,Region>();
  private readonly uint heapBase,heapSize;private ulong heapUsed;
  public int Allocations{get;private set;}public int Frees{get;private set;}public int AtomicCalls{get;private set;}
  public int LiveAllocations{get{return Allocations-Frees;}}
  public OriginalNativeMemory(uint allocationBase,uint allocationSize)
  {if(allocationSize==0)throw new ArgumentException("Nonempty virtual allocation arena required");heapBase=allocationBase;heapSize=allocationSize;}
  private static uint A(uint a,uint b){return unchecked(a+b);}
  private Region Find(uint address){foreach(var r in regions.Values)if(unchecked(address-r.Address)<r.Data.Length)return r;throw new InvalidOperationException("Unmapped or released original-layout memory at "+address.ToString("x8"));}
  private void Add(uint address,byte[] data,bool heap)
  {
   if(data==null||data.Length==0)throw new ArgumentException("Nonempty memory region required");
   foreach(var r in regions.Values)if(unchecked(address-r.Address)<r.Data.Length||unchecked(r.Address-address)<data.Length)throw new ArgumentException("Overlapping original-layout memory region");
   regions.Add(address,new Region{Address=address,Data=data,Heap=heap});
  }
  public void Map(uint address,byte[] data){MapOwned(address,data);}
  // The returned alias lets input readers share this owner's bytes without a second store.
  public byte[] MapOwned(uint address,byte[] data){if(data==null)throw new ArgumentException("Memory region required");var owned=(byte[])data.Clone();Add(address,owned,false);return owned;}
  public uint Allocate(uint size)
  {
   if(size==0||size>int.MaxValue)throw new ArgumentException("Representable nonempty allocation required");ulong aligned=(heapUsed+15)&~15UL;if(aligned+size>heapSize)throw new OutOfMemoryException("Original-layout allocation arena exhausted");uint address=A(heapBase,(uint)aligned);Add(address,new byte[(int)size],true);heapUsed=aligned+size;++Allocations;return address;
  }
  public void Free(uint address){Region r;if(!regions.TryGetValue(address,out r)||!r.Heap)throw new InvalidOperationException("Release requires a live owned allocation base");regions.Remove(address);++Frees;}
  public byte ReadByte(uint address){var r=Find(address);return r.Data[unchecked(address-r.Address)];}
  public void WriteByte(uint address,byte value){var r=Find(address);r.Data[unchecked(address-r.Address)]=value;}
  public ushort ReadHalf(uint address){return (ushort)(ReadByte(address)|(ReadByte(A(address,1))<<8));}
  public uint ReadWord(uint address){return ReadByte(address)|((uint)ReadByte(A(address,1))<<8)|((uint)ReadByte(A(address,2))<<16)|((uint)ReadByte(A(address,3))<<24);}
  public ulong ReadWide(uint address){return ReadWord(address)|((ulong)ReadWord(A(address,4))<<32);}
  public double ReadDouble(uint address){return BitConverter.Int64BitsToDouble(unchecked((long)ReadWide(address)));}
  public void WriteHalf(uint address,ushort value){WriteByte(address,(byte)value);WriteByte(A(address,1),(byte)(value>>8));}
  public void WriteWord(uint address,uint value){for(uint i=0;i<4;++i)WriteByte(A(address,i),(byte)(value>>(int)(i*8)));}
  public void WriteWide(uint address,ulong value){WriteWord(address,(uint)value);WriteWord(A(address,4),(uint)(value>>32));}
  public void WriteDouble(uint address,double value){WriteWide(address,unchecked((ulong)BitConverter.DoubleToInt64Bits(value)));}
  public byte[] ReadBytes(uint address,uint size){if(size>int.MaxValue)throw new ArgumentException("Representable memory span required");var result=new byte[(int)size];for(uint i=0;i<size;++i)result[i]=ReadByte(A(address,i));return result;}
  public void WriteBytes(uint address,byte[] data){if(data==null)throw new ArgumentException("Source bytes required");for(uint i=0;i<data.Length;++i)WriteByte(A(address,i),data[i]);}
  public void CopyBytes(uint destination,uint source,uint size){for(uint i=0;i<size;++i)WriteByte(A(destination,i),ReadByte(A(source,i)));}
  public void MoveBytes(uint destination,uint source,uint size){WriteBytes(destination,ReadBytes(source,size));}
  public void SetBytes(uint destination,int value,uint size){for(uint i=0;i<size;++i)WriteByte(A(destination,i),unchecked((byte)value));}
  public uint StrLen(uint address){uint size=0;while(ReadByte(A(address,size))!=0){if(size==uint.MaxValue)throw new InvalidOperationException("CString spans the entire address space");++size;}return size;}
  public int AtomicAdd(uint address,int delta){lock(this){uint old=ReadWord(address);WriteWord(address,unchecked(old+(uint)delta));++AtomicCalls;return unchecked((int)old);}}
 }
}
