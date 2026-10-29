using System;

namespace RouletteDeluxe
{
    // Owns one original CreditOutMgr NV data area and its selection-state object.
    // Reload updates the same State reference already held by the coordinator.
    public sealed class OriginalHandpaySelectionPersistence : IDisposable
    {
        private OriginalCreditOutNvPayload payload;
        private readonly OriginalNv2Storage storage;
        private OriginalNvFileDevice ownedFile;
        private OriginalHandpaySelectionCoordinator coordinator;
        public readonly OriginalHandpaySelectionNvState State;
        public OriginalHandpaySelectionPersistence(IOriginalNvDevice device, byte[] initialPayload, Action<string> logRecovery)
        {
            payload = new OriginalCreditOutNvPayload(initialPayload);
            State = payload.ReadSelection();
            storage = new OriginalNv2Storage(device, () => payload.Bytes, Load, logRecovery);
            storage.Reload();
        }
        public static OriginalHandpaySelectionPersistence OpenFile(string path, byte[] initialPayload, Action<string> logRecovery)
        {
            new OriginalCreditOutNvPayload(initialPayload); // Admit exact layout before opening a file.
            var device = new OriginalNvFileDevice(path);
            try
            {
                var result = new OriginalHandpaySelectionPersistence(device, initialPayload, logRecovery);
                result.ownedFile = device; return result;
            }
            catch { device.Dispose(); throw; }
        }
        private void Load(byte[] bytes)
        {
            payload = new OriginalCreditOutNvPayload(bytes);
            CopyState(payload.ReadSelection());
        }
        private void CopyState(OriginalHandpaySelectionNvState source)
        { State.InProgress = source.InProgress; State.ResidualSubstate = source.ResidualSubstate; State.HandpaySelectedByte = source.HandpaySelectedByte; }
        public long SelectionAmount { get { return payload.SelectionAmount; } }
        internal OriginalCreditOutNvPayload CurrentPayload { get { return payload; } }
        internal void SaveCurrentPayload() { CopyState(payload.ReadSelection());storage.Save(); }
        public byte[] PayloadBytes { get { return payload.Bytes; } }
        public byte[] ReadDeviceImage() { return storage.ReadDeviceImage(); }
        public void Save(OriginalHandpaySelectionNvState source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            payload.UpdateSelection(source); CopyState(source); storage.Save();
        }
        public void Reload() { storage.Reload(); }
        public void ValidateStorage() { storage.Validate(); }
        public void SoftValidateStorage(bool enabled,string context,Action<string> deviceContextLog)
        { storage.SoftValidate(enabled,context,deviceContextLog); }
        public void SetupSelectedCashOut(int type,long amount,Func<long> readCoinValue,Func<long> readLimit)
        { OriginalCreditOutCashoutSetup.Apply(payload,type,amount,readCoinValue,readLimit,storage.Save); }
        public int SelectAndSetupCashOut(OriginalCreditOutCashoutSelection.Host host,bool hopperPrimary,long amount,object originalVoucher)
        {
            int type=hopperPrimary?OriginalCreditOutCashoutSelection.HopperPrimary(host,amount,originalVoucher)
                :OriginalCreditOutCashoutSelection.PrinterPrimary(host,amount,originalVoucher);
            SetupSelectedCashOut(type,amount,()=>host.MachineAmount(0xb4),()=>host.MachineAmount(0x148));return type;
        }
        public bool Collect(OriginalCreditOutCollect.Runtime runtime,OriginalCreditOutCollect.Host host,long amount,bool forced,object originalVoucher)
        { return OriginalCreditOutCollect.Apply(this,runtime,host,amount,forced,originalVoucher); }
        public bool CashOutState(OriginalCreditOutCollect.Runtime runtime,OriginalCreditOutCashoutState.Host host)
        { return OriginalCreditOutCashoutState.Apply(this,runtime,host); }
        public bool CashOutVoucherState(OriginalCreditOutCollect.Runtime runtime,OriginalCreditOutCashoutVoucherState.Host host)
        { return OriginalCreditOutCashoutVoucherState.Apply(this,runtime,host); }
        public bool CashOutContinueState(OriginalCreditOutCollect.Runtime runtime,OriginalCreditOutCashoutContinueState.Host host)
        { return OriginalCreditOutCashoutContinueState.Apply(this,runtime,host); }
        public bool ProcessResidualState(OriginalCreditOutCollect.Runtime runtime,OriginalCreditOutResidualState.Host host)
        { return OriginalCreditOutResidualState.Apply(this,runtime,host); }
        public OriginalHandpaySelectionCoordinator CreateCoordinator(OriginalHandpaySelectionCoordinator.Host operations)
        {
            if (operations == null) throw new ArgumentNullException(nameof(operations));
            if (coordinator != null) throw new InvalidOperationException("One selection coordinator per NV owner");
            if (operations.Save != null) throw new ArgumentException("Persistence owns the selection save operation");
            var bound = new OriginalHandpaySelectionCoordinator.Host
            {
                BeforeResidualSelection = operations.BeforeResidualSelection,
                SetTransactionInProgress = operations.SetTransactionInProgress,
                PrepareScreen = operations.PrepareScreen,
                ReleaseScreenAndTiltEvents = operations.ReleaseScreenAndTiltEvents,
                MachineDisabled = operations.MachineDisabled,
                ResidualGamble = operations.ResidualGamble,
                AfterResidualGamble = operations.AfterResidualGamble,
                EmitResidualEvent = operations.EmitResidualEvent,
                Save = Save
            };
            coordinator = new OriginalHandpaySelectionCoordinator(State, bound); return coordinator;
        }
        public void Dispose() { if (ownedFile != null) { ownedFile.Dispose(); ownedFile = null; } }
    }
}
