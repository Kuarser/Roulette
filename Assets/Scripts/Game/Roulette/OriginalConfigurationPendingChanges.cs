using System;
using System.Collections.Generic;
namespace RouletteDeluxe
{
    // CfgClientHandle queue at whole-object 8c/90/94. Actual handle references
    // persist; GetPendingChanges reads Primary at retrieval time and does not drain.
    public sealed class OriginalConfigurationPendingChanges
    {
        private readonly List<OriginalConfigurationOptionHandle> pending=new List<OriginalConfigurationOptionHandle>();
        public OriginalConfigurationPendingChanges(OriginalConfigurationOptionHandle[] admitted)
        {if(admitted==null)throw new ArgumentNullException(nameof(admitted));foreach(var handle in admitted)AddChange(handle);}
        public void AddChange(OriginalConfigurationOptionHandle handle)
        {if(handle==null)throw new ArgumentNullException(nameof(handle));pending.Add(handle);}
        public OriginalConfigurationOptionHandle[] HandleSnapshot(){return pending.ToArray();}
        public OriginalSimpleConfigOption[] GetPendingChanges()
        {var result=new OriginalSimpleConfigOption[pending.Count];for(int i=0;i<result.Length;++i)result[i]=OriginalSimpleConfigOption.FromConfiguration(pending[i].Primary);return result;}
        public void ClearPendingChanges(){pending.Clear();}
    }
}
