using System;
using System.Collections.Generic;
namespace RouletteDeluxe
{
    // Native incorporation after field extraction. No upstream XML tokenizer is
    // implied here, and this layer does not reverse the sender's FE/FD bytes.
    public static class OriginalConfigurationXmlReceive
    {
        private static bool Field(byte[] bytes,string text)
        {
            if(bytes.Length!=text.Length)return false;
            for(int i=0;i<bytes.Length;++i)if(bytes[i]!=(byte)text[i])return false;
            return true;
        }
        public static bool Incorporate(OriginalSimpleConfigOption option,byte[] name,byte[] value)
        {
            if(option==null||name==null||value==null)throw new ArgumentNullException();
            if(Field(name,"Name")){option.SetName(value);return true;}
            if(Field(name,"Value")){option.SetValue(value);return true;}
            if(Field(name,"Category")){option.SetCategory(value);return true;}
            return false;
        }
        public sealed class Entry
        {
            private readonly OriginalSimpleConfigOption key;
            private readonly List<byte[]> errors=new List<byte[]>();
            public Entry(OriginalConfigurationTransactionRecord.ErrorEntry admitted)
            {
                if(admitted==null||admitted.Key==null||admitted.Errors==null)throw new ArgumentNullException();
                key=admitted.Key.DeepCopy();
                foreach(var error in admitted.Errors){if(error==null)throw new ArgumentNullException();errors.Add((byte[])error.Clone());}
            }
            public OriginalConfigurationTransactionRecord.ErrorEntry Snapshot()
            {
                var values=new byte[errors.Count][];
                for(int i=0;i<values.Length;++i)values[i]=(byte[])errors[i].Clone();
                return new OriginalConfigurationTransactionRecord.ErrorEntry{Key=key.DeepCopy(),Errors=values};
            }
            public bool Incorporate(byte[] name,byte[] value,ref uint transactionId,OriginalCfgIntegerConverter.State errno)
            {
                if(name==null||value==null||errno==null)throw new ArgumentNullException();
                if(OriginalConfigurationXmlReceive.Incorporate(key,name,value))return true;
                if(Field(name,"ErrorString")){errors.Add((byte[])value.Clone());return true;}
                if(Field(name,"TransactionId")){transactionId=SignedDecimalBits(value,errno);return true;}
                return false;
            }
        }
        internal static uint SignedDecimalBits(byte[] bytes,OriginalCfgIntegerConverter.State errno)
        {
            // strtol(c_str(), null, 10), native signed 32-bit long, ASCII C locale.
            int at=0;
            while(at<bytes.Length&&(bytes[at]==32||(bytes[at]>=9&&bytes[at]<=13)))++at;
            bool negative=false;
            if(at<bytes.Length&&(bytes[at]==43||bytes[at]==45)){negative=bytes[at]==45;++at;}
            uint limit=negative?2147483648u:2147483647u,value=0;bool overflow=false;
            while(at<bytes.Length&&bytes[at]>=48&&bytes[at]<=57)
            {
                uint digit=(uint)(bytes[at++]-48);
                if(value>(limit-digit)/10)overflow=true;
                if(!overflow)value=value*10+digit;
            }
            if(overflow){errno.Errno=34;return negative?0x80000000u:0x7fffffffu;}
            return negative?unchecked(0u-value):value;
        }
    }
}
