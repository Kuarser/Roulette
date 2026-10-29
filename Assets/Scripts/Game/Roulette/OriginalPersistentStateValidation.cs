using System;
using System.Collections.Generic;
using System.IO;
namespace RouletteDeluxe
{
    public static class OriginalPersistentStateValidation
    {
        private sealed class Device:IOriginalNvDevice,IDisposable
        {
            public byte[] Bytes=new byte[0];public int Closed;public bool FailWrite;public readonly List<byte[]> Saves=new List<byte[]>();
            public int Size{get{return Bytes.Length;}}
            public byte[] Read(int o,int n){var b=new byte[n];Array.Copy(Bytes,o,b,0,n);return b;}
            public void Write(int o,byte[] b){if(FailWrite)throw new OriginalNvFault("Injected State NV write fault");Array.Copy(b,0,Bytes,o,b.Length);if(o==0&&b.Length==1&&b[0]==1)Saves.Add(Read(20,26));}
            public void SetSize(int n){Array.Resize(ref Bytes,n);}
            public void Dispose(){++Closed;}
        }
        private static bool Equal(byte[] a,byte[] b){if(a.Length!=b.Length)return false;for(int i=0;i<a.Length;++i)if(a[i]!=b[i])return false;return true;}
        public static int Validate(string freshDirectory)
        {
            int checks=0;Action<bool> check=b=>{if(!b)throw new Exception("Persistent State regression "+checks);++checks;};
            int cases=0;var values=new List<int>{int.MinValue,-65536,-257,-256,-255,-1};for(int i=0;i<256;++i)values.Add(i);values.AddRange(new[]{256,257,int.MaxValue});
            for(int depth=0;depth<25;++depth)foreach(int value in values)
            {
                var d=new Device();var root=new OriginalPersistentState();root.Open(()=>d,true,message=>{},false,null);OriginalPersistentState current=root;
                for(int i=0;i<depth;++i){var next=current.NewSubState();if(!ReferenceEquals(current,root))current.Dispose();current=next;}
                byte[] expected=root.PayloadBytes;d.Saves.Clear();int old=expected[depth+1];
                if(value>=256)
                {bool failed=false;try{current.Assign(value);}catch(OriginalNvFault e){failed=true;check(e.NativeCode==2&&e.Message=="State::=(), stack overflow");}check(failed&&d.Saves.Count==0&&Equal(expected,root.PayloadBytes));}
                else
                {
                    check(current.Assign(value)==value);expected[depth+1]=unchecked((byte)value);expected[0]=(byte)(depth+1);
                    check(Equal(expected,root.PayloadBytes)&&current.Value==(byte)value&&current.Cursor==depth);
                    check(d.Saves.Count==(old==value?0:1));if(d.Saves.Count>0)check(Equal(expected,d.Saves[0]));
                    int saves=d.Saves.Count;current.Assign(value);check(d.Saves.Count==saves+(value<0?1:0));
                    root.ValidateStorage();root.Reload();check(Equal(expected,root.PayloadBytes));
                }
                if(!ReferenceEquals(current,root))current.Dispose();root.Dispose();check(d.Closed==1);
                using(var cold=new OriginalPersistentState()){cold.Open(()=>d,false,message=>{},false,null);check(Equal(expected,cold.PayloadBytes)&&cold.Cursor==0&&cold.Slot==0);}++cases;
            }
            // Copies alias one NV owner; truncation invalidates deeper handles but
            // leaves their inactive payload bytes intact. Cursor is owner-wide.
            var sharedDevice=new Device();var state=new OriginalPersistentState();int softCalls=0;
            state.Open(()=>sharedDevice,true,message=>{},true,text=>{check(text=="");++softCalls;});
            var copy=state.Copy();check(state.ReferenceCount==2);state.Assign(4);check(copy.Value==4&&softCalls==1);
            sharedDevice.Saves.Clear();copy.Assign(4);check(sharedDevice.Saves.Count==0&&softCalls==1);
            var child=copy.NewSubState();child.Assign(71);var grandchild=state.NewSubState();grandchild.Assign(99);check(state.ReferenceCount==4&&grandchild.Slot==2);
            byte[] dormant=state.PayloadBytes;state.Assign(5);check(state.PayloadBytes[2]==71&&state.PayloadBytes[3]==99&&state.PayloadBytes[0]==1);
            bool stale=false;try{int n=child.Value;}catch(OriginalNvFault e){stale=e.Message=="State::int(), stack overflow";}check(stale);
            var reused=state.NewSubState();check(reused.Value==0&&reused.Slot==1&&state.Cursor==1);reused.Assign(2);state.DeleteSubState();check(state.Cursor==0&&state.PayloadBytes[0]==1&&state.PayloadBytes[2]==2);
            state.DeleteSubState();check(state.Cursor==0);reused.Dispose();grandchild.Dispose();child.Dispose();state.Dispose();check(sharedDevice.Closed==0&&copy.Value==5&&copy.ReferenceCount==1);copy.Dispose();copy.Dispose();check(sharedDevice.Closed==1);
            var overflowDevice=new Device();using(var top=new OriginalPersistentState())
            {
                top.Open(()=>overflowDevice,false,message=>{},false,null);var handles=new List<OriginalPersistentState>();for(int i=0;i<24;++i)handles.Add(top.NewSubState());
                overflowDevice.Saves.Clear();bool overflow=false;try{top.NewSubState();}catch(OriginalNvFault e){overflow=e.Message=="State::NewSubState, stack overflow";}
                check(overflow&&top.PayloadBytes[0]==26&&top.Cursor==25&&overflowDevice.Saves.Count==0&&overflowDevice.Bytes[20]==25);
                foreach(var h in handles)h.Dispose();
            }
            using(var restored=new OriginalPersistentState()){restored.Open(()=>overflowDevice,false,message=>{},false,null);check(restored.PayloadBytes[0]==25&&restored.Cursor==0);}
            var faultDevice=new Device();using(var top=new OriginalPersistentState())
            {
                top.Open(()=>faultDevice,false,message=>{},false,null);faultDevice.FailWrite=true;bool failed=false;try{top.Assign(6);}catch(OriginalNvFault){failed=true;}check(failed&&top.Value==6);
                faultDevice.FailWrite=false;top.Reload();check(top.Value==0);
            }
            var first=new Device();var second=new Device();using(var left=new OriginalPersistentState())using(var right=new OriginalPersistentState())
            {left.Open(()=>first,true,message=>{},false,null);right.Open(()=>second,true,message=>{},false,null);right.Assign(3);left.AssignReference(right);check(first.Closed==1&&right.ReferenceCount==2&&left.Value==3);}
            check(second.Closed==1);
            if(Directory.Exists(freshDirectory))throw new Exception("State file regression directory must be fresh");Directory.CreateDirectory(freshDirectory);
            string path=Path.Combine(freshDirectory,"creditout-state.bin");OriginalPersistentState alias;
            using(var file=OriginalPersistentState.OpenFile(path,message=>{},false,null)){file.Assign(4);alias=file.Copy();using(var sub=file.NewSubState()){sub.Assign(2);}file.ValidateStorage();}
            check(alias.Value==4);alias.Assign(5);alias.Dispose();
            using(var file=OriginalPersistentState.OpenFile(path,message=>{},false,null)){check(file.Value==5&&file.PayloadBytes[0]==1&&file.PayloadBytes[2]==2);file.ValidateStorage();}
            UnityEngine.Debug.Log("PERSISTENT_STATE_VALIDATED cases="+cases+" checks="+checks);return checks;
        }
    }
}
