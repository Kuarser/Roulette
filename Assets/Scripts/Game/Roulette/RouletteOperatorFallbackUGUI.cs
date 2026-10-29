using System;
using UnityEngine;
using UnityEngine.UI;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private RectTransform operatorFallbackOverlay, operatorFallbackScreen;
        private PictureLabel operatorFallbackLabel;
        private OriginalBitmapFont operatorFallbackFont;
        private bool operatorFallbackActive;
        private bool operatorFallbackIsDiagnostics;
        private bool OperatorFallbackActive { get { return operatorFallbackActive; } }

        // PC video-cabinet adapter: GameDeviceClients may select mechanical
        // or transparent reels on other cabinets. This entry selects neither.
        // Source DoHandleExitDiagnostics cleans the page before sending IPC.
        public event Action PcVideoDiagnosticsExitRequested;
        public event Action PcGameStatisticsDisplayComplete;
        public event Action PcGameStatisticsExitComplete;

        private bool CanEnterPcOperatorPage()
        {
            return !PaytableTestActive && !OperatorFallbackActive && portraitRoot != null && session != null &&
                !spinning && !session.SpinPending && !session.InDoubleUp &&
                !ResultDisplayActive && !winRackActive && !IsDoubleReveal && !IsDoubleExit &&
                !autoplay && autoRoutine == null && !pcCollectPending &&
                !serviceConfirmPending && !collectConfirmPending && !autoConfirmPending &&
                !historyView && !histogramPreview && !ideckPreview && !topperPreview &&
                paysPage < 0 && !demoMenuVisible;
        }

        private static Rect SourceOperatorRectangle(Rect normalizedArea)
        {
            if (normalizedArea.width <= 0 || normalizedArea.height <= 0 ||
                normalizedArea.xMin < 0 || normalizedArea.yMin < 0 ||
                normalizedArea.xMax > 1 || normalizedArea.yMax > 1 ||
                float.IsNaN(normalizedArea.x) || float.IsNaN(normalizedArea.y) ||
                float.IsNaN(normalizedArea.width) || float.IsNaN(normalizedArea.height))
                throw new ArgumentException("Invalid PC operator display rectangle");
            // HandleDisplayGameStats multiplies binary32 normalized coordinates
            // in x87 and truncates to signed shorts; double holds this product exactly.
            return new Rect((short)(int)((double)normalizedArea.x * 1080),
                (short)(int)((double)normalizedArea.y * 1920),
                (short)(int)((double)normalizedArea.width * 1080),
                (short)(int)((double)normalizedArea.height * 1920));
        }

        public bool RequestPcGameStatistics(Rect normalizedArea)
        {
            if (!OperatorFallbackActive && !CanEnterPcOperatorPage()) return false;
            Rect area = SourceOperatorRectangle(normalizedArea);
            ShowOperatorFallback(area, false);
            Debug.Log("ROULETTE_PC_NO_GAME_STATISTICS_OPEN");
            return true;
        }

        public bool RequestPcVideoDiagnostics()
        {
            if (!CanEnterPcOperatorPage()) return false;
            ShowOperatorFallback(new Rect(0, 0, 1080, 1920), true);
            // Game::ExitDiagsFromGame/CleanOnDiagExit runs synchronously.
            ClosePcOperatorFallback();
            Action notify = PcVideoDiagnosticsExitRequested;
            if (notify != null) notify();
            Debug.Log("ROULETTE_PC_NO_DIAGNOSTICS_EXIT_REQUESTED");
            return true;
        }

        private void ShowOperatorFallback(Rect area, bool diagnostics)
        {
            if (operatorFallbackFont == null) operatorFallbackFont = OriginalBitmapFont.Load("operator_9");
            if (operatorFallbackFont == null) throw new InvalidOperationException("Missing source operator font 9");
            if (operatorFallbackOverlay != null)
            {
                operatorFallbackOverlay.gameObject.SetActive(false);
                Destroy(operatorFallbackOverlay.gameObject);
            }
            operatorFallbackOverlay = UiRect(portraitRoot, new Rect(0, 0, 1080, 1920),
                "PC operator fallback event ownership");
            Button shield = UiButton(operatorFallbackOverlay, new Rect(0, 0, 1080, 1920), () => { });
            shield.targetGraphic.color = Color.clear;
            operatorFallbackScreen = UiRect(operatorFallbackOverlay, area,
                diagnostics ? "Source GameDiagsScreenRoot" : "Source GameStatsWindow");
            Button background = UiButton(operatorFallbackScreen, new Rect(0, 0, area.width, area.height), () => { });
            // Roulette::DisplayGameStatsInit overrides Game's black background.
            background.targetGraphic.color = new Color32(75, 75, 75, 255);
            operatorFallbackLabel = null;
            operatorFallbackIsDiagnostics = diagnostics;
            operatorFallbackActive = true;
            // Roulette sends DisplayGameStatsComplete before invoking the
            // inherited text-display slot (Game sends it after that slot).
            Action complete = PcGameStatisticsDisplayComplete;
            if (!diagnostics && complete != null) complete();
            operatorFallbackLabel = new PictureLabel(operatorFallbackScreen,
                new Rect(0, (int)area.height / 2, area.width, operatorFallbackFont.Height),
                operatorFallbackFont, diagnostics ? "Source SetupText" : "Source StatsText",
                nativeSize: true, verticalAlign: -1);
            operatorFallbackLabel.Root.gameObject.AddComponent<RectMask2D>();
            operatorFallbackLabel.Set(diagnostics ? "NO DIAGNOSTICS AVAILABLE" : "No Game Statistics Available");
            operatorFallbackOverlay.SetAsLastSibling();
        }

        public void ClosePcOperatorFallback()
        {
            if (!OperatorFallbackActive) return;
            bool statistics = !operatorFallbackIsDiagnostics;
            operatorFallbackActive = false;
            operatorFallbackOverlay.gameObject.SetActive(false);
            Destroy(operatorFallbackOverlay.gameObject);
            operatorFallbackOverlay = null;
            operatorFallbackScreen = null;
            operatorFallbackLabel = null;
            RefreshPortraitUi();
            Action complete = PcGameStatisticsExitComplete;
            if (statistics && complete != null) complete();
        }

        private void TogglePcGameStatistics()
        {
            if (OperatorFallbackActive) ClosePcOperatorFallback();
            else RequestPcGameStatistics(new Rect(0, 0, 1, 1));
        }
    }
}
