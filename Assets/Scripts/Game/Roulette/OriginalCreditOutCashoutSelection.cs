using System;

namespace RouletteDeluxe
{
    // Legacy081e5c40/081e5ee0. Offsets identify original interface methods;
    // callers own the actual configuration, hardware and Voucher objects.
    public static class OriginalCreditOutCashoutSelection
    {
        public delegate bool VoucherCheck(ref long amount,object originalVoucher);
        public sealed class Host
        {
            public readonly Func<int,bool> MachineFlag,JurisdictionFlag,ControlFlag;
            public readonly Func<int,long> MachineAmount;
            public readonly Func<int,long,bool> MachineCheckAmount;
            public readonly Func<VoucherCheck> GetVoucher;
            public Host(Func<int,bool> machineFlag,Func<int,long> machineAmount,
                Func<int,long,bool> machineCheckAmount,Func<int,bool> jurisdictionFlag,
                Func<int,bool> controlFlag,Func<VoucherCheck> getVoucher)
            {
                MachineFlag=machineFlag??throw new ArgumentNullException(nameof(machineFlag));
                MachineAmount=machineAmount??throw new ArgumentNullException(nameof(machineAmount));
                MachineCheckAmount=machineCheckAmount??throw new ArgumentNullException(nameof(machineCheckAmount));
                JurisdictionFlag=jurisdictionFlag??throw new ArgumentNullException(nameof(jurisdictionFlag));
                ControlFlag=controlFlag??throw new ArgumentNullException(nameof(controlFlag));
                GetVoucher=getVoucher??throw new ArgumentNullException(nameof(getVoucher));
            }
        }
        private static int VoucherOrFallback(Host h,long amount,object originalVoucher)
        {
            // SysComm+58 is called twice; the first result is only a null check.
            if(h.GetVoucher()!=null)
            {
                var voucher=h.GetVoucher();
                if(voucher==null)throw new InvalidOperationException("Voucher owner disappeared during native lookup");
                if(voucher(ref amount,originalVoucher))return 0x12;
            }
            return 0x0c;
        }
        public static int HopperPrimary(Host h,long amount,object originalVoucher)
        {
            if(h==null)throw new ArgumentNullException(nameof(h));
            if(h.MachineFlag(0x218)&&amount<=h.MachineAmount(0x10c)&&h.ControlFlag(0xfc))
            {
                if(amount%h.MachineAmount(0xb4)==0||!h.MachineFlag(0x1bc)||
                    (h.MachineCheckAmount(0x328,amount)&&h.MachineFlag(0x140)))return 0x0b;
                return 0x0c;
            }
            if(!h.MachineFlag(0x1bc)||(h.MachineCheckAmount(0x328,amount)&&h.MachineFlag(0x140)))
            {
                if(h.MachineFlag(0x218)&&h.JurisdictionFlag(0x1f8)&&h.MachineAmount(0x148)>0&&
                    h.MachineAmount(0x148)<amount&&h.ControlFlag(0xfc))return 0x10;
                return 0x0f;
            }
            return VoucherOrFallback(h,amount,originalVoucher);
        }
        public static int PrinterPrimary(Host h,long amount,object originalVoucher)
        {
            if(h==null)throw new ArgumentNullException(nameof(h));
            if(!h.MachineFlag(0x1bc)||(h.MachineCheckAmount(0x328,amount)&&h.MachineFlag(0x140)&&
                (h.JurisdictionFlag(0x5c)||(!h.JurisdictionFlag(0x42c)&&!h.JurisdictionFlag(0x428)))))
            {
                if(h.MachineFlag(0x218)&&amount<=h.MachineAmount(0x10c)&&h.ControlFlag(0xfc))return 0x0b;
                if(h.MachineFlag(0x1bc)&&
                    (!h.MachineCheckAmount(0x328,h.MachineAmount(0x148))||!h.MachineFlag(0x140))&&
                    h.JurisdictionFlag(0x1f8)&&h.MachineAmount(0x148)>0&&h.MachineAmount(0x148)<amount)return 0x19;
                if(h.MachineFlag(0x218)&&h.JurisdictionFlag(0x1f8)&&h.MachineAmount(0x148)>0&&
                    h.MachineAmount(0x148)<amount&&h.ControlFlag(0xfc))return 0x10;
                return 0x0f;
            }
            return VoucherOrFallback(h,amount,originalVoucher);
        }
    }
}
