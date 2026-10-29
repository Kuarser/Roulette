using System;
using System.Text;
namespace RouletteDeluxe
{
    // ConfigCenter dispatch. Frames admit the actual vector owner and extent;
    // mutation, allocation and object virtual methods are required bindings.
    public sealed class OriginalConfigurationCenter
    {
        public sealed class Frame
        {
            public readonly object Owner;
            public readonly uint Count;
            private readonly Func<uint,object> at;
            public Frame(object owner,uint count,Func<uint,object> at)
            {if(owner==null||at==null)throw new ArgumentException("Actual vector frame required");Owner=owner;Count=count;this.at=at;}
            internal object At(uint index){return at(index);}
            internal Frame Shortened(){return new Frame(Owner,unchecked(Count-1),at);}
        }
        public sealed class Host
        {
            public Action LoadConfigOptions,CreateConfigObjects;
            public Func<Frame> ReadVector;
            public Action<object,uint> InvokeVirtual;
            public Action<byte[],byte[],uint,byte[]> Assert;
            public Func<object,byte[],int> CompareName;
            public Func<bool,object> GetSingleton;
            public Action<object> Append;
            public Action<Frame> ShiftFirst,DecrementEnd;
        }
        public sealed class CreationHost
        {
            public Func<uint,object> Allocate;
            public Action<object,uint,uint> WriteWord;
            public Action<object> PublishGlobal;
        }
        private readonly Host host;
        public OriginalConfigurationCenter(Host h)
        {
            if(h==null||h.LoadConfigOptions==null||h.CreateConfigObjects==null||h.ReadVector==null||h.InvokeVirtual==null||h.Assert==null||h.CompareName==null||h.GetSingleton==null||h.Append==null||h.ShiftFirst==null||h.DecrementEnd==null)throw new ArgumentException("Actual configuration load, factory, vector, virtual, assertion, string and singleton bindings required");
            host=new Host{LoadConfigOptions=h.LoadConfigOptions,CreateConfigObjects=h.CreateConfigObjects,ReadVector=h.ReadVector,InvokeVirtual=h.InvokeVirtual,Assert=h.Assert,CompareName=h.CompareName,GetSingleton=h.GetSingleton,Append=h.Append,ShiftFirst=h.ShiftFirst,DecrementEnd=h.DecrementEnd};
        }
        private Frame Read(){return host.ReadVector()??throw new InvalidOperationException("Actual vector frame admission required");}
        private void Call(object owner,uint slot)
        {if(owner==null)throw new InvalidOperationException("Actual nonnull management object admission required");host.InvokeVirtual(owner,slot);}
        private Frame Pass(Frame frame,uint slot)
        {
            if(frame.Count!=0){uint index=0;do{object owner=frame.At(index);index=unchecked(index+1);Call(owner,slot);frame=Read();}while(index<frame.Count);}
            return frame;
        }
        public int Init(){host.LoadConfigOptions();host.CreateConfigObjects();return 1;}
        public int Recover()
        {
            var frame=Read();
            if(frame.Count==0){host.Assert(Encoding.ASCII.GetBytes("ConfigObjs.size() > 0"),Encoding.ASCII.GetBytes("ConfigMgmt/ConfigCenter.cpp"),0x4c,Encoding.ASCII.GetBytes("virtual bally::int32 ConfigCenter::Recover()"));throw new InvalidOperationException("Native nonreturning assertion binding returned");}
            frame=Pass(frame,0);Pass(frame,4);return 1;
        }
        public void SaveConfigOptionsToFile(){Pass(Read(),8);}
        public void RecoverSavedConfigOptions(){Pass(Read(),12);}
        public int End()
        {
            var frame=Read();
            while(frame.Count!=0)
            {
                object owner=frame.At(0);
                if(owner!=null){Call(owner,0x1c);frame=Read();}
                if(frame.Count==0)throw new InvalidOperationException("Native post-delete vector extent requires actual admission");
                if(frame.Count>1){host.ShiftFirst(frame);frame=Read();}
                host.DecrementEnd(frame);frame=frame.Shortened();
            }
            return 1;
        }
        public int Add(object name)
        {
            if(name==null)throw new ArgumentNullException(nameof(name));
            bool bank=host.CompareName(name,Encoding.ASCII.GetBytes("Bank Progressive"))==0;
            if(!bank&&host.CompareName(name,Encoding.ASCII.GetBytes("CoolSign"))!=0)return 1;
            object owner=host.GetSingleton(bank);host.Append(owner);Call(owner,0);Call(owner,4);return 1;
        }
        public static object Create(CreationHost h)
        {
            if(h==null||h.Allocate==null||h.WriteWord==null||h.PublishGlobal==null)throw new ArgumentException("Actual allocation, field-write and global-owner bindings required");
            var allocate=h.Allocate;var write=h.WriteWord;var publish=h.PublishGlobal;
            object owner=allocate(16);
            if(owner==null)throw new InvalidOperationException("Actual configuration center allocation admission required");
            write(owner,0,0x0972bf28);write(owner,4,0);write(owner,8,0);write(owner,12,0);publish(owner);return owner;
        }
    }
}
