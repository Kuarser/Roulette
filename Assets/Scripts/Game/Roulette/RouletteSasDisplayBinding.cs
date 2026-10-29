using UnityEngine;
using RouletteDeluxe.SasDraftV1;
namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private static SasDisplayStateV1 DecodeSasCheckpointDisplay(string encoded,SasIntentCheckpointV1 checkpoint)
        {
            if(checkpoint==null||checkpoint.Stage!=SasRoundIntentStageV1.ReadyCommit)
                throw new PcRoundLifecycleDataException("Display archive binding requires ReadyCommit");
            var root=new LifecycleJsonReader(encoded).Read();
            if(!checkpoint.Qualification.Eligible)
            {
                if(root.Kind!='n')throw new PcRoundLifecycleDataException("Unqualified round has SAS display state");return null;
            }
            if(root.Kind!='{')throw new PcRoundLifecycleDataException("Qualified round lacks display state");
            ValidateLifecycleJsonShape(root,typeof(SasDisplayStateV1),encoded);
            var state=JsonUtility.FromJson<SasDisplayStateV1>(encoded);state.Validate();
            var frozen=ParsePcReceiptContext<SasFrozenInputsV1>(checkpoint.FrozenInputs);
            if(!state.shown||!checkpoint.ShownSuccessfully||checkpoint.Display==null||state.roundId!=checkpoint.RoundId||
                state.policyId!=frozen.displayPolicyId||state.displayMultiplier!=checkpoint.Display.Observation.Payload.Multiplier)
                throw new PcRoundLifecycleDataException("Display state differs from saved SAS observation");
            return state;
        }
    }
}
