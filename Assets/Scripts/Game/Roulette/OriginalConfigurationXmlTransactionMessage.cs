using System;
using System.Collections.Generic;
namespace RouletteDeluxe
{
    // XMLMsgTestRequest storage after explicit id/map/errno admission.
    public sealed class OriginalConfigurationXmlTransactionMessage
    {
        private uint transactionId;
        private readonly OriginalCfgIntegerConverter.State errno;
        private readonly SortedDictionary<OriginalSimpleConfigOption,byte[][]> errors=new SortedDictionary<OriginalSimpleConfigOption,byte[][]>(new OriginalSimpleConfigOption.NativeComparer());
        private static byte[][] Copy(byte[][] values){if(values==null)throw new ArgumentNullException();var result=new byte[values.Length][];for(int i=0;i<values.Length;++i){if(values[i]==null)throw new ArgumentNullException();result[i]=(byte[])values[i].Clone();}return result;}
        public OriginalConfigurationXmlTransactionMessage(uint admittedId,OriginalConfigurationTransactionRecord.ErrorEntry[] admitted,OriginalCfgIntegerConverter.State errno)
        {
            if(admitted==null||errno==null)throw new ArgumentNullException();transactionId=admittedId;this.errno=errno;
            foreach(var entry in admitted){if(entry==null||entry.Key==null)throw new ArgumentNullException();errors.Add(entry.Key.DeepCopy(),Copy(entry.Errors));}
        }
        public uint TransactionId{get{return transactionId;}}
        public OriginalConfigurationTransactionRecord.ErrorEntry[] Snapshot()
        {
            var result=new OriginalConfigurationTransactionRecord.ErrorEntry[errors.Count];int i=0;
            foreach(var entry in errors)result[i++]=new OriginalConfigurationTransactionRecord.ErrorEntry{Key=entry.Key.DeepCopy(),Errors=Copy(entry.Value)};
            return result;
        }
        public OriginalConfigurationTransactionRecord.Request ToRequest(object client,byte?[] callerFlags)
        {return OriginalConfigurationTransactionRecord.Request.FromErrors(Snapshot(),transactionId,client,callerFlags);}
        private static bool Named(byte[] name,string text){if(name.Length!=text.Length)return false;for(int i=0;i<name.Length;++i)if(name[i]!=(byte)text[i])return false;return true;}
        public bool BuildFromMessage(byte[] message)
        {
            var cursor=new OriginalConfigurationXmlMessage.Cursor(message,0,errno);
            errors.Clear();OriginalConfigurationXmlReceive.Entry current=null;
            while(cursor.Position<cursor.Count)
            {
                var field=cursor.Next();
                if(field.NewStruct)
                {
                    if(current!=null)
                    {
                        var entry=current.Snapshot();
                        // Native map operator[] keeps the first equivalent key/value;
                        // vector assignment replaces its error strings with the latest.
                        if(errors.ContainsKey(entry.Key))errors[entry.Key]=Copy(entry.Errors);
                        else errors.Add(entry.Key.DeepCopy(),Copy(entry.Errors));
                    }
                    current=new OriginalConfigurationXmlReceive.Entry(new OriginalConfigurationTransactionRecord.ErrorEntry{Key=new OriginalSimpleConfigOption(new byte[0],new byte[0],new byte[0]),Errors=new byte[0][]});
                }
                if(field.Name.Length!=0&&field.Value.Length!=0)
                {
                    if(current==null)
                    {
                        // Unknown fields and TransactionId never dereference the
                        // temporary pair; other recognized fields require its owner.
                        if(Named(field.Name,"TransactionId"))transactionId=OriginalConfigurationXmlReceive.SignedDecimalBits(field.Value,errno);
                        else if(Named(field.Name,"Name")||Named(field.Name,"Category")||Named(field.Name,"Value")||Named(field.Name,"ErrorString"))throw new InvalidOperationException("Native field incorporation requires an active struct owner");
                        continue;
                    }
                    current.Incorporate(field.Name,field.Value,ref transactionId,errno);
                }
            }
            // Native deletes the final temporary record without inserting it.
            return false;
        }
    }
}
