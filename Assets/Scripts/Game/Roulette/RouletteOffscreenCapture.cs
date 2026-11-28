using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        // Diagnostic-only capture of the active original-screen uGUI surface.
        // ScreenCapture needs a presentable window and is black in -nographics.
        private IEnumerator CapturePortraitOffscreen(string path, float delay)
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-rouletteDisplayRouteCapture") >= 0)
                Application.runInBackground = true;
            if (Array.IndexOf(Environment.GetCommandLineArgs(),
                "-rouletteOffscreenFromPayout") >= 0)
                yield return new WaitUntil(() => session != null &&
                    !spinning && session.ResultPhase && session.LastResult != null);
            float readyAt = Time.realtimeSinceStartup + delay;
            while (Time.realtimeSinceStartup < readyAt) yield return null;
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-rouletteDisplayRouteCapture") >= 0)
            {
                if (session == null || spinning || session.SpinPending || session.Rounds != 0 ||
                    recordedPockets.Count != 0 || pcCollectPending || !ideckSecondDisplay || ideckPreview ||
                    ideckCanvas == null || ideckCanvas.targetDisplay != ideckDisplayIndex)
                {
                    Application.Quit(8);
                    throw new InvalidOperationException("Dedicated display capture has no idle routed owner");
                }
                int targetBefore = ideckCanvas.targetDisplay;
                CaptureActiveScreenOffscreen(path);
                CaptureActiveScreenOffscreen(path + ".ideck.png", true);
                Canvas.ForceUpdateCanvases();
                if (ideckCanvas.targetDisplay != targetBefore || spinning || session.SpinPending ||
                    session.Rounds != 0 || recordedPockets.Count != 0 || pcCollectPending)
                {
                    Debug.LogError("ROULETTE_DEDICATED_CAPTURE_STATE_CHANGED: targetBefore=" + targetBefore +
                        " targetAfter=" + ideckCanvas.targetDisplay + " spinning=" + spinning +
                        " spinPending=" + session.SpinPending + " rounds=" + session.Rounds +
                        " pockets=" + recordedPockets.Count + " collectPending=" + pcCollectPending);
                    Application.Quit(8);
                    throw new InvalidOperationException("Dedicated capture changed routing or game activity");
                }
                Debug.Log("ROULETTE_DEDICATED_CAPTURE_ROUTE_RESTORED: target=" + ideckCanvas.targetDisplay);
                if (!Display.displays[targetBefore].active ||
                    SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLCore)
                {
                    Debug.LogError("ROULETTE_DEDICATED_CAPTURE_UNSUPPORTED: active=" +
                        Display.displays[targetBefore].active + " graphics=" + SystemInfo.graphicsDeviceType);
                    Application.Quit(8); yield break;
                }
                Debug.Log("ROULETTE_DEDICATED_DISPLAY_CAPTURE_PASSED: displays=" + Display.displays.Length +
                    " iDeck=" + ideckCanvas.targetDisplay + " rendering=" +
                    Display.displays[targetBefore].renderingWidth + "x" + Display.displays[targetBefore].renderingHeight);
                Application.Quit(); yield break;
            }
            yield return new WaitForEndOfFrame();
            CaptureActiveScreenOffscreen(path);
            if (Array.IndexOf(Environment.GetCommandLineArgs(),
                "-rouletteOffscreenKeepRunning") < 0)
                Application.Quit();
        }

        private void CaptureActiveScreenOffscreen(string path, bool dedicatedDeck = false)
        {
            bool captureDeck = ideckPreview || dedicatedDeck;
            Canvas source = captureDeck ? ideckCanvas :
                topperPreview || histogramPreview ? topperCanvas : portraitCanvas;
            if (source == null || !source.enabled)
                throw new InvalidOperationException("Active roulette screen is not available for offscreen capture");
            // 台面竖装旋转后，1080x1920 的输出帧只会截到中间一条竖带，
            // 静默产出错图比报错更危险，这里直接失败。
            if (!captureDeck && deckRotationDegrees != 0)
                throw new InvalidOperationException(
                    "Offscreen capture is not supported while the deck is rotated");
            int width = captureDeck ? 1280 : topperPreview || histogramPreview ? 1360 : 1080;
            int height = captureDeck ? 242 : topperPreview || histogramPreview ? 768 : 1920;
            string screen = captureDeck ? "iDeck" :
                topperPreview || histogramPreview ? "topper" : "portrait";
            var cameraObject = new GameObject("Temporary roulette capture camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            RenderTexture target = RenderTexture.GetTemporary(width, height, 24,
                RenderTextureFormat.ARGB32);
            Texture2D pixels = null;
            RenderTexture previous = RenderTexture.active;
            RenderMode previousMode = source.renderMode;
            Camera previousCamera = source.worldCamera;
            float previousDistance = source.planeDistance;
            int previousDisplay = source.targetDisplay;
            CanvasScaler scaler = source.GetComponent<CanvasScaler>();
            CanvasScaler.ScaleMode previousScaleMode = scaler.uiScaleMode;
            float previousScaleFactor = scaler.scaleFactor;
            try
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.black;
                camera.orthographic = true;
                camera.orthographicSize = height * .5f;
                camera.aspect = (float)width / height;
                camera.transform.position = new Vector3(0f, 0f, -10f);
                camera.targetTexture = target;
                source.targetDisplay = 0;
                source.renderMode = RenderMode.ScreenSpaceCamera;
                source.worldCamera = camera;
                source.planeDistance = 1f;
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
                scaler.scaleFactor = 1f;
                Canvas.ForceUpdateCanvases();
                camera.Render();
                RenderTexture.active = target;
                pixels = new Texture2D(width, height, TextureFormat.RGB24, false);
                pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                pixels.Apply(false);
                string directory = Path.GetDirectoryName(Path.GetFullPath(path));
                if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
                File.WriteAllBytes(path, pixels.EncodeToPNG());
                Debug.Log("ROULETTE_OFFSCREEN_CAPTURE: " + path +
                    " size=" + width + "x" + height +
                    " screen=" + screen +
                    " graphics=" + SystemInfo.graphicsDeviceType);
            }
            finally
            {
                scaler.uiScaleMode = previousScaleMode;
                scaler.scaleFactor = previousScaleFactor;
                source.renderMode = previousMode;
                source.worldCamera = previousCamera;
                source.planeDistance = previousDistance;
                // Restoring render mode/camera can reset the display selection.
                // Restore the original routed target after those setters.
                source.targetDisplay = previousDisplay;
                RenderTexture.active = previous;
                camera.targetTexture = null;
                RenderTexture.ReleaseTemporary(target);
                if (pixels != null) Destroy(pixels);
                Destroy(cameraObject);
            }
        }
    }
}
