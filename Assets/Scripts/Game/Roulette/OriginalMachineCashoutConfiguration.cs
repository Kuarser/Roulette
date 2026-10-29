using System;
namespace RouletteDeluxe
{
    // MachineData's IMachineData subobject is +4, vptr096b1320. The caller supplies
    // the configured557-byte MachineDataNvram area (whole object+410..+63c).
    // Configuration boot/parsing and other MachineData NV areas remain caller owned.
    public sealed class OriginalMachineCashoutConfiguration : IDisposable
    {
        public const int PayloadLength=0x22d;
        public sealed class Host
        {
            public Func<int,bool> JurisdictionFlag;
            public Func<int,int> ReadPrimaryInt; // Whole-object virtual slots2e4/310, fresh reads.
            public Func<long> ReadAftMaxTransferLimitFromGame; // Whole object+38c, outside this NV area.
            public Func<uint> ReadTransactionEventField; // Whole object+3c4, fresh after Save.
            public Action<byte[],uint> Emit;
        }
        private byte[] payload;
        private readonly Host host;
        private readonly OriginalNv2Storage storage;
        private OriginalNvFileDevice ownedFile;
        public OriginalMachineCashoutConfiguration(IOriginalNvDevice device,byte[] initialPayload,Host operations,Action<string> logRecovery)
        {
            Admit(initialPayload);
            if(operations==null||operations.JurisdictionFlag==null||operations.ReadPrimaryInt==null||operations.ReadAftMaxTransferLimitFromGame==null||operations.ReadTransactionEventField==null||operations.Emit==null)
                throw new ArgumentException("Genuine MachineData/Jurisdiction/event bindings required");
            host=new Host{JurisdictionFlag=operations.JurisdictionFlag,ReadPrimaryInt=operations.ReadPrimaryInt,ReadAftMaxTransferLimitFromGame=operations.ReadAftMaxTransferLimitFromGame,ReadTransactionEventField=operations.ReadTransactionEventField,Emit=operations.Emit};
            payload=(byte[])initialPayload.Clone();
            storage=new OriginalNv2Storage(device,()=>PayloadBytes,bytes=>{Admit(bytes);payload=(byte[])bytes.Clone();},logRecovery);
            storage.Reload();
        }
        private static void Admit(byte[] bytes)
        {if(bytes==null||bytes.Length!=PayloadLength)throw new ArgumentException("Exact native557-byte MachineDataNvram required");}
        public static OriginalMachineCashoutConfiguration OpenFile(string path,byte[] initialPayload,Host host,Action<string> logRecovery)
        {
            Admit(initialPayload);var device=new OriginalNvFileDevice(path);
            try{var owner=new OriginalMachineCashoutConfiguration(device,initialPayload,host,logRecovery);owner.ownedFile=device;return owner;}
            catch{device.Dispose();throw;}
        }
        public byte[] PayloadBytes{get{return (byte[])payload.Clone();}}
        public byte[] ReadDeviceImage(){return storage.ReadDeviceImage();}
        public void Reload(){storage.Reload();}
        public void ValidateStorage(){storage.Validate();}
        private byte Byte(int wholeOffset){return payload[wholeOffset-0x410];}
        private void SetByte(int wholeOffset,byte value){payload[wholeOffset-0x410]=value;}
        private long Amount(int wholeOffset)
        {ulong value=0;for(int i=0;i<8;++i)value|=(ulong)Byte(wholeOffset+i)<<(8*i);return unchecked((long)value);}
        public long MachineAmount(int interfaceSlot)
        {
            switch(interfaceSlot)
            {
                case 0xb4:return Amount(0x557);
                case 0xfc:return Amount(0x51f);
                case 0x10c:return Amount(0x527);
                case 0x148:return Amount(0x577);
                case 0x2f4:return host.ReadAftMaxTransferLimitFromGame();
                default:throw new ArgumentOutOfRangeException(nameof(interfaceSlot));
            }
        }
        public bool MachineFlag(int interfaceSlot)
        {
            switch(interfaceSlot)
            {
                case 0x140:return Byte(0x5ad)!=0;
                case 0x188:return Byte(0x5ae)!=0;
                case 0x190:return Byte(0x5a7)!=0;
                case 0x1bc:return Byte(0x5ac)!=0;
                case 0x218:return Byte(0x5af)!=0;
                case 0x2c8:return host.ReadPrimaryInt(0x2e4)==2;
                case 0x2fc:return host.ReadPrimaryInt(0x310)==2;
                case 0x390:return Byte(0x5d1)==1;
                default:throw new ArgumentOutOfRangeException(nameof(interfaceSlot));
            }
        }
        public int MachineInt(int interfaceSlot)
        {
            switch(interfaceSlot)
            {
                case 0x1e4:return host.JurisdictionFlag(0x124)?Byte(0x5bc):1;
                case 0x1ec:return Byte(0x5be);
                case 0x1fc:return host.JurisdictionFlag(0x120)?Byte(0x5bd):1;
                case 0x204:return Byte(0x5c1);
                case 0x20c:return Byte(0x5c0);
                default:throw new ArgumentOutOfRangeException(nameof(interfaceSlot));
            }
        }
        public byte SetMachineDisabled(bool value)
        {SetByte(0x5ae,(byte)(value?1:0));storage.Save();return Byte(0x5ae);}
        public byte SetResidualCreditGambleSetting(int value)
        {SetByte(0x5c1,unchecked((byte)value));storage.Save();return Byte(0x5c1);}
        public byte SetTransactionInProgress(bool value)
        {
            SetByte(0x5a8,(byte)(value?1:0));storage.Save();
            if(!value)
            {
                uint field=host.ReadTransactionEventField();var packet=new byte[44];packet[0]=1;packet[2]=44;
                for(int i=0;i<4;++i)packet[24+i]=(byte)(field>>(i*8));host.Emit(packet,0);
            }
            return Byte(0x5a8); // Reentrant callbacks can change the field before this fresh read.
        }
        public void MachineOperation(int interfaceSlot,bool value)
        {if(interfaceSlot!=0x19c)throw new ArgumentOutOfRangeException(nameof(interfaceSlot));SetTransactionInProgress(value);}
        public void Dispose(){if(ownedFile!=null){ownedFile.Dispose();ownedFile=null;}}
    }
}
