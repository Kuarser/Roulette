using System;
using System.Text;
namespace RouletteDeluxe
{
    // Complete normal body of MachineData::LoadConfiguration083912d0. Parser,
    // SystemConfiguration, native numeric conversions and live machine operations
    // remain required bindings; this is the pre-NV default/runtime region.
    public sealed class OriginalMachineDataDefaults
    {
        public const int RegionLength=0x3c8;
        public sealed class Host
        {
            public Action<int,int> PrimaryOperation;
            public Func<int,byte> JurisdictionByte;
            public Func<int,int> JurisdictionInt;
            public Func<int,long> JurisdictionAmount;
            public Func<int,byte[]> JurisdictionText;
            public Action<string> ParseFile;
            public Func<string,byte[]> ParserString; // null=absent; existing empty string remains present.
            public Action ClearParser;
            public Func<string,string,byte[]> SystemString;
            public Func<string,string,byte> SystemBool;
            public Func<byte[],long,long> ExtractStreamInt64,ScanInt64;
            public Func<byte[],int,int> ScanInt32;
        }
        private sealed class Region
        {
            private readonly byte[] bytes;private readonly OriginalNativeMemory memory;private readonly uint owner;
            internal Region(byte[] initial){bytes=(byte[])initial.Clone();}
            internal Region(OriginalNativeMemory shared,uint address){memory=shared;owner=address;}
            internal byte this[int offset]
            {
                get{if(offset<0||offset>=RegionLength)throw new IndexOutOfRangeException();return memory==null?bytes[offset]:memory.ReadByte(unchecked(owner+(uint)offset));}
                set{if(offset<0||offset>=RegionLength)throw new IndexOutOfRangeException();if(memory==null)bytes[offset]=value;else memory.WriteByte(unchecked(owner+(uint)offset),value);}
            }
        }
        private readonly Region region;
        public OriginalMachineDataDefaults(byte[] initialRegion)
        {if(initialRegion==null||initialRegion.Length!=RegionLength)throw new ArgumentException("Exact whole-object pre-NV region required");region=new Region(initialRegion);}
        // The caller owns the live allocation; every load write is immediately visible
        // to subsequent callbacks and MachineData controls, including on failure.
        public OriginalMachineDataDefaults(OriginalNativeMemory memory,uint owner)
        {if(memory==null)throw new ArgumentNullException(nameof(memory));memory.ReadBytes(owner,RegionLength);region=new Region(memory,owner);}
        public byte[] Bytes{get{var result=new byte[RegionLength];for(int i=0;i<result.Length;++i)result[i]=region[i];return result;}}
        public byte ReadByte(int offset){return region[offset];}
        public uint ReadUInt32(int offset){uint v=0;for(int i=0;i<4;++i)v|=(uint)region[offset+i]<<(8*i);return v;}
        public long ReadInt64(int offset){ulong v=0;for(int i=0;i<8;++i)v|=(ulong)region[offset+i]<<(8*i);return unchecked((long)v);}
        private void Write64(int offset,long value){for(int i=0;i<8;++i)region[offset+i]=(byte)(unchecked((ulong)value)>>(8*i));}
        private void Write32(int offset,uint value){for(int i=0;i<4;++i)region[offset+i]=(byte)(value>>(8*i));}
        private static byte[] CBytes(byte[] raw)
        {if(raw==null)throw new ArgumentNullException(nameof(raw));int n=Array.IndexOf(raw,(byte)0);if(n<0)n=raw.Length;var b=new byte[n];Array.Copy(raw,b,n);return b;}
        private void WriteCString(int offset,int capacity,byte[] raw)
        {byte[] b=CBytes(raw);if(b.Length>=capacity)throw new ArgumentException("Native unchecked strcpy overflow is outside managed admission");for(int i=0;i<b.Length;++i)region[offset+i]=b[i];region[offset+b.Length]=0;}
        private static bool Enabled(byte[] raw)
        {byte[] b=CBytes(raw);byte[] enabled=Encoding.ASCII.GetBytes("Enabled");if(b.Length!=enabled.Length)return false;for(int i=0;i<b.Length;++i){byte v=b[i];if(v>=65&&v<=90)v+=32;byte e=enabled[i];if(e>=65&&e<=90)e+=32;if(v!=e)return false;}return true;}
        private static long BillLimit(Host h)
        {byte flag=h.JurisdictionByte(0x64);int limit=h.JurisdictionInt(0x9c);return flag!=0?limit:999999999L;}
        private static long JackpotLimit(Host h)
        {byte flag=h.JurisdictionByte(0x48);int limit=h.JurisdictionInt(0x90);return flag!=0?limit:999999999L;}
        private void ScanAmount(Host h,string key,int offset)
        {byte[] b=h.ParserString(key);if(b!=null)Write64(offset,h.ScanInt64(CBytes(b),ReadInt64(offset)));}
        private void ScanFlag(Host h,string key,int offset)
        {byte[] b=h.ParserString(key);if(b!=null)region[offset]=(byte)(Enabled(b)?1:0);}
        public bool Load(Host supplied)
        {
            if(supplied==null||supplied.PrimaryOperation==null||supplied.JurisdictionByte==null||supplied.JurisdictionInt==null||supplied.JurisdictionAmount==null||supplied.JurisdictionText==null||supplied.ParseFile==null||supplied.ParserString==null||supplied.ClearParser==null||supplied.SystemString==null||supplied.SystemBool==null||supplied.ExtractStreamInt64==null||supplied.ScanInt64==null||supplied.ScanInt32==null)
                throw new ArgumentException("Complete native configuration owners/converters required");
            var h=new Host{PrimaryOperation=supplied.PrimaryOperation,JurisdictionByte=supplied.JurisdictionByte,JurisdictionInt=supplied.JurisdictionInt,JurisdictionAmount=supplied.JurisdictionAmount,JurisdictionText=supplied.JurisdictionText,ParseFile=supplied.ParseFile,ParserString=supplied.ParserString,ClearParser=supplied.ClearParser,SystemString=supplied.SystemString,SystemBool=supplied.SystemBool,ExtractStreamInt64=supplied.ExtractStreamInt64,ScanInt64=supplied.ScanInt64,ScanInt32=supplied.ScanInt32};
            h.PrimaryOperation(0x254,2);Write64(8,BillLimit(h));long jackpot=JackpotLimit(h);Write32(0x3c4,0x40010);
            Write64(0x20,100000);Write64(0x30,100000000);Write64(0x10,jackpot);Write64(0x28,jackpot);Write64(0x38,1);
            Write64(0x70,h.JurisdictionAmount(0xbc));Write64(0x78,BillLimit(h));Write64(0x80,5);
            for(int i=0;i<15;++i)region[0x90+i]=32;region[0x9f]=0;region[0xa0]=1;region[0xa1]=0;region[0xa2]=0;region[0xa4]=0;region[0xa6]=0;region[0xa5]=0;region[0xa7]=0;
            Write64(0x48,20000);Write64(0x50,20000);Write64(0x58,100);Write64(0x60,100);
            byte[] jurisdiction=CBytes(h.JurisdictionText(0x224));bool nsw=jurisdiction.Length==3&&jurisdiction[0]==78&&jurisdiction[1]==83&&jurisdiction[2]==87;
            string path=nsw?"/agk/cfg/machineDataNSW.cfg":"/agk/cfg/machineData.cfg";WriteCString(0x228,32,Encoding.ASCII.GetBytes(path));h.ParseFile(path);
            region[0x208]=0;byte[] manufacturer=h.ParserString("DefaultManufacturer");if(manufacturer!=null)WriteCString(0x208,32,manufacturer);
            if(region[0x208]==0)WriteCString(0x208,32,Encoding.ASCII.GetBytes("Bally Gaming, Inc."));
            else
            {
                Write64(0x18,h.ExtractStreamInt64(CBytes(h.SystemString("DefaultCreditLimit","")),ReadInt64(0x18)));
                ScanAmount(h,"DefaultTaxLimit",0x28);if(h.JurisdictionByte(0x48)!=0){Write32(0x28,ReadUInt32(0x10));Write32(0x2c,ReadUInt32(0x14));}
                ScanAmount(h,"DefaultWinLimit",0x70);
                Write64(0x30,h.ExtractStreamInt64(CBytes(h.SystemString("DefaultTicketLimitValue","")),ReadInt64(0x30)));
                ScanAmount(h,"DefaultHopperLimit",0x20);ScanAmount(h,"DefaultCashInLimit",0x78);
                if(h.JurisdictionByte(0x64)!=0){long max=ReadInt64(8);if(max>0&&max<=ReadInt64(0x18))Write64(0x78,BillLimit(h));}
                ScanAmount(h,"DefaultBillValRejectLimit",0x80);
                byte[] serial=h.ParserString("DefaultSerialNumberMaxLength");if(serial!=null)h.ScanInt32(CBytes(serial),16); // Native local only; discarded.
                for(int i=0;i<15;++i)region[0x90+i]=32;region[0x9f]=0;
                ScanFlag(h,"DefaultTicketInEnabled",0xa0);ScanFlag(h,"DefaultTicketOutEnabled",0xa1);
                byte[] attendant=h.ParserString("DefaultAttendantTicketPayEnabled");if(attendant!=null){region[0xa4]=(byte)(Enabled(attendant)?1:0);region[0xa4]=h.SystemBool("DefaultAttendantTicketPayEnabled","");}
                ScanFlag(h,"DefaultHopperEnabled",0xa5);ScanFlag(h,"DefaultGamePlayOperatorMenu",0xa7);ScanFlag(h,"DefaultFractionalCreditsEnabled",0xa3);
                byte[] ticket=h.ParserString("DefaultTicketLimitEnabled");if(ticket!=null){Write64(0x38,Enabled(ticket)?1:0);Write64(0x38,h.SystemBool("DefaultTicketLimit",""));}
            }
            Write32(0x40,ReadUInt32(0x18));Write32(0x44,ReadUInt32(0x1c));Write32(0x88,ReadUInt32(0x18));Write32(0x8c,ReadUInt32(0x1c));h.ClearParser();
            if(region[0xa5]!=0)region[0xa5]=h.JurisdictionByte(0x1ac);
            if(region[0xa1]!=0)region[0xa1]=h.JurisdictionByte(0x1b0);
            return true;
        }
    }
}
