using System;
using System.IO;
namespace RouletteDeluxe.SasDraftV1
{
    // Candidate PC storage. No host connection, wallet mutation or Unity publication.
    public sealed class SasConfigSnapshotFileStoreV3
    {
        private readonly string path, scope;
        public bool Faulted { get; private set; }
        public SasConfigSnapshotFileStoreV3(string filePath, string expectedScope)
        {
            if (string.IsNullOrWhiteSpace(filePath) || string.IsNullOrWhiteSpace(expectedScope))
                throw new ArgumentException("Storage path and scope required");
            path = Path.GetFullPath(filePath); scope = expectedScope;
            if (!Directory.Exists(Path.GetDirectoryName(path)))
                throw new DirectoryNotFoundException("SAS storage directory must be explicitly provisioned");
        }
        public SasConfigSnapshotV2 Load()
        {
            RequireHealthy();
            try { using (var lease = Lease()) return Read(); }
            catch { Faulted = true; throw; }
        }
        public SasConfigSnapshotV2 Save(long expectedGeneration, SasConfigSnapshotV2 candidate)
        {
            RequireHealthy();
            string temporary = null;
            try
            {
                if (candidate == null || expectedGeneration < -1 || candidate.Config.Scope != scope ||
                    candidate.Config.Generation != checked(expectedGeneration + 1))
                    throw new ArgumentException("SAS candidate generation or scope mismatch");
                using (var lease = Lease())
                {
                    SasConfigSnapshotV2 current = Read();
                    long actual = current == null ? -1 : current.Config.Generation;
                    if (actual != expectedGeneration)
                        throw new InvalidOperationException("SAS durable generation changed");
                    var after = candidate.CopyReceipts();
                    var before = current == null ? new SasEventReceiptV1[0] : current.CopyReceipts();
                    if (after.Length < before.Length || after.Length - before.Length > 1)
                        throw new InvalidOperationException("Durable receipt catalogue must retain its prefix and append at most one event");
                    for (int i = 0; i < before.Length; i++)
                    {
                        var a = before[i]; var b = after[i];
                        if (a.Generation != b.Generation || a.Event.Source != b.Event.Source ||
                            a.Event.Id != b.Event.Id || a.Event.Sequence != b.Event.Sequence)
                            throw new InvalidOperationException("Durable receipt prefix identity changed");
                        byte[] x = a.Event.CopyWire(), y = b.Event.CopyWire();
                        for (int j = 0; j < x.Length; j++)
                            if (x[j] != y[j]) throw new InvalidOperationException("Durable receipt prefix bytes changed");
                    }
                    if (after.Length > before.Length && after[after.Length - 1].Generation != candidate.Config.Generation)
                        throw new InvalidOperationException("Appended event must belong to the candidate generation");
                    var priorIdle = current == null ? null : current.IdleReceipt;
                    var nextIdle = candidate.IdleReceipt;
                    if (priorIdle != null && nextIdle == null)
                        throw new InvalidOperationException("Completed Idle receipt cannot be deleted");
                    if (priorIdle != null && nextIdle.IdleEntryId == priorIdle.IdleEntryId &&
                        (nextIdle.Scope != priorIdle.Scope || nextIdle.Generation != priorIdle.Generation || nextIdle.Raw32Sha256 != priorIdle.Raw32Sha256))
                        throw new InvalidOperationException("Completed Idle receipt cannot be rewritten");
                    if (nextIdle != null && (priorIdle == null || nextIdle.IdleEntryId != priorIdle.IdleEntryId) &&
                        nextIdle.Generation != candidate.Config.Generation)
                        throw new InvalidOperationException("New Idle receipt must belong to candidate generation");
                    byte[] bytes = SasConfigSnapshotCodecV2.Encode(candidate);
                    temporary = path + ".candidate." + Guid.NewGuid().ToString("N");
                    using (var file = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                    { file.Write(bytes, 0, bytes.Length); file.Flush(true); }
                    if (current == null) File.Move(temporary, path);
                    else File.Replace(temporary, path, null);
                    temporary = null;
                    byte[] readback = File.ReadAllBytes(path);
                    if (readback.Length != bytes.Length) throw new IOException("SAS readback length differs");
                    for (int i = 0; i < bytes.Length; i++)
                        if (readback[i] != bytes[i]) throw new IOException("SAS readback bytes differ");
                    return SasConfigSnapshotCodecV2.Decode(readback, scope);
                }
            }
            catch { Faulted = true; throw; }
            finally
            {
                if (temporary != null)
                {
                    try { File.Delete(temporary); } catch (IOException) { }
                    catch (UnauthorizedAccessException) { }
                }
            }
        }
        private FileStream Lease()
        { return new FileStream(path + ".lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None); }
        private SasConfigSnapshotV2 Read()
        {
            // File.Exists returns false for some permission failures. Open directly so
            // inaccessible storage faults instead of silently becoming disabled config.
            byte[] bytes;
            try { bytes = File.ReadAllBytes(path); }
            catch (FileNotFoundException) { return null; }
            return SasConfigSnapshotCodecV2.Decode(bytes, scope);
        }
        private void RequireHealthy()
        { if (Faulted) throw new InvalidOperationException("SAS storage is faulted; explicit cold recovery required"); }
    }
}
