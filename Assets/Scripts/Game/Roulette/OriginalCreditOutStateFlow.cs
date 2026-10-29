using System;
using System.Collections;
namespace RouletteDeluxe
{
    // Binds restored Collect/Finish/state4-6 to one caller-owned persistent State
    // and one135-byte cashout NV owner. All hardware/display/other bodies remain
    // mandatory genuine host operations. Templates are copied, never rewritten.
    public sealed class OriginalCreditOutStateFlow
    {
        private readonly OriginalPersistentState manager;
        private readonly OriginalHandpaySelectionPersistence payload;
        private readonly OriginalCreditOutCollect.Runtime runtime;
        private readonly OriginalCreditOutCollect.Host collect;
        private readonly OriginalCreditOutCashoutState.Host state4;
        private readonly OriginalCreditOutCashoutVoucherState.Host state5;
        private readonly OriginalCreditOutCashoutContinueState.Host state6;
        private readonly OriginalCreditOutFinishStates.Host finish;
        private readonly OriginalCreditOutResidualState.Host residualAsync;
        private readonly OriginalCreditOutResidualScreens.OptionHost optionAsync;
        private readonly OriginalHandpaySelected.Host selectedAsync;
        public OriginalCreditOutStateFlow(OriginalPersistentState manager,OriginalHandpaySelectionPersistence payload,
            OriginalCreditOutCollect.Runtime runtime,OriginalCreditOutCollect.Host collectTemplate,
            OriginalCreditOutCashoutState.Host state4Template,OriginalCreditOutCashoutVoucherState.Host state5Template,
            OriginalCreditOutCashoutContinueState.Host state6Template,OriginalCreditOutFinishStates.Host finishTemplate,
            OriginalCreditOutResidualState.Host residualTemplate=null,
            OriginalCreditOutResidualScreens.OptionHost optionTemplate=null,OriginalCreditOutResidualScreens.FailedHost failedTemplate=null,OriginalHandpaySelected.Host selectedTemplate=null)
        {
            this.manager=manager??throw new ArgumentNullException(nameof(manager));this.payload=payload??throw new ArgumentNullException(nameof(payload));
            this.runtime=runtime??throw new ArgumentNullException(nameof(runtime));
            if(collectTemplate==null||state4Template==null||state5Template==null||state6Template==null||finishTemplate==null)throw new ArgumentException("All real flow bindings required");
            collect=collectTemplate.CopyBindings();state4=state4Template.CopyBindings();state5=state5Template.CopyBindings();state6=state6Template.CopyBindings();finish=finishTemplate.CopyBindings();
            collect.ReadManagerState=()=>this.manager.Value;collect.AssignManagerState=n=>this.manager.Assign(n);collect.FinishStates=()=>Finish();
            state5.AssignManagerState=n=>this.manager.Assign(n);state5.CashOutContinueState=()=>CallState6();
            state6.AssignManagerState=n=>this.manager.Assign(n);state6.FinishStates=()=>Finish();
            finish.ManagerState=this.manager;finish.PayloadOwner=this.payload;finish.Runtime=this.runtime;
            if(selectedTemplate!=null&&optionTemplate==null)throw new ArgumentException("Option owner required for HandPaySelected binding");
            if(residualTemplate==null&&(optionTemplate!=null||failedTemplate!=null))throw new ArgumentException("Residual owner required for screen bindings");
            if(residualTemplate!=null)
            {
                var residual=residualTemplate.CopyBindings();residual.AssignManagerState=n=>this.manager.Assign(n);
                if(optionTemplate!=null)
                {
                    var option=optionTemplate.CopyBindings();
                    if(selectedTemplate!=null){var selected=selectedTemplate.CopyBindings();selected.Validate();option.HandPaySelected=()=>OriginalHandpaySelected.Apply(this.payload,selected);}
                    option.ProcessResidualGamble=()=>this.payload.ProcessResidualState(this.runtime,residual);option.Validate();
                    residual.DisplayOptionHandpayCancel=()=>OriginalCreditOutResidualScreens.Option(this.payload,option);
                }
                if(failedTemplate!=null)
                {
                    var failed=failedTemplate.CopyBindings();failed.Validate();
                    residual.DisplayCashoutFailed=n=>OriginalCreditOutResidualScreens.Failed(this.runtime,n,failed);
                }
                residual.Validate();
                residualAsync=residual;optionAsync=optionTemplate==null?null:optionTemplate.CopyBindings();selectedAsync=selectedTemplate==null?null:selectedTemplate.CopyBindings();
                finish.ProcessResidualGamble=()=>this.payload.ProcessResidualState(this.runtime,residual);
            }
            var other=finishTemplate.StateOperation;
            finish.StateOperation=(n,input)=>
            {if(n==4)RunState4(input);else if(n==5)RunState5(input);else if(n==6)RunState6(input);else other(n,input);};
            if(other==null||state4.ExternalFlag80==null||state4.ReadGameFieldE0==null||state4.GameOperation4C==null||state4.GameOperation64==null||state4.Emit==null)
                throw new ArgumentException("Real state4 and other-state bodies required");
            collect.Validate();state5.Validate();state6.Validate();finish.Validate();
        }
        public bool Collect(long amount,bool forced,object originalVoucher)
        {return payload.Collect(runtime,collect,amount,forced,originalVoucher);}
        public bool Finish()
        {using(var input=manager.Copy())return OriginalCreditOutFinishStates.Apply(input,finish);}
        public OriginalCreditOutResidualContinuation BeginResidualSelection()
        {return OriginalCreditOutResidualContinuation.Begin(manager,payload,runtime,residualAsync,optionAsync,selectedAsync);}
        public IEnumerator RunResidualSelection(Func<OriginalHandpaySelectedUnityScreen> readCurrentUi)
        {
            if(readCurrentUi==null)throw new ArgumentNullException(nameof(readCurrentUi));
            using(var continuation=BeginResidualSelection())
            {
                if(continuation==null)yield break;
                var ui=readCurrentUi();
                if(ui==null||selectedAsync.ReadManagerScreen2C4()!=ui.Binding)throw new InvalidOperationException("Actual manager2c4 Unity owner required");
                int? result=null;var display=ui.Display(n=>result=n);
                try{while(display.MoveNext())yield return display.Current;}
                finally{var disposable=display as IDisposable;if(disposable!=null)disposable.Dispose();}
                if(!result.HasValue)throw new InvalidOperationException("Native display did not return a result");
                continuation.Complete(result.Value);
            }
        }
        private void RunState4(OriginalPersistentState input)
        {
            // Native default local State; NewSubState temporary -> reference
            // assignment -> temporary release -> algorithm -> local release.
            using(var sub=new OriginalPersistentState())
            {
                using(var temporary=input.NewSubState())sub.AssignReference(temporary);
                var bound=state4.CopyBindings();bound.ReadSubState=()=>sub.Value;bound.AssignSubState=n=>sub.Assign(n);
                bound.AssignManagerState=n=>manager.Assign(n);bound.CashOutVoucherState=()=>CallState5();
                payload.CashOutState(runtime,bound);
            }
        }
        private void RunState5(OriginalPersistentState input)
        {payload.CashOutVoucherState(runtime,state5);}
        private void RunState6(OriginalPersistentState input)
        {payload.CashOutContinueState(runtime,state6);}
        private void CallState5(){using(var copy=manager.Copy())RunState5(copy);}
        private void CallState6(){using(var copy=manager.Copy())RunState6(copy);}
    }
}
