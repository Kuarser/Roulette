using System;
namespace RouletteDeluxe
{
 // Original Read registration region 0837079e..08372a7e; lower owners are admitted.
 public sealed class OriginalJurisdictionReadRegistration
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord;
   public Action<uint,ushort> WriteShort;
   public Action<uint,byte> WriteByte;
   public Func<uint,uint,uint> GetOption,GetOptionBit,ReadInteger,ReadByte,ReadShort;
   public Func<uint,uint,ulong> ReadInteger64;
   public Func<uint,uint,uint,uint> ReadBit;
   public Action<uint,uint> InvokeStep;
   public Action<uint,uint,uint> InvokeIndexed;
  }
  public const int CStringCount=184;
  private enum Kind{Integer,SignInteger64,Integer64,Short,Byte,Bit,RegisterOnly,Helper,Range}
  private struct Entry{internal Kind kind;internal uint operation,offset;internal int slot;internal Entry(Kind k,uint op,int s,uint o){kind=k;operation=op;slot=s;offset=o;}}
  private static readonly Entry[] Entries={
   new Entry(Kind.Integer,0,0,0x120u), // MeterConfig @ 0837079e
   new Entry(Kind.Integer,0,1,0x1acu), // BankMeterAddLimit @ 083707c6
   new Entry(Kind.Integer,0,2,0x1a4u), // BankMeterTransferLimit @ 083707ee
   new Entry(Kind.SignInteger64,0,3,0x194u), // DefaultHardMeterIncrement @ 08370816
   new Entry(Kind.Integer,0,4,0x68u), // MinReelSpinMs @ 08370849
   new Entry(Kind.Integer,0,5,0x8cu), // MaxLinesLimit @ 0837086e
   new Entry(Kind.Integer,0,6,0x88u), // MinLinesLimit @ 08370896
   new Entry(Kind.Integer,0,7,0x84u), // MaxBPLLimit @ 083708be
   new Entry(Kind.Integer,0,8,0x80u), // MinBPLLimit @ 083708e6
   new Entry(Kind.Integer,0,9,0x60u), // DoubleUpChancesLimit @ 0837090e
   new Entry(Kind.Integer,0,10,0x64u), // DoubleUpBetLimit @ 08370933
   new Entry(Kind.Integer,0,11,0x7cu), // MaxBetLimit @ 08370958
   new Entry(Kind.Helper,0x836a260u,0,0), // _ZN15JurisdictionMgr15ReadMaxWinLimitEv
   new Entry(Kind.Integer64,0,12,0x9cu), // MaxProgressiveWinLimit @ 08370985
   new Entry(Kind.Integer64,0,13,0xa4u), // MaxNonProgressiveWinLimit @ 083709b3
   new Entry(Kind.Integer64,0,14,0xacu), // GameEventWinLimit @ 083709e1
   new Entry(Kind.Integer,0,15,0x6cu), // MaxTaxLimit @ 08370a0f
   new Entry(Kind.Bit,0,16,0x51u), // UpgradeWithoutRamClearDisabled @ 08370a34
   new Entry(Kind.Bit,0,17,0x50u), // A2VerifyLabelsV1Enabled @ 08370a60
   new Entry(Kind.Helper,0x836a650u,0,0), // _ZN15JurisdictionMgr19ReadMaxPrinterLimitEv
   new Entry(Kind.Integer,0,18,0x74u), // MaxHopperLimit @ 08370a94
   new Entry(Kind.Bit,0,19,0x55u), // DisableOutOfServiceOnNonZeroCredits @ 08370ab9
   new Entry(Kind.Integer,0,20,0xb4u), // MaxTokenValue @ 08370ae5
   new Entry(Kind.Integer,0,21,0x78u), // MaxCashLimit @ 08370b0d
   new Entry(Kind.Integer,0,22,0x100u), // MaxCoinLimit @ 08370b32
   new Entry(Kind.Integer,0,23,0x10cu), // AFTMaxTransferLimit @ 08370b5a
   new Entry(Kind.Integer,0,24,0x118u), // AFTMustTransferWinLimit @ 08370b82
   new Entry(Kind.Integer,0,25,0x114u), // JCMCountryCode @ 08370baa
   new Entry(Kind.Helper,0x836ae50u,0,0), // _ZN15JurisdictionMgr10ReadLocaleEv
   new Entry(Kind.Byte,0,26,0x1ddu), // GTechProtocolSelection @ 08370bda
   new Entry(Kind.Integer,0,27,0x1e8u), // MaxCreditLimit @ 08370c02
   new Entry(Kind.Integer,0,28,0x208u), // SecsToAutoCompleteStart @ 08370c2a
   new Entry(Kind.Integer,0,29,0x20cu), // SecsToAutoCompleteWarning @ 08370c52
   new Entry(Kind.Bit,0,30,0x1f4u), // MachineControlPrimaryOnly @ 08370c7a
   new Entry(Kind.Helper,0x836acd0u,0,0), // _ZN15JurisdictionMgr22ReadAutoConfigFileNameEv
   new Entry(Kind.Helper,0x8369f20u,0,0), // _ZN15JurisdictionMgr27ReadMaximumLossPerHourLimitEv
   new Entry(Kind.Bit,0,31,0x218u), // CashableJackpotVouchers @ 08370cb9
   new Entry(Kind.Integer,0,32,0x110u), // AFTMaxTransferLimitFromGame @ 08370ce8
   new Entry(Kind.SignInteger64,0,33,0x21cu), // KeyedOnCreditMaxLimit @ 08370d10
   new Entry(Kind.Helper,0x836add0u,0,0), // _ZN15JurisdictionMgr15ReadLookAndFeelEv
   new Entry(Kind.Helper,0x836d8d0u,0,0), // _ZN15JurisdictionMgr24ReadStopAllReelsRacksWinEv
   new Entry(Kind.Bit,0,34,0x257u), // DisableYieldScheduler @ 08370d53
   new Entry(Kind.Helper,0x836d850u,0,0), // _ZN15JurisdictionMgr42ReadDisableYieldSchedulerGameConfigurationEv
   new Entry(Kind.Bit,0,35,0x1f7u), // SuppressDateTimeTiltEnabled @ 08370d8a
   new Entry(Kind.Bit,0,36,0x1f5u), // GameEventsInLogDisabled @ 08370db9
   new Entry(Kind.Bit,0,37,0x1e6u), // VoucherOutLogDisabled @ 08370de8
   new Entry(Kind.Bit,0,38,0x1ecu), // AutoConfigureEnabled @ 08370e17
   new Entry(Kind.Bit,0,39,0x207u), // ShowCountdown @ 08370e46
   new Entry(Kind.Bit,0,40,0x206u), // AutoCompleteEnabled @ 08370e75
   new Entry(Kind.Bit,0,41,0x202u), // MenuAccessibleFromSingleGame @ 08370ea4
   new Entry(Kind.Bit,0,42,0x203u), // DisplayMeterInCashInMenu @ 08370ed3
   new Entry(Kind.Bit,0,43,0x201u), // ShowDenomsOnDisabledMenuIcons @ 08370f02
   new Entry(Kind.Bit,0,44,0x200u), // TouchDenomDisabled @ 08370f31
   new Entry(Kind.Bit,0,45,0x1ffu), // DrawAllPokerCardsAtOnce @ 08370f60
   new Entry(Kind.Bit,0,46,0x1feu), // 12HourClockDisplayed @ 08370f8f
   new Entry(Kind.Bit,0,47,0x1fdu), // CollectFromMenuOnly @ 08370fbe
   new Entry(Kind.Bit,0,48,0x1fcu), // DefaultDisplayMeterInCash @ 08370fed
   new Entry(Kind.Bit,0,49,0x1fbu), // ShowAllMenuIcons @ 0837101c
   new Entry(Kind.Bit,0,50,0x1fau), // RebetWarning @ 0837104b
   new Entry(Kind.Bit,0,51,0x1f9u), // ResetBetOnGameChange @ 0837107a
   new Entry(Kind.Bit,0,52,0x1f8u), // SingleButtonWagering @ 083710a9
   new Entry(Kind.Bit,0,53,0x1e5u), // PrintAuditTicket @ 083710d8
   new Entry(Kind.Bit,0,54,0x1e4u), // AllDoorsSoundWarningEnabled @ 08371107
   new Entry(Kind.Bit,0,55,0x1e2u), // ExtendedBillValEvents @ 08371136
   new Entry(Kind.Bit,0,56,0x1e3u), // GameHistoryPrintOptionEnabled @ 08371165
   new Entry(Kind.Bit,0,57,0x1e1u), // GameEndNVRAMValidation @ 08371194
   new Entry(Kind.Bit,0,58,0x1dcu), // OperatorDemonstrationModeEnabled @ 083711c3
   new Entry(Kind.Bit,0,59,0x1e0u), // Clock1970Default @ 083711f2
   new Entry(Kind.Bit,0,60,0x1dfu), // AllWinsToCredit @ 08371221
   new Entry(Kind.Bit,0,61,0x1deu), // LogSoftwareAuthentication @ 08371250
   new Entry(Kind.Bit,0,62,0x225u), // XSeriesProtocolEnabled @ 0837127f
   new Entry(Kind.Helper,0x836d690u,0,0), // _ZN15JurisdictionMgr25ReadOnScreenClockRequiredEv
   new Entry(Kind.Bit,0,63,0x219u), // KeyedOnCreditEnabled @ 083712b6
   new Entry(Kind.Helper,0x836d7d0u,0,0), // _ZN15JurisdictionMgr32ReadKeyedOnCreditMaxLimitEnabledEv
   new Entry(Kind.Helper,0x836e610u,0,0), // _ZN15JurisdictionMgr28ReadBankMeterAddLimitEnabledEv
   new Entry(Kind.Helper,0x836e6d0u,0,0), // _ZN15JurisdictionMgr30ReadBetMeterCashDisplayAllowedEv
   new Entry(Kind.Bit,0,64,0x19fu), // BankMeterEnabled @ 083712fd
   new Entry(Kind.Helper,0x836e650u,0,0), // _ZN15JurisdictionMgr33ReadBankMeterTransferLimitEnabledEv
   new Entry(Kind.Helper,0x836e810u,0,0), // _ZN15JurisdictionMgr23ReadHardMetersInCreditsEv
   new Entry(Kind.Bit,0,65,0x190u), // DoubleUpEnabled @ 0837133c
   new Entry(Kind.Helper,0x836e890u,0,0), // _ZN15JurisdictionMgr18ReadWinDropEnabledEv
   new Entry(Kind.Helper,0x836e8d0u,0,0), // _ZN15JurisdictionMgr22ReadCollectDropEnabledEv
   new Entry(Kind.Helper,0x836e910u,0,0), // _ZN15JurisdictionMgr26ReadFrenchEscrowedMeteringEv
   new Entry(Kind.Bit,0,66,0x126u), // MachineAccountingInBaseDenomUnits @ 08371383
   new Entry(Kind.Bit,0,67,0xd8u), // DoubleUpBetLimitEnabled @ 083713b2
   new Entry(Kind.Bit,0,68,0xd7u), // DoubleUpChancesLimitEnabled @ 083713e1
   new Entry(Kind.Bit,0,69,0xe2u), // AutoPlayEnabled @ 08371410
   new Entry(Kind.Helper,0x836f360u,0,0), // _ZN15JurisdictionMgr24ReadMaxLinesLimitEnabledEv
   new Entry(Kind.Bit,0,70,0xdcu), // MinLinesLimitEnabled @ 08371447
   new Entry(Kind.Bit,0,71,0xdbu), // MaxBPLLimitEnabled @ 08371476
   new Entry(Kind.Bit,0,72,0xdau), // MinBPLLimitEnabled @ 083714a5
   new Entry(Kind.Helper,0x836ec90u,0,0), // _ZN15JurisdictionMgr18ReadPrinterEnabledEv
   new Entry(Kind.Bit,0,73,0xf3u), // HopperEnabled @ 083714dc
   new Entry(Kind.Helper,0x836ed10u,0,0), // _ZN15JurisdictionMgr18ReadHandpayEnabledEv
   new Entry(Kind.Helper,0x836ec50u,0,0), // _ZN15JurisdictionMgr24ReadInhibitPlayerCollectEv
   new Entry(Kind.Helper,0x836ed50u,0,0), // _ZN15JurisdictionMgr36ReadProgressiveManualJackpotRequiredEv
   new Entry(Kind.Bit,0,74,0x192u), // HardMeterSetupEnabled @ 08371523
   new Entry(Kind.Bit,0,75,0x19cu), // DefaultHardMeterIncrementEnabled @ 08371552
   new Entry(Kind.Helper,0x836e750u,0,0), // _ZN15JurisdictionMgr18ReadMetersInEepromEv
   new Entry(Kind.Helper,0x836e990u,0,0), // _ZN15JurisdictionMgr35ReadHardMeterCanceledCreditsEnabledEv
   new Entry(Kind.Bit,0,76,0x124u), // HardMeterGamesPlayedEnabled @ 08371591
   new Entry(Kind.Bit,0,77,0x48u), // BillIncrementSoftDropMeterEnabled @ 083715c0
   new Entry(Kind.Bit,0,78,0x4eu), // BillIncrementHardDropMeterEnabled @ 083715ec
   new Entry(Kind.Helper,0x836ec10u,0,0), // _ZN15JurisdictionMgr21ReadHardMeter6EnabledEv
   new Entry(Kind.Bit,0,79,0x4cu), // HardMeterJackpotEnabled @ 08371620
   new Entry(Kind.Bit,0,80,0x4du), // HardMeterBillInEnabled @ 0837164c
   new Entry(Kind.Bit,0,81,0x4bu), // HardMeterCoinDropEnabled @ 08371678
   new Entry(Kind.Bit,0,82,0x4au), // HardMeterCoinOutEnabled @ 083716a4
   new Entry(Kind.Bit,0,83,0x49u), // HardMeterCoinInEnabled @ 083716d0
   new Entry(Kind.Range,0x836ed90u,0,0), // _ZN15JurisdictionMgr22ReadKenoPercentEnabledEt
   new Entry(Kind.Range,0x836ef00u,0,0), // _ZN15JurisdictionMgr23ReadSlotsPercentEnabledEt
   new Entry(Kind.Range,0x836f070u,0,0), // _ZN15JurisdictionMgr23ReadPokerPercentEnabledEt
   new Entry(Kind.Bit,0,84,0x210u), // DisallowDateTimeResettable @ 0837174c
   new Entry(Kind.Bit,0,85,0x1c4u), // MonitorDropDoor @ 0837177b
   new Entry(Kind.Helper,0x836e550u,0,0), // _ZN15JurisdictionMgr19ReadDownloadEnabledEv
   new Entry(Kind.Bit,0,86,0x1c6u), // PUIEnabled @ 083717b2
   new Entry(Kind.Bit,0,87,0x1c7u), // DownloadInstallRestriction @ 083717e1
   new Entry(Kind.Helper,0x836e4d0u,0,0), // _ZN15JurisdictionMgr35ReadChangeLocaleForSelectedLanguageEv
   new Entry(Kind.Helper,0x836cf90u,0,0), // _ZN15JurisdictionMgr32ReadDeviceFirmwareUpgradeEnabledEv
   new Entry(Kind.Helper,0x836e490u,0,0), // _ZN15JurisdictionMgr28ReadLastTicketReprintEnabledEv
   new Entry(Kind.Helper,0x836e450u,0,0), // _ZN15JurisdictionMgr37ReadAccessAttendantMenuDuringGamePlayEv
   new Entry(Kind.Bit,0,88,0x1b5u), // BVMessageOnPlayerScreen @ 08371830
   new Entry(Kind.Bit,0,89,0x1b6u), // FreeGamesAsSeparateGames @ 0837185f
   new Entry(Kind.Helper,0x836e250u,0,0), // _ZN15JurisdictionMgr22ReadTournamentDisabledEv
   new Entry(Kind.Bit,0,90,0x1c9u), // WriteableMediaEnabled @ 08371896
   new Entry(Kind.Helper,0x836e1d0u,0,0), // _ZN15JurisdictionMgr30ReadRemoteConfigurationEnabledEv
   new Entry(Kind.Bit,0,91,0x1cbu), // ResetableDenomsEnabled @ 083718cd
   new Entry(Kind.Helper,0x836e150u,0,0), // _ZN15JurisdictionMgr24ReadProgressivesDisabledEv
   new Entry(Kind.Helper,0x836e110u,0,0), // _ZN15JurisdictionMgr28ReadSuperProgressivesEnabledEv
   new Entry(Kind.Helper,0x836e0d0u,0,0), // _ZN15JurisdictionMgr40ReadDisableOnProgressiveBroadcastOfflineEv
   new Entry(Kind.Helper,0x836e090u,0,0), // _ZN15JurisdictionMgr20ReadEthernetDisabledEv
   new Entry(Kind.Bit,0,92,0x1d0u), // DHCPDisabled @ 0837191c
   new Entry(Kind.Bit,0,93,0x1d1u), // GameChangeNvramClearEnabled @ 0837194b
   new Entry(Kind.RegisterOnly,0,94,0x0u), // (Obsolete)G2SEnabled @ 0837197a
   new Entry(Kind.Helper,0x83705b0u,0,0), // _ZN15JurisdictionMgr16ReadRNGSelectionEv
   new Entry(Kind.Helper,0x83704f0u,0,0), // _ZN15JurisdictionMgr30ReadPlayerInfoDisplaySelectionEv
   new Entry(Kind.Bit,0,95,0x1e7u), // MaxCreditLimitEnabled @ 0837199a
   new Entry(Kind.Bit,0,96,0xf6u), // MaxCoinLimitEnabled @ 083719c9
   new Entry(Kind.Bit,0,97,0x56u), // MaxCashLimitEnabled @ 083719f8
   new Entry(Kind.Bit,0,98,0x4fu), // MaxTaxLimitEnabled @ 08371a24
   new Entry(Kind.Bit,0,99,0x52u), // BillLimitToPrinterHopperEnabled @ 08371a50
   new Entry(Kind.Bit,0,100,0x54u), // PrinterMaxLimitEnabled @ 08371a7c
   new Entry(Kind.Bit,0,101,0xf7u), // MultiplePayWinEnabled @ 08371aa8
   new Entry(Kind.Bit,0,102,0x53u), // MaxHopperLimitEnabled @ 08371ad7
   new Entry(Kind.Helper,0x836f1e0u,0,0), // _ZN15JurisdictionMgr37ReadForceTiltOnIllegalGamePercentagesEv
   new Entry(Kind.Helper,0x836eb90u,0,0), // _ZN15JurisdictionMgr38ReadSecureEnableOfMoneyTransferEnabledEv
   new Entry(Kind.Helper,0x836f2e0u,0,0), // _ZN15JurisdictionMgr22ReadMaxWinLimitEnabledEv
   new Entry(Kind.Bit,0,103,0xe0u), // MaxProgressiveWinLimitEnabled @ 08371b1b
   new Entry(Kind.Helper,0x836f260u,0,0), // _ZN15JurisdictionMgr36ReadMaxNonProgressiveWinLimitEnabledEv
   new Entry(Kind.Bit,0,104,0xdeu), // JackpotLoopEnabled @ 08371b52
   new Entry(Kind.Helper,0x836f460u,0,0), // _ZN15JurisdictionMgr22ReadMaxBetLimitEnabledEv
   new Entry(Kind.Helper,0x836eb50u,0,0), // _ZN15JurisdictionMgr19ReadAutopickEnabledEv
   new Entry(Kind.Helper,0x836eb10u,0,0), // _ZN15JurisdictionMgr31ReadAlternateMechDisplayEnabledEv
   new Entry(Kind.Helper,0x836ea90u,0,0), // _ZN15JurisdictionMgr36ReadPlayButtonHoldDownDefaultEnabledEv
   new Entry(Kind.Bit,0,105,0xfbu), // PlayButtonHoldDownConfigurableEnabled @ 08371ba1
   new Entry(Kind.Bit,0,106,0xd6u), // FractionalCreditNotAllowedEnabled @ 08371bd0
   new Entry(Kind.Helper,0x836f560u,0,0), // _ZN15JurisdictionMgr30ReadPercentagesPerDenomEnabledEv
   new Entry(Kind.Bit,0,107,0xd4u), // MultiDenomEnabled @ 08371c07
   new Entry(Kind.Bit,0,108,0xd3u), // MaxTokenValueEnabled @ 08371c36
   new Entry(Kind.Bit,0,109,0xd2u), // PlayerCancelableHandpayEnabled @ 08371c65
   new Entry(Kind.Helper,0x836f660u,0,0), // _ZN15JurisdictionMgr32ReadHandpayResidualCreditEnabledEv
   new Entry(Kind.Bit,0,110,0xd0u), // WagerGameEnabled @ 08371c9c
   new Entry(Kind.Bit,0,111,0xcfu), // TokenizationEnabled @ 08371ccb
   new Entry(Kind.Helper,0x836ea50u,0,0), // _ZN15JurisdictionMgr23ReadCoinAcceptorEnabledEv
   new Entry(Kind.Bit,0,112,0x1f6u), // SuspendOnBillValidatorError @ 08371d02
   new Entry(Kind.Bit,0,113,0x5fu), // BillDenom9Enabled @ 08371d31
   new Entry(Kind.Helper,0x836ea10u,0,0), // _ZN15JurisdictionMgr21ReadBillToTrayEnabledEv
   new Entry(Kind.Bit,0,114,0xceu), // BillValidatorEnabled @ 08371d65
   new Entry(Kind.Bit,0,115,0x5eu), // BillDenom8Enabled @ 08371d94
   new Entry(Kind.Bit,0,116,0x5du), // BillDenom100DollarEnabled @ 08371dc0
   new Entry(Kind.Bit,0,117,0x5cu), // BillDenom50DollarEnabled @ 08371dec
   new Entry(Kind.Bit,0,118,0x5bu), // BillDenom20DollarEnabled @ 08371e18
   new Entry(Kind.Bit,0,119,0x5au), // BillDenom10DollarEnabled @ 08371e44
   new Entry(Kind.Bit,0,120,0x59u), // BillDenom5DollarEnabled @ 08371e70
   new Entry(Kind.Bit,0,121,0x58u), // BillDenom2DollarEnabled @ 08371e9c
   new Entry(Kind.Bit,0,122,0x57u), // BillDenom1DollarEnabled @ 08371ec8
   new Entry(Kind.Helper,0x836f760u,0,0), // _ZN15JurisdictionMgr33ReadRestrictedPromoAccountEnabledEv
   new Entry(Kind.Bit,0,123,0xccu), // NonRestrictedPromoAccountEnabled @ 08371efc
   new Entry(Kind.Bit,0,124,0xcbu), // CashableCreditAccountEnabled @ 08371f2b
   new Entry(Kind.Bit,0,125,0xcau), // SystemCashlessOutEnabled @ 08371f5a
   new Entry(Kind.Helper,0x836f860u,0,0), // _ZN15JurisdictionMgr27ReadSystemCashlessInEnabledEv
   new Entry(Kind.Bit,0,126,0xc8u), // SDSTicketInEnabled @ 08371f91
   new Entry(Kind.Bit,0,127,0xc7u), // TicketOutExtensionsEnabled @ 08371fc0
   new Entry(Kind.Bit,0,128,0x106u), // AFTMaxTransferLimitFromGameEnabled @ 08371fef
   new Entry(Kind.Helper,0x836e5d0u,0,0), // _ZN15JurisdictionMgr20ReadDisableOnStartupEv
   new Entry(Kind.Helper,0x836e710u,0,0), // _ZN15JurisdictionMgr21ReadDisableOnCommLossEv
   new Entry(Kind.Bit,0,129,0x107u), // StringentValidationEnabled @ 0837202e
   new Entry(Kind.Bit,0,130,0x108u), // AFTMustTransferWinLimitEnabled @ 0837205d
   new Entry(Kind.Bit,0,131,0x105u), // AFTMaxTransferLimitEnabled @ 0837208c
   new Entry(Kind.Bit,0,132,0x104u), // AFTPartialTransferEnabled @ 083720bb
   new Entry(Kind.Helper,0x8369f20u,0,0), // _ZN15JurisdictionMgr27ReadMaximumLossPerHourLimitEv
   new Entry(Kind.Bit,0,133,0xc6u), // AFTInhouseFromGameEnabled @ 083720f2
   new Entry(Kind.Helper,0x836f960u,0,0), // _ZN15JurisdictionMgr27ReadAFTInhouseToGameEnabledEv
   new Entry(Kind.Bit,0,134,0xc4u), // AFTDebitTransfersEnabled @ 08372129
   new Entry(Kind.Bit,0,135,0xc3u), // AFTBonusingEnabled @ 08372158
   new Entry(Kind.Bit,0,136,0xc2u), // ECTOutEnabled @ 08372187
   new Entry(Kind.Helper,0x836fa60u,0,0), // _ZN15JurisdictionMgr16ReadECTInEnabledEv
   new Entry(Kind.Bit,0,137,0xc0u), // EFTOutEnabled @ 083721be
   new Entry(Kind.Bit,0,138,0xbfu), // EFTInEnabled @ 083721ed
   new Entry(Kind.Bit,0,139,0xbdu), // SASAddressOneTimeSet @ 0837221c
   new Entry(Kind.Bit,0,140,0xbeu), // SASTicketInEnabled @ 0837224b
   new Entry(Kind.Bit,0,141,0xbcu), // TicketOutSystemValidationEnabled @ 0837227a
   new Entry(Kind.Bit,0,142,0xbbu), // TicketOutEnhancedValidationEnabled @ 083722a9
   new Entry(Kind.Bit,0,143,0xbau), // TicketOutStandardValidationEnabled @ 083722d8
   new Entry(Kind.Bit,0,144,0xb9u), // AutobetEnabled @ 08372307
   new Entry(Kind.Bit,0,145,0xb8u), // LegacyBonusingEnabled @ 08372336
   new Entry(Kind.Bit,0,146,0x224u), // ConfigureAccountingDenomEnabled @ 08372365
   new Entry(Kind.Bit,0,147,0x227u), // MinimumBetDisplayRequired @ 08372394
   new Entry(Kind.Bit,0,148,0x228u), // VoucherRedemptionLimitEnabled @ 083723c3
   new Entry(Kind.Integer,0,149,0x22cu), // VoucherRedemptionLimit @ 083723f2
   new Entry(Kind.Bit,0,150,0x230u), // CountOfGamesToCaptureInDLEnabled @ 0837241a
   new Entry(Kind.Short,0,151,0x232u), // CountOfGamesToCaptureInDL @ 08372449
   new Entry(Kind.Bit,0,152,0x234u), // CountOfBonusRoundsToCaptureInDLEnabled @ 08372472
   new Entry(Kind.Short,0,153,0x236u), // CountOfBonusRoundsToCaptureInDL @ 083724a1
   new Entry(Kind.Bit,0,154,0x238u), // TLSCertAuthenticationRequired @ 083724ca
   new Entry(Kind.Bit,0,155,0x239u), // SuppressTiltOnDisabledBillVal @ 083724f9
   new Entry(Kind.Helper,0x836e3d0u,0,0), // _ZN15JurisdictionMgr34ReadFullCashoutOnCreditLimitExceedEv
   new Entry(Kind.Helper,0x836e390u,0,0), // _ZN15JurisdictionMgr23ReadBillDenomConfigMenuEv
   new Entry(Kind.Helper,0x836e350u,0,0), // _ZN15JurisdictionMgr27ReadOSControlledAttractModeEv
   new Entry(Kind.Integer,0,156,0x1bcu), // IdleTimeToAttractMode @ 08372540
   new Entry(Kind.Helper,0x836d090u,0,0), // _ZN15JurisdictionMgr23ReadALCStandardsEnabledEv
   new Entry(Kind.Helper,0x836e310u,0,0), // _ZN15JurisdictionMgr27ReadTicketPrintAbortOnErrorEv
   new Entry(Kind.Helper,0x836ad50u,0,0), // _ZN15JurisdictionMgr22ReadCustomLocaleSuffixEv
   new Entry(Kind.Bit,0,157,0x256u), // WCLCStandardsEnabled @ 08372580
   new Entry(Kind.Bit,0,158,0x24fu), // DoubleUpAllowedForProgWins @ 083725af
   new Entry(Kind.Bit,0,159,0x23au), // GameRecallDisplayCredits @ 083725de
   new Entry(Kind.Bit,0,160,0x23bu), // SeparateProgRTPEnabled @ 0837260d
   new Entry(Kind.Bit,0,161,0x23cu), // DisplayMinMaxRTPInAudit @ 0837263c
   new Entry(Kind.Bit,0,162,0x24cu), // DisableAllGamesOnStartup @ 0837266b
   new Entry(Kind.Bit,0,163,0x24du), // RequireEnableOnCommReconnect @ 0837269a
   new Entry(Kind.Bit,0,164,0x24eu), // DisableExternalBonusing @ 083726c9
   new Entry(Kind.Bit,0,165,0x250u), // PrintSupportForOperatorScreens @ 083726f8
   new Entry(Kind.Bit,0,166,0x251u), // ProhibitProgressiveEndOfLife @ 08372727
   new Entry(Kind.Bit,0,167,0x252u), // DisableAllSoundsOnMute @ 08372756
   new Entry(Kind.Bit,0,168,0x253u), // DisableRNGOutcomeNVRamWrite @ 08372785
   new Entry(Kind.Bit,0,169,0x254u), // DisableUSBInstallMedia @ 083727b4
   new Entry(Kind.Bit,0,170,0x255u), // DisableOnScreenGAT @ 083727e3
   new Entry(Kind.Bit,0,171,0x265u), // DisableForensicDataCapture @ 08372812
   new Entry(Kind.Helper,0x836d090u,0,0), // _ZN15JurisdictionMgr23ReadALCStandardsEnabledEv
   new Entry(Kind.Bit,0,172,0x25bu), // LouisianaLotteryStandardsEnabled @ 08372849
   new Entry(Kind.Bit,0,173,0x26du), // SouthAfricaLPMStandardsEnabled @ 08372878
   new Entry(Kind.Helper,0x836cf90u,0,0), // _ZN15JurisdictionMgr32ReadDeviceFirmwareUpgradeEnabledEv
   new Entry(Kind.Integer,0,174,0x260u), // LastTicketReprintCount @ 083728af
   new Entry(Kind.Bit,0,175,0x264u), // DoNotAllowRemovalOfGameInDMG @ 083728d7
   new Entry(Kind.Bit,0,176,0x266u), // DisallowOverrideOf100PercentCheck @ 08372906
   new Entry(Kind.Bit,0,177,0x268u), // GameRecallDisplayInitialCreditsPostBet @ 08372935
   new Entry(Kind.Bit,0,178,0x26cu), // DisableOnDoorOpen @ 08372964
   new Entry(Kind.Bit,0,179,0x267u), // TranslateAllPlayerFacingMsgs @ 08372993
   new Entry(Kind.Bit,0,180,0x26au), // GreeceVLTStandardsEnabled @ 083729c2
   new Entry(Kind.Bit,0,181,0x26bu), // IllinoisLotteryStandardsEnabled @ 083729f1
   new Entry(Kind.Bit,0,182,0x269u), // AutoDetectBillValPrinterEnabled @ 08372a20
   new Entry(Kind.Bit,0,183,0x26eu), // FullWagerCatMetersRequired @ 08372a4f
  };
  private readonly Host host;private readonly uint[] names;
  public OriginalJurisdictionReadRegistration(Host h,uint[] relocatedCStrings)
  {
   if(h==null||h.ReadWord==null||h.WriteWord==null||h.WriteShort==null||h.WriteByte==null||h.GetOption==null||h.GetOptionBit==null||h.ReadInteger==null||h.ReadByte==null||h.ReadShort==null||h.ReadInteger64==null||h.ReadBit==null||h.InvokeStep==null||h.InvokeIndexed==null||relocatedCStrings==null||relocatedCStrings.Length!=CStringCount)throw new ArgumentException("Actual Read option/scalar/settings/memory hosts and all 184 relocated CString slots required");
   foreach(uint name in relocatedCStrings)if(name==0)throw new ArgumentException("Actual relocated Read CString required");
   names=(uint[])relocatedCStrings.Clone();host=new Host{ReadWord=h.ReadWord,WriteWord=h.WriteWord,WriteShort=h.WriteShort,WriteByte=h.WriteByte,GetOption=h.GetOption,GetOptionBit=h.GetOptionBit,ReadInteger=h.ReadInteger,ReadByte=h.ReadByte,ReadShort=h.ReadShort,ReadInteger64=h.ReadInteger64,ReadBit=h.ReadBit,InvokeStep=h.InvokeStep,InvokeIndexed=h.InvokeIndexed};
  }
  private static uint A(uint a,uint b){return unchecked(a+b);}
  public void Register(uint machine)
  {
   if(machine==0)throw new ArgumentException("Live jurisdiction machine owner required");
   foreach(var entry in Entries)
   {
    if(entry.kind==Kind.Helper){host.InvokeStep(entry.operation,machine);continue;}
    if(entry.kind==Kind.Range){for(uint index=75;index<107;++index)host.InvokeIndexed(entry.operation,machine,index);continue;}
    uint pair=entry.kind==Kind.Bit||entry.kind==Kind.RegisterOnly?host.GetOptionBit(machine,names[entry.slot]):host.GetOption(machine,names[entry.slot]);
    if(entry.kind==Kind.RegisterOnly||pair==0)continue;
    uint offset,bit=0;if(entry.kind==Kind.Bit)bit=host.ReadWord(A(pair,4));offset=host.ReadWord(pair);
    uint destination=A(machine,entry.offset);
    switch(entry.kind)
    {
     case Kind.Integer:host.WriteWord(destination,host.ReadInteger(machine,offset));break;
     case Kind.SignInteger64:uint low=host.ReadInteger(machine,offset);host.WriteWord(destination,low);host.WriteWord(A(destination,4),unchecked((uint)(unchecked((int)low)>>31)));break;
     case Kind.Integer64:ulong wide=host.ReadInteger64(machine,offset);host.WriteWord(destination,(uint)wide);host.WriteWord(A(destination,4),(uint)(wide>>32));break;
     case Kind.Short:host.WriteShort(destination,unchecked((ushort)host.ReadShort(machine,offset)));break;
     case Kind.Byte:host.WriteByte(destination,unchecked((byte)host.ReadByte(machine,offset)));break;
     case Kind.Bit:host.WriteByte(destination,unchecked((byte)host.ReadBit(machine,offset,bit)));break;
    }
   }
  }
 }
}
