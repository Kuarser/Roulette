using System;
namespace RouletteDeluxe
{
    // Native POD: Int/Int64 allocate one signed32 offset; Str/Bit allocate two.
    // Shared holder pointer permits type changes without copying or replacing the record.
    public sealed class OriginalCfgJurisdictionOption
    {
        public int Offset;
        private int argument;private bool argumentKnown;
        public OriginalCfgJurisdictionOption(int offset){Offset=offset;}
        public OriginalCfgJurisdictionOption(int offset,int argument){Offset=offset;this.argument=argument;argumentKnown=true;}
        public int Argument{get{if(!argumentKnown)throw new InvalidOperationException("Native second option word requires caller-owned storage");return argument;}set{argument=value;argumentKnown=true;}}
        public bool HasArgument{get{return argumentKnown;}}
    }
}
