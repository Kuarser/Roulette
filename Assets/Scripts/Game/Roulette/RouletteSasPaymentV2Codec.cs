using UnityEngine;
namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private static PcGamePaymentRecordV2 DecodeSasPaymentV2(string encoded,bool applied)
        {
            var root=new LifecycleJsonReader(encoded).Read();
            if(root.Kind!='{')throw new PcRoundLifecycleDataException("V2 payment must be an object");
            ValidateLifecycleJsonShape(root,typeof(PcGamePaymentRecordV2),encoded);
            var value=JsonUtility.FromJson<PcGamePaymentRecordV2>(encoded);
            RestoreLifecycleJsonNulls(root,value);value.Validate(applied);return value;
        }
        private static string EncodeSasPaymentV2(PcGamePaymentRecordV2 value,bool applied)
        {
            if(value==null)throw new PcRoundLifecycleDataException("V2 payment missing");value.Validate(applied);
            string encoded=WriteValidatedSasPaymentV2(value);DecodeSasPaymentV2(encoded,applied);return encoded;
        }
        // Pure canonical comparison for immediately validated in-memory records.
        // Durable encoding retains its validation and decode round-trip above.
        private static string WriteValidatedSasPaymentV2(PcGamePaymentRecordV2 value)
        {
            // JsonUtility emits default objects for some null class fields. Keep
            // absent observation evidence explicitly null in the durable text.
            string raw=JsonUtility.ToJson(value);var root=new LifecycleJsonReader(raw).Read();
            var text=new System.Text.StringBuilder("{");bool first=true;
            foreach(var field in LifecycleJsonFields(typeof(PcGamePaymentRecordV2)))
            {
                if(!first)text.Append(',');first=false;text.Append(QuoteSasCanonical(field.Name)).Append(':');
                text.Append(field.GetValue(value)==null?"null":root.Members[field.Name].Text(raw));
            }
            return text.Append('}').ToString();
        }
    }
}
