using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace RouletteDeluxe
{
    // PC integration scene for recovered selection components. Financial operations,
    // machine configuration and tilt inventory are explicit diagnostic host fixtures.
    public sealed class OriginalHandpaySelectionPcProbe : MonoBehaviour
    {
        [Serializable] private sealed class Report
        {
            public string engine, backend, mode, amountLine, inputScope;
            public int width, height, result, residualSubstate, selectedByte, inProgress, checks, disposedEvents, gambles;
            public bool machineDisabled, handpaySelected, financialExecution;
            public string[] events, callbacks;
        }
        private OriginalVerifyYesNoPresentation screen;
        private OriginalVideoInputDispatch route;
        private OriginalSpriteInputTarget yes, no;
        private OriginalHandpaySelectionPersistence owner;
        private int checks, disposed;
        private static string Arg(string key, string fallback)
        { var args = Environment.GetCommandLineArgs(); int i = Array.IndexOf(args, key); return i >= 0 && i + 1 < args.Length ? args[i + 1] : fallback; }
        private void Check(bool value, string name)
        { if (!value) throw new Exception("Selection PC probe: " + name); ++checks; }
        private static string Ascii(byte[] bytes)
        { foreach (byte b in bytes) if (b > 127) throw new Exception("Explicit ASCII probe decoder"); return Encoding.ASCII.GetString(bytes); }
        private IEnumerator Start()
        {
            var pending = new Stack<IEnumerator>(); pending.Push(Run());
            while (pending.Count != 0)
            {
                bool more; object value;
                try { more = pending.Peek().MoveNext(); value = more ? pending.Peek().Current : null; }
                catch (Exception error) { Debug.LogException(error); if (owner != null) owner.Dispose(); Application.Quit(8); yield break; }
                if (!more) { pending.Pop(); continue; }
                var nested = value as IEnumerator;
                if (nested != null) pending.Push(nested); else yield return value;
            }
        }
        private IEnumerator Run()
        {
            if (Application.platform != RuntimePlatform.WindowsPlayer) throw new Exception("Windows Player required");
            int width = int.Parse(Arg("-nativeHandpayWidth", "800")), height = int.Parse(Arg("-nativeHandpayHeight", "600"));
            string mode = Arg("-nativeHandpayChoice", "yes");
            bool disabled = mode == "disabledno";
            if (mode != "yes" && mode != "no" && mode != "exit" && !disabled) throw new Exception("Explicit probe choice required");
            string output = Path.GetFullPath(Arg("-nativeHandpayOutput", ""));
            if (Directory.Exists(output)) throw new Exception("Fresh selection probe output required"); Directory.CreateDirectory(output);
            for (int frame = 0; frame < 120 && (Screen.width != width || Screen.height != height); ++frame) yield return null;
            Check(Screen.width == width && Screen.height == height, "viewport");
            byte[] seed = new byte[135]; seed[0x24] = 0xe3; seed[0x25] = 3; seed[0x63] = 2;
            string nvPath = Path.Combine(output, "selection_nv.bin");
            var events = new List<string>(); var callbacks = new List<string>(); bool pending = false, released = false; int gambles = 0;
            owner = OriginalHandpaySelectionPersistence.OpenFile(nvPath, seed, text => callbacks.Add(text));
            Check(owner.SelectionAmount == 995, "NV selection amount");
            var translator = OriginalLanguageTranslator.Load(name => File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath,
                "Original/definitions/BAL_OS366J/LanguageTranslation/" + name)));
            var locale = OriginalMonetaryLocales.LoadPlatform("ms_MY.utf8");
            var content = OriginalHandpaySelectionText.Build(translator, owner.SelectionAmount, "en", true,
                amount => locale.FormatText(amount, 100, false, 0), Encoding.ASCII.GetBytes, null);
            string amountLine = Ascii(content.AmountLine); Check(amountLine.StartsWith("RM9.95", StringComparison.Ordinal), "original money factory");
            var lifecycle = new OriginalHandpaySelectionLifecycle(() => owner.SelectionAmount,
                code => { Check(code == 0x6020, "tilt query code"); return pending; },
                (data, flags) => { Check(flags == 0, "event dispatch argument"); events.Add(BitConverter.ToString(data).Replace("-", "")); pending = data[1] == 1; },
                () =>
                {
                    callbacks.Add("create"); var font = OriginalBitmapFont.Load("OS366J/arial-bold-12-white");
                    screen = OriginalHandpaySelectionPresentation.Create(width, height, content, value => value, Ascii,
                        (slot, bytes) => (short)font.MeasureWidth(Ascii(bytes)));
                    route = new OriginalVideoInputDispatch(node => screen != null && node != null,
                        () => new OriginalVideoInputDispatch.ClockSample { Seconds = 1, Microseconds = 2 }, value => ++disposed);
                    var root = new OriginalSpriteInputTarget { Flags = 1, Rectangle = new OriginalSpriteAlignment.Rectangle { Right = (short)width, Bottom = (short)height } };
                    yes = new OriginalSpriteInputTarget { Flags = 0x47, Rectangle = screen.Geometry.YesButton, Parent = root };
                    no = new OriginalSpriteInputTarget { Flags = 0x47, Rectangle = screen.Geometry.NoButton, Parent = root };
                    root.Children.Add(yes); root.Children.Add(no); route.Windows.Add(root); screen.AttachNativeInput(route, yes, no, true);
                },
                () => { callbacks.Add("release"); screen = null; route.Windows.Clear(); released = true; });
            var coordinator = owner.CreateCoordinator(new OriginalHandpaySelectionCoordinator.Host
            {
                BeforeResidualSelection = () => callbacks.Add("before"),
                SetTransactionInProgress = value => { Check(value, "transaction start"); callbacks.Add("transaction"); },
                PrepareScreen = lifecycle.Prepare, ReleaseScreenAndTiltEvents = lifecycle.Release,
                MachineDisabled = () => { Check(released, "release before disabled query"); callbacks.Add("disabled"); return disabled; },
                ResidualGamble = () => { Check(owner.State.ResidualSubstate == 9, "saved residual before gamble"); callbacks.Add("gamble"); ++gambles; },
                AfterResidualGamble = () => callbacks.Add("after"),
                EmitResidualEvent = () => { callbacks.Add("residualEvent"); events.Add(BitConverter.ToString(OriginalHandpaySelectionEvents.Residual(0x89abcdef, 77)).Replace("-", "")); }
            });
            Check(coordinator.Begin() && screen.NativeOrder == 9125, "coordinator creates screen"); owner.ValidateStorage();
            Check(owner.State.InProgress == 1 && owner.State.ResidualSubstate == 3, "entered NV state");
            File.WriteAllBytes(Path.Combine(output, "entered_nv.bin"), owner.ReadDeviceImage());
            yield return Capture(Path.Combine(output, "idle.png"));
            int result;
            if (mode == "exit") { screen.Input.SelectExit(); Check(screen.Input.TryGetDisplayResult(out result) && result == -1, "explicit exit result"); }
            else
            {
                var target = mode == "yes" ? yes : no;
                Send(0x200001, (short)(target.Rectangle.Left + 5), (short)(target.Rectangle.Top + 5));
                Check(screen.Input.TryGetDisplayResult(out result) && result == (mode == "yes" ? 1 : 0) && route.TouchCaptured(1) == target, "routed down result");
            }
            yield return Capture(Path.Combine(output, "pressed.png"));
            if (mode != "exit") { Send(0x200002, 0, 0); Send(0x200004, 0, 0); }
            Check(route.TouchCaptured(1) == null && !screen.YesState.Pressed && !screen.NoState.Pressed, "outside release capture cleanup");
            yield return Capture(Path.Combine(output, "released.png"));
            bool selected = coordinator.Complete(result); owner.ValidateStorage();
            Check(selected == (disabled || result != 0), "selection mapping");
            Check(screen == null && !coordinator.ScreenPresent && !pending, "screen and tilt released");
            Check(owner.State.InProgress == 0 && owner.State.ResidualSubstate == (selected ? 4 : 9) && owner.State.HandpaySelectedByte == (selected ? 1 : 0), "final NV state");
            Check(events.Count == (selected ? 2 : 3) && gambles == (selected ? 0 : 1), "event and gamble count");
            owner.Dispose(); owner = OriginalHandpaySelectionPersistence.OpenFile(nvPath, new byte[135], text => callbacks.Add(text));
            Check(owner.SelectionAmount == 995 && owner.State.InProgress == 0 && owner.State.ResidualSubstate == (selected ? 4 : 9), "cold NV reopen"); owner.ValidateStorage();
            yield return Capture(Path.Combine(output, "closed.png"));
            File.WriteAllText(Path.Combine(output, "report.json"), JsonUtility.ToJson(new Report
            {
                engine = Application.unityVersion, backend = SystemInfo.graphicsDeviceType.ToString(), mode = mode,
                amountLine = amountLine, width = width, height = height, result = result, machineDisabled = disabled,
                handpaySelected = selected, residualSubstate = owner.State.ResidualSubstate, inProgress = owner.State.InProgress,
                selectedByte = owner.State.HandpaySelectedByte, checks = checks, disposedEvents = disposed, gambles = gambles,
                events = events.ToArray(), callbacks = callbacks.ToArray(), financialExecution = false,
                inputScope = "Actual Player integration using native-format software touch events or explicit Exit flag. Explicit en/ms_MY locale, pending tilt inventory and machine/financial callback fixtures; no OS input or payment ledger."
            }, true));
            owner.Dispose(); owner = null;
            Debug.Log("HANDPAY_SESSION_PC_PASSED mode=" + mode + " result=" + result + " checks=" + checks); Application.Quit(0);
        }
        private IEnumerator Capture(string path)
        {
            yield return new WaitForEndOfFrame();
            var texture = ScreenCapture.CaptureScreenshotAsTexture();
            if (texture == null) throw new Exception("Selection framebuffer capture missing");
            try { File.WriteAllBytes(path, texture.EncodeToPNG()); }
            finally { Destroy(texture); }
        }
        private void Send(uint type, short x, short y)
        { route.InputQueue.Enqueue(new OriginalVideoInputEvent { Type = type, Size = 0x1c, Display = 1, Point = new OriginalSpriteInputTarget.Point(x, y) }); route.ProcessOne(); }
        private void OnGUI() { if (screen != null) screen.Draw(); }
        private void OnDestroy() { if (owner != null) owner.Dispose(); }
    }
}
