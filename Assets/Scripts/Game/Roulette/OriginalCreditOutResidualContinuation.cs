using System;
namespace RouletteDeluxe
{
    // Preserves native state26 -> option -> HandPaySelected stack continuation
    // across Unity input frames. No callback completion or State assignment is
    // fabricated while input is pending; copied State owns the suspended lease.
    public sealed class OriginalCreditOutResidualContinuation:IDisposable
    {
        private readonly OriginalHandpaySelectionPersistence payload;
        private readonly OriginalCreditOutCollect.Runtime runtime;
        private readonly OriginalCreditOutResidualState.Host residual;
        private readonly OriginalCreditOutResidualScreens.OptionHost option;
        private readonly OriginalHandpaySelected.Session body;
        private OriginalPersistentState lease;
        private bool completed;
        private OriginalCreditOutResidualContinuation(OriginalPersistentState lease,OriginalHandpaySelectionPersistence payload,
            OriginalCreditOutCollect.Runtime runtime,OriginalCreditOutResidualState.Host residual,
            OriginalCreditOutResidualScreens.OptionHost option,OriginalHandpaySelected.Session body)
        {this.lease=lease;this.payload=payload;this.runtime=runtime;this.residual=residual;this.option=option;this.body=body;}
        // Null means the native dispatch completed synchronously without modal.
        public static OriginalCreditOutResidualContinuation Begin(OriginalPersistentState manager,
            OriginalHandpaySelectionPersistence payload,OriginalCreditOutCollect.Runtime runtime,
            OriginalCreditOutResidualState.Host residualTemplate,OriginalCreditOutResidualScreens.OptionHost optionTemplate,
            OriginalHandpaySelected.Host selectedTemplate)
        {
            if(manager==null||payload==null||runtime==null||residualTemplate==null||optionTemplate==null||selectedTemplate==null)throw new ArgumentNullException();
            if(manager.Value!=26)throw new InvalidOperationException("Native residual manager state26 required");
            var lease=manager.Copy();
            try
            {
                var residual=residualTemplate.CopyBindings();residual.AssignManagerState=n=>manager.Assign(n);residual.Validate();
                if(payload.CurrentPayload.ResidualSubstateValue!=2){payload.ProcessResidualState(runtime,residual);lease.Dispose();return null;}
                if(!OriginalCreditOutResidualState.Option(payload,runtime,residual,true)){residual.ArmTrigger(0x318,0);lease.Dispose();return null;}
                var option=optionTemplate.CopyBindings();
                if(option.PendingScreen2C4==null)throw new ArgumentException("Actual manager2c4 owner required");
                if(option.PendingScreen2C4()){runtime.Field3AB=0;residual.ArmTrigger(0x318,0);lease.Dispose();return null;}
                option.ProcessResidualGamble=()=>payload.ProcessResidualState(runtime,residual);option.Validate();
                option.GameOperation(0x204);option.MachineOperation(0x19c,true);
                var selected=selectedTemplate.CopyBindings();var body=OriginalHandpaySelected.Begin(payload,selected);
                return new OriginalCreditOutResidualContinuation(lease,payload,runtime,residual,option,body);
            }
            catch{lease.Dispose();throw;}
        }
        public bool Complete(int displayResult)
        {
            if(completed||lease==null)throw new InvalidOperationException("Native residual continuation no longer active");
            try
            {
                bool selected=body.Complete(displayResult);
                OriginalCreditOutResidualScreens.FinishOptionSelection(payload,option,selected);
                runtime.Field3AB=0;residual.ArmTrigger(0x318,0);completed=true;return selected;
            }
            finally{Dispose();}
        }
        // Interrupted/failed display unwinds copied State only. It does not
        // invent body cleanup, clear NV or mark selection/payment completed.
        public void Dispose(){if(lease!=null){lease.Dispose();lease=null;}}
    }
}
