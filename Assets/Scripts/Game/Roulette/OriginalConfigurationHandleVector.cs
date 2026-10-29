using System;
using System.Collections.Generic;
namespace RouletteDeluxe
{
    // InsertOptionInHandles normal path with reserved spare capacity.
    // Caller supplies the persistent process cache owner; handles are not copied.
    public sealed class OriginalConfigurationHandleVector
    {
        private sealed class BytesComparer:IComparer<byte[]>
        {public int Compare(byte[] a,byte[] b){for(int i=0;i<Math.Min(a.Length,b.Length);++i)if(a[i]!=b[i])return a[i]<b[i]?-1:1;return a.Length.CompareTo(b.Length);}}
        public sealed class CacheEntry{public byte[] Category;public int Position;}
        public sealed class State
        {
            internal readonly SortedDictionary<byte[],int> Cache=new SortedDictionary<byte[],int>(new BytesComparer());
            public int Position{get;internal set;}
            public CacheEntry[] Snapshot(){var result=new CacheEntry[Cache.Count];int at=0;foreach(var pair in Cache)result[at++]=new CacheEntry{Category=(byte[])pair.Key.Clone(),Position=pair.Value};return result;}
        }
        private readonly State state;private readonly List<OriginalConfigurationOptionHandle> handles=new List<OriginalConfigurationOptionHandle>();
        public OriginalConfigurationHandleVector(State state){this.state=state??throw new ArgumentNullException(nameof(state));}
        public int Count{get{return handles.Count;}}public int Capacity{get{return handles.Capacity;}}
        public void Reserve(int requested){if(requested<0)throw new ArgumentOutOfRangeException(nameof(requested));if(requested>handles.Capacity)handles.Capacity=requested;}
        public OriginalConfigurationOptionHandle[] Snapshot(){return handles.ToArray();}
        private static bool Eq(byte[] a,byte[] b){if(a.Length!=b.Length)return false;for(int i=0;i<a.Length;++i)if(a[i]!=b[i])return false;return true;}
        private static int Slash(byte[] raw,int start){for(int i=start;i<raw.Length;++i)if(raw[i]==47)return i;return -1;}
        private static byte[] Prefix(byte[] raw,int end){if(end<0)end=raw.Length;var result=new byte[end];Array.Copy(raw,result,end);return result;}
        private static int Find(byte[] raw,byte[] needle)
        {for(int i=0;i<=raw.Length-needle.Length;++i){int j=0;while(j<needle.Length&&raw[i+j]==needle[j])++j;if(j==needle.Length)return i;}return -1;}
        public OriginalConfigurationOptionHandle Insert(OriginalConfigurationOptionHandle handle)
        {
            if(handle==null)throw new ArgumentNullException(nameof(handle));
            if(handles.Count==handles.Capacity)throw new InvalidOperationException("Native reserved spare-capacity path required");
            var category=handle.Primary.Category;int delimiter=Slash(category,1);var root=Prefix(category,delimiter);int cached;
            if(!state.Cache.TryGetValue(root,out cached)){state.Cache.Add(root,0);cached=0;}
            state.Position=cached;uint control=handle.Primary.Control;if(control==10)state.Position=0;
            int at=state.Position;
            if(at<0||at>handles.Count)throw new InvalidOperationException("Native cached iterator outside admitted vector");
            bool matched=false;byte[] selected=new byte[0],common=new byte[0];
            while(at<handles.Count)
            {
                byte[] newPrefix=root;var existing=handles[at].Primary.Category;int existingDelimiter=Slash(existing,1);var existingRoot=Prefix(existing,existingDelimiter);var existingPrefix=existingRoot;
                if(Eq(root,existingRoot))
                {
                    if((Find(existing,category)>=0&&Find(category,existing)<0&&control==10)||(Eq(category,existing)&&control==10&&handles[at].Primary.Control!=10))break;
                    if(delimiter>=0&&existingDelimiter>=0)
                    {
                        int newAt=delimiter,oldAt=existingDelimiter;
                        while(Eq(newPrefix,existingPrefix))
                        {
                            common=newPrefix;newAt=Slash(category,newAt+1);newPrefix=Prefix(category,newAt);
                            oldAt=Slash(existing,oldAt+1);
                            // Original branch tests newAt when choosing the existing full/substr path.
                            existingPrefix=Prefix(existing,newAt<0?-1:oldAt);
                            if(!Eq(newPrefix,existingPrefix))break;
                            common=newPrefix;if(newAt<0||oldAt<0)break;
                        }
                        if(matched)
                        {if(Find(common,selected)>=0&&selected.Length<common.Length)selected=common;else if(!Eq(selected,common))break;}
                        else{selected=common;matched=true;}
                    }
                }
                else if(matched)break;
                ++at;
            }
            handles.Insert(at,handle);state.Cache[root]=at;return handle;
        }
    }
}
