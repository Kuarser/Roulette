using CryPrinter;
using SBoxApi;
using SkiaSharp;
using UnityEngine;
public class PhoenixPrinterManager : BaseManager<PhoenixPrinterManager>
{
    private PhoenixPrinter phoenixPrinter;
    private const string PORT = "/dev/ttyS1";
    public void Init()
    {

        phoenixPrinter ??= new PhoenixPrinter(PORT);
        phoenixPrinter.Reinitialize();
    }

    /// <summary>
    /// Gets the current printer state.
    /// </summary>
    /// <returns>
    /// 0: The printer is ready;
    /// -1: The printer has not been set or initialized;
    /// -2: The printer has no response, is offline, or its status could not be read;
    /// -3: The printer has reported an error;
    /// -4: The printer has no paper;
    /// -5: The printer paper level is low.
    /// </returns>
    public int PrinterState()
    {
        int printerState = SBoxSandbox.PrinterState();
        switch (printerState)
        {
            case -1:
                Debug.LogError("Printer had not been set");
                return -1;
            case -2:
                Debug.LogError("Printer had no response");
                return -2;
            default:
                break;
        }

        if (phoenixPrinter == null)
        {
            Debug.LogError("Printer had not been initialized");
            return -1;
        }

        StatusReport statusReport = phoenixPrinter.GetStatus(StatusTypes.OfflineStatus);
        if (statusReport == null || statusReport.IsInvalidReport)
        {
            Debug.LogError("Failed to get printer status");
            return -2;
        }
        if (!statusReport.IsOnline)
        {
            Debug.LogError("Printer is offline");
            return -2;
        }
        if (statusReport.HasError)
        {
            Debug.LogError("statusReport.HasError");
            return -3;
        }
        if (!statusReport.IsPaperPresent)
        {
            Debug.LogError("statusReport.IsPaperPresent");
            return -4;
        }
        if (!statusReport.IsPaperLevelOkay)
        {
            Debug.LogError("statusReport.IsPaperLevelOkay");
            return -5;
        }
        Debug.Log("Printer is connected");
        return 0;
    }

    /// <summary>
    /// Checks the printer state and prints a test ticket.
    /// </summary>
    /// <returns>True if the printer state is normal and the test ticket was sent successfully.</returns>
    public bool TestPrinter()
    {
        if (phoenixPrinter == null)
        {
            Init();
        }

        int printerState = PrinterState();
        if (printerState != 0)
        {
            Debug.LogError($"Printer test failed, state code: {printerState}");
            return false;
        }

        StandardDocument document = new StandardDocument
        {
            CodePage = CodePages.CPSPACE,
        };

        document.Sections.Add(new StandardSection
        {
            Content = "=== PHOENIX PRINTER TEST ===\nPrinter is available.",
            Justification = FontJustification.JustifyLeft,
            HeightScalar = FontHeighScalar.h1,
            WidthScalar = FontWidthScalar.w1,
            Effects = FontEffects.None,
            Font = ThermalFonts.C,
            AutoNewline = true,
        });

        Texture2D tex = ZXingQrCode.GenerateQRImageWithColor("https://www.bilibili.com/", 256, 256, Color.black);

            var qrCodeBitmap = SKBitmap.Decode(tex.EncodeToPNG());
            var printerImage = new PrinterImage(qrCodeBitmap);
            phoenixPrinter.SetImage(printerImage, document, 4);

        ReturnCode result = phoenixPrinter.PrintDocument(document);
        if (result != ReturnCode.Success)
        {
            Debug.LogError($"Failed to create printer test ticket: {result}");
            return false;
        }

        result = phoenixPrinter.FormFeed();
        if (result != ReturnCode.Success)
        {
            Debug.LogError($"Failed to print test ticket: {result}");
            return false;
        }

        Debug.Log("Printer test succeeded");
        return true;
    }

    /// <summary>
    /// 打印退分小票:标题与退币数最大字号(金额行居中),时间小号,订单号与地址中号。
    /// </summary>
    /// <returns>True if the printer state is normal and the ticket was sent successfully.</returns>
    public bool PrintRefundTicket(string orderId, string address, string time, int coinCount)
    {
        if (phoenixPrinter == null)
        {
            Init();
        }

        int printerState = PrinterState();
        if (printerState != 0)
        {
            Debug.LogError($"Printer refund ticket failed, state code: {printerState}");
            return false;
        }

        StandardDocument document = new StandardDocument
        {
            CodePage = CodePages.CPSPACE,
        };

        //标题:最大字号,居中
        document.Sections.Add(new StandardSection
        {
            Content = "PAYOUT TICKET\n\n",
            Justification = FontJustification.JustifyCenter,
            HeightScalar = FontHeighScalar.h1,
            WidthScalar = FontWidthScalar.w1,
            Effects = FontEffects.None,
            Font = ThermalFonts.B,
            AutoNewline = true,
        });
        //退币数:最大字号,居中
        document.Sections.Add(new StandardSection
        {
            Content = $"${coinCount}",
            Justification = FontJustification.JustifyCenter,
            HeightScalar = FontHeighScalar.h1,
            WidthScalar = FontWidthScalar.w1,
            Effects = FontEffects.Bold,
            Font = ThermalFonts.B,
            AutoNewline = true,
        });
        //退币数英文读法:随金额居中(B字体)
        document.Sections.Add(new StandardSection
        {
            Content = NumberToEnglish.Convert(coinCount)+"\n\n",
            Justification = FontJustification.JustifyCenter,
            HeightScalar = FontHeighScalar.h1,
            WidthScalar = FontWidthScalar.w1,
            Effects = FontEffects.Bold,
            Font = ThermalFonts.B,
            AutoNewline = true,
        });
        //时间:小字号
        document.Sections.Add(new StandardSection
        {
            Content = time+"\n",
            Justification = FontJustification.JustifyLeft,
            HeightScalar = FontHeighScalar.h1,
            WidthScalar = FontWidthScalar.w1,
            Effects = FontEffects.None,
            Font = ThermalFonts.C,
            AutoNewline = true,
        });
        //订单号:中字号
        document.Sections.Add(new StandardSection
        {
            Content = $"Order No.:{orderId}\n",
            Justification = FontJustification.JustifyLeft,
            HeightScalar = FontHeighScalar.h1,
            WidthScalar = FontWidthScalar.w1,
            Effects = FontEffects.None,
            Font = ThermalFonts.C,
            AutoNewline = true,
        });
        //地址:中字号
        document.Sections.Add(new StandardSection
        {
            Content = $"Address:{address}",
            Justification = FontJustification.JustifyLeft,
            HeightScalar = FontHeighScalar.h1,
            WidthScalar = FontWidthScalar.w1,
            Effects = FontEffects.None,
            Font = ThermalFonts.C,
            AutoNewline = true,
        });

        ReturnCode result = phoenixPrinter.PrintDocument(document);
        if (result != ReturnCode.Success)
        {
            Debug.LogError($"Failed to print refund ticket: {result}");
            return false;
        }

        result = phoenixPrinter.FormFeed();
        if (result != ReturnCode.Success)
        {
            Debug.LogError($"Failed to feed refund ticket: {result}");
            return false;
        }

        Debug.Log("Refund ticket printed");
        return true;
    }
}
