using System;
using System.Collections.Generic;
namespace RouletteDeluxe
{
    // Normal logical TestRequest and ConfigChangeTransaction storage owners.
    // Explicit admission keeps unwritten native flag bytes unknown.
    public sealed class OriginalConfigurationTransactionRecord
    {
        public sealed class ErrorEntry{public OriginalSimpleConfigOption Key;public byte[][] Errors;}
        public sealed class Request
        {
            public uint Id{get;private set;}public object Client{get;private set;}
            private readonly byte?[] flags;private readonly byte[] detail;
            internal readonly SortedDictionary<OriginalSimpleConfigOption,List<byte[]>> errors=new SortedDictionary<OriginalSimpleConfigOption,List<byte[]>>(new OriginalSimpleConfigOption.NativeComparer());
            public byte?[] Flags{get{return (byte?[])flags.Clone();}}public byte[] Detail{get{return (byte[])detail.Clone();}}
            public Request(uint id,object client,byte?[] flags,byte[] detail,ErrorEntry[] admitted)
            {
                if(flags==null||flags.Length!=4||detail==null||admitted==null)throw new ArgumentException("Four caller-known/unknown flag bytes, detail and unique error map required");Id=id;Client=client;this.flags=(byte?[])flags.Clone();this.detail=(byte[])detail.Clone();
                foreach(var entry in admitted){if(entry==null||entry.Key==null||entry.Errors==null)throw new ArgumentNullException();var values=new List<byte[]>();foreach(var value in entry.Errors){if(value==null)throw new ArgumentNullException();values.Add((byte[])value.Clone());}errors.Add(entry.Key.DeepCopy(),values);}
            }
            public static Request FromOptions(OriginalSimpleConfigOption[] options,uint id,object client,byte?[] callerFlags)
            {
                if(options==null||callerFlags==null||callerFlags.Length!=3)throw new ArgumentException("Three unwritten native flag slots required");var map=new SortedDictionary<OriginalSimpleConfigOption,ErrorEntry>(new OriginalSimpleConfigOption.NativeComparer());
                foreach(var key in options){if(key==null)throw new ArgumentNullException();if(!map.ContainsKey(key))map.Add(key,new ErrorEntry{Key=key,Errors=new byte[0][]});}
                var entries=new ErrorEntry[map.Count];map.Values.CopyTo(entries,0);return new Request(id,client,new byte?[]{callerFlags[0],callerFlags[1],callerFlags[2],0},new byte[0],entries);
            }
            public static Request FromErrors(ErrorEntry[] errors,uint id,object client,byte?[] callerFlags)
            {return new Request(id,client,callerFlags,new byte[0],errors);}
            public ErrorEntry[] ErrorSnapshot(){var result=new ErrorEntry[errors.Count];int i=0;foreach(var pair in errors){var values=new byte[pair.Value.Count][];for(int j=0;j<values.Length;++j)values[j]=(byte[])pair.Value[j].Clone();result[i++]=new ErrorEntry{Key=pair.Key.DeepCopy(),Errors=values};}return result;}
            public Request DeepCopy(){return new Request(Id,Client,flags,detail,ErrorSnapshot());}
        }
        private readonly byte[] flags;
        internal readonly Request[] requestsA,requestsB;
        internal readonly OriginalSimpleConfigOption[] optionsA,optionsB;
        public object Sender{get;private set;}
        public bool HasKnownSender{get;private set;}
        internal void SetFlag(int at,byte value){flags[at]=value;}
        public byte[] Flags{get{return (byte[])flags.Clone();}}
        public Request[] RequestsA{get{return Copy(requestsA);}}public Request[] RequestsB{get{return Copy(requestsB);}}
        public OriginalSimpleConfigOption[] OptionsA{get{return Copy(optionsA);}}public OriginalSimpleConfigOption[] OptionsB{get{return Copy(optionsB);}}
        private static Request[] Copy(Request[] input){if(input==null)throw new ArgumentNullException();var output=new Request[input.Length];for(int i=0;i<input.Length;++i){if(input[i]==null)throw new ArgumentNullException();output[i]=input[i].DeepCopy();}return output;}
        private static OriginalSimpleConfigOption[] Copy(OriginalSimpleConfigOption[] input){if(input==null)throw new ArgumentNullException();var output=new OriginalSimpleConfigOption[input.Length];for(int i=0;i<input.Length;++i){if(input[i]==null)throw new ArgumentNullException();output[i]=input[i].DeepCopy();}return output;}
        public OriginalConfigurationTransactionRecord(byte[] flags,Request[] requestsA,Request[] requestsB,OriginalSimpleConfigOption[] optionsA,OriginalSimpleConfigOption[] optionsB,object sender)
            :this(flags,requestsA,requestsB,optionsA,optionsB,sender,true){}
        public OriginalConfigurationTransactionRecord(byte[] flags,Request[] requestsA,Request[] requestsB,OriginalSimpleConfigOption[] optionsA,OriginalSimpleConfigOption[] optionsB,object sender,bool hasKnownSender)
        {if(flags==null||flags.Length!=6)throw new ArgumentException("Six admitted native transaction flag bytes required");this.flags=(byte[])flags.Clone();this.requestsA=Copy(requestsA);this.requestsB=Copy(requestsB);this.optionsA=Copy(optionsA);this.optionsB=Copy(optionsB);Sender=sender;HasKnownSender=hasKnownSender;}
        public int GetErrorCount(){int count=0;foreach(var request in requestsB)foreach(var entry in request.errors)count=unchecked(count+entry.Value.Count);return count;}
        public OriginalConfigurationTransactionRecord DeepCopy(){return new OriginalConfigurationTransactionRecord(flags,requestsA,requestsB,optionsA,optionsB,Sender,HasKnownSender);}
    }
}
