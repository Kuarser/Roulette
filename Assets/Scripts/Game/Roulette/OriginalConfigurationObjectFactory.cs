using System;
namespace RouletteDeluxe
{
    // CreateConfigObjs at 08e7fd60. Provider constructors, heap, stdstrings,
    // vector append and captured virtual queries require actual caller bindings.
    public sealed class OriginalConfigurationObjectFactory
    {
        public sealed class Provider
        {
            public readonly int Index;
            public readonly string Name,Symbol;
            public readonly uint CallVa,CalleeVa,AllocationSize,PrimaryVtable;
            public readonly bool Singleton;
            internal Provider(int index,string name,string symbol,uint callVa,uint calleeVa,uint size,bool singleton,uint vtable)
            {Index=index;Name=name;Symbol=symbol;CallVa=callVa;CalleeVa=calleeVa;AllocationSize=size;Singleton=singleton;PrimaryVtable=vtable;}
        }
        private static readonly Provider[] providers=new Provider[]{
            new Provider(0,"DeviceConfig","_ZN12DeviceConfigC2Ev",0x08e7fd82u,0x08e8be30u,220u,false,0x0972c488u),
            new Provider(1,"PrtOptionConfig","_ZN15PrtOptionConfigC1Ev",0x08e7fdb9u,0x08e91920u,220u,false,0x0972c688u),
            new Provider(2,"ComboConfig","_ZN11ComboConfigC2Ev",0x08e7fdf0u,0x08e22280u,664u,false,0x0972afe8u),
            new Provider(3,"LegacyGameConfig","_ZN5bally16LegacyGameConfig11GetInstanceEv",0x08e7fe16u,0x08587c60u,0u,true,0x096ca7e8u),
            new Provider(4,"LimitConfig","_ZN11LimitConfigC2Ev",0x08e7fe4du,0x08ea9040u,228u,false,0x0972d728u),
            new Provider(5,"CashOutConfig","_ZN13CashOutConfigC2Ev",0x08e7fe84u,0x08eb4400u,220u,false,0x0972dc48u),
            new Provider(6,"VersionConfig","_ZN13VersionConfigC1Ev",0x08e7febbu,0x08eb2630u,224u,false,0x0972d968u),
            new Provider(7,"SoundConfig","_ZN11SoundConfigC1Ev",0x08e7fef2u,0x08ec47b0u,220u,false,0x0972dea8u),
            new Provider(8,"IPConfig","_ZN8IPConfig11GetInstanceEv",0x08e7ff18u,0x08ec9250u,0u,true,0x0972e048u),
            new Provider(9,"SerialCommConfig","_ZN16SerialCommConfigC2Ev",0x08e7ff4fu,0x08ec0e40u,220u,false,0x0972dda8u),
            new Provider(10,"TournamentConfig","_ZN16TournamentConfigC1Ev",0x08e7ffedu,0x08f0ffc0u,220u,false,0x09731468u),
            new Provider(11,"AddProgCtrlConfig","_ZN17AddProgCtrlConfigC1Ev",0x08e80024u,0x089f2b90u,220u,false,0x096fe748u),
            new Provider(12,"ProgComboConfig","_ZN16ProgComboManager15ProgComboConfigC2Ev",0x08e8005bu,0x089fb670u,532u,false,0x096febe8u),
            new Provider(13,"ProgMainConfig","_ZN14ProgMainConfigC1Ev",0x08e80092u,0x08f19cd0u,244u,false,0x09731808u),
            new Provider(14,"ClockConfig","_ZN11ClockConfigC1Ev",0x08e800c9u,0x08f35290u,220u,false,0x09731ca8u),
            new Provider(15,"ServiceConfig","_ZN13ServiceConfigC1Ev",0x08e80100u,0x08f37590u,220u,false,0x09731e08u),
            new Provider(16,"DHCPExtensionConfig","_ZN19DHCPExtensionConfig11GetInstanceEv",0x08e80126u,0x08f3e690u,0u,true,0x09731ec8u),
            new Provider(17,"SetupCompleteConfig","_ZN19SetupCompleteConfigC1Ev",0x08e8015du,0x08f40a10u,220u,false,0x097322a8u),
            new Provider(18,"GameConfig","_ZN10GameConfigC1Ev",0x08e80194u,0x08ed0410u,220u,false,0x0972e0c8u),
            new Provider(19,"RcgConfig","_ZN9RcgConfigC2Ev",0x08e801cbu,0x08ed16d0u,220u,false,0x0972e408u),
            new Provider(20,"SasConfig","_ZN9SasConfigC2Ev",0x08e80274u,0x08efea80u,268u,false,0x0972f9c8u),
            new Provider(21,"DeviceOwnerConfig","_ZN17DeviceOwnerConfigC2Ev",0x08e802abu,0x08e9b460u,256u,false,0x0972ca28u),
            new Provider(22,"MachineDataConfig","_ZN17MachineDataConfigC1Ev",0x08e802e2u,0x08ef3680u,236u,false,0x0972ef48u),
            new Provider(23,"VoucherDataConfig","_ZN17VoucherDataConfigC2Ev",0x08e80319u,0x08ef5590u,236u,false,0x0972f2a8u),
            new Provider(24,"HardMeterConfig","_ZN15HardMeterConfigC1Ev",0x08e80353u,0x08e93030u,220u,false,0x0972c808u),
            new Provider(25,"AFTConfig","_ZN9AFTConfigC1Ev",0x08e8038du,0x08f0c1c0u,268u,false,0x09731148u),
            new Provider(26,"BNGConfig","_ZN5bally3BNG6ConfigC1Ev",0x08e803c7u,0x093bfaa0u,220u,false,0x097583e8u),
            new Provider(27,"GameAvailableOptionsConfig","_ZN26GameAvailableOptionsConfigC2Ev",0x08e80401u,0x08f3eb40u,232u,false,0x09731fc8u),
            new Provider(28,"GameConfigInfo","_ZN14GameConfigInfoC2Ev",0x08e8043bu,0x08f3f6c0u,220u,false,0x097320e8u),
            new Provider(29,"SBDServerConfig","_ZN15SBDServerConfigC1Ev",0x08e80475u,0x08f98320u,220u,false,0x09733928u),
            new Provider(30,"SBDClientConfig","_ZN15SBDClientConfigC2Ev",0x08e804afu,0x08f9a040u,220u,false,0x09733a68u),
            new Provider(31,"SchedulerConfig","_ZN15SchedulerConfig11GetInstanceEv",0x08e804d8u,0x08f8d000u,0u,true,0x09733768u),
            new Provider(32,"MachinePowerSaveConfig","_ZN22MachinePowerSaveConfigC1Ev",0x08e80512u,0x08ef4e20u,236u,false,0x0972f0e8u),
            new Provider(33,"ShowPinConfig","_ZN13ShowPinConfigC1Ev",0x08e80560u,0x08f9ac90u,232u,false,0x09733ba8u)
        };
        public static Provider[] Providers{get{return (Provider[])providers.Clone();}}
        public sealed class Host
        {
            public Func<Provider,object> Allocate,GetSingleton;
            public Action<Provider,object> Construct;
            public Action<object> FreeAllocation,Append;
            public Func<Exception,bool> IsNativeUnwind;
            public Func<object> ReadSystemConfiguration;
            public Func<object,uint,Func<object,object,object,uint>> CaptureSystemQuery;
            public Func<uint,object> ConstructCString,CopyString;
            public Action<object> DestroyString;
            public Func<uint,int> QueryGameManager;
        }
        private readonly Host host;
        public OriginalConfigurationObjectFactory(Host h)
        {
            if(h==null||h.Allocate==null||h.GetSingleton==null||h.Construct==null||h.FreeAllocation==null||h.Append==null||h.IsNativeUnwind==null||h.ReadSystemConfiguration==null||h.CaptureSystemQuery==null||h.ConstructCString==null||h.CopyString==null||h.DestroyString==null||h.QueryGameManager==null)throw new ArgumentException("Actual provider, allocator, vector, exception, system/game query and stdstring bindings required");
            host=new Host{Allocate=h.Allocate,GetSingleton=h.GetSingleton,Construct=h.Construct,FreeAllocation=h.FreeAllocation,Append=h.Append,IsNativeUnwind=h.IsNativeUnwind,ReadSystemConfiguration=h.ReadSystemConfiguration,CaptureSystemQuery=h.CaptureSystemQuery,ConstructCString=h.ConstructCString,CopyString=h.CopyString,DestroyString=h.DestroyString,QueryGameManager=h.QueryGameManager};
        }
        private static object Known(object owner)
        {return owner??throw new InvalidOperationException("Actual native owner admission required");}
        private bool Service(bool tournament)
        {
            var method=host.CaptureSystemQuery(Known(host.ReadSystemConfiguration()),4);
            if(method==null)throw new InvalidOperationException("Actual system query method admission required");
            object empty=Known(host.ConstructCString(0x096c5b87));
            object service;
            try{service=tournament?host.CopyString(0x0a122b60):host.ConstructCString(0x096a4e5b);}
            catch(Exception ex){if(host.IsNativeUnwind(ex))host.DestroyString(empty);throw;}
            Known(service);
            object owner=Known(host.ReadSystemConfiguration());uint result;
            try{result=method(owner,service,empty);}
            catch(Exception ex){if(host.IsNativeUnwind(ex)){host.DestroyString(service);host.DestroyString(empty);}throw;}
            host.DestroyString(service);host.DestroyString(empty);return (byte)result!=0;
        }
        public void Create()
        {
            foreach(var p in providers)
            {
                if(p.Index==10&&!Service(true))continue;
                if(p.Index==20&&!Service(false))continue;
                if(p.Index==33&&host.QueryGameManager(0xe0)<=0)continue;
                object owner;
                if(p.Singleton)owner=host.GetSingleton(p);
                else
                {
                    owner=Known(host.Allocate(p));
                    try{host.Construct(p,owner);}
                    catch(Exception ex){if(host.IsNativeUnwind(ex))host.FreeAllocation(owner);throw;}
                }
                host.Append(owner);
            }
        }
    }
}
