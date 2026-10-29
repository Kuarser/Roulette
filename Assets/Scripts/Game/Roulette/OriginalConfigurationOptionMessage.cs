using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
namespace RouletteDeluxe
{
    // Native full-option message owners. Dual.Build mutates its first vector.
    public static class OriginalConfigurationOptionMessage
    {
        private static byte[] A(string text){return Encoding.ASCII.GetBytes(text);}
        private static void Write(MemoryStream stream,IEnumerable<OriginalConfigurationOption> options)
        {
            foreach(var option in options)
            {
                foreach(var bytes in new[]{A("<struct>\n"),OriginalConfigurationOptionXml.Option(option),A("</struct>\n")})stream.Write(bytes,0,bytes.Length);
            }
        }
        public static byte[] Single(OriginalConfigurationOption[] options)
        {
            if(options==null)throw new ArgumentNullException(nameof(options));
            using(var stream=new MemoryStream()){Write(stream,options);return stream.ToArray();}
        }
        public sealed class Dual:IDisposable
        {
            private readonly List<OriginalConfigurationOption> first=new List<OriginalConfigurationOption>(),second=new List<OriginalConfigurationOption>();
            private bool disposed;
            public Dual(OriginalConfigurationOption[] first,OriginalConfigurationOption[] second)
            {
                if(first==null||second==null)throw new ArgumentNullException();
                foreach(var option in first)this.first.Add(option.DeepCopy());
                foreach(var option in second)this.second.Add(option.DeepCopy());
            }
            private void Check(){if(disposed)throw new ObjectDisposedException(nameof(Dual));}
            private static OriginalConfigurationOption[] Copy(List<OriginalConfigurationOption> options)
            {var result=new OriginalConfigurationOption[options.Count];for(int i=0;i<result.Length;++i)result[i]=options[i].DeepCopy();return result;}
            public OriginalConfigurationOption[] First{get{Check();return Copy(first);}}
            public OriginalConfigurationOption[] Second{get{Check();return Copy(second);}}
            public byte[] Build()
            {
                Check();var marker=new OriginalConfigurationOption();marker.SetName(A("__UNDFINED_MIDDLE_OF_LIST__"));marker.SetCategory(new byte[0]);first.Add(marker);
                using(var stream=new MemoryStream()){Write(stream,first);Write(stream,second);return stream.ToArray();}
            }
            public void Dispose(){Check();foreach(var option in first)option.Dispose();foreach(var option in second)option.Dispose();disposed=true;}
        }
    }
}
