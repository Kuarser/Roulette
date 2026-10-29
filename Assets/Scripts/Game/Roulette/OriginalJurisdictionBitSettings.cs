using System;
namespace RouletteDeluxe
{
 public sealed class OriginalJurisdictionBitSettings
 {
  public sealed class Host{public Func<uint,uint> ReadWord;public Action<uint,byte> WriteByte;public Func<uint,uint,uint> GetOptionBit;public Func<uint,uint,uint,uint> ReadBit;}
  private struct Entry{internal uint va,offset;internal bool invert;internal Entry(uint v,uint o,bool i){va=v;offset=o;invert=i;}}
  public const int CStringCount=53;
  private static readonly Entry[] Entries={
   new Entry(0x836cf90u,0x25cu,false), // DeviceFirmwareUpgradeEnabled
   new Entry(0x836d090u,0x259u,false), // ALCStandardsEnabled
   new Entry(0x836d690u,0x226u,false), // OnScreenClockRequired
   new Entry(0x836d7d0u,0x21au,false), // KeyedOnCreditMaxLimitEnabled
   new Entry(0x836d850u,0x258u,false), // DisableYieldSchedulerGameConfiguration
   new Entry(0x836d8d0u,0x204u,false), // StopAllReelsRacksWin
   new Entry(0x836e090u,0x1cfu,true), // EthernetDisabled
   new Entry(0x836e0d0u,0x1ceu,false), // DisableOnProgressiveBroadcastOffline
   new Entry(0x836e110u,0x1cdu,false), // SuperProgressivesEnabled
   new Entry(0x836e150u,0x1ccu,false), // ProgressivesDisabled
   new Entry(0x836e1d0u,0x1cau,false), // RemoteConfigurationEnabled
   new Entry(0x836e250u,0x1c8u,false), // TournamentDisabled
   new Entry(0x836e310u,0x25au,false), // TicketPrintAbortOnError
   new Entry(0x836e350u,0x1b9u,false), // OSControlledAttractMode
   new Entry(0x836e390u,0x1b8u,false), // BillDenomConfigMenu
   new Entry(0x836e3d0u,0x1b7u,false), // FullCashoutOnCreditLimitExceed
   new Entry(0x836e450u,0x1b4u,false), // AccessAttendantMenuDuringGamePlay
   new Entry(0x836e490u,0x1b3u,false), // LastTicketReprintEnabled
   new Entry(0x836e4d0u,0x1b2u,false), // ChangeLocaleForSelectedLanguage
   new Entry(0x836e550u,0x1c5u,false), // DownloadEnabled
   new Entry(0x836e5d0u,0x1b1u,false), // DisableOnStartup
   new Entry(0x836e610u,0x1a8u,false), // BankMeterAddLimitEnabled
   new Entry(0x836e650u,0x1a0u,false), // BankMeterTransferLimitEnabled
   new Entry(0x836e6d0u,0x19eu,false), // BetMeterCashDisplayAllowed
   new Entry(0x836e710u,0x1b0u,false), // DisableOnCommLoss
   new Entry(0x836e750u,0x19du,false), // MetersInEeprom
   new Entry(0x836e810u,0x191u,false), // HardMetersInCredits
   new Entry(0x836e890u,0x129u,false), // WinDropEnabled
   new Entry(0x836e8d0u,0x128u,false), // CollectDropEnabled
   new Entry(0x836e910u,0x127u,false), // FrenchEscrowedMetering
   new Entry(0x836e990u,0x125u,false), // HardMeterCanceledCreditsEnabled
   new Entry(0x836ea10u,0xfeu,false), // BillToTrayEnabled
   new Entry(0x836ea50u,0xfdu,false), // CoinAcceptorEnabled
   new Entry(0x836ea90u,0xfcu,false), // PlayButtonHoldDownDefaultEnabled
   new Entry(0x836eb10u,0xfau,false), // AlternateMechDisplayEnabled
   new Entry(0x836eb50u,0xf9u,false), // AutopickEnabled
   new Entry(0x836eb90u,0xf8u,false), // SecureEnableOfMoneyTransferEnabled
   new Entry(0x836ec10u,0xf5u,false), // HardMeter6Enabled
   new Entry(0x836ec50u,0xf0u,false), // InhibitPlayerCollect
   new Entry(0x836ec90u,0xf4u,false), // PrinterEnabled
   new Entry(0x836ed10u,0xf2u,false), // HandpayEnabled
   new Entry(0x836ed50u,0xf1u,false), // ProgressiveManualJackpotRequired
   new Entry(0x836f1e0u,0xe3u,false), // ForceTiltOnIllegalGamePercentagesEnabled
   new Entry(0x836f260u,0xe1u,false), // MaxNonProgressiveWinLimitEnabled
   new Entry(0x836f2e0u,0xdfu,false), // MaxWinLimitEnabled
   new Entry(0x836f360u,0xddu,false), // MaxLinesLimitEnabled
   new Entry(0x836f460u,0xd9u,false), // MaxBetLimitEnabled
   new Entry(0x836f560u,0xd5u,false), // PercentagesPerDenomEnabled
   new Entry(0x836f660u,0xd1u,false), // HandpayResidualCreditEnabled
   new Entry(0x836f760u,0xcdu,false), // RestrictedPromoAccountEnabled
   new Entry(0x836f860u,0xc9u,false), // SystemCashlessInEnabled
   new Entry(0x836f960u,0xc5u,false), // AFTInhouseToGameEnabled
   new Entry(0x836fa60u,0xc1u,false), // ECTInEnabled
  };
  private readonly Host host;private readonly uint[] names;
  public OriginalJurisdictionBitSettings(Host h,uint[] relocatedCStrings)
  {
   if(h==null||h.ReadWord==null||h.WriteByte==null||h.GetOptionBit==null||h.ReadBit==null||relocatedCStrings==null||relocatedCStrings.Length!=CStringCount)throw new ArgumentException("Actual dedicated bit settings hosts and all relocated CStrings required");
   foreach(uint name in relocatedCStrings)if(name==0)throw new ArgumentException("Actual dedicated setting CString required");
   host=new Host{ReadWord=h.ReadWord,WriteByte=h.WriteByte,GetOptionBit=h.GetOptionBit,ReadBit=h.ReadBit};names=(uint[])relocatedCStrings.Clone();
  }
  public void Read(uint originalFunctionVa,uint machine)
  {
   int index=-1;for(int i=0;i<Entries.Length;++i)if(Entries[i].va==originalFunctionVa){index=i;break;}
   if(index<0)throw new ArgumentOutOfRangeException(nameof(originalFunctionVa));if(machine==0)throw new ArgumentException("Live jurisdiction owner required");
   var entry=Entries[index];uint pair=host.GetOptionBit(machine,names[index]);if(pair==0)return;
   uint bit=host.ReadWord(unchecked(pair+4)),offset=host.ReadWord(pair),value=host.ReadBit(machine,offset,bit);if(entry.invert)value^=1;host.WriteByte(unchecked(machine+entry.offset),unchecked((byte)value));
  }
 }
}
