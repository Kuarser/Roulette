using System;
using System.Collections.Generic;
using System.Text;
namespace RouletteDeluxe
{
    // Native NV2 LogRecovery marker content, clock and stream control boundaries.
    public sealed class OriginalNvRecoveryLog
    {
        public sealed class LocalFields
        {
            public readonly int Second,Minute,Hour,Day,Month,Year;
            public LocalFields(int second,int minute,int hour,int day,int month,int year)
            {Second=second;Minute=minute;Hour=hour;Day=day;Month=month;Year=year;}
        }
        public sealed class MarkerStream
        {
            public readonly Func<byte[],uint,bool> Open;
            public readonly Func<bool> Close;
            public readonly Func<uint> GetState;
            public readonly Action<uint> ClearState;
            // Actual ofstream destruction: includes its second filebuf close,
            // basic_file, locale and ios_base destruction. No managed default.
            public readonly Action Destroy;
            public MarkerStream(Func<byte[],uint,bool> open,Func<bool> close,Func<uint> getState,Action<uint> clearState,Action destroy)
            {if(open==null||close==null||getState==null||clearState==null||destroy==null)throw new ArgumentException("Actual marker ofstream owner required");Open=open;Close=close;GetState=getState;ClearState=clearState;Destroy=destroy;}
        }
        public sealed class Host
        {
            public Func<int,byte[],OriginalConfigurationNvData.ByteSlot[],int> Stat;
            public Func<byte[]> GetStoragePath;
            public Func<OriginalConfigurationNvData.ByteSlot[],int> Time;
            public Func<OriginalConfigurationNvData.ByteSlot[],LocalFields> LocalTime;
            public Func<byte[],byte[],int[],int> Sprintf;
            public Func<MarkerStream> CreateStream;
        }
        private readonly Host host;
        public OriginalNvRecoveryLog(Host h)
        {
            if(h==null||h.Stat==null||h.GetStoragePath==null||h.Time==null||h.LocalTime==null||h.Sprintf==null||h.CreateStream==null)throw new ArgumentException("Actual NV storage, POSIX clock/stat/format and marker stream bindings required");
            host=new Host{Stat=h.Stat,GetStoragePath=h.GetStoragePath,Time=h.Time,LocalTime=h.LocalTime,Sprintf=h.Sprintf,CreateStream=h.CreateStream};
        }
        private static OriginalConfigurationNvData.ByteSlot[] Unknown(int count)
        {var result=new OriginalConfigurationNvData.ByteSlot[count];for(int i=0;i<count;++i)result[i]=new OriginalConfigurationNvData.ByteSlot(null);return result;}
        public void LogRecovery(byte[] recoveredCopy)
        {
            if(recoveredCopy==null)throw new ArgumentException("Actual recovered-copy stdstring required");
            byte[] tag=(byte[])recoveredCopy.Clone(),directory=Encoding.ASCII.GetBytes("/nvram/NVRecovery/");
            if(host.Stat(3,directory,Unknown(88))!=0)return;
            var stamp=new byte[20];byte[] path=host.GetStoragePath();
            if(path==null)throw new InvalidOperationException("Actual NV recovery storage path owner required");
            path=(byte[])path.Clone();for(int i=0;i<path.Length;++i)if(path[i]==(byte)'/')path[i]=(byte)'-';
            var time=Unknown(4);host.Time(time); // Native return ignored; actual output word passed to localtime.
            var fields=host.LocalTime(time);
            if(fields==null)throw new InvalidOperationException("Actual localtime field frame required");
            host.Sprintf(stamp,Encoding.ASCII.GetBytes("%02i:%02i:%02i-%02i_%02i_%04i"),new[]{fields.Hour,fields.Minute,fields.Second,unchecked(fields.Month+1),fields.Day,unchecked(fields.Year+1900)});
            int length=Array.IndexOf(stamp,(byte)0);if(length<0)throw new InvalidOperationException("Native sprintf timestamp requires admitted terminator");
            var marker=new List<byte>();marker.AddRange(directory);marker.AddRange(tag);marker.AddRange(path);marker.Add((byte)'-');for(int i=0;i<length;++i)marker.Add(stamp[i]);
            var stream=host.CreateStream();if(stream==null)throw new InvalidOperationException("Actual marker ofstream owner required");
            if(stream.Open(marker.ToArray(),0x11))stream.ClearState(0);else stream.ClearState(stream.GetState()|4u);
            if(!stream.Close())stream.ClearState(stream.GetState()|4u);
            stream.Destroy();
        }
    }
}
