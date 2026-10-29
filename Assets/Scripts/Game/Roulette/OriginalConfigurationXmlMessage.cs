using System;
using System.Collections.Generic;
namespace RouletteDeluxe
{
    public static class OriginalConfigurationXmlMessage
    {
        public sealed class ParsedField
        {public readonly byte[] Name,Value;public readonly bool NewStruct;public ParsedField(byte[] name,byte[] value,bool fresh){Name=name;Value=value;NewStruct=fresh;}}
        public sealed class Cursor
        {
            private readonly byte[][] tokens;
            private readonly OriginalCfgIntegerConverter.State errno;
            public int Position{get;private set;}
            public int Count{get{return tokens.Length;}}
            public uint Order{get;private set;}
            public Cursor(byte[] message,uint admittedOrder,OriginalCfgIntegerConverter.State errno)
            {
                if(message==null||errno==null)throw new ArgumentNullException();this.errno=errno;Order=admittedOrder;
                var found=new List<byte[]>();int start=0;
                // tokenise.constprop.181 skips only the leading delimiter run.
                // Later adjacent delimiters produce empty stored strings.
                while(start<message.Length&&(message[start]==34||message[start]==10))++start;
                while(start<message.Length)
                {
                    int end=start;
                    while(end<message.Length&&message[end]!=34&&message[end]!=10)++end;
                    var token=new byte[end-start];Array.Copy(message,start,token,0,token.Length);found.Add(token);start=end+1;
                }
                tokens=found.ToArray();
            }
            private static bool Has(byte[] bytes,string text)
            {for(int i=0;i<=bytes.Length-text.Length;++i){int j=0;while(j<text.Length&&bytes[i+j]==(byte)text[j])++j;if(j==text.Length)return true;}return false;}
            public byte[][] TokenSnapshot(){var result=new byte[tokens.Length][];for(int i=0;i<tokens.Length;++i)result[i]=(byte[])tokens[i].Clone();return result;}
            public ParsedField Next()
            {
                byte[] name=new byte[0];uint localOrder=0;int mode=0;bool mayStart=true,fresh=false;
                while(Position<tokens.Length)
                {
                    var token=tokens[Position];
                    if(Has(token,"="))
                    {
                        if(Has(token,"name"))mode=3;else if(Has(token,"value"))mode=4;else if(Has(token,"order"))mode=5;
                    }
                    else if(mayStart&&Has(token,"<struct>")){mayStart=false;mode=1;fresh=true;}
                    else if(mode==4)
                    {
                        var value=(byte[])token.Clone();for(int i=0;i<value.Length;++i)if(value[i]==254)value[i]=34;else if(value[i]==253)value[i]=0;
                        Order=localOrder;
                        // Native returns with iterator still on the value token.
                        return new ParsedField((byte[])name.Clone(),value,fresh);
                    }
                    else if(mode==3){name=(byte[])token.Clone();mode=0;}
                    else if(mode==5){localOrder=OriginalConfigurationXmlReceive.SignedDecimalBits(token,errno);mode=0;}
                    else mode=0;
                    ++Position;
                }
                // Exhaustion clears name/value, preserves caller order and returns false.
                return new ParsedField(new byte[0],new byte[0],false);
            }
        }
        public sealed class SimpleOptions
        {
            private readonly List<OriginalSimpleConfigOption> options=new List<OriginalSimpleConfigOption>();
            private readonly OriginalCfgIntegerConverter.State errno;
            public SimpleOptions(OriginalSimpleConfigOption[] admitted,OriginalCfgIntegerConverter.State errno)
            {if(admitted==null||errno==null)throw new ArgumentNullException();this.errno=errno;foreach(var key in admitted){if(key==null)throw new ArgumentNullException();options.Add(key.DeepCopy());}}
            public OriginalSimpleConfigOption[] Snapshot(){var result=new OriginalSimpleConfigOption[options.Count];for(int i=0;i<result.Length;++i)result[i]=options[i].DeepCopy();return result;}
            public bool BuildFromMessage(byte[] message)
            {
                var cursor=new Cursor(message,0,errno);
                while(cursor.Position<cursor.Count)
                {
                    var field=cursor.Next();
                    if(field.NewStruct)options.Add(new OriginalSimpleConfigOption(new byte[0],new byte[0],new byte[0]));
                    if(field.Name.Length!=0&&field.Value.Length!=0&&options.Count!=0)
                        OriginalConfigurationXmlReceive.Incorporate(options[options.Count-1],field.Name,field.Value);
                }
                // Original normal return is false even when the stored vector changed.
                return false;
            }
        }
    }
}
