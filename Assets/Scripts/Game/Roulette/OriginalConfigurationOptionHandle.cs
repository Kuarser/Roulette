using System;
namespace RouletteDeluxe
{
    // Native CfgOptionHandle constructor: two independent ConfigurationOption copies
    // at whole offsets 0 and 0x84, followed by six zero words at 0x108..0x11c.
    public sealed class OriginalConfigurationOptionHandle:IDisposable
    {
        private readonly OriginalConfigurationOption primary,secondary;
        private readonly uint[] tailWords=new uint[6];private bool disposed;
        public OriginalConfigurationOptionHandle(OriginalConfigurationOption option)
        {if(option==null)throw new ArgumentNullException(nameof(option));primary=option.DeepCopy();secondary=option.DeepCopy();}
        private void Check(){if(disposed)throw new ObjectDisposedException(nameof(OriginalConfigurationOptionHandle));}
        public OriginalConfigurationOption Primary{get{Check();return primary;}}
        public OriginalConfigurationOption Secondary{get{Check();return secondary;}}
        public uint[] ConstructorTailWords{get{Check();return (uint[])tailWords.Clone();}}
        public void Dispose(){if(disposed)return;primary.Dispose();secondary.Dispose();disposed=true;}
    }
}
