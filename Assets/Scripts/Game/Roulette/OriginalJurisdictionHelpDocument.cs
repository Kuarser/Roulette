using System;
using System.Collections.Generic;
using System.IO;
namespace RouletteDeluxe
{
 // Managed successful-file admission of InitHelpText's complete byte parsing rules.
 // Native stream, COW and tree lifetime qualification remains separate.
 public sealed class OriginalJurisdictionHelpDocument
 {
  public sealed class Entry
  {
   public readonly byte[] Key,Value;
   internal Entry(byte[] key,byte[] value){Key=(byte[])key.Clone();Value=(byte[])value.Clone();}
  }
  private sealed class ByteOrder:IComparer<byte[]>
  {
   public int Compare(byte[] a,byte[] b){int n=Math.Min(a.Length,b.Length);for(int i=0;i<n;++i)if(a[i]!=b[i])return a[i]-b[i];return a.Length.CompareTo(b.Length);}
  }
  private readonly SortedDictionary<byte[],byte[]> values=new SortedDictionary<byte[],byte[]>(new ByteOrder());
  private OriginalJurisdictionHelpDocument(){}
  public int Count{get{return values.Count;}}
  public Entry[] Entries{get{var result=new Entry[values.Count];int i=0;foreach(var pair in values)result[i++]=new Entry(pair.Key,pair.Value);return result;}}
  public byte[] GetHelpText(byte[] key)
  {if(key==null)throw new ArgumentNullException(nameof(key));byte[] result;return values.TryGetValue(key,out result)?(byte[])result.Clone():new byte[0];}
  private static bool Letter(byte value){return value>=65&&value<=90||value>=97&&value<=122;}
  private static int Find(byte[] source,int start,byte value){for(int i=start;i<source.Length;++i)if(source[i]==value)return i;return -1;}
  private static byte[] Slice(byte[] source,int start,int length){var result=new byte[length];Array.Copy(source,start,result,0,length);return result;}
  public static OriginalJurisdictionHelpDocument Parse(byte[] input)
  {
   if(input==null)throw new ArgumentNullException(nameof(input));byte[] source=(byte[])input.Clone();var document=new OriginalJurisdictionHelpDocument();int cursor=0;
   while(cursor<source.Length)
   {
    if(!Letter(source[cursor])){++cursor;continue;}
    int line=Find(source,cursor,10),tab=Find(source,cursor,9);if(line<0)line=source.Length-1;
    if(tab>=0&&tab<=line)
    {
     byte[] key=Slice(source,cursor,tab-cursor);
     // Native substring clamps an unsigned requested length to the remaining bytes.
     int count=Math.Max(0,line-tab-1);byte[] value=Slice(source,tab+1,count);
     document.values[key]=value;
    }
    cursor=line+1;
   }
   return document;
  }
  public static OriginalJurisdictionHelpDocument Load(string path)
  {if(path==null)throw new ArgumentNullException(nameof(path));return Parse(File.ReadAllBytes(path));}
 }
}
